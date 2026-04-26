using Armali.Horizon.Contracts.Autoconfig;
using Armali.Horizon.Contracts.Identity;
using Armali.Horizon.Contracts.Segaris;
using Armali.Horizon.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace Armali.Horizon.Smoke.Tests;

/// <summary>
/// Smoke tests del stack Horizon completo (Identity + Segaris + Autoconfig + MCP + Redis).
/// <para>
/// Estos tests asumen que el docker-compose ya está en marcha. El workflow
/// <c>.github/workflows/smoke.yml</c> levanta los contenedores con
/// <c>docker compose -f docker-compose.local.yml up -d --build --wait</c> antes
/// de ejecutarlos. Ejecución local equivalente:
/// <code>
/// docker compose -f docker-compose.local.yml up -d --build
/// dotnet test test/Armali.Horizon.Smoke.Tests
/// </code>
/// </para>
/// <para>
/// Validan tres aspectos:
/// <list type="bullet">
///   <item>Cada app expone <c>/health</c> respondiendo 200.</item>
///   <item>Identity acepta login con el usuario seed y devuelve un token válido.</item>
///   <item>Segaris y Autoconfig contestan a peticiones request/response sobre el bus IO.</item>
/// </list>
/// Las páginas/operaciones que dependen de Azure Data Lake quedan fuera del smoke
/// porque CI no dispone de <c>DATALAKE_ACCOUNT_KEY</c>.
/// </para>
/// </summary>
[TestClass]
public class HorizonStackSmokeTests
{
    // Endpoints por defecto cuando el compose está expuesto en localhost. Pueden
    // sobreescribirse con variables de entorno para que el mismo proyecto sirva
    // contra un stack remoto.
    private static readonly string RedisEndpoint =
        Environment.GetEnvironmentVariable("SMOKE_REDIS_ENDPOINT") ?? "localhost:6379";
    private static readonly string IdentityHealth =
        Environment.GetEnvironmentVariable("SMOKE_IDENTITY_HEALTH") ?? "http://localhost:5149/health";
    private static readonly string SegarisHealth =
        Environment.GetEnvironmentVariable("SMOKE_SEGARIS_HEALTH") ?? "http://localhost:5122/health";
    private static readonly string AutoconfigHealth =
        Environment.GetEnvironmentVariable("SMOKE_AUTOCONFIG_HEALTH") ?? "http://localhost:5004/health";
    private static readonly string McpHealth =
        Environment.GetEnvironmentVariable("SMOKE_MCP_HEALTH") ?? "http://localhost:5180/health";

    private static readonly string SeedUser =
        Environment.GetEnvironmentVariable("IDENTITY_SEED_USER") ?? "armali";
    private static readonly string SeedPassword =
        Environment.GetEnvironmentVariable("IDENTITY_SEED_PASSWORD") ?? "armali";

    private static HorizonEventService Events = null!;
    private static HttpClient Http = null!;

    [ClassInitialize]
    public static async Task ClassInit(TestContext _)
    {
        // Configura el bus para que apunte al Redis del compose. Timeout amplio
        // para tolerar arranques lentos en CI.
        HorizonEventService.Settings = new HorizonEventSettings
        {
            Endpoint = RedisEndpoint,
            DefaultTimeoutSeconds = 30,
        };

        // Construimos manualmente el HorizonEventService como cliente puro: no
        // registra handlers, sólo emite RequestAsync hacia los servicios reales.
        var sp = new ServiceCollection().BuildServiceProvider();
        Events = new HorizonEventService(sp, NullLogger<HorizonEventService>.Instance);
        await Events.StartAsync(CancellationToken.None);

        Http = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        if (Events is not null)
            await Events.StopAsync(CancellationToken.None);
        Http?.Dispose();
    }

    [TestMethod, TestCategory("Smoke")]
    public async Task HealthEndpoints_AllAppsRespondOk()
    {
        foreach (var url in new[] { IdentityHealth, SegarisHealth, AutoconfigHealth, McpHealth })
        {
            var resp = await Http.GetAsync(url);
            resp.IsSuccessStatusCode.ShouldBeTrue(
                $"{url} respondió {(int)resp.StatusCode} {resp.StatusCode}");
        }
    }

    [TestMethod, TestCategory("Smoke")]
    public async Task Identity_LoginAndWhoAmI_WithSeedUser()
    {
        var auth = new HorizonAuthClient(Events);
        var login = await auth.LoginAsync(SeedUser, SeedPassword);

        login.Success.ShouldBeTrue($"Login fallido: {login.Error?.Code} - {login.Error?.Message}");
        login.Identity.ShouldNotBeNull();
        auth.Token.ShouldNotBeNullOrEmpty();

        var me = await auth.WhoAmIAsync();
        me.ShouldNotBeNull();
        me!.UserName.ShouldBe(SeedUser);
    }

    [TestMethod, TestCategory("Smoke")]
    public async Task Segaris_ListProjectStatuses_RespondsForAuthedUser()
    {
        // Validamos la cadena MCP/cliente → Redis → Segaris → Identity (auth) → Segaris (DB).
        // Operación elegida porque no depende de Data Lake y devuelve éxito aunque la
        // tabla esté vacía.
        var auth = new HorizonAuthClient(Events);
        (await auth.LoginAsync(SeedUser, SeedPassword)).Success.ShouldBeTrue();

        var segaris = new HorizonSegarisClient(Events, auth.Token);
        var res = await segaris.ListProjectStatusesAsync();

        res.ShouldNotBeNull();
        res.Success.ShouldBeTrue($"Segaris devolvió error: {res.Error?.Code} - {res.Error?.Message}");
        res.Statuses.ShouldNotBeNull();
    }

    [TestMethod, TestCategory("Smoke")]
    public async Task Segaris_RejectsRequestWithInvalidToken()
    {
        // Confirma que el handler valida el token contra Identity y no contesta
        // datos a tokens basura.
        var segaris = new HorizonSegarisClient(Events, "token-invalido-smoke");
        var res = await segaris.ListProjectStatusesAsync();

        res.ShouldNotBeNull();
        res.Success.ShouldBeFalse();
        res.Error.ShouldNotBeNull();
    }

    [TestMethod, TestCategory("Smoke")]
    public async Task Autoconfig_GetConfigFile_RespondsEvenIfNotFound()
    {
        // No esperamos un archivo concreto: sólo validamos que el handler responde
        // (no hay timeout) y que el contrato request/response se mantiene.
        var ac = new HorizonAutoconfigClient(Events);
        var res = await ac.GetConfigFileAsync(
            nodeName: "smoke-node",
            appName: "smoke-app",
            version: "1.0.0",
            fileName: "smoke.json");

        res.ShouldNotBeNull();
        res.Found.ShouldBeFalse();
        res.Error.ShouldNotBeNull();
    }
}

