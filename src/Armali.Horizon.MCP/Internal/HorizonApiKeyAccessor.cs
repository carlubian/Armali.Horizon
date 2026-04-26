using Microsoft.AspNetCore.Http;

namespace Armali.Horizon.MCP.Internal;

/// <summary>
/// Resuelve la API key Horizon de la petición HTTP MCP en curso.
/// <para>
/// El cliente MCP (Codex u otros) debe enviar la cabecera
/// <c>X-Horizon-Api-Key</c> con un token válido emitido por Identity.
/// El servidor MCP no persiste credenciales: simplemente las propaga al
/// bus IO en cada llamada para que Identity y Segaris validen.
/// </para>
/// </summary>
public class HorizonApiKeyAccessor
{
    /// <summary>Nombre de la cabecera HTTP donde viaja la API key.</summary>
    public const string HeaderName = "X-Horizon-Api-Key";
    
    private readonly IHttpContextAccessor Http;
    
    public HorizonApiKeyAccessor(IHttpContextAccessor http)
    {
        Http = http;
    }
    
    /// <summary>Devuelve la API key de la petición actual o null si no hay.</summary>
    public string? GetApiKey()
    {
        var ctx = Http.HttpContext;
        if (ctx is null) return null;
        if (!ctx.Request.Headers.TryGetValue(HeaderName, out var values)) return null;
        var raw = values.ToString();
        return string.IsNullOrWhiteSpace(raw) ? null : raw.Trim();
    }
}

