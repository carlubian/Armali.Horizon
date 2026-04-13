using Armali.Horizon.Segaris.Components.Pages;
using Armali.Horizon.Segaris.Model;
using Armali.Horizon.Segaris.Services;
using Bunit;

namespace Armali.Horizon.Segaris.Tests;

[TestClass]
public class CapexPageTests : BlazorTestBase
{
    // ── Renderizado inicial ─────────────────────────────────

    [TestMethod]
    public void Page_RendersTableHeaders()
    {
        var cut = Render<Capex>();

        // La tabla debe contener las cabeceras definidas en el componente
        var headers = cut.FindAll("th");
        headers.Count.ShouldBeGreaterThanOrEqualTo(6);

        var headerTexts = headers.Select(h => h.TextContent.Trim()).ToList();
        headerTexts.ShouldContain("Name");
        headerTexts.ShouldContain("Date");
        headerTexts.ShouldContain("Amount");
        headerTexts.ShouldContain("Category");
        headerTexts.ShouldContain("Status");
        headerTexts.ShouldContain("Actions");
    }

    [TestMethod]
    public void Page_RendersAddExpenseButton()
    {
        var cut = Render<Capex>();

        // El botón "Add Expense" debe estar visible en el sidebar
        cut.Markup.ShouldContain("Add Expense");
    }

    // ── Tabla con datos ─────────────────────────────────────

    [TestMethod]
    public async Task Page_ShowsEntitiesInTable()
    {
        // Arrange: insertar una entidad de prueba
        var service = new CapexService(DbFactory);
        await service.AddCapex(new CapexEntity
        {
            Name = "Test Expense",
            Date = new DateTime(2026, 3, 15),
            Amount = 250.00,
            CategoryId = 1,
            StatusId = 2,
            IsPrivate = false,
            Creator = TestUserId
        });

        // Act: renderizar la página
        var cut = Render<Capex>();

        // Assert: esperar a que la entidad aparezca en el markup
        cut.WaitForAssertion(() =>
        {
            cut.Markup.ShouldContain("Test Expense");
            cut.Markup.ShouldContain("250");
        });
    }

    [TestMethod]
    public async Task Page_FiltersPrivateEntitiesFromOtherUsers()
    {
        var service = new CapexService(DbFactory);

        // Entidad pública de otro usuario — visible para todos
        await service.AddCapex(new CapexEntity
        {
            Name = "Public Item",
            Date = DateTime.Today,
            Amount = 100,
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = "otheruser"
        });

        // Entidad privada de otro usuario — NO visible para testuser
        await service.AddCapex(new CapexEntity
        {
            Name = "Private Item",
            Date = DateTime.Today,
            Amount = 999,
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = true,
            Creator = "otheruser"
        });

        var cut = Render<Capex>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.ShouldContain("Public Item");
            cut.Markup.ShouldNotContain("Private Item");
        });
    }

    // ── Popup de creación ───────────────────────────────────

    [TestMethod]
    public void AddExpense_Click_ShowsCreatePopup()
    {
        var cut = Render<Capex>();

        // Buscar y hacer clic en el botón "Add Expense"
        var addButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Add Expense"));
        addButton.Click();

        // El popup de creación debe aparecer con el título "New CAPEX"
        cut.WaitForAssertion(() =>
        {
            cut.Markup.ShouldContain("New CAPEX");
        });
    }

    [TestMethod]
    public void CreatePopup_ContainsFormFields()
    {
        var cut = Render<Capex>();

        // Abrir el popup de creación
        var addButton = cut.FindAll("button")
            .First(b => b.TextContent.Contains("Add Expense"));
        addButton.Click();

        cut.WaitForAssertion(() =>
        {
            // Debe tener los labels de los campos del formulario
            cut.Markup.ShouldContain("Name");
            cut.Markup.ShouldContain("Category");
            cut.Markup.ShouldContain("Status");
            cut.Markup.ShouldContain("Amount");
            cut.Markup.ShouldContain("Date");
            cut.Markup.ShouldContain("Project");

            // Debe tener botones Save y Cancel
            cut.Markup.ShouldContain("Save");
            cut.Markup.ShouldContain("Cancel");
        });
    }

    // ── Popup de eliminación ────────────────────────────────

    [TestMethod]
    public async Task DeleteButton_Click_ShowsConfirmation()
    {
        // Arrange: insertar entidad
        var service = new CapexService(DbFactory);
        await service.AddCapex(new CapexEntity
        {
            Name = "To Delete",
            Date = DateTime.Today,
            Amount = 50,
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = TestUserId
        });

        var cut = Render<Capex>();

        // Esperar a que la fila aparezca
        cut.WaitForAssertion(() => cut.Markup.ShouldContain("To Delete"));

        // Buscar el botón de eliminar (icono fa-trash) dentro de la fila
        var trashButton = cut.FindAll("button")
            .First(b => b.InnerHtml.Contains("fa-trash"));
        trashButton.Click();

        // El popup de confirmación debe aparecer
        cut.WaitForAssertion(() =>
        {
            cut.Markup.ShouldContain("Confirm delete");
            cut.Markup.ShouldContain("To Delete");
        });
    }

    // ── Flujo CRUD completo ─────────────────────────────────

    [TestMethod]
    public async Task EditButton_Click_ShowsEditPopup()
    {
        var service = new CapexService(DbFactory);
        await service.AddCapex(new CapexEntity
        {
            Name = "Editable Item",
            Date = DateTime.Today,
            Amount = 300,
            CategoryId = 1,
            StatusId = 1,
            IsPrivate = false,
            Creator = TestUserId
        });

        var cut = Render<Capex>();

        cut.WaitForAssertion(() => cut.Markup.ShouldContain("Editable Item"));

        // Buscar el botón de editar (icono fa-pen) dentro de la fila
        var editButton = cut.FindAll("button")
            .First(b => b.InnerHtml.Contains("fa-pen"));
        editButton.Click();

        // El popup debe mostrar "Edit CAPEX"
        cut.WaitForAssertion(() =>
        {
            cut.Markup.ShouldContain("Edit CAPEX");
        });
    }
}

