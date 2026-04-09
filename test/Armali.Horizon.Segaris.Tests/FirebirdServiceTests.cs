using Armali.Horizon.Segaris.Model;
using Armali.Horizon.Segaris.Services;

namespace Armali.Horizon.Segaris.Tests;

[TestClass]
public class FirebirdServiceTests
{
    private TestDbContextFactory _factory = null!;
    private FirebirdService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new TestDbContextFactory();
        _service = new FirebirdService(_factory);
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    // ── GetFirebirdCategories ────────────────────────────────

    [TestMethod]
    public async Task GetFirebirdCategories_ReturnsSeedData()
    {
        var categories = await _service.GetFirebirdCategories();

        categories.ShouldNotBeEmpty();
        categories.Count.ShouldBe(6);
        categories.ShouldContain(c => c.Name == "Cat A");
        categories.ShouldContain(c => c.Name == "Cat F");
    }

    // ── GetFirebirdStatuses ──────────────────────────────────

    [TestMethod]
    public async Task GetFirebirdStatuses_ReturnsSeedData()
    {
        var statuses = await _service.GetFirebirdStatuses();

        statuses.Count.ShouldBe(4);
        statuses.ShouldContain(s => s.Name == "Unknown" && s.Color == "blue");
        statuses.ShouldContain(s => s.Name == "Available" && s.Color == "green");
        statuses.ShouldContain(s => s.Name == "Unavailable" && s.Color == "red");
        statuses.ShouldContain(s => s.Name == "Blocked" && s.Color == "gray");
    }

    // ── AddFirebird + GetFirebirdEntities ────────────────────

    [TestMethod]
    public async Task AddFirebird_AndRetrieve_ReturnsEntity()
    {
        var entity = new FirebirdEntity
        {
            Name = "Test Firebird",
            CategoryId = 1,
            StatusId = 1,
            Location = "Madrid",
            Birthday = new DateTime(2000, 5, 15),
            IsAware = true,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddFirebird(entity);

        var all = await _service.GetFirebirdEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Test Firebird");
        all[0].Location.ShouldBe("Madrid");
        all[0].IsAware.ShouldBeTrue();
    }

    // ── Privacy filtering ────────────────────────────────────

    [TestMethod]
    public async Task GetFirebirdEntities_FiltersPrivateEntitiesFromOthers()
    {
        // Entidad pública de user1 — visible para todos
        await _service.AddFirebird(new FirebirdEntity
        {
            Name = "Publico",
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        });

        // Entidad privada de user1 — solo visible para user1
        await _service.AddFirebird(new FirebirdEntity
        {
            Name = "Privado User1",
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = true,
            Creator = "user1"
        });

        // user1 ve ambas
        var user1Results = await _service.GetFirebirdEntities("user1");
        user1Results.Count.ShouldBe(2);

        // user2 solo ve la pública
        var user2Results = await _service.GetFirebirdEntities("user2");
        user2Results.Count.ShouldBe(1);
        user2Results[0].Name.ShouldBe("Publico");
    }

    // ── Ordering ─────────────────────────────────────────────

    [TestMethod]
    public async Task GetFirebirdEntities_OrdersByCategoryThenName()
    {
        await _service.AddFirebird(new FirebirdEntity
        {
            Name = "Zeta",
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        });

        await _service.AddFirebird(new FirebirdEntity
        {
            Name = "Alpha",
            CategoryId = 2,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        });

        await _service.AddFirebird(new FirebirdEntity
        {
            Name = "Alpha",
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        });

        var all = await _service.GetFirebirdEntities("user1");
        all.Count.ShouldBe(3);
        // Categoría 1 primero, ordenados por nombre
        all[0].Name.ShouldBe("Alpha");
        all[0].CategoryId.ShouldBe(1);
        all[1].Name.ShouldBe("Zeta");
        all[1].CategoryId.ShouldBe(1);
        // Categoría 2 después
        all[2].Name.ShouldBe("Alpha");
        all[2].CategoryId.ShouldBe(2);
    }

    // ── UpdateFirebird ───────────────────────────────────────

    [TestMethod]
    public async Task UpdateFirebird_ModifiesEntity()
    {
        var entity = new FirebirdEntity
        {
            Name = "Original",
            CategoryId = 1,
            StatusId = 1,
            Location = "Barcelona",
            Birthday = new DateTime(1995, 1, 1),
            IsAware = false,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddFirebird(entity);

        // Actualizar
        entity.Name = "Modificado";
        entity.CategoryId = 2;
        entity.Location = "Lisboa";
        entity.IsAware = true;
        await _service.UpdateFirebird(entity);

        var all = await _service.GetFirebirdEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Modificado");
        all[0].CategoryId.ShouldBe(2);
        all[0].Location.ShouldBe("Lisboa");
        all[0].IsAware.ShouldBeTrue();
    }

    // ── DeleteFirebird ───────────────────────────────────────

    [TestMethod]
    public async Task DeleteFirebird_RemovesEntity()
    {
        var entity = new FirebirdEntity
        {
            Name = "Para eliminar",
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddFirebird(entity);
        var all = await _service.GetFirebirdEntities("user1");
        all.Count.ShouldBe(1);

        await _service.DeleteFirebird(entity.Id);

        var afterDelete = await _service.GetFirebirdEntities("user1");
        afterDelete.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteFirebird_NonExistentId_DoesNotThrow()
    {
        // No debe lanzar excepción al intentar eliminar un ID que no existe
        await Should.NotThrowAsync(() => _service.DeleteFirebird(9999));
    }

    // ── FirebirdSubEntity CRUD ───────────────────────────────

    /// <summary>
    /// Helper que crea una FirebirdEntity padre y la devuelve con su Id asignado.
    /// </summary>
    private async Task<FirebirdEntity> CreateParentEntity(string name = "Registro Test")
    {
        var entity = new FirebirdEntity
        {
            Name = name,
            CategoryId = 1,
            StatusId = 2,
            Location = "Test Location",
            Birthday = new DateTime(2000, 1, 1),
            IsAware = false,
            IsPrivate = false,
            Creator = "user1"
        };
        await _service.AddFirebird(entity);
        return entity;
    }

    [TestMethod]
    public async Task AddFirebirdSubEntity_AndRetrieve_ReturnsSubEntity()
    {
        var parent = await CreateParentEntity();

        var sub = new FirebirdSubEntity
        {
            Date = new DateTime(2026, 3, 1),
            Description = "Primer evento",
            FirebirdId = parent.Id
        };

        await _service.AddFirebirdSubEntity(sub);

        var subs = await _service.GetFirebirdSubEntities(parent);
        subs.Count.ShouldBe(1);
        subs[0].Description.ShouldBe("Primer evento");
        subs[0].FirebirdId.ShouldBe(parent.Id);
    }

    [TestMethod]
    public async Task GetFirebirdSubEntities_FiltersOnlyByParent()
    {
        var parent1 = await CreateParentEntity("Registro A");
        var parent2 = await CreateParentEntity("Registro B");

        await _service.AddFirebirdSubEntity(new FirebirdSubEntity
        {
            Date = DateTime.Now,
            Description = "Sub de A",
            FirebirdId = parent1.Id
        });

        await _service.AddFirebirdSubEntity(new FirebirdSubEntity
        {
            Date = DateTime.Now,
            Description = "Sub de B",
            FirebirdId = parent2.Id
        });

        var subsParent1 = await _service.GetFirebirdSubEntities(parent1);
        subsParent1.Count.ShouldBe(1);
        subsParent1[0].Description.ShouldBe("Sub de A");

        var subsParent2 = await _service.GetFirebirdSubEntities(parent2);
        subsParent2.Count.ShouldBe(1);
        subsParent2[0].Description.ShouldBe("Sub de B");
    }

    [TestMethod]
    public async Task UpdateFirebirdSubEntity_ModifiesSubEntity()
    {
        var parent = await CreateParentEntity();

        var sub = new FirebirdSubEntity
        {
            Date = new DateTime(2026, 1, 1),
            Description = "Original",
            FirebirdId = parent.Id
        };
        await _service.AddFirebirdSubEntity(sub);

        // Actualizar
        sub.Description = "Modificado";
        sub.Date = new DateTime(2026, 6, 15);
        await _service.UpdateFirebirdSubEntity(sub);

        var subs = await _service.GetFirebirdSubEntities(parent);
        subs.Count.ShouldBe(1);
        subs[0].Description.ShouldBe("Modificado");
        subs[0].Date.ShouldBe(new DateTime(2026, 6, 15));
    }

    [TestMethod]
    public async Task DeleteFirebirdSubEntity_RemovesSubEntity()
    {
        var parent = await CreateParentEntity();

        var sub = new FirebirdSubEntity
        {
            Date = DateTime.Now,
            Description = "Para eliminar",
            FirebirdId = parent.Id
        };
        await _service.AddFirebirdSubEntity(sub);

        var before = await _service.GetFirebirdSubEntities(parent);
        before.Count.ShouldBe(1);

        await _service.DeleteFirebirdSubEntity(sub.Id);

        var after = await _service.GetFirebirdSubEntities(parent);
        after.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteFirebirdSubEntity_NonExistentId_DoesNotThrow()
    {
        await Should.NotThrowAsync(() => _service.DeleteFirebirdSubEntity(9999));
    }
}

