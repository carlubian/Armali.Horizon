using Microsoft.Extensions.Logging;

namespace Armali.Horizon.Logs;

public class HorizonLogger(ILogger logger) : IHorizonLogger
{
    private readonly ILogger _logger = logger;

    public void Trace(string template, params object[] segments)
    {
        _logger.LogDebug($"{{NodeName}}:{{AppName}} {template}", new string[] { Horizon._nodeName, Horizon._appName }.Union(segments).ToArray());
    }

    public void Info(string template, params object[] segments)
    {
        _logger.LogInformation($"{{NodeName}}:{{AppName}} {template}", new string[] { Horizon._nodeName, Horizon._appName }.Union(segments).ToArray());
    }
    public void Warning(string template, params object[] segments)
    {
        _logger.LogWarning($"{{NodeName}}:{{AppName}} {template}", new string[] { Horizon._nodeName, Horizon._appName }.Union(segments).ToArray());
    }

    public void Error(string template, params object[] segments)
    {
        _logger.LogError($"{{NodeName}}:{{AppName}} {template}", new string[] { Horizon._nodeName, Horizon._appName }.Union(segments).ToArray());
    }
}
