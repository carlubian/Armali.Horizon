using Armali.Horizon.Segaris.Model;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Segaris.Services;

public class FirebirdService
{
    private readonly IDbContextFactory<SegarisDbContext> Factory;

    public FirebirdService(IDbContextFactory<SegarisDbContext> factory)
    {
        Factory = factory;
    }
    
    public async Task<List<FirebirdCategory>> GetFirebirdCategories()
    {
        await using var context = Factory.CreateDbContext();
        return await context.FirebirdCategories
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<FirebirdStatus>> GetFirebirdStatuses()
    {
        await using var context = Factory.CreateDbContext();
        return await context.FirebirdStatuses
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<FirebirdEntity>> GetFirebirdEntities(string userId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.FirebirdEntities
            .Where(e => !e.IsPrivate || e.Creator == userId)
            .OrderBy(e => e.CategoryId)
            .ThenBy(e => e.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddFirebird(FirebirdEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.FirebirdEntities.Add(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateFirebird(FirebirdEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.FirebirdEntities.Update(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteFirebird(int id)
    {
        await using var context = Factory.CreateDbContext();
        var Entity = await context.FirebirdEntities.FindAsync(id);
        if (Entity != null)
        {
            context.FirebirdEntities.Remove(Entity);
            await context.SaveChangesAsync();
        }
    }
    
    public async Task<List<FirebirdSubEntity>> GetFirebirdSubEntities(FirebirdEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        return await context.FirebirdSubEntities
            .Where(e => e.FirebirdId == entity.Id)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task AddFirebirdSubEntity(FirebirdSubEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.FirebirdSubEntities.Add(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateFirebirdSubEntity(FirebirdSubEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.FirebirdSubEntities.Update(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteFirebirdSubEntity(int id)
    {
        await using var context = Factory.CreateDbContext();
        var SubEntity = await context.FirebirdSubEntities.FindAsync(id);
        if (SubEntity != null)
        {
            context.FirebirdSubEntities.Remove(SubEntity);
            await context.SaveChangesAsync();
        }
    }
}