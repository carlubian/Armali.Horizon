namespace Armali.Horizon.Althes.Services.Llm;

/// <summary>Rol de un mensaje pasado al LLM.</summary>
public enum LlmMessageRole
{
    System,
    User,
    Assistant,
}

public record LlmMessage(LlmMessageRole Role, string Content);

public class LlmOptions
{
    public string? Model { get; set; }
    public double? Temperature { get; set; }
    public int? MaxOutputTokens { get; set; }
    /// <summary>Forzar respuesta en formato JSON cuando el proveedor lo soporte.</summary>
    public bool JsonMode { get; set; }
}

public record LlmResponse(string Content, int PromptTokens, int CompletionTokens);

/// <summary>
/// Cliente LLM abstracto. v1 sólo soporta chat completion básico — la decisión
/// de skill se modela como JSON estructurado en la respuesta, sin usar function
/// calling nativo, para mantener la interfaz portable entre proveedores.
/// </summary>
public interface ILlmClient
{
    Task<LlmResponse> CompleteAsync(
        IReadOnlyList<LlmMessage> messages,
        LlmOptions options,
        CancellationToken ct);
}

