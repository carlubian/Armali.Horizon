using Armali.Horizon.Segaris.Model;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Segaris.Services;

public class ExpenseService
{
    private readonly IDbContextFactory<SegarisDbContext> Factory;

    public ExpenseService(IDbContextFactory<SegarisDbContext> factory)
    {
        Factory = factory;
    }

    // ── Helpers ──────────────────────────────────────────────────────

    private static DateTime YearStart(int year) => new(year, 1, 1);
    private static DateTime YearEnd(int year) => new(year, 12, 31, 23, 59, 59);

    /// Garantiza que los 12 meses aparezcan aunque no haya datos.
    private static List<MonthlyAggregate> PadMonths(List<MonthlyAggregate> source)
    {
        var dict = source.ToDictionary(m => m.Month);
        return Enumerable.Range(1, 12)
            .Select(m => dict.TryGetValue(m, out var v)
                ? v
                : new MonthlyAggregate { Month = m })
            .ToList();
    }

    // ── Capex ────────────────────────────────────────────────────────

    public async Task<List<MonthlyAggregate>> GetCapexMonthly(int year, string userId)
    {
        await using var ctx = Factory.CreateDbContext();
        var start = YearStart(year);
        var end = YearEnd(year);

        var raw = await ctx.CapexEntities
            .Where(e => e.Date >= start && e.Date <= end)
            .Where(e => !e.IsPrivate || e.Creator == userId)
            .GroupBy(e => e.Date.Month)
            .Select(g => new MonthlyAggregate
            {
                Month = g.Key,
                Income = Math.Round(g.Where(x => x.Amount > 0).Sum(x => x.Amount), 2),
                Expense = Math.Round(Math.Abs(g.Where(x => x.Amount < 0).Sum(x => x.Amount)), 2)
            })
            .AsNoTracking()
            .ToListAsync();

        return PadMonths(raw);
    }

    public async Task<List<CategoryAggregate>> GetCapexByCategory(int year, string userId)
    {
        await using var ctx = Factory.CreateDbContext();
        var start = YearStart(year);
        var end = YearEnd(year);

        return await ctx.CapexEntities
            .Where(e => e.Date >= start && e.Date <= end)
            .Where(e => !e.IsPrivate || e.Creator == userId)
            .Join(ctx.CapexCategories, e => e.CategoryId, c => c.Id, (e, c) => new { c.Name, e.Amount })
            .GroupBy(x => x.Name)
            .Select(g => new CategoryAggregate
            {
                Name = g.Key,
                Total = Math.Round(g.Sum(x => Math.Abs(x.Amount)), 2)
            })
            .AsNoTracking()
            .ToListAsync();
    }

    // ── Opex (importes en SubEntity, categoría en Entity padre) ─────

    public async Task<List<MonthlyAggregate>> GetOpexMonthly(int year, string userId)
    {
        await using var ctx = Factory.CreateDbContext();
        var start = YearStart(year);
        var end = YearEnd(year);

        var raw = await ctx.OpexSubEntities
            .Where(s => s.Date >= start && s.Date <= end)
            .Join(ctx.OpexEntities.Where(e => !e.IsPrivate || e.Creator == userId),
                s => s.ContractId, e => e.Id, (s, _) => s)
            .GroupBy(s => s.Date.Month)
            .Select(g => new MonthlyAggregate
            {
                Month = g.Key,
                Income = Math.Round(g.Where(x => x.Amount > 0).Sum(x => x.Amount), 2),
                Expense = Math.Round(Math.Abs(g.Where(x => x.Amount < 0).Sum(x => x.Amount)), 2)
            })
            .AsNoTracking()
            .ToListAsync();

        return PadMonths(raw);
    }

    public async Task<List<CategoryAggregate>> GetOpexByCategory(int year, string userId)
    {
        await using var ctx = Factory.CreateDbContext();
        var start = YearStart(year);
        var end = YearEnd(year);

        return await ctx.OpexSubEntities
            .Where(s => s.Date >= start && s.Date <= end)
            .Join(ctx.OpexEntities.Where(e => !e.IsPrivate || e.Creator == userId),
                s => s.ContractId, e => e.Id, (s, e) => new { e.CategoryId, s.Amount })
            .Join(ctx.OpexCategories, x => x.CategoryId, c => c.Id, (x, c) => new { c.Name, x.Amount })
            .GroupBy(x => x.Name)
            .Select(g => new CategoryAggregate
            {
                Name = g.Key,
                Total = Math.Round(g.Sum(x => Math.Abs(x.Amount)), 2)
            })
            .AsNoTracking()
            .ToListAsync();
    }

    // ── Travel (importes en SubEntity, categoría de gasto = TravelSubEntityCategory) ─

    public async Task<List<MonthlyAggregate>> GetTravelMonthly(int year, string userId)
    {
        await using var ctx = Factory.CreateDbContext();
        var start = YearStart(year);
        var end = YearEnd(year);

        var raw = await ctx.TravelSubEntities
            .Where(s => s.Date >= start && s.Date <= end)
            .Join(ctx.TravelEntities.Where(e => !e.IsPrivate || e.Creator == userId),
                s => s.TravelId, e => e.Id, (s, _) => s)
            .GroupBy(s => s.Date.Month)
            .Select(g => new MonthlyAggregate
            {
                Month = g.Key,
                Income = Math.Round(g.Where(x => x.Amount > 0).Sum(x => x.Amount), 2),
                Expense = Math.Round(Math.Abs(g.Where(x => x.Amount < 0).Sum(x => x.Amount)), 2)
            })
            .AsNoTracking()
            .ToListAsync();

        return PadMonths(raw);
    }

    public async Task<List<CategoryAggregate>> GetTravelByExpenseCategory(int year, string userId)
    {
        await using var ctx = Factory.CreateDbContext();
        var start = YearStart(year);
        var end = YearEnd(year);

        return await ctx.TravelSubEntities
            .Where(s => s.Date >= start && s.Date <= end)
            .Join(ctx.TravelEntities.Where(e => !e.IsPrivate || e.Creator == userId),
                s => s.TravelId, e => e.Id, (s, _) => s)
            .Join(ctx.TravelSubEntityCategories, s => s.CategoryId, c => c.Id, (s, c) => new { c.Name, s.Amount })
            .GroupBy(x => x.Name)
            .Select(g => new CategoryAggregate
            {
                Name = g.Key,
                Total = Math.Round(g.Sum(x => Math.Abs(x.Amount)), 2)
            })
            .AsNoTracking()
            .ToListAsync();
    }

    // ── Inventory (solo gastos; importes en InvOrderSubEntity) ───────

    public async Task<List<MonthlyAggregate>> GetInventoryMonthly(int year, string userId)
    {
        await using var ctx = Factory.CreateDbContext();
        var start = YearStart(year);
        var end = YearEnd(year);

        // Se usa PurchaseDate del pedido padre como fecha de referencia
        var raw = await ctx.InvOrderSubEntities
            .Join(ctx.InvOrderEntities
                    .Where(o => o.PurchaseDate >= start && o.PurchaseDate <= end)
                    .Where(o => !o.IsPrivate || o.Creator == userId),
                s => s.OrderId, o => o.Id,
                (s, o) => new { o.PurchaseDate.Month, s.Amount })
            .GroupBy(x => x.Month)
            .Select(g => new MonthlyAggregate
            {
                Month = g.Key,
                Income = 0,
                Expense = Math.Round(Math.Abs(g.Sum(x => x.Amount)), 2)
            })
            .AsNoTracking()
            .ToListAsync();

        return PadMonths(raw);
    }

    public async Task<List<CategoryAggregate>> GetInventoryByVendor(int year, string userId)
    {
        await using var ctx = Factory.CreateDbContext();
        var start = YearStart(year);
        var end = YearEnd(year);

        return await ctx.InvOrderSubEntities
            .Join(ctx.InvOrderEntities
                    .Where(o => o.PurchaseDate >= start && o.PurchaseDate <= end)
                    .Where(o => !o.IsPrivate || o.Creator == userId),
                s => s.OrderId, o => o.Id,
                (s, o) => new { o.VendorId, s.Amount })
            .Join(ctx.InvVendorEntities, x => x.VendorId, v => v.Id,
                (x, v) => new { v.Name, x.Amount })
            .GroupBy(x => x.Name)
            .Select(g => new CategoryAggregate
            {
                Name = g.Key,
                Total = Math.Round(g.Sum(x => Math.Abs(x.Amount)), 2)
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<CategoryAggregate>> GetInventoryByItemCategory(int year, string userId)
    {
        await using var ctx = Factory.CreateDbContext();
        var start = YearStart(year);
        var end = YearEnd(year);

        return await ctx.InvOrderSubEntities
            .Join(ctx.InvOrderEntities
                    .Where(o => o.PurchaseDate >= start && o.PurchaseDate <= end)
                    .Where(o => !o.IsPrivate || o.Creator == userId),
                s => s.OrderId, o => o.Id,
                (s, _) => s)
            .Join(ctx.InvItemEntities, s => s.ItemId, i => i.Id,
                (s, i) => new { i.CategoryId, s.Amount })
            .Join(ctx.InvItemCategories, x => x.CategoryId, c => c.Id,
                (x, c) => new { c.Name, x.Amount })
            .GroupBy(x => x.Name)
            .Select(g => new CategoryAggregate
            {
                Name = g.Key,
                Total = Math.Round(g.Sum(x => Math.Abs(x.Amount)), 2)
            })
            .AsNoTracking()
            .ToListAsync();
    }
}

