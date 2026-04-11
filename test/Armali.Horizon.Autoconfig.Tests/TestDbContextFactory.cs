using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Autoconfig.Tests;

/// <summary>
/// Factory de DbContext que usa SQLite in-memory para tests.
/// La conexión se mantiene abierta durante toda la vida del helper
/// para que la base de datos no se destruya entre operaciones.
/// </summary>
public class TestDbContextFactory : IDbContextFactory<AutoconfigDbContext>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AutoconfigDbContext> _options;

    public TestDbContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AutoconfigDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new AutoconfigDbContext(_options);
        context.Database.EnsureCreated();
    }

    public AutoconfigDbContext CreateDbContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}

