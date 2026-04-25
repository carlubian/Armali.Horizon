using System.ComponentModel;
using Armali.Horizon.Contracts.Segaris;
using ModelContextProtocol.Server;

namespace Armali.Horizon.MCP.Tools;

/// <summary>Tools de lectura del módulo Asset de Segaris.</summary>
[McpServerToolType]
public static class SegarisAssetMcpTools
{
    [McpServerTool, Description("Lista las categorías de activos.")]
    public static async Task<object> SegarisListAssetCategories(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListAssetCategoriesAsync(), r => new { success = true, categories = r.Categories });
    
    [McpServerTool, Description("Lista los estados configurables de activos.")]
    public static async Task<object> SegarisListAssetStatuses(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListAssetStatusesAsync(), r => new { success = true, statuses = r.Statuses });
    
    [McpServerTool, Description(
        "Lista los activos visibles para el usuario autenticado, ordenados por fecha. " +
        "Los activos privados sólo son visibles para su creador.")]
    public static async Task<object> SegarisListAssets(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListAssetsAsync(), r => new { success = true, assets = r.Assets });
}

/// <summary>Tools de lectura del módulo Capex de Segaris.</summary>
[McpServerToolType]
public static class SegarisCapexMcpTools
{
    [McpServerTool, Description("Lista las categorías de gastos Capex.")]
    public static async Task<object> SegarisListCapexCategories(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListCapexCategoriesAsync(), r => new { success = true, categories = r.Categories });
    
    [McpServerTool, Description("Lista los estados configurables de Capex.")]
    public static async Task<object> SegarisListCapexStatuses(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListCapexStatusesAsync(), r => new { success = true, statuses = r.Statuses });
    
    [McpServerTool, Description("Lista las entradas Capex visibles para el usuario autenticado.")]
    public static async Task<object> SegarisListCapex(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListCapexAsync(), r => new { success = true, items = r.Items });
}

/// <summary>Tools de lectura del módulo Opex de Segaris.</summary>
[McpServerToolType]
public static class SegarisOpexMcpTools
{
    [McpServerTool, Description("Lista las categorías de contratos Opex.")]
    public static async Task<object> SegarisListOpexCategories(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListOpexCategoriesAsync(), r => new { success = true, categories = r.Categories });
    
    [McpServerTool, Description("Lista los estados configurables de contratos Opex.")]
    public static async Task<object> SegarisListOpexStatuses(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListOpexStatusesAsync(), r => new { success = true, statuses = r.Statuses });
    
    [McpServerTool, Description("Lista los contratos Opex visibles para el usuario autenticado.")]
    public static async Task<object> SegarisListOpex(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListOpexAsync(), r => new { success = true, contracts = r.Contracts });
    
    [McpServerTool, Description("Lista las imputaciones de gasto (sub-entidades) de un contrato Opex concreto.")]
    public static async Task<object> SegarisListOpexEntries(
        HorizonSegarisClient client,
        [Description("ID del contrato Opex padre.")] int contractId) =>
        SegarisToolHelpers.Wrap(await client.ListOpexSubEntitiesAsync(contractId), r => new { success = true, entries = r.Entries });
    
    [McpServerTool, Description("Devuelve estadísticas agregadas (nº de imputaciones y total) de un contrato Opex.")]
    public static async Task<object> SegarisGetOpexStats(
        HorizonSegarisClient client,
        [Description("ID del contrato Opex.")] int contractId) =>
        SegarisToolHelpers.Wrap(await client.GetOpexStatsAsync(contractId), r => new { success = true, stats = r.Stats });
}

/// <summary>Tools de lectura del módulo Travel de Segaris.</summary>
[McpServerToolType]
public static class SegarisTravelMcpTools
{
    [McpServerTool, Description("Lista las categorías de viajes.")]
    public static async Task<object> SegarisListTravelCategories(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListTravelCategoriesAsync(), r => new { success = true, categories = r.Categories });
    
    [McpServerTool, Description("Lista los centros de coste utilizados en viajes.")]
    public static async Task<object> SegarisListTravelCostCenters(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListTravelCostCentersAsync(), r => new { success = true, costCenters = r.CostCenters });
    
    [McpServerTool, Description("Lista los estados configurables de viajes.")]
    public static async Task<object> SegarisListTravelStatuses(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListTravelStatusesAsync(), r => new { success = true, statuses = r.Statuses });
    
    [McpServerTool, Description("Lista los viajes visibles para el usuario autenticado.")]
    public static async Task<object> SegarisListTravels(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListTravelsAsync(), r => new { success = true, travels = r.Travels });
    
    [McpServerTool, Description("Lista las categorías de imputaciones de viaje (transporte, alojamiento, etc.).")]
    public static async Task<object> SegarisListTravelEntryCategories(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListTravelSubEntityCategoriesAsync(), r => new { success = true, categories = r.Categories });
    
    [McpServerTool, Description("Lista las imputaciones de gasto (sub-entidades) de un viaje concreto.")]
    public static async Task<object> SegarisListTravelEntries(
        HorizonSegarisClient client,
        [Description("ID del viaje padre.")] int travelId) =>
        SegarisToolHelpers.Wrap(await client.ListTravelSubEntitiesAsync(travelId), r => new { success = true, entries = r.Entries });
}

/// <summary>Tools de lectura del módulo Maintenance de Segaris.</summary>
[McpServerToolType]
public static class SegarisMaintMcpTools
{
    [McpServerTool, Description("Lista las categorías de mantenimiento.")]
    public static async Task<object> SegarisListMaintCategories(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListMaintCategoriesAsync(), r => new { success = true, categories = r.Categories });
    
    [McpServerTool, Description("Lista los estados configurables de mantenimiento.")]
    public static async Task<object> SegarisListMaintStatuses(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListMaintStatusesAsync(), r => new { success = true, statuses = r.Statuses });
    
    [McpServerTool, Description("Lista las órdenes/eventos de mantenimiento visibles para el usuario autenticado.")]
    public static async Task<object> SegarisListMaint(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListMaintAsync(), r => new { success = true, items = r.Items });
}

