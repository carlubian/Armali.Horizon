using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using ZstdSharp;

namespace Armali.Horizon.IO;

/// <summary>
/// Servicio central de eventos Horizon sobre Redis pub/sub.
/// <para>
/// Soporta dos modos de comunicación:
/// <list type="bullet">
///   <item><b>Fire-and-forget</b>: <see cref="PublishAsync{T}"/> + <see cref="Subscribe{T}"/></item>
///   <item><b>Request/Response</b>: <see cref="RequestAsync{TResponse}"/> (solicitante) +
///         handlers registrados con <see cref="HorizonEventBuilder.HandleRequest{THandler,TRequest,TResponse}"/>
///         (respondedor)</item>
/// </list>
/// Los payloads viajan comprimidos con Zstd dentro de un sobre <see cref="HorizonEvent"/>.
/// </para>
/// </summary>
public class HorizonEventService : IHostedService
{
    public static HorizonEventSettings Settings { get; set; } = new();
    
    private ConnectionMultiplexer? Redis;
    private IDatabase? Database;
    
    private readonly IServiceProvider ServiceProvider;
    private readonly ILogger<HorizonEventService> Logger;
    
    /// <summary>
    /// Canal de respuestas único para esta instancia.
    /// Cada proceso genera su propio canal para recibir respuestas a sus peticiones.
    /// </summary>
    private readonly string ReplyChannel = $"horizon:replies:{Guid.NewGuid():N}";
    
    /// <summary>
    /// Peticiones pendientes indexadas por CorrelationId.
    /// El callback recibe los bytes ya descomprimidos del payload de respuesta.
    /// </summary>
    private readonly ConcurrentDictionary<Guid, Action<byte[]>> PendingCallbacks = new();
    
    /// <summary>
    /// Handlers registrados durante la configuración del host.
    /// Clave = EventType del request. Se rellena desde <see cref="HorizonEventBuilder"/>.
    /// </summary>
    internal static readonly Dictionary<string, HandlerRegistration> RegisteredHandlers = new();
    
    public HorizonEventService(IServiceProvider serviceProvider, ILogger<HorizonEventService> logger)
    {
        ServiceProvider = serviceProvider;
        Logger = logger;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Redis = await ConnectionMultiplexer.ConnectAsync(Settings.Endpoint);
        Database = Redis.GetDatabase();
        
        var subscriber = Redis.GetSubscriber();
        
        // ── Suscripción al canal de respuestas (para RequestAsync) ──
        await subscriber.SubscribeAsync(RedisChannel.Literal(ReplyChannel), (_, message) =>
        {
            HandleReply(message);
        });
        
        // ── Suscripción a canales con handlers registrados ──
        // Agrupamos por canal para crear una sola suscripción Redis por canal
        var channelGroups = RegisteredHandlers.Values
            .GroupBy(h => h.Channel)
            .ToList();
        
        foreach (var group in channelGroups)
        {
            // Diccionario local EventType → Invoker para despacho rápido
            var handlers = group.ToDictionary(h => h.EventType, h => h.Invoker);
            
            await subscriber.SubscribeAsync(RedisChannel.Literal(group.Key), (ch, message) =>
            {
                // Lanzamos el dispatch como tarea para no bloquear el hilo de Redis
                Task.Run(() => DispatchRequestAsync(message, handlers));
            });
            
            Logger.LogInformation(
                "Escuchando peticiones en canal '{Channel}' para {Count} EventType(s): {Types}",
                group.Key, handlers.Count, string.Join(", ", handlers.Keys));
        }
    }
    
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        // Cancelar peticiones pendientes para que no queden colgadas
        foreach (var key in PendingCallbacks.Keys)
            PendingCallbacks.TryRemove(key, out _);
        
        if (Redis != null)
            await Redis.DisposeAsync();
    }
    
    // ── Fire-and-forget Pub/Sub ──────────────────────────────────────
    
    /// <summary>
    /// Publica un evento fire-and-forget en el canal indicado.
    /// </summary>
    public async Task PublishAsync<T>(string channel, T payload) where T : IHorizonEventPayload
    {
        var @event = CreateEvent(payload);
        var json = JsonSerializer.Serialize(@event);
        await Database!.PublishAsync(RedisChannel.Literal(channel), json);
    }
    
    /// <summary>
    /// Se suscribe a un canal y deserializa los mensajes al tipo concreto <typeparamref name="T"/>.
    /// </summary>
    public void Subscribe<T>(string channel, Action<T> onReceive, Predicate<string>? eventTypeCondition = null)
        where T : IHorizonEventPayload
    {
        var predicate = eventTypeCondition ?? (_ => true);
        var subscriber = Redis!.GetSubscriber();
        
        subscriber.Subscribe(RedisChannel.Literal(channel), (_, message) =>
        {
            var jsonEvent = JsonSerializer.Deserialize<HorizonEvent>((string?)message ?? string.Empty);
            if (jsonEvent == null || !predicate(jsonEvent.EventType))
                return;
            
            using var decompressor = new Decompressor();
            var decompressed = decompressor.Unwrap(jsonEvent.Payload);
            var payload = JsonSerializer.Deserialize<T>(decompressed);
            if (payload != null)
                onReceive(payload);
        });
    }
    
    // ── Request / Response ───────────────────────────────────────────
    
    /// <summary>
    /// Envía una petición al canal indicado y espera la respuesta tipada.
    /// <para>
    /// Internamente publica un <see cref="HorizonEvent"/> con <see cref="HorizonEvent.CorrelationId"/>
    /// y <see cref="HorizonEvent.ReplyTo"/> configurados, y espera a que el respondedor
    /// publique la respuesta en el canal de respuestas de esta instancia.
    /// </para>
    /// </summary>
    /// <param name="channel">Canal Redis del servicio destino.</param>
    /// <param name="request">Payload de la petición.</param>
    /// <param name="timeout">Timeout (usa <see cref="HorizonEventSettings.DefaultTimeout"/> si es null).</param>
    /// <param name="ct">Token de cancelación opcional.</param>
    /// <returns>Respuesta deserializada como <typeparamref name="TResponse"/>.</returns>
    /// <exception cref="TaskCanceledException">Si se agota el timeout o se cancela el token.</exception>
    public async Task<TResponse> RequestAsync<TResponse>(
        string channel,
        IHorizonEventPayload request,
        TimeSpan? timeout = null,
        CancellationToken ct = default)
        where TResponse : IHorizonEventPayload
    {
        var correlationId = Guid.NewGuid();
        var actualTimeout = timeout ?? Settings.DefaultTimeout;
        
        // TaskCompletionSource que se completará cuando llegue la respuesta
        var tcs = new TaskCompletionSource<TResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
        
        // Registrar callback: cuando llegue un reply con este CorrelationId, deserializar y completar
        PendingCallbacks[correlationId] = decompressedBytes =>
        {
            try
            {
                var response = JsonSerializer.Deserialize<TResponse>(decompressedBytes);
                if (response != null)
                    tcs.TrySetResult(response);
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        };
        
        try
        {
            // Publicar la petición con los campos de correlación
            var @event = CreateEvent(request, correlationId, ReplyChannel);
            var json = JsonSerializer.Serialize(@event);
            await Database!.PublishAsync(RedisChannel.Literal(channel), json);
            
            // Esperar respuesta con timeout
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(actualTimeout);
            await using (cts.Token.Register(() => tcs.TrySetCanceled(ct)))
            {
                return await tcs.Task;
            }
        }
        finally
        {
            // Limpiar siempre el callback pendiente
            PendingCallbacks.TryRemove(correlationId, out _);
        }
    }
    
    // ── Helpers internos ─────────────────────────────────────────────
    
    /// <summary>
    /// Crea un <see cref="HorizonEvent"/> comprimiendo el payload con Zstd.
    /// </summary>
    private static HorizonEvent CreateEvent(
        IHorizonEventPayload payload,
        Guid? correlationId = null,
        string? replyTo = null)
    {
        using var compressor = new Compressor();
        var serialized = JsonSerializer.SerializeToUtf8Bytes(payload, payload.GetType());
        var compressed = compressor.Wrap(serialized);
        
        return new HorizonEvent
        {
            EventId = Guid.NewGuid(),
            EventType = payload.EventType,
            Payload = compressed.ToArray(),
            CorrelationId = correlationId,
            ReplyTo = replyTo
        };
    }
    
    /// <summary>
    /// Procesa un mensaje en el canal de respuestas:
    /// busca el callback pendiente por CorrelationId y lo invoca con el payload descomprimido.
    /// </summary>
    private void HandleReply(RedisValue message)
    {
        try
        {
            var jsonEvent = JsonSerializer.Deserialize<HorizonEvent>((string?)message ?? "");
            if (jsonEvent?.CorrelationId == null) return;
            
            if (PendingCallbacks.TryGetValue(jsonEvent.CorrelationId.Value, out var callback))
            {
                using var decompressor = new Decompressor();
                var decompressed = decompressor.Unwrap(jsonEvent.Payload);
                callback(decompressed.ToArray());
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error procesando respuesta en canal {ReplyChannel}", ReplyChannel);
        }
    }
    
    /// <summary>
    /// Despacha una petición entrante al handler registrado y publica la respuesta
    /// en el canal ReplyTo del solicitante.
    /// </summary>
    private async Task DispatchRequestAsync(
        RedisValue message,
        Dictionary<string, HandlerInvoker> handlers)
    {
        try
        {
            var jsonEvent = JsonSerializer.Deserialize<HorizonEvent>((string?)message ?? "");
            if (jsonEvent == null) return;
            
            // Si no tiene ReplyTo, es un evento fire-and-forget — ignorar en el dispatcher
            if (string.IsNullOrEmpty(jsonEvent.ReplyTo) || jsonEvent.CorrelationId == null)
                return;
            
            // Buscar handler por EventType
            if (!handlers.TryGetValue(jsonEvent.EventType, out var invoker))
            {
                Logger.LogWarning("No hay handler registrado para EventType '{EventType}'", jsonEvent.EventType);
                return;
            }
            
            // Descomprimir payload de la petición
            using var decompressor = new Decompressor();
            var decompressedRequest = decompressor.Unwrap(jsonEvent.Payload);
            
            // Invocar el handler (crea un scope de DI internamente)
            var responseBytes = await invoker(decompressedRequest.ToArray(), ServiceProvider, CancellationToken.None);
            
            // Comprimir y enviar respuesta con el mismo CorrelationId
            using var compressor = new Compressor();
            var compressedResponse = compressor.Wrap(responseBytes);
            
            var responseEvent = new HorizonEvent
            {
                EventId = Guid.NewGuid(),
                EventType = jsonEvent.EventType + ":response",
                Payload = compressedResponse.ToArray(),
                CorrelationId = jsonEvent.CorrelationId
            };
            
            var json = JsonSerializer.Serialize(responseEvent);
            await Database!.PublishAsync(RedisChannel.Literal(jsonEvent.ReplyTo), json);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error despachando petición");
        }
    }
}

/// <summary>
/// Delegado type-erased que invoca un handler concreto.
/// Recibe los bytes descomprimidos del request, el service provider para crear un scope,
/// y devuelve los bytes serializados del response.
/// </summary>
internal delegate Task<byte[]> HandlerInvoker(
    byte[] decompressedRequest,
    IServiceProvider serviceProvider,
    CancellationToken ct);

/// <summary>
/// Registro interno: asocia un canal + EventType con su invocador type-erased.
/// </summary>
internal class HandlerRegistration
{
    public required string Channel { get; init; }
    public required string EventType { get; init; }
    public required HandlerInvoker Invoker { get; init; }
}
