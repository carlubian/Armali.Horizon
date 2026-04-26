using System.ComponentModel;
using Armali.Horizon.Contracts.Identity;
using ModelContextProtocol.Server;

namespace Armali.Horizon.MCP.Tools;

/// <summary>
/// Tools de Identity expuestas vía MCP.
/// <para>
/// La tool más útil para integraciones LLM es <c>whoami</c>: permite verificar
/// que la API key configurada en el cliente MCP funciona y devuelve el usuario,
/// roles y user-id resueltos por Identity.
/// </para>
/// </summary>
[McpServerToolType]
public static class IdentityMcpTools
{
    [McpServerTool, Description(
        "Devuelve el usuario, user-id y roles asociados a la API key configurada " +
        "en la cabecera X-Horizon-Api-Key. Si la API key falta o es inválida, " +
        "devuelve { authenticated: false }.")]
    public static async Task<object> WhoAmI(HorizonAuthClient identity)
    {
        if (string.IsNullOrEmpty(identity.Token))
            return new { authenticated = false, reason = "missing_api_key" };
        
        try
        {
            var me = await identity.WhoAmIAsync();
            if (me is null)
                return new { authenticated = false, reason = "invalid_token" };
            return new
            {
                authenticated = true,
                userId = me.UserId,
                userName = me.UserName,
                roles = me.Roles,
            };
        }
        catch (Exception ex)
        {
            return new { authenticated = false, reason = "transport_error", error = ex.Message };
        }
    }
}

