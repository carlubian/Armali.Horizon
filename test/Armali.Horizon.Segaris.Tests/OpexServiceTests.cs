using Armali.Horizon.Segaris.Model;
using Armali.Horizon.Segaris.Services;

namespace Armali.Horizon.Segaris.Tests;

[TestClass]
public class OpexServiceTests
{
    private TestDbContextFactory _factory = null!;
    private OpexService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new TestDbContextFactory();
        _service = new OpexService(_factory);
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    // ── GetOpexCategories ────────────────────────────────────

    [TestMethod]
    public async Task GetOpexCategories_ReturnsSeedData()
    {
        // Las categorías se crean como seed en SegarisDbContext.OnModelCreating
        var categories = await _service.GetOpexCategories();

        categories.ShouldNotBeEmpty();
        categories.Count.ShouldBe(7);
        categories.ShouldContain(c => c.Name == "Government");
        categories.ShouldContain(c => c.Name == "Software");
    }

    // ── GetOpexStatuses ──────────────────────────────────────

    [TestMethod]
    public async Task GetOpexStatuses_ReturnsSeedData()
    {
        var statuses = await _service.GetOpexStatuses();

        statuses.Count.ShouldBe(4);
        statuses.ShouldContain(s => s.Name == "Planning" && s.Color == "blue");
        statuses.ShouldContain(s => s.Name == "Active" && s.Color == "green");
        statuses.ShouldContain(s => s.Name == "Paused" && s.Color == "gold");
        statuses.ShouldContain(s => s.Name == "Closed" && s.Color == "red");
    }

    // ── AddOpex + GetOpexEntities ────────────────────────────

    [TestMethod]
    public async Task AddOpex_AndRetrieve_ReturnsEntity()
    {
        var entity = new OpexEntity
        {
            Name = "Test Opex",
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddOpex(entity);

        var all = await _service.GetOpexEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Test Opex");
    }

    // ── Privacy filtering ────────────────────────────────────

    [TestMethod]
    public async Task GetOpexEntities_FiltersPrivateEntitiesFromOthers()
    {
        // Entidad pública de user1 — visible para todos
        await _service.AddOpex(new OpexEntity
        {
            Name = "Publico",
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        });

        // Entidad privada de user1 — solo visible para user1
        await _service.AddOpex(new OpexEntity
        {
            Name = "Privado User1",
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = true,
            Creator = "user1"
        });

        // user1 ve ambas
        var user1Results = await _service.GetOpexEntities("user1");
        user1Results.Count.ShouldBe(2);

        // user2 solo ve la pública
        var user2Results = await _service.GetOpexEntities("user2");
        user2Results.Count.ShouldBe(1);
        user2Results[0].Name.ShouldBe("Publico");
    }

    // ── UpdateOpex ───────────────────────────────────────────

    [TestMethod]
    public async Task UpdateOpex_ModifiesEntity()
    {
        var entity = new OpexEntity
        {
            Name = "Original",
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddOpex(entity);

        // Actualizar
        entity.Name = "Modificado";
        entity.CategoryId = 2;
        await _service.UpdateOpex(entity);

        var all = await _service.GetOpexEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Modificado");
        all[0].CategoryId.ShouldBe(2);
    }

    // ── DeleteOpex ───────────────────────────────────────────

    [TestMethod]
    public async Task DeleteOpex_RemovesEntity()
    {
        var entity = new OpexEntity
        {
            Name = "Para eliminar",
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddOpex(entity);
        var all = await _service.GetOpexEntities("user1");
        all.Count.ShouldBe(1);

        await _service.DeleteOpex(entity.Id);

        var afterDelete = await _service.GetOpexEntities("user1");
        afterDelete.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteOpex_NonExistentId_DoesNotThrow()
    {
        // No debe lanzar excepción al intentar eliminar un ID que no existe
        await Should.NotThrowAsync(() => _service.DeleteOpex(9999));
    }

    // ── OpexSubEntity CRUD ───────────────────────────────────

    /// <summary>
    /// Helper que crea una OpexEntity padre y la devuelve con su Id asignado.
    /// </summary>
    private async Task<OpexEntity> CreateParentEntity(string name = "Contrato Test")
    {
        var entity = new OpexEntity
        {
            Name = name,
            CategoryId = 1,
            StatusId = 2,
            IsPrivate = false,
            Creator = "user1"
        };
        await _service.AddOpex(entity);
        return entity;
    }

    [TestMethod]
    public async Task AddOpexSubEntity_AndRetrieve_ReturnsSubEntity()
    {
        var parent = await CreateParentEntity();

        var sub = new OpexSubEntity
        {
            Date = new DateTime(2026, 3, 1),
            Amount = 150.50,
            ContractId = parent.Id
        };

        await _service.AddOpexSubEntity(sub);

        var subs = await _service.GetOpexSubEntities(parent);
        subs.Count.ShouldBe(1);
        subs[0].Amount.ShouldBe(150.50);
        subs[0].ContractId.ShouldBe(parent.Id);
    }

    [TestMethod]
    public async Task GetOpexSubEntities_OrdersByDateDescending()
    {
        var parent = await CreateParentEntity();

        await _service.AddOpexSubEntity(new OpexSubEntity
        {
            Date = new DateTime(2026, 1, 1),
            Amount = 100,
            ContractId = parent.Id
        });

        await _service.AddOpexSubEntity(new OpexSubEntity
        {
            Date = new DateTime(2026, 6, 1),
            Amount = 200,
            ContractId = parent.Id
        });

        await _service.AddOpexSubEntity(new OpexSubEntity
        {
            Date = new DateTime(2026, 3, 1),
            Amount = 300,
            ContractId = parent.Id
        });

        var subs = await _service.GetOpexSubEntities(parent);
        subs.Count.ShouldBe(3);
        // Orden descendente por fecha
        subs[0].Date.ShouldBe(new DateTime(2026, 6, 1));
        subs[1].Date.ShouldBe(new DateTime(2026, 3, 1));
        subs[2].Date.ShouldBe(new DateTime(2026, 1, 1));
    }

    [TestMethod]
    public async Task GetOpexSubEntities_FiltersOnlyByParentContract()
    {
        var parent1 = await CreateParentEntity("Contrato A");
        var parent2 = await CreateParentEntity("Contrato B");

        await _service.AddOpexSubEntity(new OpexSubEntity
        {
            Date = DateTime.Now,
            Amount = 100,
            ContractId = parent1.Id
        });

        await _service.AddOpexSubEntity(new OpexSubEntity
        {
            Date = DateTime.Now,
            Amount = 200,
            ContractId = parent2.Id
        });

        var subsParent1 = await _service.GetOpexSubEntities(parent1);
        subsParent1.Count.ShouldBe(1);
        subsParent1[0].Amount.ShouldBe(100);

        var subsParent2 = await _service.GetOpexSubEntities(parent2);
        subsParent2.Count.ShouldBe(1);
        subsParent2[0].Amount.ShouldBe(200);
    }

    [TestMethod]
    public async Task UpdateOpexSubEntity_ModifiesSubEntity()
    {
        var parent = await CreateParentEntity();

        var sub = new OpexSubEntity
        {
            Date = new DateTime(2026, 1, 1),
            Amount = 100,
            ContractId = parent.Id
        };
        await _service.AddOpexSubEntity(sub);

        // Actualizar
        sub.Amount = 999.99;
        sub.Date = new DateTime(2026, 6, 15);
        await _service.UpdateOpexSubEntity(sub);

        var subs = await _service.GetOpexSubEntities(parent);
        subs.Count.ShouldBe(1);
        subs[0].Amount.ShouldBe(999.99);
        subs[0].Date.ShouldBe(new DateTime(2026, 6, 15));
    }

    [TestMethod]
    public async Task DeleteOpexSubEntity_RemovesSubEntity()
    {
        var parent = await CreateParentEntity();

        var sub = new OpexSubEntity
        {
            Date = DateTime.Now,
            Amount = 50,
            ContractId = parent.Id
        };
        await _service.AddOpexSubEntity(sub);

        var before = await _service.GetOpexSubEntities(parent);
        before.Count.ShouldBe(1);

        await _service.DeleteOpexSubEntity(sub.Id);

        var after = await _service.GetOpexSubEntities(parent);
        after.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteOpexSubEntity_NonExistentId_DoesNotThrow()
    {
        await Should.NotThrowAsync(() => _service.DeleteOpexSubEntity(9999));
    }

    // ── GetOpexStats ─────────────────────────────────────────

    [TestMethod]
    public async Task GetOpexStats_ReturnsCorrectCountAndTotal()
    {
        var parent = await CreateParentEntity();

        await _service.AddOpexSubEntity(new OpexSubEntity
        {
            Date = DateTime.Now,
            Amount = 100.555,
            ContractId = parent.Id
        });

        await _service.AddOpexSubEntity(new OpexSubEntity
        {
            Date = DateTime.Now,
            Amount = 200.445,
            ContractId = parent.Id
        });

        var stats = await _service.GetOpexStats(parent.Id);
        stats.SubEntityCount.ShouldBe(2);
        // 100.555 + 200.445 = 301.00 — truncado a 2 decimales
        stats.TotalAmount.ShouldBe(301.00);
    }

    [TestMethod]
    public async Task GetOpexStats_WithNoSubEntities_ReturnsZeros()
    {
        var parent = await CreateParentEntity();

        var stats = await _service.GetOpexStats(parent.Id);
        stats.SubEntityCount.ShouldBe(0);
        stats.TotalAmount.ShouldBe(0);
    }

    [TestMethod]
    public async Task GetOpexStats_OnlyCountsSubEntitiesOfGivenContract()
    {
        var parent1 = await CreateParentEntity("Contrato A");
        var parent2 = await CreateParentEntity("Contrato B");

        await _service.AddOpexSubEntity(new OpexSubEntity
        {
            Date = DateTime.Now,
            Amount = 50,
            ContractId = parent1.Id
        });

        await _service.AddOpexSubEntity(new OpexSubEntity
        {
            Date = DateTime.Now,
            Amount = 75,
            ContractId = parent1.Id
        });

        await _service.AddOpexSubEntity(new OpexSubEntity
        {
            Date = DateTime.Now,
            Amount = 300,
            ContractId = parent2.Id
        });

        var stats1 = await _service.GetOpexStats(parent1.Id);
        stats1.SubEntityCount.ShouldBe(2);
        stats1.TotalAmount.ShouldBe(125.00);

        var stats2 = await _service.GetOpexStats(parent2.Id);
        stats2.SubEntityCount.ShouldBe(1);
        stats2.TotalAmount.ShouldBe(300.00);
    }

    [TestMethod]
    public async Task GetOpexStats_RoundsTotalToTwoDecimals()
    {
        var parent = await CreateParentEntity();

        await _service.AddOpexSubEntity(new OpexSubEntity
        {
            Date = DateTime.Now,
            Amount = 33.336,
            ContractId = parent.Id
        });

        await _service.AddOpexSubEntity(new OpexSubEntity
        {
            Date = DateTime.Now,
            Amount = 33.336,
            ContractId = parent.Id
        });

        await _service.AddOpexSubEntity(new OpexSubEntity
        {
            Date = DateTime.Now,
            Amount = 33.336,
            ContractId = parent.Id
        });

        var stats = await _service.GetOpexStats(parent.Id);
        stats.SubEntityCount.ShouldBe(3);
        // 33.336 * 3 = 100.008 → redondeado a 100.01
        stats.TotalAmount.ShouldBe(100.01);
    }
}

