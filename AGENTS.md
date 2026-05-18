# AGENTS.md

Guía operativa para agentes que trabajen en `Armali.Horizon`. Este archivo debe reflejar el estado real del repositorio. Si una tarea cambia arquitectura, comandos, proyectos, Docker, autenticación, estilo compartido o convenciones de módulo, actualiza este documento dentro de la misma tarea.

## Reglas Para Agentes

- Antes de modificar código, revisa el contexto local con `rg`, `rg --files`, `dotnet test --list-tests` o lecturas puntuales. No asumas que este archivo está perfecto.
- Mantén los cambios dentro del patrón existente. No introduzcas frameworks, iconos, estilos, stores, clientes HTTP o abstracciones nuevas si ya hay una forma Horizon de hacerlo.
- Si cambias comportamiento compartido, añade o ajusta pruebas en el proyecto `test/` correspondiente.
- Si cambias una convención documentada aquí, edita `AGENTS.md` en el mismo cambio. Si descubres que una regla estaba obsoleta, corrígela.
- No reviertas cambios ajenos del árbol de trabajo. Ahora mismo pueden aparecer ficheros SQLite auxiliares (`*.db-wal`, `*.db-shm`) o cambios de configuración local.
- Los comentarios inline del código deben escribirse en español. Los summaries de API pueden estar en español o inglés.

## Arquitectura

Solución .NET 10 Blazor Server (`Armali.Horizon.slnx`) con siete proyectos:

| Proyecto | Rol |
|---|---|
| `Armali.Horizon.Core` | Utilidades compartidas: logging Serilog/Seq, `HorizonIdentity`, extensiones LINQ C# 14. |
| `Armali.Horizon.IO` | Bus Redis pub/sub con payloads JSON comprimidos con Zstd; soporta publish/subscribe y request/response. |
| `Armali.Horizon.Contracts` | Contratos entre apps. Concentra payloads, DTOs, canales y clientes de `Identity`, `Autoconfig` y `Segaris`. |
| `Armali.Horizon.Blazor` | Librería de componentes Razor, layout, autenticación visual, sesión local y design system Tailwind. |
| `Armali.Horizon.Segaris` | App Blazor Server principal: módulos de negocio, EF Core SQLite, Azure Data Lake. |
| `Armali.Horizon.Autoconfig` | App Blazor Server para aprovisionamiento/configuración, EF Core SQLite, Azure Data Lake. |
| `Armali.Horizon.Identity` | App Blazor Server central de usuarios, roles, sesiones y API keys; expone operaciones vía IO. |
| `Armali.Horizon.MCP` | Servidor Model Context Protocol (HTTP). Expone tools MCP que traducen a peticiones request/response sobre el bus IO. No tiene UI ni base de datos. |

Dependencias principales:

```text
Core
  <- IO
  <- Contracts
  <- Blazor

Segaris / Autoconfig / Identity
  -> Blazor
  -> Contracts
  -> IO
  -> Core

MCP
  -> Contracts
  -> IO
  -> Core
```

`Segaris`, `Autoconfig`, `Identity` y `MCP` son ejecutables. `Core`, `IO`, `Contracts` y `Blazor` son librerías compartidas.

## Comandos

- Ejecutar Segaris: `dotnet run --project src/Armali.Horizon.Segaris`
- Ejecutar Autoconfig: `dotnet run --project src/Armali.Horizon.Autoconfig`
- Ejecutar Identity: `dotnet run --project src/Armali.Horizon.Identity`
- Ejecutar MCP: `dotnet run --project src/Armali.Horizon.MCP`
- Ejecutar Althes: `dotnet run --project src/Armali.Horizon.Althes`
- Ejecutar Althes UI: `dotnet run --project src/Armali.Horizon.Althes.UI`
- Tests: `dotnet test Armali.Horizon.slnx /p:SkipTailwindBuild=true`
- Migraciones EF: `dotnet ef migrations add <Name> --project src/Armali.Horizon.<App>`
- Docker imagen individual: `docker build -f src/Armali.Horizon.<App>/Dockerfile .`
- Docker compose producción/registry: `docker compose up` o `docker compose -f docker-compose.yml up`
- Docker compose local/build: `docker compose -f docker-compose.local.yml up --build`

Tailwind se compila en build con un binario local `tailwindcss.exe` en los proyectos app y en `Armali.Horizon.Blazor`. Para builds de Docker o tests, usa `/p:SkipTailwindBuild=true` cuando Tailwind ya se ha generado aparte o no es relevante.

## Configuración Y Docker

La configuración vive bajo `Horizon` en `appsettings*.json`:

- `Horizon:ConnectionStrings:<App>` apunta a SQLite.
- `Horizon:Events:Endpoint` apunta a Redis (`localhost:6379` local, `horizon-redis:6379` en compose).
- `Horizon:Events:DefaultTimeoutSeconds` controla timeouts de request/response IO.
- `Horizon:Logging:*` configura Serilog/Seq mediante `UseHorizonLogging()`.
- `Horizon:Seed:*` existe en Identity para crear el usuario inicial si la base está vacía.

`docker-compose.yml` despliega imágenes de `olyssia.azurecr.io` con volúmenes en `/data/volumes/...`. `docker-compose.local.yml` construye desde los Dockerfile locales y usa volúmenes con nombre. Ambos levantan `redis`, `identity`, `segaris`, `autoconfig` y `mcp`.

Puertos actuales:

| Servicio | Puerto |
|---|---:|
| Identity | 5149 |
| Segaris | 5122 |
| Autoconfig | 5004 |
| MCP | 5180 |
| Althes | 5200 |
| Althes UI | 5210 |
| Redis | 6379 |

Variables relevantes: `HORIZON_SEQ_ENDPOINT`, `HORIZON_SEQ_APIKEY`, `HORIZON_NODE_NAME`, `DATALAKE_ACCOUNT_KEY`, `IDENTITY_SEED_USER`, `IDENTITY_SEED_PASSWORD`, `ALTHES_PROJECT_ID`, `ALTHES_API_KEY`, `OPENAI_API_KEY`.

Los Dockerfile descargan Tailwind standalone para Linux, compilan `Armali.Horizon.Blazor/wwwroot/app.css` y el CSS de la app, y luego ejecutan `dotnet build/publish` con `SkipTailwindBuild=true`.

## Autenticación E Identity

La autenticación está centralizada en `Armali.Horizon.Identity`. Las otras apps no deben gestionar usuarios localmente ni consultar la base de Identity: hablan con Identity por el bus IO en el canal `IdentityChannels.Channel` (`"identity"`).

Piezas principales:

- Contratos y cliente: `src/Armali.Horizon.Contracts/Identity`.
- Identidad compartida: `src/Armali.Horizon.Core/Model/HorizonIdentity.cs`.
- Handlers IO de Identity: `src/Armali.Horizon.Identity/Handlers`.
- Servicio de dominio: `src/Armali.Horizon.Identity/Services/IdentityService.cs`.
- UI Identity: `Users`, `Sessions`, `ApiKeys`, `Profile`, `Home`.

`HorizonSessionService` guarda `HorizonIdentity` en `localStorage` con la clave `"Horizon:Session"`. `HorizonAuthentication` lee esa sesión, valida el token con `identity.auth.whoami`, refresca roles si han cambiado y redirige a `/horizon/login` si no hay sesión válida. Si falla el bus IO durante la validación, usa la caché local para no bloquear la app por una caída temporal de red.

En páginas nuevas, prefiere:

```razor
<HorizonAuthentication @bind-Identity="CurrentUser" OnAuthenticated="LoadData" />
```

`@bind-User` sigue existiendo por compatibilidad y devuelve sólo `UserId`. Para permisos usa `CurrentUser?.HasRole("admin")` o `IdentityChannels.AdminRole`.

Identity emite tokens de sesión al hacer login. Las API keys se crean desde `CreateTokenRequest`; actualmente se crean para el usuario autenticado y son permanentes aunque el contrato tenga campos para expiración/usuario destino.

## Bus IO

`Armali.Horizon.IO` transporta `IHorizonEventPayload` dentro de `HorizonEvent`:

- `EventId`: identificador único.
- `EventType`: operación (`identity.auth.login`, `identity.users.list`, etc.).
- `Payload`: JSON comprimido con Zstd.
- `CorrelationId`: enlaza request/response.
- `ReplyTo`: canal temporal de respuesta.

Patrones:

- Fire-and-forget: `PublishAsync<T>` + `Subscribe<T>`.
- Request/response: `RequestAsync<TResponse>` + `IHorizonRequestHandler<TReq,TRes>`.

Registro:

```csharp
// Cliente sin handlers
builder.Host.UseHorizonEvents();

// Servicio que responde peticiones
builder.Host.UseHorizonEvents(events =>
{
    events.HandleRequest<LoginHandler, LoginRequest, LoginResponse>(IdentityChannels.Channel);
});
```

Los handlers se resuelven en un scope DI nuevo por petición, así que pueden inyectar servicios scoped. Las requests y responses deben implementar `IHorizonEventPayload`; usa `service.resource.action` y `:response` para respuestas.

## Patrones Blazor

Las apps usan Razor Components con render interactivo server:

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(Armali.Horizon.Blazor._Imports).Assembly);
```

Layout típico:

```razor
<HorizonAuthentication @bind-Identity="CurrentUser" OnAuthenticated="LoadData" />
<HorizonLayout AppName="Segaris" PageName="PageTitle">
    <Sidebar> ... </Sidebar>
    <SidebarBottom> ... </SidebarBottom>
    <Body> ... </Body>
</HorizonLayout>
```

Tablas:

- Usa `HorizonTable<TItem>` para datos paginados.
- Declara columnas con `HorizonCellHeader`; para filtros usa `Filterable="true"` y `ValueSelector`.
- Usa celdas tipadas: `HorizonCellText`, `HorizonCellTwoLine`, `HorizonCellCurrency`, `HorizonCellDateTime`, `HorizonCellStatus`, `HorizonCellBoolean`, `HorizonCellButtons`.
- Para tablas de referencia no paginadas usa `HorizonStaticTable`.

CRUD:

- Usa `PopupIntent` (`None`, `Create`, `Edit`) y flags como `ShowEditPopup`/`ShowDeletePopup`.
- El flujo común es `HorizonPopup` -> `HorizonDialog` -> `HorizonForm` -> `HorizonFormCell`.
- Para enlaces FK con búsqueda, usa `HorizonEntityLink` y popups selector (`ProjectSelectorPopup`, `AssetSelectorPopup`, etc.) en vez de inventar controles nuevos.

## Diseño Y Estilo

Los tokens de diseño viven en `src/Armali.Horizon.Blazor/wwwroot/app.css` bajo `@theme`. Usa clases Tailwind con tokens `hz-*`:

- Fondos: `hz-styx`, `hz-spartan`, `hz-gray`, `hz-gray-light`.
- Texto: `hz-arendelle`, `hz-artemisia`.
- Acciones/acento: `hz-opera`, `hz-tribunal`, `hz-apollo`, `hz-sol`, `hz-tefiti`, `hz-meridian` y variantes `*-light`.
- Fuentes: `font-hz-header`, `font-hz-text`, `font-hz-special`.

Iconos: Font Awesome 6 Solid local (`fa-solid fa-*`). No importes otros sets de iconos.

Las apps pueden tener su propio `wwwroot/app.css`, pero deben apoyarse en la librería compartida y mantener consistencia visual. No conviertas páginas internas en landing pages; son herramientas de trabajo.

## Datos Y Servicios

Servicios de dominio:

- Inyectan `IDbContextFactory<TDbContext>`, no el `DbContext` directamente.
- Crean contextos cortos por operación con `await using var context = await Factory.CreateDbContextAsync()` o `Factory.CreateDbContext()`.
- En consultas de lectura usa `AsNoTracking()`.
- Evita `Include()` por defecto; resuelve catálogos por FK en UI con helpers como `Index()`. Hay excepciones reales cuando el método necesita navegar relaciones para una operación concreta, como parte de `ProjectService`.
- Las apps aplican migraciones automáticamente en producción; Identity las aplica siempre al arrancar porque puede no existir la BD.

Modelo de privacidad:

- Entidades con visibilidad por usuario usan `bool IsPrivate` y `string Creator`.
- Servicios filtran con `!e.IsPrivate || e.Creator == userId`.
- En UI usa `Utils.PrivacyIcon()` y `Utils.PrivacyColor()`.

Azure Data Lake:

- Segaris usa `DatalakeService`; Archive y Project dependen de él.
- Autoconfig usa `AutoconfigDatalakeService`.
- Ambos leen `DATALAKE_ACCOUNT_KEY` del entorno.

## Convención De Módulos Segaris

Los módulos de negocio siguen una triple:

1. Modelo: `src/Armali.Horizon.Segaris/Model/{Module}Model.cs`.
2. Servicio: `src/Armali.Horizon.Segaris/Services/{Module}Service.cs`.
3. Página(s): `src/Armali.Horizon.Segaris/Components/Pages/{Module}.razor`.

Módulos actuales: Admin, Archive, Assets, Capex, Clothes, Expense/Expenses, Firebird, Inventory, Maintenance (`MaintService`/`MaintModel`), Mood, Opex, Project, Travel.

Al añadir módulo:

- Registra el servicio en `src/Armali.Horizon.Segaris/Program.cs`.
- Añade `DbSet` y migración EF en `SegarisDbContext`.
- Reutiliza layout, tabla, popup y componentes existentes.
- Añade tests de servicio en `test/Armali.Horizon.Segaris.Tests`.

Inventory usa varias páginas (`InvVendors`, `InvItems`, `InvOrder`) con un modelo/servicio común. Project y Firebird también tienen páginas auxiliares. Respeta esa organización si amplías módulos existentes.

### Bus IO de Segaris

Segaris expone operaciones request/response de **lectura** sobre el canal `SegarisChannels.Channel` (`"segaris"`). Los handlers están en `src/Armali.Horizon.Segaris/Handlers` y siguen este patrón:

1. Reciben un `*Request` que extiende `Identity.AuthenticatedRequest` (lleva `Token`).
2. Llaman a `HorizonAuthClient.AuthAsync(req)` para validar el token contra Identity y resolver la `HorizonIdentity` actual.
3. Si la identidad es null, devuelven `Success = false` + `SegarisErrorInfo` con código `unauthorized`.
4. Si es válida, llaman al servicio de dominio y mapean entidad → DTO con `SegarisDtoMapper`.
5. La privacidad se respeta usando `id.UserId` como filtro (`!IsPrivate || Creator == id.UserId`).

Operaciones disponibles (todas en el canal `segaris`):

- `segaris.project.programs.list`, `axes.list`, `statuses.list`, `subCategories.list`, `subEntities.list`, `riskCategories.list`, `risks.list`, `budgets.list`
- `segaris.projects.list`
- `segaris.asset.categories.list`, `statuses.list` y `segaris.assets.list`
- `segaris.capex.categories.list`, `statuses.list`, `list`
- `segaris.opex.categories.list`, `statuses.list`, `list`, `subEntries.list`, `stats.get`
- `segaris.travel.categories.list`, `costCenters.list`, `statuses.list`, `subCategories.list`, `entries.list` y `segaris.travels.list`
- `segaris.maint.categories.list`, `statuses.list`, `list`
- `segaris.inv.vendorStatuses.list`, `vendors.list`, `vendorStats.get`, `itemCategories.list`, `itemStatuses.list`, `items.list`, `shoppingList.get`, `itemPriceHistory.get`, `orderStatuses.list`, `orders.list`, `orderLines.list`, `orderStats.get`
- `segaris.clothes.categories.list`, `statuses.list`, `washTypes.list`, `colors.list`, `colorStyles.list`, `list`, `colorAssignments.list`
- `segaris.firebird.categories.list`, `statuses.list`, `list`, `subEntities.list`
- `segaris.admin.categories.list`, `list`, `steps.list`, `stats.get`

Cliente recomendado: `HorizonSegarisClient` en `Armali.Horizon.Contracts.Segaris`. No reimplementes los requests en cada app.

Para añadir un nuevo handler de lectura:

1. Define `Request`/`Response` en `SegarisPayloads.cs`. La request hereda de `AuthenticatedRequest`; la response implementa `ISegarisResponse`.
2. Si la entidad no tiene DTO, añádelo a `SegarisDtos.cs` (sin nav properties).
3. Crea el handler en `SegarisHandlers.cs` siguiendo el patrón Auth → Service → Map → Response.
4. Añade el mapeo entidad→DTO en `SegarisDtoMapper.cs`.
5. Registra el handler en `Program.cs` con `events.HandleRequest<...>(SegarisChannels.Channel)`.
6. Añade el método correspondiente al `HorizonSegarisClient`.
7. Si va a exponerse vía MCP, añade la tool en `Armali.Horizon.MCP/Tools/`.

## MCP

`Armali.Horizon.MCP` es un servidor [Model Context Protocol](https://modelcontextprotocol.io) HTTP que expone funcionalidad Horizon como tools MCP para clientes LLM (Codex y similares). El proyecto usa el SDK oficial `ModelContextProtocol.AspNetCore` y se integra con el resto del sistema:

- `UseHorizonLogging()` para Serilog/Seq.
- `UseHorizonEvents()` como cliente puro del bus IO (sin handlers).
- Reutiliza `HorizonAuthClient` y `HorizonSegarisClient` de `Contracts`.

Reglas:

- El servidor MCP **no almacena credenciales**. Cada request HTTP entrante debe traer la cabecera `X-Horizon-Api-Key` con un token válido emitido por Identity (sesión o API key permanente).
- `HorizonApiKeyAccessor` lee esa cabecera y la inyecta en los clientes Horizon a través del scope DI de cada petición.
- Las tools viven en `src/Armali.Horizon.MCP/Tools/` agrupadas por módulo (`SegarisProjectMcpTools`, `SegarisAssetMcpTools`, etc.). Cada operación expone una tool independiente con descripciones `[Description]` para que el LLM entienda qué hace.
- Por ahora sólo se exponen tools de **lectura** sobre Segaris e Identity (`whoami`). Las mutaciones se añadirán explícitamente.
- Endpoint MCP: `http://host:5180/mcp` (transporte Streamable HTTP, modo stateless). Health check en `/health`.

Configuración del cliente Codex (ejemplo):

```toml
[mcp_servers.horizon]
url = "http://localhost:5180/mcp"
headers = { "X-Horizon-Api-Key" = "<API_KEY_DE_IDENTITY>" }
```

Para añadir una tool:

1. Asegúrate de tener el contrato y el handler IO en Segaris (ver sección anterior).
2. Añade el método al `HorizonSegarisClient` correspondiente.
3. En `Tools/`, en la clase del módulo, añade un método estático `[McpServerTool]` con `[Description]` y argumentos tipados.
4. Devuelve el resultado vía `SegarisToolHelpers.Wrap` para que los errores se propaguen como `{ success: false, errorCode, errorMessage }`.

## Autoconfig

Autoconfig sigue los mismos patrones de layout/autenticación, pero con `AutoconfigDbContext` y `AutoconfigService`. Módulos actuales:

- `Nodes`
- `Apps` y versiones
- `Files` para operaciones de Data Lake

Tests en `test/Armali.Horizon.Autoconfig.Tests` usan `TestDbContextFactory` y bUnit para páginas.

Autoconfig también expone una operación request/response sobre IO en el canal `AutoconfigChannels.Channel` (`"autoconfig"`):

- `autoconfig.config.get` (`GetConfigFileRequest` → `GetConfigFileResponse`): pide un archivo de configuración por `NodeName`, `AppName`, `Version` (`"A.B.C"`) y `FileName`. La petición es **anónima**: los archivos se consideran globales.
- Resolución por fallback: (1) Major.Minor.Patch exacto, (2) mismo Major.Minor con Patch más alto, (3) mismo Major con Minor.Patch más altos. Si la versión candidata no contiene el archivo, se descarta y se sigue. `ResolvedVersion` es la versión real desde la que se ha servido.
- El contenido se devuelve como `string` UTF-8. Si los bytes no son UTF-8 válidos se responde `not_text`. El tamaño máximo se controla con `Horizon:Autoconfig:MaxFileBytes` (env `Horizon__Autoconfig__MaxFileBytes`, por defecto 2 MB) y se devuelve `too_large` si se supera.
- Cliente recomendado: `HorizonAutoconfigClient` en `Armali.Horizon.Contracts.Autoconfig`. No reimplementes el request en cada app.

## Althes

`Armali.Horizon.Althes` es una **oficina virtual de agentes autónomos**. Cada contenedor representa un *proyecto* (`ProjectId`) con su propio set de agentes definidos en un archivo JSON servido por Autoconfig. Los agentes viven dentro del mismo proceso pero se comunican entre sí (y con el usuario) exclusivamente a través del bus IO. No tiene UI en v1; sólo backend.

Piezas:

- App: `src/Armali.Horizon.Althes` (ASP.NET, sin Blazor). Persistencia SQLite (`AlthesDbContext`).
- Contratos: `src/Armali.Horizon.Contracts/Althes/` con `AlthesChannels`, payloads, DTOs y `HorizonAlthesClient`.
- Runtime por agente: `Services/AgentRuntime.cs` (un loop por agente, cola FIFO `Channel<T>`). Orquestado por `AgentRuntimeHost`.
- Skills modulares: `Services/Skills/` (think, notify_user, ask_user, notify_agent, ask_agent). Se añaden implementando `IAgentSkill` y registrando en DI.
- LLM: `Services/Llm/ILlmClient.cs` + `OpenAiLlmClient` (HTTP directo contra cualquier endpoint compatible con OpenAI Chat Completions).

### Canales y autenticación

- Canal externo request/response: `althes:{projectId}` (`AlthesChannels.For(projectId)`).
- Canal interno por agente (fire-and-forget, lo alimenta el bus IO y los handlers externos): `althes:{projectId}:agent:{name}`.
- Canal de inbox del usuario (para UIs futuras en vivo): `althes:{projectId}:user`.
- Las operaciones externas requieren rol Identity `althes.user` (`AlthesChannels.UserRole`). Los admins lo cumplen implícitamente. El rol **no se crea por seed**: añádelo manualmente a los usuarios necesarios.
- La instancia lee su propia API key Identity de la variable `ALTHES_API_KEY` (nombre configurable en `Horizon:Althes:ApiKeyEnv`).

### Configuración de agentes (Autoconfig)

Cada instancia pide a Autoconfig el archivo `{ProjectId}.agents.json` desde el nodo/app/versión configurados en `Horizon:Althes:Config`. Esquema:

```json
{
  "agents": [
    {
      "name": "planner",
      "systemPrompt": "Eres un planificador...",
      "model": "gpt-4o-mini",
      "carryOverSummary": true,
      "allowedSkills": ["think", "ask_user", "notify_agent"],
      "allowedRecipients": ["worker", "reviewer"],
      "maxActionsPerHour": 60
    }
  ]
}
```

`allowedSkills` y `allowedRecipients` vacíos o ausentes = sin restricción. Si Autoconfig no responde, Althes intenta rehidratar desde la copia local que persiste en su SQLite.

### Runs, contexto y cierre

- Cada agente tiene 0..1 `AgentRun` activo. Un run = contexto coherente desde el system prompt hasta el cierre.
- "Nueva conversación" = cerrar el run actual (genera `Summary` siempre) y abrir uno nuevo. Disparadores: `StartNewRunRequest` externo, `SendMessageRequest` con `StartNewRun=true`, o cierre forzado por el `ContextManager` al cruzar el hard-limit.
- Si el agente tiene `carryOverSummary=true`, el siguiente run arranca con el resumen del anterior inyectado como segundo mensaje system.
- `ContextManager` vigila `TokenCount` del run frente a `Horizon:Althes:Llm:ContextWindow`:
  - Soft-limit (`SoftLimitFraction`, default 0.7): pide al LLM un resumen de los mensajes antiguos y los sustituye por un único system. Run pasa a `Compacted`.
  - Hard-limit (`HardLimitFraction`, default 0.9) tras compactar: cierra el run con `Completed` y abre uno nuevo. Si la compresión falla, cierra con `Aborted`.

### Skills v1 (modulares)

| Name | Tipo | Efecto |
|---|---|---|
| `think` | continúa | Razonamiento interno; el texto se añade al contexto sin efectos externos. |
| `notify_user` | continúa | Publica un evento informativo en el inbox del usuario. |
| `ask_user` | await | Persiste un `UserQuery`, publica la pregunta y suspende el burst. Se reanuda cuando `AnswerUserQueryRequest` inyecta `UserAnswer` en el inbox del agente. |
| `notify_agent` | continúa | Publica fire-and-forget en el canal interno del agente destino. Respeta `allowedRecipients`. |
| `ask_agent` | await | Publica `Question` con correlation id al destino y suspende el burst. Cualquier mensaje al origen con el mismo correlation id cuenta como `Answer`. Timeout sintético según `Limits.AwaitTimeoutSeconds`. |

Para añadir una skill: implementa `IAgentSkill`, regístrala en `Program.cs` como `AddSingleton<IAgentSkill, MySkill>()`, opcionalmente decláranla en `allowedSkills` del agente. El runtime la documenta automáticamente al LLM en el system prompt.

### Límites y rate limiting

`Horizon:Althes:Limits`:

- `MaxActionsPerHour` (global o sobrecargado por agente vía `MaxActionsPerHour` en su definición). Ventana deslizante en memoria (se resetea al reiniciar — decisión v1).
- `MaxRequestSeconds`: timeout duro por llamada LLM/skill.
- `MaxConcurrentLlmCalls`: cupo global (placeholder, semáforo aún no aplicado en v1).
- `MaxBurstTurns`: máximo de turnos LLM por un único item de inbox.
- `AwaitTimeoutSeconds`: tras este tiempo sin respuesta, se inyecta un `Timeout` sintético al inbox del agente que esperaba.

### Operaciones disponibles (canal `althes:{projectId}`)

Todas autenticadas (`althes.user` / `admin`):

- `althes.agents.list`, `althes.agents.send`, `althes.agents.newRun`
- `althes.runs.list`, `althes.runs.get`
- `althes.userQueries.list`, `althes.userQueries.answer`

Cliente recomendado: `HorizonAlthesClient(events, projectId, token)`. No reimplementes los requests en cada app.

### Puesta en marcha

1. Crear API key Identity con rol `althes.user` y exportarla como `ALTHES_API_KEY` (sólo necesaria si el contenedor a su vez consume servicios autenticados; la sección de handlers ya valida el token del cliente).
2. Subir el archivo `{ProjectId}.agents.json` a Autoconfig en el nodo indicado por `HORIZON_NODE_NAME`, app `Althes`, versión coincidente con la del assembly de la app (formato `Major.Minor.Patch`).
3. `docker compose -f docker-compose.local.yml up --build althes` (puerto 5200, health en `/health`).
4. Para correr varios proyectos: levantar varios contenedores con `ALTHES_PROJECT_ID` distinto y archivos `{ProjectId}.agents.json` separados en Autoconfig.

## Autoconfig

Identity tiene UI propia y handlers IO. El rol administrativo compartido es `IdentityChannels.AdminRole` (`"admin"`).

Reglas:

- Las operaciones admin deben validar con `AuthAdminAsync`.
- Las operaciones autenticadas no admin validan con `AuthAsync`.
- No permitas que un admin se borre a sí mismo ni se quite su propio rol `admin`.
- No devuelvas tokens en claro salvo en la respuesta inmediata a su creación.
- `TokenCleanupService` elimina o revoca periódicamente tokens expirados/revocados según la lógica de `IdentityService`.

## Tests

El árbol `test/` tiene cinco proyectos unitarios incluidos en `Armali.Horizon.slnx`:

- `Armali.Horizon.Core.Tests`
- `Armali.Horizon.IO.Tests`
- `Armali.Horizon.Segaris.Tests`
- `Armali.Horizon.Autoconfig.Tests`
- `Armali.Horizon.Althes.Tests`

Más un proyecto de smoke / integración **fuera** de la solución principal:

- `Armali.Horizon.Smoke.Tests` (no está en `Armali.Horizon.slnx`).

`test/Directory.Build.props` centraliza `net10.0`, MSTest, Shouldly, `Microsoft.NET.Test.Sdk` y bUnit. Los tests de servicios instancian servicios con `TestDbContextFactory` y SQLite in-memory; evita arrancar apps completas para probar lógica de dominio.

Ejecuta al menos los tests afectados. Para cambios transversales, ejecuta:

```powershell
dotnet test Armali.Horizon.slnx /p:SkipTailwindBuild=true
```

### Smoke tests (stack completo)

`test/Armali.Horizon.Smoke.Tests` valida que el stack docker arranca y se comunica end-to-end. No está en `Armali.Horizon.slnx` para que `dotnet test Armali.Horizon.slnx` no lo dispare sin un stack en marcha. Lo ejecuta el workflow `.github/workflows/smoke.yml` en cada pull request a `main`.

Cubre:

- `/health` de Identity, Segaris, Autoconfig y MCP responde 200.
- Login con el usuario seed (`armali/armali`) sobre el bus IO contra Identity.
- `WhoAmI` con el token devuelto por login.
- `segaris.project.statuses.list` autenticado (cadena cliente → Redis → Segaris → Identity).
- Rechazo de `segaris.project.statuses.list` con token inválido.
- `autoconfig.config.get` con datos inexistentes (basta con que el bus responda).

No cubre (deliberado):

- Operaciones que dependen de Azure Data Lake. CI no tiene `DATALAKE_ACCOUNT_KEY`.
- Mutaciones (CRUDs vía UI o IO).
- Páginas Razor concretas.

Cada test está marcado con `[TestCategory("Smoke")]`. Endpoints y credenciales se pueden sobreescribir vía variables de entorno (`SMOKE_REDIS_ENDPOINT`, `SMOKE_*_HEALTH`, `IDENTITY_SEED_USER`, `IDENTITY_SEED_PASSWORD`) para apuntar a un stack remoto.

Ejecución local equivalente al workflow:

```powershell
docker compose -f docker-compose.local.yml up -d --build
# Espera a que /health responda en 5149, 5122, 5004, 5180
dotnet test test/Armali.Horizon.Smoke.Tests
docker compose -f docker-compose.local.yml down -v
```

Los warnings normales de arranque (DataProtection sobre `/home/app/.aspnet/DataProtection-Keys`, "No XML encryptor configured") **no** rompen el smoke porque la validación es por HTTP/health y request/response IO, nunca por parseo de logs.

Para añadir un nuevo smoke test:

1. Añade un método `[TestMethod, TestCategory("Smoke")]` en `HorizonStackSmokeTests` (o un nuevo TestClass del mismo proyecto).
2. Reutiliza `Events`, `HorizonAuthClient`, `HorizonSegarisClient`, `HorizonAutoconfigClient` ya configurados.
3. Si dependes de un puerto/endpoint nuevo, expónlo como variable de entorno `SMOKE_*` con default a `localhost`.
4. No introduzcas dependencias con Data Lake ni datos seed específicos: usa operaciones que funcionen sobre BD vacía.

## Althes UI

`Armali.Horizon.Althes.UI` (puerto **5210**) es la interfaz web Blazor Server para gestionar instancias Althes y sus conversaciones. A diferencia del resto de apps, **no es un ejecutable backend**: no tiene handlers IO ni base de datos de dominio. Su única persistencia es una SQLite local (`althes-ui.db`) con la tabla `AlthesProject`.

### Modelo de proyectos

`AlthesProject` guarda la conexión a una instancia:

| Campo | Descripción |
|---|---|
| `Name` | Nombre para mostrar |
| `ProjectId` | `ALTHES_PROJECT_ID` del contenedor Althes |
| `ApiKey` | API key Identity con rol `althes.user` (no se devuelve en claro tras su creación) |

Un único Redis global conecta la UI con todos los proyectos (topología A). La UI usa `AlthesConnectionManager` para mantener un `HorizonAlthesClient` por `ProjectId`.

Reglas de acceso:
- Cualquier usuario con rol `althes.user` puede ver proyectos y conversaciones.
- **Solo `admin`** puede crear, editar o borrar proyectos (incluida la API key).
- Borrar un proyecto sólo elimina la entrada local; no toca el contenedor Althes.

### Páginas

- `Home (/)`  — dos cards: Projects / Conversations.
- `Projects (/projects)` — CRUD de `AlthesProject` con `HorizonTable` + `HorizonPopup`. API key oculta tras guardado.
- `Conversations (/conversations?pid=X)` — interfaz de chat:
  - **Columna izquierda**: `HorizonTreeView` con `Project → Conversation`. Badge de preguntas pendientes. Botón "Nueva conversación". Botón eliminar conversación (solo admin).
  - **Área principal**: `HorizonChatLog` con `HorizonChatBubble`s diferenciadas por visibilidad. `HorizonFloatingPanel` con lista de agentes y estado. `HorizonChatComposer` con soporte de `@mención` para mensajes libres y modo "Respondiendo a:" para `ask_user`.

### Servicios propios

- `AlthesProjectStore` — CRUD de `AlthesProject` sobre SQLite.
- `AlthesConnectionManager` — singleton con un `HorizonAlthesClient` por `ProjectId`.
- `AlthesLiveSubscription` — scoped; suscribe al canal `althes:{projectId}:user` de cada proyecto y dispara `event Action<string, AgentInboxMessage>` a la página Conversations.

### Estructura de archivos

```
src/Armali.Horizon.Althes.UI/
  AlthesUiDbContext.cs
  Program.cs
  Dockerfile
  Model/            AlthesProject.cs, PopupIntent.cs
  Migrations/       Initial
  Services/         AlthesProjectStore.cs, AlthesConnectionManager.cs, AlthesLiveSubscription.cs
  Components/       App.razor, Routes.razor, _Imports.razor
  Components/Pages/ Home.razor, Projects.razor, Conversations.razor, NotFound.razor
  wwwroot/          app.css, tailwind.css (generado)
```

### Puesta en marcha

1. Todos los proyectos Althes deben estar accesibles vía el mismo Redis global.
2. Cada proyecto necesita una API key Identity con rol `althes.user` (ver sección Althes).
3. Al crear un `AlthesProject` en la UI, introducir esa API key — solo admins lo verán/editarán.
4. `docker compose -f docker-compose.local.yml up --build althes-ui` (puerto 5210).

## Checklist Al Terminar

- Compila o prueba el área afectada, salvo que haya un bloqueo explícito.
- Revisa `git diff` para evitar cambios accidentales.
- Actualiza `AGENTS.md` si cambiaste o descubriste convenciones relevantes.
- Menciona en la respuesta final qué pruebas ejecutaste y cualquier riesgo pendiente.
