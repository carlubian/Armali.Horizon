using Armali.Horizon.Contracts.Althes;
using Armali.Horizon.IO;

namespace Armali.Horizon.Althes.UI.Services;

/// <summary>
/// Servicio scoped (uno por circuito Blazor) que mantiene suscripciones al
/// canal de inbox del usuario de los proyectos cargados. Cuando llega un
/// <see cref="AgentInboxMessage"/>, dispara <see cref="OnMessage"/> para que
/// la página suscrita actualice su estado.
/// <para>
/// El registro de suscripciones se hace una vez al cargar la lista de proyectos
/// (<see cref="SubscribeAll"/>). La baja se hace en <see cref="IDisposable.Dispose"/>.
/// </para>
/// </summary>
public class AlthesLiveSubscription : IDisposable
{
    private readonly HorizonEventService Events;
    private readonly List<(string channel, Action<AgentInboxMessage> handler)> Subscriptions = [];
    
    /// <summary>
    /// Se dispara cuando llega un mensaje en cualquier proyecto suscrito.
    /// El primer string es el ProjectId; el segundo es el mensaje.
    /// </summary>
    public event Action<string, AgentInboxMessage>? OnMessage;
    
    public AlthesLiveSubscription(HorizonEventService events) => Events = events;
    
    /// <summary>
    /// Suscribe al canal de inbox del usuario de cada proyecto.
    /// Llamar una sola vez al inicializar la página Conversations.
    /// </summary>
    public void SubscribeAll(IEnumerable<string> projectIds)
    {
        foreach (var pid in projectIds)
            Subscribe(pid);
    }
    
    public void Subscribe(string projectId)
    {
        var channel = AlthesChannels.UserInbox(projectId);
        // Evitar duplicados
        if (Subscriptions.Any(s => s.channel == channel)) return;
        
        void Handler(AgentInboxMessage msg) => OnMessage?.Invoke(projectId, msg);
        Events.Subscribe<AgentInboxMessage>(channel, Handler);
        Subscriptions.Add((channel, Handler));
    }
    
    public void Dispose()
    {
        // HorizonEventService no expone Unsubscribe en v1; las suscripciones
        // se limpian con el scope (el circuito Blazor vive mientras el cliente
        // mantiene la conexión SignalR).
        Subscriptions.Clear();
    }
}

