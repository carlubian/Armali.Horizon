using Armali.Horizon.Contracts.Identity;
using Armali.Horizon.IO;

namespace Armali.Horizon.Contracts.Althes;

// ─────────────────────────────────────────────────────────────────────────────
// Payloads externos: peticiones autenticadas dirigidas a una instancia Althes.
// Requieren rol Identity "althes.user" (o "admin").
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Devuelve la lista de agentes definidos en la instancia + estado runtime.</summary>
public class ListAgentsRequest : AuthenticatedRequest
{
    public override string EventType => "althes.agents.list";
}

public class ListAgentsResponse : IHorizonEventPayload
{
    public string EventType => "althes.agents.list:response";
    public bool Success { get; set; }
    public AlthesErrorInfo? Error { get; set; }
    public List<AlthesAgentDto> Agents { get; set; } = [];
}

/// <summary>
/// Inyecta un mensaje del usuario en el inbox de un agente. El sistema lo
/// trata como evento que despierta al agente y arranca un nuevo turno.
/// </summary>
public class SendMessageRequest : AuthenticatedRequest
{
    public override string EventType => "althes.agents.send";
    public string AgentName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    /// <summary>Si es true, cierra la conversación activa antes de procesar el mensaje.</summary>
    public bool StartNewConversation { get; set; }
}

public class SendMessageResponse : IHorizonEventPayload
{
    public string EventType => "althes.agents.send:response";
    public bool Success { get; set; }
    public AlthesErrorInfo? Error { get; set; }
    public string? RunId { get; set; }
    public string? ConversationId { get; set; }
}

// ─── Conversaciones (cara a la UI) ──────────────────────────────────────────

/// <summary>Lista conversaciones por orden cronológico descendente.</summary>
public class ListConversationsRequest : AuthenticatedRequest
{
    public override string EventType => "althes.conversations.list";
    public bool IncludeClosed { get; set; } = true;
    public int MaxConversations { get; set; } = 50;
}

public class ListConversationsResponse : IHorizonEventPayload
{
    public string EventType => "althes.conversations.list:response";
    public bool Success { get; set; }
    public AlthesErrorInfo? Error { get; set; }
    public List<ConversationDto> Conversations { get; set; } = [];
}

/// <summary>
/// Devuelve los mensajes user-facing de una conversación: todos los runs de
/// todos los agentes mezclados en orden cronológico, filtrados por
/// <c>Visibility != Hidden</c>.
/// </summary>
public class GetConversationRequest : AuthenticatedRequest
{
    public override string EventType => "althes.conversations.get";
    public string ConversationId { get; set; } = string.Empty;
}

public class GetConversationResponse : IHorizonEventPayload
{
    public string EventType => "althes.conversations.get:response";
    public bool Success { get; set; }
    public AlthesErrorInfo? Error { get; set; }
    public ConversationDto? Conversation { get; set; }
    public List<ChatMessageDto> Messages { get; set; } = [];
}

/// <summary>Cierra la conversación activa (cierra todos los runs activos).</summary>
public class StartNewConversationRequest : AuthenticatedRequest
{
    public override string EventType => "althes.conversations.startNew";
    /// <summary>Nombre opcional para la nueva conversación.</summary>
    public string? Name { get; set; }
}

public class StartNewConversationResponse : IHorizonEventPayload
{
    public string EventType => "althes.conversations.startNew:response";
    public bool Success { get; set; }
    public AlthesErrorInfo? Error { get; set; }
    public string? ClosedConversationId { get; set; }
}

/// <summary>Hard-delete de una conversación: borra runs, mensajes y queries asociados.</summary>
public class DeleteConversationRequest : AuthenticatedRequest
{
    public override string EventType => "althes.conversations.delete";
    public string ConversationId { get; set; } = string.Empty;
}

public class DeleteConversationResponse : IHorizonEventPayload
{
    public string EventType => "althes.conversations.delete:response";
    public bool Success { get; set; }
    public AlthesErrorInfo? Error { get; set; }
}

/// <summary>Cierra el run activo del agente (próximo mensaje arranca uno nuevo).</summary>
public class StartNewRunRequest : AuthenticatedRequest
{
    public override string EventType => "althes.agents.newRun";
    public string AgentName { get; set; } = string.Empty;
}

public class StartNewRunResponse : IHorizonEventPayload
{
    public string EventType => "althes.agents.newRun:response";
    public bool Success { get; set; }
    public AlthesErrorInfo? Error { get; set; }
    public string? ClosedRunId { get; set; }
}

/// <summary>Devuelve los runs (con o sin mensajes) de un agente, en orden descendente.</summary>
public class GetRunsRequest : AuthenticatedRequest
{
    public override string EventType => "althes.runs.list";
    public string AgentName { get; set; } = string.Empty;
    public int MaxRuns { get; set; } = 20;
    /// <summary>Si es true incluye los mensajes completos de cada run.</summary>
    public bool IncludeMessages { get; set; }
}

public class GetRunsResponse : IHorizonEventPayload
{
    public string EventType => "althes.runs.list:response";
    public bool Success { get; set; }
    public AlthesErrorInfo? Error { get; set; }
    public List<AgentRunDto> Runs { get; set; } = [];
}

/// <summary>Devuelve un run concreto con sus mensajes.</summary>
public class GetRunRequest : AuthenticatedRequest
{
    public override string EventType => "althes.runs.get";
    public string RunId { get; set; } = string.Empty;
}

public class GetRunResponse : IHorizonEventPayload
{
    public string EventType => "althes.runs.get:response";
    public bool Success { get; set; }
    public AlthesErrorInfo? Error { get; set; }
    public AgentRunDto? Run { get; set; }
}

/// <summary>Lista preguntas pendientes (y opcionalmente resueltas) dirigidas al usuario.</summary>
public class ListUserQueriesRequest : AuthenticatedRequest
{
    public override string EventType => "althes.userQueries.list";
    public bool IncludeAnswered { get; set; }
}

public class ListUserQueriesResponse : IHorizonEventPayload
{
    public string EventType => "althes.userQueries.list:response";
    public bool Success { get; set; }
    public AlthesErrorInfo? Error { get; set; }
    public List<UserQueryDto> Queries { get; set; } = [];
}

/// <summary>Responde a una pregunta pendiente del usuario. Despierta al agente solicitante.</summary>
public class AnswerUserQueryRequest : AuthenticatedRequest
{
    public override string EventType => "althes.userQueries.answer";
    public string QueryId { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}

public class AnswerUserQueryResponse : IHorizonEventPayload
{
    public string EventType => "althes.userQueries.answer:response";
    public bool Success { get; set; }
    public AlthesErrorInfo? Error { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// Payloads internos: agent-to-agent fire-and-forget sobre el canal del inbox
// del agente destino. No requieren autenticación: viajan dentro de la misma
// instancia (mismo proyecto, mismo Identity de proceso).
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Tipo de mensaje que llega al inbox de un agente.</summary>
public enum AgentInboxKind
{
    /// <summary>Otro agente o el usuario manda información (fire-and-forget).</summary>
    Notify = 0,
    /// <summary>Otro agente plantea una pregunta y espera respuesta correlacionada.</summary>
    Question = 1,
    /// <summary>Respuesta a una pregunta que este agente había planteado antes.</summary>
    Answer = 2,
    /// <summary>El usuario contestó a una pregunta que este agente había planteado.</summary>
    UserAnswer = 3,
    /// <summary>Timeout esperando una respuesta (sintético, lo genera el runtime).</summary>
    Timeout = 4,
}

/// <summary>
/// Mensaje que viaja al canal interno de un agente para alimentar su cola FIFO,
/// o al canal del inbox del usuario para que la UI lo muestre en vivo.
/// </summary>
public class AgentInboxMessage : IHorizonEventPayload
{
    public string EventType => "althes.inbox.message";
    public AgentInboxKind Kind { get; set; }
    /// <summary>"user", otro agentName, o "system".</summary>
    public string Sender { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    /// <summary>Correla preguntas con respuestas (ambas comparten id).</summary>
    public string? CorrelationId { get; set; }
    /// <summary>Conversación a la que pertenece (sólo eventos hacia el usuario).</summary>
    public string? ConversationId { get; set; }
    /// <summary>Run que generó el mensaje (sólo eventos hacia el usuario).</summary>
    public string? RunId { get; set; }
    /// <summary>
    /// Si la skill es <c>ask_user</c>, id de la <see cref="UserQueryDto"/> creada,
    /// para que la UI pueda invocar <see cref="AnswerUserQueryRequest"/> con él.
    /// </summary>
    public string? UserQueryId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

