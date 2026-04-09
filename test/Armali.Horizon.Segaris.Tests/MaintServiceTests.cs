using Armali.Horizon.Segaris.Model;
using Armali.Horizon.Segaris.Services;

namespace Armali.Horizon.Segaris.Tests;

[TestClass]
public class MaintServiceTests
{
    private TestDbContextFactory _factory = null!;
    private MaintService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new TestDbContextFactory();
        _service = new MaintService(_factory);
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    // ── GetMaintCategories ───────────────────────────────────

    [TestMethod]
    public async Task GetMaintCategories_ReturnsSeedData()
    {
        var categories = await _service.GetMaintCategories();

        categories.ShouldNotBeEmpty();
        categories.Count.ShouldBe(7);
        categories.ShouldContain(c => c.Name == "Platform");
    }

    // ── GetMaintStatuses ─────────────────────────────────────

    [TestMethod]
    public async Task GetMaintStatuses_ReturnsSeedData()
    {
        var statuses = await _service.GetMaintStatuses();

        statuses.Count.ShouldBe(4);
        statuses.ShouldContain(s => s.Name == "Created" && s.Color == "blue");
        statuses.ShouldContain(s => s.Name == "Active" && s.Color == "gold");
        statuses.ShouldContain(s => s.Name == "Completed" && s.Color == "green");
        statuses.ShouldContain(s => s.Name == "Canceled" && s.Color == "red");
    }

    // ── AddMaint + GetMaintEntities ──────────────────────────

    [TestMethod]
    public async Task AddMaint_AndRetrieve_ReturnsEntity()
    {
        var entity = new MaintEntity
        {
            Name = "Test Maintenance",
            Date = new DateTime(2026, 1, 15),
            Details = "Revisión general del equipo",
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddMaint(entity);

        var all = await _service.GetMaintEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Test Maintenance");
        all[0].Details.ShouldBe("Revisión general del equipo");
    }

    // ── Privacy filtering ────────────────────────────────────

    [TestMethod]
    public async Task GetMaintEntities_FiltersPrivateEntitiesFromOthers()
    {
        // Entidad pública de user1 — visible para todos
        await _service.AddMaint(new MaintEntity
        {
            Name = "Publico",
            Date = DateTime.Now,
            Details = "Detalle público",
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        });

        // Entidad privada de user1 — solo visible para user1
        await _service.AddMaint(new MaintEntity
        {
            Name = "Privado User1",
            Date = DateTime.Now,
            Details = "Detalle privado",
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = true,
            Creator = "user1"
        });

        // user1 ve ambas
        var user1Results = await _service.GetMaintEntities("user1");
        user1Results.Count.ShouldBe(2);

        // user2 solo ve la pública
        var user2Results = await _service.GetMaintEntities("user2");
        user2Results.Count.ShouldBe(1);
        user2Results[0].Name.ShouldBe("Publico");
    }

    // ── UpdateMaint ──────────────────────────────────────────

    [TestMethod]
    public async Task UpdateMaint_ModifiesEntity()
    {
        var entity = new MaintEntity
        {
            Name = "Original",
            Date = DateTime.Now,
            Details = "Detalle original",
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddMaint(entity);

        // Actualizar
        entity.Name = "Modificado";
        entity.Details = "Detalle modificado";
        await _service.UpdateMaint(entity);

        var all = await _service.GetMaintEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Modificado");
        all[0].Details.ShouldBe("Detalle modificado");
    }

    // ── DeleteMaint ──────────────────────────────────────────

    [TestMethod]
    public async Task DeleteMaint_RemovesEntity()
    {
        var entity = new MaintEntity
        {
            Name = "Para eliminar",
            Date = DateTime.Now,
            Details = "Se va a eliminar",
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddMaint(entity);
        var all = await _service.GetMaintEntities("user1");
        all.Count.ShouldBe(1);

        await _service.DeleteMaint(entity.Id);

        var afterDelete = await _service.GetMaintEntities("user1");
        afterDelete.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteMaint_NonExistentId_DoesNotThrow()
    {
        // No debe lanzar excepción al intentar eliminar un ID que no existe
        await Should.NotThrowAsync(() => _service.DeleteMaint(9999));
    }
}

