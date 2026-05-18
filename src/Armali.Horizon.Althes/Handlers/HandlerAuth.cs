using Armali.Horizon.Contracts.Althes;
using Armali.Horizon.Contracts.Identity;
using Armali.Horizon.Core.Model;

namespace Armali.Horizon.Althes.Handlers;

/// <summary>
/// Helpers compartidos para los handlers IO de Althes. Centraliza la
/// validación del token contra Identity y la comprobación del rol requerido.
/// </summary>
internal static class HandlerAuth
{
    public static AlthesErrorInfo Unauthorized() =>
        new() { Code = AlthesErrorCodes.Unauthorized, Message = "Token inválido o expirado." };
    
    public static AlthesErrorInfo Forbidden() =>
        new() { Code = AlthesErrorCodes.Forbidden, Message = $"Requiere rol '{AlthesChannels.UserRole}'." };
    
    /// <summary>
    /// Valida el token contra Identity y exige rol althes.user (admins pasan también).
    /// Devuelve la identidad o null si no autoriza.
    /// </summary>
    public static async Task<HorizonIdentity?> AuthAlthesAsync(this HorizonAuthClient auth, AuthenticatedRequest req)
    {
        auth.Token = req.Token;
        var id = await auth.WhoAmIAsync();
        if (id is null) return null;
        if (id.HasRole(AlthesChannels.UserRole) || id.HasRole(IdentityChannels.AdminRole))
            return id;
        return null;
    }
}

