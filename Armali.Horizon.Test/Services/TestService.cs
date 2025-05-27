using Armali.Horizon.Logs;
using Armali.Horizon.Messaging;
using Armali.Horizon.Messaging.Model;
using Armali.Horizon.Test.Model;
using Microsoft.Extensions.Hosting;

namespace Armali.Horizon.Test.Services;

internal class TestService(IHorizonLogger log, AppTermination termination, IHorizonMessaging messaging) : IHostedService
{
    private readonly IHorizonLogger _log = log;
    private readonly AppTermination _termination = termination;
    private readonly IHorizonMessaging _messaging = messaging;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _log.Info("Hello, world");

        _messaging.OnMessageReceived += LogMessageReceived;
        await _messaging.SendMessage("Ping", new PingMessagePayload());

        _termination.Terminate();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _log.Info("Bye, world");

        await Task.CompletedTask;
    }

    private void LogMessageReceived(MessagePayload message)
    {
        var deserializedMessage = message.Deserialize<PingMessagePayload>();

        if (deserializedMessage is not null)
        {
            _log.Info($"Message received: {deserializedMessage.Message}");
            return;
        }

        _log.Warning($"Received unexpected message type: {message}");
    }
}
