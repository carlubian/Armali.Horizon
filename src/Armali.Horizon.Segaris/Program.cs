using Armali.Horizon.Blazor.Services;
using Armali.Horizon.Core.Logs;
using Armali.Horizon.Segaris.Components;
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

        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode()
            .AddAdditionalAssemblies(typeof(Armali.Horizon.Blazor._Imports).Assembly);

        app.Run();
    }
}