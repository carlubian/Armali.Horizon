namespace Armali.Horizon.Contracts.Althes;

// ─────────────────────────────────────────────────────────────────────────────
// DTOs externos (compartidos con UI futura y MCP)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Información ligera de un agente cargado por la instancia.</summary>
public class AlthesAgentDto
{
    public string Name { get; set; } = string.Empty;
    /// <summary>Descripción corta del rol del agente.</summary>
    public string? Description { get; set; }
    public string SystemPrompt { get; set; } = string.Empty;
    public string? Model { get; set; }
    public bool CarryOverSummary { get; set; }
    public string[] AllowedSkills { get; set; } = [];
    public string[] AllowedRecipients { get; set; } = [];
    public int? MaxActionsPerHour { get; set; }
    
    /// <summary>Estado en vivo: id del run activo, número de items en cola, etc.</summary>
    public string? ActiveRunId { get; set; }
    public int PendingInboxItems { get; set; }
    public bool IsAwaiting { get; set; }
}

/// <summary>Mensaje persistido dentro de un <see cref="AgentRunDto"/>.</summary>
public class AgentMessageDto
{
    public string Id { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? Skill { get; set; }
    public string? Sender { get; set; }
    public string? Recipient { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? RawContent { get; set; }
    public string Visibility { get; set; } = "Hidden";
    public int TokenCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CorrelationId { get; set; }
    public string? UserQueryId { get; set; }
}

/// <summary>
/// Mensaje "limpio" pensado para alimentar el feed de chat de la UI Althes.
/// Es la proyección user-facing de un <see cref="AgentMessageDto"/>.
/// </summary>
public class ChatMessageDto
{
    public string Id { get; set; } = string.Empty;
    public string RunId { get; set; } = string.Empty;
    /// <summary>"user" si lo escribió el humano, agentName en cualquier otro caso.</summary>
    public string Sender { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Visibility { get; set; } = "Outgoing";
    public DateTime CreatedAt { get; set; }
    /// <summary>Cuando es una pregunta pendiente, id del UserQuery para responder.</summary>
    public string? UserQueryId { get; set; }
    /// <summary>Para preguntas: indica si ya fue respondida.</summary>
    public bool? IsAnswered { get; set; }
}

/// <summary>Conversación agregadora de runs de varios agentes.</summary>
public class ConversationDto
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int MessageCount { get; set; }
    public int PendingQuestions { get; set; }
}

/// <summary>Una ejecución de un agente — contexto desde el system prompt hasta el cierre.</summary>
public class AgentRunDto
{
    public string Id { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string ConversationId { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Trigger { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public int TokenCount { get; set; }
    public List<AgentMessageDto> Messages { get; set; } = [];
}

/// <summary>Pregunta de un agente al usuario, opcionalmente con respuesta.</summary>
public class UserQueryDto
{
    public string Id { get; set; } = string.Empty;
    public string AgentName { get; set; } = string.Empty;
    public string RunId { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;  // pending | answered | timeout
    public string? Answer { get; set; }
    public DateTime? AnsweredAt { get; set; }
}

/// <summary>Códigos de error estándar devueltos por las operaciones Althes.</summary>
public static class AlthesErrorCodes
{
    public const string Unauthorized = "unauthorized";
    public const string Forbidden = "forbidden";
    public const string AgentNotFound = "agent_not_found";
    public const string RunNotFound = "run_not_found";
    public const string ConversationNotFound = "conversation_not_found";
    public const string QueryNotFound = "query_not_found";
    public const string AlreadyAnswered = "already_answered";
    public const string Internal = "internal";
    public const string Invalid = "invalid";
}

public class AlthesErrorInfo
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

