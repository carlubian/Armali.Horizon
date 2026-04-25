using Armali.Horizon.Autoconfig;
using Armali.Horizon.Autoconfig.Components;
using Armali.Horizon.Autoconfig.Handlers;
using Armali.Horizon.Autoconfig.Services;
using Armali.Horizon.Blazor.Services;
using Armali.Horizon.Contracts.Autoconfig;
using Armali.Horizon.Core.Logs;
using Armali.Horizon.IO;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Logging centralizado con Serilog + Seq
builder.Host.UseHorizonLogging();

// Opciones específicas de Autoconfig (Horizon:Autoconfig:MaxFileBytes, etc.)
var autoconfigOptions = builder.Configuration.GetSection("Horizon:Autoconfig").Get<AutoconfigOptions>()
    ?? new AutoconfigOptions();
builder.Services.AddSingleton(autoconfigOptions);

// Bus de eventos Horizon: registra el handler de configuración request/response
builder.Host.UseHorizonEvents(events =>
{
    events.HandleRequest<GetConfigFileHandler, GetConfigFileRequest, GetConfigFileResponse>(
        AutoconfigChannels.Channel);
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<HorizonSessionService>();

builder.Services.AddDbContextFactory<AutoconfigDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetSection("Horizon")["ConnectionStrings:Autoconfig"]));
builder.Services.AddSingleton<AutoconfigDatalakeService>();
builder.Services.AddScoped<AutoconfigService>();

var app = builder.Build();

// Aplicar migraciones pendientes automáticamente en producción
if (!app.Environment.IsDevelopment())
{
    using var db = app.Services.GetRequiredService<IDbContextFactory<AutoconfigDbContext>>().CreateDbContext();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Armali.Horizon.Blazor._Imports).Assembly);

app.Run();