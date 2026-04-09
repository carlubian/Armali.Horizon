using Armali.Horizon.Segaris.Model;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Segaris.Services;

public class AssetService
{
    private readonly IDbContextFactory<SegarisDbContext> Factory;

    public AssetService(IDbContextFactory<SegarisDbContext> factory)
    {
        Factory = factory;
    }
    
    public async Task<List<AssetCategory>> GetAssetCategories()
    {
        await using var context = Factory.CreateDbContext();
        return await context.AssetCategories
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<AssetStatus>> GetAssetStatuses()
    {
        await using var context = Factory.CreateDbContext();
        return await context.AssetStatuses
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<AssetEntity>> GetAssetEntities(string userId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.AssetEntities
            .Where(e => !e.IsPrivate || e.Creator == userId)
            .OrderByDescending(e => e.Date)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAsset(AssetEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.AssetEntities.Add(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateAsset(AssetEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.AssetEntities.Update(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteAsset(int id)
    {
        await using var context = Factory.CreateDbContext();
        var Entity = await context.AssetEntities.FindAsync(id);
        if (Entity != null)
        {
            context.AssetEntities.Remove(Entity);
            await context.SaveChangesAsync();
        }
    }
}