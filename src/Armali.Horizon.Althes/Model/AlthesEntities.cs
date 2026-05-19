using System.ComponentModel.DataAnnotations;

namespace Armali.Horizon.Althes.Model;

/// <summary>
/// Definición persistida de un agente. Espejo de la entrada en el archivo de
/// configuración servido por Autoconfig: se sincroniza al arrancar y queda
/// como respaldo para el caso de no poder recargar la config.
/// </summary>
public class Agent
{
    [Key]
    public string Name { get; set; } = string.Empty;
    /// <summary>Descripción corta del rol del agente, visible para otros agentes.</summary>
    public string? Description { get; set; }
    public string SystemPrompt { get; set; } = string.Empty;
    public string? Model { get; set; }
    public bool CarryOverSummary { get; set; }
    /// <summary>Lista CSV de skills permitidas. Vacío = todas.</summary>
    public string AllowedSkillsCsv { get; set; } = string.Empty;
    /// <summary>Lista CSV de nombres de agente con los que puede hablar. Vacío = todos.</summary>
    public string AllowedRecipientsCsv { get; set; } = string.Empty;
    public int? MaxActionsPerHour { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum AgentRunStatus
{
    Active = 0,
    Completed = 1,
    Aborted = 2,
    /// <summary>El run fue comprimido (mensajes antiguos resumidos) y sigue activo.</summary>
    Compacted = 3,
}

public enum AgentRunTrigger
{
    Manual = 0,
    UserMessage = 1,
    AgentMessage = 2,
    ContextLimit = 3,
}

public enum ConversationStatus
{
    Active = 0,
    Closed = 1,
}

/// <summary>
/// Agrupa todos los Runs de todos los agentes que comparten un mismo contexto
/// de "conversación" en la UI. La instancia Althes tiene como mucho una
/// <c>Conversation</c> con <see cref="Status"/> = <see cref="ConversationStatus.Active"/>
/// en cada momento. Se crea perezosamente al primer mensaje entrante y se
/// cierra cuando el usuario inicia una nueva conversación.
/// </summary>
public class Conversation
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    /// <summary>Nombre opcional dado por el usuario; si es null la UI usa la fecha.</summary>
    public string? Name { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public ConversationStatus Status { get; set; } = ConversationStatus.Active;
}

/// <summary>
/// Una ejecución coherente de un agente. Arranca con el system prompt y termina
/// cuando se cierra explícitamente, se aborta o se reemplaza por uno nuevo
/// tras superar el límite de contexto. Pertenece a una <see cref="Conversation"/>.
/// </summary>
public class AgentRun
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    /// <summary>FK a la <see cref="Conversation"/> a la que pertenece este run.</summary>
    public string ConversationId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public AgentRunStatus Status { get; set; } = AgentRunStatus.Active;
    public AgentRunTrigger Trigger { get; set; }
    /// <summary>Resumen generado al cerrar el run (siempre, también al cerrar normal).</summary>
    public string? Summary { get; set; }
    public int TokenCount { get; set; }
}

public enum AgentMessageRole
{
    /// <summary>Mensaje system (prompt base o resumen inyectado).</summary>
    System = 0,
    /// <summary>Mensaje entrante del usuario o de otro agente.</summary>
    User = 1,
    /// <summary>Respuesta del propio agente (incluye decisión de skill).</summary>
    Assistant = 2,
    /// <summary>Resultado de ejecutar una skill (devuelto al LLM como contexto).</summary>
    Tool = 3,
}

/// <summary>
/// Visibilidad de cara al usuario final. La UI filtra por este campo para
/// construir el feed de chat; los valores distintos de <see cref="Hidden"/>
/// son los que se muestran al humano.
/// </summary>
public enum UserVisibility
{
    /// <summary>Mensaje interno (pensamiento, comunicación entre agentes, etc.).</summary>
    Hidden = 0,
    /// <summary>Notificación de un agente hacia el usuario (sin esperar respuesta).</summary>
    Outgoing = 1,
    /// <summary>Mensaje del usuario hacia un agente (vía SendMessage).</summary>
    Incoming = 2,
    /// <summary>Pregunta de un agente al usuario (espera respuesta correlacionada).</summary>
    Question = 3,
    /// <summary>Respuesta del usuario a una <see cref="Question"/>.</summary>
    Answer = 4,
}

/// <summary>Mensaje individual dentro de un run.</summary>
public class AgentMessage
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string RunId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public AgentMessageRole Role { get; set; }
    /// <summary>Skill que generó este mensaje (si aplica): think, askUser, etc.</summary>
    public string? Skill { get; set; }
    /// <summary>Quién envía: "user", agentName, "system" o null.</summary>
    public string? Sender { get; set; }
    /// <summary>Quién recibe (para skills de envío a agente/usuario).</summary>
    public string? Recipient { get; set; }
    /// <summary>Texto formateado para el LLM (puede incluir prefijos del tipo "[user says] ...").</summary>
    public string Content { get; set; } = string.Empty;
    /// <summary>
    /// Versión "limpia" del contenido pensada para mostrar en la UI cuando
    /// <see cref="Visibility"/> es distinta de <see cref="UserVisibility.Hidden"/>.
    /// Null para mensajes internos no destinados al humano.
    /// </summary>
    public string? RawContent { get; set; }
    /// <summary>Filtro principal de la UI: sólo los != Hidden aparecen en el feed.</summary>
    public UserVisibility Visibility { get; set; } = UserVisibility.Hidden;
    public int TokenCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>Correlaciona preguntas con sus respuestas en el bus interno.</summary>
    public string? CorrelationId { get; set; }
    /// <summary>Si esta línea es una <see cref="UserVisibility.Question"/>, id del UserQuery asociado.</summary>
    public string? UserQueryId { get; set; }
}

public enum UserQueryStatus
{
    Pending = 0,
    Answered = 1,
    Timeout = 2,
    Cancelled = 3,
}

/// <summary>Pregunta planteada por un agente al usuario, opcionalmente respondida.</summary>
public class UserQuery
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    /// <summary>FK a la conversación a la que pertenece (para hard-delete eficiente).</summary>
    public string ConversationId { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string RunId { get; set; } = string.Empty;
    /// <summary>Correlation id que el agente usa para identificar la respuesta.</summary>
    public string CorrelationId { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public UserQueryStatus Status { get; set; } = UserQueryStatus.Pending;
    public string? Answer { get; set; }
    public DateTime? AnsweredAt { get; set; }
    /// <summary>Usuario Identity que respondió (UserId).</summary>
    public string? AnsweredBy { get; set; }
}

