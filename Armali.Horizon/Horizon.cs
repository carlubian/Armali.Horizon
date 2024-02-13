using Armali.Horizon.Logs;
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
    public static IConfigurationRoot Configuration { get; private set; }

    internal static string _nodeName = "Unknown node";
    internal static string _appName = "Unknown app";

    private readonly HostApplicationBuilder _builder = Host.CreateApplicationBuilder(args);

    public IServiceCollection Services { 
        get {  return _builder.Services; } 
    }

    /// <summary>
    /// Configures basic launch parameters of the Horizon app.
    /// </summary>
    /// <param name="hostName">A name to identify the device in which the app is hosted.</param>
    /// <param name="appName">A name to identify the app inside the device.</param>
    public Horizon Initialize(string hostName, string appName)
    {
        // Deployment information
        _nodeName = hostName;
        _appName = appName;

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
        });
        Microsoft.Extensions.Logging.ILogger logger = factory.CreateLogger("Armali Horizon");

        _builder.Services.AddSingleton<IHorizonLogger>(new HorizonLogger(logger));

        return this;
    }

    public IHost Build() => _builder.Build();
}
