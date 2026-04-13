using Armali.Horizon.Segaris.Model;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Segaris.Services;

public class ProjectService
{
    private readonly IDbContextFactory<SegarisDbContext> Factory;
    private readonly DatalakeService DatalakeService;

    public ProjectService(IDbContextFactory<SegarisDbContext> factory, DatalakeService datalakeService)
    {
        Factory = factory;
        DatalakeService = datalakeService;
    }
    
    public async Task<List<ProjectProgram>> GetProjectPrograms()
    {
        await using var context = Factory.CreateDbContext();
        return await context.ProjectPrograms
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<ProjectAxis>> GetProjectAxis(int programId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.ProjectAxis
            .Where(a => a.ProgramId == programId)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<ProjectAxis>> GetAllProjectAxis()
    {
        await using var context = Factory.CreateDbContext();
        return await context.ProjectAxis
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<ProjectStatus>> GetProjectStatuses()
    {
        await using var context = Factory.CreateDbContext();
        return await context.ProjectStatuses
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<ProjectEntity>> GetProjectEntities(string userId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.ProjectEntities
            .Include(c => c.Program)
            .Include(c => c.Axis)
            .Include(c => c.Status)
            .Where(e => !e.IsPrivate || e.Creator == userId)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<string> GetNextAvailableCode()
    {
        await using var context = Factory.CreateDbContext();
        // Project codes are int values saved as string.
        // Obtain the max value, increment, and return as six-digit string with leading zeros.
        // Run the query as embedded SQL to avoid EF errors with string-to-int conversion.
        
        var Connection = context.Database.GetDbConnection();
        await Connection.OpenAsync();
        
        await using var Command = Connection.CreateCommand();
        Command.CommandText = "SELECT MAX(CAST(Code AS INTEGER)) FROM ProjectEntities";
        
        var Result = await Command.ExecuteScalarAsync();
        
        var MaxCode = Result == DBNull.Value || Result == null ? 0 : Convert.ToInt32(Result);
        var NextCode = MaxCode + 1;
        
        return NextCode.ToString("D6");
    }

    public async Task AddProject(ProjectEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.ProjectEntities.Add(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateProject(ProjectEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.ProjectEntities.Update(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteProject(int id)
    {
        await using var context = Factory.CreateDbContext();
        var Entity = await context.ProjectEntities.FindAsync(id);
        if (Entity != null)
        {
            context.ProjectEntities.Remove(Entity);
            await context.SaveChangesAsync();
        }
    }
    
    public async Task<List<ProjectSubEntityCategory>> GetProjectSubEntityCategories()
    {
        await using var context = Factory.CreateDbContext();
        return await context.ProjectSubEntityCategories
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<ProjectSubEntity>> GetProjectSubEntities(ProjectEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        return await context.ProjectSubEntities
            .Where(e => e.ProjectId == entity.Id)
            .OrderByDescending(e => e.Date)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task AddProjectSubEntity(ProjectSubEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.ProjectSubEntities.Add(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateProjectSubEntity(ProjectSubEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.ProjectSubEntities.Update(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteProjectSubEntity(int id)
    {
        await using var context = Factory.CreateDbContext();
        var SubEntity = await context.ProjectSubEntities.FindAsync(id);
        if (SubEntity != null)
        {
            context.ProjectSubEntities.Remove(SubEntity);
            await context.SaveChangesAsync();
        }
    }

    public async Task<string> UploadFileAsync(ProjectEntity p, Stream stream, string originalFileName, string? existingFileName = null)
    {
        // Delete existing file if present
        if (!string.IsNullOrEmpty(existingFileName))
        {
            await DatalakeService.DeleteProjectFileAsync(existingFileName, p.Program!.Name, p.Axis!.Name, p.Code);
        }
        
        // Generate UUID filename while preserving extension
        var Extension = Path.GetExtension(originalFileName);
        var NewFileName = $"{Guid.NewGuid()}{Extension}";
        
        await DatalakeService.UploadProjectFileAsync(stream, NewFileName, p.Program!.Name, p.Axis!.Name, p.Code);
        return NewFileName;
    }
    
    public async Task<Stream> DownloadFileAsync(ProjectEntity p, string fileName)
    {
        return await DatalakeService.GetProjectFileAsync(fileName, p.Program!.Name, p.Axis!.Name, p.Code);
    }
    
    public async Task DeleteFileAsync(ProjectEntity p, string fileName)
    {
        await DatalakeService.DeleteProjectFileAsync(fileName, p.Program!.Name, p.Axis!.Name, p.Code);
    }
    
    // ── Risk Analysis ────────────────────────────────────────
    
    public async Task<List<ProjectRiskCategory>> GetProjectRiskCategories()
    {
        await using var context = Factory.CreateDbContext();
        return await context.ProjectRiskCategories
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<ProjectRiskElement>> GetProjectRiskElements(int projectId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.ProjectRiskElements
            .Where(e => e.ProjectId == projectId)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task AddProjectRiskElement(ProjectRiskElement element)
    {
        await using var context = Factory.CreateDbContext();
        context.ProjectRiskElements.Add(element);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateProjectRiskElement(ProjectRiskElement element)
    {
        await using var context = Factory.CreateDbContext();
        context.ProjectRiskElements.Update(element);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteProjectRiskElement(int id)
    {
        await using var context = Factory.CreateDbContext();
        var Element = await context.ProjectRiskElements.FindAsync(id);
        if (Element != null)
        {
            context.ProjectRiskElements.Remove(Element);
            await context.SaveChangesAsync();
        }
    }
    
    // ── Budgets ──────────────────────────────────────────────
    
    public async Task<List<ProjectBudget>> GetProjectBudgets(int projectId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.ProjectBudgets
            .Where(b => b.ProjectId == projectId)
            .OrderBy(b => b.Year)
            .AsNoTracking()
            .ToListAsync();
    }
    
    /// <summary>
    /// Verifica si ya existe un presupuesto para el mismo proyecto y año.
    /// Opcionalmente excluye un Id (para permitir la edición del mismo registro).
    /// </summary>
    public async Task<bool> BudgetYearExists(int projectId, int year, int? excludeId = null)
    {
        await using var context = Factory.CreateDbContext();
        return await context.ProjectBudgets
            .AnyAsync(b => b.ProjectId == projectId 
                        && b.Year == year 
                        && (excludeId == null || b.Id != excludeId));
    }
    
    public async Task AddProjectBudget(ProjectBudget budget)
    {
        await using var context = Factory.CreateDbContext();
        context.ProjectBudgets.Add(budget);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateProjectBudget(ProjectBudget budget)
    {
        await using var context = Factory.CreateDbContext();
        context.ProjectBudgets.Update(budget);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteProjectBudget(int id)
    {
        await using var context = Factory.CreateDbContext();
        var Budget = await context.ProjectBudgets.FindAsync(id);
        if (Budget != null)
        {
            context.ProjectBudgets.Remove(Budget);
            await context.SaveChangesAsync();
        }
    }
    
    /// <summary>
    /// Calcula el gasto real de un proyecto para un año concreto, sumando:
    /// 1) CapexEntity asociados al proyecto cuya Date esté en el año.
    /// 2) OpexSubEntity de contratos Opex asociados al proyecto cuya Date esté en el año.
    /// 3) TravelSubEntity de viajes asociados al proyecto cuyo StartDate esté en el año.
    /// 4) InvOrderSubEntity vinculadas al proyecto cuyo pedido padre tenga PurchaseDate en el año.
    /// </summary>
    public async Task<double> CalculateActualBudget(int projectId, int year)
    {
        await using var context = Factory.CreateDbContext();
        
        var YearStart = new DateTime(year, 1, 1);
        var YearEnd = new DateTime(year + 1, 1, 1);
        
        // 1) Capex directo vinculado al proyecto en ese año
        var CapexTotal = await context.CapexEntities
            .Where(c => c.ProjectId == projectId
                     && c.Date >= YearStart
                     && c.Date < YearEnd)
            .SumAsync(c => c.Amount);
        
        // 2) Opex: sub-entidades de contratos vinculados al proyecto, con Date en el año
        var OpexContractIds = await context.OpexEntities
            .Where(o => o.ProjectId == projectId)
            .Select(o => o.Id)
            .ToListAsync();
        
        var OpexTotal = OpexContractIds.Count > 0
            ? await context.OpexSubEntities
                .Where(s => OpexContractIds.Contains(s.ContractId)
                         && s.Date >= YearStart
                         && s.Date < YearEnd)
                .SumAsync(s => s.Amount)
            : 0.0;
        
        // 3) Travel: sub-entidades de viajes vinculados al proyecto, con StartDate en el año
        var TravelIds = await context.TravelEntities
            .Where(t => t.ProjectId == projectId
                     && t.StartDate >= YearStart
                     && t.StartDate < YearEnd)
            .Select(t => t.Id)
            .ToListAsync();
        
        var TravelTotal = TravelIds.Count > 0
            ? await context.TravelSubEntities
                .Where(s => TravelIds.Contains(s.TravelId))
                .SumAsync(s => s.Amount)
            : 0.0;
        
        // 4) Inventario: líneas de pedido vinculadas al proyecto cuyo pedido padre
        //    tenga PurchaseDate en el año objetivo.
        var InvOrderIds = await context.InvOrderEntities
            .Where(o => o.PurchaseDate >= YearStart
                     && o.PurchaseDate < YearEnd)
            .Select(o => o.Id)
            .ToListAsync();
        
        var InventoryTotal = InvOrderIds.Count > 0
            ? await context.InvOrderSubEntities
                .Where(s => s.ProjectId == projectId
                         && InvOrderIds.Contains(s.OrderId))
                .SumAsync(s => s.Amount)
            : 0.0;
        
        // Final) Devolver el gasto total como valor absoluto, ya que el porcentaje
        // de gastos se calcula mejor sobre cifras positivas
        var FinalSum = CapexTotal + OpexTotal + TravelTotal + InventoryTotal;
        return Math.Abs(FinalSum);
    }
}
