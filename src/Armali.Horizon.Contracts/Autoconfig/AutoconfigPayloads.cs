using Armali.Horizon.IO;

namespace Armali.Horizon.Contracts.Autoconfig;

// ─────────────────────────────────────────────────────────────────────────────
// Get Config File
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Solicita el contenido de un archivo de configuración a Horizon.Autoconfig.
/// El servicio resuelve la mejor versión compatible con la solicitada según
/// estas reglas, en orden de prioridad:
/// <list type="number">
///   <item>Coincidencia exacta de Major.Minor.Patch.</item>
///   <item>Mismo Major.Minor con el Patch más alto disponible.</item>
///   <item>Mismo Major con el Minor.Patch más altos disponibles.</item>
/// </list>
/// Si una versión candidata no contiene el archivo solicitado, se descarta
/// y se prueba la siguiente. Si no hay ninguna versión válida con el archivo
/// se devuelve <see cref="GetConfigFileResponse.Found"/> = false.
/// <para>
/// Esta operación es anónima: los archivos de configuración se consideran
/// globales y no requieren token. Si en el futuro se necesita restringir
/// el acceso, este contrato debe extender <c>AuthenticatedRequest</c>.
/// </para>
/// </summary>
public class GetConfigFileRequest : IHorizonEventPayload
{
    public string EventType => "autoconfig.config.get";

    /// <summary>Nombre exacto del Nodo registrado en Autoconfig.</summary>
    public string NodeName { get; set; } = string.Empty;

    /// <summary>Nombre exacto de la App dentro del Nodo.</summary>
    public string AppName { get; set; } = string.Empty;

    /// <summary>Versión solicitada en formato <c>A.B.C</c> (Major.Minor.Patch).</summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>Nombre exacto del archivo dentro de la versión.</summary>
    public string FileName { get; set; } = string.Empty;
}

/// <summary>
/// Respuesta con el contenido del archivo de configuración o información del fallo.
/// </summary>
public class GetConfigFileResponse : IHorizonEventPayload
{
    public string EventType => "autoconfig.config.get:response";

    /// <summary>True si se ha encontrado un archivo compatible y se devuelve su contenido.</summary>
    public bool Found { get; set; }

    /// <summary>
    /// Versión real desde la que se ha servido el archivo en formato <c>A.B.C</c>.
    /// Puede no coincidir con la solicitada si se aplicó fallback por Patch o Minor.
    /// Vacía si <see cref="Found"/> es false.
    /// </summary>
    public string ResolvedVersion { get; set; } = string.Empty;

    /// <summary>Contenido del archivo decodificado como UTF-8. Vacío si <see cref="Found"/> es false.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Información del error cuando <see cref="Found"/> es false.</summary>
    public AutoconfigErrorInfo? Error { get; set; }
}

