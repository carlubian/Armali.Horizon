namespace Armali.Horizon.Althes.Services.Llm;

/// <summary>
/// Estimador heurístico de tokens (≈ 4 chars/token). Suficiente para vigilar
/// el llenado de la ventana de contexto sin depender de un tokenizer del
/// proveedor. Si en el futuro se quiere precisión por modelo se puede
/// reemplazar la implementación detrás de esta misma estática.
/// </summary>
public static class TokenEstimator
{
    public static int Estimate(string? text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        return Math.Max(1, text.Length / 4);
    }
}

