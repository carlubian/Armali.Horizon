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

        var response = await _db.ExecuteAsync("PING", json);

        // Partial deserialization to base MessagePayload
        var basePayload = JsonSerializer.Deserialize<MessagePayload>(json);
        basePayload!.RawData = response.ToString();

        OnMessageReceived?.Invoke(basePayload!); // TODO Allow clients to subscribe to specific events
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
