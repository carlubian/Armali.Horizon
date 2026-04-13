using Armali.Horizon.Autoconfig.Components.Pages;
using Armali.Horizon.Autoconfig.Model;
using Armali.Horizon.Autoconfig.Services;
using Bunit;

namespace Armali.Horizon.Autoconfig.Tests;

[TestClass]
public class NodesPageTests : BlazorTestBase
{
    // ── Renderizado inicial ─────────────────────────────────

    [TestMethod]
    public void Page_RendersTableHeaders()
    {
        var cut = Render<Nodes>();

        var headers = cut.FindAll("th");
        headers.Count.ShouldBeGreaterThanOrEqualTo(4);

        var headerTexts = headers.Select(h => h.TextContent.Trim()).ToList();
        headerTexts.ShouldContain("Name");
        headerTexts.ShouldContain("Uri");
        headerTexts.ShouldContain("Info");
        headerTexts.ShouldContain("Actions");
    }

    [TestMethod]
    public void Page_RendersAddNodeButton()
    {
        var cut = Render<Nodes>();

        cut.Markup.ShouldContain("Add Node");
    }

    // ── Tabla con datos ─────────────────────────────────────

    [TestMethod]
    public async Task Page_ShowsNodesInTable()
    {
        // Arrange: insertar un nodo de prueba
        var service = new AutoconfigService(DbFactory);
        await service.AddNode(new AutoconfigNode
        {
            Name = "Node Alpha",
            Uri = "https://alpha.example.com",
            IconHint = "fa-server"
        });

        var cut = Render<Nodes>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.ShouldContain("Node Alpha");
            cut.Markup.ShouldContain("https://alpha.example.com");
        });
    }

    [TestMethod]
    public async Task Page_ShowsMultipleNodes()
    {
        var service = new AutoconfigService(DbFactory);
        await service.AddNode(new AutoconfigNode { Name = "Node A", Uri = "https://a.test" });
        await service.AddNode(new AutoconfigNode { Name = "Node B", Uri = "https://b.test" });

        var cut = Render<Nodes>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.ShouldContain("Node A");
            cut.Markup.ShouldContain("Node B");
        });
    }

    // ── Popup de creación ───────────────────────────────────

    [TestMethod]
    public void AddNode_Click_ShowsCreatePopup()
    {
        var cut = Render<Nodes>();

        var addButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Add Node"));
        addButton.Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.ShouldContain("New Node");
        });
    }

    [TestMethod]
    public void CreatePopup_ContainsFormFields()
    {
        var cut = Render<Nodes>();

        var addButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Add Node"));
        addButton.Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.ShouldContain("Name");
            cut.Markup.ShouldContain("Uri");
            cut.Markup.ShouldContain("Save");
            cut.Markup.ShouldContain("Cancel");
        });
    }

    // ── Popup de eliminación ────────────────────────────────

    [TestMethod]
    public async Task DeleteButton_Click_ShowsConfirmation()
    {
        var service = new AutoconfigService(DbFactory);
        await service.AddNode(new AutoconfigNode
        {
            Name = "To Delete Node",
            Uri = "https://delete.test"
        });

        var cut = Render<Nodes>();

        cut.WaitForAssertion(() => cut.Markup.ShouldContain("To Delete Node"));

        var trashButton = cut.FindAll("button")
            .First(b => b.InnerHtml.Contains("fa-trash"));
        trashButton.Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.ShouldContain("Confirm delete");
            cut.Markup.ShouldContain("To Delete Node");
        });
    }

    // ── Popup de edición ────────────────────────────────────

    [TestMethod]
    public async Task EditButton_Click_ShowsEditPopup()
    {
        var service = new AutoconfigService(DbFactory);
        await service.AddNode(new AutoconfigNode
        {
            Name = "Editable Node",
            Uri = "https://edit.test"
        });

        var cut = Render<Nodes>();

        cut.WaitForAssertion(() => cut.Markup.ShouldContain("Editable Node"));

        var editButton = cut.FindAll("button")
            .First(b => b.InnerHtml.Contains("fa-pen"));
        editButton.Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.ShouldContain("Edit Node");
        });
    }

    // ── Estadísticas en la tabla ────────────────────────────

    [TestMethod]
    public async Task Page_ShowsNodeStats()
    {
        var service = new AutoconfigService(DbFactory);
        await service.AddNode(new AutoconfigNode { Name = "Stats Node", Uri = "https://stats.test" });

        var cut = Render<Nodes>();

        // Un nodo sin apps debe mostrar "0 apps"
        cut.WaitForAssertion(() =>
        {
            cut.Markup.ShouldContain("Stats Node");
            cut.Markup.ShouldContain("0 apps");
        });
    }
}
