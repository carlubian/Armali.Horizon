using Armali.Horizon.Contracts.Identity;
using Armali.Horizon.IO;

namespace Armali.Horizon.Contracts.Segaris;

// ─────────────────────────────────────────────────────────────────────────────
// Operaciones de lectura expuestas por Horizon.Segaris sobre el bus IO.
// Todas requieren un token válido (sesión o API key) emitido por Identity.
// La privacidad por usuario se aplica en el handler usando la identidad
// resuelta a partir del token: las entidades privadas sólo son visibles para
// su Creator.
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Respuesta base que cualquier handler de Segaris puede usar para indicar
/// errores de autenticación o internos sin necesidad de un tipo específico.
/// </summary>
public interface ISegarisResponse : IHorizonEventPayload
{
    bool Success { get; set; }
    SegarisErrorInfo? Error { get; set; }
}

// ── Project ─────────────────────────────────────────────────────────────────

public class ListProjectProgramsRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.project.programs.list";
}
public class ListProjectProgramsResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.project.programs.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisRefDto> Programs { get; set; } = [];
}

public class ListProjectAxesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.project.axes.list";
    /// <summary>Filtra por programa. Si es null devuelve todos los ejes.</summary>
    public int? ProgramId { get; set; }
}
public class ListProjectAxesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.project.axes.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<ProjectAxisDto> Axes { get; set; } = [];
}

public class ListProjectStatusesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.project.statuses.list";
}
public class ListProjectStatusesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.project.statuses.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisStatusDto> Statuses { get; set; } = [];
}

public class ListProjectsRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.projects.list";
}
public class ListProjectsResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.projects.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<ProjectEntityDto> Projects { get; set; } = [];
}

public class ListProjectSubEntityCategoriesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.project.subCategories.list";
}
public class ListProjectSubEntityCategoriesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.project.subCategories.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisRefDto> Categories { get; set; } = [];
}

public class ListProjectSubEntitiesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.project.subEntities.list";
    public int ProjectId { get; set; }
}
public class ListProjectSubEntitiesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.project.subEntities.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<ProjectSubEntityDto> SubEntities { get; set; } = [];
}

public class ListProjectRiskCategoriesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.project.riskCategories.list";
}
public class ListProjectRiskCategoriesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.project.riskCategories.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisRefDto> Categories { get; set; } = [];
}

public class ListProjectRisksRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.project.risks.list";
    public int ProjectId { get; set; }
}
public class ListProjectRisksResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.project.risks.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<ProjectRiskElementDto> Risks { get; set; } = [];
}

public class ListProjectBudgetsRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.project.budgets.list";
    public int ProjectId { get; set; }
}
public class ListProjectBudgetsResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.project.budgets.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<ProjectBudgetDto> Budgets { get; set; } = [];
}

// ── Asset ───────────────────────────────────────────────────────────────────

public class ListAssetCategoriesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.asset.categories.list";
}
public class ListAssetCategoriesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.asset.categories.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisRefDto> Categories { get; set; } = [];
}

public class ListAssetStatusesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.asset.statuses.list";
}
public class ListAssetStatusesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.asset.statuses.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisStatusDto> Statuses { get; set; } = [];
}

public class ListAssetsRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.assets.list";
}
public class ListAssetsResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.assets.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<AssetEntityDto> Assets { get; set; } = [];
}

// ── Capex ───────────────────────────────────────────────────────────────────

public class ListCapexCategoriesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.capex.categories.list";
}
public class ListCapexCategoriesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.capex.categories.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisRefDto> Categories { get; set; } = [];
}

public class ListCapexStatusesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.capex.statuses.list";
}
public class ListCapexStatusesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.capex.statuses.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisStatusDto> Statuses { get; set; } = [];
}

public class ListCapexRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.capex.list";
}
public class ListCapexResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.capex.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<CapexEntityDto> Items { get; set; } = [];
}

// ── Opex ────────────────────────────────────────────────────────────────────

public class ListOpexCategoriesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.opex.categories.list";
}
public class ListOpexCategoriesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.opex.categories.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisRefDto> Categories { get; set; } = [];
}

public class ListOpexStatusesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.opex.statuses.list";
}
public class ListOpexStatusesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.opex.statuses.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisStatusDto> Statuses { get; set; } = [];
}

public class ListOpexRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.opex.list";
}
public class ListOpexResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.opex.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<OpexEntityDto> Contracts { get; set; } = [];
}

public class ListOpexSubEntitiesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.opex.subEntries.list";
    public int ContractId { get; set; }
}
public class ListOpexSubEntitiesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.opex.subEntries.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<OpexSubEntityDto> Entries { get; set; } = [];
}

public class GetOpexStatsRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.opex.stats.get";
    public int ContractId { get; set; }
}
public class GetOpexStatsResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.opex.stats.get:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public OpexStatsDto Stats { get; set; } = new();
}

// ── Travel ──────────────────────────────────────────────────────────────────

public class ListTravelCategoriesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.travel.categories.list";
}
public class ListTravelCategoriesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.travel.categories.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisRefDto> Categories { get; set; } = [];
}

public class ListTravelCostCentersRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.travel.costCenters.list";
}
public class ListTravelCostCentersResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.travel.costCenters.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisRefDto> CostCenters { get; set; } = [];
}

public class ListTravelStatusesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.travel.statuses.list";
}
public class ListTravelStatusesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.travel.statuses.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisStatusDto> Statuses { get; set; } = [];
}

public class ListTravelsRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.travels.list";
}
public class ListTravelsResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.travels.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<TravelEntityDto> Travels { get; set; } = [];
}

public class ListTravelSubEntityCategoriesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.travel.subCategories.list";
}
public class ListTravelSubEntityCategoriesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.travel.subCategories.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisRefDto> Categories { get; set; } = [];
}

public class ListTravelSubEntitiesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.travel.entries.list";
    public int TravelId { get; set; }
}
public class ListTravelSubEntitiesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.travel.entries.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<TravelSubEntityDto> Entries { get; set; } = [];
}

// ── Maintenance ─────────────────────────────────────────────────────────────

public class ListMaintCategoriesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.maint.categories.list";
}
public class ListMaintCategoriesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.maint.categories.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisRefDto> Categories { get; set; } = [];
}

public class ListMaintStatusesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.maint.statuses.list";
}
public class ListMaintStatusesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.maint.statuses.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisStatusDto> Statuses { get; set; } = [];
}

public class ListMaintRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.maint.list";
}
public class ListMaintResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.maint.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<MaintEntityDto> Items { get; set; } = [];
}

// ── Inventory ───────────────────────────────────────────────────────────────

public class ListInvVendorStatusesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.inv.vendorStatuses.list";
}
public class ListInvVendorStatusesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.inv.vendorStatuses.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisStatusDto> Statuses { get; set; } = [];
}

public class ListInvVendorsRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.inv.vendors.list";
}
public class ListInvVendorsResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.inv.vendors.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<InvVendorEntityDto> Vendors { get; set; } = [];
}

public class ListInvItemCategoriesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.inv.itemCategories.list";
}
public class ListInvItemCategoriesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.inv.itemCategories.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisRefDto> Categories { get; set; } = [];
}

public class ListInvItemStatusesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.inv.itemStatuses.list";
}
public class ListInvItemStatusesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.inv.itemStatuses.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisStatusDto> Statuses { get; set; } = [];
}

public class ListInvItemsRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.inv.items.list";
    /// <summary>Si se indica, filtra por proveedor.</summary>
    public int? VendorId { get; set; }
}
public class ListInvItemsResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.inv.items.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<InvItemEntityDto> Items { get; set; } = [];
}

public class GetShoppingListRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.inv.shoppingList.get";
}
public class GetShoppingListResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.inv.shoppingList.get:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<InvItemEntityDto> Items { get; set; } = [];
}

public class ListInvOrderStatusesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.inv.orderStatuses.list";
}
public class ListInvOrderStatusesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.inv.orderStatuses.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisStatusDto> Statuses { get; set; } = [];
}

public class ListInvOrdersRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.inv.orders.list";
}
public class ListInvOrdersResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.inv.orders.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<InvOrderEntityDto> Orders { get; set; } = [];
}

public class ListInvOrderLinesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.inv.orderLines.list";
    public int OrderId { get; set; }
}
public class ListInvOrderLinesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.inv.orderLines.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<InvOrderSubEntityDto> Lines { get; set; } = [];
}

public class GetInvVendorStatsRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.inv.vendorStats.get";
    public int VendorId { get; set; }
}
public class GetInvVendorStatsResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.inv.vendorStats.get:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public InvVendorStatsDto Stats { get; set; } = new();
}

public class GetInvOrderStatsRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.inv.orderStats.get";
    public int OrderId { get; set; }
}
public class GetInvOrderStatsResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.inv.orderStats.get:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public InvOrderStatsDto Stats { get; set; } = new();
}

public class GetInvItemPriceHistoryRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.inv.itemPriceHistory.get";
    public int ItemId { get; set; }
}
public class GetInvItemPriceHistoryResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.inv.itemPriceHistory.get:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<InvItemPriceHistoryDto> History { get; set; } = [];
}

// ── Clothes ─────────────────────────────────────────────────────────────────

public class ListClothesCategoriesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.clothes.categories.list";
}
public class ListClothesCategoriesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.clothes.categories.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisRefDto> Categories { get; set; } = [];
}

public class ListClothesStatusesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.clothes.statuses.list";
}
public class ListClothesStatusesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.clothes.statuses.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisStatusDto> Statuses { get; set; } = [];
}

public class ListClothesWashTypesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.clothes.washTypes.list";
}
public class ListClothesWashTypesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.clothes.washTypes.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisRefDto> WashTypes { get; set; } = [];
}

public class ListClothesColorsRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.clothes.colors.list";
}
public class ListClothesColorsResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.clothes.colors.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<ClothesColorDto> Colors { get; set; } = [];
}

public class ListClothesColorStylesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.clothes.colorStyles.list";
}
public class ListClothesColorStylesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.clothes.colorStyles.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisRefDto> ColorStyles { get; set; } = [];
}

public class ListClothesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.clothes.list";
}
public class ListClothesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.clothes.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<ClothesEntityDto> Items { get; set; } = [];
}

public class ListClothesColorAssignmentsRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.clothes.colorAssignments.list";
    /// <summary>ID de la prenda cuyos colores se quieren consultar.</summary>
    public int GarmentId { get; set; }
}
public class ListClothesColorAssignmentsResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.clothes.colorAssignments.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<ClothesColorAssignmentDto> Assignments { get; set; } = [];
}

// ── Firebird (People) ───────────────────────────────────────────────────────

public class ListFirebirdCategoriesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.firebird.categories.list";
}
public class ListFirebirdCategoriesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.firebird.categories.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisRefDto> Categories { get; set; } = [];
}

public class ListFirebirdStatusesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.firebird.statuses.list";
}
public class ListFirebirdStatusesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.firebird.statuses.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisStatusDto> Statuses { get; set; } = [];
}

public class ListFirebirdsRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.firebird.list";
}
public class ListFirebirdsResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.firebird.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<FirebirdEntityDto> Items { get; set; } = [];
}

public class ListFirebirdSubEntitiesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.firebird.subEntities.list";
    /// <summary>ID de la persona cuyos eventos se quieren consultar.</summary>
    public int FirebirdId { get; set; }
}
public class ListFirebirdSubEntitiesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.firebird.subEntities.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<FirebirdSubEntityDto> SubEntities { get; set; } = [];
}

// ── Admin (Processes) ───────────────────────────────────────────────────────

public class ListAdminCategoriesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.admin.categories.list";
}
public class ListAdminCategoriesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.admin.categories.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<SegarisRefDto> Categories { get; set; } = [];
}

public class ListAdminRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.admin.list";
}
public class ListAdminResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.admin.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<AdminEntityDto> Items { get; set; } = [];
}

public class ListAdminSubEntitiesRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.admin.steps.list";
    /// <summary>ID del proceso cuyos pasos se quieren consultar.</summary>
    public int ProcessId { get; set; }
}
public class ListAdminSubEntitiesResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.admin.steps.list:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public List<AdminSubEntityDto> Steps { get; set; } = [];
}

public class GetAdminStatsRequest : AuthenticatedRequest
{
    public override string EventType => "segaris.admin.stats.get";
    /// <summary>ID del proceso del que se quieren estadísticas.</summary>
    public int ProcessId { get; set; }
}
public class GetAdminStatsResponse : IHorizonEventPayload, ISegarisResponse
{
    public string EventType => "segaris.admin.stats.get:response";
    public bool Success { get; set; }
    public SegarisErrorInfo? Error { get; set; }
    public AdminStatsDto Stats { get; set; } = new();
}
