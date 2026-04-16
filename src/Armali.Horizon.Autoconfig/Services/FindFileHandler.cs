using Armali.Horizon.Autoconfig.Model;
using Armali.Horizon.IO;

namespace Armali.Horizon.Autoconfig.Services;

/// <summary>
/// Handler que resuelve peticiones de ficheros de configuración mediante FindValidFile.
/// </summary>
public class FindFileHandler(AutoconfigService svc) : IHorizonRequestHandler<FindFileRequest, FindFileResponse>
{
    public async Task<FindFileResponse> HandleAsync(FindFileRequest request, CancellationToken ct = default)
    {
        var result = await svc.FindValidFile(request.NodeName, request.AppName, request.Version, request.FileName);

        if (result == null)
            return new FindFileResponse { Found = false };

        return new FindFileResponse
        {
            Found = true,
            Content = result.Content,
            ResolvedVersion = result.ResolvedVersion
        };
    }
}

