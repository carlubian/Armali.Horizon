# AGENTS.md

## Architecture

This is a .NET 10 Blazor Server solution (`.slnx`) with five projects in a layered dependency graph:

```
Armali.Horizon.Core  (shared: logging via Serilog/Seq, identity model, LINQ extensions)
    ↑
Armali.Horizon.IO    (Redis pub/sub events with Zstd-compressed payloads)
Armali.Horizon.Blazor (reusable Razor component library + Tailwind CSS design system)
    ↑
Armali.Horizon.Segaris     (Blazor Server app — EF Core SQLite, domain services, pages)
Armali.Horizon.Autoconfig  (Blazor Server app — config provisioning, EF Core SQLite)
```

**Segaris** and **Autoconfig** are runnable applications; **Blazor** and **Core** are reusable libraries shared across apps. Segaris depends on Blazor → Core. Autoconfig depends on Blazor → Core and also IO.

The `test/` folder mirrors the main code with four MSTest projects: `Armali.Horizon.Core.Tests`, `Armali.Horizon.IO.Tests`, `Armali.Horizon.Segaris.Tests`, and `Armali.Horizon.Autoconfig.Tests`.

## Build & Run

- **Tailwind CSS** is compiled at build time via a local `tailwindcss.exe` binary (no npm). All three app projects (`Armali.Horizon.Blazor`, `Armali.Horizon.Segaris`, `Armali.Horizon.Autoconfig`) have a `TailwindBuild` MSBuild target that runs `.\tailwindcss.exe -i .\wwwroot\app.css -o .\wwwroot\tailwind.css --minify`. Pass `/p:SkipTailwindBuild=true` to skip it (used in Docker builds after a manual Tailwind step).
- Run Segaris: `dotnet run --project src/Armali.Horizon.Segaris`
- Run Autoconfig: `dotnet run --project src/Armali.Horizon.Autoconfig`
- Run all tests: `dotnet test Armali.Horizon.slnx /p:SkipTailwindBuild=true`
- EF Core migrations (Segaris): `dotnet ef migrations add <Name> --project src/Armali.Horizon.Segaris`
- EF Core migrations (Autoconfig): `dotnet ef migrations add <Name> --project src/Armali.Horizon.Autoconfig`
- Docker (single): `docker build -f src/Armali.Horizon.Segaris/Dockerfile .` or `docker build -f src/Armali.Horizon.Autoconfig/Dockerfile .`
- Docker (compose): `docker-compose up` — `docker-compose.yml` is at the repository root

## Design System & Styling

All colors and fonts are defined as CSS custom properties in `src/Armali.Horizon.Blazor/wwwroot/app.css` under `@theme`. Use the `hz-*` prefix for all color tokens (e.g., `bg-hz-opera`, `text-hz-arendelle`). Key tokens:
- **hz-styx / hz-spartan / hz-gray**: background tones (dark); `hz-gray-light` for lighter variant
- **hz-arendelle**: primary text color (light); **hz-artemisia**: secondary text color (cool light)
- **hz-opera** (green), **hz-tribunal** (red), **hz-apollo** (blue), **hz-sol** (yellow): accent actions — each has a `*-light` hover variant (e.g., `hz-opera-light`)
- **hz-tefiti** (teal), **hz-meridian** (gold): additional accent tokens
- Fonts: `font-hz-header` (League Spartan), `font-hz-text` (Nunito Medium), `font-hz-special` (Nunito Extrabold)

Icons use **Font Awesome 6 Solid** (`fa-solid fa-*`). Never import other icon sets.

## Component Patterns (Armali.Horizon.Blazor)

### Page layout
Every Segaris page follows this skeleton — see `Capex.razor` as the canonical example:
```razor
<HorizonAuthentication @bind-User="CurrentUser" OnAuthenticated="LoadData" />
<HorizonLayout AppName="Segaris" PageName="PageTitle">
    <Sidebar> ... HorizonHeroButton ... </Sidebar>
    <SidebarBottom> ... </SidebarBottom>
    <Body> ... HorizonTable ... </Body>
</HorizonLayout>
```
`OnAuthenticated` fires after the user session is resolved — use it to trigger initial data loads that depend on the current user identity.

Autoconfig pages reuse the same authentication/layout/table shell — see `src/Armali.Horizon.Autoconfig/Components/Pages/Nodes.razor`.

### Table with filtering
`HorizonTable<TItem>` is generic and paginated. Columns are declared with `<HorizonCellHeader>` (set `Filterable="true"` + `ValueSelector` for filter support). Rows use `<RowTemplate>` with typed cell components (`HorizonCellText`, `HorizonCellTwoLine`, `HorizonCellCurrency`, `HorizonCellDateTime`, `HorizonCellStatus`, `HorizonCellBoolean`, `HorizonCellButtons`). For non-data-bound reference tables use `HorizonStaticTable` with `HorizonStaticTableHeader` / `HorizonStaticTableRow` / `HorizonStaticTableCell`.

### CRUD popup flow
All modules use the same pattern: `PopupIntent` enum (`None`, `Create`, `Edit`) + `ShowEditPopup`/`ShowDeletePopup` bools controlling `<HorizonPopup>` → `<HorizonDialog>` → `<HorizonForm>` with `<HorizonFormCell>` wrappers.

When an FK link needs a searchable chooser instead of a plain combobox, pages use `HorizonEntityLink` plus selector popups such as `ProjectSelectorPopup` (`Capex.razor`, `Opex.razor`, `Travel.razor`, `Assets.razor`) and `AssetSelectorPopup` (`Maintenance.razor`).

### Model marker interfaces
Domain models implement interfaces from `src/Armali.Horizon.Blazor/Utils.cs`: `Identifiable` (has `Id`), `Nameable` (has `Name`), `Colorable` (has `Color`). These are required by components like `HorizonCellStatus` and the `Utils.Index()` helper.

## Domain Module Convention (Segaris)

Each business module (Capex, Opex, Assets, Travel, Project, Archive, Maintenance, Firebird, Clothes, Mood, Inventory) follows the **same triple**:
1. **Model** (`Model/{Module}Model.cs`): `{Module}Entity`, `{Module}Category`, `{Module}Status` — entities use FK integers (`CategoryId`, `StatusId`) with `[ForeignKey]` attributes. Some modules add extra lookup types (e.g., `TravelCostCenter`, `ProjectProgram`, `ClothesWashType`) or sub-entities (`OpexSubEntity`, `TravelSubEntity`, `ProjectSubEntity`, `FirebirdSubEntity`, `InvOrderSubEntity`).
2. **Service** (`Services/{Module}Service.cs`): scoped DI service that injects `IDbContextFactory<SegarisDbContext>` and creates short-lived `DbContext` instances per operation (`await using var context = Factory.CreateDbContext()`). `DatalakeService` is a shared service for Azure Data Lake file operations (used by Archive and Project modules).
3. **Page** (`Components/Pages/{Module}.razor`): Razor page using the layout/table/popup pattern above. Some modules intentionally span multiple pages while keeping one model/service layer — Inventory uses `InvVendors.razor`, `InvItems.razor`, and `InvOrder.razor`; companion info/calendar pages include `AssetsCode.razor`, `ClothesCode.razor`, and `FirebirdCalendar.razor`.

When adding a new module, replicate this triple and register the service in `Program.cs` with `builder.Services.AddScoped<{Module}Service>()`.

## Key Conventions

- **Authentication**: `HorizonSessionService` stores `HorizonIdentity` in browser `localStorage` under key `"Horizon:Session"`. Pages gate access via `<HorizonAuthentication>` which redirects to `/horizon/login` if unauthenticated.
- **Privacy model**: entities with user-scoped visibility have `bool IsPrivate` + `string Creator` fields. Services filter with `.Where(e => !e.IsPrivate || e.Creator == userId)`. Use `Utils.PrivacyIcon()` / `Utils.PrivacyColor()` for rendering the lock/globe/share icon in `HorizonCellTwoLine`.
- **DbContextFactory pattern**: services inject `IDbContextFactory<SegarisDbContext>` (not `SegarisDbContext` directly) and create short-lived contexts per method. This avoids concurrency issues in Blazor Server's long-lived scopes.
- **Autoconfig service pattern**: `AutoconfigService` mirrors the same approach with `IDbContextFactory<AutoconfigDbContext>` and short-lived contexts per method.
- **Production migrations**: both apps call `Database.Migrate()` automatically when `!app.Environment.IsDevelopment()` in `Program.cs`.
- **Static helper `Index()`**: imported globally via `using static Armali.Horizon.Segaris.Services.Utils` in `_Imports.razor` — used to look up Category/Status by FK id in Razor templates.
- **No Include() on EF queries**: services use `AsNoTracking()` and resolve FK references via the `Index()` helper at render time instead of eager loading navigation properties.
- **Data Lake credential**: `src/Armali.Horizon.Segaris/Services/DatalakeService.cs` reads `DATALAKE_ACCOUNT_KEY` from the environment in its constructor; Archive/Project file operations depend on it.
- **Tests**: `test/Directory.Build.props` centralizes `net10.0`, MSTest, and Shouldly. Service tests instantiate services directly with `TestDbContextFactory` (`SQLite in-memory`) instead of booting the Blazor apps.
- **C# 14 extensions**: `src/Armali.Horizon.Core/Linq/HorizonExtensions.cs` uses the new `extension<T>` syntax (requires .NET 10 / C# 14).
- **Comments and docs in Spanish**: inline code comments are written in Spanish; public API summaries may be in either language.

## Inter-Process Communication (Armali.Horizon.IO)

### Overview

`Armali.Horizon.IO` provides a Redis pub/sub messaging layer with Zstd-compressed payloads. It supports two communication patterns:

| Pattern | Use case |
|---|---|
| **Fire-and-forget** | `PublishAsync<T>` + `Subscribe<T>` — emit events without expecting a reply |
| **Request/Response** | `RequestAsync<TResponse>` (caller) + `IHorizonRequestHandler<TReq,TRes>` (responder) — send a request and await a typed response with timeout |

Every message travels inside a `HorizonEvent` envelope:
```
HorizonEvent
├── EventId        (Guid — unique per message)
├── EventType      (string — identifies the operation, e.g. "autoconfig.nodes.get")
├── Payload        (byte[] — Zstd-compressed, JSON-serialized payload)
├── CorrelationId  (Guid? — links a request to its response; null for fire-and-forget)
└── ReplyTo        (string? — Redis channel for the response; null for fire-and-forget)
```

### Configuration

Add to `appsettings.json`:
```json
{
  "Horizon": {
    "Events": {
      "Endpoint": "localhost:6379",
      "DefaultTimeoutSeconds": 10
    }
  }
}
```

### Registering the event system

In `Program.cs`, call `UseHorizonEvents()` on the host builder:

```csharp
// Requester only (fire-and-forget + RequestAsync, no handlers):
builder.Host.UseHorizonEvents();

// Responder with handlers:
builder.Host.UseHorizonEvents(events =>
{
    events.HandleRequest<GetNodesHandler, GetNodesRequest, GetNodesResponse>("autoconfig");
    events.HandleRequest<GetAppsHandler, GetAppsRequest, GetAppsResponse>("autoconfig");
});
```

`HorizonEventService` is registered as a singleton and can be injected directly into any service or page.

### Defining payloads (models)

Each operation needs a request and a response class implementing `IHorizonEventPayload`. The `EventType` string is the key that routes requests to the correct handler.

```csharp
// ── Request ──
public class GetNodesRequest : IHorizonEventPayload
{
    public string EventType => "autoconfig.nodes.get";
}

// ── Response ──
public class GetNodesResponse : IHorizonEventPayload
{
    public string EventType => "autoconfig.nodes.get:response";
    public List<AutoconfigNode> Nodes { get; set; } = [];
}
```

**Convention**: use `"service.resource.action"` for request EventType, and append `":response"` for the response EventType.

### Implementing a handler (responder side)

```csharp
public class GetNodesHandler : IHorizonRequestHandler<GetNodesRequest, GetNodesResponse>
{
    private readonly AutoconfigService Svc;
    public GetNodesHandler(AutoconfigService svc) => Svc = svc;

    public async Task<GetNodesResponse> HandleAsync(GetNodesRequest request, CancellationToken ct = default)
    {
        var nodes = await Svc.GetNodes();
        return new GetNodesResponse { Nodes = nodes };
    }
}
```

Handlers are resolved from DI with a **new scope per request**, so they can safely inject scoped services like `AutoconfigService`.

### Sending a request (caller side)

```csharp
// Inject HorizonEventService in your service or page
var response = await EventService.RequestAsync<GetNodesResponse>(
    "autoconfig",                   // Redis channel
    new GetNodesRequest(),          // request payload
    TimeSpan.FromSeconds(5));       // optional timeout override

var nodes = response.Nodes;         // typed result
```

### Adding a new operation (checklist)

1. **Define request + response payload classes** implementing `IHorizonEventPayload` (request needs parameterless constructor)
2. **Implement `IHorizonRequestHandler<TReq, TRes>`** with the business logic
3. **Register** the handler in `UseHorizonEvents(events => { events.HandleRequest<...>(...); })`
4. **Call** `RequestAsync<TResponse>(channel, request)` from the caller side

No changes to the IO library itself are needed — only new model classes and handler implementations.

### Fire-and-forget usage

```csharp
// Publisher
await eventService.PublishAsync("notifications", new AlertPayload { ... });

// Subscriber (call once, e.g. in StartAsync or OnInitialized)
eventService.Subscribe<AlertPayload>("notifications", payload =>
{
    Console.WriteLine($"Alert: {payload.Message}");
});
```


