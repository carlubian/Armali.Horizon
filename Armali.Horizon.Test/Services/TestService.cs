using Armali.Horizon.Logs;
using Microsoft.Extensions.Hosting;

namespace Armali.Horizon.Test.Services;

internal class TestService : IHostedService
{
    private IHorizonLogger _log;
    private AppTermination _termination;

    public TestService(IHorizonLogger log, AppTermination termination)
    {
        _log = log;
        _termination = termination;

    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _log.Info("Hello, world");
        _termination.Terminate();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _log.Info("Bye, world");
    }
}
