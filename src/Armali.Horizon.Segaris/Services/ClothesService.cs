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
    
    public async Task<List<ClothesColor>> GetClothesColors()
    {
        await using var context = Factory.CreateDbContext();
        return await context.ClothesColors
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<ClothesColorStyle>> GetClothesColorStyles()
    {
        await using var context = Factory.CreateDbContext();
        return await context.ClothesColorStyles
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
    
    // ── Color assignments ───────────────────────────────────
    
    public async Task<List<ClothesColorAssignment>> GetColorAssignments(int garmentId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.ClothesColorAssignments
            .Where(a => a.GarmentId == garmentId)
            .OrderBy(a => a.StyleId)
            .AsNoTracking()
            .ToListAsync();
    }
    
    /// <summary>
    /// Devuelve todas las asignaciones de color agrupadas por GarmentId.
    /// Se usa para mostrar los colores en la tabla principal sin N+1 queries.
    /// </summary>
    public async Task<Dictionary<int, List<ClothesColorAssignment>>> GetAllColorAssignments()
    {
        await using var context = Factory.CreateDbContext();
        var all = await context.ClothesColorAssignments
            .AsNoTracking()
            .ToListAsync();
        return all
            .GroupBy(a => a.GarmentId)
            .ToDictionary(g => g.Key, g => g.OrderBy(a => a.StyleId).ToList());
    }
    
    public async Task AddColorAssignment(ClothesColorAssignment assignment)
    {
        await using var context = Factory.CreateDbContext();
        context.ClothesColorAssignments.Add(assignment);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateColorAssignment(ClothesColorAssignment assignment)
    {
        await using var context = Factory.CreateDbContext();
        context.ClothesColorAssignments.Update(assignment);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteColorAssignment(int id)
    {
        await using var context = Factory.CreateDbContext();
        var assignment = await context.ClothesColorAssignments.FindAsync(id);
        if (assignment != null)
        {
            context.ClothesColorAssignments.Remove(assignment);
            await context.SaveChangesAsync();
        }
    }
}