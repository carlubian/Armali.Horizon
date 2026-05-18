using Armali.Horizon.Althes.Model;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Althes;

public class AlthesDbContext(DbContextOptions<AlthesDbContext> options) : DbContext(options)
{
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<AgentRun> Runs => Set<AgentRun>();
    public DbSet<AgentMessage> Messages => Set<AgentMessage>();
    public DbSet<UserQuery> UserQueries => Set<UserQuery>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Índices para consultas frecuentes
        modelBuilder.Entity<Conversation>().HasIndex(c => c.Status);
        modelBuilder.Entity<Conversation>().HasIndex(c => c.StartedAt);
        modelBuilder.Entity<AgentRun>().HasIndex(r => new { r.AgentName, r.Status });
        modelBuilder.Entity<AgentRun>().HasIndex(r => r.ConversationId);
        modelBuilder.Entity<AgentRun>().HasIndex(r => r.StartedAt);
        modelBuilder.Entity<AgentMessage>().HasIndex(m => m.RunId);
        modelBuilder.Entity<AgentMessage>().HasIndex(m => m.CorrelationId);
        modelBuilder.Entity<AgentMessage>().HasIndex(m => m.Visibility);
        modelBuilder.Entity<UserQuery>().HasIndex(q => q.Status);
        modelBuilder.Entity<UserQuery>().HasIndex(q => q.ConversationId);
        modelBuilder.Entity<UserQuery>().HasIndex(q => q.CorrelationId).IsUnique();
    }
}

