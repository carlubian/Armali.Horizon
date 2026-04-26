using Armali.Horizon.Identity.Model;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Identity;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityUserRole> UserRoles { get; set; }
    public DbSet<IdentityToken> Tokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IdentityUser>()
            .HasIndex(u => u.UserName)
            .IsUnique();

        modelBuilder.Entity<IdentityUserRole>()
            .HasIndex(ur => new { ur.UserId, ur.Role })
            .IsUnique();

        modelBuilder.Entity<IdentityToken>()
            .HasIndex(t => t.TokenHash)
            .IsUnique();

        modelBuilder.Entity<IdentityToken>()
            .HasIndex(t => t.UserId);
    }
}