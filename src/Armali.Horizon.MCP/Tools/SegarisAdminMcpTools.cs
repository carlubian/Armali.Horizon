using System.ComponentModel;
using Armali.Horizon.Contracts.Segaris;
using ModelContextProtocol.Server;

namespace Armali.Horizon.MCP.Tools;

/// <summary>Tools de lectura del módulo Admin (Processes) de Segaris.</summary>
[McpServerToolType]
public static class SegarisAdminMcpTools
{
    [McpServerTool, Description("Lista las categorías de procesos administrativos.")]
    public static async Task<object> SegarisListAdminCategories(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListAdminCategoriesAsync(), r => new { success = true, categories = r.Categories });
    
    [McpServerTool, Description(
        "Lista los procesos administrativos visibles para el usuario autenticado. " +
        "Los procesos privados sólo son visibles para su creador.")]
    public static async Task<object> SegarisListAdmin(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListAdminAsync(), r => new { success = true, items = r.Items });
    
    [McpServerTool, Description("Lista los pasos/etapas de un proceso administrativo concreto.")]
    public static async Task<object> SegarisListAdminSteps(
        HorizonSegarisClient client,
        [Description("ID del proceso cuyos pasos se quieren consultar.")] int processId) =>
        SegarisToolHelpers.Wrap(await client.ListAdminSubEntitiesAsync(processId), r => new { success = true, steps = r.Steps });
    
    [McpServerTool, Description(
        "Devuelve estadísticas agregadas de un proceso administrativo: pasos completados, " +
        "pendientes, a tiempo, retrasados y un color/nombre global del estado.")]
    public static async Task<object> SegarisGetAdminStats(
        HorizonSegarisClient client,
        [Description("ID del proceso del que se quieren estadísticas.")] int processId) =>
        SegarisToolHelpers.Wrap(await client.GetAdminStatsAsync(processId), r => new { success = true, stats = r.Stats });
}

