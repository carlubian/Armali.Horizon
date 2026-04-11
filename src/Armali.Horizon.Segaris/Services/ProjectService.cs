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
}