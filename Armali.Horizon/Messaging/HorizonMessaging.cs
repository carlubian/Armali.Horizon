using Armali.Horizon.Logs;
using Armali.Horizon.Messaging.Model;
using StackExchange.Redis;
using System.Text.Json;

namespace Armali.Horizon.Messaging;

public class HorizonMessaging(IHorizonLogger log) : IHorizonMessaging
{
    private readonly IHorizonLogger _log = log;
    private string _connectionString = "localhost:6400";
    private string _component = "Horizon";

#pragma warning disable CS8618 // These fields are set by StartAsync, called automatically by the host
    private ConnectionMultiplexer _conn;
    private IDatabase _db;
#pragma warning restore CS8618

    public event MessageReceivedDelegate? OnMessageReceived;

    public void SetConnection(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void SetComponent(string component)
    {
        _component = component;
    }

    public async Task SendMessage(string eventName, IMessagePayload message)
    {
        // Serialize the message payload to JSON
        var json = JsonSerializer.Serialize(message);
        var subscriber = _conn.GetSubscriber();
        var channel = $"horizon:{_component}:{eventName}";

        _log.Trace("Publishing message to channel {Channel} - {Message}", channel, json);

        // Publish the message to the specified channel
        await subscriber.PublishAsync(channel, json);
    }

    public void Listen(string eventName)
    {
        Listen(_component, eventName);
    }

    public void Listen(string component, string eventName)
    {
        var channel = $"horizon:{component}:{eventName}";
        var subscriber = _conn.GetSubscriber();

        subscriber.Subscribe(channel, (channel, message) =>
        {
            _log.Trace("Received message on channel {Channel} - {Message}", channel, message);
            // Deserialize the message into a MessagePayload
            var payload = JsonSerializer.Deserialize<MessagePayload>(message!);
            if (payload != null)
            {
                payload.RawData = message!;
                OnMessageReceived?.Invoke(payload);
            }
            else
            {
                _log.Warning("Failed to deserialize message: {Message}", message);
            }
        });
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _log.Trace("Messaging is starting");

        _conn = ConnectionMultiplexer.Connect(_connectionString);
        _db = _conn.GetDatabase();

        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _log.Trace("Messaging is stopping");
        _conn.Close();

        await Task.CompletedTask;
    }
}
