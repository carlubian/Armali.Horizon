using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Althes.Tests;

/// <summary>SQLite in-memory por test, mismo patrón que en Autoconfig.Tests.</summary>
public class TestDbContextFactory : IDbContextFactory<AlthesDbContext>, IDisposable
{
    private readonly SqliteConnection Connection;
    private readonly DbContextOptions<AlthesDbContext> Options;
    
    public TestDbContextFactory()
    {
        Connection = new SqliteConnection("DataSource=:memory:");
        Connection.Open();
        Options = new DbContextOptionsBuilder<AlthesDbContext>().UseSqlite(Connection).Options;
        using var ctx = new AlthesDbContext(Options);
        ctx.Database.EnsureCreated();
    }
    
    public AlthesDbContext CreateDbContext() => new(Options);
    public void Dispose() => Connection.Dispose();
}

