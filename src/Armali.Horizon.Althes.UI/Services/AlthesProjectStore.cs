using Armali.Horizon.Althes.UI.Model;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Althes.UI.Services;

/// <summary>CRUD de proyectos Althes configurados en la UI.</summary>
public class AlthesProjectStore
{
    private readonly IDbContextFactory<AlthesUiDbContext> Factory;
    
    public AlthesProjectStore(IDbContextFactory<AlthesUiDbContext> factory)
        => Factory = factory;
    
    public async Task<List<AlthesProject>> ListAsync(CancellationToken ct = default)
    {
        await using var db = await Factory.CreateDbContextAsync(ct);
        return await db.Projects.AsNoTracking().OrderBy(p => p.Name).ToListAsync(ct);
    }
    
    public async Task<AlthesProject?> GetAsync(int id, CancellationToken ct = default)
    {
        await using var db = await Factory.CreateDbContextAsync(ct);
        return await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
    }
    
    public async Task AddAsync(AlthesProject project, CancellationToken ct = default)
    {
        await using var db = await Factory.CreateDbContextAsync(ct);
        project.CreatedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;
        db.Projects.Add(project);
        await db.SaveChangesAsync(ct);
    }
    
    public async Task UpdateAsync(AlthesProject project, CancellationToken ct = default)
    {
        await using var db = await Factory.CreateDbContextAsync(ct);
        var existing = await db.Projects.FindAsync([project.Id], ct);
        if (existing is null) return;
        existing.Name = project.Name;
        existing.ProjectId = project.ProjectId;
        // Solo actualizar la ApiKey si se proporciona un valor nuevo no vacío
        if (!string.IsNullOrWhiteSpace(project.ApiKey))
            existing.ApiKey = project.ApiKey;
        existing.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }
    
    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        await using var db = await Factory.CreateDbContextAsync(ct);
        var existing = await db.Projects.FindAsync([id], ct);
        if (existing is null) return;
        db.Projects.Remove(existing);
        await db.SaveChangesAsync(ct);
    }
}

