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
Armali.Horizon.Autoconfig  (Blazor Server app — config provisioning, EF Core SQLite, Azure Data Lake)
```

**Segaris** and **Autoconfig** are runnable applications; **Blazor** and **Core** are reusable libraries shared across apps. Segaris depends on Blazor → Core. Autoconfig depends on Blazor → Core and also IO.

## Build & Run

- **Tailwind CSS** is compiled at build time via a local `tailwindcss.exe` binary (no npm). All three app projects (`Armali.Horizon.Blazor`, `Armali.Horizon.Segaris`, `Armali.Horizon.Autoconfig`) have a `TailwindBuild` MSBuild target that runs `.\tailwindcss.exe -i .\wwwroot\app.css -o .\wwwroot\tailwind.css --minify`. Pass `/p:SkipTailwindBuild=true` to skip it (used in Docker builds after a manual Tailwind step).
- Run Segaris: `dotnet run --project src/Armali.Horizon.Segaris`
- Run Autoconfig: `dotnet run --project src/Armali.Horizon.Autoconfig`
- EF Core migrations (Segaris): `dotnet ef migrations add <Name> --project src/Armali.Horizon.Segaris`
- EF Core migrations (Autoconfig): `dotnet ef migrations add <Name> --project src/Armali.Horizon.Autoconfig`
- Docker (single): `docker build -f src/Armali.Horizon.Segaris/Dockerfile .` or `docker build -f src/Armali.Horizon.Autoconfig/Dockerfile .`
- Docker (compose): `docker-compose up` — `docker-compose.yml` at root defines both services

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

### Table with filtering
`HorizonTable<TItem>` is generic and paginated. Columns are declared with `<HorizonCellHeader>` (set `Filterable="true"` + `ValueSelector` for filter support). Rows use `<RowTemplate>` with typed cell components (`HorizonCellText`, `HorizonCellTwoLine`, `HorizonCellCurrency`, `HorizonCellDateTime`, `HorizonCellStatus`, `HorizonCellBoolean`, `HorizonCellButtons`). For non-data-bound reference tables use `HorizonStaticTable` with `HorizonStaticTableHeader` / `HorizonStaticTableRow` / `HorizonStaticTableCell`.

### CRUD popup flow
All modules use the same pattern: `PopupIntent` enum (`None`, `Create`, `Edit`) + `ShowEditPopup`/`ShowDeletePopup` bools controlling `<HorizonPopup>` → `<HorizonDialog>` → `<HorizonForm>` with `<HorizonFormCell>` wrappers.

### Model marker interfaces
Domain models implement interfaces from `src/Armali.Horizon.Blazor/Utils.cs`: `Identifiable` (has `Id`), `Nameable` (has `Name`), `Colorable` (has `Color`). These are required by components like `HorizonCellStatus` and the `Utils.Index()` helper.

## Domain Module Convention (Segaris)

Each business module (Capex, Opex, Assets, Travel, Project, Archive, Maintenance, Firebird, Clothes, Mood, Inventory) follows the **same triple**:
1. **Model** (`Model/{Module}Model.cs`): `{Module}Entity`, `{Module}Category`, `{Module}Status` — entities use FK integers (`CategoryId`, `StatusId`) with `[ForeignKey]` attributes. Some modules add extra lookup types (e.g., `TravelCostCenter`, `ProjectProgram`, `ClothesWashType`) or sub-entities (`OpexSubEntity`, `TravelSubEntity`, `ProjectSubEntity`, `FirebirdSubEntity`, `InvOrderSubEntity`).
2. **Service** (`Services/{Module}Service.cs`): scoped DI service that injects `IDbContextFactory<SegarisDbContext>` and creates short-lived `DbContext` instances per operation (`await using var context = Factory.CreateDbContext()`). `DatalakeService` is a shared service for Azure Data Lake file operations (used by Archive and Project modules).
3. **Page** (`Components/Pages/{Module}.razor`): Razor page using the layout/table/popup pattern above.

When adding a new module, replicate this triple and register the service in `Program.cs` with `builder.Services.AddScoped<{Module}Service>()`.

## Key Conventions

- **Authentication**: `HorizonSessionService` stores `HorizonIdentity` in browser `localStorage` under key `"Horizon:Session"`. Pages gate access via `<HorizonAuthentication>` which redirects to `/horizon/login` if unauthenticated.
- **Privacy model**: entities with user-scoped visibility have `bool IsPrivate` + `string Creator` fields. Services filter with `.Where(e => !e.IsPrivate || e.Creator == userId)`. Use `Utils.PrivacyIcon()` / `Utils.PrivacyColor()` for rendering the lock/globe/share icon in `HorizonCellTwoLine`.
- **DbContextFactory pattern**: services inject `IDbContextFactory<SegarisDbContext>` (not `SegarisDbContext` directly) and create short-lived contexts per method. This avoids concurrency issues in Blazor Server's long-lived scopes.
- **Static helper `Index()`**: imported globally via `using static Armali.Horizon.Segaris.Services.Utils` in `_Imports.razor` — used to look up Category/Status by FK id in Razor templates.
- **No Include() on EF queries**: services use `AsNoTracking()` and resolve FK references via the `Index()` helper at render time instead of eager loading navigation properties.
- **C# 14 extensions**: `src/Armali.Horizon.Core/Linq/HorizonExtensions.cs` uses the new `extension<T>` syntax (requires .NET 10 / C# 14).
- **Comments and docs in Spanish**: inline code comments are written in Spanish; public API summaries may be in either language.

