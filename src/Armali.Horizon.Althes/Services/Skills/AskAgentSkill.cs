using System.Text.Json;
using Armali.Horizon.Althes.Configuration;
using Armali.Horizon.Contracts.Althes;
using Armali.Horizon.IO;
using Microsoft.Extensions.Options;

namespace Armali.Horizon.Althes.Services.Skills;

/// <summary>
/// Pregunta a otro agente esperando respuesta. Publica una <see cref="AgentInboxKind.Question"/>
/// con un correlation id y termina el burst. Cuando el destinatario responda
/// (vía <see cref="NotifyAgentSkill"/> o automáticamente), el sistema entregará
/// un <see cref="AgentInboxKind.Answer"/> de vuelta al agente origen.
/// <para>
/// En v1 las respuestas se generan implícitamente: el primer mensaje que el
/// destinatario envíe al origen con el mismo CorrelationId cuenta como reply.
/// Si no hay respuesta en <c>Limits.AwaitTimeoutSeconds</c>, el runtime inyecta
/// un <see cref="AgentInboxKind.Timeout"/>.
/// </para>
/// </summary>
public class AskAgentSkill : IAgentSkill
{
    private readonly HorizonEventService Events;
    private readonly AlthesOptions Options;
    
    public AskAgentSkill(HorizonEventService events, IOptions<AlthesOptions> options)
    {
        Events = events;
        Options = options.Value;
    }
    
    public string Name => "ask_agent";
    public string Description => "Plantea una pregunta a otro agente y espera su respuesta.";
    public string ArgsSchema => """{ "agent": "string", "question": "string" }""";
    
    public async Task<SkillOutcome> ExecuteAsync(JsonElement args, SkillContext ctx, CancellationToken ct)
    {
        var target = args.TryGetProperty("agent", out var a) ? a.GetString() ?? "" : "";
        var question = args.TryGetProperty("question", out var q) ? q.GetString() ?? "" : "";
        
        if (string.IsNullOrWhiteSpace(target))
            return SkillOutcome.Continue("[ask_agent error] missing 'agent'");
        if (ctx.AllowedRecipients.Count > 0 && !ctx.AllowedRecipients.Contains(target))
            return SkillOutcome.Continue($"[ask_agent error] '{target}' no está en allowedRecipients");
        
        var correlationId = Guid.NewGuid().ToString("N");
        await Events.PublishAsync(AlthesChannels.AgentInbox(Options.ProjectId, target), new AgentInboxMessage
        {
            Kind = AgentInboxKind.Question,
            Sender = ctx.AgentName,
            Content = question,
            CorrelationId = correlationId,
        });
        
        return new SkillOutcome
        {
            Kind = SkillOutcomeKind.Await,
            FeedbackToLlm = $"[waiting answer from {target} to: {question}] correlation={correlationId}",
            CorrelationId = correlationId,
            IsUserAwait = false,
        };
    }
}

