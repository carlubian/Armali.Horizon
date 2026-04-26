using Armali.Horizon.Core.Model;
using Armali.Horizon.IO;

namespace Armali.Horizon.Contracts.Identity;

/// <summary>
/// Base para toda petición que requiera un usuario autenticado.
/// El llamante debe rellenar <see cref="Token"/> con el bearer obtenido en login
/// o con una API key permanente.
/// </summary>
public abstract class AuthenticatedRequest : IHorizonEventPayload
{
    public abstract string EventType { get; }
    public string Token { get; set; } = string.Empty;
}

// ─────────────────────────────────────────────────────────────────────────────
// Login / Logout / Whoami
// ─────────────────────────────────────────────────────────────────────────────

public class LoginRequest : IHorizonEventPayload
{
    public string EventType => "identity.auth.login";
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse : IHorizonEventPayload
{
    public string EventType => "identity.auth.login:response";
    public bool Success { get; set; }
    public IdentityErrorInfo? Error { get; set; }
    public HorizonIdentity? Identity { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

public class LogoutRequest : AuthenticatedRequest
{
    public override string EventType => "identity.auth.logout";
}

public class LogoutResponse : IHorizonEventPayload
{
    public string EventType => "identity.auth.logout:response";
    public bool Success { get; set; }
}

/// <summary>
/// Valida un token y devuelve la identidad asociada (incluye roles).
/// Útil para que un cliente compruebe periódicamente que su token sigue activo.
/// </summary>
public class WhoAmIRequest : AuthenticatedRequest
{
    public override string EventType => "identity.auth.whoami";
}

public class WhoAmIResponse : IHorizonEventPayload
{
    public string EventType => "identity.auth.whoami:response";
    public bool Authenticated { get; set; }
    public HorizonIdentity? Identity { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// Cambio de password (autoservicio)
// ─────────────────────────────────────────────────────────────────────────────

public class ChangePasswordRequest : AuthenticatedRequest
{
    public override string EventType => "identity.auth.changePassword";
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ChangePasswordResponse : IHorizonEventPayload
{
    public string EventType => "identity.auth.changePassword:response";
    public bool Success { get; set; }
    public IdentityErrorInfo? Error { get; set; }
}

// ─────────────────────────────────────────────────────────────────────────────
// Gestión de usuarios (admin)
// ─────────────────────────────────────────────────────────────────────────────

public class ListUsersRequest : AuthenticatedRequest
{
    public override string EventType => "identity.users.list";
}

public class ListUsersResponse : IHorizonEventPayload
{
    public string EventType => "identity.users.list:response";
    public List<IdentityUserDto> Users { get; set; } = [];
}

public class CreateUserRequest : AuthenticatedRequest
{
    public override string EventType => "identity.users.create";
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string[] Roles { get; set; } = [];
}

public class CreateUserResponse : IHorizonEventPayload
{
    public string EventType => "identity.users.create:response";
    public bool Success { get; set; }
    public IdentityErrorInfo? Error { get; set; }
    public IdentityUserDto? User { get; set; }
}

public class UpdateUserRequest : AuthenticatedRequest
{
    public override string EventType => "identity.users.update";
    public string UserId { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public bool? IsActive { get; set; }
    /// <summary>Si no es null, sustituye la contraseña sin pedir la anterior (operación admin).</summary>
    public string? NewPassword { get; set; }
}

public class UpdateUserResponse : IHorizonEventPayload
{
    public string EventType => "identity.users.update:response";
    public bool Success { get; set; }
    public IdentityErrorInfo? Error { get; set; }
    public IdentityUserDto? User { get; set; }
}

public class DeleteUserRequest : AuthenticatedRequest
{
    public override string EventType => "identity.users.delete";
    public string UserId { get; set; } = string.Empty;
}

public class DeleteUserResponse : IHorizonEventPayload
{
    public string EventType => "identity.users.delete:response";
    public bool Success { get; set; }
    public IdentityErrorInfo? Error { get; set; }
}

public class SetUserRolesRequest : AuthenticatedRequest
{
    public override string EventType => "identity.users.setRoles";
    public string UserId { get; set; } = string.Empty;
    public string[] Roles { get; set; } = [];
}

public class SetUserRolesResponse : IHorizonEventPayload
{
    public string EventType => "identity.users.setRoles:response";
    public bool Success { get; set; }
    public IdentityErrorInfo? Error { get; set; }
    public string[] Roles { get; set; } = [];
}

// ─────────────────────────────────────────────────────────────────────────────
// Catálogo de roles (admin)
// ─────────────────────────────────────────────────────────────────────────────

public class ListRolesRequest : AuthenticatedRequest
{
    public override string EventType => "identity.roles.list";
}

public class ListRolesResponse : IHorizonEventPayload
{
    public string EventType => "identity.roles.list:response";
    public List<string> Roles { get; set; } = [];
}

// ─────────────────────────────────────────────────────────────────────────────
// Tokens / API keys
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Crea un token. Si <see cref="Kind"/> es <see cref="TokenKind.ApiKey"/> el token
/// pertenece al usuario autenticado salvo que un admin indique
/// <see cref="TargetUserId"/>. Las sesiones se crean implícitamente en el login,
/// no por aquí.
/// </summary>
public class CreateTokenRequest : AuthenticatedRequest
{
    public override string EventType => "identity.tokens.create";
    public string Label { get; set; } = string.Empty;
    public TokenKind Kind { get; set; } = TokenKind.ApiKey;
    public DateTime? ExpiresAt { get; set; }
    /// <summary>Sólo admins: emitir el token a nombre de otro usuario.</summary>
    public string? TargetUserId { get; set; }
}

public class CreateTokenResponse : IHorizonEventPayload
{
    public string EventType => "identity.tokens.create:response";
    public bool Success { get; set; }
    public IdentityErrorInfo? Error { get; set; }
    /// <summary>Valor en claro del token — sólo se devuelve aquí, nunca más.</summary>
    public string Token { get; set; } = string.Empty;
    public IdentityTokenDto? Info { get; set; }
}

/// <summary>
/// Lista los tokens. Por defecto sólo los del usuario autenticado;
/// admins pueden filtrar por <see cref="UserId"/> o pasar null para "todos".
/// </summary>
public class ListTokensRequest : AuthenticatedRequest
{
    public override string EventType => "identity.tokens.list";
    public string? UserId { get; set; }
    public TokenKind? Kind { get; set; }
}

public class ListTokensResponse : IHorizonEventPayload
{
    public string EventType => "identity.tokens.list:response";
    public List<IdentityTokenDto> Tokens { get; set; } = [];
}

public class RevokeTokenRequest : AuthenticatedRequest
{
    public override string EventType => "identity.tokens.revoke";
    public string TokenId { get; set; } = string.Empty;
}

public class RevokeTokenResponse : IHorizonEventPayload
{
    public string EventType => "identity.tokens.revoke:response";
    public bool Success { get; set; }
    public IdentityErrorInfo? Error { get; set; }
}

