using Microsoft.EntityFrameworkCore;
using Armali.Horizon.Segaris.Model;

namespace Armali.Horizon.Segaris.Services;

public class CapexService
{
    private readonly IDbContextFactory<SegarisDbContext> Factory;

    public CapexService(IDbContextFactory<SegarisDbContext> factory)
    {
        Factory = factory;
    }
    
    public async Task<List<CapexCategory>> GetCapexCategories()
    {
        await using var context = Factory.CreateDbContext();
        return await context.CapexCategories
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<CapexStatus>> GetCapexStatuses()
    {
        await using var context = Factory.CreateDbContext();
        return await context.CapexStatuses
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<CapexEntity>> GetCapexEntities(string userId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.CapexEntities
            .Where(e => !e.IsPrivate || e.Creator == userId)
            .OrderByDescending(e => e.Date)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddCapex(CapexEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.CapexEntities.Add(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateCapex(CapexEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.CapexEntities.Update(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteCapex(int id)
    {
        await using var context = Factory.CreateDbContext();
        var Entity = await context.CapexEntities.FindAsync(id);
        if (Entity != null)
        {
            context.CapexEntities.Remove(Entity);
            await context.SaveChangesAsync();
        }
    }
}