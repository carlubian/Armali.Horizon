using Armali.Horizon.Contracts.Identity;
using Armali.Horizon.IO;
using Armali.Horizon.Identity.Services;

namespace Armali.Horizon.Identity.Handlers;

// ── Auth ────────────────────────────────────────────────────────────────────

public class LoginHandler(IdentityService svc) : IHorizonRequestHandler<LoginRequest, LoginResponse>
{
    public async Task<LoginResponse> HandleAsync(LoginRequest req, CancellationToken ct = default)
    {
        var result = await svc.LoginAsync(req.UserName, req.Password);
        if (result is null)
            return new LoginResponse
            {
                Success = false,
                Error = new IdentityErrorInfo
                {
                    Code = "invalid_credentials",
                    Message = "Usuario o contraseña incorrectos.",
                },
            };
        
        return new LoginResponse
        {
            Success = true,
            Identity = result.Value.Identity,
            ExpiresAt = result.Value.Stored.ExpiresAt,
        };
    }
}

public class LogoutHandler(IdentityService svc) : IHorizonRequestHandler<LogoutRequest, LogoutResponse>
{
    public async Task<LogoutResponse> HandleAsync(LogoutRequest req, CancellationToken ct = default)
    {
        var ok = await svc.RevokeByPlainAsync(req.Token);
        return new LogoutResponse { Success = ok };
    }
}

public class WhoAmIHandler(IdentityService svc) : IHorizonRequestHandler<WhoAmIRequest, WhoAmIResponse>
{
    public async Task<WhoAmIResponse> HandleAsync(WhoAmIRequest req, CancellationToken ct = default)
    {
        var id = await svc.ValidateTokenAsync(req.Token);
        return new WhoAmIResponse { Authenticated = id is not null, Identity = id };
    }
}

public class ChangePasswordHandler(IdentityService svc)
    : IHorizonRequestHandler<ChangePasswordRequest, ChangePasswordResponse>
{
    public async Task<ChangePasswordResponse> HandleAsync(ChangePasswordRequest req, CancellationToken ct = default)
    {
        var id = await svc.AuthAsync(req);
        if (id is null) return new ChangePasswordResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        
        var ok = await svc.ChangePasswordAsync(id.UserId, req.CurrentPassword, req.NewPassword);
        if (!ok)
            return new ChangePasswordResponse
            {
                Success = false,
                Error = new IdentityErrorInfo { Code = "invalid_credentials", Message = "Contraseña actual incorrecta." },
            };
        return new ChangePasswordResponse { Success = true };
    }
}

// ── Users (admin) ───────────────────────────────────────────────────────────

public class ListUsersHandler(IdentityService svc) : IHorizonRequestHandler<ListUsersRequest, ListUsersResponse>
{
    public async Task<ListUsersResponse> HandleAsync(ListUsersRequest req, CancellationToken ct = default)
    {
        var id = await svc.AuthAdminAsync(req);
        if (id is null) return new ListUsersResponse();
        return new ListUsersResponse { Users = await svc.ListUsersAsync() };
    }
}

public class CreateUserHandler(IdentityService svc) : IHorizonRequestHandler<CreateUserRequest, CreateUserResponse>
{
    public async Task<CreateUserResponse> HandleAsync(CreateUserRequest req, CancellationToken ct = default)
    {
        var admin = await svc.AuthAdminAsync(req);
        if (admin is null) return new CreateUserResponse { Success = false, Error = HandlerAuth.Forbidden() };
        
        var dto = await svc.CreateUserAsync(req.UserName, req.DisplayName, req.Password, req.Roles);
        if (dto is null)
            return new CreateUserResponse
            {
                Success = false,
                Error = new IdentityErrorInfo { Code = "user_exists", Message = "El nombre de usuario ya existe." },
            };
        return new CreateUserResponse { Success = true, User = dto };
    }
}

public class UpdateUserHandler(IdentityService svc) : IHorizonRequestHandler<UpdateUserRequest, UpdateUserResponse>
{
    public async Task<UpdateUserResponse> HandleAsync(UpdateUserRequest req, CancellationToken ct = default)
    {
        var admin = await svc.AuthAdminAsync(req);
        if (admin is null) return new UpdateUserResponse { Success = false, Error = HandlerAuth.Forbidden() };
        
        var dto = await svc.UpdateUserAsync(req.UserId, req.DisplayName, req.IsActive, req.NewPassword);
        if (dto is null)
            return new UpdateUserResponse
            {
                Success = false,
                Error = new IdentityErrorInfo { Code = "not_found", Message = "Usuario no encontrado." },
            };
        return new UpdateUserResponse { Success = true, User = dto };
    }
}

public class DeleteUserHandler(IdentityService svc) : IHorizonRequestHandler<DeleteUserRequest, DeleteUserResponse>
{
    public async Task<DeleteUserResponse> HandleAsync(DeleteUserRequest req, CancellationToken ct = default)
    {
        var admin = await svc.AuthAdminAsync(req);
        if (admin is null) return new DeleteUserResponse { Success = false, Error = HandlerAuth.Forbidden() };
        if (admin.UserId == req.UserId)
            return new DeleteUserResponse
            {
                Success = false,
                Error = new IdentityErrorInfo { Code = "self_delete", Message = "No puedes borrar tu propio usuario." },
            };
        
        var ok = await svc.DeleteUserAsync(req.UserId);
        return new DeleteUserResponse
        {
            Success = ok,
            Error = ok ? null : new IdentityErrorInfo { Code = "not_found", Message = "Usuario no encontrado." },
        };
    }
}

public class SetUserRolesHandler(IdentityService svc) : IHorizonRequestHandler<SetUserRolesRequest, SetUserRolesResponse>
{
    public async Task<SetUserRolesResponse> HandleAsync(SetUserRolesRequest req, CancellationToken ct = default)
    {
        var admin = await svc.AuthAdminAsync(req);
        if (admin is null) return new SetUserRolesResponse { Success = false, Error = HandlerAuth.Forbidden() };
        
        // Defensa: un admin no puede revocarse a sí mismo el rol "admin"
        var keepsAdmin = req.Roles.Any(r => string.Equals(r, IdentityChannels.AdminRole, StringComparison.OrdinalIgnoreCase));
        if (admin.UserId == req.UserId && !keepsAdmin)
            return new SetUserRolesResponse
            {
                Success = false,
                Error = new IdentityErrorInfo { Code = "self_admin_remove", Message = "No puedes quitarte el rol admin a ti mismo." },
            };
        
        var roles = await svc.SetRolesAsync(req.UserId, req.Roles);
        if (roles is null)
            return new SetUserRolesResponse
            {
                Success = false,
                Error = new IdentityErrorInfo { Code = "not_found", Message = "Usuario no encontrado." },
            };
        return new SetUserRolesResponse { Success = true, Roles = roles };
    }
}

public class ListRolesHandler(IdentityService svc) : IHorizonRequestHandler<ListRolesRequest, ListRolesResponse>
{
    public async Task<ListRolesResponse> HandleAsync(ListRolesRequest req, CancellationToken ct = default)
    {
        var admin = await svc.AuthAdminAsync(req);
        if (admin is null) return new ListRolesResponse();
        return new ListRolesResponse { Roles = await svc.ListRolesAsync() };
    }
}

// ── Tokens / API keys ───────────────────────────────────────────────────────

public class CreateTokenHandler(IdentityService svc) : IHorizonRequestHandler<CreateTokenRequest, CreateTokenResponse>
{
    public async Task<CreateTokenResponse> HandleAsync(CreateTokenRequest req, CancellationToken ct = default)
    {
        var caller = await svc.AuthAsync(req);
        if (caller is null) return new CreateTokenResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        
        // De momento sólo permitimos crear tokens para uno mismo (ignoramos TargetUserId)
        var targetUserId = caller.UserId;
        
        // Las sesiones se emiten sólo vía login, y las API keys son siempre permanentes
        var kind = TokenKind.ApiKey;
        DateTime? expiresAt = null;
        
        var result = await svc.CreateTokenAsync(targetUserId, req.Label, kind, expiresAt);
        if (result is null)
            return new CreateTokenResponse
            {
                Success = false,
                Error = new IdentityErrorInfo { Code = "not_found", Message = "Usuario no encontrado." },
            };
        return new CreateTokenResponse { Success = true, Token = result.Value.Plain, Info = result.Value.Info };
    }
}

public class ListTokensHandler(IdentityService svc) : IHorizonRequestHandler<ListTokensRequest, ListTokensResponse>
{
    public async Task<ListTokensResponse> HandleAsync(ListTokensRequest req, CancellationToken ct = default)
    {
        var caller = await svc.AuthAsync(req);
        if (caller is null) return new ListTokensResponse();
        
        // Sin admin → sólo puede ver sus propios tokens
        var isAdmin = caller.HasRole(IdentityChannels.AdminRole);
        var filterUserId = isAdmin ? req.UserId : caller.UserId;
        
        return new ListTokensResponse { Tokens = await svc.ListTokensAsync(filterUserId, req.Kind, req.Token) };
    }
}

public class RevokeTokenHandler(IdentityService svc) : IHorizonRequestHandler<RevokeTokenRequest, RevokeTokenResponse>
{
    public async Task<RevokeTokenResponse> HandleAsync(RevokeTokenRequest req, CancellationToken ct = default)
    {
        var caller = await svc.AuthAsync(req);
        if (caller is null) return new RevokeTokenResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        
        var token = await svc.FindTokenAsync(req.TokenId);
        if (token is null)
            return new RevokeTokenResponse
            {
                Success = false,
                Error = new IdentityErrorInfo { Code = "not_found", Message = "Token no encontrado." },
            };
        
        // No-admin sólo puede revocar tokens propios
        if (token.UserId != caller.UserId && !caller.HasRole(IdentityChannels.AdminRole))
            return new RevokeTokenResponse { Success = false, Error = HandlerAuth.Forbidden() };
        
        await svc.RevokeByIdAsync(req.TokenId);
        return new RevokeTokenResponse { Success = true };
    }
}

