using Armali.Horizon.Segaris.Model;
using Armali.Horizon.Segaris.Services;

namespace Armali.Horizon.Segaris.Tests;

[TestClass]
public class InventoryServiceTests
{
    private TestDbContextFactory _factory = null!;
    private InventoryService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new TestDbContextFactory();
        _service = new InventoryService(_factory);
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    // ═══════════════════════════════════════════════════════════
    // ── Vendor (nivel simple, estilo Capex) ──────────────────
    // ═══════════════════════════════════════════════════════════

    // ── GetInvVendorStatuses ─────────────────────────────────

    [TestMethod]
    public async Task GetInvVendorStatuses_ReturnsSeedData()
    {
        var statuses = await _service.GetInvVendorStatuses();

        statuses.Count.ShouldBe(3);
        statuses.ShouldContain(s => s.Name == "Planning" && s.Color == "blue");
        statuses.ShouldContain(s => s.Name == "Active" && s.Color == "green");
        statuses.ShouldContain(s => s.Name == "Closed" && s.Color == "red");
    }

    // ── AddInvVendor + GetInvVendorEntities ──────────────────

    [TestMethod]
    public async Task AddInvVendor_AndRetrieve_ReturnsEntity()
    {
        var entity = new InvVendorEntity
        {
            Name = "Proveedor Test",
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddInvVendor(entity);

        var all = await _service.GetInvVendorEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Proveedor Test");
    }

    // ── Privacy filtering (Vendors) ──────────────────────────

    [TestMethod]
    public async Task GetInvVendorEntities_FiltersPrivateEntitiesFromOthers()
    {
        // Entidad pública de user1 — visible para todos
        await _service.AddInvVendor(new InvVendorEntity
        {
            Name = "Publico",
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        });

        // Entidad privada de user1 — solo visible para user1
        await _service.AddInvVendor(new InvVendorEntity
        {
            Name = "Privado User1",
            StatusId = 1,
            IsPrivate = true,
            Creator = "user1"
        });

        // user1 ve ambas
        var user1Results = await _service.GetInvVendorEntities("user1");
        user1Results.Count.ShouldBe(2);

        // user2 solo ve la pública
        var user2Results = await _service.GetInvVendorEntities("user2");
        user2Results.Count.ShouldBe(1);
        user2Results[0].Name.ShouldBe("Publico");
    }

    // ── UpdateInvVendor ──────────────────────────────────────

    [TestMethod]
    public async Task UpdateInvVendor_ModifiesEntity()
    {
        var entity = new InvVendorEntity
        {
            Name = "Original",
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddInvVendor(entity);

        entity.Name = "Modificado";
        entity.StatusId = 2;
        await _service.UpdateInvVendor(entity);

        var all = await _service.GetInvVendorEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Modificado");
        all[0].StatusId.ShouldBe(2);
    }

    // ── DeleteInvVendor ──────────────────────────────────────

    [TestMethod]
    public async Task DeleteInvVendor_RemovesEntity()
    {
        var entity = new InvVendorEntity
        {
            Name = "Para eliminar",
            StatusId = 1,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddInvVendor(entity);
        var all = await _service.GetInvVendorEntities("user1");
        all.Count.ShouldBe(1);

        await _service.DeleteInvVendor(entity.Id);

        var afterDelete = await _service.GetInvVendorEntities("user1");
        afterDelete.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteInvVendor_NonExistentId_DoesNotThrow()
    {
        await Should.NotThrowAsync(() => _service.DeleteInvVendor(9999));
    }

    // ═══════════════════════════════════════════════════════════
    // ── Item (nivel simple, estilo Capex) ────────────────────
    // ═══════════════════════════════════════════════════════════

    // ── GetInvItemCategories ─────────────────────────────────

    [TestMethod]
    public async Task GetInvItemCategories_ReturnsSeedData()
    {
        var categories = await _service.GetInvItemCategories();

        categories.ShouldNotBeEmpty();
        categories.Count.ShouldBe(11);
        categories.ShouldContain(c => c.Name == "Bath Amenities");
        categories.ShouldContain(c => c.Name == "Office Supplies");
    }

    // ── GetInvItemStatuses ───────────────────────────────────

    [TestMethod]
    public async Task GetInvItemStatuses_ReturnsSeedData()
    {
        var statuses = await _service.GetInvItemStatuses();

        statuses.Count.ShouldBe(3);
        statuses.ShouldContain(s => s.Name == "Active" && s.Color == "green");
        statuses.ShouldContain(s => s.Name == "Deprecated" && s.Color == "red");
        statuses.ShouldContain(s => s.Name == "Replaced" && s.Color == "blue");
    }

    // ── AddInvItem + GetInvItemEntities ──────────────────────

    /// <summary>
    /// Helper que crea un Vendor padre para poder asignar Items.
    /// </summary>
    private async Task<InvVendorEntity> CreateVendor(string name = "Vendor Test")
    {
        var vendor = new InvVendorEntity
        {
            Name = name,
            StatusId = 2,
            IsPrivate = false,
            Creator = "user1"
        };
        await _service.AddInvVendor(vendor);
        return vendor;
    }

    [TestMethod]
    public async Task AddInvItem_AndRetrieve_ReturnsEntity()
    {
        var vendor = await CreateVendor();

        var entity = new InvItemEntity
        {
            Name = "Jabón líquido",
            CurrentStock = 50,
            MinStock = 10,
            CategoryId = 1,
            StatusId = 1,
            VendorId = vendor.Id,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddInvItem(entity);

        var all = await _service.GetInvItemEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Jabón líquido");
        all[0].CurrentStock.ShouldBe(50);
    }

    // ── Privacy filtering (Items) ────────────────────────────

    [TestMethod]
    public async Task GetInvItemEntities_FiltersPrivateEntitiesFromOthers()
    {
        var vendor = await CreateVendor();

        await _service.AddInvItem(new InvItemEntity
        {
            Name = "Publico",
            CurrentStock = 10,
            MinStock = 5,
            CategoryId = 1,
            StatusId = 1,
            VendorId = vendor.Id,
            IsPrivate = false,
            Creator = "user1"
        });

        await _service.AddInvItem(new InvItemEntity
        {
            Name = "Privado User1",
            CurrentStock = 20,
            MinStock = 5,
            CategoryId = 1,
            StatusId = 1,
            VendorId = vendor.Id,
            IsPrivate = true,
            Creator = "user1"
        });

        var user1Results = await _service.GetInvItemEntities("user1");
        user1Results.Count.ShouldBe(2);

        var user2Results = await _service.GetInvItemEntities("user2");
        user2Results.Count.ShouldBe(1);
        user2Results[0].Name.ShouldBe("Publico");
    }

    // ── GetInvItemByVendor ───────────────────────────────────

    [TestMethod]
    public async Task GetInvItemByVendor_FiltersOnlyByVendor()
    {
        var vendorA = await CreateVendor("Vendor A");
        var vendorB = await CreateVendor("Vendor B");

        await _service.AddInvItem(new InvItemEntity
        {
            Name = "Item A",
            CurrentStock = 10,
            MinStock = 5,
            CategoryId = 1,
            StatusId = 1,
            VendorId = vendorA.Id,
            IsPrivate = false,
            Creator = "user1"
        });

        await _service.AddInvItem(new InvItemEntity
        {
            Name = "Item B",
            CurrentStock = 20,
            MinStock = 5,
            CategoryId = 2,
            StatusId = 1,
            VendorId = vendorB.Id,
            IsPrivate = false,
            Creator = "user1"
        });

        var itemsA = await _service.GetInvItemByVendor(vendorA.Id, "user1");
        itemsA.Count.ShouldBe(1);
        itemsA[0].Name.ShouldBe("Item A");

        var itemsB = await _service.GetInvItemByVendor(vendorB.Id, "user1");
        itemsB.Count.ShouldBe(1);
        itemsB[0].Name.ShouldBe("Item B");
    }

    [TestMethod]
    public async Task GetInvItemByVendor_RespectsPrivacy()
    {
        var vendor = await CreateVendor();

        await _service.AddInvItem(new InvItemEntity
        {
            Name = "Item público",
            CurrentStock = 5,
            MinStock = 1,
            CategoryId = 1,
            StatusId = 1,
            VendorId = vendor.Id,
            IsPrivate = false,
            Creator = "user1"
        });

        await _service.AddInvItem(new InvItemEntity
        {
            Name = "Item privado",
            CurrentStock = 5,
            MinStock = 1,
            CategoryId = 1,
            StatusId = 1,
            VendorId = vendor.Id,
            IsPrivate = true,
            Creator = "user1"
        });

        var user1Items = await _service.GetInvItemByVendor(vendor.Id, "user1");
        user1Items.Count.ShouldBe(2);

        var user2Items = await _service.GetInvItemByVendor(vendor.Id, "user2");
        user2Items.Count.ShouldBe(1);
        user2Items[0].Name.ShouldBe("Item público");
    }

    // ── UpdateInvItem ────────────────────────────────────────

    [TestMethod]
    public async Task UpdateInvItem_ModifiesEntity()
    {
        var vendor = await CreateVendor();

        var entity = new InvItemEntity
        {
            Name = "Original",
            CurrentStock = 10,
            MinStock = 5,
            CategoryId = 1,
            StatusId = 1,
            VendorId = vendor.Id,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddInvItem(entity);

        entity.Name = "Modificado";
        entity.CurrentStock = 99;
        await _service.UpdateInvItem(entity);

        var all = await _service.GetInvItemEntities("user1");
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Modificado");
        all[0].CurrentStock.ShouldBe(99);
    }

    // ── DeleteInvItem ────────────────────────────────────────

    [TestMethod]
    public async Task DeleteInvItem_RemovesEntity()
    {
        var vendor = await CreateVendor();

        var entity = new InvItemEntity
        {
            Name = "Para eliminar",
            CurrentStock = 1,
            MinStock = 0,
            CategoryId = 1,
            StatusId = 1,
            VendorId = vendor.Id,
            IsPrivate = false,
            Creator = "user1"
        };

        await _service.AddInvItem(entity);
        var all = await _service.GetInvItemEntities("user1");
        all.Count.ShouldBe(1);

        await _service.DeleteInvItem(entity.Id);

        var afterDelete = await _service.GetInvItemEntities("user1");
        afterDelete.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteInvItem_NonExistentId_DoesNotThrow()
    {
        await Should.NotThrowAsync(() => _service.DeleteInvItem(9999));
    }

    // ═══════════════════════════════════════════════════════════
    // ── Order (nivel padre-hijo, estilo Opex) ────────────────
    // ═══════════════════════════════════════════════════════════

    // ── GetInvOrderStatuses ──────────────────────────────────

    [TestMethod]
    public async Task GetInvOrderStatuses_ReturnsSeedData()
    {
        var statuses = await _service.GetInvOrderStatuses();

        statuses.Count.ShouldBe(4);
        statuses.ShouldContain(s => s.Name == "Planning" && s.Color == "blue");
        statuses.ShouldContain(s => s.Name == "In Progress" && s.Color == "gold");
        statuses.ShouldContain(s => s.Name == "Completed" && s.Color == "green");
        statuses.ShouldContain(s => s.Name == "Canceled" && s.Color == "red");
    }

    // ── Helpers ──────────────────────────────────────────────

    /// <summary>
    /// Helper que crea una InvOrderEntity padre y la devuelve con su Id asignado.
    /// </summary>
    private async Task<InvOrderEntity> CreateParentOrder(DateTime? purchaseDate = null)
    {
        var vendor = await CreateVendor();
        var order = new InvOrderEntity
        {
            PurchaseDate = purchaseDate ?? new DateTime(2026, 1, 15),
            ReceptionDate = new DateTime(2026, 2, 1),
            StatusId = 1,
            VendorId = vendor.Id,
            IsPrivate = false,
            Creator = "user1"
        };
        await _service.AddInvOrder(order);
        return order;
    }

    /// <summary>
    /// Helper que crea un InvItemEntity para poder asignar sub-entidades de orden.
    /// </summary>
    private async Task<InvItemEntity> CreateItem(int vendorId, string name = "Item Test")
    {
        var item = new InvItemEntity
        {
            Name = name,
            CurrentStock = 100,
            MinStock = 10,
            CategoryId = 1,
            StatusId = 1,
            VendorId = vendorId,
            IsPrivate = false,
            Creator = "user1"
        };
        await _service.AddInvItem(item);
        return item;
    }

    // ── AddInvOrder + GetInvOrderEntities ────────────────────

    [TestMethod]
    public async Task AddInvOrder_AndRetrieve_ReturnsEntity()
    {
        var order = await CreateParentOrder();

        var all = await _service.GetInvOrderEntities("user1");
        all.Count.ShouldBe(1);
        all[0].PurchaseDate.ShouldBe(new DateTime(2026, 1, 15));
    }

    // ── Privacy filtering (Orders) ───────────────────────────

    [TestMethod]
    public async Task GetInvOrderEntities_FiltersPrivateEntitiesFromOthers()
    {
        var vendor = await CreateVendor();

        // Orden pública de user1 — visible para todos
        await _service.AddInvOrder(new InvOrderEntity
        {
            PurchaseDate = DateTime.Now,
            ReceptionDate = DateTime.Now.AddDays(7),
            StatusId = 1,
            VendorId = vendor.Id,
            IsPrivate = false,
            Creator = "user1"
        });

        // Orden privada de user1 — solo visible para user1
        await _service.AddInvOrder(new InvOrderEntity
        {
            PurchaseDate = DateTime.Now,
            ReceptionDate = DateTime.Now.AddDays(7),
            StatusId = 1,
            VendorId = vendor.Id,
            IsPrivate = true,
            Creator = "user1"
        });

        var user1Results = await _service.GetInvOrderEntities("user1");
        user1Results.Count.ShouldBe(2);

        var user2Results = await _service.GetInvOrderEntities("user2");
        user2Results.Count.ShouldBe(1);
    }

    // ── UpdateInvOrder ───────────────────────────────────────

    [TestMethod]
    public async Task UpdateInvOrder_ModifiesEntity()
    {
        var order = await CreateParentOrder();

        order.StatusId = 3;
        order.ReceptionDate = new DateTime(2026, 5, 20);
        await _service.UpdateInvOrder(order);

        var all = await _service.GetInvOrderEntities("user1");
        all.Count.ShouldBe(1);
        all[0].StatusId.ShouldBe(3);
        all[0].ReceptionDate.ShouldBe(new DateTime(2026, 5, 20));
    }

    // ── DeleteInvOrder ───────────────────────────────────────

    [TestMethod]
    public async Task DeleteInvOrder_RemovesEntity()
    {
        var order = await CreateParentOrder();
        var all = await _service.GetInvOrderEntities("user1");
        all.Count.ShouldBe(1);

        await _service.DeleteInvOrder(order.Id);

        var afterDelete = await _service.GetInvOrderEntities("user1");
        afterDelete.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteInvOrder_NonExistentId_DoesNotThrow()
    {
        await Should.NotThrowAsync(() => _service.DeleteInvOrder(9999));
    }

    // ═══════════════════════════════════════════════════════════
    // ── InvOrderSubEntity CRUD (estilo Opex sub-entities) ────
    // ═══════════════════════════════════════════════════════════

    [TestMethod]
    public async Task AddInvOrderSubEntity_AndRetrieve_ReturnsSubEntity()
    {
        var order = await CreateParentOrder();
        var item = await CreateItem(order.VendorId);

        var sub = new InvOrderSubEntity
        {
            ItemId = item.Id,
            ItemCount = 10,
            Amount = 250.50,
            OrderId = order.Id
        };

        await _service.AddInvOrderSubEntity(sub);

        var subs = await _service.GetInvOrderSubEntities(order);
        subs.Count.ShouldBe(1);
        subs[0].Amount.ShouldBe(250.50);
        subs[0].ItemCount.ShouldBe(10);
        subs[0].OrderId.ShouldBe(order.Id);
    }

    [TestMethod]
    public async Task GetInvOrderSubEntities_FiltersOnlyByParentOrder()
    {
        var order1 = await CreateParentOrder();
        var order2 = await CreateParentOrder(new DateTime(2026, 3, 1));
        var item1 = await CreateItem(order1.VendorId, "Item A");
        var item2 = await CreateItem(order2.VendorId, "Item B");

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item1.Id,
            ItemCount = 5,
            Amount = 100,
            OrderId = order1.Id
        });

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item2.Id,
            ItemCount = 3,
            Amount = 200,
            OrderId = order2.Id
        });

        var subsOrder1 = await _service.GetInvOrderSubEntities(order1);
        subsOrder1.Count.ShouldBe(1);
        subsOrder1[0].Amount.ShouldBe(100);

        var subsOrder2 = await _service.GetInvOrderSubEntities(order2);
        subsOrder2.Count.ShouldBe(1);
        subsOrder2[0].Amount.ShouldBe(200);
    }

    [TestMethod]
    public async Task UpdateInvOrderSubEntity_ModifiesSubEntity()
    {
        var order = await CreateParentOrder();
        var item = await CreateItem(order.VendorId);

        var sub = new InvOrderSubEntity
        {
            ItemId = item.Id,
            ItemCount = 5,
            Amount = 100,
            OrderId = order.Id
        };
        await _service.AddInvOrderSubEntity(sub);

        sub.Amount = 999.99;
        sub.ItemCount = 50;
        await _service.UpdateInvOrderSubEntity(sub);

        var subs = await _service.GetInvOrderSubEntities(order);
        subs.Count.ShouldBe(1);
        subs[0].Amount.ShouldBe(999.99);
        subs[0].ItemCount.ShouldBe(50);
    }

    [TestMethod]
    public async Task DeleteInvOrderSubEntity_RemovesSubEntity()
    {
        var order = await CreateParentOrder();
        var item = await CreateItem(order.VendorId);

        var sub = new InvOrderSubEntity
        {
            ItemId = item.Id,
            ItemCount = 2,
            Amount = 50,
            OrderId = order.Id
        };
        await _service.AddInvOrderSubEntity(sub);

        var before = await _service.GetInvOrderSubEntities(order);
        before.Count.ShouldBe(1);

        await _service.DeleteInvOrderSubEntity(sub.Id);

        var after = await _service.GetInvOrderSubEntities(order);
        after.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteInvOrderSubEntity_NonExistentId_DoesNotThrow()
    {
        await Should.NotThrowAsync(() => _service.DeleteInvOrderSubEntity(9999));
    }

    // ═══════════════════════════════════════════════════════════
    // ── GetInvOrderStats (estilo Opex stats) ─────────────────
    // ═══════════════════════════════════════════════════════════

    [TestMethod]
    public async Task GetInvOrderStats_ReturnsCorrectCountAndTotal()
    {
        var order = await CreateParentOrder();
        var item = await CreateItem(order.VendorId);

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id,
            ItemCount = 2,
            Amount = 100.555,
            OrderId = order.Id
        });

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id,
            ItemCount = 3,
            Amount = 200.445,
            OrderId = order.Id
        });

        var stats = await _service.GetInvOrderStats(order.Id);
        stats.ItemCount.ShouldBe(2);
        // 100.555 + 200.445 = 301.00 — truncado a 2 decimales
        stats.TotalAmount.ShouldBe(301.00);
    }

    [TestMethod]
    public async Task GetInvOrderStats_WithNoSubEntities_ReturnsZeros()
    {
        var order = await CreateParentOrder();

        var stats = await _service.GetInvOrderStats(order.Id);
        stats.ItemCount.ShouldBe(0);
        stats.TotalAmount.ShouldBe(0);
    }

    [TestMethod]
    public async Task GetInvOrderStats_OnlyCountsSubEntitiesOfGivenOrder()
    {
        var order1 = await CreateParentOrder();
        var order2 = await CreateParentOrder(new DateTime(2026, 4, 1));
        var item1 = await CreateItem(order1.VendorId, "Item A");
        var item2 = await CreateItem(order2.VendorId, "Item B");

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item1.Id,
            ItemCount = 2,
            Amount = 50,
            OrderId = order1.Id
        });

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item1.Id,
            ItemCount = 1,
            Amount = 75,
            OrderId = order1.Id
        });

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item2.Id,
            ItemCount = 5,
            Amount = 300,
            OrderId = order2.Id
        });

        var stats1 = await _service.GetInvOrderStats(order1.Id);
        stats1.ItemCount.ShouldBe(2);
        stats1.TotalAmount.ShouldBe(125.00);

        var stats2 = await _service.GetInvOrderStats(order2.Id);
        stats2.ItemCount.ShouldBe(1);
        stats2.TotalAmount.ShouldBe(300.00);
    }

    [TestMethod]
    public async Task GetInvOrderStats_RoundsTotalToTwoDecimals()
    {
        var order = await CreateParentOrder();
        var item = await CreateItem(order.VendorId);

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id,
            ItemCount = 1,
            Amount = 33.336,
            OrderId = order.Id
        });

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id,
            ItemCount = 1,
            Amount = 33.336,
            OrderId = order.Id
        });

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id,
            ItemCount = 1,
            Amount = 33.336,
            OrderId = order.Id
        });

        var stats = await _service.GetInvOrderStats(order.Id);
        stats.ItemCount.ShouldBe(3);
        // 33.336 * 3 = 100.008 → redondeado a 100.01
        stats.TotalAmount.ShouldBe(100.01);
    }

    // ═══════════════════════════════════════════════════════════
    // ── GetInvVendorStats ────────────────────────────────────
    // ═══════════════════════════════════════════════════════════

    [TestMethod]
    public async Task GetInvVendorStats_ReturnsCorrectOrderCountAndTotal()
    {
        var vendor = await CreateVendor();
        var item = await CreateItem(vendor.Id);

        // Crear dos órdenes para el mismo vendor
        var order1 = new InvOrderEntity
        {
            PurchaseDate = new DateTime(2026, 1, 1),
            ReceptionDate = new DateTime(2026, 1, 15),
            StatusId = 1,
            VendorId = vendor.Id,
            IsPrivate = false,
            Creator = "user1"
        };
        await _service.AddInvOrder(order1);

        var order2 = new InvOrderEntity
        {
            PurchaseDate = new DateTime(2026, 2, 1),
            ReceptionDate = new DateTime(2026, 2, 15),
            StatusId = 1,
            VendorId = vendor.Id,
            IsPrivate = false,
            Creator = "user1"
        };
        await _service.AddInvOrder(order2);

        // Sub-entidades en cada orden
        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id,
            ItemCount = 5,
            Amount = 100,
            OrderId = order1.Id
        });

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id,
            ItemCount = 3,
            Amount = 200,
            OrderId = order2.Id
        });

        var stats = await _service.GetInvVendorStats(vendor.Id);
        stats.OrderCount.ShouldBe(2);
        stats.TotalAmount.ShouldBe(300.00);
    }

    [TestMethod]
    public async Task GetInvVendorStats_WithNoOrders_ReturnsZeros()
    {
        var vendor = await CreateVendor();

        var stats = await _service.GetInvVendorStats(vendor.Id);
        stats.OrderCount.ShouldBe(0);
        stats.TotalAmount.ShouldBe(0);
    }

    [TestMethod]
    public async Task GetInvVendorStats_OnlyCountsOrdersOfGivenVendor()
    {
        var vendorA = await CreateVendor("Vendor A");
        var vendorB = await CreateVendor("Vendor B");
        var itemA = await CreateItem(vendorA.Id, "Item A");
        var itemB = await CreateItem(vendorB.Id, "Item B");

        var orderA = new InvOrderEntity
        {
            PurchaseDate = DateTime.Now,
            ReceptionDate = DateTime.Now.AddDays(7),
            StatusId = 1,
            VendorId = vendorA.Id,
            IsPrivate = false,
            Creator = "user1"
        };
        await _service.AddInvOrder(orderA);

        var orderB = new InvOrderEntity
        {
            PurchaseDate = DateTime.Now,
            ReceptionDate = DateTime.Now.AddDays(7),
            StatusId = 1,
            VendorId = vendorB.Id,
            IsPrivate = false,
            Creator = "user1"
        };
        await _service.AddInvOrder(orderB);

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = itemA.Id,
            ItemCount = 1,
            Amount = 50,
            OrderId = orderA.Id
        });

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = itemB.Id,
            ItemCount = 2,
            Amount = 400,
            OrderId = orderB.Id
        });

        var statsA = await _service.GetInvVendorStats(vendorA.Id);
        statsA.OrderCount.ShouldBe(1);
        statsA.TotalAmount.ShouldBe(50.00);

        var statsB = await _service.GetInvVendorStats(vendorB.Id);
        statsB.OrderCount.ShouldBe(1);
        statsB.TotalAmount.ShouldBe(400.00);
    }

    [TestMethod]
    public async Task GetInvVendorStats_RoundsTotalToTwoDecimals()
    {
        var vendor = await CreateVendor();
        var item = await CreateItem(vendor.Id);

        var order = new InvOrderEntity
        {
            PurchaseDate = DateTime.Now,
            ReceptionDate = DateTime.Now.AddDays(7),
            StatusId = 1,
            VendorId = vendor.Id,
            IsPrivate = false,
            Creator = "user1"
        };
        await _service.AddInvOrder(order);

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id,
            ItemCount = 1,
            Amount = 33.336,
            OrderId = order.Id
        });

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id,
            ItemCount = 1,
            Amount = 33.336,
            OrderId = order.Id
        });

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id,
            ItemCount = 1,
            Amount = 33.336,
            OrderId = order.Id
        });

        var stats = await _service.GetInvVendorStats(vendor.Id);
        stats.OrderCount.ShouldBe(1);
        // 33.336 * 3 = 100.008 → redondeado a 100.01
        stats.TotalAmount.ShouldBe(100.01);
    }

    // ═══════════════════════════════════════════════════════════
    // ── GetShoppingList (ítems con stock bajo) ──────────────
    // ═══════════════════════════════════════════════════════════

    [TestMethod]
    public async Task GetShoppingList_ReturnsOnlyItemsBelowMinStock()
    {
        var vendor = await CreateVendor();

        // Item con stock suficiente — no debe aparecer
        await _service.AddInvItem(new InvItemEntity
        {
            Name = "Suficiente",
            CurrentStock = 20,
            MinStock = 10,
            CategoryId = 1,
            StatusId = 1,
            VendorId = vendor.Id,
            IsPrivate = false,
            Creator = "user1"
        });

        // Item con stock bajo — debe aparecer
        await _service.AddInvItem(new InvItemEntity
        {
            Name = "Faltante",
            CurrentStock = 3,
            MinStock = 10,
            CategoryId = 1,
            StatusId = 1,
            VendorId = vendor.Id,
            IsPrivate = false,
            Creator = "user1"
        });

        var list = await _service.GetShoppingList("user1");
        list.Count.ShouldBe(1);
        list[0].Name.ShouldBe("Faltante");
    }

    [TestMethod]
    public async Task GetShoppingList_ExcludesItemsWithZeroMinStock()
    {
        var vendor = await CreateVendor();

        // Item sin mínimo configurado — no debe aparecer aunque CurrentStock sea 0
        await _service.AddInvItem(new InvItemEntity
        {
            Name = "Sin minimo",
            CurrentStock = 0,
            MinStock = 0,
            CategoryId = 1,
            StatusId = 1,
            VendorId = vendor.Id,
            IsPrivate = false,
            Creator = "user1"
        });

        var list = await _service.GetShoppingList("user1");
        list.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task GetShoppingList_RespectsPrivacyFilter()
    {
        var vendor = await CreateVendor();

        // Item privado de user1 con stock bajo
        await _service.AddInvItem(new InvItemEntity
        {
            Name = "Privado bajo",
            CurrentStock = 1,
            MinStock = 10,
            CategoryId = 1,
            StatusId = 1,
            VendorId = vendor.Id,
            IsPrivate = true,
            Creator = "user1"
        });

        // Item público con stock bajo
        await _service.AddInvItem(new InvItemEntity
        {
            Name = "Publico bajo",
            CurrentStock = 2,
            MinStock = 10,
            CategoryId = 1,
            StatusId = 1,
            VendorId = vendor.Id,
            IsPrivate = false,
            Creator = "user1"
        });

        // user1 ve ambos
        var user1List = await _service.GetShoppingList("user1");
        user1List.Count.ShouldBe(2);

        // user2 solo ve el público
        var user2List = await _service.GetShoppingList("user2");
        user2List.Count.ShouldBe(1);
        user2List[0].Name.ShouldBe("Publico bajo");
    }

    [TestMethod]
    public async Task GetShoppingList_IsOrderedByVendorThenName()
    {
        var vendorA = await CreateVendor("Vendor A");
        var vendorB = await CreateVendor("Vendor B");

        await _service.AddInvItem(new InvItemEntity
        {
            Name = "Zeta",
            CurrentStock = 0,
            MinStock = 5,
            CategoryId = 1,
            StatusId = 1,
            VendorId = vendorA.Id,
            IsPrivate = false,
            Creator = "user1"
        });

        await _service.AddInvItem(new InvItemEntity
        {
            Name = "Alfa",
            CurrentStock = 0,
            MinStock = 5,
            CategoryId = 1,
            StatusId = 1,
            VendorId = vendorA.Id,
            IsPrivate = false,
            Creator = "user1"
        });

        await _service.AddInvItem(new InvItemEntity
        {
            Name = "Beta",
            CurrentStock = 1,
            MinStock = 10,
            CategoryId = 1,
            StatusId = 1,
            VendorId = vendorB.Id,
            IsPrivate = false,
            Creator = "user1"
        });

        var list = await _service.GetShoppingList("user1");
        list.Count.ShouldBe(3);

        // Primero vendorA (Id menor), ordenados por nombre
        list[0].Name.ShouldBe("Alfa");
        list[1].Name.ShouldBe("Zeta");
        // Luego vendorB
        list[2].Name.ShouldBe("Beta");
    }

    [TestMethod]
    public async Task GetShoppingList_ReturnsEmpty_WhenAllItemsHaveSufficientStock()
    {
        var vendor = await CreateVendor();

        await _service.AddInvItem(new InvItemEntity
        {
            Name = "Ok",
            CurrentStock = 50,
            MinStock = 10,
            CategoryId = 1,
            StatusId = 1,
            VendorId = vendor.Id,
            IsPrivate = false,
            Creator = "user1"
        });

        var list = await _service.GetShoppingList("user1");
        list.ShouldBeEmpty();
    }

    // ═══════════════════════════════════════════════════════════
    // ── GetInvItemPriceHistory ──────────────────────────────
    // ═══════════════════════════════════════════════════════════

    [TestMethod]
    public async Task GetInvItemPriceHistory_ReturnsCorrectUnitPrice()
    {
        var order = await CreateParentOrder();
        var item = await CreateItem(order.VendorId);

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id,
            ItemCount = 4,
            Amount = 100.00,
            OrderId = order.Id
        });

        var history = await _service.GetInvItemPriceHistory(item.Id, "user1");
        history.Count.ShouldBe(1);
        history[0].UnitPrice.ShouldBe(25.00);
        history[0].TotalAmount.ShouldBe(100.00);
        history[0].ItemCount.ShouldBe(4);
    }

    [TestMethod]
    public async Task GetInvItemPriceHistory_RoundsUnitPriceToTwoDecimals()
    {
        var order = await CreateParentOrder();
        var item = await CreateItem(order.VendorId);

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id,
            ItemCount = 3,
            Amount = 100.00,
            OrderId = order.Id
        });

        var history = await _service.GetInvItemPriceHistory(item.Id, "user1");
        history.Count.ShouldBe(1);
        // 100 / 3 = 33.333... → redondeado a 33.33
        history[0].UnitPrice.ShouldBe(33.33);
    }

    [TestMethod]
    public async Task GetInvItemPriceHistory_IgnoresZeroItemCount()
    {
        var order = await CreateParentOrder();
        var item = await CreateItem(order.VendorId);

        // Entrada con ItemCount == 0 — debe ignorarse
        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id,
            ItemCount = 0,
            Amount = 50.00,
            OrderId = order.Id
        });

        // Entrada válida
        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id,
            ItemCount = 2,
            Amount = 80.00,
            OrderId = order.Id
        });

        var history = await _service.GetInvItemPriceHistory(item.Id, "user1");
        history.Count.ShouldBe(1);
        history[0].UnitPrice.ShouldBe(40.00);
    }

    [TestMethod]
    public async Task GetInvItemPriceHistory_RespectsPrivacyOfParentOrder()
    {
        var vendor = await CreateVendor();
        var item = await CreateItem(vendor.Id);

        // Orden pública de user1
        var publicOrder = new InvOrderEntity
        {
            PurchaseDate = new DateTime(2026, 1, 10),
            ReceptionDate = new DateTime(2026, 1, 20),
            StatusId = 1,
            VendorId = vendor.Id,
            IsPrivate = false,
            Creator = "user1"
        };
        await _service.AddInvOrder(publicOrder);

        // Orden privada de user1
        var privateOrder = new InvOrderEntity
        {
            PurchaseDate = new DateTime(2026, 2, 10),
            ReceptionDate = new DateTime(2026, 2, 20),
            StatusId = 1,
            VendorId = vendor.Id,
            IsPrivate = true,
            Creator = "user1"
        };
        await _service.AddInvOrder(privateOrder);

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id, ItemCount = 5, Amount = 50, OrderId = publicOrder.Id
        });

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id, ItemCount = 3, Amount = 90, OrderId = privateOrder.Id
        });

        // user1 ve ambas
        var user1History = await _service.GetInvItemPriceHistory(item.Id, "user1");
        user1History.Count.ShouldBe(2);

        // user2 solo ve la pública
        var user2History = await _service.GetInvItemPriceHistory(item.Id, "user2");
        user2History.Count.ShouldBe(1);
        user2History[0].UnitPrice.ShouldBe(10.00);
    }

    [TestMethod]
    public async Task GetInvItemPriceHistory_ReturnsEmptyWhenNoOrders()
    {
        var vendor = await CreateVendor();
        var item = await CreateItem(vendor.Id);

        var history = await _service.GetInvItemPriceHistory(item.Id, "user1");
        history.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task GetInvItemPriceHistory_OnlyReturnsEntriesForGivenItem()
    {
        var order = await CreateParentOrder();
        var itemA = await CreateItem(order.VendorId, "Item A");
        var itemB = await CreateItem(order.VendorId, "Item B");

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = itemA.Id, ItemCount = 2, Amount = 40, OrderId = order.Id
        });

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = itemB.Id, ItemCount = 5, Amount = 100, OrderId = order.Id
        });

        var historyA = await _service.GetInvItemPriceHistory(itemA.Id, "user1");
        historyA.Count.ShouldBe(1);
        historyA[0].UnitPrice.ShouldBe(20.00);

        var historyB = await _service.GetInvItemPriceHistory(itemB.Id, "user1");
        historyB.Count.ShouldBe(1);
        historyB[0].UnitPrice.ShouldBe(20.00);
    }

    [TestMethod]
    public async Task GetInvItemPriceHistory_OrderedByPurchaseDateDescending()
    {
        var vendor = await CreateVendor();
        var item = await CreateItem(vendor.Id);

        var olderOrder = new InvOrderEntity
        {
            PurchaseDate = new DateTime(2025, 6, 1),
            ReceptionDate = new DateTime(2025, 6, 15),
            StatusId = 1, VendorId = vendor.Id,
            IsPrivate = false, Creator = "user1"
        };
        await _service.AddInvOrder(olderOrder);

        var newerOrder = new InvOrderEntity
        {
            PurchaseDate = new DateTime(2026, 3, 1),
            ReceptionDate = new DateTime(2026, 3, 15),
            StatusId = 1, VendorId = vendor.Id,
            IsPrivate = false, Creator = "user1"
        };
        await _service.AddInvOrder(newerOrder);

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id, ItemCount = 1, Amount = 10, OrderId = olderOrder.Id
        });

        await _service.AddInvOrderSubEntity(new InvOrderSubEntity
        {
            ItemId = item.Id, ItemCount = 1, Amount = 15, OrderId = newerOrder.Id
        });

        var history = await _service.GetInvItemPriceHistory(item.Id, "user1");
        history.Count.ShouldBe(2);
        // El más reciente primero
        history[0].PurchaseDate.ShouldBe(new DateTime(2026, 3, 1));
        history[1].PurchaseDate.ShouldBe(new DateTime(2025, 6, 1));
    }
}

