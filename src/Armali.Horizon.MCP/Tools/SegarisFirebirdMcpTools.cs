using System.ComponentModel;
using Armali.Horizon.Contracts.Segaris;
using ModelContextProtocol.Server;

namespace Armali.Horizon.MCP.Tools;

/// <summary>Tools de lectura del módulo Firebird (People) de Segaris.</summary>
[McpServerToolType]
public static class SegarisFirebirdMcpTools
{
    [McpServerTool, Description(
        "Lista las categorías de personas. " +
        "Nota: «Firebird» es un nombre en clave; este módulo gestiona personas.")]
    public static async Task<object> SegarisListFirebirdCategories(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListFirebirdCategoriesAsync(), r => new { success = true, categories = r.Categories });
    
    [McpServerTool, Description(
        "Lista los estados posibles de personas. " +
        "Nota: «Firebird» es un nombre en clave; este módulo gestiona personas.")]
    public static async Task<object> SegarisListFirebirdStatuses(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListFirebirdStatusesAsync(), r => new { success = true, statuses = r.Statuses });
    
    [McpServerTool, Description(
        "Lista las personas registradas visibles para el usuario autenticado. " +
        "Las entradas privadas sólo son visibles para su creador. " +
        "Cada persona incluye categoría, estado, ubicación, fecha de nacimiento y si está al tanto (IsAware). " +
        "Nota: El «cumpleaños» de una persona solo incluye día y mes, se ignora el año.")]
    public static async Task<object> SegarisListFirebirds(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListFirebirdsAsync(), r => new { success = true, items = r.Items });
    
    [McpServerTool, Description(
        "Lista los eventos asociados a una persona concreta (sub-entidades con fecha y descripción). ")]
    public static async Task<object> SegarisListFirebirdSubEntities(
        HorizonSegarisClient client,
        [Description("ID de la persona cuyos eventos se quieren consultar.")] int firebirdId) =>
        SegarisToolHelpers.Wrap(await client.ListFirebirdSubEntitiesAsync(firebirdId), r => new { success = true, subEntities = r.SubEntities });
}

