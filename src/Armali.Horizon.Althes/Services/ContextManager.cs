using Armali.Horizon.Althes.Configuration;
using Armali.Horizon.Althes.Model;
using Armali.Horizon.Althes.Services.Llm;
using Microsoft.Extensions.Options;

namespace Armali.Horizon.Althes.Services;

/// <summary>
/// Vigila el llenado de la ventana de contexto de un run. Cuando se cruza el
/// soft-limit pide al LLM un resumen de los mensajes antiguos y los sustituye
/// por un único mensaje system. Cuando se cruza el hard-limit, devuelve la
/// orden de cerrar el run y abrir uno nuevo.
/// </summary>
public class ContextManager
{
    private readonly AlthesOptions Options;
    private readonly ILlmClient Llm;
    private readonly ConversationStore Store;
    private readonly ILogger<ContextManager> Logger;
    
    public ContextManager(IOptions<AlthesOptions> options, ILlmClient llm, ConversationStore store, ILogger<ContextManager> logger)
    {
        Options = options.Value;
        Llm = llm;
        Store = store;
        Logger = logger;
    }
    
    public enum Action { None, Compacted, NeedNewRun }
    
    public async Task<Action> EnforceAsync(AgentRun run, string? agentModel, CancellationToken ct)
    {
        var window = Options.Llm.ContextWindow;
        var soft = (int)(window * Options.Context.SoftLimitFraction);
        var hard = (int)(window * Options.Context.HardLimitFraction);
        
        if (run.TokenCount < soft) return Action.None;
        
        // Por encima de hard ya no merece la pena intentar compactar otra vez:
        // forzamos cierre y nuevo run.
        if (run.TokenCount >= hard && run.Status == AgentRunStatus.Compacted)
            return Action.NeedNewRun;
        
        var messages = await Store.GetMessagesAsync(run.Id, ct);
        var keep = Options.Context.KeepRecentMessages;
        if (messages.Count <= keep + 1) return Action.None;  // nada que compactar
        
        // Mantenemos system inicial(es) + N recientes; el resto se resume.
        var systemInitial = messages.Where(m => m.Role == AgentMessageRole.System && m.Skill != "summary").Take(1).ToList();
        var recent = messages.TakeLast(keep).ToList();
        var toSummarize = messages.Except(systemInitial).Except(recent).ToList();
        
        if (toSummarize.Count == 0) return Action.None;
        
        try
        {
            var prompt = $$"""
Resume de forma concisa (máximo 200 palabras) los hechos, decisiones y datos
relevantes de los siguientes mensajes para que el agente pueda continuar la
conversación sin perder contexto crítico. No incluyas pensamientos triviales.

{{string.Join("\n---\n", toSummarize.Select(m => $"[{m.Role}/{m.Skill ?? "-"}/{m.Sender ?? "-"}] {m.Content}"))}}
""";
            var summary = await Llm.CompleteAsync(
                [new LlmMessage(LlmMessageRole.System, "Eres un asistente que resume conversaciones."),
                 new LlmMessage(LlmMessageRole.User, prompt)],
                new LlmOptions { Model = agentModel, Temperature = 0.2, MaxOutputTokens = 400 },
                ct);
            
            await Store.ReplaceWithSummaryAsync(
                run.Id,
                toSummarize.Select(m => m.Id).ToList(),
                $"[run summary so far]\n{summary.Content}",
                TokenEstimator.Estimate(summary.Content),
                ct);
            
            Logger.LogInformation("Run {RunId} compactado: {N} mensajes → 1 summary.", run.Id, toSummarize.Count);
            return Action.Compacted;
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Compresión fallida del run {RunId}. Forzando cierre.", run.Id);
            return Action.NeedNewRun;
        }
    }
}

