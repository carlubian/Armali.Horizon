using Armali.Horizon.IO;

namespace Armali.Horizon.Contracts.Autoconfig;

/// <summary>
/// Cliente de alto nivel para Horizon.Autoconfig sobre el bus IO.
/// Encapsula la llamada <see cref="HorizonEventService.RequestAsync{T}"/> al
/// canal <see cref="AutoconfigChannels.Channel"/> para que apps con UI o
/// headless no tengan que conocer los detalles del payload.
/// </summary>
public class HorizonAutoconfigClient
{
    private readonly HorizonEventService Events;
    private readonly TimeSpan? Timeout;

    public HorizonAutoconfigClient(HorizonEventService events, TimeSpan? timeout = null)
    {
        Events = events;
        Timeout = timeout;
    }

    /// <summary>
    /// Pide un archivo de configuración. Devuelve la respuesta tal cual la emite
    /// el servicio: <see cref="GetConfigFileResponse.Found"/>, la versión real
    /// resuelta y el contenido como texto.
    /// </summary>
    public Task<GetConfigFileResponse> GetConfigFileAsync(string nodeName, string appName, string version, string fileName)
    {
        var req = new GetConfigFileRequest
        {
            NodeName = nodeName,
            AppName = appName,
            Version = version,
            FileName = fileName,
        };
        return Events.RequestAsync<GetConfigFileResponse>(AutoconfigChannels.Channel, req, Timeout);
    }
}

