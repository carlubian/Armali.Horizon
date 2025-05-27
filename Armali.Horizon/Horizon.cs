using Armali.Horizon.Logs;
using Armali.Horizon.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Armali.Horizon;

/// <summary>
/// Static entry point to configure the functionalities of the Horizon host.
/// </summary>
public class Horizon(string[] args)
{
    /// <summary>
    /// The Cancellation Token used to launch and manage the lifecycle of the app.
    /// </summary>
    public CancellationTokenSource CancellationToken {  get; private set; } = new();
#pragma warning disable CS8618
    public static IConfigurationRoot Configuration { get; private set; }
#pragma warning restore CS8618

    private readonly HostApplicationBuilder _builder = Host.CreateApplicationBuilder(args);

    public IServiceCollection Services { 
        get {  return _builder.Services; } 
    }

    /// <summary>
    /// Configures basic parameters of the Horizon app.
    /// </summary>
    public Horizon Initialize()
    {
        // Configuration system
        Configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        return this;
    }

    /// <summary>
    /// Configures the mechanism to stop a running app once it has finished execution.
    /// This is optional, if not configured, the app will run continuously.
    /// </summary>
    /// <param name="builder">The Host Application Builder</param>
    public Horizon AddTermination()
    {
        // App termination
        CancellationToken = new CancellationTokenSource();
        _builder.Services.AddSingleton<AppTermination>(new AppTermination(CancellationToken));

        return this;
    }

    /// <summary>
    /// Configures the Horizon log service.
    /// </summary>
    /// <param name="builder">The Host Application Builder</param>
    public Horizon AddLogging()
    {
        // Serilog configuration
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Seq(Configuration["Horizon:Logs:Endpoint"] ?? "http://localhost:5341")
            .CreateLogger();
        AppDomain.CurrentDomain.ProcessExit += (_1, _2) => Log.CloseAndFlush();

        // Microsoft ILogger configuration
        using ILoggerFactory factory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.AddSerilog();
            builder.AddFilter(Configuration["Horizon:Component"] ?? "Horizon", 
                (LogLevel)Enum.Parse(typeof(LogLevel), Configuration["Horizon:Logs:LogLevel"] ?? "Debug"));
        });
        Microsoft.Extensions.Logging.ILogger logger = factory.CreateLogger(Configuration["Horizon:Component"] ?? "Horizon");

        _builder.Services.AddSingleton<IHorizonLogger>(new HorizonLogger(logger));

        return this;
    }

    public Horizon AddMessaging()
    {
        _builder.Services.AddSingleton<IHorizonMessaging>(provider =>
        {
            var logService = provider.GetRequiredService<IHorizonLogger>();
            var msgService = new HorizonMessaging(logService);
            var endpoint = Configuration["Horizon:Messaging:Endpoint"] ?? "localhost:6400";
            msgService.SetConnection(endpoint);
            msgService.SetComponent(Configuration["Horizon:Component"] ?? "Horizon");

            return msgService;
        });
        _builder.Services.AddSingleton<IHostedService, IHorizonMessaging>(provider => provider.GetRequiredService<IHorizonMessaging>());

        return this;
    }

    public IHost Build() => _builder.Build();
}
