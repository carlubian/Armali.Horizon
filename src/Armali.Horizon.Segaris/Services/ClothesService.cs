using Armali.Horizon.Segaris.Model;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Segaris.Services;

public class ClothesService
{
    private readonly IDbContextFactory<SegarisDbContext> Factory;

    public ClothesService(IDbContextFactory<SegarisDbContext> factory)
    {
        Factory = factory;
    }
    
    public async Task<List<ClothesCategory>> GetClothesCategories()
    {
        await using var context = Factory.CreateDbContext();
        return await context.ClothesCategories
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<ClothesStatus>> GetClothesStatuses()
    {
        await using var context = Factory.CreateDbContext();
        return await context.ClothesStatuses
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<ClothesWashType>> GetClothesWashTypes()
    {
        await using var context = Factory.CreateDbContext();
        return await context.ClothesWashTypes
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<ClothesEntity>> GetClothesEntities(string userId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.ClothesEntities
            .Where(e => !e.IsPrivate || e.Creator == userId)
            .OrderByDescending(e => e.Date)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddClothes(ClothesEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.ClothesEntities.Add(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateClothes(ClothesEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.ClothesEntities.Update(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteClothes(int id)
    {
        await using var context = Factory.CreateDbContext();
        var Entity = await context.ClothesEntities.FindAsync(id);
        if (Entity != null)
        {
            context.ClothesEntities.Remove(Entity);
            await context.SaveChangesAsync();
        }
    }
}