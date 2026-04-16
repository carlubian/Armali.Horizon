using Armali.Horizon.Segaris.Model;
using Armali.Horizon.Segaris.Services;

namespace Armali.Horizon.Segaris.Tests;

[TestClass]
public class AdminServiceTests
{
    private TestDbContextFactory _factory = null!;
    private AdminService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new TestDbContextFactory();
        _service = new AdminService(_factory);
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    // ── GetAdminCategories ─────────────────────────────────────

    [TestMethod]
    public async Task GetAdminCategories_ReturnsSeedData()
    {
        var categories = await _service.GetAdminCategories();

        categories.ShouldNotBeEmpty();
        categories.Count.ShouldBe(8);
        categories.ShouldContain(c => c.Name == "Government");
        categories.ShouldContain(c => c.Name == "Legal");
        categories.ShouldContain(c => c.Name == "Other");
    }

    // ── AddAdmin + GetAdminEntities ────────────────────────────

    [TestMethod]
    public async Task AddAdmin_AndRetrieve_ReturnsEntity()
    {
        var entity = new AdminEntity
        {
            Name = "Test Process",
            CategoryId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddAdmin(entity);

        var all = await _service.GetAdminEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Test Process");
    }

    // ── Privacy filtering ──────────────────────────────────────

    [TestMethod]
    public async Task GetAdminEntities_FiltersPrivateEntitiesFromOthers()
    {
        await _service.AddAdmin(new AdminEntity
        {
            Name = "Publico",
            CategoryId = 1,
            IsPrivate = false,
            Creator = "user1"
        });

        await _service.AddAdmin(new AdminEntity
        {
            Name = "Privado User1",
            CategoryId = 1,
            IsPrivate = true,
            Creator = "user1"
        });

        var user1Results = await _service.GetAdminEntities("user1");
        user1Results.Count.ShouldBe(2);

        var user2Results = await _service.GetAdminEntities("user2");
        user2Results.Count.ShouldBe(1);
        user2Results[0].Name.ShouldBe("Publico");
    }

    // ── UpdateAdmin ────────────────────────────────────────────

    [TestMethod]
    public async Task UpdateAdmin_ModifiesEntity()
    {
        var entity = new AdminEntity
        {
            Name = "Original",
            CategoryId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddAdmin(entity);

        entity.Name = "Modificado";
        entity.CategoryId = 2;
        await _service.UpdateAdmin(entity);

        var all = await _service.GetAdminEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Modificado");
        all[0].CategoryId.ShouldBe(2);
    }

    // ── DeleteAdmin ────────────────────────────────────────────

    [TestMethod]
    public async Task DeleteAdmin_RemovesEntity()
    {
        var entity = new AdminEntity
        {
            Name = "Para eliminar",
            CategoryId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddAdmin(entity);
        var all = await _service.GetAdminEntities("user1");
        all.Count.ShouldBe(1);

        await _service.DeleteAdmin(entity.Id);

        var afterDelete = await _service.GetAdminEntities("user1");
        afterDelete.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteAdmin_NonExistentId_DoesNotThrow()
    {
        await Should.NotThrowAsync(() => _service.DeleteAdmin(9999));
    }

    // ── AdminSubEntity CRUD ────────────────────────────────────

    private async Task<AdminEntity> CreateParentEntity(string name = "Proceso Test")
    {
        var entity = new AdminEntity
        {
            Name = name,
            CategoryId = 1,
            IsPrivate = false,
            Creator = "user1"
        };
        await _service.AddAdmin(entity);
        return entity;
    }

    [TestMethod]
    public async Task AddAdminSubEntity_AndRetrieve_ReturnsSubEntity()
    {
        var parent = await CreateParentEntity();

        var sub = new AdminSubEntity
        {
            Name = "Paso 1",
            StartDate = new DateTime(2026, 4, 1),
            DueDate = new DateTime(2026, 5, 1),
            IsCompleted = false,
            ProcessId = parent.Id
        };

        await _service.AddAdminSubEntity(sub);

        var subs = await _service.GetAdminSubEntities(parent);
        subs.Count.ShouldBe(1);
        subs[0].Name.ShouldBe("Paso 1");
        subs[0].ProcessId.ShouldBe(parent.Id);
    }

    [TestMethod]
    public async Task GetAdminSubEntities_OrdersByDueDateAscending()
    {
        var parent = await CreateParentEntity();

        await _service.AddAdminSubEntity(new AdminSubEntity
        {
            Name = "Late",
            StartDate = new DateTime(2026, 1, 1),
            DueDate = new DateTime(2026, 12, 1),
            ProcessId = parent.Id
        });

        await _service.AddAdminSubEntity(new AdminSubEntity
        {
            Name = "Early",
            StartDate = new DateTime(2026, 1, 1),
            DueDate = new DateTime(2026, 3, 1),
            ProcessId = parent.Id
        });

        await _service.AddAdminSubEntity(new AdminSubEntity
        {
            Name = "Middle",
            StartDate = new DateTime(2026, 1, 1),
            DueDate = new DateTime(2026, 6, 1),
            ProcessId = parent.Id
        });

        var subs = await _service.GetAdminSubEntities(parent);
        subs.Count.ShouldBe(3);
        subs[0].Name.ShouldBe("Early");
        subs[1].Name.ShouldBe("Middle");
        subs[2].Name.ShouldBe("Late");
    }

    [TestMethod]
    public async Task GetAdminSubEntities_FiltersOnlyByParentProcess()
    {
        var parent1 = await CreateParentEntity("Proceso A");
        var parent2 = await CreateParentEntity("Proceso B");

        await _service.AddAdminSubEntity(new AdminSubEntity
        {
            Name = "Paso A",
            StartDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(30),
            ProcessId = parent1.Id
        });

        await _service.AddAdminSubEntity(new AdminSubEntity
        {
            Name = "Paso B",
            StartDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(30),
            ProcessId = parent2.Id
        });

        var subsParent1 = await _service.GetAdminSubEntities(parent1);
        subsParent1.Count.ShouldBe(1);
        subsParent1[0].Name.ShouldBe("Paso A");

        var subsParent2 = await _service.GetAdminSubEntities(parent2);
        subsParent2.Count.ShouldBe(1);
        subsParent2[0].Name.ShouldBe("Paso B");
    }

    [TestMethod]
    public async Task UpdateAdminSubEntity_ModifiesSubEntity()
    {
        var parent = await CreateParentEntity();

        var sub = new AdminSubEntity
        {
            Name = "Original",
            StartDate = new DateTime(2026, 1, 1),
            DueDate = new DateTime(2026, 2, 1),
            IsCompleted = false,
            ProcessId = parent.Id
        };
        await _service.AddAdminSubEntity(sub);

        sub.Name = "Modificado";
        sub.IsCompleted = true;
        await _service.UpdateAdminSubEntity(sub);

        var subs = await _service.GetAdminSubEntities(parent);
        subs.Count.ShouldBe(1);
        subs[0].Name.ShouldBe("Modificado");
        subs[0].IsCompleted.ShouldBeTrue();
    }

    [TestMethod]
    public async Task DeleteAdminSubEntity_RemovesSubEntity()
    {
        var parent = await CreateParentEntity();

        var sub = new AdminSubEntity
        {
            Name = "Para eliminar",
            StartDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(30),
            ProcessId = parent.Id
        };
        await _service.AddAdminSubEntity(sub);

        var before = await _service.GetAdminSubEntities(parent);
        before.Count.ShouldBe(1);

        await _service.DeleteAdminSubEntity(sub.Id);

        var after = await _service.GetAdminSubEntities(parent);
        after.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteAdminSubEntity_NonExistentId_DoesNotThrow()
    {
        await Should.NotThrowAsync(() => _service.DeleteAdminSubEntity(9999));
    }

    // ── ComputeStepStatus (lógica pura) ────────────────────────

    [TestMethod]
    public void ComputeStepStatus_Completed_ReturnsFinished()
    {
        var step = new AdminSubEntity
        {
            StartDate = new DateTime(2026, 1, 1),
            DueDate = new DateTime(2026, 2, 1),
            IsCompleted = true
        };

        // Completado sin importar fechas
        AdminService.ComputeStepStatus(step, new DateTime(2026, 3, 1))
            .ShouldBe(AdminStepStatus.Finished);
    }

    [TestMethod]
    public void ComputeStepStatus_FutureStart_ReturnsNotStarted()
    {
        var step = new AdminSubEntity
        {
            StartDate = new DateTime(2026, 6, 1),
            DueDate = new DateTime(2026, 7, 1),
            IsCompleted = false
        };

        // Hoy es antes de StartDate
        AdminService.ComputeStepStatus(step, new DateTime(2026, 4, 14))
            .ShouldBe(AdminStepStatus.NotStarted);
    }

    [TestMethod]
    public void ComputeStepStatus_InProgressWithinDeadline_ReturnsOnTime()
    {
        var step = new AdminSubEntity
        {
            StartDate = new DateTime(2026, 4, 1),
            DueDate = new DateTime(2026, 5, 1),
            IsCompleted = false
        };

        // Hoy está entre StartDate y DueDate
        AdminService.ComputeStepStatus(step, new DateTime(2026, 4, 14))
            .ShouldBe(AdminStepStatus.OnTime);
    }

    [TestMethod]
    public void ComputeStepStatus_OnDueDate_ReturnsOnTime()
    {
        var step = new AdminSubEntity
        {
            StartDate = new DateTime(2026, 4, 1),
            DueDate = new DateTime(2026, 4, 14),
            IsCompleted = false
        };

        // Hoy es exactamente DueDate — aún a tiempo
        AdminService.ComputeStepStatus(step, new DateTime(2026, 4, 14))
            .ShouldBe(AdminStepStatus.OnTime);
    }

    [TestMethod]
    public void ComputeStepStatus_PastDueDate_ReturnsDelayed()
    {
        var step = new AdminSubEntity
        {
            StartDate = new DateTime(2026, 1, 1),
            DueDate = new DateTime(2026, 3, 1),
            IsCompleted = false
        };

        // DueDate ya pasó
        AdminService.ComputeStepStatus(step, new DateTime(2026, 4, 14))
            .ShouldBe(AdminStepStatus.Delayed);
    }

    [TestMethod]
    public void ComputeStepStatus_CompletedPastDue_StillFinished()
    {
        var step = new AdminSubEntity
        {
            StartDate = new DateTime(2026, 1, 1),
            DueDate = new DateTime(2026, 2, 1),
            IsCompleted = true
        };

        // Completado aunque DueDate ya pasó → Finished prevalece
        AdminService.ComputeStepStatus(step, new DateTime(2026, 4, 14))
            .ShouldBe(AdminStepStatus.Finished);
    }

    // ── ComputeStats (lógica pura) ─────────────────────────────

    [TestMethod]
    public void ComputeStats_EmptyList_ReturnsZeros()
    {
        var stats = AdminService.ComputeStats([], new DateTime(2026, 4, 14));

        stats.Total.ShouldBe(0);
        stats.Finished.ShouldBe(0);
        stats.NotStarted.ShouldBe(0);
        stats.OnTime.ShouldBe(0);
        stats.Delayed.ShouldBe(0);
        stats.OverallColor.ShouldBe("gray");
        stats.OverallName.ShouldBe("Empty");
        stats.Summary.ShouldBe("No steps");
    }

    [TestMethod]
    public void ComputeStats_AllFinished_GreenStatus()
    {
        var steps = new List<AdminSubEntity>
        {
            new() { StartDate = new DateTime(2026, 1, 1), DueDate = new DateTime(2026, 2, 1), IsCompleted = true },
            new() { StartDate = new DateTime(2026, 2, 1), DueDate = new DateTime(2026, 3, 1), IsCompleted = true }
        };

        var stats = AdminService.ComputeStats(steps, new DateTime(2026, 4, 14));

        stats.Total.ShouldBe(2);
        stats.Finished.ShouldBe(2);
        stats.OverallColor.ShouldBe("green");
        stats.OverallName.ShouldBe("Finished");
    }

    [TestMethod]
    public void ComputeStats_HasDelayed_RedStatus()
    {
        var steps = new List<AdminSubEntity>
        {
            new() { StartDate = new DateTime(2026, 1, 1), DueDate = new DateTime(2026, 2, 1), IsCompleted = true },
            new() { StartDate = new DateTime(2026, 1, 1), DueDate = new DateTime(2026, 3, 1), IsCompleted = false },
            new() { StartDate = new DateTime(2026, 4, 1), DueDate = new DateTime(2026, 5, 1), IsCompleted = false }
        };

        var stats = AdminService.ComputeStats(steps, new DateTime(2026, 4, 14));

        stats.Finished.ShouldBe(1);
        stats.Delayed.ShouldBe(1);
        stats.OnTime.ShouldBe(1);
        stats.OverallColor.ShouldBe("red");
        stats.OverallName.ShouldBe("Delayed");
    }

    [TestMethod]
    public void ComputeStats_PendingNoDelayed_GoldStatus()
    {
        var steps = new List<AdminSubEntity>
        {
            new() { StartDate = new DateTime(2026, 1, 1), DueDate = new DateTime(2026, 2, 1), IsCompleted = true },
            new() { StartDate = new DateTime(2026, 4, 1), DueDate = new DateTime(2026, 5, 1), IsCompleted = false },
            new() { StartDate = new DateTime(2026, 6, 1), DueDate = new DateTime(2026, 7, 1), IsCompleted = false }
        };

        var stats = AdminService.ComputeStats(steps, new DateTime(2026, 4, 14));

        stats.Finished.ShouldBe(1);
        stats.OnTime.ShouldBe(1);
        stats.NotStarted.ShouldBe(1);
        stats.Delayed.ShouldBe(0);
        stats.OverallColor.ShouldBe("gold");
        stats.OverallName.ShouldBe("In Progress");
    }

    [TestMethod]
    public void ComputeStats_SummaryText_FormatsCorrectly()
    {
        var steps = new List<AdminSubEntity>
        {
            new() { StartDate = new DateTime(2026, 1, 1), DueDate = new DateTime(2026, 2, 1), IsCompleted = true },
            new() { StartDate = new DateTime(2026, 1, 1), DueDate = new DateTime(2026, 2, 1), IsCompleted = true },
            new() { StartDate = new DateTime(2026, 4, 1), DueDate = new DateTime(2026, 5, 1), IsCompleted = false },
            new() { StartDate = new DateTime(2026, 6, 1), DueDate = new DateTime(2026, 7, 1), IsCompleted = false },
            new() { StartDate = new DateTime(2026, 1, 1), DueDate = new DateTime(2026, 3, 1), IsCompleted = false }
        };

        var stats = AdminService.ComputeStats(steps, new DateTime(2026, 4, 14));

        stats.Summary.ShouldBe("2 finished, 1 not started, 1 on time, 1 delayed");
    }

    // ── AdminComputedStatus ────────────────────────────────────

    [TestMethod]
    public void AdminComputedStatus_FromStats_MapsCorrectly()
    {
        var stats = new AdminStats { Finished = 3, OnTime = 1 };
        var computed = AdminComputedStatus.FromStats(stats);

        computed.Name.ShouldBe("In Progress");
        computed.Color.ShouldBe("gold");
    }

    // ── GetAdminStats (integración con DB) ─────────────────────

    [TestMethod]
    public async Task GetAdminStats_WithNoSubEntities_ReturnsEmpty()
    {
        var parent = await CreateParentEntity();

        var stats = await _service.GetAdminStats(parent.Id);
        stats.Total.ShouldBe(0);
        stats.OverallColor.ShouldBe("gray");
    }

    [TestMethod]
    public async Task GetAdminStats_WithMixedSteps_ReturnsCorrectCounts()
    {
        var parent = await CreateParentEntity();

        // Completado
        await _service.AddAdminSubEntity(new AdminSubEntity
        {
            Name = "Done",
            StartDate = new DateTime(2026, 1, 1),
            DueDate = new DateTime(2026, 2, 1),
            IsCompleted = true,
            ProcessId = parent.Id
        });

        // En curso (depende de la fecha actual)
        await _service.AddAdminSubEntity(new AdminSubEntity
        {
            Name = "Current",
            StartDate = DateTime.Today.AddDays(-10),
            DueDate = DateTime.Today.AddDays(30),
            IsCompleted = false,
            ProcessId = parent.Id
        });

        var stats = await _service.GetAdminStats(parent.Id);
        stats.Total.ShouldBe(2);
        stats.Finished.ShouldBe(1);
        // El segundo paso debería ser OnTime (DueDate en el futuro)
        stats.OnTime.ShouldBe(1);
    }

    [TestMethod]
    public async Task GetAdminStats_OnlyCountsStepsOfGivenProcess()
    {
        var parent1 = await CreateParentEntity("Proceso A");
        var parent2 = await CreateParentEntity("Proceso B");

        await _service.AddAdminSubEntity(new AdminSubEntity
        {
            Name = "Paso A1",
            StartDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(10),
            ProcessId = parent1.Id
        });

        await _service.AddAdminSubEntity(new AdminSubEntity
        {
            Name = "Paso A2",
            StartDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(10),
            ProcessId = parent1.Id
        });

        await _service.AddAdminSubEntity(new AdminSubEntity
        {
            Name = "Paso B1",
            StartDate = DateTime.Today,
            DueDate = DateTime.Today.AddDays(10),
            ProcessId = parent2.Id
        });

        var stats1 = await _service.GetAdminStats(parent1.Id);
        stats1.Total.ShouldBe(2);

        var stats2 = await _service.GetAdminStats(parent2.Id);
        stats2.Total.ShouldBe(1);
    }
}

