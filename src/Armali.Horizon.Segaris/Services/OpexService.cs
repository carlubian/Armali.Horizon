using Armali.Horizon.Segaris.Model;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Segaris.Services;

public class OpexService
{
    private readonly IDbContextFactory<SegarisDbContext> Factory;

    public OpexService(IDbContextFactory<SegarisDbContext> factory)
    {
        Factory = factory;
    }
    
    public async Task<List<OpexCategory>> GetOpexCategories()
    {
        await using var context = Factory.CreateDbContext();
        return await context.OpexCategories
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<OpexStatus>> GetOpexStatuses()
    {
        await using var context = Factory.CreateDbContext();
        return await context.OpexStatuses
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<OpexEntity>> GetOpexEntities(string userId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.OpexEntities
            .Where(e => !e.IsPrivate || e.Creator == userId)
            .OrderBy(e => e.CategoryId)
            .ThenBy(e => e.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddOpex(OpexEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.OpexEntities.Add(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateOpex(OpexEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.OpexEntities.Update(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteOpex(int id)
    {
        await using var context = Factory.CreateDbContext();
        var Entity = await context.OpexEntities.FindAsync(id);
        if (Entity != null)
        {
            context.OpexEntities.Remove(Entity);
            await context.SaveChangesAsync();
        }
    }
    
    public async Task<List<OpexSubEntity>> GetOpexSubEntities(OpexEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        return await context.OpexSubEntities
            .Where(e => e.ContractId == entity.Id)
            .OrderByDescending(e => e.Date)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task AddOpexSubEntity(OpexSubEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.OpexSubEntities.Add(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateOpexSubEntity(OpexSubEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.OpexSubEntities.Update(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteOpexSubEntity(int id)
    {
        await using var context = Factory.CreateDbContext();
        var SubEntity = await context.OpexSubEntities.FindAsync(id);
        if (SubEntity != null)
        {
            context.OpexSubEntities.Remove(SubEntity);
            await context.SaveChangesAsync();
        }
    }

    public async Task<OpexStats> GetOpexStats(int id)
    {
        await using var context = Factory.CreateDbContext();
        var result = new OpexStats
        {
            SubEntityCount = await context.OpexSubEntities.CountAsync(e => e.ContractId == id),
            TotalAmount = await context.OpexSubEntities.Where(e => e.ContractId == id).SumAsync(e => e.Amount)
        };
        
        // Truncate amount to two decimal places
        result.TotalAmount = Math.Round(result.TotalAmount, 2);
        
        return result;
    }
}