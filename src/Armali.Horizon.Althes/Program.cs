using Armali.Horizon.Althes;
using Armali.Horizon.Althes.Configuration;
using Armali.Horizon.Althes.Handlers;
using Armali.Horizon.Althes.Services;
using Armali.Horizon.Althes.Services.Llm;
using Armali.Horizon.Althes.Services.Skills;
using Armali.Horizon.Contracts.Althes;
using Armali.Horizon.Contracts.Autoconfig;
using Armali.Horizon.Contracts.Identity;
using Armali.Horizon.Core.Logs;
using Armali.Horizon.IO;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Logging Serilog + Seq, mismo patrón que el resto de apps.
builder.Host.UseHorizonLogging();

// ── Opciones de Althes ─────────────────────────────────────────────────────
builder.Services.Configure<AlthesOptions>(builder.Configuration.GetSection("Horizon:Althes"));
var althes = builder.Configuration.GetSection("Horizon:Althes").Get<AlthesOptions>() ?? new AlthesOptions();
if (string.IsNullOrWhiteSpace(althes.ProjectId))
    throw new InvalidOperationException("Horizon:Althes:ProjectId es obligatorio.");

// ── Bus IO con handlers registrados en el canal del proyecto ───────────────
var channel = AlthesChannels.For(althes.ProjectId);
builder.Host.UseHorizonEvents(events =>
{
    events.HandleRequest<ListAgentsHandler, ListAgentsRequest, ListAgentsResponse>(channel);
    events.HandleRequest<SendMessageHandler, SendMessageRequest, SendMessageResponse>(channel);
    events.HandleRequest<StartNewRunHandler, StartNewRunRequest, StartNewRunResponse>(channel);
    events.HandleRequest<GetRunsHandler, GetRunsRequest, GetRunsResponse>(channel);
    events.HandleRequest<GetRunHandler, GetRunRequest, GetRunResponse>(channel);
    events.HandleRequest<ListUserQueriesHandler, ListUserQueriesRequest, ListUserQueriesResponse>(channel);
    events.HandleRequest<AnswerUserQueryHandler, AnswerUserQueryRequest, AnswerUserQueryResponse>(channel);
    events.HandleRequest<ListConversationsHandler, ListConversationsRequest, ListConversationsResponse>(channel);
    events.HandleRequest<GetConversationHandler, GetConversationRequest, GetConversationResponse>(channel);
    events.HandleRequest<StartNewConversationHandler, StartNewConversationRequest, StartNewConversationResponse>(channel);
    events.HandleRequest<DeleteConversationHandler, DeleteConversationRequest, DeleteConversationResponse>(channel);
});

// ── Persistencia SQLite ────────────────────────────────────────────────────
builder.Services.AddDbContextFactory<AlthesDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetSection("Horizon")["ConnectionStrings:Althes"]
        ?? "Data Source=althes.db"));

// ── Clientes Horizon ───────────────────────────────────────────────────────
// Autoconfig (anónimo) — singleton para reusar en AgentRegistry.
builder.Services.AddSingleton(sp => new HorizonAutoconfigClient(sp.GetRequiredService<HorizonEventService>()));

// Identity AuthClient — scoped: cada handler IO lo crea limpio y le setea
// el token de la request entrante antes de validar.
builder.Services.AddScoped(sp => new HorizonAuthClient(sp.GetRequiredService<HorizonEventService>()));

// ── LLM ────────────────────────────────────────────────────────────────────
builder.Services.AddHttpClient<ILlmClient, OpenAiLlmClient>();
builder.Services.AddSingleton(sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AlthesOptions>>().Value.Llm);

// ── Skills ─────────────────────────────────────────────────────────────────
builder.Services.AddSingleton<IAgentSkill, ThinkSkill>();
builder.Services.AddSingleton<IAgentSkill, NotifyUserSkill>();
builder.Services.AddSingleton<IAgentSkill, AskUserSkill>();
builder.Services.AddSingleton<IAgentSkill, NotifyAgentSkill>();
builder.Services.AddSingleton<IAgentSkill, AskAgentSkill>();
builder.Services.AddSingleton<SkillRegistry>();

// ── Servicios runtime ──────────────────────────────────────────────────────
builder.Services.AddSingleton<AgentInboxRouter>();
builder.Services.AddSingleton<RateLimiter>();
builder.Services.AddSingleton<AgentRegistry>();
builder.Services.AddScoped<ConversationStore>();
builder.Services.AddScoped<ContextManager>();
builder.Services.AddSingleton<AgentRuntimeHost>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<AgentRuntimeHost>());

var app = builder.Build();

// Aplicar migraciones EF al arrancar. La BD se crea desde cero si no existe;
// las nuevas migraciones se aplican incrementalmente sobre bases existentes.
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AlthesDbContext>>();
    await using var db = await factory.CreateDbContextAsync();
    await db.Database.MigrateAsync();
}

// Health check sencillo para probes/smoke.
app.MapGet("/health", () => Results.Ok(new { status = "ok", app = "althes", project = althes.ProjectId }));

app.Run();

