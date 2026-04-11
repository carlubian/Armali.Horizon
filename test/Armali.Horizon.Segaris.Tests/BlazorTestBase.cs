using System.Text.Json;
using Armali.Horizon.Blazor.Services;
using Armali.Horizon.Core.Model;
using Armali.Horizon.Segaris.Services;
using Bunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Armali.Horizon.Segaris.Tests;

/// <summary>
/// Base para tests de componentes Blazor de Segaris.
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
        Services.AddSingleton<IDbContextFactory<SegarisDbContext>>(DbFactory);
        Services.AddScoped<HorizonSessionService>();
        Services.AddScoped<CapexService>();

        // ProjectService se construye sin DatalakeService real para evitar
        // la llamada HTTP a Azure Data Lake en el constructor de DatalakeService.
        // Los tests de UI no ejercitan operaciones de archivo.
        Services.AddScoped<ProjectService>(sp =>
            new ProjectService(
                sp.GetRequiredService<IDbContextFactory<SegarisDbContext>>(),
                null!));
    }

    [TestCleanup]
    public void CleanupBlazor()
    {
        DbFactory.Dispose();
        Dispose();
    }
}


