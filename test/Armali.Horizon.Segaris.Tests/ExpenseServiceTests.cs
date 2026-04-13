using Armali.Horizon.Segaris.Model;
using Armali.Horizon.Segaris.Services;

namespace Armali.Horizon.Segaris.Tests;

[TestClass]
public class ExpenseServiceTests
{
    private TestDbContextFactory _factory = null!;
    private ExpenseService _service = null!;
    private CapexService _capexService = null!;
    private OpexService _opexService = null!;
    private TravelService _travelService = null!;
    private InventoryService _inventoryService = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new TestDbContextFactory();
        _service = new ExpenseService(_factory);
        _capexService = new CapexService(_factory);
        _opexService = new OpexService(_factory);
        _travelService = new TravelService(_factory);
        _inventoryService = new InventoryService(_factory);
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    // ── Helpers para seed de datos ───────────────────────────────

    private async Task SeedCapex(double amount, int month, int categoryId = 1, bool isPrivate = false, string creator = "user1")
    {
        await _capexService.AddCapex(new CapexEntity
        {
            Name = $"Capex {amount}",
            Date = new DateTime(2026, month, 15),
            Amount = amount,
            CategoryId = categoryId,
            StatusId = 1,
            IsPrivate = isPrivate,
            Creator = creator
        });
    }

    private async Task SeedOpexWithSub(double amount, int month, int categoryId = 1, bool isPrivate = false, string creator = "user1")
    {
        var opex = new OpexEntity
        {
            Name = $"Opex contract {amount}",
            CategoryId = categoryId,
            StatusId = 1,
            IsPrivate = isPrivate,
            Creator = creator
        };
        await _opexService.AddOpex(opex);
        await _opexService.AddOpexSubEntity(new OpexSubEntity
        {
            Date = new DateTime(2026, month, 10),
            Amount = amount,
            ContractId = opex.Id
        });
    }

    private async Task<int> SeedTravelWithSub(double amount, int month, int subCategoryId = 1, bool isPrivate = false, string creator = "user1")
    {
        var travel = new TravelEntity
        {
            Name = $"Trip {amount}",
            CategoryId = 1,
            StatusId = 1,
            CostCenterId = 1,
            StartDate = new DateTime(2026, month, 1),
            EndDate = new DateTime(2026, month, 5),
            IsPrivate = isPrivate,
            Creator = creator
        };
        await _travelService.AddTravel(travel);
        await _travelService.AddTravelSubEntity(new TravelSubEntity
        {
            Name = "Expense",
            Date = new DateTime(2026, month, 3),
            Amount = amount,
            CategoryId = subCategoryId,
            TravelId = travel.Id
        });
        return travel.Id;
    }

    private async Task SeedInventoryOrder(double subAmount, int month, int vendorId, int itemId, bool isPrivate = false, string creator = "user1")
    {
        var order = new InvOrderEntity
        {
            PurchaseDate = new DateTime(2026, month, 5),
            ReceptionDate = new DateTime(2026, month, 10),
            StatusId = 1,
            VendorId = vendorId,
            IsPrivate = isPrivate,
            Creator = creator
        };
        await _inventoryService.AddInvOrder(order);
        await _inventoryService.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = itemId,
            ItemCount = 1,
            Amount = subAmount,
            OrderId = order.Id
        });
    }

    // ── Capex monthly ───────────────────────────────────────────

    [TestMethod]
    public async Task GetCapexMonthly_ReturnsAllTwelveMonths()
    {
        var result = await _service.GetCapexMonthly(2026, "user1");
        result.Count.ShouldBe(12);
        result.ShouldAllBe(m => m.Income == 0 && m.Expense == 0);
    }

    [TestMethod]
    public async Task GetCapexMonthly_AggregatesIncomeAndExpense()
    {
        await SeedCapex(500, 3);       // income
        await SeedCapex(-200, 3);      // expense
        await SeedCapex(100, 3);       // income

        var result = await _service.GetCapexMonthly(2026, "user1");
        var march = result.First(m => m.Month == 3);

        march.Income.ShouldBe(600);
        march.Expense.ShouldBe(200);
    }

    [TestMethod]
    public async Task GetCapexMonthly_FiltersByYear()
    {
        await SeedCapex(100, 6); // año 2026

        var result2025 = await _service.GetCapexMonthly(2025, "user1");
        result2025.ShouldAllBe(m => m.Income == 0 && m.Expense == 0);
    }

    [TestMethod]
    public async Task GetCapexMonthly_RespectsPrivacy()
    {
        await SeedCapex(300, 1, isPrivate: true, creator: "user1");

        // user1 ve su propio dato privado
        var u1 = await _service.GetCapexMonthly(2026, "user1");
        u1.First(m => m.Month == 1).Income.ShouldBe(300);

        // user2 no lo ve
        var u2 = await _service.GetCapexMonthly(2026, "user2");
        u2.First(m => m.Month == 1).Income.ShouldBe(0);
    }

    // ── Capex by category ───────────────────────────────────────

    [TestMethod]
    public async Task GetCapexByCategory_GroupsByCategory()
    {
        await SeedCapex(100, 2, categoryId: 1);  // Government
        await SeedCapex(-50, 4, categoryId: 1);  // Government (expense)
        await SeedCapex(200, 2, categoryId: 2);  // Employment

        var result = await _service.GetCapexByCategory(2026, "user1");

        result.Count.ShouldBe(2);
        result.First(c => c.Name == "Government").Total.ShouldBe(150);  // |100| + |-50|
        result.First(c => c.Name == "Employment").Total.ShouldBe(200);
    }

    // ── Opex monthly ────────────────────────────────────────────

    [TestMethod]
    public async Task GetOpexMonthly_AggregatesSubEntities()
    {
        await SeedOpexWithSub(-750, 5);
        await SeedOpexWithSub(200, 5);

        var result = await _service.GetOpexMonthly(2026, "user1");
        var may = result.First(m => m.Month == 5);

        may.Income.ShouldBe(200);
        may.Expense.ShouldBe(750);
    }

    [TestMethod]
    public async Task GetOpexMonthly_RespectsPrivacy()
    {
        await SeedOpexWithSub(-100, 3, isPrivate: true, creator: "user1");

        var u2 = await _service.GetOpexMonthly(2026, "user2");
        u2.First(m => m.Month == 3).Expense.ShouldBe(0);
    }

    // ── Opex by category ────────────────────────────────────────

    [TestMethod]
    public async Task GetOpexByCategory_GroupsByCategoryFromParent()
    {
        await SeedOpexWithSub(-400, 2, categoryId: 1);  // Government
        await SeedOpexWithSub(-100, 2, categoryId: 4);  // Software

        var result = await _service.GetOpexByCategory(2026, "user1");
        result.Count.ShouldBe(2);
        result.First(c => c.Name == "Government").Total.ShouldBe(400);
        result.First(c => c.Name == "Software").Total.ShouldBe(100);
    }

    // ── Travel monthly ──────────────────────────────────────────

    [TestMethod]
    public async Task GetTravelMonthly_AggregatesSubEntities()
    {
        await SeedTravelWithSub(-300, 7);
        await SeedTravelWithSub(50, 7);

        var result = await _service.GetTravelMonthly(2026, "user1");
        var july = result.First(m => m.Month == 7);

        july.Income.ShouldBe(50);
        july.Expense.ShouldBe(300);
    }

    // ── Travel by expense category ──────────────────────────────

    [TestMethod]
    public async Task GetTravelByExpenseCategory_GroupsBySubEntityCategory()
    {
        // subCategoryId 1 = "Hotels", 3 = "Airplane" (seed data)
        await SeedTravelWithSub(-500, 8, subCategoryId: 1);
        await SeedTravelWithSub(-200, 8, subCategoryId: 3);

        var result = await _service.GetTravelByExpenseCategory(2026, "user1");
        result.Count.ShouldBe(2);
        result.First(c => c.Name == "Hotels").Total.ShouldBe(500);
        result.First(c => c.Name == "Airplane").Total.ShouldBe(200);
    }

    // ── Inventory monthly ───────────────────────────────────────

    [TestMethod]
    public async Task GetInventoryMonthly_OnlyExpense()
    {
        // Necesitamos un vendedor e ítem existentes; los seed no los crean,
        // así que los insertamos directamente.
        await _inventoryService.AddInvVendor(new InvVendorEntity
        {
            Name = "Vendor A", StatusId = 1, Creator = "user1"
        });
        await _inventoryService.AddInvItem(new InvItemEntity
        {
            Name = "Item A", CategoryId = 1, StatusId = 1, VendorId = 1, Creator = "user1"
        });

        await SeedInventoryOrder(150, 4, vendorId: 1, itemId: 1);

        var result = await _service.GetInventoryMonthly(2026, "user1");
        var april = result.First(m => m.Month == 4);

        april.Income.ShouldBe(0);
        april.Expense.ShouldBe(150);
    }

    // ── Inventory by vendor ─────────────────────────────────────

    [TestMethod]
    public async Task GetInventoryByVendor_GroupsByVendor()
    {
        await _inventoryService.AddInvVendor(new InvVendorEntity
        {
            Name = "Vendor A", StatusId = 1, Creator = "user1"
        });
        await _inventoryService.AddInvVendor(new InvVendorEntity
        {
            Name = "Vendor B", StatusId = 1, Creator = "user1"
        });
        await _inventoryService.AddInvItem(new InvItemEntity
        {
            Name = "Item X", CategoryId = 1, StatusId = 1, VendorId = 1, Creator = "user1"
        });

        await SeedInventoryOrder(100, 6, vendorId: 1, itemId: 1);
        await SeedInventoryOrder(250, 6, vendorId: 2, itemId: 1);

        var result = await _service.GetInventoryByVendor(2026, "user1");
        result.Count.ShouldBe(2);
        result.First(c => c.Name == "Vendor A").Total.ShouldBe(100);
        result.First(c => c.Name == "Vendor B").Total.ShouldBe(250);
    }

    // ── Inventory by item category ──────────────────────────────

    [TestMethod]
    public async Task GetInventoryByItemCategory_GroupsByItemCategory()
    {
        await _inventoryService.AddInvVendor(new InvVendorEntity
        {
            Name = "Vendor A", StatusId = 1, Creator = "user1"
        });
        // categoryId 1 = "Bath Amenities", categoryId 3 = "Cleaning Products"
        await _inventoryService.AddInvItem(new InvItemEntity
        {
            Name = "Item A", CategoryId = 1, StatusId = 1, VendorId = 1, Creator = "user1"
        });
        await _inventoryService.AddInvItem(new InvItemEntity
        {
            Name = "Item B", CategoryId = 3, StatusId = 1, VendorId = 1, Creator = "user1"
        });

        await SeedInventoryOrder(80, 9, vendorId: 1, itemId: 1);
        await SeedInventoryOrder(120, 9, vendorId: 1, itemId: 2);

        var result = await _service.GetInventoryByItemCategory(2026, "user1");
        result.Count.ShouldBe(2);
        result.First(c => c.Name == "Bath Amenities").Total.ShouldBe(80);
        result.First(c => c.Name == "Cleaning Products").Total.ShouldBe(120);
    }
}

