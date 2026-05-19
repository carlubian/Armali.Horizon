namespace Armali.Horizon.Althes.Configuration;

/// <summary>
/// Sección <c>Horizon:Althes</c> de <c>appsettings.json</c>.
/// </summary>
public class AlthesOptions
{
    /// <summary>Identificador del proyecto (instancia). Obligatorio.</summary>
    public string ProjectId { get; set; } = "default";
    
    /// <summary>Nombre de la variable de entorno con la API key de Identity usada por esta instancia.</summary>
    public string ApiKeyEnv { get; set; } = "ALTHES_API_KEY";
    
    /// <summary>
    /// Nombre de la variable de entorno que contiene el NodeName de Autoconfig
    /// (la máquina/host donde corre la instancia). Por defecto <c>HORIZON_NODE_NAME</c>.
    /// </summary>
    public string NodeEnv { get; set; } = "HORIZON_NODE_NAME";
    
    public AlthesLlmOptions Llm { get; set; } = new();
    public AlthesLimitsOptions Limits { get; set; } = new();
    public AlthesContextOptions Context { get; set; } = new();
}

/// <summary>
/// Constantes de identificación de la app frente a Autoconfig.
/// El NodeName se resuelve por variable de entorno y la versión desde el
/// assembly, así que aquí sólo queda el nombre de la app.
/// </summary>
public static class AlthesAppId
{
    /// <summary>Nombre canónico de la app dentro de Autoconfig.</summary>
    public const string AppName = "Althes";
}

public class AlthesLlmOptions
{
    public string Provider { get; set; } = "openai";
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    /// <summary>Nombre de la variable de entorno con la API key del LLM.</summary>
    public string ApiKeyEnv { get; set; } = "OPENAI_API_KEY";
    public string Model { get; set; } = "gpt-4o-mini";
    public double Temperature { get; set; } = 0.7;
    public int MaxOutputTokens { get; set; } = 1024;
    /// <summary>Tamaño aproximado de la ventana de contexto del modelo, en tokens.</summary>
    public int ContextWindow { get; set; } = 128_000;
}

public class AlthesLimitsOptions
{
    /// <summary>Límite global por agente. Cero o negativo = sin límite.</summary>
    public int MaxActionsPerHour { get; set; } = 120;
    /// <summary>Timeout duro por llamada LLM o ejecución de skill.</summary>
    public int MaxRequestSeconds { get; set; } = 60;
    /// <summary>Semáforo global para no saturar al LLM.</summary>
    public int MaxConcurrentLlmCalls { get; set; } = 4;
    /// <summary>Máximo de turnos LLM consecutivos por un único item de inbox.</summary>
    public int MaxBurstTurns { get; set; } = 10;
    /// <summary>Cuánto se espera una respuesta antes de inyectar un Timeout sintético.</summary>
    public int AwaitTimeoutSeconds { get; set; } = 600;
}

public class AlthesContextOptions
{
    /// <summary>Fracción de la ventana que dispara compresión.</summary>
    public double SoftLimitFraction { get; set; } = 0.7;
    /// <summary>Fracción que dispara cierre forzado y nuevo run.</summary>
    public double HardLimitFraction { get; set; } = 0.9;
    /// <summary>Cuántos mensajes recientes se conservan tras comprimir.</summary>
    public int KeepRecentMessages { get; set; } = 10;
}

// ─────────────────────────────────────────────────────────────────────────────
// Esquema del archivo de agentes (servido por Autoconfig)
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Raíz del archivo <c>{ProjectId}.agents.json</c>.</summary>
public class AgentsConfigFile
{
    public List<AgentConfigEntry> Agents { get; set; } = [];
}

/// <summary>Definición de un agente tal como aparece en el JSON de configuración.</summary>
public class AgentConfigEntry
{
    public string Name { get; set; } = string.Empty;
    /// <summary>Descripción corta del rol del agente, visible para otros agentes en su prompt.</summary>
    public string? Description { get; set; }
    public string SystemPrompt { get; set; } = string.Empty;
    public string? Model { get; set; }
    public bool CarryOverSummary { get; set; } = true;
    /// <summary>Nombres de skills permitidas. Vacío o null = todas las registradas.</summary>
    public List<string>? AllowedSkills { get; set; }
    /// <summary>Nombres de agentes con los que puede comunicarse. Vacío o null = todos.</summary>
    public List<string>? AllowedRecipients { get; set; }
    public int? MaxActionsPerHour { get; set; }
}


