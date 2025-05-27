using System.Text.Json;

namespace Armali.Horizon.Messaging.Model;

/// <summary>
/// Base class for message payloads used in Horizon messaging.
/// </summary>
public class MessagePayload : IMessagePayload
{
    public string TypeHint => this.GetType().Name;

    internal string RawData { get; set; } = string.Empty;

    /// <summary>
    /// Deserialize this generic message payload into a specific type.
    /// </summary>
    /// <typeparam name="T">Subclass of IMessagePayload</typeparam>
    /// <returns>An instance of the deserialized class</returns>
    public virtual T Deserialize<T>() where T : IMessagePayload
    {
        // Temp: Deserialize the JSON response
        var outmsg = JsonSerializer.Deserialize<T>(RawData ?? string.Empty);
        return outmsg!;
    }
}
