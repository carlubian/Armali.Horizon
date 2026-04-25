namespace Armali.Horizon.Contracts.Segaris;

/// <summary>
/// Constantes compartidas del módulo Segaris para el bus IO.
/// </summary>
public static class SegarisChannels
{
    /// <summary>Canal Redis donde escucha el servicio Horizon.Segaris.</summary>
    public const string Channel = "segaris";
}

/// <summary>
/// Códigos de error devueltos por las operaciones IO de Segaris.
/// </summary>
public static class SegarisErrorCodes
{
    /// <summary>Token ausente, inválido o expirado.</summary>
    public const string Unauthorized = "unauthorized";
    /// <summary>Error inesperado en el servicio.</summary>
    public const string Internal = "internal";
}

/// <summary>
/// Información de error compartida por las respuestas de Segaris.
/// </summary>
public class SegarisErrorInfo
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

