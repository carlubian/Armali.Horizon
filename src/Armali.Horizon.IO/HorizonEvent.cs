namespace Armali.Horizon.IO;

/// <summary>
/// Sobre (envelope) que envuelve cada mensaje transmitido por Redis pub/sub.
/// El Payload viaja comprimido con Zstd; CorrelationId y ReplyTo habilitan
/// el patrón request/response sobre el mismo canal.
/// </summary>
public class HorizonEvent
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public byte[] Payload { get; set; } = [];
    
    /// <summary>
    /// Identificador de correlación que vincula una petición con su respuesta.
    /// Nulo en eventos fire-and-forget.
    /// </summary>
    public Guid? CorrelationId { get; set; }
    
    /// <summary>
    /// Canal Redis donde el solicitante espera recibir la respuesta.
    /// Nulo en eventos fire-and-forget.
    /// </summary>
    public string? ReplyTo { get; set; }
}
