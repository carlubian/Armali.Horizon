using Armali.Horizon.Althes.Configuration;
using Armali.Horizon.Contracts.Althes;
using Armali.Horizon.IO;
using Microsoft.Extensions.Options;

namespace Armali.Horizon.Althes.Services;

/// <summary>
/// Hosted service que orquesta los <see cref="AgentRuntime"/> de la instancia:
/// carga la config de Autoconfig, crea un runtime por agente, suscribe sus
/// canales internos del bus IO y arranca los loops.
/// </summary>
public class AgentRuntimeHost : BackgroundService
{
    private readonly IServiceProvider Services;
    private readonly AgentRegistry Registry;
    private readonly AgentInboxRouter Router;
    private readonly HorizonEventService Events;
    private readonly AlthesOptions Options;
    private readonly ILogger<AgentRuntimeHost> Logger;
    private readonly ILoggerFactory LoggerFactory;
    
    private readonly List<Task> RuntimeTasks = new();
    private readonly List<AgentRuntime> Runtimes = new();
    
    public AgentRuntimeHost(
        IServiceProvider services,
        AgentRegistry registry,
        AgentInboxRouter router,
        HorizonEventService events,
        IOptions<AlthesOptions> options,
        ILogger<AgentRuntimeHost> logger,
        ILoggerFactory loggerFactory)
    {
        Services = services;
        Registry = registry;
        Router = router;
        Events = events;
        Options = options.Value;
        Logger = logger;
        LoggerFactory = loggerFactory;
    }
    
    public IReadOnlyList<AgentRuntime> ActiveRuntimes => Runtimes;
    public AgentRuntime? GetRuntime(string name) =>
        Runtimes.FirstOrDefault(r => string.Equals(r.AgentName, name, StringComparison.OrdinalIgnoreCase));
    
    /// <summary>
    /// Cierra todos los runs activos de todos los agentes (genera Summary por cada uno)
    /// y marca la conversación activa como cerrada. La próxima entrada de inbox
    /// creará una nueva conversación + runs frescos automáticamente.
    /// </summary>
    public async Task<string?> StartNewConversationAsync(CancellationToken ct)
    {
        // Cerrar runs activos en paralelo.
        await Task.WhenAll(Runtimes.Select(r => r.CloseActiveRunAsync(ct)));
        
        // Cerrar la conversación activa, si existe.
        using var scope = Services.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<ConversationStore>();
        var active = await store.GetActiveConversationAsync(ct);
        if (active is null) return null;
        await store.CloseConversationAsync(active.Id, ct);
        return active.Id;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Cargar config de Autoconfig (con fallback a caché local).
        await Registry.LoadAsync(stoppingToken);
        
        if (Registry.All.Count == 0)
        {
            Logger.LogWarning("Sin agentes registrados; Althes corre vacío.");
            return;
        }
        
        // Por cada agente: crear inbox, suscribir su canal IO, lanzar runtime.
        foreach (var def in Registry.All)
        {
            var inbox = Router.GetOrCreate(def.Name);
            var channel = AlthesChannels.AgentInbox(Options.ProjectId, def.Name);
            
            // Puente bus IO → inbox FIFO local.
            Events.Subscribe<AgentInboxMessage>(channel, msg => inbox.Enqueue(msg));
            
            var runtime = new AgentRuntime(def, inbox, Services, MicrosoftOptions.Wrap(Options), LoggerFactory);
            Runtimes.Add(runtime);
            RuntimeTasks.Add(Task.Run(() => runtime.RunLoopAsync(stoppingToken), stoppingToken));
            
            Logger.LogInformation("Agente '{Name}' suscrito a {Channel}.", def.Name, channel);
        }
        
        // Esperar a que todos terminen (al apagar el host).
        await Task.WhenAll(RuntimeTasks);
    }
    
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogInformation("Parando host de agentes…");
        await base.StopAsync(cancellationToken);
    }
}

/// <summary>Helper para envolver un valor concreto en un <see cref="IOptions{T}"/>.</summary>
internal static class MicrosoftOptions
{
    public static IOptions<T> Wrap<T>(T value) where T : class, new() => new Wrapper<T>(value);
    private class Wrapper<T> : IOptions<T> where T : class, new()
    {
        public Wrapper(T value) { Value = value; }
        public T Value { get; }
    }
}

