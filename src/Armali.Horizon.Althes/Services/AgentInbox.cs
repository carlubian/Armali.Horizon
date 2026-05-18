using System.Collections.Concurrent;
using System.Threading.Channels;
using Armali.Horizon.Contracts.Althes;

namespace Armali.Horizon.Althes.Services;

/// <summary>
/// Cola FIFO en memoria para los mensajes entrantes de un único agente.
/// El runtime del agente la lee con <c>WaitToReadAsync</c> para dormirse
/// sin coste de CPU cuando no hay trabajo pendiente.
/// </summary>
public class AgentInbox
{
    private readonly Channel<AgentInboxMessage> Channel = System.Threading.Channels.Channel.CreateUnbounded<AgentInboxMessage>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
    
    public ChannelReader<AgentInboxMessage> Reader => Channel.Reader;
    public int Pending => Channel.Reader.Count;
    
    public void Enqueue(AgentInboxMessage msg) => Channel.Writer.TryWrite(msg);
    public void Complete() => Channel.Writer.TryComplete();
}

/// <summary>
/// Registro global de inboxes por nombre de agente. Lo usan los handlers IO
/// externos, los puentes pub/sub del bus interno y los propios runtimes.
/// </summary>
public class AgentInboxRouter
{
    private readonly ConcurrentDictionary<string, AgentInbox> Inboxes = new(StringComparer.OrdinalIgnoreCase);
    
    public AgentInbox GetOrCreate(string agentName) =>
        Inboxes.GetOrAdd(agentName, _ => new AgentInbox());
    
    public bool TryGet(string agentName, out AgentInbox inbox) =>
        Inboxes.TryGetValue(agentName, out inbox!);
    
    public IEnumerable<string> KnownAgents => Inboxes.Keys;
    
    /// <summary>Entrega un mensaje a un agente. No-op si no existe.</summary>
    public bool Deliver(string agentName, AgentInboxMessage msg)
    {
        if (!Inboxes.TryGetValue(agentName, out var inbox)) return false;
        inbox.Enqueue(msg);
        return true;
    }
}

