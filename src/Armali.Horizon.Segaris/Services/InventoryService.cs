using Armali.Horizon.Segaris.Model;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Segaris.Services;

public class InventoryService
{
    private readonly IDbContextFactory<SegarisDbContext> Factory;

    public InventoryService(IDbContextFactory<SegarisDbContext> factory)
    {
        Factory = factory;
    }

    public async Task<List<InvVendorStatus>> GetInvVendorStatuses()
    {
        await using var context = Factory.CreateDbContext();
        return await context.InvVendorStatuses
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<InvVendorEntity>> GetInvVendorEntities(string userId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.InvVendorEntities
            .Where(e => !e.IsPrivate || e.Creator == userId)
            .OrderBy(e => e.StatusId)
            .ThenBy(e => e.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddInvVendor(InvVendorEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.InvVendorEntities.Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateInvVendor(InvVendorEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.InvVendorEntities.Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteInvVendor(int id)
    {
        await using var context = Factory.CreateDbContext();
        var Entity = await context.InvVendorEntities.FindAsync(id);
        if (Entity != null)
        {
            context.InvVendorEntities.Remove(Entity);
            await context.SaveChangesAsync();
        }
    }
    
    public async Task<List<InvItemCategory>> GetInvItemCategories()
    {
        await using var context = Factory.CreateDbContext();
        return await context.InvItemCategories
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task<List<InvItemStatus>> GetInvItemStatuses()
    {
        await using var context = Factory.CreateDbContext();
        return await context.InvItemStatuses
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<InvItemEntity>> GetInvItemEntities(string userId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.InvItemEntities
            .Where(e => !e.IsPrivate || e.Creator == userId)
            .OrderBy(e => e.CategoryId)
            .ThenBy(e => e.Name)
            .AsNoTracking()
            .ToListAsync();
    }
    
    /// Retorna ítems cuyo CurrentStock < MinStock (y MinStock > 0), respetando privacidad.
    public async Task<List<InvItemEntity>> GetShoppingList(string userId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.InvItemEntities
            .Where(e => e.MinStock > 0 && e.CurrentStock < e.MinStock)
            .Where(e => !e.IsPrivate || e.Creator == userId)
            .OrderBy(e => e.VendorId)
            .ThenBy(e => e.Name)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<InvItemEntity>> GetInvItemByVendor(int vendorId, string userId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.InvItemEntities
            .Where(e => e.VendorId == vendorId && (!e.IsPrivate || e.Creator == userId))
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddInvItem(InvItemEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.InvItemEntities.Add(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateInvItem(InvItemEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.InvItemEntities.Update(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteInvItem(int id)
    {
        await using var context = Factory.CreateDbContext();
        var Entity = await context.InvItemEntities.FindAsync(id);
        if (Entity != null)
        {
            context.InvItemEntities.Remove(Entity);
            await context.SaveChangesAsync();
        }
    }
    
    public async Task<List<InvOrderStatus>> GetInvOrderStatuses()
    {
        await using var context = Factory.CreateDbContext();
        return await context.InvOrderStatuses
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<InvOrderEntity>> GetInvOrderEntities(string userId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.InvOrderEntities
            .Where(e => !e.IsPrivate || e.Creator == userId)
            .OrderByDescending(e => e.PurchaseDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task AddInvOrder(InvOrderEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.InvOrderEntities.Add(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateInvOrder(InvOrderEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.InvOrderEntities.Update(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteInvOrder(int id)
    {
        await using var context = Factory.CreateDbContext();
        var Entity = await context.InvOrderEntities.FindAsync(id);
        if (Entity != null)
        {
            context.InvOrderEntities.Remove(Entity);
            await context.SaveChangesAsync();
        }
    }
    
    public async Task<List<InvOrderSubEntity>> GetInvOrderSubEntities(InvOrderEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        return await context.InvOrderSubEntities
            .Where(e => e.OrderId == entity.Id)
            .AsNoTracking()
            .ToListAsync();
    }
    
    public async Task AddInvOrderSubEntity(InvOrderSubEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.InvOrderSubEntities.Add(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task UpdateInvOrderSubEntity(InvOrderSubEntity entity)
    {
        await using var context = Factory.CreateDbContext();
        context.InvOrderSubEntities.Update(entity);
        await context.SaveChangesAsync();
    }
    
    public async Task DeleteInvOrderSubEntity(int id)
    {
        await using var context = Factory.CreateDbContext();
        var SubEntity = await context.InvOrderSubEntities.FindAsync(id);
        if (SubEntity != null)
        {
            context.InvOrderSubEntities.Remove(SubEntity);
            await context.SaveChangesAsync();
        }
    }
    
    public async Task<InvVendorStats> GetInvVendorStats(int id)
    {
        await using var context = Factory.CreateDbContext();
        var result = new InvVendorStats
        {
            OrderCount = await context.InvOrderEntities.CountAsync(e => e.VendorId == id),
            TotalAmount = await context.InvOrderSubEntities
                .Where(e => e.Order != null && e.Order.VendorId == id)
                .SumAsync(e => e.Amount)
        };
        
        // Truncate amount to two decimal places
        result.TotalAmount = Math.Round(result.TotalAmount, 2);
        
        return result;
    }
    
    public async Task<InvOrderStats> GetInvOrderStats(int id)
    {
        await using var context = Factory.CreateDbContext();
        var result = new InvOrderStats
        {
            ItemCount = await context.InvOrderSubEntities.CountAsync(e => e.OrderId == id),
            TotalAmount = await context.InvOrderSubEntities
                .Where(e => e.OrderId == id)
                .SumAsync(e => e.Amount)
        };
        
        // Truncate amount to two decimal places
        result.TotalAmount = Math.Round(result.TotalAmount, 2);
        
        return result;
    }
    
    /// Retorna el historial de precios unitarios de un ítem a través de todos los pedidos,
    /// respetando la privacidad del pedido padre. Ignora entradas con ItemCount == 0.
    public async Task<List<InvItemPriceHistory>> GetInvItemPriceHistory(int itemId, string userId)
    {
        await using var context = Factory.CreateDbContext();
        return await context.InvOrderSubEntities
            .Where(s => s.ItemId == itemId && s.ItemCount > 0)
            .Join(
                context.InvOrderEntities.Where(o => !o.IsPrivate || o.Creator == userId),
                s => s.OrderId,
                o => o.Id,
                (s, o) => new InvItemPriceHistory
                {
                    Id = s.Id,
                    PurchaseDate = o.PurchaseDate,
                    VendorId = o.VendorId,
                    ItemCount = s.ItemCount,
                    TotalAmount = Math.Round(s.Amount, 2),
                    UnitPrice = Math.Round(s.Amount / s.ItemCount, 2)
                })
            .OrderByDescending(h => h.PurchaseDate)
            .AsNoTracking()
            .ToListAsync();
    }
}