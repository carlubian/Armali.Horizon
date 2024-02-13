using Armali.Horizon.Test.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Armali.Horizon.Test;

internal class Program
{
    static async Task Main(string[] args)
    {
        var horizon = new Horizon(args);
        horizon.Initialize("Debug", "Horizon Test")
            .AddTermination()
            .AddLogging();

        // Add app-specific services here
        horizon.Services.AddHostedService<TestService>();
        //

        IHost host = horizon.Build();
        await host.RunAsync(horizon.CancellationToken.Token);
    }
}
