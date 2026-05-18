using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Armali.Horizon.Althes.Configuration;

namespace Armali.Horizon.Althes.Services.Llm;

/// <summary>
/// Implementación de <see cref="ILlmClient"/> sobre la API REST de OpenAI
/// (endpoint <c>POST /v1/chat/completions</c>). Habla con cualquier endpoint
/// compatible con OpenAI (Azure OpenAI vía gateway, vLLM, etc.) cambiando
/// <see cref="AlthesLlmOptions.BaseUrl"/>.
/// </summary>
public class OpenAiLlmClient : ILlmClient
{
    private readonly HttpClient Http;
    private readonly AlthesLlmOptions Options;
    private readonly ILogger<OpenAiLlmClient> Logger;
    
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
    
    public OpenAiLlmClient(HttpClient http, AlthesLlmOptions options, ILogger<OpenAiLlmClient> logger)
    {
        Http = http;
        Options = options;
        Logger = logger;
        
        Http.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
        var apiKey = Environment.GetEnvironmentVariable(options.ApiKeyEnv) ?? "";
        if (!string.IsNullOrEmpty(apiKey))
            Http.DefaultRequestHeaders.Authorization = new("Bearer", apiKey);
    }
    
    public async Task<LlmResponse> CompleteAsync(
        IReadOnlyList<LlmMessage> messages,
        LlmOptions options,
        CancellationToken ct)
    {
        // Payload mínimo de chat completions. Algunos modelos no aceptan
        // response_format, así que sólo lo enviamos si el caller lo pide.
        var body = new ChatCompletionRequest
        {
            Model = options.Model ?? Options.Model,
            Temperature = options.Temperature ?? Options.Temperature,
            MaxTokens = options.MaxOutputTokens ?? Options.MaxOutputTokens,
            ResponseFormat = options.JsonMode ? new ResponseFormat { Type = "json_object" } : null,
            Messages = messages.Select(m => new ChatMessage
            {
                Role = m.Role switch
                {
                    LlmMessageRole.System => "system",
                    LlmMessageRole.User => "user",
                    LlmMessageRole.Assistant => "assistant",
                    _ => "user",
                },
                Content = m.Content,
            }).ToList(),
        };
        
        using var resp = await Http.PostAsJsonAsync("chat/completions", body, JsonOpts, ct);
        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadAsStringAsync(ct);
            Logger.LogError("OpenAI {Status}: {Body}", resp.StatusCode, err);
            throw new InvalidOperationException($"LLM error {(int)resp.StatusCode}: {err}");
        }
        
        var parsed = await resp.Content.ReadFromJsonAsync<ChatCompletionResponse>(JsonOpts, ct)
            ?? throw new InvalidOperationException("Respuesta LLM vacía.");
        
        var content = parsed.Choices?.FirstOrDefault()?.Message?.Content ?? "";
        var usage = parsed.Usage ?? new UsageInfo();
        return new LlmResponse(content, usage.PromptTokens, usage.CompletionTokens);
    }
    
    // ── DTOs internos para serializar/deserializar JSON OpenAI ──────────
    
    private class ChatCompletionRequest
    {
        [JsonPropertyName("model")] public string Model { get; set; } = "";
        [JsonPropertyName("messages")] public List<ChatMessage> Messages { get; set; } = [];
        [JsonPropertyName("temperature")] public double? Temperature { get; set; }
        [JsonPropertyName("max_tokens")] public int? MaxTokens { get; set; }
        [JsonPropertyName("response_format")] public ResponseFormat? ResponseFormat { get; set; }
    }
    
    private class ChatMessage
    {
        [JsonPropertyName("role")] public string Role { get; set; } = "user";
        [JsonPropertyName("content")] public string Content { get; set; } = "";
    }
    
    private class ResponseFormat
    {
        [JsonPropertyName("type")] public string Type { get; set; } = "text";
    }
    
    private class ChatCompletionResponse
    {
        [JsonPropertyName("choices")] public List<Choice>? Choices { get; set; }
        [JsonPropertyName("usage")] public UsageInfo? Usage { get; set; }
    }
    
    private class Choice
    {
        [JsonPropertyName("message")] public ChatMessage? Message { get; set; }
    }
    
    private class UsageInfo
    {
        [JsonPropertyName("prompt_tokens")] public int PromptTokens { get; set; }
        [JsonPropertyName("completion_tokens")] public int CompletionTokens { get; set; }
        [JsonPropertyName("total_tokens")] public int TotalTokens { get; set; }
    }
}

