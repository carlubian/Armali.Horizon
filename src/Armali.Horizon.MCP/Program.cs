using Armali.Horizon.Contracts.Identity;
using Armali.Horizon.Contracts.Segaris;
using Armali.Horizon.Core.Logs;
using Armali.Horizon.IO;
using Armali.Horizon.MCP.Internal;
using Armali.Horizon.MCP.Tools;

namespace Armali.Horizon.MCP;

/// <summary>
/// Punto de entrada del servidor MCP de Horizon.
/// <para>
/// Expone vía HTTP los handlers de lectura de Segaris (y, cuando sea procedente,
/// de otros servicios) como tools del Model Context Protocol, traduciendo cada
/// llamada en una petición request/response sobre el bus IO existente.
/// </para>
/// <para>
/// La autenticación se delega en Identity: cada request HTTP entrante debe
/// incluir la cabecera <c>X-Horizon-Api-Key</c> con un token válido (sesión o
/// API key emitida por Identity). El servidor MCP no almacena credenciales:
/// es el cliente (Codex u otro) quien persiste la API key.
/// </para>
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Logging centralizado con Serilog + Seq (mismo patrón que el resto de apps).
        builder.Host.UseHorizonLogging();
        
        // Bus de eventos Horizon: el MCP server es un cliente puro,
        // no registra handlers. Sólo emite RequestAsync hacia Segaris/Identity.
        builder.Host.UseHorizonEvents();
        
        // Acceso al HttpContext para leer la API key por petición MCP.
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSingleton<HorizonApiKeyAccessor>();
        
        // Clientes Horizon. Se construyen con el token resuelto desde la cabecera
        // de la petición MCP en curso, gracias a HorizonApiKeyAccessor.
        builder.Services.AddScoped(sp =>
        {
            var events = sp.GetRequiredService<HorizonEventService>();
            var token = sp.GetRequiredService<HorizonApiKeyAccessor>().GetApiKey();
            return new HorizonAuthClient(events, token);
        });
        builder.Services.AddScoped(sp =>
        {
            var events = sp.GetRequiredService<HorizonEventService>();
            var token = sp.GetRequiredService<HorizonApiKeyAccessor>().GetApiKey();
            return new HorizonSegarisClient(events, token);
        });
        
        // Servidor MCP: descubre tools por reflexión a partir de los tipos
        // marcados con [McpServerToolType] en este ensamblado.
        builder.Services
            .AddMcpServer()
            .WithHttpTransport(options =>
            {
                // Stateless: cada request HTTP es independiente. Encaja con un
                // gateway sin sesión que resuelve credenciales por cabecera.
                options.Stateless = true;
            })
            .WithToolsFromAssembly(typeof(IdentityMcpTools).Assembly);
        
        var app = builder.Build();
        
        // Endpoint estándar MCP (Streamable HTTP). Codex y otros clientes
        // se configuran apuntando a http://host:5180/mcp.
        app.MapMcp("/mcp");
        
        // Endpoint de salud sencillo para probes/readiness.
        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
        
        app.Run();
    }
}

