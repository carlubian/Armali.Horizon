using Armali.Horizon.Autoconfig.Model;
using Armali.Horizon.Autoconfig.Services;

namespace Armali.Horizon.Autoconfig.Tests;

[TestClass]
public class AutoconfigServiceTests
{
    private TestDbContextFactory _factory = null!;
    private AutoconfigService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _factory = new TestDbContextFactory();
        // AutoconfigDatalakeService es null — solo se testean operaciones de DB.
        // Los deletes no invocan el Datalake si no hay ficheros asociados.
        _service = new AutoconfigService(_factory, null!);
    }

    [TestCleanup]
    public void Cleanup() => _factory.Dispose();

    // ── Helpers ──────────────────────────────────────────────

    private async Task<AutoconfigNode> CrearNodo(string name = "TestNode")
    {
        var node = new AutoconfigNode { Name = name, Uri = "https://example.com" };
        await _service.AddNode(node);
        return node;
    }

    private async Task InsertarApp(int nodeId, string name = "TestApp")
    {
        // Insertamos directamente via DbContext porque el servicio no expone AddApp
        await using var ctx = _factory.CreateDbContext();
        ctx.Apps.Add(new AutoconfigApp { Name = name, NodeId = nodeId });
        await ctx.SaveChangesAsync();
    }

    // ── Node CRUD ────────────────────────────────────────────

    [TestMethod]
    public async Task AddNode_AndRetrieve_ReturnsNode()
    {
        var node = await CrearNodo("Servidor A");

        var all = await _service.GetNodes();
        all.Count.ShouldBe(1);
        all[0].Name.ShouldBe("Servidor A");
    }

    [TestMethod]
    public async Task UpdateNode_ModifiesNode()
    {
        var node = await CrearNodo("Original");
        node.Name = "Modificado";
        await _service.UpdateNode(node);

        var all = await _service.GetNodes();
        all[0].Name.ShouldBe("Modificado");
    }

    [TestMethod]
    public async Task DeleteNode_RemovesNode()
    {
        var node = await CrearNodo();
        await _service.DeleteNode(node.Id);

        var all = await _service.GetNodes();
        all.ShouldBeEmpty();
    }

    [TestMethod]
    public async Task DeleteNode_NonExistentId_DoesNotThrow()
    {
        await Should.NotThrowAsync(() => _service.DeleteNode(9999));
    }

    // ── GetAppsIn ────────────────────────────────────────────

    [TestMethod]
    public async Task GetAppsIn_FiltersByNodeId()
    {
        var node1 = await CrearNodo("Node1");
        var node2 = await CrearNodo("Node2");

        await InsertarApp(node1.Id, "App1");
        await InsertarApp(node1.Id, "App2");
        await InsertarApp(node2.Id, "App3");

        var appsNode1 = await _service.GetAppsIn(node1.Id);
        appsNode1.Count.ShouldBe(2);

        var appsNode2 = await _service.GetAppsIn(node2.Id);
        appsNode2.Count.ShouldBe(1);
        appsNode2[0].Name.ShouldBe("App3");
    }

    // ── GetNodeStats ─────────────────────────────────────────

    [TestMethod]
    public async Task GetNodeStats_CalculatesCorrectly()
    {
        // Crear jerarquía: Node → App → Version → Files
        var node = await CrearNodo();

        await using var ctx = _factory.CreateDbContext();
        var app = new AutoconfigApp { Name = "App1", NodeId = node.Id };
        ctx.Apps.Add(app);
        await ctx.SaveChangesAsync();

        var version = new AutoconfigVersion { Major = 1, Minor = 0, Patch = 0, AppId = app.Id };
        ctx.Versions.Add(version);
        await ctx.SaveChangesAsync();

        ctx.Files.Add(new AutoconfigFile { Name = "config.json", KbSize = 100, VersionId = version.Id });
        ctx.Files.Add(new AutoconfigFile { Name = "data.bin", KbSize = 250, VersionId = version.Id });
        await ctx.SaveChangesAsync();

        var stats = await _service.GetNodeStats(node.Id);

        stats.AppCount.ShouldBe(1);
        stats.TotalKbSize.ShouldBe(350);
    }

    [TestMethod]
    public async Task GetNodeStats_NodeWithNoApps_ReturnsZeros()
    {
        var node = await CrearNodo();

        var stats = await _service.GetNodeStats(node.Id);

        stats.AppCount.ShouldBe(0);
        stats.TotalKbSize.ShouldBe(0);
    }
}

