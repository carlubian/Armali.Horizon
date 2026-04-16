using Armali.Horizon.IO;

namespace Armali.Horizon.Megnir.Model;

/// <summary>
/// Solicita un fichero de configuración a Autoconfig mediante FindValidFile.
/// </summary>
public class FindFileRequest : IHorizonEventPayload
{
    public string EventType => "autoconfig.file.find";
    public string NodeName { get; set; } = string.Empty;
    public string AppName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}

/// <summary>
/// Respuesta con el contenido del fichero y la versión resuelta.
/// Si Found es false, el fichero no existe en Autoconfig.
/// </summary>
public class FindFileResponse : IHorizonEventPayload
{
    public string EventType => "autoconfig.file.find:response";
    public bool Found { get; set; }
    public string Content { get; set; } = string.Empty;
    public string ResolvedVersion { get; set; } = string.Empty;
}

