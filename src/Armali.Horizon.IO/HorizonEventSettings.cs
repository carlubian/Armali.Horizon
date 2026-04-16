namespace Armali.Horizon.IO;

/// <summary>
/// Configuración del sistema de eventos Horizon.
/// Se lee de la sección "Horizon:Events" en appsettings.json.
/// </summary>
public class HorizonEventSettings
{
    /// <summary>Endpoint de conexión a Redis (host:puerto).</summary>
    public string Endpoint { get; set; } = "localhost:6379";
    
    /// <summary>Timeout por defecto para peticiones request/response (en segundos).</summary>
    public int DefaultTimeoutSeconds { get; set; } = 10;
    
    /// <summary>Timeout como TimeSpan, derivado de <see cref="DefaultTimeoutSeconds"/>.</summary>
    public TimeSpan DefaultTimeout => TimeSpan.FromSeconds(DefaultTimeoutSeconds);
}