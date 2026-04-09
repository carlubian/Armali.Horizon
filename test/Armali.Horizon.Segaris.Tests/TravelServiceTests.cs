using Armali.Horizon.Segaris.Model;
using Armali.Horizon.Segaris.Services;

namespace Armali.Horizon.Segaris.Tests;

[TestClass]
public class TravelServiceTests
{
    private TestDbContextFactory _factory = null!;
    private TravelService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new TestDbContextFactory();
        _service = new TravelService(_factory);
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    // ── GetTravelCategories ──────────────────────────────────

    [TestMethod]
    public async Task GetTravelCategories_ReturnsSeedData()
    {
        var categories = await _service.GetTravelCategories();

        categories.ShouldNotBeEmpty();
        categories.Count.ShouldBe(5);
        categories.ShouldContain(c => c.Name == "Local");
        categories.ShouldContain(c => c.Name == "Schengen");
        categories.ShouldContain(c => c.Name == "Non-Schengen");
    }

    // ── GetTravelStatuses ────────────────────────────────────

    [TestMethod]
    public async Task GetTravelStatuses_ReturnsSeedData()
    {
        var statuses = await _service.GetTravelStatuses();

        statuses.Count.ShouldBe(4);
        statuses.ShouldContain(s => s.Name == "Planning" && s.Color == "blue");
        statuses.ShouldContain(s => s.Name == "Active" && s.Color == "gold");
        statuses.ShouldContain(s => s.Name == "Completed" && s.Color == "green");
        statuses.ShouldContain(s => s.Name == "Canceled" && s.Color == "red");
    }

    // ── GetTravelCostCenters ─────────────────────────────────

    [TestMethod]
    public async Task GetTravelCostCenters_ReturnsSeedData()
    {
        var costCenters = await _service.GetTravelCostCenters();

        costCenters.Count.ShouldBe(4);
        costCenters.ShouldContain(c => c.Name == "Armali");
        costCenters.ShouldContain(c => c.Name == "Common Fund");
        costCenters.ShouldContain(c => c.Name == "AMI3");
        costCenters.ShouldContain(c => c.Name == "Other CC");
    }

    // ── GetTravelSubEntityCategories ─────────────────────────

    [TestMethod]
    public async Task GetTravelSubEntityCategories_ReturnsSeedData()
    {
        var subCategories = await _service.GetTravelSubEntityCategories();

        subCategories.Count.ShouldBe(13);
        subCategories.ShouldContain(c => c.Name == "Hotels");
        subCategories.ShouldContain(c => c.Name == "Airplane");
        subCategories.ShouldContain(c => c.Name == "Food and Drinks");
        subCategories.ShouldContain(c => c.Name == "Other Expenses");
    }

    // ── AddTravel + GetTravelEntities ────────────────────────

    [TestMethod]
    public async Task AddTravel_AndRetrieve_ReturnsEntity()
    {
        var entity = new TravelEntity
        {
            Name = "Test Travel",
            CategoryId = 1,
            StatusId = 1,
            CostCenterId = 1,
            Destination = "París",
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 6, 10),
            Pax = 2,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddTravel(entity);

        var all = await _service.GetTravelEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Test Travel");
        all[0].Destination.ShouldBe("París");
        all[0].Pax.ShouldBe(2);
    }

    // ── Privacy filtering ────────────────────────────────────

    [TestMethod]
    public async Task GetTravelEntities_FiltersPrivateEntitiesFromOthers()
    {
        // Entidad pública de user1 — visible para todos
        await _service.AddTravel(new TravelEntity
        {
            Name = "Publico",
            CategoryId = 1,
            StatusId = 1,
            CostCenterId = 1,
            Destination = "Madrid",
            StartDate = new DateTime(2026, 5, 1),
            EndDate = new DateTime(2026, 5, 5),
            Pax = 1,
            IsPrivate = false,
            Creator = "user1"
        });

        // Entidad privada de user1 — solo visible para user1
        await _service.AddTravel(new TravelEntity
        {
            Name = "Privado User1",
            CategoryId = 1,
            StatusId = 1,
            CostCenterId = 1,
            Destination = "Berlín",
            StartDate = new DateTime(2026, 7, 1),
            EndDate = new DateTime(2026, 7, 5),
            Pax = 1,
            IsPrivate = true,
            Creator = "user1"
        });

        // user1 ve ambas
        var user1Results = await _service.GetTravelEntities("user1");
        user1Results.Count.ShouldBe(2);

        // user2 solo ve la pública
        var user2Results = await _service.GetTravelEntities("user2");
        user2Results.Count.ShouldBe(1);
        user2Results[0].Name.ShouldBe("Publico");
    }

    // ── Ordering ─────────────────────────────────────────────

    [TestMethod]
    public async Task GetTravelEntities_OrdersByStartDateDescending()
    {
        await _service.AddTravel(new TravelEntity
        {
            Name = "Viaje Enero",
            CategoryId = 1,
            StatusId = 1,
            CostCenterId = 1,
            StartDate = new DateTime(2026, 1, 1),
            EndDate = new DateTime(2026, 1, 5),
            IsPrivate = false,
            Creator = "user1"
        });

        await _service.AddTravel(new TravelEntity
        {
            Name = "Viaje Junio",
            CategoryId = 1,
            StatusId = 1,
            CostCenterId = 1,
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 6, 10),
            IsPrivate = false,
            Creator = "user1"
        });

        await _service.AddTravel(new TravelEntity
        {
            Name = "Viaje Marzo",
            CategoryId = 1,
            StatusId = 1,
            CostCenterId = 1,
            StartDate = new DateTime(2026, 3, 15),
            EndDate = new DateTime(2026, 3, 20),
            IsPrivate = false,
            Creator = "user1"
        });

        var all = await _service.GetTravelEntities("user1");
        all.Count.ShouldBe(3);
        // Orden descendente por StartDate
        all[0].Name.ShouldBe("Viaje Junio");
        all[1].Name.ShouldBe("Viaje Marzo");
        all[2].Name.ShouldBe("Viaje Enero");
    }

    // ── UpdateTravel ─────────────────────────────────────────

    [TestMethod]
    public async Task UpdateTravel_ModifiesEntity()
    {
        var entity = new TravelEntity
        {
            Name = "Original",
            CategoryId = 1,
            StatusId = 1,
            CostCenterId = 1,
            Destination = "Roma",
            StartDate = new DateTime(2026, 4, 1),
            EndDate = new DateTime(2026, 4, 5),
            Pax = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddTravel(entity);

        // Actualizar
        entity.Name = "Modificado";
        entity.Destination = "Tokio";
        entity.Pax = 3;
        entity.CategoryId = 2;
        await _service.UpdateTravel(entity);

        var all = await _service.GetTravelEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Modificado");
        all[0].Destination.ShouldBe("Tokio");
        all[0].Pax.ShouldBe(3);
        all[0].CategoryId.ShouldBe(2);
    }

    // ── DeleteTravel ─────────────────────────────────────────

    [TestMethod]
    public async Task DeleteTravel_RemovesEntity()
    {
        var entity = new TravelEntity
        {
            Name = "Para eliminar",
            CategoryId = 1,
            StatusId = 1,
            CostCenterId = 1,
            StartDate = new DateTime(2026, 5, 1),
            EndDate = new DateTime(2026, 5, 5),
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddTravel(entity);
        var all = await _service.GetTravelEntities("user1");
        all.Count.ShouldBe(1);

        await _service.DeleteTravel(entity.Id);

        var afterDelete = await _service.GetTravelEntities("user1");
        afterDelete.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteTravel_NonExistentId_DoesNotThrow()
    {
        // No debe lanzar excepción al intentar eliminar un ID que no existe
        await Should.NotThrowAsync(() => _service.DeleteTravel(9999));
    }

    // ── TravelSubEntity CRUD ─────────────────────────────────

    /// <summary>
    /// Helper que crea una TravelEntity padre y la devuelve con su Id asignado.
    /// </summary>
    private async Task<TravelEntity> CreateParentEntity(string name = "Viaje Test")
    {
        var entity = new TravelEntity
        {
            Name = name,
            CategoryId = 1,
            StatusId = 2,
            CostCenterId = 1,
            Destination = "Test Destination",
            StartDate = new DateTime(2026, 6, 1),
            EndDate = new DateTime(2026, 6, 10),
            Pax = 1,
            IsPrivate = false,
            Creator = "user1"
        };
        await _service.AddTravel(entity);
        return entity;
    }

    [TestMethod]
    public async Task AddTravelSubEntity_AndRetrieve_ReturnsSubEntity()
    {
        var parent = await CreateParentEntity();

        var sub = new TravelSubEntity
        {
            Name = "Hotel Reserva",
            CategoryId = 1,
            Date = new DateTime(2026, 6, 1),
            Amount = 250.00,
            TravelId = parent.Id
        };

        await _service.AddTravelSubEntity(sub);

        var subs = await _service.GetTravelSubEntities(parent);
        subs.Count.ShouldBe(1);
        subs[0].Name.ShouldBe("Hotel Reserva");
        subs[0].Amount.ShouldBe(250.00);
        subs[0].TravelId.ShouldBe(parent.Id);
    }

    [TestMethod]
    public async Task GetTravelSubEntities_OrdersByDateDescending()
    {
        var parent = await CreateParentEntity();

        await _service.AddTravelSubEntity(new TravelSubEntity
        {
            Name = "Gasto Enero",
            CategoryId = 1,
            Date = new DateTime(2026, 1, 1),
            Amount = 100,
            TravelId = parent.Id
        });

        await _service.AddTravelSubEntity(new TravelSubEntity
        {
            Name = "Gasto Junio",
            CategoryId = 1,
            Date = new DateTime(2026, 6, 1),
            Amount = 200,
            TravelId = parent.Id
        });

        await _service.AddTravelSubEntity(new TravelSubEntity
        {
            Name = "Gasto Marzo",
            CategoryId = 1,
            Date = new DateTime(2026, 3, 1),
            Amount = 300,
            TravelId = parent.Id
        });

        var subs = await _service.GetTravelSubEntities(parent);
        subs.Count.ShouldBe(3);
        // Orden descendente por fecha
        subs[0].Date.ShouldBe(new DateTime(2026, 6, 1));
        subs[1].Date.ShouldBe(new DateTime(2026, 3, 1));
        subs[2].Date.ShouldBe(new DateTime(2026, 1, 1));
    }

    [TestMethod]
    public async Task GetTravelSubEntities_FiltersOnlyByParentTravel()
    {
        var parent1 = await CreateParentEntity("Viaje A");
        var parent2 = await CreateParentEntity("Viaje B");

        await _service.AddTravelSubEntity(new TravelSubEntity
        {
            Name = "Gasto de A",
            CategoryId = 1,
            Date = DateTime.Now,
            Amount = 100,
            TravelId = parent1.Id
        });

        await _service.AddTravelSubEntity(new TravelSubEntity
        {
            Name = "Gasto de B",
            CategoryId = 2,
            Date = DateTime.Now,
            Amount = 200,
            TravelId = parent2.Id
        });

        var subsParent1 = await _service.GetTravelSubEntities(parent1);
        subsParent1.Count.ShouldBe(1);
        subsParent1[0].Amount.ShouldBe(100);

        var subsParent2 = await _service.GetTravelSubEntities(parent2);
        subsParent2.Count.ShouldBe(1);
        subsParent2[0].Amount.ShouldBe(200);
    }

    [TestMethod]
    public async Task UpdateTravelSubEntity_ModifiesSubEntity()
    {
        var parent = await CreateParentEntity();

        var sub = new TravelSubEntity
        {
            Name = "Original",
            CategoryId = 1,
            Date = new DateTime(2026, 1, 1),
            Amount = 100,
            TravelId = parent.Id
        };
        await _service.AddTravelSubEntity(sub);

        // Actualizar
        sub.Name = "Modificado";
        sub.Amount = 999.99;
        sub.Date = new DateTime(2026, 6, 15);
        sub.CategoryId = 3;
        await _service.UpdateTravelSubEntity(sub);

        var subs = await _service.GetTravelSubEntities(parent);
        subs.Count.ShouldBe(1);
        subs[0].Name.ShouldBe("Modificado");
        subs[0].Amount.ShouldBe(999.99);
        subs[0].Date.ShouldBe(new DateTime(2026, 6, 15));
        subs[0].CategoryId.ShouldBe(3);
    }

    [TestMethod]
    public async Task DeleteTravelSubEntity_RemovesSubEntity()
    {
        var parent = await CreateParentEntity();

        var sub = new TravelSubEntity
        {
            Name = "Para eliminar",
            CategoryId = 1,
            Date = DateTime.Now,
            Amount = 50,
            TravelId = parent.Id
        };
        await _service.AddTravelSubEntity(sub);

        var before = await _service.GetTravelSubEntities(parent);
        before.Count.ShouldBe(1);

        await _service.DeleteTravelSubEntity(sub.Id);

        var after = await _service.GetTravelSubEntities(parent);
        after.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteTravelSubEntity_NonExistentId_DoesNotThrow()
    {
        await Should.NotThrowAsync(() => _service.DeleteTravelSubEntity(9999));
    }

    // ── ProjectId opcional ───────────────────────────────────

    [TestMethod]
    public async Task AddTravel_WithProjectId_PersistsValue()
    {
        var entity = new TravelEntity
        {
            Name = "Viaje con proyecto",
            CategoryId = 1,
            StatusId = 1,
            CostCenterId = 2,
            Destination = "Múnich",
            StartDate = new DateTime(2026, 9, 1),
            EndDate = new DateTime(2026, 9, 5),
            Pax = 1,
            ProjectId = 42,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddTravel(entity);

        var all = await _service.GetTravelEntities("user1");
        all.Count.ShouldBe(1);
        all[0].ProjectId.ShouldBe(42);
    }

    [TestMethod]
    public async Task AddTravel_WithoutProjectId_DefaultsToNull()
    {
        var entity = new TravelEntity
        {
            Name = "Viaje sin proyecto",
            CategoryId = 1,
            StatusId = 1,
            CostCenterId = 1,
            Destination = "Lisboa",
            StartDate = new DateTime(2026, 8, 1),
            EndDate = new DateTime(2026, 8, 3),
            Pax = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddTravel(entity);

        var all = await _service.GetTravelEntities("user1");
        all.Count.ShouldBe(1);
        all[0].ProjectId.ShouldBeNull();
    }
}

