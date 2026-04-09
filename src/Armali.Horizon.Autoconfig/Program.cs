using Armali.Horizon.Autoconfig;
using Armali.Horizon.Autoconfig.Components;
using Armali.Horizon.Autoconfig.Services;
using Armali.Horizon.Blazor.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<HorizonSessionService>();

builder.Services.AddDbContextFactory<AutoconfigDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Autoconfig")));
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