using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Segaris.Tests;

/// <summary>
/// Factory de DbContext que usa SQLite in-memory para tests.
/// La conexión se mantiene abierta durante toda la vida del helper
/// para que la base de datos no se destruya entre operaciones.
/// Implementa IDisposable para cerrar la conexión al final del test.
/// </summary>
internal class TestDbContextFactory : IDbContextFactory<SegarisDbContext>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<SegarisDbContext> _options;

    public TestDbContextFactory()
    {
        // Conexión SQLite in-memory compartida — se mantiene abierta
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<SegarisDbContext>()
            .UseSqlite(_connection)
            .Options;

        // Crear el esquema
        using var context = new SegarisDbContext(_options);
        context.Database.EnsureCreated();
    }

    public SegarisDbContext CreateDbContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}

