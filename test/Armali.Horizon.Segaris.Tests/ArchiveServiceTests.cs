using Armali.Horizon.Segaris.Model;
using Armali.Horizon.Segaris.Services;

namespace Armali.Horizon.Segaris.Tests;

/// <summary>
/// Tests de las operaciones CRUD de base de datos de ArchiveService.
/// Las operaciones de archivo (Upload/Download/Delete file) dependen de
/// DatalakeService que requiere credenciales de Azure, por lo que se
/// prueban por separado en tests de integración. Aquí se pasa null
/// como DatalakeService y las entidades se crean sin File para evitar
/// que DeleteArchive intente llamar al datalake.
/// </summary>
[TestClass]
public class ArchiveServiceTests
{
    private TestDbContextFactory _factory = null!;
    private ArchiveService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new TestDbContextFactory();
        // DatalakeService es null — solo se testean operaciones de DB.
        // DeleteArchive no invoca DatalakeService si Entity.File está vacío.
        _service = new ArchiveService(_factory, null!);
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    // ── GetArchiveCategories ─────────────────────────────────

    [TestMethod]
    public async Task GetArchiveCategories_ReturnsSeedData()
    {
        var categories = await _service.GetArchiveCategories();

        categories.ShouldNotBeEmpty();
        categories.Count.ShouldBe(8);
        categories.ShouldContain(c => c.Name == "Government");
    }

    // ── GetArchiveStatuses ───────────────────────────────────

    [TestMethod]
    public async Task GetArchiveStatuses_ReturnsSeedData()
    {
        var statuses = await _service.GetArchiveStatuses();

        statuses.Count.ShouldBe(3);
        statuses.ShouldContain(s => s.Name == "Pending" && s.Color == "gold");
        statuses.ShouldContain(s => s.Name == "Active" && s.Color == "green");
        statuses.ShouldContain(s => s.Name == "Deprecated" && s.Color == "red");
    }

    // ── AddArchive + GetArchiveEntities ──────────────────────

    [TestMethod]
    public async Task AddArchive_AndRetrieve_ReturnsEntity()
    {
        var entity = new ArchiveEntity
        {
            Name = "Test Archive",
            Date = new DateTime(2026, 1, 15),
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddArchive(entity);

        var all = await _service.GetArchiveEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Test Archive");
    }

    // ── Privacy filtering ────────────────────────────────────

    [TestMethod]
    public async Task GetArchiveEntities_FiltersPrivateEntitiesFromOthers()
    {
        // Entidad pública de user1 — visible para todos
        await _service.AddArchive(new ArchiveEntity
        {
            Name = "Publico",
            Date = DateTime.Now,
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        });

        // Entidad privada de user1 — solo visible para user1
        await _service.AddArchive(new ArchiveEntity
        {
            Name = "Privado User1",
            Date = DateTime.Now,
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = true,
            Creator = "user1"
        });

        // user1 ve ambas
        var user1Results = await _service.GetArchiveEntities("user1");
        user1Results.Count.ShouldBe(2);

        // user2 solo ve la pública
        var user2Results = await _service.GetArchiveEntities("user2");
        user2Results.Count.ShouldBe(1);
        user2Results[0].Name.ShouldBe("Publico");
    }

    // ── UpdateArchive ────────────────────────────────────────

    [TestMethod]
    public async Task UpdateArchive_ModifiesEntity()
    {
        var entity = new ArchiveEntity
        {
            Name = "Original",
            Date = DateTime.Now,
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddArchive(entity);

        // Actualizar
        entity.Name = "Modificado";
        await _service.UpdateArchive(entity);

        var all = await _service.GetArchiveEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Modificado");
    }

    // ── DeleteArchive ────────────────────────────────────────

    [TestMethod]
    public async Task DeleteArchive_RemovesEntity()
    {
        // File vacío para que DeleteArchive no invoque DatalakeService
        var entity = new ArchiveEntity
        {
            Name = "Para eliminar",
            Date = DateTime.Now,
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddArchive(entity);
        var all = await _service.GetArchiveEntities("user1");
        all.Count.ShouldBe(1);

        await _service.DeleteArchive(entity.Id);

        var afterDelete = await _service.GetArchiveEntities("user1");
        afterDelete.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteArchive_NonExistentId_DoesNotThrow()
    {
        // No debe lanzar excepción al intentar eliminar un ID que no existe
        await Should.NotThrowAsync(() => _service.DeleteArchive(9999));
    }
}

