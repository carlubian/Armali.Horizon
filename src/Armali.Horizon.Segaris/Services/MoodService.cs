using Armali.Horizon.Segaris.Model;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Segaris.Services;

public class MoodService
{
    private readonly IDbContextFactory<SegarisDbContext> Factory;

    public MoodService(IDbContextFactory<SegarisDbContext> factory)
    {
        Factory = factory;
    }
    
    public async Task<List<MoodCategory>> GetMoodCategories()
    {
        await using var context = Factory.CreateDbContext();
        return await context.MoodCategories
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<MoodEntity>> GetMoodEntities(string userId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.MoodEntities
            .Where(e => e.Creator == userId)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<MoodEntity>> GetMoodBetweenDates(DateTime start, DateTime end, string userId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.MoodEntities
            .Where(m => m.Creator == userId && m.Date >= start && m.Date <= end)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddMood(MoodEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.MoodEntities.Add(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateMood(MoodEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.MoodEntities.Update(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteMood(int id)
    {
        await using var context = Factory.CreateDbContext();
        var Entity = await context.MoodEntities.FindAsync(id);
        if (Entity != null)
        {
            context.MoodEntities.Remove(Entity);
            await context.SaveChangesAsync();
        }
    }
}