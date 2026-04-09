using Armali.Horizon.Segaris.Model;
using Armali.Horizon.Segaris.Services;

namespace Armali.Horizon.Segaris.Tests;

[TestClass]
public class AssetServiceTests
{
    private TestDbContextFactory _factory = null!;
    private AssetService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new TestDbContextFactory();
        _service = new AssetService(_factory);
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    // ── GetAssetCategories ───────────────────────────────────

    [TestMethod]
    public async Task GetAssetCategories_ReturnsSeedData()
    {
        var categories = await _service.GetAssetCategories();

        categories.ShouldNotBeEmpty();
        categories.Count.ShouldBe(7);
        categories.ShouldContain(c => c.Name == "Computers");
    }

    // ── GetAssetStatuses ─────────────────────────────────────

    [TestMethod]
    public async Task GetAssetStatuses_ReturnsSeedData()
    {
        var statuses = await _service.GetAssetStatuses();

        statuses.Count.ShouldBe(4);
        statuses.ShouldContain(s => s.Name == "Planning" && s.Color == "blue");
        statuses.ShouldContain(s => s.Name == "Active" && s.Color == "green");
        statuses.ShouldContain(s => s.Name == "Unavailable" && s.Color == "gold");
        statuses.ShouldContain(s => s.Name == "Retired" && s.Color == "red");
    }

    // ── AddAsset + GetAssetEntities ──────────────────────────

    [TestMethod]
    public async Task AddAsset_AndRetrieve_ReturnsEntity()
    {
        var entity = new AssetEntity
        {
            Name = "Test Asset",
            AssetCode = "AST-001",
            Location = "Office A",
            Date = new DateTime(2026, 1, 15),
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddAsset(entity);

        var all = await _service.GetAssetEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Test Asset");
        all[0].AssetCode.ShouldBe("AST-001");
    }

    // ── Privacy filtering ────────────────────────────────────

    [TestMethod]
    public async Task GetAssetEntities_FiltersPrivateEntitiesFromOthers()
    {
        // Entidad pública de user1 — visible para todos
        await _service.AddAsset(new AssetEntity
        {
            Name = "Publico",
            AssetCode = "PUB-001",
            Location = "Warehouse",
            Date = DateTime.Now,
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        });

        // Entidad privada de user1 — solo visible para user1
        await _service.AddAsset(new AssetEntity
        {
            Name = "Privado User1",
            AssetCode = "PRV-001",
            Location = "Office B",
            Date = DateTime.Now,
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = true,
            Creator = "user1"
        });

        // user1 ve ambas
        var user1Results = await _service.GetAssetEntities("user1");
        user1Results.Count.ShouldBe(2);

        // user2 solo ve la pública
        var user2Results = await _service.GetAssetEntities("user2");
        user2Results.Count.ShouldBe(1);
        user2Results[0].Name.ShouldBe("Publico");
    }

    // ── UpdateAsset ──────────────────────────────────────────

    [TestMethod]
    public async Task UpdateAsset_ModifiesEntity()
    {
        var entity = new AssetEntity
        {
            Name = "Original",
            AssetCode = "ORI-001",
            Location = "Room 1",
            Date = DateTime.Now,
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddAsset(entity);

        // Actualizar
        entity.Name = "Modificado";
        entity.Location = "Room 2";
        await _service.UpdateAsset(entity);

        var all = await _service.GetAssetEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Modificado");
        all[0].Location.ShouldBe("Room 2");
    }

    // ── DeleteAsset ──────────────────────────────────────────

    [TestMethod]
    public async Task DeleteAsset_RemovesEntity()
    {
        var entity = new AssetEntity
        {
            Name = "Para eliminar",
            AssetCode = "DEL-001",
            Location = "Storage",
            Date = DateTime.Now,
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddAsset(entity);
        var all = await _service.GetAssetEntities("user1");
        all.Count.ShouldBe(1);

        await _service.DeleteAsset(entity.Id);

        var afterDelete = await _service.GetAssetEntities("user1");
        afterDelete.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteAsset_NonExistentId_DoesNotThrow()
    {
        // No debe lanzar excepción al intentar eliminar un ID que no existe
        await Should.NotThrowAsync(() => _service.DeleteAsset(9999));
    }
}

