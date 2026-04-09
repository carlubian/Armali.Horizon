using Microsoft.Extensions.Hosting;
using Serilog;

namespace Armali.Horizon.Core.Logs;

public class HorizonLogLifecycleService: IHostedService
{
    // No necesitamos hacer nada especial al inicio porque Serilog 
    // se inicializa en la construcción del Host, pero el Stop es crítico.
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken)
    {
        // "Flush" final para no perder ese último log de error crítico
        Log.CloseAndFlush();
        return Task.CompletedTask;
    }
}
