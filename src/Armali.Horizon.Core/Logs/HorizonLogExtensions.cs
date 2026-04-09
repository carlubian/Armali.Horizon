using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace Armali.Horizon.Core.Logs;

public static class HorizonLogExtensions
{
    public static IHostBuilder UseHorizonLogging(this IHostBuilder builder)
    {
        return builder.UseSerilog((context, services, configuration) =>
            {
                // Recuperamos la configuración o usamos defaults
                var settings = context.Configuration.GetSection("Horizon").GetSection("Logging")
                                   .Get<HorizonLogSettings>() ?? new HorizonLogSettings();

                // Nivel mínimo de log
                var levelSwitch = new Serilog.Core.LoggingLevelSwitch();
                if (Enum.TryParse(settings.MinimumLevel, out LogEventLevel level))
                    levelSwitch.MinimumLevel = level;

                configuration
                    .MinimumLevel.ControlledBy(levelSwitch)
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // Reducir ruido de .NET
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
                    .Enrich.With(new HorizonCallerEnricher())
                    .WriteTo.Console(outputTemplate: 
                        "[{Timestamp:HH:mm:ss} {Level:u3}] [{ClassName}.{MethodName}] {Message:lj}{NewLine}{Exception}")
                    // Seq: WriteTo es fire-and-forget con buffer interno y reintentos automáticos.
                    // Si Seq no está disponible, los eventos se descartan silenciosamente sin afectar a la app.
                    .WriteTo.Seq(
                        settings.Endpoint,
                        apiKey: settings.ApiKey,
                        queueSizeLimit: settings.QueueSizeLimit);
            })
            .ConfigureServices((context, services) =>
            {
                // Aquí registramos nuestro "Guardían"
                services.AddHostedService<HorizonLogLifecycleService>();
            });
    }
}
