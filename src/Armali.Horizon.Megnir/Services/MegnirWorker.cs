using System.Reflection;
using System.Text.Json;
using Armali.Horizon.IO;
using Armali.Horizon.Megnir.Model;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Armali.Horizon.Megnir.Services;

/// <summary>
/// Worker principal de Megnir. Ejecuta la tarea programada y detiene el host al finalizar (modo run-once).
/// </summary>
public class MegnirWorker(IHostApplicationLifetime lifetime, ILogger<MegnirWorker> logger, HorizonEventService eventService) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Megnir iniciado");

        try
        {
            await RunAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error durante la ejecución de Megnir");
            Environment.ExitCode = 1;
        }
        finally
        {
            logger.LogInformation("Megnir finalizado");
            lifetime.StopApplication();
        }
    }

    /// <summary>
    /// Punto de entrada de la tarea programada.
    /// </summary>
    private async Task RunAsync(CancellationToken ct)
    {
        var config = await LoadConfigAsync(ct);

        logger.LogInformation("Configuración cargada: {JobCount} jobs definidos", config.Jobs.Length);
        foreach (var job in config.Jobs)
        {
            logger.LogInformation("Job '{Name}': {FileCount} ficheros, {DirCount} directorios",
                job.Name, job.Files.Length, job.Directories.Length);
        }

        // TODO: ejecutar los jobs de backup
    }

    /// <summary>
    /// Solicita la configuración megnir.json a Autoconfig vía Horizon.IO.
    /// Lanza excepción si no hay respuesta o el fichero no existe.
    /// </summary>
    internal async Task<MegnirBackup> LoadConfigAsync(CancellationToken ct)
    {
        var nodeName = Environment.GetEnvironmentVariable("HORIZON_NODE")
            ?? throw new InvalidOperationException("Variable de entorno HORIZON_NODE no definida");
        var appName = Environment.GetEnvironmentVariable("HORIZON_APP")
            ?? throw new InvalidOperationException("Variable de entorno HORIZON_APP no definida");
        var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "0.0.0";

        logger.LogInformation("Solicitando configuración: nodo={Node}, app={App}, versión={Version}",
            nodeName, appName, version);

        var response = await eventService.RequestAsync<FindFileResponse>(
            "autoconfig",
            new FindFileRequest
            {
                NodeName = nodeName,
                AppName = appName,
                Version = version,
                FileName = "megnir.json"
            },
            ct: ct);

        if (!response.Found)
            throw new InvalidOperationException(
                $"Autoconfig no encontró megnir.json para nodo='{nodeName}', app='{appName}', versión='{version}'");

        logger.LogInformation("Configuración resuelta desde versión {ResolvedVersion}", response.ResolvedVersion);

        var backup = JsonSerializer.Deserialize<MegnirBackup>(response.Content)
            ?? throw new InvalidOperationException("El contenido de megnir.json no es un JSON válido de MegnirBackup");

        if (backup.Jobs.Length == 0)
            logger.LogWarning("La configuración megnir.json no contiene ningún job");

        return backup;
    }
}
