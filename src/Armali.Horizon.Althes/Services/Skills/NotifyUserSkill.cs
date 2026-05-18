using System.Text.Json;
using Armali.Horizon.Althes.Configuration;
using Armali.Horizon.Contracts.Althes;
using Armali.Horizon.IO;
using Microsoft.Extensions.Options;

namespace Armali.Horizon.Althes.Services.Skills;

/// <summary>
/// Informa al usuario (fire-and-forget): publica un evento en el canal de
/// inbox del usuario para que la UI futura lo muestre. No espera respuesta.
/// </summary>
public class NotifyUserSkill : IAgentSkill
{
    private readonly HorizonEventService Events;
    private readonly AlthesOptions Options;
    
    public NotifyUserSkill(HorizonEventService events, IOptions<AlthesOptions> options)
    {
        Events = events;
        Options = options.Value;
    }
    
    public string Name => "notify_user";
    public string Description => "Envía un mensaje informativo al usuario. No espera respuesta.";
    public string ArgsSchema => """{ "content": "string" }""";
    
    public async Task<SkillOutcome> ExecuteAsync(JsonElement args, SkillContext ctx, CancellationToken ct)
    {
        var content = args.TryGetProperty("content", out var c) ? c.GetString() ?? "" : "";
        
        // Mensaje sobre el canal de inbox del usuario para clientes en vivo.
        await Events.PublishAsync(AlthesChannels.UserInbox(Options.ProjectId), new AgentInboxMessage
        {
            Kind = AgentInboxKind.Notify,
            Sender = ctx.AgentName,
            Content = content,
            ConversationId = ctx.ConversationId,
            RunId = ctx.RunId,
        });
        
        return new SkillOutcome
        {
            Kind = SkillOutcomeKind.Continue,
            FeedbackToLlm = $"[user notified] {content}",
            UserVisibleContent = content,
            Visibility = Model.UserVisibility.Outgoing,
        };
    }
}

