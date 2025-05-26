using Armali.Horizon.Logs;
using Armali.Horizon.Messaging;
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

        _messaging.OnMessageReceived += msg => _log.Info($"Message received: {msg}");
        await _messaging.SendMessage("Message to Garnet");

        _termination.Terminate();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _log.Info("Bye, world");

        await Task.CompletedTask;
    }
}
