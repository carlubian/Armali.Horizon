using Microsoft.Extensions.Logging;
using Pastel;
using System.Diagnostics;

namespace Armali.Horizon.Logs;

#pragma warning disable IDE0079
#pragma warning disable CA2254
public class HorizonLogger(ILogger logger) : IHorizonLogger
{
    private readonly ILogger _logger = logger;

    public void Trace(string template, params object[] segments)
    {
        _logger.LogDebug($"{{Class}}:{{Method}}".Pastel(ConsoleColor.Cyan) + $" {template}".Pastel(ConsoleColor.White),
            [.. GetCallerFromStack().Union(segments)]);
    }

    public void Info(string template, params object[] segments)
    {
        _logger.LogInformation($"{{Class}}:{{Method}}".Pastel(ConsoleColor.Cyan) + $" {template}".Pastel(ConsoleColor.White),
            [.. GetCallerFromStack().Union(segments)]);
    }

    public void Warning(string template, params object[] segments)
    {
        _logger.LogWarning($"{{Class}}:{{Method}}".Pastel(ConsoleColor.Cyan) + $" {template}".Pastel(ConsoleColor.White), 
            [.. GetCallerFromStack().Union(segments)]);
    }

    public void Error(string template, params object[] segments)
    {
        _logger.LogError($"{{Class}}:{{Method}}".Pastel(ConsoleColor.Cyan) + $" {template}".Pastel(ConsoleColor.White), 
            [.. GetCallerFromStack().Union(segments)]);
    }

    #pragma warning disable CA1822
    private string[] GetCallerFromStack()
    {
        var stack = new StackTrace();
        var caller = stack.GetFrame(2);

        /*
         * In the case of async methods, an additional two stack frames
         * are added by .NET to wrap the asynchronicity. These use a dynamic
         * generated class name that contains a "d__2" or similar string in
         * its name. By detecting this fragment and skipping to the real
         * frame, the same code can be used for sync/async calls.
         */
        if (caller?.GetMethod()?.DeclaringType?.Name.Contains("__") ?? false)
            caller = stack.GetFrame(4);

        var methodName = caller?.GetMethod()?.Name ?? "UMTH";
        var className = caller?.GetMethod()?.DeclaringType?.Name ?? "UCLS";

        /*
         * Experimental catch for anonymous lambda expressions that don't have
         * a real method name. Probably won't be truly representative, but
         * the class name at least is correct in this case.
         */
        if (methodName.Contains("__"))
            methodName = stack.GetFrame(6)?.GetMethod()?.Name ?? "UMTH";

        return [className, methodName];
    }
}
