using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Armali.Horizon.IO;

public static class HorizonEventExtensions
{
    /// <summary>
    /// Registra el sistema de eventos Horizon en el host.
    /// <para>
    /// Modo básico (solo pub/sub fire-and-forget):
    /// <code>builder.Host.UseHorizonEvents();</code>
    /// </para>
    /// <para>
    /// Modo con handlers de request/response:
    /// <code>
    /// builder.Host.UseHorizonEvents(events =>
    /// {
    ///     events.HandleRequest&lt;MyHandler, MyRequest, MyResponse&gt;("mi-canal");
    /// });
    /// </code>
    /// </para>
    /// </summary>
    public static IHostBuilder UseHorizonEvents(this IHostBuilder builder, Action<HorizonEventBuilder>? configure = null)
    {
        return builder.ConfigureServices((context, services) =>
        {
            // Recuperamos la configuración o usamos defaults
            var settings = context.Configuration.GetSection("Horizon").GetSection("Events")
                .Get<HorizonEventSettings>() ?? new HorizonEventSettings();
            
            // Asignamos la configuración estática
            HorizonEventService.Settings = settings;
            
            // Registrar como singleton + hosted service para que sea inyectable directamente
            services.AddSingleton<HorizonEventService>();
            services.AddHostedService(sp => sp.GetRequiredService<HorizonEventService>());
            
            // Configurar handlers si se proporcionan
            if (configure != null)
            {
                var eventBuilder = new HorizonEventBuilder(services);
                configure(eventBuilder);
            }
        });
    }
}

/// <summary>
/// Builder fluent para registrar handlers de request/response en el sistema de eventos.
/// </summary>
public class HorizonEventBuilder
{
    private readonly IServiceCollection Services;
    
    internal HorizonEventBuilder(IServiceCollection services) => Services = services;
    
    /// <summary>
    /// Registra un handler que procesará peticiones <typeparamref name="TRequest"/>
    /// y devolverá respuestas <typeparamref name="TResponse"/> en el canal indicado.
    /// <para>
    /// El handler se resuelve desde DI con un scope nuevo por cada petición,
    /// permitiendo inyectar servicios scoped como DbContextFactory.
    /// </para>
    /// <para>
    /// El EventType se obtiene automáticamente creando una instancia default del request
    /// (requiere constructor sin parámetros).
    /// </para>
    /// </summary>
    /// <typeparam name="THandler">Tipo del handler (debe implementar IHorizonRequestHandler).</typeparam>
    /// <typeparam name="TRequest">Tipo del payload de petición (requiere new()).</typeparam>
    /// <typeparam name="TResponse">Tipo del payload de respuesta.</typeparam>
    /// <param name="channel">Canal Redis donde escuchar peticiones.</param>
    public HorizonEventBuilder HandleRequest<THandler, TRequest, TResponse>(string channel)
        where THandler : class, IHorizonRequestHandler<TRequest, TResponse>
        where TRequest : IHorizonEventPayload, new()
        where TResponse : IHorizonEventPayload
    {
        // Registrar el handler en DI como scoped
        Services.AddScoped<THandler>();
        
        // Obtener el EventType del request (se crea una instancia temporal para leerlo)
        var eventType = new TRequest().EventType;
        
        // Crear invocador type-erased que captura los tipos genéricos en un closure
        HandlerInvoker invoker = async (requestBytes, sp, ct) =>
        {
            var request = JsonSerializer.Deserialize<TRequest>(requestBytes)
                ?? throw new InvalidOperationException(
                    $"No se pudo deserializar la petición como {typeof(TRequest).Name}");
            
            // Crear scope de DI para resolver servicios scoped
            using var scope = sp.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<THandler>();
            var response = await handler.HandleAsync(request, ct);
            return JsonSerializer.SerializeToUtf8Bytes(response);
        };
        
        // Registrar en el mapa estático de handlers
        HorizonEventService.RegisteredHandlers[eventType] = new HandlerRegistration
        {
            Channel = channel,
            EventType = eventType,
            Invoker = invoker
        };
        
        return this;
    }
}
