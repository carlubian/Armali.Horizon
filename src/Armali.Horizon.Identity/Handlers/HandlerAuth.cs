using Armali.Horizon.Contracts.Identity;
using Armali.Horizon.Core.Model;
using Armali.Horizon.Identity.Services;

namespace Armali.Horizon.Identity.Handlers;

/// <summary>
/// Helpers internos para los handlers IO de Identity. Centraliza la
/// validación de token y la comprobación de rol admin para evitar repetir
/// la lógica en cada handler.
/// </summary>
internal static class HandlerAuth
{
    public static IdentityErrorInfo Forbidden(string code = "forbidden", string? msg = null) =>
        new() { Code = code, Message = msg ?? "Acceso denegado." };

    public static IdentityErrorInfo Unauthorized() =>
        new() { Code = "unauthorized", Message = "Token inválido o expirado." };

    /// <summary>Valida el token y devuelve la identidad o null.</summary>
    public static Task<HorizonIdentity?> AuthAsync(this IdentityService svc, AuthenticatedRequest req) =>
        svc.ValidateTokenAsync(req.Token);

    /// <summary>Valida token y exige rol admin. Devuelve null si no procede.</summary>
    public static async Task<HorizonIdentity?> AuthAdminAsync(this IdentityService svc, AuthenticatedRequest req)
    {
        var id = await svc.ValidateTokenAsync(req.Token);
        if (id is null) return null;
        return id.HasRole(IdentityChannels.AdminRole) ? id : null;
    }
}

