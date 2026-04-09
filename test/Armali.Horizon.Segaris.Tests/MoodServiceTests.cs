using Armali.Horizon.Segaris.Model;
using Armali.Horizon.Segaris.Services;

namespace Armali.Horizon.Segaris.Tests;

[TestClass]
public class MoodServiceTests
{
    private TestDbContextFactory _factory = null!;
    private MoodService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new TestDbContextFactory();
        _service = new MoodService(_factory);
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    // ── GetMoodCategories ────────────────────────────────────

    [TestMethod]
    public async Task GetMoodCategories_ReturnsSeedData()
    {
        var categories = await _service.GetMoodCategories();

        categories.ShouldNotBeEmpty();
        categories.Count.ShouldBe(81);
        categories.ShouldContain(c => c.Name == "Happy" && c.PrimaryColor == "green");
    }

    // ── AddMood + GetMoodEntities ────────────────────────────

    [TestMethod]
    public async Task AddMood_AndRetrieve_ReturnsEntity()
    {
        var entity = new MoodEntity
        {
            Date = new DateTime(2026, 1, 15),
            Score = 8,
            CategoryId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddMood(entity);

        var all = await _service.GetMoodEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Score.ShouldBe(8);
        all[0].CategoryId.ShouldBe(1);
    }

    // ── Privacy filtering ────────────────────────────────────
    // Mood usa filtro solo por Creator (sin lógica IsPrivate):
    // un usuario nunca ve las entidades de otro.

    [TestMethod]
    public async Task GetMoodEntities_OnlyReturnsOwnEntries()
    {
        await _service.AddMood(new MoodEntity
        {
            Date = DateTime.Now,
            Score = 7,
            CategoryId = 1,
            IsPrivate = false,
            Creator = "user1"
        });

        await _service.AddMood(new MoodEntity
        {
            Date = DateTime.Now,
            Score = 5,
            CategoryId = 2,
            IsPrivate = false,
            Creator = "user2"
        });

        // user1 solo ve su propia entrada
        var user1Results = await _service.GetMoodEntities("user1");
        user1Results.Count.ShouldBe(1);
        user1Results[0].Score.ShouldBe(7);

        // user2 solo ve su propia entrada
        var user2Results = await _service.GetMoodEntities("user2");
        user2Results.Count.ShouldBe(1);
        user2Results[0].Score.ShouldBe(5);
    }

    // ── GetMoodBetweenDates ──────────────────────────────────

    [TestMethod]
    public async Task GetMoodBetweenDates_FiltersCorrectly()
    {
        // Entrada dentro del rango
        await _service.AddMood(new MoodEntity
        {
            Date = new DateTime(2026, 3, 15),
            Score = 9,
            CategoryId = 1,
            IsPrivate = false,
            Creator = "user1"
        });

        // Entrada fuera del rango (anterior)
        await _service.AddMood(new MoodEntity
        {
            Date = new DateTime(2026, 1, 1),
            Score = 3,
            CategoryId = 2,
            IsPrivate = false,
            Creator = "user1"
        });

        // Entrada fuera del rango (posterior)
        await _service.AddMood(new MoodEntity
        {
            Date = new DateTime(2026, 6, 1),
            Score = 6,
            CategoryId = 3,
            IsPrivate = false,
            Creator = "user1"
        });

        // Entrada de otro usuario dentro del rango — no debe aparecer
        await _service.AddMood(new MoodEntity
        {
            Date = new DateTime(2026, 3, 20),
            Score = 4,
            CategoryId = 1,
            IsPrivate = false,
            Creator = "user2"
        });

        var results = await _service.GetMoodBetweenDates(
            new DateTime(2026, 3, 1),
            new DateTime(2026, 3, 31),
            "user1");

        results.Count.ShouldBe(1);
        results[0].Score.ShouldBe(9);
    }

    // ── UpdateMood ───────────────────────────────────────────

    [TestMethod]
    public async Task UpdateMood_ModifiesEntity()
    {
        var entity = new MoodEntity
        {
            Date = DateTime.Now,
            Score = 5,
            CategoryId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddMood(entity);

        // Actualizar
        entity.Score = 10;
        entity.CategoryId = 2;
        await _service.UpdateMood(entity);

        var all = await _service.GetMoodEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Score.ShouldBe(10);
        all[0].CategoryId.ShouldBe(2);
    }

    // ── DeleteMood ───────────────────────────────────────────

    [TestMethod]
    public async Task DeleteMood_RemovesEntity()
    {
        var entity = new MoodEntity
        {
            Date = DateTime.Now,
            Score = 4,
            CategoryId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddMood(entity);
        var all = await _service.GetMoodEntities("user1");
        all.Count.ShouldBe(1);

        await _service.DeleteMood(entity.Id);

        var afterDelete = await _service.GetMoodEntities("user1");
        afterDelete.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteMood_NonExistentId_DoesNotThrow()
    {
        // No debe lanzar excepción al intentar eliminar un ID que no existe
        await Should.NotThrowAsync(() => _service.DeleteMood(9999));
    }
}

