namespace Armali.Horizon.Contracts.Althes;

/// <summary>
/// Constantes y helpers compartidos del módulo Althes.
/// <para>
/// Cada instancia de Althes representa un <b>proyecto</b> independiente
/// identificado por <c>ProjectId</c>. Los canales se prefijan con el
/// proyecto para que varias instancias puedan convivir sobre el mismo Redis
/// sin solaparse.
/// </para>
/// </summary>
public static class AlthesChannels
{
    /// <summary>Prefijo raíz de todos los canales Althes.</summary>
    public const string Root = "althes";
    
    /// <summary>Rol Identity requerido para invocar operaciones externas de Althes.</summary>
    public const string UserRole = "althes.user";
    
    /// <summary>
    /// Canal request/response donde escucha la instancia del proyecto indicado.
    /// Los handlers externos (UI, MCP, otras apps) publican aquí.
    /// </summary>
    public static string For(string projectId) => $"{Root}:{projectId}";
    
    /// <summary>
    /// Canal interno fire-and-forget donde un agente concreto recibe mensajes
    /// (preguntas, notificaciones, respuestas). Usado por agentes hermanos y
    /// por los handlers externos para alimentar el inbox FIFO del agente.
    /// </summary>
    public static string AgentInbox(string projectId, string agentName) =>
        $"{Root}:{projectId}:agent:{agentName}";
    
    /// <summary>
    /// Canal de eventos del inbox del usuario (preguntas planteadas por agentes
    /// y notificaciones). Sirve para que una UI futura se subscriba en tiempo real.
    /// </summary>
    public static string UserInbox(string projectId) => $"{Root}:{projectId}:user";
}

