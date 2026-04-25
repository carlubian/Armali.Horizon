using Armali.Horizon.Contracts.Identity;
using Armali.Horizon.Contracts.Segaris;
using Armali.Horizon.Core.Model;

namespace Armali.Horizon.Segaris.Handlers;

/// <summary>
/// Helpers internos para los handlers IO de Segaris. Centralizan la
/// validación del token contra Identity (vía bus IO) y la construcción de
/// respuestas de error coherentes.
/// </summary>
internal static class HandlerAuth
{
    /// <summary>Crea un error de token inválido.</summary>
    public static SegarisErrorInfo Unauthorized() =>
        new() { Code = SegarisErrorCodes.Unauthorized, Message = "Token inválido o expirado." };
    
    /// <summary>Crea un error interno con el mensaje indicado.</summary>
    public static SegarisErrorInfo Internal(string msg) =>
        new() { Code = SegarisErrorCodes.Internal, Message = msg };
    
    /// <summary>
    /// Resuelve la identidad del token usando el cliente de Identity.
    /// Devuelve null si el token no es válido o si el bus IO no responde.
    /// </summary>
    public static async Task<HorizonIdentity?> AuthAsync(this HorizonAuthClient identity, AuthenticatedRequest req)
    {
        if (string.IsNullOrEmpty(req.Token)) return null;
        try
        {
            identity.Token = req.Token;
            return await identity.WhoAmIAsync();
        }
        catch
        {
            // Si Identity está caído u ocurre un timeout, no autorizamos.
            return null;
        }
    }
}

