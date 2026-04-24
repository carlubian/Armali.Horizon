using Armali.Horizon.Core.Model;
using Armali.Horizon.IO;

namespace Armali.Horizon.Contracts.Identity;

/// <summary>
/// Cliente de alto nivel para Horizon.Identity sobre el bus IO.
/// <para>
/// Encapsula las llamadas <see cref="HorizonEventService.RequestAsync{T}"/> al
/// canal <see cref="IdentityChannels.Channel"/> para que apps con UI o
/// headless (consolas, integraciones tipo Codex) no tengan que conocer los
/// detalles de los payloads.
/// </para>
/// <para>
/// Una vez obtenido un token vía <see cref="LoginAsync"/> o reutilizando una
/// API key, todas las llamadas posteriores requieren ese token: pásalo al
/// constructor o en cada método.
/// </para>
/// </summary>
public class HorizonAuthClient
{
    private readonly HorizonEventService Events;
    private readonly TimeSpan? Timeout;
    
    /// <summary>Token actual asociado al cliente (puede actualizarse tras login).</summary>
    public string? Token { get; set; }
    
    public HorizonAuthClient(HorizonEventService events, string? token = null, TimeSpan? timeout = null)
    {
        Events = events;
        Token = token;
        Timeout = timeout;
    }
    
    private string RequireToken() =>
        Token ?? throw new InvalidOperationException(
            "No hay token activo. Llama a LoginAsync o asigna Token primero.");
    
    private Task<TRes> Send<TRes>(IHorizonEventPayload req) where TRes : IHorizonEventPayload =>
        Events.RequestAsync<TRes>(IdentityChannels.Channel, req, Timeout);
    
    // ── Auth ────────────────────────────────────────────────────────────
    
    /// <summary>Autentica con usuario+password y, si tiene éxito, persiste el token interno.</summary>
    public async Task<LoginResponse> LoginAsync(string userName, string password)
    {
        var res = await Send<LoginResponse>(new LoginRequest { UserName = userName, Password = password });
        if (res.Success && res.Identity != null)
            Token = res.Identity.Token;
        return res;
    }
    
    /// <summary>Devuelve la identidad del token actual o null si no es válido.</summary>
    public async Task<HorizonIdentity?> WhoAmIAsync()
    {
        var res = await Send<WhoAmIResponse>(new WhoAmIRequest { Token = RequireToken() });
        return res.Authenticated ? res.Identity : null;
    }
    
    /// <summary>Revoca el token actual y limpia la sesión local.</summary>
    public async Task<bool> LogoutAsync()
    {
        if (Token is null) return true;
        var res = await Send<LogoutResponse>(new LogoutRequest { Token = Token });
        Token = null;
        return res.Success;
    }
    
    public Task<ChangePasswordResponse> ChangePasswordAsync(string current, string @new) =>
        Send<ChangePasswordResponse>(new ChangePasswordRequest
        {
            Token = RequireToken(),
            CurrentPassword = current,
            NewPassword = @new,
        });
    
    // ── Usuarios (admin) ────────────────────────────────────────────────
    
    public Task<ListUsersResponse> ListUsersAsync() =>
        Send<ListUsersResponse>(new ListUsersRequest { Token = RequireToken() });
    
    public Task<CreateUserResponse> CreateUserAsync(string userName, string displayName, string password, params string[] roles) =>
        Send<CreateUserResponse>(new CreateUserRequest
        {
            Token = RequireToken(),
            UserName = userName,
            DisplayName = displayName,
            Password = password,
            Roles = roles,
        });
    
    public Task<UpdateUserResponse> UpdateUserAsync(string userId, string? displayName = null, bool? isActive = null, string? newPassword = null) =>
        Send<UpdateUserResponse>(new UpdateUserRequest
        {
            Token = RequireToken(),
            UserId = userId,
            DisplayName = displayName,
            IsActive = isActive,
            NewPassword = newPassword,
        });
    
    public Task<DeleteUserResponse> DeleteUserAsync(string userId) =>
        Send<DeleteUserResponse>(new DeleteUserRequest { Token = RequireToken(), UserId = userId });
    
    public Task<SetUserRolesResponse> SetUserRolesAsync(string userId, params string[] roles) =>
        Send<SetUserRolesResponse>(new SetUserRolesRequest
        {
            Token = RequireToken(),
            UserId = userId,
            Roles = roles,
        });
    
    public Task<ListRolesResponse> ListRolesAsync() =>
        Send<ListRolesResponse>(new ListRolesRequest { Token = RequireToken() });
    
    // ── Tokens / API keys ──────────────────────────────────────────────
    
    public Task<CreateTokenResponse> CreateApiKeyAsync(string label, DateTime? expiresAt = null, string? targetUserId = null) =>
        Send<CreateTokenResponse>(new CreateTokenRequest
        {
            Token = RequireToken(),
            Label = label,
            Kind = TokenKind.ApiKey,
            ExpiresAt = expiresAt,
            TargetUserId = targetUserId,
        });
    
    public Task<ListTokensResponse> ListTokensAsync(string? userId = null, TokenKind? kind = null) =>
        Send<ListTokensResponse>(new ListTokensRequest
        {
            Token = RequireToken(),
            UserId = userId,
            Kind = kind,
        });
    
    public Task<RevokeTokenResponse> RevokeTokenAsync(string tokenId) =>
        Send<RevokeTokenResponse>(new RevokeTokenRequest { Token = RequireToken(), TokenId = tokenId });
}

