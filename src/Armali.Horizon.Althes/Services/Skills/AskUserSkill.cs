using System.Text.Json;
using Armali.Horizon.Althes.Configuration;
using Armali.Horizon.Althes.Model;
using Armali.Horizon.Contracts.Althes;
using Armali.Horizon.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Armali.Horizon.Althes.Services.Skills;

/// <summary>
/// Plantea una pregunta al usuario y espera respuesta. Persiste un
/// <see cref="UserQuery"/> pendiente y termina el burst del agente: cuando el
/// usuario responda vía <c>AnswerUserQueryRequest</c>, el handler inyectará
/// un nuevo <see cref="AgentInboxKind.UserAnswer"/> en el inbox del agente.
/// </summary>
public class AskUserSkill : IAgentSkill
{
    private readonly IDbContextFactory<AlthesDbContext> Factory;
    private readonly HorizonEventService Events;
    private readonly AlthesOptions Options;
    
    public AskUserSkill(IDbContextFactory<AlthesDbContext> factory, HorizonEventService events, IOptions<AlthesOptions> options)
    {
        Factory = factory;
        Events = events;
        Options = options.Value;
    }
    
    public string Name => "ask_user";
    public string Description =>
        "Plantea una pregunta al usuario y espera su respuesta. El agente se " +
        "suspenderá hasta que el usuario conteste o se agote el timeout.";
    public string ArgsSchema => """{ "question": "string" }""";
    
    public async Task<SkillOutcome> ExecuteAsync(JsonElement args, SkillContext ctx, CancellationToken ct)
    {
        var question = args.TryGetProperty("question", out var q) ? q.GetString() ?? "" : "";
        var correlationId = Guid.NewGuid().ToString("N");
        
        await using var db = await Factory.CreateDbContextAsync(ct);
        var userQuery = new UserQuery
        {
            AgentName = ctx.AgentName,
            RunId = ctx.RunId,
            ConversationId = ctx.ConversationId,
            CorrelationId = correlationId,
            Question = question,
        };
        db.UserQueries.Add(userQuery);
        await db.SaveChangesAsync(ct);
        
        // Publicar en el canal de inbox del usuario para UIs en vivo.
        await Events.PublishAsync(AlthesChannels.UserInbox(Options.ProjectId), new AgentInboxMessage
        {
            Kind = AgentInboxKind.Question,
            Sender = ctx.AgentName,
            Content = question,
            CorrelationId = correlationId,
            ConversationId = ctx.ConversationId,
            RunId = ctx.RunId,
            UserQueryId = userQuery.Id,
        });
        
        return new SkillOutcome
        {
            Kind = SkillOutcomeKind.Await,
            FeedbackToLlm = $"[waiting user answer to: {question}]",
            UserVisibleContent = question,
            Visibility = Model.UserVisibility.Question,
            CorrelationId = correlationId,
            UserQueryId = userQuery.Id,
            IsUserAwait = true,
        };
    }
}

