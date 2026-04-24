namespace Armali.Horizon.Contracts.Identity;

/// <summary>
/// Constantes compartidas del módulo Identity.
/// </summary>
public static class IdentityChannels
{
    /// <summary>Canal Redis donde escucha el servicio Horizon.Identity.</summary>
    public const string Channel = "identity";
    
    /// <summary>Rol con permisos administrativos sobre usuarios, roles y tokens.</summary>
    public const string AdminRole = "admin";
}

/// <summary>
/// Tipo de token emitido por el servicio Identity.
/// <list type="bullet">
///   <item><b>Session</b>: token volátil con expiración corta, emitido en cada login.</item>
///   <item><b>ApiKey</b>: token permanente (o de larga vida), creado explícitamente
///         por el usuario para apps headless o integraciones externas.</item>
/// </list>
/// </summary>
public enum TokenKind
{
    Session = 0,
    ApiKey = 1
}

