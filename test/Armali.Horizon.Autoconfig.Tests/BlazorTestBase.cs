using System.Text.Json;
using Armali.Horizon.Blazor.Services;
using Armali.Horizon.Core.Model;
using Armali.Horizon.Autoconfig.Services;
using Bunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Armali.Horizon.Autoconfig.Tests;

/// <summary>
/// Base para tests de componentes Blazor de Autoconfig.
/// Configura bUnit con servicios reales (sobre SQLite in-memory) y
/// un mock de IJSRuntime que simula un usuario autenticado en localStorage.
/// </summary>
public abstract class BlazorTestBase : BunitContext
{
    protected TestDbContextFactory DbFactory = null!;
    protected const string TestUserId = "testuser";
    protected const string TestUserName = "Test User";

    [TestInitialize]
    public void SetupBlazor()
    {
        DbFactory = new TestDbContextFactory();

        // JS Interop en modo loose: llamadas no configuradas devuelven default
        JSInterop.Mode = JSRuntimeMode.Loose;

        // Simular usuario autenticado en localStorage
        var identity = new HorizonIdentity { UserId = TestUserId, UserName = TestUserName };
        JSInterop.Setup<string>("localStorage.getItem", "Horizon:Session")
            .SetResult(JsonSerializer.Serialize(identity));

        // Registrar servicios — misma DI que Program.cs pero con TestDbContextFactory
        Services.AddSingleton<IDbContextFactory<AutoconfigDbContext>>(DbFactory);
        Services.AddScoped<HorizonSessionService>();
        Services.AddScoped<AutoconfigService>();
    }

    [TestCleanup]
    public void CleanupBlazor()
    {
        DbFactory.Dispose();
        Dispose();
    }
}
