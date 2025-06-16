using Armali.Horizon.Messaging.Model;
using Microsoft.Extensions.Hosting;

namespace Armali.Horizon.Messaging;

/// <summary>
/// Delegate for handling received messages in Horizon messaging system. 
/// The generic MessagePayload can be deserialized into a specific type using the Deserialize method.
/// </summary>
/// <param name="message">Message received</param>
public delegate void MessageReceivedDelegate(MessagePayload message);

/// <summary>
/// A system used to send and receive messages within the Horizon framework.
/// </summary>
public interface IHorizonMessaging : IHostedService
{
    /// <summary>
    /// Send a message to the specified event name. Requires a message payload that implements IMessagePayload.
    /// </summary>
    /// <param name="eventName">A name for the event, used to route messages</param>
    /// <param name="message">The payload of the message</param>
    /// <returns></returns>
    Task SendMessage(string eventName, IMessagePayload message);

    /// <summary>
    /// Subscribe to a specific event name to receive messages. Received events will trigger the OnMessageReceived event.
    /// The client can subscribe to multiple events, and all messages will be routed to the OnMessageReceived event handler.
    /// </summary>
    /// <param name="eventName">The name of the event to subscribe to</param>
    void Listen(string eventName);

    /// <summary>
    /// Subscribe to a specific event name from a specific component to receive messages. Received events will trigger the OnMessageReceived event.
    /// The client can subscribe to multiple events, and all messages will be routed to the OnMessageReceived event handler.
    /// </summary>
    /// <param name="component">The name of the component that produces the event</param>
    /// <param name="eventName">The name of the event to subscribe to</param>
    void Listen(string component, string eventName);

    /// <summary>
    /// Event triggered when a message is received. 
    /// The message can be deserialized into a specific type using the Deserialize method.
    /// </summary>
    event MessageReceivedDelegate OnMessageReceived;
}
