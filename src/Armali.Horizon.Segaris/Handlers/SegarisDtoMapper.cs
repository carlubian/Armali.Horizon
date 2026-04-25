using Armali.Horizon.Contracts.Segaris;
using Armali.Horizon.Segaris.Model;

namespace Armali.Horizon.Segaris.Handlers;

/// <summary>
/// Mapeos entidad → DTO para el bus IO. Mantiene los handlers livianos
/// y evita exponer las nav properties de EF.
/// </summary>
internal static class SegarisDtoMapper
{
    public static SegarisRefDto ToDto(this ProjectProgram p) => new() { Id = p.Id, Name = p.Name };
    public static ProjectAxisDto ToDto(this ProjectAxis a) => new() { Id = a.Id, Name = a.Name, ProgramId = a.ProgramId };
    public static SegarisStatusDto ToDto(this ProjectStatus s) => new() { Id = s.Id, Name = s.Name, Color = s.Color };
    public static SegarisRefDto ToDto(this ProjectSubEntityCategory c) => new() { Id = c.Id, Name = c.Name };
    public static SegarisRefDto ToDto(this ProjectRiskCategory c) => new() { Id = c.Id, Name = c.Name };
    
    public static ProjectEntityDto ToDto(this ProjectEntity e) => new()
    {
        Id = e.Id, Name = e.Name, Code = e.Code,
        ProgramId = e.ProgramId, AxisId = e.AxisId, StatusId = e.StatusId,
        IsPrivate = e.IsPrivate, Creator = e.Creator,
    };
    
    public static ProjectSubEntityDto ToDto(this ProjectSubEntity e) => new()
    {
        Id = e.Id, Name = e.Name, Date = e.Date, File = e.File,
        CategoryId = e.CategoryId, ProjectId = e.ProjectId,
    };
    
    public static ProjectRiskElementDto ToDto(this ProjectRiskElement e) => new()
    {
        Id = e.Id, Name = e.Name, CategoryId = e.CategoryId,
        Probability = e.Probability, Severity = e.Severity, Mitigation = e.Mitigation,
        ProjectId = e.ProjectId, Score = e.Score,
    };
    
    public static ProjectBudgetDto ToDto(this ProjectBudget e) => new()
    {
        Id = e.Id, Year = e.Year, Estimated = e.Estimated, Actual = e.Actual,
        ProjectId = e.ProjectId, SpentPercent = e.SpentPercent,
    };
    
    // ── Asset ───────────────────────────────────────────────────────────
    public static SegarisRefDto ToDto(this AssetCategory c) => new() { Id = c.Id, Name = c.Name };
    public static SegarisStatusDto ToDto(this AssetStatus s) => new() { Id = s.Id, Name = s.Name, Color = s.Color };
    public static AssetEntityDto ToDto(this AssetEntity e) => new()
    {
        Id = e.Id, Name = e.Name, AssetCode = e.AssetCode, Location = e.Location,
        Date = e.Date, CategoryId = e.CategoryId, StatusId = e.StatusId,
        ProjectId = e.ProjectId, IsPrivate = e.IsPrivate, Creator = e.Creator,
    };
    
    // ── Capex ───────────────────────────────────────────────────────────
    public static SegarisRefDto ToDto(this CapexCategory c) => new() { Id = c.Id, Name = c.Name };
    public static SegarisStatusDto ToDto(this CapexStatus s) => new() { Id = s.Id, Name = s.Name, Color = s.Color };
    public static CapexEntityDto ToDto(this CapexEntity e) => new()
    {
        Id = e.Id, Name = e.Name, Date = e.Date, Amount = e.Amount,
        CategoryId = e.CategoryId, StatusId = e.StatusId,
        ProjectId = e.ProjectId, IsPrivate = e.IsPrivate, Creator = e.Creator,
    };
    
    // ── Opex ────────────────────────────────────────────────────────────
    public static SegarisRefDto ToDto(this OpexCategory c) => new() { Id = c.Id, Name = c.Name };
    public static SegarisStatusDto ToDto(this OpexStatus s) => new() { Id = s.Id, Name = s.Name, Color = s.Color };
    public static OpexEntityDto ToDto(this OpexEntity e) => new()
    {
        Id = e.Id, Name = e.Name, CategoryId = e.CategoryId, StatusId = e.StatusId,
        ProjectId = e.ProjectId, IsPrivate = e.IsPrivate, Creator = e.Creator,
    };
    public static OpexSubEntityDto ToDto(this OpexSubEntity e) => new()
    {
        Id = e.Id, Date = e.Date, Amount = e.Amount, ContractId = e.ContractId,
    };
    public static OpexStatsDto ToDto(this OpexStats s) => new()
    {
        SubEntityCount = s.SubEntityCount, TotalAmount = s.TotalAmount,
    };
    
    // ── Travel ──────────────────────────────────────────────────────────
    public static SegarisRefDto ToDto(this TravelCategory c) => new() { Id = c.Id, Name = c.Name };
    public static SegarisRefDto ToDto(this TravelCostCenter c) => new() { Id = c.Id, Name = c.Name };
    public static SegarisStatusDto ToDto(this TravelStatus s) => new() { Id = s.Id, Name = s.Name, Color = s.Color };
    public static SegarisRefDto ToDto(this TravelSubEntityCategory c) => new() { Id = c.Id, Name = c.Name };
    public static TravelEntityDto ToDto(this TravelEntity e) => new()
    {
        Id = e.Id, Name = e.Name, CategoryId = e.CategoryId, StatusId = e.StatusId,
        CostCenterId = e.CostCenterId, Destination = e.Destination,
        StartDate = e.StartDate, EndDate = e.EndDate, Pax = e.Pax,
        ProjectId = e.ProjectId, IsPrivate = e.IsPrivate, Creator = e.Creator,
    };
    public static TravelSubEntityDto ToDto(this TravelSubEntity e) => new()
    {
        Id = e.Id, Name = e.Name, CategoryId = e.CategoryId,
        Date = e.Date, Amount = e.Amount, TravelId = e.TravelId,
    };
    
    // ── Maintenance ─────────────────────────────────────────────────────
    public static SegarisRefDto ToDto(this MaintCategory c) => new() { Id = c.Id, Name = c.Name };
    public static SegarisStatusDto ToDto(this MaintStatus s) => new() { Id = s.Id, Name = s.Name, Color = s.Color };
    public static MaintEntityDto ToDto(this MaintEntity e) => new()
    {
        Id = e.Id, Name = e.Name, Date = e.Date, Details = e.Details,
        CategoryId = e.CategoryId, StatusId = e.StatusId, AssetId = e.AssetId,
        IsPrivate = e.IsPrivate, Creator = e.Creator,
    };
    
    // ── Inventory ───────────────────────────────────────────────────────
    public static SegarisStatusDto ToDto(this InvVendorStatus s) => new() { Id = s.Id, Name = s.Name, Color = s.Color };
    public static SegarisStatusDto ToDto(this InvItemStatus s) => new() { Id = s.Id, Name = s.Name, Color = s.Color };
    public static SegarisStatusDto ToDto(this InvOrderStatus s) => new() { Id = s.Id, Name = s.Name, Color = s.Color };
    public static SegarisRefDto ToDto(this InvItemCategory c) => new() { Id = c.Id, Name = c.Name };
    public static InvVendorEntityDto ToDto(this InvVendorEntity e) => new()
    {
        Id = e.Id, Name = e.Name, StatusId = e.StatusId,
        IsPrivate = e.IsPrivate, Creator = e.Creator,
    };
    public static InvItemEntityDto ToDto(this InvItemEntity e) => new()
    {
        Id = e.Id, Name = e.Name, CurrentStock = e.CurrentStock, MinStock = e.MinStock,
        CategoryId = e.CategoryId, StatusId = e.StatusId, VendorId = e.VendorId,
        IsPrivate = e.IsPrivate, Creator = e.Creator,
    };
    public static InvOrderEntityDto ToDto(this InvOrderEntity e) => new()
    {
        Id = e.Id, PurchaseDate = e.PurchaseDate, ReceptionDate = e.ReceptionDate,
        StatusId = e.StatusId, VendorId = e.VendorId,
        IsPrivate = e.IsPrivate, Creator = e.Creator,
    };
    public static InvOrderSubEntityDto ToDto(this InvOrderSubEntity e) => new()
    {
        Id = e.Id, ItemId = e.ItemId, ItemCount = e.ItemCount,
        Amount = e.Amount, OrderId = e.OrderId, ProjectId = e.ProjectId,
    };
    public static InvVendorStatsDto ToDto(this InvVendorStats s) => new()
    {
        OrderCount = s.OrderCount, TotalAmount = s.TotalAmount,
    };
    public static InvOrderStatsDto ToDto(this InvOrderStats s) => new()
    {
        ItemCount = s.ItemCount, TotalAmount = s.TotalAmount,
    };
    public static InvItemPriceHistoryDto ToDto(this InvItemPriceHistory h) => new()
    {
        Id = h.Id, PurchaseDate = h.PurchaseDate, VendorId = h.VendorId,
        ItemCount = h.ItemCount, TotalAmount = h.TotalAmount, UnitPrice = h.UnitPrice,
    };
    
    // ── Clothes ────────────────────────────────────────────────────────────
    public static SegarisRefDto ToDto(this ClothesCategory c) => new() { Id = c.Id, Name = c.Name };
    public static SegarisStatusDto ToDto(this ClothesStatus s) => new() { Id = s.Id, Name = s.Name, Color = s.Color };
    public static SegarisRefDto ToDto(this ClothesWashType w) => new() { Id = w.Id, Name = w.Name };
    public static ClothesColorDto ToDto(this ClothesColor c) => new() { Id = c.Id, Name = c.Name, Reference = c.Reference };
    public static SegarisRefDto ToDto(this ClothesColorStyle s) => new() { Id = s.Id, Name = s.Name };
    public static ClothesEntityDto ToDto(this ClothesEntity e) => new()
    {
        Id = e.Id, Name = e.Name, Date = e.Date, GarmentCode = e.GarmentCode,
        CategoryId = e.CategoryId, StatusId = e.StatusId, WashTypeId = e.WashTypeId,
        IsPrivate = e.IsPrivate, Creator = e.Creator,
    };
    public static ClothesColorAssignmentDto ToDto(this ClothesColorAssignment a) => new()
    {
        Id = a.Id, GarmentId = a.GarmentId, ColorId = a.ColorId, StyleId = a.StyleId,
    };
    
    // ── Admin (Processes) ──────────────────────────────────────────────────
    public static SegarisRefDto ToDto(this AdminCategory c) => new() { Id = c.Id, Name = c.Name };
    public static AdminEntityDto ToDto(this AdminEntity e) => new()
    {
        Id = e.Id, Name = e.Name, CategoryId = e.CategoryId,
        IsPrivate = e.IsPrivate, Creator = e.Creator,
    };
    public static AdminSubEntityDto ToDto(this AdminSubEntity e) => new()
    {
        Id = e.Id, Name = e.Name, StartDate = e.StartDate, DueDate = e.DueDate,
        IsCompleted = e.IsCompleted, ProcessId = e.ProcessId,
    };
    public static AdminStatsDto ToDto(this AdminStats s) => new()
    {
        Finished = s.Finished, NotStarted = s.NotStarted, OnTime = s.OnTime,
        Delayed = s.Delayed, Total = s.Total, OverallColor = s.OverallColor,
        OverallName = s.OverallName, Summary = s.Summary,
    };
}
