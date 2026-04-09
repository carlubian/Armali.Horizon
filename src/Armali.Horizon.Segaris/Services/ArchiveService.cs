using Armali.Horizon.Segaris.Model;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Segaris.Services;

public class ArchiveService
{
    private readonly IDbContextFactory<SegarisDbContext> Factory;
    private readonly DatalakeService DatalakeService;

    public ArchiveService(IDbContextFactory<SegarisDbContext> factory, DatalakeService datalakeService)
    {
        Factory = factory;
        DatalakeService = datalakeService;
    }
    
    public async Task<List<ArchiveCategory>> GetArchiveCategories()
    {
        await using var context = Factory.CreateDbContext();
        return await context.ArchiveCategories
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<ArchiveStatus>> GetArchiveStatuses()
    {
        await using var context = Factory.CreateDbContext();
        return await context.ArchiveStatuses
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<ArchiveEntity>> GetArchiveEntities(string userId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.ArchiveEntities
            .Where(e => !e.IsPrivate || e.Creator == userId)
            .OrderByDescending(e => e.Date)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddArchive(ArchiveEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.ArchiveEntities.Add(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateArchive(ArchiveEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.ArchiveEntities.Update(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteArchive(int id)
    {
        await using var context = Factory.CreateDbContext();
        var Entity = await context.ArchiveEntities.FindAsync(id);
        if (Entity != null)
        {
            // Delete file from datalake if exists
            if (!string.IsNullOrEmpty(Entity.File))
            {
                await DatalakeService.DeleteFileAsync(Entity.File);
            }
            
            context.ArchiveEntities.Remove(Entity);
            await context.SaveChangesAsync();
        }
    }
    
    public async Task<string> UploadFileAsync(Stream content, string originalFileName, string? existingFileName = null)
    {
        // Delete existing file if present
        if (!string.IsNullOrEmpty(existingFileName))
        {
            await DatalakeService.DeleteFileAsync(existingFileName);
        }
        
        // Generate UUID filename while preserving extension
        var Extension = Path.GetExtension(originalFileName);
        var NewFileName = $"{Guid.NewGuid()}{Extension}";
        
        await DatalakeService.UploadFileAsync(content, NewFileName);
        return NewFileName;
    }
    
    public async Task<Stream> DownloadFileAsync(string fileName)
    {
        return await DatalakeService.GetFileAsync(fileName);
    }
    
    public async Task DeleteFileAsync(string fileName)
    {
        await DatalakeService.DeleteFileAsync(fileName);
    }
}