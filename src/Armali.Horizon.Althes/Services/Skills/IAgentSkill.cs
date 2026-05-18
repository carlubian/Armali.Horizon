using System.Text.Json;
using Armali.Horizon.Althes.Model;

namespace Armali.Horizon.Althes.Services.Skills;

/// <summary>
/// Resultado de la ejecución de una skill. Indica cómo debe continuar el
/// runtime del agente:
/// <list type="bullet">
///   <item><see cref="Continue"/>: se ejecuta otro turno LLM con el resultado.</item>
///   <item><see cref="EndBurst"/>: el burst termina; el agente vuelve a esperar inbox.</item>
///   <item><see cref="Await"/>: se queda esperando una respuesta correlacionada.
///         El runtime termina el burst; la respuesta llegará como nuevo item de inbox.</item>
/// </list>
/// </summary>
public enum SkillOutcomeKind
{
    Continue,
    EndBurst,
    Await,
}

public class SkillOutcome
{
    public SkillOutcomeKind Kind { get; init; } = SkillOutcomeKind.EndBurst;
    /// <summary>Texto que se inyecta al contexto del LLM como mensaje tool/system.</summary>
    public string FeedbackToLlm { get; init; } = "";
    /// <summary>
    /// Texto "limpio" (sin prefijos de tipo "[user notified] ...") para mostrar
    /// en la UI cuando esta skill produce un mensaje visible al humano.
    /// Null si la skill no produce nada destinado al usuario.
    /// </summary>
    public string? UserVisibleContent { get; init; }
    /// <summary>Visibilidad para la UI. Default = Hidden (skill puramente interna).</summary>
    public UserVisibility Visibility { get; init; } = UserVisibility.Hidden;
    /// <summary>Correlation id propagado (para ask_user/ask_agent).</summary>
    public string? CorrelationId { get; init; }
    /// <summary>UserQuery asociada cuando la skill plantea una <see cref="UserVisibility.Question"/>.</summary>
    public string? UserQueryId { get; init; }
    /// <summary>Indica si esta skill produce un await de tipo "user query" (para timeout vs auto-resolve).</summary>
    public bool IsUserAwait { get; init; }
    
    public static SkillOutcome Continue(string feedback) => new()
    {
        Kind = SkillOutcomeKind.Continue, FeedbackToLlm = feedback,
    };
    
    public static SkillOutcome EndBurst(string feedback) => new()
    {
        Kind = SkillOutcomeKind.EndBurst, FeedbackToLlm = feedback,
    };
    
    public static SkillOutcome Await(string feedback) => new()
    {
        Kind = SkillOutcomeKind.Await, FeedbackToLlm = feedback,
    };
}

/// <summary>
/// Contexto que recibe una skill cuando es invocada por el runtime de un agente.
/// </summary>
public class SkillContext
{
    public required string AgentName { get; init; }
    public required string RunId { get; init; }
    public required string ConversationId { get; init; }
    public required string ProjectId { get; init; }
    /// <summary>Subset de agentes con los que este agente puede comunicarse.</summary>
    public required IReadOnlySet<string> AllowedRecipients { get; init; }
    public required IServiceProvider Services { get; init; }
}

/// <summary>
/// Contrato de skill modular. Para añadir una nueva acción posible para los
/// agentes basta con implementar esta interfaz y registrar la clase en DI.
/// </summary>
public interface IAgentSkill
{
    /// <summary>Identificador canónico, e.g. "ask_user".</summary>
    string Name { get; }
    /// <summary>Descripción humana que se inyecta al prompt para que el LLM la conozca.</summary>
    string Description { get; }
    /// <summary>Esquema JSON de los argumentos esperados (resumen para el prompt).</summary>
    string ArgsSchema { get; }
    
    Task<SkillOutcome> ExecuteAsync(JsonElement args, SkillContext context, CancellationToken ct);
}

