using System.ComponentModel;
using Armali.Horizon.Contracts.Segaris;
using ModelContextProtocol.Server;

namespace Armali.Horizon.MCP.Tools;

/// <summary>Tools de lectura del módulo Inventory de Segaris.</summary>
[McpServerToolType]
public static class SegarisInventoryMcpTools
{
    // ── Proveedores ────────────────────────────────────────────────────

    [McpServerTool, Description("Lista los estados configurables de proveedores de inventario.")]
    public static async Task<object> SegarisListInvVendorStatuses(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListInvVendorStatusesAsync(), r => new { success = true, statuses = r.Statuses });

    [McpServerTool, Description("Lista los proveedores de inventario visibles para el usuario autenticado.")]
    public static async Task<object> SegarisListInvVendors(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListInvVendorsAsync(), r => new { success = true, vendors = r.Vendors });

    [McpServerTool, Description(
        "Devuelve estadísticas agregadas de un proveedor de inventario (nº de artículos, pedidos, etc.).")]
    public static async Task<object> SegarisGetInvVendorStats(
        HorizonSegarisClient client,
        [Description("ID del proveedor.")] int vendorId) =>
        SegarisToolHelpers.Wrap(await client.GetInvVendorStatsAsync(vendorId), r => new { success = true, stats = r.Stats });

    // ── Artículos ──────────────────────────────────────────────────────

    [McpServerTool, Description("Lista las categorías de artículos de inventario.")]
    public static async Task<object> SegarisListInvItemCategories(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListInvItemCategoriesAsync(), r => new { success = true, categories = r.Categories });

    [McpServerTool, Description("Lista los estados configurables de artículos de inventario.")]
    public static async Task<object> SegarisListInvItemStatuses(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListInvItemStatusesAsync(), r => new { success = true, statuses = r.Statuses });

    [McpServerTool, Description(
        "Lista los artículos de inventario. Opcionalmente filtra por proveedor.")]
    public static async Task<object> SegarisListInvItems(
        HorizonSegarisClient client,
        [Description("ID del proveedor para filtrar. Omitir para listar todos.")] int? vendorId = null) =>
        SegarisToolHelpers.Wrap(await client.ListInvItemsAsync(vendorId), r => new { success = true, items = r.Items });

    [McpServerTool, Description(
        "Devuelve la lista de compra: artículos cuya cantidad actual está por debajo del mínimo configurado.")]
    public static async Task<object> SegarisGetShoppingList(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.GetShoppingListAsync(), r => new { success = true, items = r.Items });

    [McpServerTool, Description(
        "Devuelve el historial de precios de un artículo de inventario, ordenado cronológicamente.")]
    public static async Task<object> SegarisGetInvItemPriceHistory(
        HorizonSegarisClient client,
        [Description("ID del artículo.")] int itemId) =>
        SegarisToolHelpers.Wrap(await client.GetInvItemPriceHistoryAsync(itemId), r => new { success = true, history = r.History });

    // ── Pedidos ────────────────────────────────────────────────────────

    [McpServerTool, Description("Lista los estados configurables de pedidos de inventario.")]
    public static async Task<object> SegarisListInvOrderStatuses(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListInvOrderStatusesAsync(), r => new { success = true, statuses = r.Statuses });

    [McpServerTool, Description("Lista los pedidos de inventario visibles para el usuario autenticado.")]
    public static async Task<object> SegarisListInvOrders(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListInvOrdersAsync(), r => new { success = true, orders = r.Orders });

    [McpServerTool, Description("Lista las líneas (artículos) de un pedido de inventario concreto.")]
    public static async Task<object> SegarisListInvOrderLines(
        HorizonSegarisClient client,
        [Description("ID del pedido.")] int orderId) =>
        SegarisToolHelpers.Wrap(await client.ListInvOrderLinesAsync(orderId), r => new { success = true, lines = r.Lines });

    [McpServerTool, Description("Devuelve estadísticas agregadas de un pedido de inventario (nº de líneas, total, etc.).")]
    public static async Task<object> SegarisGetInvOrderStats(
        HorizonSegarisClient client,
        [Description("ID del pedido.")] int orderId) =>
        SegarisToolHelpers.Wrap(await client.GetInvOrderStatsAsync(orderId), r => new { success = true, stats = r.Stats });
}

