using Armali.Horizon.Segaris.Model;
using Armali.Horizon.Segaris.Services;

namespace Armali.Horizon.Segaris.Tests;

/// <summary>
/// Tests de las operaciones CRUD de base de datos de ProjectService.
/// Las operaciones de archivo (Upload/Download/Delete file) dependen de
/// DatalakeService que requiere credenciales de Azure, por lo que se
/// prueban por separado en tests de integración. Aquí se pasa null
/// como DatalakeService y solo se testean operaciones de DB.
/// </summary>
[TestClass]
public class ProjectServiceTests
{
    private TestDbContextFactory _factory = null!;
    private ProjectService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new TestDbContextFactory();
        // DatalakeService es null — solo se testean operaciones de DB.
        _service = new ProjectService(_factory, null!);
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    // ── GetProjectPrograms ─────────────────────────────────

    [TestMethod]
    public async Task GetProjectPrograms_ReturnsSeedData()
    {
        var programs = await _service.GetProjectPrograms();

        programs.ShouldNotBeEmpty();
        programs.Count.ShouldBe(7);
        programs.ShouldContain(p => p.Name == "DIGI");
        programs.ShouldContain(p => p.Name == "ENTR");
        programs.ShouldContain(p => p.Name == "PLAT");
    }

    // ── GetProjectAxis (filtered by program) ───────────────

    [TestMethod]
    public async Task GetProjectAxis_FiltersByProgramId()
    {
        // ProgramId = 1 (DIGI) tiene 5 ejes: DEVL, MODS, MUSI, TVMV, VGME
        var axesDigi = await _service.GetProjectAxis(1);
        axesDigi.Count.ShouldBe(5);
        axesDigi.ShouldContain(a => a.Name == "DEVL");
        axesDigi.ShouldContain(a => a.Name == "VGME");

        // ProgramId = 4 (HOME) tiene 4 ejes: EQPM, FUNC, FURN, STRU
        var axesHome = await _service.GetProjectAxis(4);
        axesHome.Count.ShouldBe(4);
        axesHome.ShouldContain(a => a.Name == "FURN");
    }

    [TestMethod]
    public async Task GetProjectAxis_NonExistentProgram_ReturnsEmpty()
    {
        var axes = await _service.GetProjectAxis(999);
        axes.ShouldBeEmpty();
    }

    // ── GetAllProjectAxis ──────────────────────────────────

    [TestMethod]
    public async Task GetAllProjectAxis_ReturnsAllSeedData()
    {
        var allAxes = await _service.GetAllProjectAxis();
        allAxes.Count.ShouldBe(30);
    }

    // ── GetProjectStatuses ─────────────────────────────────

    [TestMethod]
    public async Task GetProjectStatuses_ReturnsSeedData()
    {
        var statuses = await _service.GetProjectStatuses();

        statuses.Count.ShouldBe(5);
        statuses.ShouldContain(s => s.Name == "Planning" && s.Color == "blue");
        statuses.ShouldContain(s => s.Name == "Active" && s.Color == "gold");
        statuses.ShouldContain(s => s.Name == "Paused" && s.Color == "gray");
        statuses.ShouldContain(s => s.Name == "Completed" && s.Color == "green");
        statuses.ShouldContain(s => s.Name == "Closed" && s.Color == "red");
    }

    // ── GetProjectSubEntityCategories ──────────────────────

    [TestMethod]
    public async Task GetProjectSubEntityCategories_ReturnsSeedData()
    {
        var categories = await _service.GetProjectSubEntityCategories();

        categories.Count.ShouldBe(6);
        categories.ShouldContain(c => c.Name == "Project Documentation");
        categories.ShouldContain(c => c.Name == "Invoice");
        categories.ShouldContain(c => c.Name == "Project Output");
    }

    // ── AddProject + GetProjectEntities ────────────────────

    [TestMethod]
    public async Task AddProject_AndRetrieve_ReturnsEntity()
    {
        var entity = new ProjectEntity
        {
            Name = "Test Project",
            Code = "000001",
            ProgramId = 1,
            AxisId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddProject(entity);

        var all = await _service.GetProjectEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Test Project");
        all[0].Code.ShouldBe("000001");
        all[0].ProgramId.ShouldBe(1);
        all[0].AxisId.ShouldBe(1);
    }

    // ── Privacy filtering ──────────────────────────────────

    [TestMethod]
    public async Task GetProjectEntities_FiltersPrivateEntitiesFromOthers()
    {
        // Entidad pública de user1 — visible para todos
        await _service.AddProject(new ProjectEntity
        {
            Name = "Publico",
            Code = "000001",
            ProgramId = 1,
            AxisId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        });

        // Entidad privada de user1 — solo visible para user1
        await _service.AddProject(new ProjectEntity
        {
            Name = "Privado User1",
            Code = "000002",
            ProgramId = 1,
            AxisId = 1,
            StatusId = 1,
            IsPrivate = true,
            Creator = "user1"
        });

        // user1 ve ambas
        var user1Results = await _service.GetProjectEntities("user1");
        user1Results.Count.ShouldBe(2);

        // user2 solo ve la pública
        var user2Results = await _service.GetProjectEntities("user2");
        user2Results.Count.ShouldBe(1);
        user2Results[0].Name.ShouldBe("Publico");
    }

    // ── GetProjectEntities includes navigation properties ──

    [TestMethod]
    public async Task GetProjectEntities_IncludesNavigationProperties()
    {
        await _service.AddProject(new ProjectEntity
        {
            Name = "Con navegación",
            Code = "000001",
            ProgramId = 1,  // DIGI
            AxisId = 1,     // DEVL
            StatusId = 2,   // Active
            IsPrivate = false,
            Creator = "user1"
        });

        var all = await _service.GetProjectEntities("user1");
        all.Count.ShouldBe(1);

        // Las propiedades de navegación deben estar cargadas por Include()
        all[0].Program.ShouldNotBeNull();
        all[0].Program!.Name.ShouldBe("DIGI");
        all[0].Axis.ShouldNotBeNull();
        all[0].Axis!.Name.ShouldBe("DEVL");
        all[0].Status.ShouldNotBeNull();
        all[0].Status!.Name.ShouldBe("Active");
    }

    // ── UpdateProject ──────────────────────────────────────

    [TestMethod]
    public async Task UpdateProject_ModifiesEntity()
    {
        var entity = new ProjectEntity
        {
            Name = "Original",
            Code = "000001",
            ProgramId = 1,
            AxisId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddProject(entity);

        // Actualizar
        entity.Name = "Modificado";
        entity.ProgramId = 2;
        entity.AxisId = 6;
        entity.StatusId = 4;
        await _service.UpdateProject(entity);

        var all = await _service.GetProjectEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Modificado");
        all[0].ProgramId.ShouldBe(2);
        all[0].AxisId.ShouldBe(6);
        all[0].StatusId.ShouldBe(4);
    }

    // ── DeleteProject ──────────────────────────────────────

    [TestMethod]
    public async Task DeleteProject_RemovesEntity()
    {
        var entity = new ProjectEntity
        {
            Name = "Para eliminar",
            Code = "000001",
            ProgramId = 1,
            AxisId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddProject(entity);
        var all = await _service.GetProjectEntities("user1");
        all.Count.ShouldBe(1);

        await _service.DeleteProject(entity.Id);

        var afterDelete = await _service.GetProjectEntities("user1");
        afterDelete.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteProject_NonExistentId_DoesNotThrow()
    {
        // No debe lanzar excepción al intentar eliminar un ID que no existe
        await Should.NotThrowAsync(() => _service.DeleteProject(9999));
    }

    // ── GetNextAvailableCode ───────────────────────────────

    [TestMethod]
    public async Task GetNextAvailableCode_EmptyTable_Returns000001()
    {
        var code = await _service.GetNextAvailableCode();
        code.ShouldBe("000001");
    }

    [TestMethod]
    public async Task GetNextAvailableCode_WithExistingEntities_ReturnsNext()
    {
        await _service.AddProject(new ProjectEntity
        {
            Name = "Primero",
            Code = "000005",
            ProgramId = 1,
            AxisId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        });

        await _service.AddProject(new ProjectEntity
        {
            Name = "Segundo",
            Code = "000010",
            ProgramId = 1,
            AxisId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        });

        var code = await _service.GetNextAvailableCode();
        // MAX(Code) = 10, siguiente = 11
        code.ShouldBe("000011");
    }

    // ── ProjectSubEntity CRUD ──────────────────────────────

    /// <summary>
    /// Helper que crea una ProjectEntity padre y la devuelve con su Id asignado.
    /// </summary>
    private async Task<ProjectEntity> CreateParentEntity(string name = "Proyecto Test")
    {
        var entity = new ProjectEntity
        {
            Name = name,
            Code = "000001",
            ProgramId = 1,
            AxisId = 1,
            StatusId = 2,
            IsPrivate = false,
            Creator = "user1"
        };
        await _service.AddProject(entity);
        return entity;
    }

    [TestMethod]
    public async Task AddProjectSubEntity_AndRetrieve_ReturnsSubEntity()
    {
        var parent = await CreateParentEntity();

        var sub = new ProjectSubEntity
        {
            Name = "Documento de riesgos",
            Date = new DateTime(2026, 3, 1),
            CategoryId = 2,
            ProjectId = parent.Id
        };

        await _service.AddProjectSubEntity(sub);

        var subs = await _service.GetProjectSubEntities(parent);
        subs.Count.ShouldBe(1);
        subs[0].Name.ShouldBe("Documento de riesgos");
        subs[0].CategoryId.ShouldBe(2);
        subs[0].ProjectId.ShouldBe(parent.Id);
    }

    [TestMethod]
    public async Task GetProjectSubEntities_OrdersByDateDescending()
    {
        var parent = await CreateParentEntity();

        await _service.AddProjectSubEntity(new ProjectSubEntity
        {
            Name = "Enero",
            Date = new DateTime(2026, 1, 1),
            CategoryId = 1,
            ProjectId = parent.Id
        });

        await _service.AddProjectSubEntity(new ProjectSubEntity
        {
            Name = "Junio",
            Date = new DateTime(2026, 6, 1),
            CategoryId = 1,
            ProjectId = parent.Id
        });

        await _service.AddProjectSubEntity(new ProjectSubEntity
        {
            Name = "Marzo",
            Date = new DateTime(2026, 3, 1),
            CategoryId = 1,
            ProjectId = parent.Id
        });

        var subs = await _service.GetProjectSubEntities(parent);
        subs.Count.ShouldBe(3);
        // Orden descendente por fecha
        subs[0].Date.ShouldBe(new DateTime(2026, 6, 1));
        subs[1].Date.ShouldBe(new DateTime(2026, 3, 1));
        subs[2].Date.ShouldBe(new DateTime(2026, 1, 1));
    }

    [TestMethod]
    public async Task GetProjectSubEntities_FiltersOnlyByParentProject()
    {
        var parent1 = await CreateParentEntity("Proyecto A");

        // Segundo padre con código diferente para evitar colisiones
        var parent2 = new ProjectEntity
        {
            Name = "Proyecto B",
            Code = "000002",
            ProgramId = 2,
            AxisId = 6,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };
        await _service.AddProject(parent2);

        await _service.AddProjectSubEntity(new ProjectSubEntity
        {
            Name = "Doc de A",
            Date = DateTime.Now,
            CategoryId = 1,
            ProjectId = parent1.Id
        });

        await _service.AddProjectSubEntity(new ProjectSubEntity
        {
            Name = "Doc de B",
            Date = DateTime.Now,
            CategoryId = 2,
            ProjectId = parent2.Id
        });

        var subsParent1 = await _service.GetProjectSubEntities(parent1);
        subsParent1.Count.ShouldBe(1);
        subsParent1[0].Name.ShouldBe("Doc de A");

        var subsParent2 = await _service.GetProjectSubEntities(parent2);
        subsParent2.Count.ShouldBe(1);
        subsParent2[0].Name.ShouldBe("Doc de B");
    }

    [TestMethod]
    public async Task UpdateProjectSubEntity_ModifiesSubEntity()
    {
        var parent = await CreateParentEntity();

        var sub = new ProjectSubEntity
        {
            Name = "Original",
            Date = new DateTime(2026, 1, 1),
            CategoryId = 1,
            ProjectId = parent.Id
        };
        await _service.AddProjectSubEntity(sub);

        // Actualizar
        sub.Name = "Modificado";
        sub.Date = new DateTime(2026, 6, 15);
        sub.CategoryId = 3;
        await _service.UpdateProjectSubEntity(sub);

        var subs = await _service.GetProjectSubEntities(parent);
        subs.Count.ShouldBe(1);
        subs[0].Name.ShouldBe("Modificado");
        subs[0].Date.ShouldBe(new DateTime(2026, 6, 15));
        subs[0].CategoryId.ShouldBe(3);
    }

    [TestMethod]
    public async Task DeleteProjectSubEntity_RemovesSubEntity()
    {
        var parent = await CreateParentEntity();

        var sub = new ProjectSubEntity
        {
            Name = "Para eliminar",
            Date = DateTime.Now,
            CategoryId = 1,
            ProjectId = parent.Id
        };
        await _service.AddProjectSubEntity(sub);

        var before = await _service.GetProjectSubEntities(parent);
        before.Count.ShouldBe(1);

        await _service.DeleteProjectSubEntity(sub.Id);

        var after = await _service.GetProjectSubEntities(parent);
        after.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteProjectSubEntity_NonExistentId_DoesNotThrow()
    {
        await Should.NotThrowAsync(() => _service.DeleteProjectSubEntity(9999));
    }

    // ── SubEntity con File (sin invocar Datalake) ──────────

    [TestMethod]
    public async Task AddProjectSubEntity_WithFile_PersistsFileName()
    {
        var parent = await CreateParentEntity();

        var sub = new ProjectSubEntity
        {
            Name = "Contrato firmado",
            Date = new DateTime(2026, 4, 1),
            File = "abc123.pdf",
            CategoryId = 3,
            ProjectId = parent.Id
        };

        await _service.AddProjectSubEntity(sub);

        var subs = await _service.GetProjectSubEntities(parent);
        subs.Count.ShouldBe(1);
        subs[0].File.ShouldBe("abc123.pdf");
    }

    // ── ProjectRiskCategory seed data ───────────────────────

    [TestMethod]
    public async Task GetProjectRiskCategories_ReturnsSeedData()
    {
        var categories = await _service.GetProjectRiskCategories();

        categories.Count.ShouldBe(8);
        categories.ShouldContain(c => c.Name == "Technical");
        categories.ShouldContain(c => c.Name == "Financial");
        categories.ShouldContain(c => c.Name == "Schedule");
        categories.ShouldContain(c => c.Name == "Operational");
    }

    // ── ProjectRiskElement CRUD ─────────────────────────────

    [TestMethod]
    public async Task AddProjectRiskElement_AndRetrieve_ReturnsElement()
    {
        var parent = await CreateParentEntity();

        var element = new ProjectRiskElement
        {
            Name = "Server outage",
            CategoryId = 1,
            Probability = 5,
            Severity = 8,
            Mitigation = 3,
            ProjectId = parent.Id
        };

        await _service.AddProjectRiskElement(element);

        var elements = await _service.GetProjectRiskElements(parent.Id);
        elements.Count.ShouldBe(1);
        elements[0].Name.ShouldBe("Server outage");
        elements[0].CategoryId.ShouldBe(1);
        elements[0].Probability.ShouldBe(5);
        elements[0].Severity.ShouldBe(8);
        elements[0].Mitigation.ShouldBe(3);
        elements[0].ProjectId.ShouldBe(parent.Id);
    }

    [TestMethod]
    public async Task ProjectRiskElement_Score_IsCalculatedCorrectly()
    {
        var element = new ProjectRiskElement
        {
            Probability = 5,
            Severity = 8,
            Mitigation = 3
        };

        element.Score.ShouldBe(120);
    }

    [TestMethod]
    public async Task ProjectRiskElement_Score_BelowThreshold()
    {
        var element = new ProjectRiskElement
        {
            Probability = 2,
            Severity = 3,
            Mitigation = 4
        };

        element.Score.ShouldBe(24);
        (element.Score < 100).ShouldBeTrue();
    }

    [TestMethod]
    public async Task GetProjectRiskElements_FiltersOnlyByParentProject()
    {
        var parent1 = await CreateParentEntity("Proyecto A");

        var parent2 = new ProjectEntity
        {
            Name = "Proyecto B",
            Code = "000002",
            ProgramId = 2,
            AxisId = 6,
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };
        await _service.AddProject(parent2);

        await _service.AddProjectRiskElement(new ProjectRiskElement
        {
            Name = "Riesgo de A",
            CategoryId = 1,
            Probability = 5,
            Severity = 5,
            Mitigation = 5,
            ProjectId = parent1.Id
        });

        await _service.AddProjectRiskElement(new ProjectRiskElement
        {
            Name = "Riesgo de B",
            CategoryId = 2,
            Probability = 3,
            Severity = 3,
            Mitigation = 3,
            ProjectId = parent2.Id
        });

        var riskParent1 = await _service.GetProjectRiskElements(parent1.Id);
        riskParent1.Count.ShouldBe(1);
        riskParent1[0].Name.ShouldBe("Riesgo de A");

        var riskParent2 = await _service.GetProjectRiskElements(parent2.Id);
        riskParent2.Count.ShouldBe(1);
        riskParent2[0].Name.ShouldBe("Riesgo de B");
    }

    [TestMethod]
    public async Task UpdateProjectRiskElement_ModifiesElement()
    {
        var parent = await CreateParentEntity();

        var element = new ProjectRiskElement
        {
            Name = "Original risk",
            CategoryId = 1,
            Probability = 2,
            Severity = 3,
            Mitigation = 4,
            ProjectId = parent.Id
        };
        await _service.AddProjectRiskElement(element);

        // Actualizar
        element.Name = "Modified risk";
        element.CategoryId = 3;
        element.Probability = 7;
        element.Severity = 8;
        element.Mitigation = 9;
        await _service.UpdateProjectRiskElement(element);

        var elements = await _service.GetProjectRiskElements(parent.Id);
        elements.Count.ShouldBe(1);
        elements[0].Name.ShouldBe("Modified risk");
        elements[0].CategoryId.ShouldBe(3);
        elements[0].Probability.ShouldBe(7);
        elements[0].Severity.ShouldBe(8);
        elements[0].Mitigation.ShouldBe(9);
    }

    [TestMethod]
    public async Task DeleteProjectRiskElement_RemovesElement()
    {
        var parent = await CreateParentEntity();

        var element = new ProjectRiskElement
        {
            Name = "Para eliminar",
            CategoryId = 1,
            Probability = 1,
            Severity = 1,
            Mitigation = 1,
            ProjectId = parent.Id
        };
        await _service.AddProjectRiskElement(element);

        var before = await _service.GetProjectRiskElements(parent.Id);
        before.Count.ShouldBe(1);

        await _service.DeleteProjectRiskElement(element.Id);

        var after = await _service.GetProjectRiskElements(parent.Id);
        after.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteProjectRiskElement_NonExistentId_DoesNotThrow()
    {
        await Should.NotThrowAsync(() => _service.DeleteProjectRiskElement(9999));
    }
}

