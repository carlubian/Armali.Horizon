using System.Collections.Concurrent;
using Armali.Horizon.Althes.Configuration;
using Microsoft.Extensions.Options;

namespace Armali.Horizon.Althes.Services;

/// <summary>
/// Limitador de tasa por agente sobre ventana deslizante de 1 hora.
/// Se mantiene en memoria — al reiniciar el contenedor las cuentas se resetean
/// (decisión explícita para v1).
/// </summary>
public class RateLimiter
{
    private readonly ConcurrentDictionary<string, ConcurrentQueue<DateTime>> Timestamps = new();
    private readonly AlthesOptions Options;
    
    public RateLimiter(IOptions<AlthesOptions> options) => Options = options.Value;
    
    /// <summary>
    /// Intenta registrar una acción para el agente. Devuelve false si supera el
    /// límite (global de <see cref="AlthesLimitsOptions.MaxActionsPerHour"/> o
    /// el específico del agente).
    /// </summary>
    public bool TryConsume(string agentName, int? agentLimit)
    {
        var limit = agentLimit ?? Options.Limits.MaxActionsPerHour;
        if (limit <= 0) return true;
        
        var queue = Timestamps.GetOrAdd(agentName, _ => new ConcurrentQueue<DateTime>());
        var cutoff = DateTime.UtcNow - TimeSpan.FromHours(1);
        
        // Drenar timestamps fuera de la ventana
        while (queue.TryPeek(out var oldest) && oldest < cutoff)
            queue.TryDequeue(out _);
        
        if (queue.Count >= limit) return false;
        queue.Enqueue(DateTime.UtcNow);
        return true;
    }
    
    /// <summary>Número de acciones del agente en la ventana actual.</summary>
    public int Current(string agentName)
    {
        if (!Timestamps.TryGetValue(agentName, out var queue)) return 0;
        var cutoff = DateTime.UtcNow - TimeSpan.FromHours(1);
        return queue.Count(ts => ts >= cutoff);
    }
}

