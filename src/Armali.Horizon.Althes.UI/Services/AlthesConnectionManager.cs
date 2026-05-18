using System.Collections.Concurrent;
using Armali.Horizon.Contracts.Althes;
using Armali.Horizon.IO;

namespace Armali.Horizon.Althes.UI.Services;

/// <summary>
/// Singleton que mantiene un <see cref="HorizonAlthesClient"/> por ProjectId.
/// Todos los clientes comparten el mismo <see cref="HorizonEventService"/> del
/// bus Redis global (topología A). El token se configura por llamada mediante
/// el setter <see cref="HorizonAlthesClient.Token"/>.
/// </summary>
public class AlthesConnectionManager
{
    private readonly HorizonEventService Events;
    private readonly ConcurrentDictionary<string, HorizonAlthesClient> Clients = new(StringComparer.OrdinalIgnoreCase);
    
    public AlthesConnectionManager(HorizonEventService events) => Events = events;
    
    /// <summary>
    /// Devuelve (creando si no existe) el cliente para el proyecto indicado y
    /// configura el token de la sesión actual antes de devolverlo.
    /// </summary>
    public HorizonAlthesClient GetClient(string projectId, string token)
    {
        var client = Clients.GetOrAdd(projectId, _ => new HorizonAlthesClient(Events, projectId));
        client.Token = token;
        return client;
    }
    
    /// <summary>Elimina el cliente del cache (p.ej. al borrar un proyecto).</summary>
    public void Remove(string projectId) => Clients.TryRemove(projectId, out _);
}

