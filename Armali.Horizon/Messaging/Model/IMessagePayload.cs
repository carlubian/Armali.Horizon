namespace Armali.Horizon.Messaging.Model;

/// <summary>
/// A generic interface for message payloads used in Horizon messaging.
/// </summary>
public interface IMessagePayload
{
    /// <summary>
    /// The name of a class that this message payload represents.
    /// </summary>
    string TypeHint { get; }
}
