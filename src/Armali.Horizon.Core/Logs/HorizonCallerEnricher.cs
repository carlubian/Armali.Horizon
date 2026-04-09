using System.Diagnostics;
using System.Reflection;
using Serilog.Core;
using Serilog.Events;

namespace Armali.Horizon.Core.Logs;

public class HorizonCallerEnricher: ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        // 1. El nombre de la Clase (SourceContext)
        // Serilog ya captura esto automáticamente si usas ILogger<T>, 
        // pero se guarda como "SourceContext". Si quieres un alias explícito:
        if (logEvent.Properties.TryGetValue("SourceContext", out var sourceContext))
        {
            logEvent.AddPropertyIfAbsent(propertyFactory
                    .CreateProperty("ClassName", sourceContext));
        }

        // 2. El nombre del Método
        // Aquí es donde entra la "magia" (y el coste de reflexión).
        // Saltamos los frames de Serilog para encontrar tu código.
        var stackTrace = new StackTrace(skipFrames: 1); // Saltamos el frame actual
            
        // Buscamos el primer frame que NO sea de Serilog ni de System
        var relevantFrame = stackTrace.GetFrames()
            .FirstOrDefault(f => 
                f.GetMethod() != null &&
                f.GetMethod()!.DeclaringType != null &&
                !f.GetMethod()!.DeclaringType!.Assembly.FullName!.Contains("Serilog") &&
                !f.GetMethod()!.DeclaringType!.Assembly.FullName!.Contains("System.Private.CoreLib"));

        if (relevantFrame == null) return;
        
        var method = relevantFrame.GetMethod();
        var methodName = method?.Name ?? "UnknownMethod";

        // Pequeño fix para métodos asíncronos (que generan máquinas de estado)
        if (method?.DeclaringType?.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>() != null)
        {
            // Intentamos limpiar el nombre si es una Async State Machine
            methodName = "Async_" + method.DeclaringType.Name; 
        }

        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("MethodName", methodName));
    }
}