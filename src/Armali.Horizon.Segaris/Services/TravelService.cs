using Armali.Horizon.Segaris.Model;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Segaris.Services;

public class TravelService
{
    private readonly IDbContextFactory<SegarisDbContext> Factory;

    public TravelService(IDbContextFactory<SegarisDbContext> factory)
    {
        Factory = factory;
    }
    
    public async Task<List<TravelCategory>> GetTravelCategories()
    {
        await using var context = Factory.CreateDbContext();
        return await context.TravelCategories
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<TravelCostCenter>> GetTravelCostCenters()
    {
        await using var context = Factory.CreateDbContext();
        return await context.TravelCostCenters
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<TravelStatus>> GetTravelStatuses()
    {
        await using var context = Factory.CreateDbContext();
        return await context.TravelStatuses
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<TravelEntity>> GetTravelEntities(string userId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.TravelEntities
            .Where(e => !e.IsPrivate || e.Creator == userId)
            .OrderByDescending(e => e.StartDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddTravel(TravelEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.TravelEntities.Add(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateTravel(TravelEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.TravelEntities.Update(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteTravel(int id)
    {
        await using var context = Factory.CreateDbContext();
        var Entity = await context.TravelEntities.FindAsync(id);
        if (Entity != null)
        {
            context.TravelEntities.Remove(Entity);
            await context.SaveChangesAsync();
        }
    }
    
    public async Task<List<TravelSubEntityCategory>> GetTravelSubEntityCategories()
    {
        await using var context = Factory.CreateDbContext();
        return await context.TravelSubEntityCategories
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<TravelSubEntity>> GetTravelSubEntities(TravelEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        return await context.TravelSubEntities
            .Where(e => e.TravelId == entity.Id)
            .OrderByDescending(e => e.Date)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task AddTravelSubEntity(TravelSubEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.TravelSubEntities.Add(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateTravelSubEntity(TravelSubEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.TravelSubEntities.Update(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteTravelSubEntity(int id)
    {
        await using var context = Factory.CreateDbContext();
        var SubEntity = await context.TravelSubEntities.FindAsync(id);
        if (SubEntity != null)
        {
            context.TravelSubEntities.Remove(SubEntity);
            await context.SaveChangesAsync();
        }
    }
}