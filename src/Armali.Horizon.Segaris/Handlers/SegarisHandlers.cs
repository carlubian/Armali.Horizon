using Armali.Horizon.Contracts.Identity;
using Armali.Horizon.Contracts.Segaris;
using Armali.Horizon.IO;
using Armali.Horizon.Segaris.Services;

namespace Armali.Horizon.Segaris.Handlers;

// ─────────────────────────────────────────────────────────────────────────────
// Handlers IO de Segaris (sólo lectura). Todos validan el token contra el
// servicio Identity y aplican la privacidad por usuario donde corresponda.
//
// Para añadir un nuevo handler de lectura:
//   1. Define Request/Response en Armali.Horizon.Contracts.Segaris.SegarisPayloads.
//   2. Añade el método correspondiente al service de dominio si no existe.
//   3. Crea el handler aquí siguiendo el patrón Auth → Service → Map → Response.
//   4. Registra el handler en Program.cs (UseHorizonEvents).
// ─────────────────────────────────────────────────────────────────────────────

// ── Project ─────────────────────────────────────────────────────────────────

public class ListProjectProgramsHandler(HorizonAuthClient identity, ProjectService svc)
    : IHorizonRequestHandler<ListProjectProgramsRequest, ListProjectProgramsResponse>
{
    public async Task<ListProjectProgramsResponse> HandleAsync(ListProjectProgramsRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListProjectProgramsResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetProjectPrograms();
        return new ListProjectProgramsResponse { Success = true, Programs = data.ConvertAll(p => p.ToDto()) };
    }
}

public class ListProjectAxesHandler(HorizonAuthClient identity, ProjectService svc)
    : IHorizonRequestHandler<ListProjectAxesRequest, ListProjectAxesResponse>
{
    public async Task<ListProjectAxesResponse> HandleAsync(ListProjectAxesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListProjectAxesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = req.ProgramId is { } pid ? await svc.GetProjectAxis(pid) : await svc.GetAllProjectAxis();
        return new ListProjectAxesResponse { Success = true, Axes = data.ConvertAll(a => a.ToDto()) };
    }
}

public class ListProjectStatusesHandler(HorizonAuthClient identity, ProjectService svc)
    : IHorizonRequestHandler<ListProjectStatusesRequest, ListProjectStatusesResponse>
{
    public async Task<ListProjectStatusesResponse> HandleAsync(ListProjectStatusesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListProjectStatusesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetProjectStatuses();
        return new ListProjectStatusesResponse { Success = true, Statuses = data.ConvertAll(s => s.ToDto()) };
    }
}

public class ListProjectsHandler(HorizonAuthClient identity, ProjectService svc)
    : IHorizonRequestHandler<ListProjectsRequest, ListProjectsResponse>
{
    public async Task<ListProjectsResponse> HandleAsync(ListProjectsRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListProjectsResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetProjectEntities(id.UserId);
        return new ListProjectsResponse { Success = true, Projects = data.ConvertAll(p => p.ToDto()) };
    }
}

public class ListProjectSubEntityCategoriesHandler(HorizonAuthClient identity, ProjectService svc)
    : IHorizonRequestHandler<ListProjectSubEntityCategoriesRequest, ListProjectSubEntityCategoriesResponse>
{
    public async Task<ListProjectSubEntityCategoriesResponse> HandleAsync(ListProjectSubEntityCategoriesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListProjectSubEntityCategoriesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetProjectSubEntityCategories();
        return new ListProjectSubEntityCategoriesResponse { Success = true, Categories = data.ConvertAll(c => c.ToDto()) };
    }
}

public class ListProjectSubEntitiesHandler(HorizonAuthClient identity, ProjectService svc)
    : IHorizonRequestHandler<ListProjectSubEntitiesRequest, ListProjectSubEntitiesResponse>
{
    public async Task<ListProjectSubEntitiesResponse> HandleAsync(ListProjectSubEntitiesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListProjectSubEntitiesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        // Pasamos un stub del proyecto: el servicio sólo usa el Id para filtrar.
        var data = await svc.GetProjectSubEntities(new() { Id = req.ProjectId });
        return new ListProjectSubEntitiesResponse { Success = true, SubEntities = data.ConvertAll(e => e.ToDto()) };
    }
}

public class ListProjectRiskCategoriesHandler(HorizonAuthClient identity, ProjectService svc)
    : IHorizonRequestHandler<ListProjectRiskCategoriesRequest, ListProjectRiskCategoriesResponse>
{
    public async Task<ListProjectRiskCategoriesResponse> HandleAsync(ListProjectRiskCategoriesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListProjectRiskCategoriesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetProjectRiskCategories();
        return new ListProjectRiskCategoriesResponse { Success = true, Categories = data.ConvertAll(c => c.ToDto()) };
    }
}

public class ListProjectRisksHandler(HorizonAuthClient identity, ProjectService svc)
    : IHorizonRequestHandler<ListProjectRisksRequest, ListProjectRisksResponse>
{
    public async Task<ListProjectRisksResponse> HandleAsync(ListProjectRisksRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListProjectRisksResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetProjectRiskElements(req.ProjectId);
        return new ListProjectRisksResponse { Success = true, Risks = data.ConvertAll(e => e.ToDto()) };
    }
}

public class ListProjectBudgetsHandler(HorizonAuthClient identity, ProjectService svc)
    : IHorizonRequestHandler<ListProjectBudgetsRequest, ListProjectBudgetsResponse>
{
    public async Task<ListProjectBudgetsResponse> HandleAsync(ListProjectBudgetsRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListProjectBudgetsResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetProjectBudgets(req.ProjectId);
        return new ListProjectBudgetsResponse { Success = true, Budgets = data.ConvertAll(b => b.ToDto()) };
    }
}

// ── Asset ───────────────────────────────────────────────────────────────────

public class ListAssetCategoriesHandler(HorizonAuthClient identity, AssetService svc)
    : IHorizonRequestHandler<ListAssetCategoriesRequest, ListAssetCategoriesResponse>
{
    public async Task<ListAssetCategoriesResponse> HandleAsync(ListAssetCategoriesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListAssetCategoriesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetAssetCategories();
        return new ListAssetCategoriesResponse { Success = true, Categories = data.ConvertAll(c => c.ToDto()) };
    }
}

public class ListAssetStatusesHandler(HorizonAuthClient identity, AssetService svc)
    : IHorizonRequestHandler<ListAssetStatusesRequest, ListAssetStatusesResponse>
{
    public async Task<ListAssetStatusesResponse> HandleAsync(ListAssetStatusesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListAssetStatusesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetAssetStatuses();
        return new ListAssetStatusesResponse { Success = true, Statuses = data.ConvertAll(s => s.ToDto()) };
    }
}

public class ListAssetsHandler(HorizonAuthClient identity, AssetService svc)
    : IHorizonRequestHandler<ListAssetsRequest, ListAssetsResponse>
{
    public async Task<ListAssetsResponse> HandleAsync(ListAssetsRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListAssetsResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetAssetEntities(id.UserId);
        return new ListAssetsResponse { Success = true, Assets = data.ConvertAll(e => e.ToDto()) };
    }
}

// ── Capex ───────────────────────────────────────────────────────────────────

public class ListCapexCategoriesHandler(HorizonAuthClient identity, CapexService svc)
    : IHorizonRequestHandler<ListCapexCategoriesRequest, ListCapexCategoriesResponse>
{
    public async Task<ListCapexCategoriesResponse> HandleAsync(ListCapexCategoriesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListCapexCategoriesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetCapexCategories();
        return new ListCapexCategoriesResponse { Success = true, Categories = data.ConvertAll(c => c.ToDto()) };
    }
}

public class ListCapexStatusesHandler(HorizonAuthClient identity, CapexService svc)
    : IHorizonRequestHandler<ListCapexStatusesRequest, ListCapexStatusesResponse>
{
    public async Task<ListCapexStatusesResponse> HandleAsync(ListCapexStatusesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListCapexStatusesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetCapexStatuses();
        return new ListCapexStatusesResponse { Success = true, Statuses = data.ConvertAll(s => s.ToDto()) };
    }
}

public class ListCapexHandler(HorizonAuthClient identity, CapexService svc)
    : IHorizonRequestHandler<ListCapexRequest, ListCapexResponse>
{
    public async Task<ListCapexResponse> HandleAsync(ListCapexRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListCapexResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetCapexEntities(id.UserId);
        return new ListCapexResponse { Success = true, Items = data.ConvertAll(e => e.ToDto()) };
    }
}

// ── Opex ────────────────────────────────────────────────────────────────────

public class ListOpexCategoriesHandler(HorizonAuthClient identity, OpexService svc)
    : IHorizonRequestHandler<ListOpexCategoriesRequest, ListOpexCategoriesResponse>
{
    public async Task<ListOpexCategoriesResponse> HandleAsync(ListOpexCategoriesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListOpexCategoriesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetOpexCategories();
        return new ListOpexCategoriesResponse { Success = true, Categories = data.ConvertAll(c => c.ToDto()) };
    }
}

public class ListOpexStatusesHandler(HorizonAuthClient identity, OpexService svc)
    : IHorizonRequestHandler<ListOpexStatusesRequest, ListOpexStatusesResponse>
{
    public async Task<ListOpexStatusesResponse> HandleAsync(ListOpexStatusesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListOpexStatusesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetOpexStatuses();
        return new ListOpexStatusesResponse { Success = true, Statuses = data.ConvertAll(s => s.ToDto()) };
    }
}

public class ListOpexHandler(HorizonAuthClient identity, OpexService svc)
    : IHorizonRequestHandler<ListOpexRequest, ListOpexResponse>
{
    public async Task<ListOpexResponse> HandleAsync(ListOpexRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListOpexResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetOpexEntities(id.UserId);
        return new ListOpexResponse { Success = true, Contracts = data.ConvertAll(e => e.ToDto()) };
    }
}

public class ListOpexSubEntitiesHandler(HorizonAuthClient identity, OpexService svc)
    : IHorizonRequestHandler<ListOpexSubEntitiesRequest, ListOpexSubEntitiesResponse>
{
    public async Task<ListOpexSubEntitiesResponse> HandleAsync(ListOpexSubEntitiesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListOpexSubEntitiesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetOpexSubEntities(new() { Id = req.ContractId });
        return new ListOpexSubEntitiesResponse { Success = true, Entries = data.ConvertAll(e => e.ToDto()) };
    }
}

public class GetOpexStatsHandler(HorizonAuthClient identity, OpexService svc)
    : IHorizonRequestHandler<GetOpexStatsRequest, GetOpexStatsResponse>
{
    public async Task<GetOpexStatsResponse> HandleAsync(GetOpexStatsRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new GetOpexStatsResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetOpexStats(req.ContractId);
        return new GetOpexStatsResponse { Success = true, Stats = data.ToDto() };
    }
}

// ── Travel ──────────────────────────────────────────────────────────────────

public class ListTravelCategoriesHandler(HorizonAuthClient identity, TravelService svc)
    : IHorizonRequestHandler<ListTravelCategoriesRequest, ListTravelCategoriesResponse>
{
    public async Task<ListTravelCategoriesResponse> HandleAsync(ListTravelCategoriesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListTravelCategoriesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetTravelCategories();
        return new ListTravelCategoriesResponse { Success = true, Categories = data.ConvertAll(c => c.ToDto()) };
    }
}

public class ListTravelCostCentersHandler(HorizonAuthClient identity, TravelService svc)
    : IHorizonRequestHandler<ListTravelCostCentersRequest, ListTravelCostCentersResponse>
{
    public async Task<ListTravelCostCentersResponse> HandleAsync(ListTravelCostCentersRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListTravelCostCentersResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetTravelCostCenters();
        return new ListTravelCostCentersResponse { Success = true, CostCenters = data.ConvertAll(c => c.ToDto()) };
    }
}

public class ListTravelStatusesHandler(HorizonAuthClient identity, TravelService svc)
    : IHorizonRequestHandler<ListTravelStatusesRequest, ListTravelStatusesResponse>
{
    public async Task<ListTravelStatusesResponse> HandleAsync(ListTravelStatusesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListTravelStatusesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetTravelStatuses();
        return new ListTravelStatusesResponse { Success = true, Statuses = data.ConvertAll(s => s.ToDto()) };
    }
}

public class ListTravelsHandler(HorizonAuthClient identity, TravelService svc)
    : IHorizonRequestHandler<ListTravelsRequest, ListTravelsResponse>
{
    public async Task<ListTravelsResponse> HandleAsync(ListTravelsRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListTravelsResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetTravelEntities(id.UserId);
        return new ListTravelsResponse { Success = true, Travels = data.ConvertAll(e => e.ToDto()) };
    }
}

public class ListTravelSubEntityCategoriesHandler(HorizonAuthClient identity, TravelService svc)
    : IHorizonRequestHandler<ListTravelSubEntityCategoriesRequest, ListTravelSubEntityCategoriesResponse>
{
    public async Task<ListTravelSubEntityCategoriesResponse> HandleAsync(ListTravelSubEntityCategoriesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListTravelSubEntityCategoriesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetTravelSubEntityCategories();
        return new ListTravelSubEntityCategoriesResponse { Success = true, Categories = data.ConvertAll(c => c.ToDto()) };
    }
}

public class ListTravelSubEntitiesHandler(HorizonAuthClient identity, TravelService svc)
    : IHorizonRequestHandler<ListTravelSubEntitiesRequest, ListTravelSubEntitiesResponse>
{
    public async Task<ListTravelSubEntitiesResponse> HandleAsync(ListTravelSubEntitiesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListTravelSubEntitiesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetTravelSubEntities(new() { Id = req.TravelId });
        return new ListTravelSubEntitiesResponse { Success = true, Entries = data.ConvertAll(e => e.ToDto()) };
    }
}

// ── Maintenance ─────────────────────────────────────────────────────────────

public class ListMaintCategoriesHandler(HorizonAuthClient identity, MaintService svc)
    : IHorizonRequestHandler<ListMaintCategoriesRequest, ListMaintCategoriesResponse>
{
    public async Task<ListMaintCategoriesResponse> HandleAsync(ListMaintCategoriesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListMaintCategoriesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetMaintCategories();
        return new ListMaintCategoriesResponse { Success = true, Categories = data.ConvertAll(c => c.ToDto()) };
    }
}

public class ListMaintStatusesHandler(HorizonAuthClient identity, MaintService svc)
    : IHorizonRequestHandler<ListMaintStatusesRequest, ListMaintStatusesResponse>
{
    public async Task<ListMaintStatusesResponse> HandleAsync(ListMaintStatusesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListMaintStatusesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetMaintStatuses();
        return new ListMaintStatusesResponse { Success = true, Statuses = data.ConvertAll(s => s.ToDto()) };
    }
}

public class ListMaintHandler(HorizonAuthClient identity, MaintService svc)
    : IHorizonRequestHandler<ListMaintRequest, ListMaintResponse>
{
    public async Task<ListMaintResponse> HandleAsync(ListMaintRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListMaintResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetMaintEntities(id.UserId);
        return new ListMaintResponse { Success = true, Items = data.ConvertAll(e => e.ToDto()) };
    }
}

// ── Inventory ───────────────────────────────────────────────────────────────

public class ListInvVendorStatusesHandler(HorizonAuthClient identity, InventoryService svc)
    : IHorizonRequestHandler<ListInvVendorStatusesRequest, ListInvVendorStatusesResponse>
{
    public async Task<ListInvVendorStatusesResponse> HandleAsync(ListInvVendorStatusesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListInvVendorStatusesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetInvVendorStatuses();
        return new ListInvVendorStatusesResponse { Success = true, Statuses = data.ConvertAll(s => s.ToDto()) };
    }
}

public class ListInvVendorsHandler(HorizonAuthClient identity, InventoryService svc)
    : IHorizonRequestHandler<ListInvVendorsRequest, ListInvVendorsResponse>
{
    public async Task<ListInvVendorsResponse> HandleAsync(ListInvVendorsRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListInvVendorsResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetInvVendorEntities(id.UserId);
        return new ListInvVendorsResponse { Success = true, Vendors = data.ConvertAll(e => e.ToDto()) };
    }
}

public class ListInvItemCategoriesHandler(HorizonAuthClient identity, InventoryService svc)
    : IHorizonRequestHandler<ListInvItemCategoriesRequest, ListInvItemCategoriesResponse>
{
    public async Task<ListInvItemCategoriesResponse> HandleAsync(ListInvItemCategoriesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListInvItemCategoriesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetInvItemCategories();
        return new ListInvItemCategoriesResponse { Success = true, Categories = data.ConvertAll(c => c.ToDto()) };
    }
}

public class ListInvItemStatusesHandler(HorizonAuthClient identity, InventoryService svc)
    : IHorizonRequestHandler<ListInvItemStatusesRequest, ListInvItemStatusesResponse>
{
    public async Task<ListInvItemStatusesResponse> HandleAsync(ListInvItemStatusesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListInvItemStatusesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetInvItemStatuses();
        return new ListInvItemStatusesResponse { Success = true, Statuses = data.ConvertAll(s => s.ToDto()) };
    }
}

public class ListInvItemsHandler(HorizonAuthClient identity, InventoryService svc)
    : IHorizonRequestHandler<ListInvItemsRequest, ListInvItemsResponse>
{
    public async Task<ListInvItemsResponse> HandleAsync(ListInvItemsRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListInvItemsResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = req.VendorId is { } vid
            ? await svc.GetInvItemByVendor(vid, id.UserId)
            : await svc.GetInvItemEntities(id.UserId);
        return new ListInvItemsResponse { Success = true, Items = data.ConvertAll(e => e.ToDto()) };
    }
}

public class GetShoppingListHandler(HorizonAuthClient identity, InventoryService svc)
    : IHorizonRequestHandler<GetShoppingListRequest, GetShoppingListResponse>
{
    public async Task<GetShoppingListResponse> HandleAsync(GetShoppingListRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new GetShoppingListResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetShoppingList(id.UserId);
        return new GetShoppingListResponse { Success = true, Items = data.ConvertAll(e => e.ToDto()) };
    }
}

public class ListInvOrderStatusesHandler(HorizonAuthClient identity, InventoryService svc)
    : IHorizonRequestHandler<ListInvOrderStatusesRequest, ListInvOrderStatusesResponse>
{
    public async Task<ListInvOrderStatusesResponse> HandleAsync(ListInvOrderStatusesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListInvOrderStatusesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetInvOrderStatuses();
        return new ListInvOrderStatusesResponse { Success = true, Statuses = data.ConvertAll(s => s.ToDto()) };
    }
}

public class ListInvOrdersHandler(HorizonAuthClient identity, InventoryService svc)
    : IHorizonRequestHandler<ListInvOrdersRequest, ListInvOrdersResponse>
{
    public async Task<ListInvOrdersResponse> HandleAsync(ListInvOrdersRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListInvOrdersResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetInvOrderEntities(id.UserId);
        return new ListInvOrdersResponse { Success = true, Orders = data.ConvertAll(e => e.ToDto()) };
    }
}

public class ListInvOrderLinesHandler(HorizonAuthClient identity, InventoryService svc)
    : IHorizonRequestHandler<ListInvOrderLinesRequest, ListInvOrderLinesResponse>
{
    public async Task<ListInvOrderLinesResponse> HandleAsync(ListInvOrderLinesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListInvOrderLinesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetInvOrderSubEntities(new() { Id = req.OrderId });
        return new ListInvOrderLinesResponse { Success = true, Lines = data.ConvertAll(e => e.ToDto()) };
    }
}

public class GetInvVendorStatsHandler(HorizonAuthClient identity, InventoryService svc)
    : IHorizonRequestHandler<GetInvVendorStatsRequest, GetInvVendorStatsResponse>
{
    public async Task<GetInvVendorStatsResponse> HandleAsync(GetInvVendorStatsRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new GetInvVendorStatsResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetInvVendorStats(req.VendorId);
        return new GetInvVendorStatsResponse { Success = true, Stats = data.ToDto() };
    }
}

public class GetInvOrderStatsHandler(HorizonAuthClient identity, InventoryService svc)
    : IHorizonRequestHandler<GetInvOrderStatsRequest, GetInvOrderStatsResponse>
{
    public async Task<GetInvOrderStatsResponse> HandleAsync(GetInvOrderStatsRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new GetInvOrderStatsResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetInvOrderStats(req.OrderId);
        return new GetInvOrderStatsResponse { Success = true, Stats = data.ToDto() };
    }
}

public class GetInvItemPriceHistoryHandler(HorizonAuthClient identity, InventoryService svc)
    : IHorizonRequestHandler<GetInvItemPriceHistoryRequest, GetInvItemPriceHistoryResponse>
{
    public async Task<GetInvItemPriceHistoryResponse> HandleAsync(GetInvItemPriceHistoryRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new GetInvItemPriceHistoryResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetInvItemPriceHistory(req.ItemId, id.UserId);
        return new GetInvItemPriceHistoryResponse { Success = true, History = data.ConvertAll(h => h.ToDto()) };
    }
}

// ── Clothes ─────────────────────────────────────────────────────────────────

public class ListClothesCategoriesHandler(HorizonAuthClient identity, ClothesService svc)
    : IHorizonRequestHandler<ListClothesCategoriesRequest, ListClothesCategoriesResponse>
{
    public async Task<ListClothesCategoriesResponse> HandleAsync(ListClothesCategoriesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListClothesCategoriesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetClothesCategories();
        return new ListClothesCategoriesResponse { Success = true, Categories = data.ConvertAll(c => c.ToDto()) };
    }
}

public class ListClothesStatusesHandler(HorizonAuthClient identity, ClothesService svc)
    : IHorizonRequestHandler<ListClothesStatusesRequest, ListClothesStatusesResponse>
{
    public async Task<ListClothesStatusesResponse> HandleAsync(ListClothesStatusesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListClothesStatusesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetClothesStatuses();
        return new ListClothesStatusesResponse { Success = true, Statuses = data.ConvertAll(s => s.ToDto()) };
    }
}

public class ListClothesWashTypesHandler(HorizonAuthClient identity, ClothesService svc)
    : IHorizonRequestHandler<ListClothesWashTypesRequest, ListClothesWashTypesResponse>
{
    public async Task<ListClothesWashTypesResponse> HandleAsync(ListClothesWashTypesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListClothesWashTypesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetClothesWashTypes();
        return new ListClothesWashTypesResponse { Success = true, WashTypes = data.ConvertAll(w => w.ToDto()) };
    }
}

public class ListClothesColorsHandler(HorizonAuthClient identity, ClothesService svc)
    : IHorizonRequestHandler<ListClothesColorsRequest, ListClothesColorsResponse>
{
    public async Task<ListClothesColorsResponse> HandleAsync(ListClothesColorsRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListClothesColorsResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetClothesColors();
        return new ListClothesColorsResponse { Success = true, Colors = data.ConvertAll(c => c.ToDto()) };
    }
}

public class ListClothesColorStylesHandler(HorizonAuthClient identity, ClothesService svc)
    : IHorizonRequestHandler<ListClothesColorStylesRequest, ListClothesColorStylesResponse>
{
    public async Task<ListClothesColorStylesResponse> HandleAsync(ListClothesColorStylesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListClothesColorStylesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetClothesColorStyles();
        return new ListClothesColorStylesResponse { Success = true, ColorStyles = data.ConvertAll(s => s.ToDto()) };
    }
}

public class ListClothesHandler(HorizonAuthClient identity, ClothesService svc)
    : IHorizonRequestHandler<ListClothesRequest, ListClothesResponse>
{
    public async Task<ListClothesResponse> HandleAsync(ListClothesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListClothesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetClothesEntities(id.UserId);
        return new ListClothesResponse { Success = true, Items = data.ConvertAll(e => e.ToDto()) };
    }
}

public class ListClothesColorAssignmentsHandler(HorizonAuthClient identity, ClothesService svc)
    : IHorizonRequestHandler<ListClothesColorAssignmentsRequest, ListClothesColorAssignmentsResponse>
{
    public async Task<ListClothesColorAssignmentsResponse> HandleAsync(ListClothesColorAssignmentsRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListClothesColorAssignmentsResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetColorAssignments(req.GarmentId);
        return new ListClothesColorAssignmentsResponse { Success = true, Assignments = data.ConvertAll(a => a.ToDto()) };
    }
}

// ── Admin (Processes) ───────────────────────────────────────────────────────

public class ListAdminCategoriesHandler(HorizonAuthClient identity, AdminService svc)
    : IHorizonRequestHandler<ListAdminCategoriesRequest, ListAdminCategoriesResponse>
{
    public async Task<ListAdminCategoriesResponse> HandleAsync(ListAdminCategoriesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListAdminCategoriesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetAdminCategories();
        return new ListAdminCategoriesResponse { Success = true, Categories = data.ConvertAll(c => c.ToDto()) };
    }
}

public class ListAdminHandler(HorizonAuthClient identity, AdminService svc)
    : IHorizonRequestHandler<ListAdminRequest, ListAdminResponse>
{
    public async Task<ListAdminResponse> HandleAsync(ListAdminRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListAdminResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetAdminEntities(id.UserId);
        return new ListAdminResponse { Success = true, Items = data.ConvertAll(e => e.ToDto()) };
    }
}

public class ListAdminSubEntitiesHandler(HorizonAuthClient identity, AdminService svc)
    : IHorizonRequestHandler<ListAdminSubEntitiesRequest, ListAdminSubEntitiesResponse>
{
    public async Task<ListAdminSubEntitiesResponse> HandleAsync(ListAdminSubEntitiesRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new ListAdminSubEntitiesResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetAdminSubEntities(new() { Id = req.ProcessId });
        return new ListAdminSubEntitiesResponse { Success = true, Steps = data.ConvertAll(e => e.ToDto()) };
    }
}

public class GetAdminStatsHandler(HorizonAuthClient identity, AdminService svc)
    : IHorizonRequestHandler<GetAdminStatsRequest, GetAdminStatsResponse>
{
    public async Task<GetAdminStatsResponse> HandleAsync(GetAdminStatsRequest req, CancellationToken ct = default)
    {
        var id = await identity.AuthAsync(req);
        if (id is null) return new GetAdminStatsResponse { Success = false, Error = HandlerAuth.Unauthorized() };
        var data = await svc.GetAdminStats(req.ProcessId);
        return new GetAdminStatsResponse { Success = true, Stats = data.ToDto() };
    }
}
