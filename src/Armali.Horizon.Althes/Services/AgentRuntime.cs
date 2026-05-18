using System.Collections.Concurrent;
using System.Text.Json;
using Armali.Horizon.Althes.Configuration;
using Armali.Horizon.Althes.Model;
using Armali.Horizon.Althes.Services.Llm;
using Armali.Horizon.Althes.Services.Skills;
using Armali.Horizon.Contracts.Althes;
using Microsoft.Extensions.Options;

namespace Armali.Horizon.Althes.Services;

/// <summary>
/// Bucle de ejecución de un único agente. Procesa la cola FIFO de su
/// <see cref="AgentInbox"/>: por cada item, abre o reutiliza un run, persiste
/// el mensaje entrante y dispara una secuencia de turnos LLM hasta que el
/// agente decida terminar, entre en estado <i>await</i> o se agote el burst.
/// <para>
/// Se instancia uno por agente desde <see cref="AgentRuntimeHost"/>.
/// </para>
/// </summary>
public class AgentRuntime
{
    // ── Dependencias ─────────────────────────────────────────────────
    private readonly AgentConfigEntry Definition;
    private readonly AgentInbox Inbox;
    private readonly IServiceProvider RootServices;
    private readonly AlthesOptions Options;
    private readonly ILogger Logger;
    
    // ── Estado en vivo ───────────────────────────────────────────────
    /// <summary>Awaits pendientes: correlation id → tipo esperado + deadline.</summary>
    private readonly ConcurrentDictionary<string, PendingAwait> Pendings = new();
    
    public string AgentName => Definition.Name;
    public string? ActiveRunId { get; private set; }
    public bool IsAwaiting => !Pendings.IsEmpty;
    public int PendingInboxItems => Inbox.Pending;
    
    public AgentRuntime(
        AgentConfigEntry definition,
        AgentInbox inbox,
        IServiceProvider rootServices,
        IOptions<AlthesOptions> options,
        ILoggerFactory loggerFactory)
    {
        Definition = definition;
        Inbox = inbox;
        RootServices = rootServices;
        Options = options.Value;
        Logger = loggerFactory.CreateLogger($"Althes.Agent.{definition.Name}");
    }
    
    public async Task RunLoopAsync(CancellationToken ct)
    {
        Logger.LogInformation("Runtime arrancado.");
        try
        {
            while (await Inbox.Reader.WaitToReadAsync(ct))
            {
                while (Inbox.Reader.TryRead(out var item))
                {
                    try
                    {
                        await ProcessItemAsync(item, ct);
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, "Error procesando item de inbox.");
                    }
                }
            }
        }
        catch (OperationCanceledException) { }
        Logger.LogInformation("Runtime detenido.");
    }
    
    private async Task ProcessItemAsync(AgentInboxMessage item, CancellationToken ct)
    {
        // Si el item trae un correlation id que coincide con un await pendiente,
        // promovemos el Kind a Answer/UserAnswer para que el LLM lo vea como tal.
        if (item.CorrelationId is not null && Pendings.TryRemove(item.CorrelationId, out var pending))
        {
            item.Kind = pending.IsUserQuery ? AgentInboxKind.UserAnswer : AgentInboxKind.Answer;
        }
        
        using var scope = RootServices.CreateScope();
        var sp = scope.ServiceProvider;
        var store = sp.GetRequiredService<ConversationStore>();
        var ctxMgr = sp.GetRequiredService<ContextManager>();
        var llm = sp.GetRequiredService<ILlmClient>();
        var skills = sp.GetRequiredService<SkillRegistry>();
        var limiter = sp.GetRequiredService<RateLimiter>();
        
        // Obtener (o crear) la conversación activa y el run del agente.
        var conversation = await store.GetOrCreateActiveConversationAsync(ct: ct);
        var run = await store.GetActiveRunAsync(AgentName, ct);
        if (run is null || run.ConversationId != conversation.Id)
        {
            // No hay run, o pertenece a una conversación cerrada → arrancar uno nuevo
            // dentro de la conversación activa actual.
            run = await StartFreshRun(store, conversation.Id, AgentRunTrigger.UserMessage, null, ct);
        }
        ActiveRunId = run.Id;
        
        // Persistir el mensaje entrante como User (o Tool si es respuesta sintética).
        var inboundRole = item.Kind switch
        {
            AgentInboxKind.Answer => AgentMessageRole.Tool,
            AgentInboxKind.UserAnswer => AgentMessageRole.Tool,
            AgentInboxKind.Timeout => AgentMessageRole.Tool,
            _ => AgentMessageRole.User,
        };
        var inboundLabel = item.Kind switch
        {
            AgentInboxKind.Question => $"[{item.Sender} asks] {item.Content}",
            AgentInboxKind.Notify => $"[{item.Sender} says] {item.Content}",
            AgentInboxKind.Answer => $"[answer from {item.Sender} to your previous ask_agent] {item.Content}",
            AgentInboxKind.UserAnswer => $"[user answer to your previous ask_user] {item.Content}",
            AgentInboxKind.Timeout => $"[timeout waiting for {item.Sender}] {item.Content}",
            _ => item.Content,
        };
        // Visibilidad UI: sólo los mensajes del usuario humano o respuestas humanas.
        var (inboundVisibility, inboundRaw) = item.Sender == "user"
            ? item.Kind == AgentInboxKind.UserAnswer
                ? (UserVisibility.Answer, item.Content)
                : (UserVisibility.Incoming, item.Content)
            : (UserVisibility.Hidden, (string?)null);
        
        await store.AppendAsync(new AgentMessage
        {
            RunId = run.Id,
            AgentName = AgentName,
            Role = inboundRole,
            Sender = item.Sender,
            Content = inboundLabel,
            RawContent = inboundRaw,
            Visibility = inboundVisibility,
            TokenCount = TokenEstimator.Estimate(inboundLabel),
            CorrelationId = item.CorrelationId,
            Skill = item.Kind == AgentInboxKind.Question ? "incoming_question" : null,
        }, ct);
        run.TokenCount += TokenEstimator.Estimate(inboundLabel);
        
        // Burst de turnos LLM.
        var skillsForAgent = skills.ForAgent(Definition.AllowedSkills ?? []).ToList();
        var allowedRecipients = (Definition.AllowedRecipients ?? []).ToHashSet(StringComparer.OrdinalIgnoreCase);
        
        for (var turn = 0; turn < Options.Limits.MaxBurstTurns; turn++)
        {
            // Rate limit
            if (!limiter.TryConsume(AgentName, Definition.MaxActionsPerHour))
            {
                await store.AppendAsync(new AgentMessage
                {
                    RunId = run.Id,
                    AgentName = AgentName,
                    Role = AgentMessageRole.System,
                    Skill = "rate_limit",
                    Content = "Rate limit alcanzado. Burst terminado.",
                    TokenCount = 12,
                }, ct);
                Logger.LogWarning("Rate limit alcanzado.");
                break;
            }
            
            // Gestión del límite de contexto antes del próximo turno
            var ctxAction = await ctxMgr.EnforceAsync(run, Definition.Model, ct);
            if (ctxAction == ContextManager.Action.NeedNewRun)
            {
                await CloseAndStartNewRun(store, llm, run, AgentRunTrigger.ContextLimit, ct);
                run = (await store.GetActiveRunAsync(AgentName, ct))!;
                ActiveRunId = run.Id;
            }
            // Construir mensajes para el LLM
            var history = await store.GetMessagesAsync(run.Id, ct);
            var llmMessages = BuildLlmMessages(history, skillsForAgent);
            
            // Llamada LLM con timeout duro
            using var turnCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            turnCts.CancelAfter(TimeSpan.FromSeconds(Options.Limits.MaxRequestSeconds));
            
            LlmResponse llmResp;
            try
            {
                llmResp = await llm.CompleteAsync(llmMessages,
                    new LlmOptions { Model = Definition.Model, JsonMode = true,
                        Temperature = Options.Llm.Temperature, MaxOutputTokens = Options.Llm.MaxOutputTokens },
                    turnCts.Token);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Fallo de LLM en turno {Turn}.", turn);
                await store.AppendAsync(new AgentMessage
                {
                    RunId = run.Id, AgentName = AgentName, Role = AgentMessageRole.System,
                    Skill = "llm_error", Content = $"LLM error: {ex.Message}", TokenCount = 20,
                }, ct);
                break;
            }
            
            // Persistir la respuesta cruda del LLM como mensaje assistant.
            await store.AppendAsync(new AgentMessage
            {
                RunId = run.Id, AgentName = AgentName, Role = AgentMessageRole.Assistant,
                Content = llmResp.Content, TokenCount = llmResp.CompletionTokens > 0 ? llmResp.CompletionTokens : TokenEstimator.Estimate(llmResp.Content),
            }, ct);
            
            // Parsear decisión { skill, args }
            if (!TryParseDecision(llmResp.Content, out var skillName, out var argsElement))
            {
                Logger.LogWarning("Respuesta LLM no parseable como decisión JSON. Burst terminado.");
                break;
            }
            
            if (string.Equals(skillName, "end", StringComparison.OrdinalIgnoreCase))
                break;
            
            var skill = skills.Get(skillName);
            if (skill is null || !skillsForAgent.Contains(skill))
            {
                await store.AppendAsync(new AgentMessage
                {
                    RunId = run.Id, AgentName = AgentName, Role = AgentMessageRole.Tool,
                    Skill = "invalid_skill", Content = $"Skill '{skillName}' no disponible para este agente.",
                    TokenCount = 20,
                }, ct);
                continue;
            }
            
            // Ejecutar skill
            SkillOutcome outcome;
            try
            {
                outcome = await skill.ExecuteAsync(argsElement, new SkillContext
                {
                    AgentName = AgentName,
                    RunId = run.Id,
                    ConversationId = run.ConversationId,
                    ProjectId = Options.ProjectId,
                    AllowedRecipients = allowedRecipients,
                    Services = sp,
                }, turnCts.Token);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Skill '{Skill}' lanzó excepción.", skillName);
                outcome = SkillOutcome.EndBurst($"[skill error] {ex.Message}");
            }
            
            // Persistir feedback de la skill como mensaje Tool, propagando los
            // metadatos user-facing producidos por la propia skill.
            await store.AppendAsync(new AgentMessage
            {
                RunId = run.Id, AgentName = AgentName, Role = AgentMessageRole.Tool,
                Skill = skillName, Content = outcome.FeedbackToLlm,
                RawContent = outcome.UserVisibleContent,
                Visibility = outcome.Visibility,
                CorrelationId = outcome.CorrelationId,
                UserQueryId = outcome.UserQueryId,
                Sender = AgentName,
                TokenCount = TokenEstimator.Estimate(outcome.FeedbackToLlm),
            }, ct);
            
            if (outcome.Kind == SkillOutcomeKind.EndBurst) break;
            if (outcome.Kind == SkillOutcomeKind.Await)
            {
                // Registrar el await pendiente y programar el timeout sintético.
                // ask_user no usa Pendings: el matching usa el correlation id
                // persistido en UserQuery → no necesita ser promovido en el inbox.
                if (!outcome.IsUserAwait && outcome.CorrelationId is { } corrId)
                {
                    var deadline = DateTime.UtcNow.AddSeconds(Options.Limits.AwaitTimeoutSeconds);
                    Pendings[corrId] = new PendingAwait(corrId, false, deadline);
                    _ = ScheduleTimeoutAsync(corrId, deadline - DateTime.UtcNow, ct);
                }
                break;
            }
        }
        
        ActiveRunId = run.Id;
    }
    
    /// <summary>Construye los mensajes para el LLM a partir del historial.</summary>
    private List<LlmMessage> BuildLlmMessages(List<AgentMessage> history, List<IAgentSkill> available)
    {
        var msgs = new List<LlmMessage>(history.Count + 2);
        
        // System prompt = prompt del agente + instrucciones de formato de salida.
        var skillsDoc = string.Join("\n", available.Select(s => $"- {s.Name}: {s.Description} args={s.ArgsSchema}"));
        msgs.Add(new LlmMessage(LlmMessageRole.System, $$"""
{{Definition.SystemPrompt}}

Eres un agente dentro de Althes. En cada turno debes responder EXCLUSIVAMENTE
con un objeto JSON con la forma:
  { "skill": "<nombre>", "args": { ... } }
Para indicar que has terminado y quieres volver a esperar mensajes, responde:
  { "skill": "end" }

Skills disponibles:
{{skillsDoc}}

Reglas:
- No incluyas texto fuera del JSON.
- Usa "think" para razonar; el resultado se añade al contexto sin efectos externos.
- "ask_user" y "ask_agent" suspenden el burst hasta recibir respuesta.
"""));
        
        foreach (var m in history)
        {
            var role = m.Role switch
            {
                AgentMessageRole.System => LlmMessageRole.System,
                AgentMessageRole.User => LlmMessageRole.User,
                AgentMessageRole.Assistant => LlmMessageRole.Assistant,
                AgentMessageRole.Tool => LlmMessageRole.User,  // tool feedback como user
                _ => LlmMessageRole.User,
            };
            msgs.Add(new LlmMessage(role, m.Content));
        }
        return msgs;
    }
    
    private static bool TryParseDecision(string raw, out string skill, out JsonElement args)
    {
        skill = "";
        args = default;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        try
        {
            // Tolerar bloques de código si el LLM se desvía.
            var trimmed = raw.Trim();
            if (trimmed.StartsWith("```"))
            {
                var firstNl = trimmed.IndexOf('\n');
                trimmed = trimmed[(firstNl + 1)..];
                var lastFence = trimmed.LastIndexOf("```", StringComparison.Ordinal);
                if (lastFence >= 0) trimmed = trimmed[..lastFence];
            }
            using var doc = JsonDocument.Parse(trimmed);
            if (!doc.RootElement.TryGetProperty("skill", out var s)) return false;
            skill = s.GetString() ?? "";
            args = doc.RootElement.TryGetProperty("args", out var a) ? a.Clone() : default;
            return !string.IsNullOrEmpty(skill);
        }
        catch
        {
            return false;
        }
    }
    
    private static (string? correlationId, bool isUser) ExtractCorrelation(string skill, JsonElement args, string feedback)
    {
        // Obsoleto: las skills propagan correlation id directamente en SkillOutcome.
        return (null, false);
    }
    
    private async Task ScheduleTimeoutAsync(string correlationId, TimeSpan delay, CancellationToken ct)
    {
        try
        {
            await Task.Delay(delay, ct);
            if (Pendings.TryRemove(correlationId, out var pending))
            {
                Inbox.Enqueue(new AgentInboxMessage
                {
                    Kind = AgentInboxKind.Timeout,
                    Sender = "system",
                    Content = $"timeout after {delay.TotalSeconds:F0}s",
                    CorrelationId = correlationId,
                });
            }
        }
        catch (OperationCanceledException) { }
    }
    
    private async Task<AgentRun> StartFreshRun(ConversationStore store, string conversationId, AgentRunTrigger trigger, string? carryOverSummary, CancellationToken ct)
    {
        var run = await store.StartRunAsync(AgentName, conversationId, trigger, ct);
        if (!string.IsNullOrEmpty(carryOverSummary))
        {
            await store.AppendAsync(new AgentMessage
            {
                RunId = run.Id,
                AgentName = AgentName,
                Role = AgentMessageRole.System,
                Skill = "carryover",
                Content = $"[carry-over summary from previous run]\n{carryOverSummary}",
                TokenCount = TokenEstimator.Estimate(carryOverSummary),
                Visibility = UserVisibility.Hidden,
            }, ct);
            run.TokenCount += TokenEstimator.Estimate(carryOverSummary);
        }
        return run;
    }
    
    private async Task CloseAndStartNewRun(ConversationStore store, ILlmClient llm, AgentRun current, AgentRunTrigger trigger, CancellationToken ct)
    {
        var summary = await SummarizeRunAsync(llm, store, current, ct);
        await store.CloseRunAsync(current.Id, AgentRunStatus.Completed, summary, ct);
        // El nuevo run vive en la misma conversación (el cierre por límite de
        // contexto no termina la conversación).
        await StartFreshRun(store, current.ConversationId, trigger,
            Definition.CarryOverSummary ? summary : null, ct);
    }
    
    /// <summary>Genera un resumen del run completo. Se llama siempre al cerrar.</summary>
    private async Task<string?> SummarizeRunAsync(ILlmClient llm, ConversationStore store, AgentRun run, CancellationToken ct)
    {
        try
        {
            var msgs = await store.GetMessagesAsync(run.Id, ct);
            if (msgs.Count == 0) return null;
            var prompt = $$"""
Resume en menos de 200 palabras los hechos, decisiones y datos clave del
siguiente run del agente {{AgentName}}. Pensado para servir como contexto
inicial de su próximo run.

{{string.Join("\n---\n", msgs.Take(200).Select(m => $"[{m.Role}/{m.Skill ?? "-"}] {m.Content}"))}}
""";
            var resp = await llm.CompleteAsync(
                [new LlmMessage(LlmMessageRole.System, "Resumes runs de agentes."),
                 new LlmMessage(LlmMessageRole.User, prompt)],
                new LlmOptions { Model = Definition.Model, Temperature = 0.2, MaxOutputTokens = 400 },
                ct);
            return resp.Content;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Resumen del run {RunId} fallido.", run.Id);
            return null;
        }
    }
    
    public async Task CloseActiveRunAsync(CancellationToken ct)
    {
        using var scope = RootServices.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<ConversationStore>();
        var llm = scope.ServiceProvider.GetRequiredService<ILlmClient>();
        var run = await store.GetActiveRunAsync(AgentName, ct);
        if (run is null) return;
        var summary = await SummarizeRunAsync(llm, store, run, ct);
        await store.CloseRunAsync(run.Id, AgentRunStatus.Completed, summary, ct);
        ActiveRunId = null;
    }
    
    private record PendingAwait(string CorrelationId, bool IsUserQuery, DateTime Deadline);
}

