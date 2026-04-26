namespace Armali.Horizon.Contracts.Autoconfig;

/// <summary>
/// Constantes compartidas del módulo Autoconfig.
/// </summary>
public static class AutoconfigChannels
{
    /// <summary>Canal Redis donde escucha el servicio Horizon.Autoconfig.</summary>
    public const string Channel = "autoconfig";
}

/// <summary>
/// Códigos de error devueltos por el servicio Autoconfig en respuestas request/response.
/// </summary>
public static class AutoconfigErrorCodes
{
    /// <summary>El formato de la versión solicitada no es <c>A.B.C</c> con enteros no negativos.</summary>
    public const string InvalidVersion = "invalid_version";
    /// <summary>No existe ningún Nodo con el nombre indicado.</summary>
    public const string NodeNotFound = "node_not_found";
    /// <summary>No existe ninguna App con ese nombre dentro del Nodo indicado.</summary>
    public const string AppNotFound = "app_not_found";
    /// <summary>No hay ninguna versión compatible (mismo Major) que contenga el archivo solicitado.</summary>
    public const string FileNotFound = "file_not_found";
    /// <summary>El archivo existe pero su contenido supera el tamaño máximo configurado.</summary>
    public const string TooLarge = "too_large";
    /// <summary>El archivo no se ha podido decodificar como texto UTF-8.</summary>
    public const string NotText = "not_text";
    /// <summary>Error inesperado al leer el archivo del Datalake.</summary>
    public const string Internal = "internal";
}

/// <summary>
/// Información de error devuelta por las operaciones de Autoconfig.
/// </summary>
public class AutoconfigErrorInfo
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

