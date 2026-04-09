using Armali.Horizon.Autoconfig.Model;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Autoconfig.Services;

public class AutoconfigService
{
    private readonly IDbContextFactory<AutoconfigDbContext> Factory;

    public AutoconfigService(IDbContextFactory<AutoconfigDbContext> factory)
    {
        Factory = factory;
    }
    
    public async Task<List<AutoconfigNode>> GetNodes()
    {
        await using var context = Factory.CreateDbContext();
        return await context.Nodes
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<NodeStats> GetNodeStats(int nodeId)
    {
        await using var context = Factory.CreateDbContext();
        var result = new NodeStats();
        
        // Fill the result as follows:
        // AppCount is the sum of apps linked to this node
        // TotalKbSize is the sum of all file sizes linked to this node (through apps and versions)
        result.AppCount = await context.Apps
            .Where(a => a.NodeId == nodeId)
            .AsNoTracking()
            .CountAsync();
        result.TotalKbSize = await context.Files
            .Where(f => context.Versions
                .Where(v => context.Apps
                    .Where(a => a.NodeId == nodeId)
                    .Select(a => a.Id)
                    .Contains(v.AppId))
                .Select(v => v.Id)
                .Contains(f.VersionId))
            .AsNoTracking()
            .SumAsync(f => f.KbSize);

        return result;
    }
    
    public async Task AddNode(AutoconfigNode entity)
    {
        await using var context = Factory.CreateDbContext();
        context.Nodes.Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateNode(AutoconfigNode entity)
    {
        await using var context = Factory.CreateDbContext();
        context.Nodes.Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteNode(int id)
    {
        await using var context = Factory.CreateDbContext();
        var Entity = await context.Nodes.FindAsync(id);
        if (Entity != null)
        {
            context.Nodes.Remove(Entity);
            await context.SaveChangesAsync();
        }
    }
    
    public async Task<List<AutoconfigApp>> GetApps()
    {
        await using var context = Factory.CreateDbContext();
        return await context.Apps
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<AutoconfigApp>> GetAppsIn(int nodeId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.Apps
            .Where(a => a.NodeId == nodeId)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<AutoconfigVersion>> GetVersions()
    {
        await using var context = Factory.CreateDbContext();
        return await context.Versions
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<AutoconfigVersion>> GetVersionsIn(int appId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.Versions
            .Where(a => a.AppId == appId)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<AutoconfigFile>> GetFiles()
    {
        await using var context = Factory.CreateDbContext();
        return await context.Files
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<AutoconfigFile>> GetFilesIn(int versionId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.Files
            .Where(a => a.VersionId == versionId)
            .AsNoTracking()
            .ToListAsync();
    }
}
