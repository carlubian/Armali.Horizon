using System.Text.Json;
using Armali.Horizon.Althes.Configuration;
using Armali.Horizon.Althes.Model;
using Armali.Horizon.Contracts.Autoconfig;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Armali.Horizon.Althes.Services;

/// <summary>
/// Carga la definición de agentes desde Autoconfig al arrancar y la cachea en
/// memoria. Si Autoconfig no responde, intenta hidratarse de la copia local
/// persistida en SQLite para sobrevivir reinicios en caliente.
/// </summary>
public class AgentRegistry
{
    private readonly HorizonAutoconfigClient Autoconfig;
    private readonly IDbContextFactory<AlthesDbContext> Factory;
    private readonly AlthesOptions Options;
    private readonly ILogger<AgentRegistry> Logger;
    
    private Dictionary<string, AgentConfigEntry> ByName = new(StringComparer.OrdinalIgnoreCase);
    
    public AgentRegistry(
        HorizonAutoconfigClient autoconfig,
        IDbContextFactory<AlthesDbContext> factory,
        IOptions<AlthesOptions> options,
        ILogger<AgentRegistry> logger)
    {
        Autoconfig = autoconfig;
        Factory = factory;
        Options = options.Value;
        Logger = logger;
    }
    
    public IReadOnlyCollection<AgentConfigEntry> All => ByName.Values;
    public AgentConfigEntry? Get(string name) => ByName.TryGetValue(name, out var a) ? a : null;
    
    /// <summary>
    /// Resuelve y carga la config. Llama una vez al iniciar la app.
    /// </summary>
    public async Task LoadAsync(CancellationToken ct)
    {
        var fileName = $"{Options.ProjectId}.agents.json";
        // NodeName: el host físico/lógico donde corre el contenedor (env var).
        var node = Environment.GetEnvironmentVariable(Options.NodeEnv);
        if (string.IsNullOrWhiteSpace(node))
        {
            Logger.LogError(
                "Variable de entorno '{Env}' no definida. Imposible resolver NodeName para Autoconfig. Intentando caché local.",
                Options.NodeEnv);
        }
        // AppName: nombre canónico del proyecto/app.
        var appName = AlthesAppId.AppName;
        // Version: del assembly de entrada (formato A.B.C).
        var version = System.Reflection.Assembly.GetEntryAssembly()
            ?.GetName().Version?.ToString(3) ?? "1.0.0";
        
        AgentsConfigFile? config = null;
        
        if (!string.IsNullOrWhiteSpace(node))
        {
            try
            {
                var resp = await Autoconfig.GetConfigFileAsync(node, appName, version, fileName);
                if (resp.Found)
                {
                    config = JsonSerializer.Deserialize<AgentsConfigFile>(resp.Content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    });
                    Logger.LogInformation("Agentes cargados desde Autoconfig {Node}/{App}/{Ver}/{File} (resolved: {R}).",
                        node, appName, version, fileName, resp.ResolvedVersion);
                }
                else
                {
                    Logger.LogWarning("Autoconfig no devolvió {File} ({Node}/{App}/{Ver}): {Code} {Msg}. Intentando caché local.",
                        fileName, node, appName, version, resp.Error?.Code, resp.Error?.Message);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(ex, "Fallo consultando Autoconfig. Intentando caché local.");
            }
        }
        
        if (config is null)
        {
            // Fallback: rehidratar desde la copia local persistida en la BD.
            await using var db = await Factory.CreateDbContextAsync(ct);
            var cached = await db.Agents.AsNoTracking().ToListAsync(ct);
            if (cached.Count == 0)
            {
                Logger.LogError("No hay agentes en Autoconfig ni en caché local. Althes arranca vacío.");
                ByName = new(StringComparer.OrdinalIgnoreCase);
                return;
            }
            config = new AgentsConfigFile
            {
                Agents = cached.Select(a => new AgentConfigEntry
                {
                    Name = a.Name,
                    SystemPrompt = a.SystemPrompt,
                    Model = a.Model,
                    CarryOverSummary = a.CarryOverSummary,
                    AllowedSkills = SplitCsv(a.AllowedSkillsCsv),
                    AllowedRecipients = SplitCsv(a.AllowedRecipientsCsv),
                    MaxActionsPerHour = a.MaxActionsPerHour,
                }).ToList(),
            };
        }
        
        // Persistir la copia local para futuros arranques en frío.
        await using (var db = await Factory.CreateDbContextAsync(ct))
        {
            var existing = await db.Agents.ToListAsync(ct);
            db.Agents.RemoveRange(existing);
            foreach (var a in config.Agents)
            {
                db.Agents.Add(new Agent
                {
                    Name = a.Name,
                    SystemPrompt = a.SystemPrompt,
                    Model = a.Model,
                    CarryOverSummary = a.CarryOverSummary,
                    AllowedSkillsCsv = string.Join(',', a.AllowedSkills ?? []),
                    AllowedRecipientsCsv = string.Join(',', a.AllowedRecipients ?? []),
                    MaxActionsPerHour = a.MaxActionsPerHour,
                    UpdatedAt = DateTime.UtcNow,
                });
            }
            await db.SaveChangesAsync(ct);
        }
        
        ByName = config.Agents.ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);
        Logger.LogInformation("AgentRegistry cargado con {N} agentes: {Names}",
            ByName.Count, string.Join(", ", ByName.Keys));
    }
    
    private static List<string> SplitCsv(string csv) =>
        string.IsNullOrWhiteSpace(csv) ? [] : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
}


