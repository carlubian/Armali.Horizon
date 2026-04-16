using Armali.Horizon.Segaris.Model;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Segaris.Services;

public class AdminService
{
    private readonly IDbContextFactory<SegarisDbContext> Factory;

    public AdminService(IDbContextFactory<SegarisDbContext> factory)
    {
        Factory = factory;
    }

    // ── Categorías ─────────────────────────────────────────────

    public async Task<List<AdminCategory>> GetAdminCategories()
    {
        await using var context = Factory.CreateDbContext();
        return await context.AdminCategories
            .AsNoTracking()
            .ToListAsync();
    }

    // ── Entidades principales ──────────────────────────────────

    public async Task<List<AdminEntity>> GetAdminEntities(string userId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.AdminEntities
            .Where(e => !e.IsPrivate || e.Creator == userId)
            .OrderBy(e => e.CategoryId)
            .ThenBy(e => e.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAdmin(AdminEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.AdminEntities.Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAdmin(AdminEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.AdminEntities.Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAdmin(int id)
    {
        await using var context = Factory.CreateDbContext();
        var entity = await context.AdminEntities.FindAsync(id);
        if (entity != null)
        {
            context.AdminEntities.Remove(entity);
            await context.SaveChangesAsync();
        }
    }

    // ── Sub-entidades (pasos / etapas) ─────────────────────────

    public async Task<List<AdminSubEntity>> GetAdminSubEntities(AdminEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        return await context.AdminSubEntities
            .Where(e => e.ProcessId == entity.Id)
            .OrderBy(e => e.DueDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddAdminSubEntity(AdminSubEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.AdminSubEntities.Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateAdminSubEntity(AdminSubEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.AdminSubEntities.Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAdminSubEntity(int id)
    {
        await using var context = Factory.CreateDbContext();
        var subEntity = await context.AdminSubEntities.FindAsync(id);
        if (subEntity != null)
        {
            context.AdminSubEntities.Remove(subEntity);
            await context.SaveChangesAsync();
        }
    }

    // ── Estadísticas calculadas ────────────────────────────────

    /// <summary>
    /// Calcula el estado de un paso individual según las reglas de negocio.
    /// </summary>
    public static AdminStepStatus ComputeStepStatus(AdminSubEntity step)
    {
        return ComputeStepStatus(step, DateTime.Today);
    }

    /// <summary>
    /// Sobrecarga testable que acepta la fecha "hoy" como parámetro.
    /// </summary>
    public static AdminStepStatus ComputeStepStatus(AdminSubEntity step, DateTime today)
    {
        if (step.IsCompleted)
            return AdminStepStatus.Finished;
        if (today < step.StartDate)
            return AdminStepStatus.NotStarted;
        if (step.DueDate >= today)
            return AdminStepStatus.OnTime;
        return AdminStepStatus.Delayed;
    }

    /// <summary>
    /// Obtiene las estadísticas agregadas de un proceso a partir de sus pasos.
    /// </summary>
    public async Task<AdminStats> GetAdminStats(int processId)
    {
        await using var context = Factory.CreateDbContext();
        var steps = await context.AdminSubEntities
            .Where(e => e.ProcessId == processId)
            .AsNoTracking()
            .ToListAsync();

        return ComputeStats(steps);
    }

    /// <summary>
    /// Calcula estadísticas a partir de una lista de pasos (usado internamente y en tests).
    /// </summary>
    public static AdminStats ComputeStats(IEnumerable<AdminSubEntity> steps)
    {
        return ComputeStats(steps, DateTime.Today);
    }

    /// <summary>
    /// Sobrecarga testable que acepta la fecha "hoy" como parámetro.
    /// </summary>
    public static AdminStats ComputeStats(IEnumerable<AdminSubEntity> steps, DateTime today)
    {
        var stats = new AdminStats();
        foreach (var step in steps)
        {
            var status = ComputeStepStatus(step, today);
            switch (status)
            {
                case AdminStepStatus.Finished:
                    stats.Finished++;
                    break;
                case AdminStepStatus.NotStarted:
                    stats.NotStarted++;
                    break;
                case AdminStepStatus.OnTime:
                    stats.OnTime++;
                    break;
                case AdminStepStatus.Delayed:
                    stats.Delayed++;
                    break;
            }
        }

        return stats;
    }
}

