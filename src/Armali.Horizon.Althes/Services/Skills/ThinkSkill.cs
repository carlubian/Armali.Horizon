using System.Text.Json;

namespace Armali.Horizon.Althes.Services.Skills;

/// <summary>
/// Pensamiento autónomo: el agente registra una reflexión interna que se
/// añade al contexto del LLM. No tiene efectos externos. Se usa para
/// encadenar razonamiento antes de actuar.
/// </summary>
public class ThinkSkill : IAgentSkill
{
    public string Name => "think";
    public string Description => "Anota un razonamiento interno. No envía nada al exterior.";
    public string ArgsSchema => """{ "thought": "string" }""";
    
    public Task<SkillOutcome> ExecuteAsync(JsonElement args, SkillContext ctx, CancellationToken ct)
    {
        var thought = args.TryGetProperty("thought", out var t) ? t.GetString() ?? "" : "";
        // El "feedback" al LLM es el propio pensamiento, así sigue presente en
        // los próximos turnos sin necesidad de releer la BD.
        return Task.FromResult(SkillOutcome.Continue($"[thought registered] {thought}"));
    }
}

