using Armali.Horizon.Autoconfig.Model;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Autoconfig;

public class AutoconfigDbContext(DbContextOptions<AutoconfigDbContext> options) : DbContext(options)
{
    // Entities
    public DbSet<AutoconfigNode> Nodes { get; set; }
    public DbSet<AutoconfigApp> Apps { get; set; }
    public DbSet<AutoconfigVersion> Versions { get; set; }
    public DbSet<AutoconfigFile> Files { get; set; }
    
    // Model creation
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        //modelBuilder.Entity<CapexStatus>().HasData(
        //    new CapexStatus { Id = 1, Name = "Planning", Color = "blue" },
        //    new CapexStatus { Id = 2, Name = "Completed", Color = "green" },
        //    new CapexStatus { Id = 3, Name = "Canceled", Color = "red" }
        //);
    }
}
