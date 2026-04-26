using System.Text;
using Armali.Horizon.Autoconfig.Services;
using Armali.Horizon.Contracts.Autoconfig;
using Armali.Horizon.IO;

namespace Armali.Horizon.Autoconfig.Handlers;

/// <summary>
/// Handler IO de la operación <see cref="GetConfigFileRequest"/>.
/// Resuelve la mejor versión compatible que contenga el archivo y devuelve
/// su contenido como texto. Las peticiones son anónimas (no requieren token).
/// </summary>
public class GetConfigFileHandler : IHorizonRequestHandler<GetConfigFileRequest, GetConfigFileResponse>
{
    private readonly AutoconfigService Service;
    private readonly AutoconfigDatalakeService Datalake;
    private readonly AutoconfigOptions Options;

    public GetConfigFileHandler(AutoconfigService service, AutoconfigDatalakeService datalake, AutoconfigOptions options)
    {
        Service = service;
        Datalake = datalake;
        Options = options;
    }

    public async Task<GetConfigFileResponse> HandleAsync(GetConfigFileRequest req, CancellationToken ct = default)
    {
        // Validación del formato A.B.C
        if (!TryParseVersion(req.Version, out var major, out var minor, out var patch))
            return Fail(AutoconfigErrorCodes.InvalidVersion,
                $"La versión '{req.Version}' no tiene formato 'A.B.C' con enteros no negativos.");

        // Distinguimos node_not_found / app_not_found para dar diagnóstico útil al cliente.
        if (!await Service.NodeExists(req.NodeName))
            return Fail(AutoconfigErrorCodes.NodeNotFound, $"No existe el nodo '{req.NodeName}'.");
        if (!await Service.AppExists(req.NodeName, req.AppName))
            return Fail(AutoconfigErrorCodes.AppNotFound,
                $"No existe la app '{req.AppName}' en el nodo '{req.NodeName}'.");

        var Resolved = await Service.ResolveBestVersionFileAsync(
            req.NodeName, req.AppName, major, minor, patch, req.FileName);
        if (Resolved is null)
            return Fail(AutoconfigErrorCodes.FileNotFound,
                $"No hay versión compatible con '{req.Version}' que contenga el archivo '{req.FileName}'.");

        var (ctx, _) = Resolved.Value;

        // Descarga del Datalake con tope de tamaño + decodificación UTF-8 estricta.
        try
        {
            await using var stream = await Datalake.GetFileAsync(
                ctx.NodeName, ctx.AppName, ctx.VersionName, req.FileName);

            using var buffer = new MemoryStream();
            var max = Options.MaxFileBytes;
            var chunk = new byte[8192];
            int read;
            while ((read = await stream.ReadAsync(chunk.AsMemory(0, chunk.Length), ct)) > 0)
            {
                if (buffer.Length + read > max)
                    return Fail(AutoconfigErrorCodes.TooLarge,
                        $"El archivo supera el tamaño máximo permitido ({max} bytes).");
                buffer.Write(chunk, 0, read);
            }

            string text;
            try
            {
                // UTF-8 estricto: lanza si los bytes no son válidos.
                text = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true)
                    .GetString(buffer.GetBuffer(), 0, (int)buffer.Length);
            }
            catch (DecoderFallbackException)
            {
                return Fail(AutoconfigErrorCodes.NotText,
                    $"El archivo '{req.FileName}' no es texto UTF-8 válido.");
            }

            return new GetConfigFileResponse
            {
                Found = true,
                ResolvedVersion = ctx.VersionName,
                Content = text,
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return Fail(AutoconfigErrorCodes.Internal, $"Error leyendo el archivo: {ex.Message}");
        }
    }

    private static GetConfigFileResponse Fail(string code, string message) =>
        new()
        {
            Found = false,
            Error = new AutoconfigErrorInfo { Code = code, Message = message },
        };

    /// <summary>Parsea una cadena <c>A.B.C</c> con enteros no negativos.</summary>
    public static bool TryParseVersion(string raw, out int major, out int minor, out int patch)
    {
        major = minor = patch = 0;
        if (string.IsNullOrWhiteSpace(raw)) return false;
        var parts = raw.Split('.');
        if (parts.Length != 3) return false;
        return int.TryParse(parts[0], out major) && major >= 0
            && int.TryParse(parts[1], out minor) && minor >= 0
            && int.TryParse(parts[2], out patch) && patch >= 0;
    }
}

/// <summary>
/// Opciones de configuración de Autoconfig (sección <c>Horizon:Autoconfig</c>).
/// </summary>
public class AutoconfigOptions
{
    /// <summary>Tamaño máximo en bytes del archivo a devolver vía IO. Por defecto 2 MB.</summary>
    public int MaxFileBytes { get; set; } = 2 * 1024 * 1024;
}


