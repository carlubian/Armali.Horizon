using Armali.Horizon.Contracts.Segaris;

namespace Armali.Horizon.MCP.Tools;

/// <summary>
/// Helpers compartidos por las tools MCP de Segaris.
/// Centraliza la conversión de respuestas <see cref="ISegarisResponse"/>
/// a un objeto plano amigable para clientes LLM.
/// </summary>
internal static class SegarisToolHelpers
{
    /// <summary>
    /// Si la respuesta indica error, devuelve un envoltorio con info diagnóstica;
    /// si tuvo éxito, devuelve el objeto producido por <paramref name="ok"/>.
    /// </summary>
    public static object Wrap<TRes>(TRes res, Func<TRes, object> ok)
        where TRes : ISegarisResponse
    {
        if (!res.Success)
        {
            return new
            {
                success = false,
                errorCode = res.Error?.Code ?? "unknown",
                errorMessage = res.Error?.Message ?? "Operación rechazada por Segaris.",
            };
        }
        return ok(res);
    }
}

