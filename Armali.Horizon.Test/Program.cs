using Armali.Horizon.Test.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Armali.Horizon.Test;

internal class Program
{
    static async Task Main(string[] args)
    {
        var horizon = new Horizon(args);
        horizon.Initialize()
            .AddTermination()
            .AddLogging()
            .AddMessaging();

        // Add app-specific services here
        horizon.Services.AddSingleton<IHostedService, TestService>();
        //

        IHost host = horizon.Build();
        await host.RunAsync(horizon);
    }
}
