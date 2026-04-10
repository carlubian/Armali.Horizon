using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Armali.Horizon.IO;

public static class HorizonEventExtensions
{
    public static IHostBuilder UseHorizonEvents(this IHostBuilder builder)
    {
        return builder.ConfigureServices((context, services) =>
        {
            // Recuperamos la configuración o usamos defaults
            var settings = context.Configuration.GetSection("Horizon").GetSection("Events")
                .Get<HorizonEventSettings>() ?? new HorizonEventSettings();
            
            // Asignamos la configuración estática
            HorizonEventService.Settings = settings;

            services.AddHostedService<HorizonEventService>();
        });
    }
}
