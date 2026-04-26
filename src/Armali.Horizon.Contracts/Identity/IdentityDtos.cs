namespace Armali.Horizon.Contracts.Identity;

/// <summary>
/// DTO de usuario expuesto fuera del servicio Identity.
/// </summary>
public class IdentityUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string[] Roles { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO de token (sesión o API key) — nunca contiene el valor en claro,
/// salvo en la respuesta inmediata a su creación (<see cref="CreateTokenResponse.Token"/>).
/// </summary>
public class IdentityTokenDto
{
    public string TokenId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public TokenKind Kind { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUsedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    
    /// <summary>
    /// Sólo se rellena en respuestas a peticiones autenticadas: indica si este
    /// token es exactamente el que está usando el solicitante (i.e. su sesión activa).
    /// </summary>
    public bool IsCurrent { get; set; }
}

/// <summary>
/// Resultado genérico devuelto por endpoints que no producen datos.
/// </summary>
public class IdentityErrorInfo
{
    /// <summary>Código simbólico (e.g. "invalid_credentials", "forbidden").</summary>
    public string Code { get; set; } = string.Empty;
    /// <summary>Mensaje descriptivo.</summary>
    public string Message { get; set; } = string.Empty;
}

