using Armali.Horizon.Segaris.Model;
using Armali.Horizon.Segaris.Services;

namespace Armali.Horizon.Segaris.Tests;

[TestClass]
public class ClothesServiceTests
{
    private TestDbContextFactory _factory = null!;
    private ClothesService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new TestDbContextFactory();
        _service = new ClothesService(_factory);
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    // ── GetClothesCategories ─────────────────────────────────

    [TestMethod]
    public async Task GetClothesCategories_ReturnsSeedData()
    {
        var categories = await _service.GetClothesCategories();

        categories.ShouldNotBeEmpty();
        categories.Count.ShouldBe(11);
        categories.ShouldContain(c => c.Name == "Short T-Shirt");
    }

    // ── GetClothesStatuses ───────────────────────────────────

    [TestMethod]
    public async Task GetClothesStatuses_ReturnsSeedData()
    {
        var statuses = await _service.GetClothesStatuses();

        statuses.Count.ShouldBe(3);
        statuses.ShouldContain(s => s.Name == "Planning" && s.Color == "blue");
        statuses.ShouldContain(s => s.Name == "Active" && s.Color == "green");
        statuses.ShouldContain(s => s.Name == "Retired" && s.Color == "red");
    }

    // ── GetClothesWashTypes ──────────────────────────────────

    [TestMethod]
    public async Task GetClothesWashTypes_ReturnsSeedData()
    {
        var washTypes = await _service.GetClothesWashTypes();

        washTypes.Count.ShouldBe(4);
        washTypes.ShouldContain(w => w.Name == "White Wash");
        washTypes.ShouldContain(w => w.Name == "Color Wash");
        washTypes.ShouldContain(w => w.Name == "Special Wash");
        washTypes.ShouldContain(w => w.Name == "Wash Alone");
    }

    // ── GetClothesColors ─────────────────────────────────────

    [TestMethod]
    public async Task GetClothesColors_ReturnsSeedData()
    {
        var colors = await _service.GetClothesColors();

        colors.Count.ShouldBe(20);
        colors.ShouldContain(c => c.Name == "Black" && c.Reference == "#000000");
        colors.ShouldContain(c => c.Name == "White" && c.Reference == "#FFFFFF");
        colors.ShouldContain(c => c.Name == "Navy" && c.Reference == "#000080");
        colors.ShouldContain(c => c.Name == "Teal" && c.Reference == "#008080");
    }

    // ── GetClothesColorStyles ────────────────────────────────

    [TestMethod]
    public async Task GetClothesColorStyles_ReturnsSeedData()
    {
        var styles = await _service.GetClothesColorStyles();

        styles.Count.ShouldBe(3);
        styles.ShouldContain(s => s.Name == "Primary");
        styles.ShouldContain(s => s.Name == "Secondary");
        styles.ShouldContain(s => s.Name == "Details");
    }

    // ── AddClothes + GetClothesEntities ──────────────────────

    [TestMethod]
    public async Task AddClothes_AndRetrieve_ReturnsEntity()
    {
        var entity = new ClothesEntity
        {
            Name = "Test Garment",
            Date = new DateTime(2026, 1, 15),
            GarmentCode = "GRM-001",
            CategoryId = 1,
            StatusId = 1,
            WashTypeId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddClothes(entity);

        var all = await _service.GetClothesEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Test Garment");
        all[0].GarmentCode.ShouldBe("GRM-001");
    }

    // ── Privacy filtering ────────────────────────────────────

    [TestMethod]
    public async Task GetClothesEntities_FiltersPrivateEntitiesFromOthers()
    {
        // Entidad pública de user1 — visible para todos
        await _service.AddClothes(new ClothesEntity
        {
            Name = "Publico",
            Date = DateTime.Now,
            GarmentCode = "PUB-001",
            CategoryId = 1,
            StatusId = 1,
            WashTypeId = 1,
            IsPrivate = false,
            Creator = "user1"
        });

        // Entidad privada de user1 — solo visible para user1
        await _service.AddClothes(new ClothesEntity
        {
            Name = "Privado User1",
            Date = DateTime.Now,
            GarmentCode = "PRV-001",
            CategoryId = 1,
            StatusId = 1,
            WashTypeId = 2,
            IsPrivate = true,
            Creator = "user1"
        });

        // user1 ve ambas
        var user1Results = await _service.GetClothesEntities("user1");
        user1Results.Count.ShouldBe(2);

        // user2 solo ve la pública
        var user2Results = await _service.GetClothesEntities("user2");
        user2Results.Count.ShouldBe(1);
        user2Results[0].Name.ShouldBe("Publico");
    }

    // ── UpdateClothes ────────────────────────────────────────

    [TestMethod]
    public async Task UpdateClothes_ModifiesEntity()
    {
        var entity = new ClothesEntity
        {
            Name = "Original",
            Date = DateTime.Now,
            GarmentCode = "ORI-001",
            CategoryId = 1,
            StatusId = 1,
            WashTypeId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddClothes(entity);

        // Actualizar
        entity.Name = "Modificado";
        entity.GarmentCode = "MOD-001";
        await _service.UpdateClothes(entity);

        var all = await _service.GetClothesEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Modificado");
        all[0].GarmentCode.ShouldBe("MOD-001");
    }

    // ── DeleteClothes ────────────────────────────────────────

    [TestMethod]
    public async Task DeleteClothes_RemovesEntity()
    {
        var entity = new ClothesEntity
        {
            Name = "Para eliminar",
            Date = DateTime.Now,
            GarmentCode = "DEL-001",
            CategoryId = 1,
            StatusId = 1,
            WashTypeId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddClothes(entity);
        var all = await _service.GetClothesEntities("user1");
        all.Count.ShouldBe(1);

        await _service.DeleteClothes(entity.Id);

        var afterDelete = await _service.GetClothesEntities("user1");
        afterDelete.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteClothes_NonExistentId_DoesNotThrow()
    {
        // No debe lanzar excepción al intentar eliminar un ID que no existe
        await Should.NotThrowAsync(() => _service.DeleteClothes(9999));
    }

    // ── Color assignment CRUD ────────────────────────────────

    [TestMethod]
    public async Task AddColorAssignment_AndRetrieve_ReturnsAssignment()
    {
        // Crear prenda primero
        var garment = new ClothesEntity
        {
            Name = "Garment Colors",
            Date = DateTime.Now,
            CategoryId = 1,
            StatusId = 1,
            WashTypeId = 1,
            Creator = "user1"
        };
        await _service.AddClothes(garment);

        var assignment = new ClothesColorAssignment
        {
            GarmentId = garment.Id,
            ColorId = 1,  // Black
            StyleId = 1   // Primary
        };
        await _service.AddColorAssignment(assignment);

        var assignments = await _service.GetColorAssignments(garment.Id);
        assignments.Count.ShouldBe(1);
        assignments[0].ColorId.ShouldBe(1);
        assignments[0].StyleId.ShouldBe(1);
    }

    [TestMethod]
    public async Task GetColorAssignments_OrdersByStyleId()
    {
        var garment = new ClothesEntity
        {
            Name = "Multi Color",
            Date = DateTime.Now,
            CategoryId = 1,
            StatusId = 1,
            WashTypeId = 1,
            Creator = "user1"
        };
        await _service.AddClothes(garment);

        // Insertar en orden inverso (Details antes que Primary)
        await _service.AddColorAssignment(new ClothesColorAssignment
        {
            GarmentId = garment.Id, ColorId = 9, StyleId = 3  // Details
        });
        await _service.AddColorAssignment(new ClothesColorAssignment
        {
            GarmentId = garment.Id, ColorId = 1, StyleId = 1  // Primary
        });
        await _service.AddColorAssignment(new ClothesColorAssignment
        {
            GarmentId = garment.Id, ColorId = 2, StyleId = 2  // Secondary
        });

        var assignments = await _service.GetColorAssignments(garment.Id);
        assignments.Count.ShouldBe(3);
        assignments[0].StyleId.ShouldBe(1);
        assignments[1].StyleId.ShouldBe(2);
        assignments[2].StyleId.ShouldBe(3);
    }

    [TestMethod]
    public async Task GetAllColorAssignments_GroupsByGarmentId()
    {
        var garment1 = new ClothesEntity
        {
            Name = "Garment A", Date = DateTime.Now,
            CategoryId = 1, StatusId = 1, WashTypeId = 1, Creator = "user1"
        };
        var garment2 = new ClothesEntity
        {
            Name = "Garment B", Date = DateTime.Now,
            CategoryId = 1, StatusId = 1, WashTypeId = 1, Creator = "user1"
        };
        await _service.AddClothes(garment1);
        await _service.AddClothes(garment2);

        await _service.AddColorAssignment(new ClothesColorAssignment
        {
            GarmentId = garment1.Id, ColorId = 1, StyleId = 1
        });
        await _service.AddColorAssignment(new ClothesColorAssignment
        {
            GarmentId = garment1.Id, ColorId = 2, StyleId = 2
        });
        await _service.AddColorAssignment(new ClothesColorAssignment
        {
            GarmentId = garment2.Id, ColorId = 3, StyleId = 1
        });

        var dict = await _service.GetAllColorAssignments();
        dict.Keys.Count.ShouldBe(2);
        dict[garment1.Id].Count.ShouldBe(2);
        dict[garment2.Id].Count.ShouldBe(1);
    }

    [TestMethod]
    public async Task UpdateColorAssignment_ModifiesAssignment()
    {
        var garment = new ClothesEntity
        {
            Name = "Update Color", Date = DateTime.Now,
            CategoryId = 1, StatusId = 1, WashTypeId = 1, Creator = "user1"
        };
        await _service.AddClothes(garment);

        var assignment = new ClothesColorAssignment
        {
            GarmentId = garment.Id, ColorId = 1, StyleId = 1
        };
        await _service.AddColorAssignment(assignment);

        assignment.ColorId = 7;  // Cambiar de Black a Blue
        assignment.StyleId = 2;  // Cambiar de Primary a Secondary
        await _service.UpdateColorAssignment(assignment);

        var assignments = await _service.GetColorAssignments(garment.Id);
        assignments.Count.ShouldBe(1);
        assignments[0].ColorId.ShouldBe(7);
        assignments[0].StyleId.ShouldBe(2);
    }

    [TestMethod]
    public async Task DeleteColorAssignment_RemovesAssignment()
    {
        var garment = new ClothesEntity
        {
            Name = "Delete Color", Date = DateTime.Now,
            CategoryId = 1, StatusId = 1, WashTypeId = 1, Creator = "user1"
        };
        await _service.AddClothes(garment);

        var assignment = new ClothesColorAssignment
        {
            GarmentId = garment.Id, ColorId = 1, StyleId = 1
        };
        await _service.AddColorAssignment(assignment);

        await _service.DeleteColorAssignment(assignment.Id);

        var assignments = await _service.GetColorAssignments(garment.Id);
        assignments.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteColorAssignment_NonExistentId_DoesNotThrow()
    {
        await Should.NotThrowAsync(() => _service.DeleteColorAssignment(9999));
    }
}
