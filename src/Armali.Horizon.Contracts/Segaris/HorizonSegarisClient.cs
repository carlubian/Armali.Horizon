using Armali.Horizon.IO;

namespace Armali.Horizon.Contracts.Segaris;

/// <summary>
/// Cliente de alto nivel para Horizon.Segaris sobre el bus IO.
/// <para>
/// Encapsula las llamadas <see cref="HorizonEventService.RequestAsync{T}"/> al
/// canal <see cref="SegarisChannels.Channel"/>. Sigue el mismo patrón que
/// <see cref="HorizonAutoconfigClient"/> y <see cref="Identity.HorizonAuthClient"/>.
/// </para>
/// <para>
/// Todas las operaciones requieren un token bearer (sesión o API key emitida por
/// Identity). El token se asigna al construir el cliente o por cada llamada
/// modificando <see cref="Token"/>.
/// </para>
/// </summary>
public class HorizonSegarisClient
{
    private readonly HorizonEventService Events;
    private readonly TimeSpan? Timeout;
    
    /// <summary>Token actual asociado al cliente.</summary>
    public string? Token { get; set; }
    
    public HorizonSegarisClient(HorizonEventService events, string? token = null, TimeSpan? timeout = null)
    {
        Events = events;
        Token = token;
        Timeout = timeout;
    }
    
    private string RequireToken() =>
        Token ?? throw new InvalidOperationException(
            "No hay token activo. Asigna Token antes de llamar al cliente Segaris.");
    
    private Task<TRes> Send<TRes>(Identity.AuthenticatedRequest req)
        where TRes : IHorizonEventPayload
    {
        req.Token = RequireToken();
        return Events.RequestAsync<TRes>(SegarisChannels.Channel, req, Timeout);
    }
    
    // ── Project ─────────────────────────────────────────────────────────
    
    public Task<ListProjectProgramsResponse> ListProjectProgramsAsync() =>
        Send<ListProjectProgramsResponse>(new ListProjectProgramsRequest());
    
    public Task<ListProjectAxesResponse> ListProjectAxesAsync(int? programId = null) =>
        Send<ListProjectAxesResponse>(new ListProjectAxesRequest { ProgramId = programId });
    
    public Task<ListProjectStatusesResponse> ListProjectStatusesAsync() =>
        Send<ListProjectStatusesResponse>(new ListProjectStatusesRequest());
    
    public Task<ListProjectsResponse> ListProjectsAsync() =>
        Send<ListProjectsResponse>(new ListProjectsRequest());
    
    public Task<ListProjectSubEntityCategoriesResponse> ListProjectSubEntityCategoriesAsync() =>
        Send<ListProjectSubEntityCategoriesResponse>(new ListProjectSubEntityCategoriesRequest());
    
    public Task<ListProjectSubEntitiesResponse> ListProjectSubEntitiesAsync(int projectId) =>
        Send<ListProjectSubEntitiesResponse>(new ListProjectSubEntitiesRequest { ProjectId = projectId });
    
    public Task<ListProjectRiskCategoriesResponse> ListProjectRiskCategoriesAsync() =>
        Send<ListProjectRiskCategoriesResponse>(new ListProjectRiskCategoriesRequest());
    
    public Task<ListProjectRisksResponse> ListProjectRisksAsync(int projectId) =>
        Send<ListProjectRisksResponse>(new ListProjectRisksRequest { ProjectId = projectId });
    
    public Task<ListProjectBudgetsResponse> ListProjectBudgetsAsync(int projectId) =>
        Send<ListProjectBudgetsResponse>(new ListProjectBudgetsRequest { ProjectId = projectId });
    
    // ── Asset ───────────────────────────────────────────────────────────
    
    public Task<ListAssetCategoriesResponse> ListAssetCategoriesAsync() =>
        Send<ListAssetCategoriesResponse>(new ListAssetCategoriesRequest());
    
    public Task<ListAssetStatusesResponse> ListAssetStatusesAsync() =>
        Send<ListAssetStatusesResponse>(new ListAssetStatusesRequest());
    
    public Task<ListAssetsResponse> ListAssetsAsync() =>
        Send<ListAssetsResponse>(new ListAssetsRequest());
    
    // ── Capex ───────────────────────────────────────────────────────────
    
    public Task<ListCapexCategoriesResponse> ListCapexCategoriesAsync() =>
        Send<ListCapexCategoriesResponse>(new ListCapexCategoriesRequest());
    
    public Task<ListCapexStatusesResponse> ListCapexStatusesAsync() =>
        Send<ListCapexStatusesResponse>(new ListCapexStatusesRequest());
    
    public Task<ListCapexResponse> ListCapexAsync() =>
        Send<ListCapexResponse>(new ListCapexRequest());
    
    // ── Opex ────────────────────────────────────────────────────────────
    
    public Task<ListOpexCategoriesResponse> ListOpexCategoriesAsync() =>
        Send<ListOpexCategoriesResponse>(new ListOpexCategoriesRequest());
    
    public Task<ListOpexStatusesResponse> ListOpexStatusesAsync() =>
        Send<ListOpexStatusesResponse>(new ListOpexStatusesRequest());
    
    public Task<ListOpexResponse> ListOpexAsync() =>
        Send<ListOpexResponse>(new ListOpexRequest());
    
    public Task<ListOpexSubEntitiesResponse> ListOpexSubEntitiesAsync(int contractId) =>
        Send<ListOpexSubEntitiesResponse>(new ListOpexSubEntitiesRequest { ContractId = contractId });
    
    public Task<GetOpexStatsResponse> GetOpexStatsAsync(int contractId) =>
        Send<GetOpexStatsResponse>(new GetOpexStatsRequest { ContractId = contractId });
    
    // ── Travel ──────────────────────────────────────────────────────────
    
    public Task<ListTravelCategoriesResponse> ListTravelCategoriesAsync() =>
        Send<ListTravelCategoriesResponse>(new ListTravelCategoriesRequest());
    
    public Task<ListTravelCostCentersResponse> ListTravelCostCentersAsync() =>
        Send<ListTravelCostCentersResponse>(new ListTravelCostCentersRequest());
    
    public Task<ListTravelStatusesResponse> ListTravelStatusesAsync() =>
        Send<ListTravelStatusesResponse>(new ListTravelStatusesRequest());
    
    public Task<ListTravelsResponse> ListTravelsAsync() =>
        Send<ListTravelsResponse>(new ListTravelsRequest());
    
    public Task<ListTravelSubEntityCategoriesResponse> ListTravelSubEntityCategoriesAsync() =>
        Send<ListTravelSubEntityCategoriesResponse>(new ListTravelSubEntityCategoriesRequest());
    
    public Task<ListTravelSubEntitiesResponse> ListTravelSubEntitiesAsync(int travelId) =>
        Send<ListTravelSubEntitiesResponse>(new ListTravelSubEntitiesRequest { TravelId = travelId });
    
    // ── Maintenance ─────────────────────────────────────────────────────
    
    public Task<ListMaintCategoriesResponse> ListMaintCategoriesAsync() =>
        Send<ListMaintCategoriesResponse>(new ListMaintCategoriesRequest());
    
    public Task<ListMaintStatusesResponse> ListMaintStatusesAsync() =>
        Send<ListMaintStatusesResponse>(new ListMaintStatusesRequest());
    
    public Task<ListMaintResponse> ListMaintAsync() =>
        Send<ListMaintResponse>(new ListMaintRequest());
    
    // ── Inventory ───────────────────────────────────────────────────────
    
    public Task<ListInvVendorStatusesResponse> ListInvVendorStatusesAsync() =>
        Send<ListInvVendorStatusesResponse>(new ListInvVendorStatusesRequest());
    
    public Task<ListInvVendorsResponse> ListInvVendorsAsync() =>
        Send<ListInvVendorsResponse>(new ListInvVendorsRequest());
    
    public Task<ListInvItemCategoriesResponse> ListInvItemCategoriesAsync() =>
        Send<ListInvItemCategoriesResponse>(new ListInvItemCategoriesRequest());
    
    public Task<ListInvItemStatusesResponse> ListInvItemStatusesAsync() =>
        Send<ListInvItemStatusesResponse>(new ListInvItemStatusesRequest());
    
    public Task<ListInvItemsResponse> ListInvItemsAsync(int? vendorId = null) =>
        Send<ListInvItemsResponse>(new ListInvItemsRequest { VendorId = vendorId });
    
    public Task<GetShoppingListResponse> GetShoppingListAsync() =>
        Send<GetShoppingListResponse>(new GetShoppingListRequest());
    
    public Task<ListInvOrderStatusesResponse> ListInvOrderStatusesAsync() =>
        Send<ListInvOrderStatusesResponse>(new ListInvOrderStatusesRequest());
    
    public Task<ListInvOrdersResponse> ListInvOrdersAsync() =>
        Send<ListInvOrdersResponse>(new ListInvOrdersRequest());
    
    public Task<ListInvOrderLinesResponse> ListInvOrderLinesAsync(int orderId) =>
        Send<ListInvOrderLinesResponse>(new ListInvOrderLinesRequest { OrderId = orderId });
    
    public Task<GetInvVendorStatsResponse> GetInvVendorStatsAsync(int vendorId) =>
        Send<GetInvVendorStatsResponse>(new GetInvVendorStatsRequest { VendorId = vendorId });
    
    public Task<GetInvOrderStatsResponse> GetInvOrderStatsAsync(int orderId) =>
        Send<GetInvOrderStatsResponse>(new GetInvOrderStatsRequest { OrderId = orderId });
    
    public Task<GetInvItemPriceHistoryResponse> GetInvItemPriceHistoryAsync(int itemId) =>
        Send<GetInvItemPriceHistoryResponse>(new GetInvItemPriceHistoryRequest { ItemId = itemId });
}

