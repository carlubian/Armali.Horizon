using Armali.Horizon.Althes.UI;
using Armali.Horizon.Althes.UI.Services;
using Armali.Horizon.Blazor.Services;
using Armali.Horizon.Core.Logs;
using Armali.Horizon.IO;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseHorizonLogging();

// Bus IO — cliente puro, sin handlers propios.
builder.Host.UseHorizonEvents();

// Componentes Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<HorizonSessionService>();

// BD local de proyectos
builder.Services.AddDbContextFactory<AlthesUiDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetSection("Horizon")["ConnectionStrings:AlthesUI"]
        ?? "Data Source=althes-ui.db"));

// Servicios de la app
builder.Services.AddSingleton<AlthesConnectionManager>();
builder.Services.AddScoped<AlthesProjectStore>();
builder.Services.AddScoped<AlthesLiveSubscription>();

var app = builder.Build();

// Migración automática al arrancar
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AlthesUiDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.MigrateAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.MapGet("/health", () => Results.Ok(new { status = "ok", app = "althes-ui" }));
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<Armali.Horizon.Althes.UI.Components.App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Armali.Horizon.Blazor._Imports).Assembly);

app.Run();

