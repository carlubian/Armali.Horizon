using Armali.Horizon.Logs;
using StackExchange.Redis;

namespace Armali.Horizon.Messaging;

public class HorizonMessaging(IHorizonLogger log) : IHorizonMessaging
{
    private readonly IHorizonLogger _log = log;
    private string _connectionString = "localhost:6400";

#pragma warning disable CS8618 // These fields are set by StartAsync, called automatically by the host
    private ConnectionMultiplexer _conn;
    private IDatabase _db;
#pragma warning restore CS8618

    public event MessageReceivedDelegate? OnMessageReceived;

    public void SetConnection(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task SendMessage(object message)
    {
        var response = await _db.ExecuteAsync("PING", message);
        OnMessageReceived?.Invoke(response);
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
