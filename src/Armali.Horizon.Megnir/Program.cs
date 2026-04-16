using Armali.Horizon.Core.Logs;
using Armali.Horizon.IO;
using Armali.Horizon.Megnir.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .UseHorizonLogging()
    .UseHorizonEvents()
    .ConfigureServices(services =>
    {
        services.AddHostedService<MegnirWorker>();
    })
    .Build();

host.Run();

