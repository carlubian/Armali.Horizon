using System.ComponentModel;
using Armali.Horizon.Contracts.Segaris;
using ModelContextProtocol.Server;

namespace Armali.Horizon.MCP.Tools;

/// <summary>
/// Tools de lectura del módulo Project de Segaris.
/// </summary>
[McpServerToolType]
public static class SegarisProjectMcpTools
{
    [McpServerTool, Description("Lista los programas de proyecto definidos en Segaris.")]
    public static async Task<object> SegarisListProjectPrograms(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListProjectProgramsAsync(), r => new { success = true, programs = r.Programs });
    
    [McpServerTool, Description("Lista los ejes de proyecto. Opcionalmente filtra por programId.")]
    public static async Task<object> SegarisListProjectAxes(
        HorizonSegarisClient client,
        [Description("ID del programa al que pertenecen los ejes. Omitir para devolver todos.")]
        int? programId = null) =>
        SegarisToolHelpers.Wrap(await client.ListProjectAxesAsync(programId), r => new { success = true, axes = r.Axes });
    
    [McpServerTool, Description("Lista los estados configurables que puede tener un proyecto.")]
    public static async Task<object> SegarisListProjectStatuses(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListProjectStatusesAsync(), r => new { success = true, statuses = r.Statuses });
    
    [McpServerTool, Description(
        "Lista los proyectos visibles para el usuario autenticado. " +
        "Los proyectos privados sólo se devuelven si el usuario es su creador.")]
    public static async Task<object> SegarisListProjects(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListProjectsAsync(), r => new { success = true, projects = r.Projects });
    
    [McpServerTool, Description("Lista las categorías de ficheros asociados a proyectos.")]
    public static async Task<object> SegarisListProjectSubEntityCategories(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListProjectSubEntityCategoriesAsync(), r => new { success = true, categories = r.Categories });
    
    [McpServerTool, Description("Lista los ficheros asociados a un proyecto concreto.")]
    public static async Task<object> SegarisListProjectSubEntities(
        HorizonSegarisClient client,
        [Description("ID del proyecto cuyo listado de ficheros se solicita.")] int projectId) =>
        SegarisToolHelpers.Wrap(await client.ListProjectSubEntitiesAsync(projectId), r => new { success = true, subEntities = r.SubEntities });
    
    [McpServerTool, Description("Lista las categorías de riesgos de proyecto.")]
    public static async Task<object> SegarisListProjectRiskCategories(HorizonSegarisClient client) =>
        SegarisToolHelpers.Wrap(await client.ListProjectRiskCategoriesAsync(), r => new { success = true, categories = r.Categories });
    
    [McpServerTool, Description("Lista los elementos de riesgo asociados a un proyecto, con su Score calculado.")]
    public static async Task<object> SegarisListProjectRisks(
        HorizonSegarisClient client,
        [Description("ID del proyecto cuyos riesgos se solicitan.")] int projectId) =>
        SegarisToolHelpers.Wrap(await client.ListProjectRisksAsync(projectId), r => new { success = true, risks = r.Risks });
    
    [McpServerTool, Description(
        "Lista los presupuestos anuales de un proyecto (estimado vs real). " +
        "El campo SpentPercent es el porcentaje del estimado ya gastado.")]
    public static async Task<object> SegarisListProjectBudgets(
        HorizonSegarisClient client,
        [Description("ID del proyecto cuyos presupuestos se solicitan.")] int projectId) =>
        SegarisToolHelpers.Wrap(await client.ListProjectBudgetsAsync(projectId), r => new { success = true, budgets = r.Budgets });
}

