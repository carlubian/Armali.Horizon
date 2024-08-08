using Microsoft.Extensions.Hosting;

namespace Armali.Horizon;

public static class Extensions
{
    public static Task RunAsync(this IHost host, Horizon horizon)
    {
        return host.RunAsync(horizon.CancellationToken.Token);
    }
}
