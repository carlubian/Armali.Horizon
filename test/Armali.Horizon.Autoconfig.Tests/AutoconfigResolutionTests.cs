using Armali.Horizon.Autoconfig.Handlers;
using Armali.Horizon.Autoconfig.Model;
using Armali.Horizon.Autoconfig.Services;

namespace Armali.Horizon.Autoconfig.Tests;

/// <summary>
/// Pruebas de la lógica de resolución de versiones de <see cref="AutoconfigService.ResolveBestVersionFileAsync"/>.
/// El Datalake no se invoca: sólo se valida la elección de versión y de archivo en BD.
/// </summary>
[TestClass]
public class AutoconfigResolutionTests
{
    private TestDbContextFactory _factory = null!;
    private AutoconfigService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new TestDbContextFactory();
        _service = new AutoconfigService(_factory, null!);
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    // ── Helpers ──────────────────────────────────────────────

    private async Task<(int nodeId, int appId)> CrearNodeApp(string node = "node1", string app = "app1")
    {
        await using var ctx = _factory.CreateDbContext();
        var n = new AutoconfigNode { Name = node };
        ctx.Nodes.Add(n);
        await ctx.SaveChangesAsync();
        var a = new AutoconfigApp { Name = app, NodeId = n.Id };
        ctx.Apps.Add(a);
        await ctx.SaveChangesAsync();
        return (n.Id, a.Id);
    }

    private async Task<int> InsertarVersion(int appId, int major, int minor, int patch, params string[] files)
    {
        await using var ctx = _factory.CreateDbContext();
        var v = new AutoconfigVersion { AppId = appId, Major = major, Minor = minor, Patch = patch, Date = DateTime.UtcNow };
        ctx.Versions.Add(v);
        await ctx.SaveChangesAsync();
        foreach (var f in files)
            ctx.Files.Add(new AutoconfigFile { VersionId = v.Id, Name = f, KbSize = 1 });
        await ctx.SaveChangesAsync();
        return v.Id;
    }

    // ── Reglas de fallback ──────────────────────────────────

    [TestMethod]
    public async Task Resuelve_CoincidenciaExacta()
    {
        var (_, app) = await CrearNodeApp();
        await InsertarVersion(app, 1, 2, 3, "config.json");
        await InsertarVersion(app, 1, 2, 5, "config.json");

        var result = await _service.ResolveBestVersionFileAsync("node1", "app1", 1, 2, 3, "config.json");

        result.ShouldNotBeNull();
        result!.Value.Context.VersionName.ShouldBe("1.2.3");
    }

    [TestMethod]
    public async Task Resuelve_PatchMasReciente_CuandoNoExiste()
    {
        var (_, app) = await CrearNodeApp();
        await InsertarVersion(app, 1, 2, 1, "config.json");
        await InsertarVersion(app, 1, 2, 4, "config.json");
        await InsertarVersion(app, 1, 2, 2, "config.json");

        var result = await _service.ResolveBestVersionFileAsync("node1", "app1", 1, 2, 3, "config.json");

        result.ShouldNotBeNull();
        result!.Value.Context.VersionName.ShouldBe("1.2.4");
    }

    [TestMethod]
    public async Task Resuelve_MinorMasReciente_CuandoNoHayPatchCompatible()
    {
        var (_, app) = await CrearNodeApp();
        // Sólo hay otros Minor dentro del mismo Major.
        await InsertarVersion(app, 1, 1, 9, "config.json");
        await InsertarVersion(app, 1, 4, 0, "config.json");
        await InsertarVersion(app, 1, 3, 7, "config.json");

        var result = await _service.ResolveBestVersionFileAsync("node1", "app1", 1, 2, 3, "config.json");

        result.ShouldNotBeNull();
        result!.Value.Context.VersionName.ShouldBe("1.4.0");
    }

    [TestMethod]
    public async Task Devuelve_Null_SiNoHayMajorCompatible()
    {
        var (_, app) = await CrearNodeApp();
        await InsertarVersion(app, 2, 0, 0, "config.json");
        await InsertarVersion(app, 3, 1, 0, "config.json");

        var result = await _service.ResolveBestVersionFileAsync("node1", "app1", 1, 2, 3, "config.json");

        result.ShouldBeNull();
    }

    [TestMethod]
    public async Task Sigue_Buscando_SiVersionCompatibleNoTieneArchivo()
    {
        var (_, app) = await CrearNodeApp();
        // Versión exacta sin el archivo → debe pasar a 1.2.* con patch más alto que tenga el archivo.
        await InsertarVersion(app, 1, 2, 3, "otra.json");
        await InsertarVersion(app, 1, 2, 5, "otra.json");
        await InsertarVersion(app, 1, 2, 2, "config.json");
        // Existe también una versión con minor distinto, pero la 1.2.2 (mismo minor) gana sobre 1.5.0.
        await InsertarVersion(app, 1, 5, 0, "config.json");

        var result = await _service.ResolveBestVersionFileAsync("node1", "app1", 1, 2, 3, "config.json");

        result.ShouldNotBeNull();
        result!.Value.Context.VersionName.ShouldBe("1.2.2");
    }

    [TestMethod]
    public async Task Sigue_Buscando_AOtroMinor_SiNingunPatchTieneArchivo()
    {
        var (_, app) = await CrearNodeApp();
        await InsertarVersion(app, 1, 2, 3, "otra.json");
        await InsertarVersion(app, 1, 2, 5, "otra.json");
        await InsertarVersion(app, 1, 1, 9, "config.json");
        await InsertarVersion(app, 1, 4, 0, "config.json");

        var result = await _service.ResolveBestVersionFileAsync("node1", "app1", 1, 2, 3, "config.json");

        result.ShouldNotBeNull();
        result!.Value.Context.VersionName.ShouldBe("1.4.0");
    }

    [TestMethod]
    public async Task Devuelve_Null_SiNodeNoExiste()
    {
        var (_, app) = await CrearNodeApp();
        await InsertarVersion(app, 1, 2, 3, "config.json");

        var result = await _service.ResolveBestVersionFileAsync("nope", "app1", 1, 2, 3, "config.json");

        result.ShouldBeNull();
    }

    [TestMethod]
    public async Task Devuelve_Null_SiAppNoExisteEnElNode()
    {
        await CrearNodeApp("node1", "app1");

        var result = await _service.ResolveBestVersionFileAsync("node1", "otra", 1, 2, 3, "config.json");

        result.ShouldBeNull();
    }

    // ── Parser de versión ───────────────────────────────────

    [TestMethod]
    [DataRow("1.2.3", true, 1, 2, 3)]
    [DataRow("0.0.0", true, 0, 0, 0)]
    [DataRow("10.20.30", true, 10, 20, 30)]
    [DataRow("1.2", false, 0, 0, 0)]
    [DataRow("1.2.3.4", false, 0, 0, 0)]
    [DataRow("a.b.c", false, 0, 0, 0)]
    [DataRow("-1.0.0", false, 0, 0, 0)]
    [DataRow("", false, 0, 0, 0)]
    public void TryParseVersion_Casos(string raw, bool ok, int major, int minor, int patch)
    {
        var result = GetConfigFileHandler.TryParseVersion(raw, out var m, out var n, out var p);
        result.ShouldBe(ok);
        if (ok)
        {
            m.ShouldBe(major);
            n.ShouldBe(minor);
            p.ShouldBe(patch);
        }
    }
}

