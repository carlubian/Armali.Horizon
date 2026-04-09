using Armali.Horizon.Segaris.Model;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Segaris.Services;

public class MaintService
{
    private readonly IDbContextFactory<SegarisDbContext> Factory;

    public MaintService(IDbContextFactory<SegarisDbContext> factory)
    {
        Factory = factory;
    }
    
    public async Task<List<MaintCategory>> GetMaintCategories()
    {
        await using var context = Factory.CreateDbContext();
        return await context.MaintCategories
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<MaintStatus>> GetMaintStatuses()
    {
        await using var context = Factory.CreateDbContext();
        return await context.MaintStatuses
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<MaintEntity>> GetMaintEntities(string userId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.MaintEntities
            .Where(e => !e.IsPrivate || e.Creator == userId)
            .OrderByDescending(e => e.Date)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddMaint(MaintEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.MaintEntities.Add(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateMaint(MaintEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.MaintEntities.Update(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteMaint(int id)
    {
        await using var context = Factory.CreateDbContext();
        var Entity = await context.MaintEntities.FindAsync(id);
        if (Entity != null)
        {
            context.MaintEntities.Remove(Entity);
            await context.SaveChangesAsync();
        }
    }
}