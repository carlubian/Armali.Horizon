using Armali.Horizon.Althes.UI.Model;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Althes.UI;

public class AlthesUiDbContext(DbContextOptions<AlthesUiDbContext> options) : DbContext(options)
{
    public DbSet<AlthesProject> Projects => Set<AlthesProject>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // ProjectId único por instancia gestionada por la UI
        modelBuilder.Entity<AlthesProject>().HasIndex(p => p.ProjectId).IsUnique();
    }
}

