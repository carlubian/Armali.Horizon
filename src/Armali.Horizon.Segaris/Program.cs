using Armali.Horizon.Blazor.Services;
using Armali.Horizon.Contracts.Identity;
using Armali.Horizon.Contracts.Segaris;
using Armali.Horizon.Core.Logs;
using Armali.Horizon.IO;
using Armali.Horizon.Segaris.Components;
using Armali.Horizon.Segaris.Handlers;
using Armali.Horizon.Segaris.Services;
using Microsoft.EntityFrameworkCore;

namespace Armali.Horizon.Segaris;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Logging centralizado con Serilog + Seq
        builder.Host.UseHorizonLogging();

        // Bus de eventos Horizon: registramos los handlers de lectura de Segaris
        // en el canal "segaris". Cada handler valida el token contra Identity
        // antes de operar, por lo que se requiere también el cliente Auth.
        builder.Host.UseHorizonEvents(events =>
        {
            events
                // Project
                .HandleRequest<ListProjectProgramsHandler, ListProjectProgramsRequest, ListProjectProgramsResponse>(SegarisChannels.Channel)
                .HandleRequest<ListProjectAxesHandler, ListProjectAxesRequest, ListProjectAxesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListProjectStatusesHandler, ListProjectStatusesRequest, ListProjectStatusesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListProjectsHandler, ListProjectsRequest, ListProjectsResponse>(SegarisChannels.Channel)
                .HandleRequest<ListProjectSubEntityCategoriesHandler, ListProjectSubEntityCategoriesRequest, ListProjectSubEntityCategoriesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListProjectSubEntitiesHandler, ListProjectSubEntitiesRequest, ListProjectSubEntitiesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListProjectRiskCategoriesHandler, ListProjectRiskCategoriesRequest, ListProjectRiskCategoriesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListProjectRisksHandler, ListProjectRisksRequest, ListProjectRisksResponse>(SegarisChannels.Channel)
                .HandleRequest<ListProjectBudgetsHandler, ListProjectBudgetsRequest, ListProjectBudgetsResponse>(SegarisChannels.Channel)
                // Asset
                .HandleRequest<ListAssetCategoriesHandler, ListAssetCategoriesRequest, ListAssetCategoriesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListAssetStatusesHandler, ListAssetStatusesRequest, ListAssetStatusesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListAssetsHandler, ListAssetsRequest, ListAssetsResponse>(SegarisChannels.Channel)
                // Capex
                .HandleRequest<ListCapexCategoriesHandler, ListCapexCategoriesRequest, ListCapexCategoriesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListCapexStatusesHandler, ListCapexStatusesRequest, ListCapexStatusesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListCapexHandler, ListCapexRequest, ListCapexResponse>(SegarisChannels.Channel)
                // Opex
                .HandleRequest<ListOpexCategoriesHandler, ListOpexCategoriesRequest, ListOpexCategoriesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListOpexStatusesHandler, ListOpexStatusesRequest, ListOpexStatusesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListOpexHandler, ListOpexRequest, ListOpexResponse>(SegarisChannels.Channel)
                .HandleRequest<ListOpexSubEntitiesHandler, ListOpexSubEntitiesRequest, ListOpexSubEntitiesResponse>(SegarisChannels.Channel)
                .HandleRequest<GetOpexStatsHandler, GetOpexStatsRequest, GetOpexStatsResponse>(SegarisChannels.Channel)
                // Travel
                .HandleRequest<ListTravelCategoriesHandler, ListTravelCategoriesRequest, ListTravelCategoriesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListTravelCostCentersHandler, ListTravelCostCentersRequest, ListTravelCostCentersResponse>(SegarisChannels.Channel)
                .HandleRequest<ListTravelStatusesHandler, ListTravelStatusesRequest, ListTravelStatusesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListTravelsHandler, ListTravelsRequest, ListTravelsResponse>(SegarisChannels.Channel)
                .HandleRequest<ListTravelSubEntityCategoriesHandler, ListTravelSubEntityCategoriesRequest, ListTravelSubEntityCategoriesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListTravelSubEntitiesHandler, ListTravelSubEntitiesRequest, ListTravelSubEntitiesResponse>(SegarisChannels.Channel)
                // Maintenance
                .HandleRequest<ListMaintCategoriesHandler, ListMaintCategoriesRequest, ListMaintCategoriesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListMaintStatusesHandler, ListMaintStatusesRequest, ListMaintStatusesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListMaintHandler, ListMaintRequest, ListMaintResponse>(SegarisChannels.Channel)
                // Inventory
                .HandleRequest<ListInvVendorStatusesHandler, ListInvVendorStatusesRequest, ListInvVendorStatusesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListInvVendorsHandler, ListInvVendorsRequest, ListInvVendorsResponse>(SegarisChannels.Channel)
                .HandleRequest<ListInvItemCategoriesHandler, ListInvItemCategoriesRequest, ListInvItemCategoriesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListInvItemStatusesHandler, ListInvItemStatusesRequest, ListInvItemStatusesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListInvItemsHandler, ListInvItemsRequest, ListInvItemsResponse>(SegarisChannels.Channel)
                .HandleRequest<GetShoppingListHandler, GetShoppingListRequest, GetShoppingListResponse>(SegarisChannels.Channel)
                .HandleRequest<ListInvOrderStatusesHandler, ListInvOrderStatusesRequest, ListInvOrderStatusesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListInvOrdersHandler, ListInvOrdersRequest, ListInvOrdersResponse>(SegarisChannels.Channel)
                .HandleRequest<ListInvOrderLinesHandler, ListInvOrderLinesRequest, ListInvOrderLinesResponse>(SegarisChannels.Channel)
                .HandleRequest<GetInvVendorStatsHandler, GetInvVendorStatsRequest, GetInvVendorStatsResponse>(SegarisChannels.Channel)
                .HandleRequest<GetInvOrderStatsHandler, GetInvOrderStatsRequest, GetInvOrderStatsResponse>(SegarisChannels.Channel)
                .HandleRequest<GetInvItemPriceHistoryHandler, GetInvItemPriceHistoryRequest, GetInvItemPriceHistoryResponse>(SegarisChannels.Channel)
                // Clothes
                .HandleRequest<ListClothesCategoriesHandler, ListClothesCategoriesRequest, ListClothesCategoriesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListClothesStatusesHandler, ListClothesStatusesRequest, ListClothesStatusesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListClothesWashTypesHandler, ListClothesWashTypesRequest, ListClothesWashTypesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListClothesColorsHandler, ListClothesColorsRequest, ListClothesColorsResponse>(SegarisChannels.Channel)
                .HandleRequest<ListClothesColorStylesHandler, ListClothesColorStylesRequest, ListClothesColorStylesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListClothesHandler, ListClothesRequest, ListClothesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListClothesColorAssignmentsHandler, ListClothesColorAssignmentsRequest, ListClothesColorAssignmentsResponse>(SegarisChannels.Channel)
                // Firebird (People)
                .HandleRequest<ListFirebirdCategoriesHandler, ListFirebirdCategoriesRequest, ListFirebirdCategoriesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListFirebirdStatusesHandler, ListFirebirdStatusesRequest, ListFirebirdStatusesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListFirebirdsHandler, ListFirebirdsRequest, ListFirebirdsResponse>(SegarisChannels.Channel)
                .HandleRequest<ListFirebirdSubEntitiesHandler, ListFirebirdSubEntitiesRequest, ListFirebirdSubEntitiesResponse>(SegarisChannels.Channel)
                // Admin (Processes)
                .HandleRequest<ListAdminCategoriesHandler, ListAdminCategoriesRequest, ListAdminCategoriesResponse>(SegarisChannels.Channel)
                .HandleRequest<ListAdminHandler, ListAdminRequest, ListAdminResponse>(SegarisChannels.Channel)
                .HandleRequest<ListAdminSubEntitiesHandler, ListAdminSubEntitiesRequest, ListAdminSubEntitiesResponse>(SegarisChannels.Channel)
                .HandleRequest<GetAdminStatsHandler, GetAdminStatsRequest, GetAdminStatsResponse>(SegarisChannels.Channel);
        });
        
        // Cliente Identity para que los handlers validen el token recibido.
        // Se resuelve en el scope DI por petición; el HorizonEventService es singleton.
        builder.Services.AddScoped(sp => new HorizonAuthClient(sp.GetRequiredService<HorizonEventService>()));

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        
        builder.Services.AddScoped<HorizonSessionService>();
        
        builder.Services.AddDbContextFactory<SegarisDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetSection("Horizon")["ConnectionStrings:Segaris"]));
        builder.Services.AddScoped<CapexService>();
        builder.Services.AddScoped<OpexService>();
        builder.Services.AddScoped<InventoryService>();
        builder.Services.AddScoped<AssetService>();
        builder.Services.AddScoped<TravelService>();
        builder.Services.AddScoped<ProjectService>();
        builder.Services.AddScoped<DatalakeService>();
        builder.Services.AddScoped<ArchiveService>();
        builder.Services.AddScoped<MaintService>();
        builder.Services.AddScoped<FirebirdService>();
        builder.Services.AddScoped<ClothesService>();
        builder.Services.AddScoped<MoodService>();
        builder.Services.AddScoped<AdminService>();
        builder.Services.AddScoped<ExpenseService>();

        var app = builder.Build();

        // Aplicar migraciones pendientes automáticamente en producción
        if (!app.Environment.IsDevelopment())
        {
            using var db = app.Services.GetRequiredService<IDbContextFactory<SegarisDbContext>>().CreateDbContext();
            db.Database.Migrate();
        }

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseAntiforgery();

        // Endpoint de salud para smoke tests / readiness probes.
        app.MapGet("/health", () => Results.Ok(new { status = "ok", app = "segaris" }));

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddAdditionalAssemblies(typeof(Armali.Horizon.Blazor._Imports).Assembly);

        app.Run();
    }
}