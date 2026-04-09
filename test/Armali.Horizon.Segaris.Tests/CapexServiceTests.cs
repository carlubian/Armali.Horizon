using Armali.Horizon.Segaris.Model;
using Armali.Horizon.Segaris.Services;

namespace Armali.Horizon.Segaris.Tests;

[TestClass]
public class CapexServiceTests
{
    private TestDbContextFactory _factory = null!;
    private CapexService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new TestDbContextFactory();
        _service = new CapexService(_factory);
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    // ── GetCapexCategories ───────────────────────────────────

    [TestMethod]
    public async Task GetCapexCategories_ReturnsSeedData()
    {
        // Las categorías se crean como seed en SegarisDbContext.OnModelCreating
        var categories = await _service.GetCapexCategories();

        categories.ShouldNotBeEmpty();
        categories.Count.ShouldBe(11);
        categories.ShouldContain(c => c.Name == "Government");
    }

    // ── GetCapexStatuses ─────────────────────────────────────

    [TestMethod]
    public async Task GetCapexStatuses_ReturnsSeedData()
    {
        var statuses = await _service.GetCapexStatuses();

        statuses.Count.ShouldBe(3);
        statuses.ShouldContain(s => s.Name == "Planning" && s.Color == "blue");
        statuses.ShouldContain(s => s.Name == "Completed" && s.Color == "green");
        statuses.ShouldContain(s => s.Name == "Canceled" && s.Color == "red");
    }

    // ── AddCapex + GetCapexEntities ──────────────────────────

    [TestMethod]
    public async Task AddCapex_AndRetrieve_ReturnsEntity()
    {
        var entity = new CapexEntity
        {
            Name = "Test Capex",
            Date = new DateTime(2026, 1, 15),
            Amount = 500.00,
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddCapex(entity);

        var all = await _service.GetCapexEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Test Capex");
        all[0].Amount.ShouldBe(500.00);
    }

    // ── Privacy filtering ────────────────────────────────────

    [TestMethod]
    public async Task GetCapexEntities_FiltersPrivateEntitiesFromOthers()
    {
        // Entidad pública de user1 — visible para todos
        await _service.AddCapex(new CapexEntity
        {
            Name = "Publico",
            Date = DateTime.Now,
            Amount = 100,
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        });

        // Entidad privada de user1 — solo visible para user1
        await _service.AddCapex(new CapexEntity
        {
            Name = "Privado User1",
            Date = DateTime.Now,
            Amount = 200,
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = true,
            Creator = "user1"
        });

        // user1 ve ambas
        var user1Results = await _service.GetCapexEntities("user1");
        user1Results.Count.ShouldBe(2);

        // user2 solo ve la pública
        var user2Results = await _service.GetCapexEntities("user2");
        user2Results.Count.ShouldBe(1);
        user2Results[0].Name.ShouldBe("Publico");
    }

    // ── UpdateCapex ──────────────────────────────────────────

    [TestMethod]
    public async Task UpdateCapex_ModifiesEntity()
    {
        var entity = new CapexEntity
        {
            Name = "Original",
            Date = DateTime.Now,
            Amount = 100,
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddCapex(entity);

        // Actualizar
        entity.Name = "Modificado";
        entity.Amount = 999;
        await _service.UpdateCapex(entity);

        var all = await _service.GetCapexEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Modificado");
        all[0].Amount.ShouldBe(999);
    }

    // ── DeleteCapex ──────────────────────────────────────────

    [TestMethod]
    public async Task DeleteCapex_RemovesEntity()
    {
        var entity = new CapexEntity
        {
            Name = "Para eliminar",
            Date = DateTime.Now,
            Amount = 50,
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddCapex(entity);
        var all = await _service.GetCapexEntities("user1");
        all.Count.ShouldBe(1);

        await _service.DeleteCapex(entity.Id);

        var afterDelete = await _service.GetCapexEntities("user1");
        afterDelete.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteCapex_NonExistentId_DoesNotThrow()
    {
        // No debe lanzar excepción al intentar eliminar un ID que no existe
        await Should.NotThrowAsync(() => _service.DeleteCapex(9999));
    }
}
