using System.Text.Json;
using Armali.Horizon.Althes.Configuration;
using Armali.Horizon.Contracts.Althes;
using Armali.Horizon.IO;
using Microsoft.Extensions.Options;

namespace Armali.Horizon.Althes.Services.Skills;

/// <summary>
/// Envía un mensaje fire-and-forget a otro agente del mismo proyecto.
/// </summary>
public class NotifyAgentSkill : IAgentSkill
{
    private readonly HorizonEventService Events;
    private readonly AlthesOptions Options;
    
    public NotifyAgentSkill(HorizonEventService events, IOptions<AlthesOptions> options)
    {
        Events = events;
        Options = options.Value;
    }
    
    public string Name => "notify_agent";
    public string Description => "Envía un mensaje a otro agente sin esperar respuesta.";
    public string ArgsSchema => """{ "agent": "string", "content": "string" }""";
    
    public async Task<SkillOutcome> ExecuteAsync(JsonElement args, SkillContext ctx, CancellationToken ct)
    {
        var target = args.TryGetProperty("agent", out var a) ? a.GetString() ?? "" : "";
        var content = args.TryGetProperty("content", out var c) ? c.GetString() ?? "" : "";
        
        if (string.IsNullOrWhiteSpace(target))
            return SkillOutcome.Continue("[notify_agent error] missing 'agent'");
        if (ctx.AllowedRecipients.Count > 0 && !ctx.AllowedRecipients.Contains(target))
            return SkillOutcome.Continue($"[notify_agent error] '{target}' no está en allowedRecipients");
        
        await Events.PublishAsync(AlthesChannels.AgentInbox(Options.ProjectId, target), new AgentInboxMessage
        {
            Kind = AgentInboxKind.Notify,
            Sender = ctx.AgentName,
            Content = content,
        });
        
        return SkillOutcome.Continue($"[sent to {target}] {content}");
    }
}

