using System.ComponentModel;
using Armali.Horizon.Contracts.Segaris;
using ModelContextProtocol.Server;

namespace Armali.Horizon.MCP.Tools;

/// <summary>Tools de lectura del módulo Clothes de Segaris.</summary>
[McpServerToolType]
public static class SegarisClothesMcpTools
{
    [McpServerTool, Description("Lista las categorías de prendas de ropa.")]
    public static async Task<object> SegarisListClothesCategories(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListClothesCategoriesAsync(), r => new { success = true, categories = r.Categories });
    
    [McpServerTool, Description("Lista los estados configurables de prendas de ropa.")]
    public static async Task<object> SegarisListClothesStatuses(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListClothesStatusesAsync(), r => new { success = true, statuses = r.Statuses });
    
    [McpServerTool, Description("Lista los tipos de lavado disponibles para prendas.")]
    public static async Task<object> SegarisListClothesWashTypes(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListClothesWashTypesAsync(), r => new { success = true, washTypes = r.WashTypes });
    
    [McpServerTool, Description("Lista el catálogo de colores predefinidos con nombre y código hexadecimal.")]
    public static async Task<object> SegarisListClothesColors(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListClothesColorsAsync(), r => new { success = true, colors = r.Colors });
    
    [McpServerTool, Description("Lista los estilos de color (Primary, Secondary, Details, etc.).")]
    public static async Task<object> SegarisListClothesColorStyles(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListClothesColorStylesAsync(), r => new { success = true, colorStyles = r.ColorStyles });
    
    [McpServerTool, Description(
        "Lista las prendas de ropa visibles para el usuario autenticado, ordenadas por fecha. " +
        "Las prendas privadas sólo son visibles para su creador.")]
    public static async Task<object> SegarisListClothes(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListClothesAsync(), r => new { success = true, items = r.Items });
    
    [McpServerTool, Description("Lista las asignaciones de color de una prenda concreta (color + estilo).")]
    public static async Task<object> SegarisListClothesColorAssignments(
        HorizonSegarisClient client,
        [Description("ID de la prenda (garment) cuyos colores se quieren consultar.")] int garmentId) =>
        SegarisToolHelpers.Wrap(await client.ListClothesColorAssignmentsAsync(garmentId), r => new { success = true, assignments = r.Assignments });
}

