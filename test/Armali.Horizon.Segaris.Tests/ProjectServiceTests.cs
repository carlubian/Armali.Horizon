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

    // ── ProjectBudget CRUD ──────────────────────────────────

    [TestMethod]
    public async Task AddProjectBudget_AndRetrieve_ReturnsBudget()
    {
        var parent = await CreateParentEntity();

        var budget = new ProjectBudget
        {
            Year = 2026,
            Estimated = 50000.0,
            Actual = 12000.0,
            ProjectId = parent.Id
        };

        await _service.AddProjectBudget(budget);

        var budgets = await _service.GetProjectBudgets(parent.Id);
        budgets.Count.ShouldBe(1);
        budgets[0].Year.ShouldBe(2026);
        budgets[0].Estimated.ShouldBe(50000.0);
        budgets[0].Actual.ShouldBe(12000.0);
        budgets[0].ProjectId.ShouldBe(parent.Id);
    }

    [TestMethod]
    public async Task GetProjectBudgets_OrdersByYearAscending()
    {
        var parent = await CreateParentEntity();

        await _service.AddProjectBudget(new ProjectBudget
        {
            Year = 2028, Estimated = 1000, Actual = 0, ProjectId = parent.Id
        });
        await _service.AddProjectBudget(new ProjectBudget
        {
            Year = 2026, Estimated = 3000, Actual = 0, ProjectId = parent.Id
        });
        await _service.AddProjectBudget(new ProjectBudget
        {
            Year = 2027, Estimated = 2000, Actual = 0, ProjectId = parent.Id
        });

        var budgets = await _service.GetProjectBudgets(parent.Id);
        budgets.Count.ShouldBe(3);
        budgets[0].Year.ShouldBe(2026);
        budgets[1].Year.ShouldBe(2027);
        budgets[2].Year.ShouldBe(2028);
    }

    [TestMethod]
    public async Task GetProjectBudgets_FiltersOnlyByParentProject()
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

        await _service.AddProjectBudget(new ProjectBudget
        {
            Year = 2026, Estimated = 1000, Actual = 500, ProjectId = parent1.Id
        });
        await _service.AddProjectBudget(new ProjectBudget
        {
            Year = 2026, Estimated = 9000, Actual = 100, ProjectId = parent2.Id
        });

        var budgetsP1 = await _service.GetProjectBudgets(parent1.Id);
        budgetsP1.Count.ShouldBe(1);
        budgetsP1[0].Estimated.ShouldBe(1000);

        var budgetsP2 = await _service.GetProjectBudgets(parent2.Id);
        budgetsP2.Count.ShouldBe(1);
        budgetsP2[0].Estimated.ShouldBe(9000);
    }

    [TestMethod]
    public async Task UpdateProjectBudget_ModifiesBudget()
    {
        var parent = await CreateParentEntity();

        var budget = new ProjectBudget
        {
            Year = 2026, Estimated = 5000, Actual = 1000, ProjectId = parent.Id
        };
        await _service.AddProjectBudget(budget);

        budget.Estimated = 8000;
        budget.Actual = 3500;
        await _service.UpdateProjectBudget(budget);

        var budgets = await _service.GetProjectBudgets(parent.Id);
        budgets.Count.ShouldBe(1);
        budgets[0].Estimated.ShouldBe(8000);
        budgets[0].Actual.ShouldBe(3500);
    }

    [TestMethod]
    public async Task DeleteProjectBudget_RemovesBudget()
    {
        var parent = await CreateParentEntity();

        var budget = new ProjectBudget
        {
            Year = 2026, Estimated = 5000, Actual = 0, ProjectId = parent.Id
        };
        await _service.AddProjectBudget(budget);

        var before = await _service.GetProjectBudgets(parent.Id);
        before.Count.ShouldBe(1);

        await _service.DeleteProjectBudget(budget.Id);

        var after = await _service.GetProjectBudgets(parent.Id);
        after.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteProjectBudget_NonExistentId_DoesNotThrow()
    {
        await Should.NotThrowAsync(() => _service.DeleteProjectBudget(9999));
    }

    // ── BudgetYearExists ─────────────────────────────────────

    [TestMethod]
    public async Task BudgetYearExists_ReturnsTrueWhenDuplicate()
    {
        var parent = await CreateParentEntity();

        await _service.AddProjectBudget(new ProjectBudget
        {
            Year = 2026, Estimated = 1000, Actual = 0, ProjectId = parent.Id
        });

        var exists = await _service.BudgetYearExists(parent.Id, 2026);
        exists.ShouldBeTrue();
    }

    [TestMethod]
    public async Task BudgetYearExists_ReturnsFalseWhenNoDuplicate()
    {
        var parent = await CreateParentEntity();

        await _service.AddProjectBudget(new ProjectBudget
        {
            Year = 2026, Estimated = 1000, Actual = 0, ProjectId = parent.Id
        });

        var exists = await _service.BudgetYearExists(parent.Id, 2027);
        exists.ShouldBeFalse();
    }

    [TestMethod]
    public async Task BudgetYearExists_ExcludesOwnId_ReturnsFalse()
    {
        var parent = await CreateParentEntity();

        var budget = new ProjectBudget
        {
            Year = 2026, Estimated = 1000, Actual = 0, ProjectId = parent.Id
        };
        await _service.AddProjectBudget(budget);

        // Al editar el mismo registro, excluir su propio Id
        var exists = await _service.BudgetYearExists(parent.Id, 2026, budget.Id);
        exists.ShouldBeFalse();
    }

    // ── SpentPercent (propiedad calculada) ────────────────────

    [TestMethod]
    public void ProjectBudget_SpentPercent_CalculatedCorrectly()
    {
        var budget = new ProjectBudget { Estimated = 10000, Actual = 2500 };
        budget.SpentPercent.ShouldBe(25.0);
    }

    [TestMethod]
    public void ProjectBudget_SpentPercent_ZeroEstimated_ReturnsZero()
    {
        var budget = new ProjectBudget { Estimated = 0, Actual = 500 };
        budget.SpentPercent.ShouldBe(0);
    }

    [TestMethod]
    public void ProjectBudget_SpentPercent_OverBudget_ReturnsOver100()
    {
        var budget = new ProjectBudget { Estimated = 1000, Actual = 1500 };
        budget.SpentPercent.ShouldBe(150.0);
    }

    // ── CalculateActualBudget ─────────────────────────────────

    [TestMethod]
    public async Task CalculateActualBudget_NoData_ReturnsZero()
    {
        var parent = await CreateParentEntity();
        var result = await _service.CalculateActualBudget(parent.Id, 2026);
        result.ShouldBe(0.0);
    }

    [TestMethod]
    public async Task CalculateActualBudget_SumsCapexInYear()
    {
        var parent = await CreateParentEntity();
        var capexService = new CapexService(_factory);

        // Capex en 2026 vinculado al proyecto
        await capexService.AddCapex(new CapexEntity
        {
            Name = "Capex A", Date = new DateTime(2026, 3, 1), Amount = 1000,
            CategoryId = 1, StatusId = 1, ProjectId = parent.Id,
            IsPrivate = false, Creator = "user1"
        });
        await capexService.AddCapex(new CapexEntity
        {
            Name = "Capex B", Date = new DateTime(2026, 8, 15), Amount = 500,
            CategoryId = 1, StatusId = 1, ProjectId = parent.Id,
            IsPrivate = false, Creator = "user1"
        });

        // Capex en 2025 — no debe contar
        await capexService.AddCapex(new CapexEntity
        {
            Name = "Capex viejo", Date = new DateTime(2025, 12, 31), Amount = 9999,
            CategoryId = 1, StatusId = 1, ProjectId = parent.Id,
            IsPrivate = false, Creator = "user1"
        });

        var result = await _service.CalculateActualBudget(parent.Id, 2026);
        result.ShouldBe(1500.0);
    }

    [TestMethod]
    public async Task CalculateActualBudget_SumsOpexSubEntitiesInYear()
    {
        var parent = await CreateParentEntity();
        var opexService = new OpexService(_factory);

        // Contrato Opex vinculado al proyecto
        var contract = new OpexEntity
        {
            Name = "Contrato Test", CategoryId = 1, StatusId = 1,
            ProjectId = parent.Id, IsPrivate = false, Creator = "user1"
        };
        await opexService.AddOpex(contract);

        // Sub-entidades en 2026
        await opexService.AddOpexSubEntity(new OpexSubEntity
        {
            Date = new DateTime(2026, 2, 1), Amount = 200, ContractId = contract.Id
        });
        await opexService.AddOpexSubEntity(new OpexSubEntity
        {
            Date = new DateTime(2026, 6, 1), Amount = 300, ContractId = contract.Id
        });

        // Sub-entidad en 2025 — no debe contar
        await opexService.AddOpexSubEntity(new OpexSubEntity
        {
            Date = new DateTime(2025, 11, 1), Amount = 8888, ContractId = contract.Id
        });

        var result = await _service.CalculateActualBudget(parent.Id, 2026);
        result.ShouldBe(500.0);
    }

    [TestMethod]
    public async Task CalculateActualBudget_SumsTravelSubEntitiesInYear()
    {
        var parent = await CreateParentEntity();
        var travelService = new TravelService(_factory);

        // Viaje vinculado al proyecto con StartDate en 2026
        var travel = new TravelEntity
        {
            Name = "Viaje Test", CategoryId = 1, StatusId = 1, CostCenterId = 1,
            Destination = "Paris", StartDate = new DateTime(2026, 4, 1),
            EndDate = new DateTime(2026, 4, 10), Pax = 1,
            ProjectId = parent.Id, IsPrivate = false, Creator = "user1"
        };
        await travelService.AddTravel(travel);

        // Sub-entidades del viaje
        await travelService.AddTravelSubEntity(new TravelSubEntity
        {
            Name = "Hotel", CategoryId = 1, Date = new DateTime(2026, 4, 2),
            Amount = 400, TravelId = travel.Id
        });
        await travelService.AddTravelSubEntity(new TravelSubEntity
        {
            Name = "Vuelo", CategoryId = 3, Date = new DateTime(2026, 4, 1),
            Amount = 250, TravelId = travel.Id
        });

        // Viaje en 2025 — no debe contar
        var oldTravel = new TravelEntity
        {
            Name = "Viaje viejo", CategoryId = 1, StatusId = 1, CostCenterId = 1,
            Destination = "Rome", StartDate = new DateTime(2025, 12, 20),
            EndDate = new DateTime(2025, 12, 30), Pax = 1,
            ProjectId = parent.Id, IsPrivate = false, Creator = "user1"
        };
        await travelService.AddTravel(oldTravel);

        await travelService.AddTravelSubEntity(new TravelSubEntity
        {
            Name = "Hotel viejo", CategoryId = 1, Date = new DateTime(2025, 12, 21),
            Amount = 7777, TravelId = oldTravel.Id
        });

        var result = await _service.CalculateActualBudget(parent.Id, 2026);
        result.ShouldBe(650.0);
    }

    [TestMethod]
    public async Task CalculateActualBudget_CombinesAllSources()
    {
        var parent = await CreateParentEntity();
        var capexService = new CapexService(_factory);
        var opexService = new OpexService(_factory);
        var travelService = new TravelService(_factory);
        var inventoryService = new InventoryService(_factory);

        // Capex: 1000
        await capexService.AddCapex(new CapexEntity
        {
            Name = "Capex", Date = new DateTime(2026, 5, 1), Amount = 1000,
            CategoryId = 1, StatusId = 1, ProjectId = parent.Id,
            IsPrivate = false, Creator = "user1"
        });

        // Opex: 200 + 300 = 500
        var contract = new OpexEntity
        {
            Name = "Contrato", CategoryId = 1, StatusId = 1,
            ProjectId = parent.Id, IsPrivate = false, Creator = "user1"
        };
        await opexService.AddOpex(contract);
        await opexService.AddOpexSubEntity(new OpexSubEntity
        {
            Date = new DateTime(2026, 1, 15), Amount = 200, ContractId = contract.Id
        });
        await opexService.AddOpexSubEntity(new OpexSubEntity
        {
            Date = new DateTime(2026, 7, 1), Amount = 300, ContractId = contract.Id
        });

        // Travel: 150
        var travel = new TravelEntity
        {
            Name = "Viaje", CategoryId = 1, StatusId = 1, CostCenterId = 1,
            Destination = "Berlin", StartDate = new DateTime(2026, 9, 1),
            EndDate = new DateTime(2026, 9, 5), Pax = 1,
            ProjectId = parent.Id, IsPrivate = false, Creator = "user1"
        };
        await travelService.AddTravel(travel);
        await travelService.AddTravelSubEntity(new TravelSubEntity
        {
            Name = "Tren", CategoryId = 4, Date = new DateTime(2026, 9, 1),
            Amount = 150, TravelId = travel.Id
        });

        // Inventory: 75
        var vendor = new InvVendorEntity
        {
            Name = "Vendor Test", StatusId = 1, IsPrivate = false, Creator = "user1"
        };
        await inventoryService.AddInvVendor(vendor);
        var item = new InvItemEntity
        {
            Name = "Item Test", CurrentStock = 10, MinStock = 5,
            CategoryId = 1, StatusId = 1, VendorId = vendor.Id,
            IsPrivate = false, Creator = "user1"
        };
        await inventoryService.AddInvItem(item);
        var order = new InvOrderEntity
        {
            PurchaseDate = new DateTime(2026, 6, 1),
            ReceptionDate = new DateTime(2026, 6, 15),
            StatusId = 1, VendorId = vendor.Id,
            IsPrivate = false, Creator = "user1"
        };
        await inventoryService.AddInvOrder(order);
        await inventoryService.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id, ItemCount = 5, Amount = 75,
            OrderId = order.Id, ProjectId = parent.Id
        });

        // Total esperado: 1000 + 500 + 150 + 75 = 1725
        var result = await _service.CalculateActualBudget(parent.Id, 2026);
        result.ShouldBe(1725.0);
    }

    [TestMethod]
    public async Task CalculateActualBudget_IgnoresUnlinkedEntities()
    {
        var parent = await CreateParentEntity();
        var capexService = new CapexService(_factory);

        // Capex vinculado a otro proyecto (null = sin proyecto)
        await capexService.AddCapex(new CapexEntity
        {
            Name = "Sin proyecto", Date = new DateTime(2026, 3, 1), Amount = 5000,
            CategoryId = 1, StatusId = 1, ProjectId = null,
            IsPrivate = false, Creator = "user1"
        });

        // Capex vinculado al proyecto pero en otro año
        await capexService.AddCapex(new CapexEntity
        {
            Name = "Otro año", Date = new DateTime(2027, 1, 1), Amount = 3000,
            CategoryId = 1, StatusId = 1, ProjectId = parent.Id,
            IsPrivate = false, Creator = "user1"
        });

        var result = await _service.CalculateActualBudget(parent.Id, 2026);
        result.ShouldBe(0.0);
    }

    [TestMethod]
    public async Task CalculateActualBudget_SumsInventorySubEntitiesInYear()
    {
        var parent = await CreateParentEntity();
        var inventoryService = new InventoryService(_factory);

        var vendor = new InvVendorEntity
        {
            Name = "Vendor Budget", StatusId = 1, IsPrivate = false, Creator = "user1"
        };
        await inventoryService.AddInvVendor(vendor);

        var item = new InvItemEntity
        {
            Name = "Item Budget", CurrentStock = 10, MinStock = 5,
            CategoryId = 1, StatusId = 1, VendorId = vendor.Id,
            IsPrivate = false, Creator = "user1"
        };
        await inventoryService.AddInvItem(item);

        // Pedido en 2026 — debe contar
        var order2026 = new InvOrderEntity
        {
            PurchaseDate = new DateTime(2026, 4, 1),
            ReceptionDate = new DateTime(2026, 4, 15),
            StatusId = 1, VendorId = vendor.Id,
            IsPrivate = false, Creator = "user1"
        };
        await inventoryService.AddInvOrder(order2026);

        await inventoryService.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id, ItemCount = 3, Amount = 120,
            OrderId = order2026.Id, ProjectId = parent.Id
        });
        await inventoryService.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id, ItemCount = 2, Amount = 80,
            OrderId = order2026.Id, ProjectId = parent.Id
        });

        // Pedido en 2025 vinculado al proyecto — no debe contar
        var order2025 = new InvOrderEntity
        {
            PurchaseDate = new DateTime(2025, 12, 1),
            ReceptionDate = new DateTime(2025, 12, 15),
            StatusId = 1, VendorId = vendor.Id,
            IsPrivate = false, Creator = "user1"
        };
        await inventoryService.AddInvOrder(order2025);

        await inventoryService.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id, ItemCount = 10, Amount = 9999,
            OrderId = order2025.Id, ProjectId = parent.Id
        });

        var result = await _service.CalculateActualBudget(parent.Id, 2026);
        result.ShouldBe(200.0);
    }

    [TestMethod]
    public async Task CalculateActualBudget_IgnoresInventorySubEntitiesWithoutProjectLink()
    {
        var parent = await CreateParentEntity();
        var inventoryService = new InventoryService(_factory);

        var vendor = new InvVendorEntity
        {
            Name = "Vendor Unlinked", StatusId = 1, IsPrivate = false, Creator = "user1"
        };
        await inventoryService.AddInvVendor(vendor);

        var item = new InvItemEntity
        {
            Name = "Item Unlinked", CurrentStock = 10, MinStock = 5,
            CategoryId = 1, StatusId = 1, VendorId = vendor.Id,
            IsPrivate = false, Creator = "user1"
        };
        await inventoryService.AddInvItem(item);

        var order = new InvOrderEntity
        {
            PurchaseDate = new DateTime(2026, 5, 1),
            ReceptionDate = new DateTime(2026, 5, 15),
            StatusId = 1, VendorId = vendor.Id,
            IsPrivate = false, Creator = "user1"
        };
        await inventoryService.AddInvOrder(order);

        // Sub-entidad sin ProjectId — no debe contar
        await inventoryService.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id, ItemCount = 5, Amount = 500,
            OrderId = order.Id, ProjectId = null
        });

        // Sub-entidad vinculada a otro proyecto — no debe contar
        var otherParent = await CreateParentEntity();
        await inventoryService.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id, ItemCount = 2, Amount = 300,
            OrderId = order.Id, ProjectId = otherParent.Id
        });

        var result = await _service.CalculateActualBudget(parent.Id, 2026);
        result.ShouldBe(0.0);
    }
}

