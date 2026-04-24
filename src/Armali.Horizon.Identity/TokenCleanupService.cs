using Armali.Horizon.Identity.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Armali.Horizon.Identity;

/// <summary>
/// Servicio de fondo que purga tokens revocados/expirados con más de 30 días
/// de antigüedad. Se ejecuta inmediatamente al arrancar y luego cada 24 h.
/// </summary>
public class TokenCleanupService(IServiceProvider sp, ILogger<TokenCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan Period = TimeSpan.FromHours(24);
    private static readonly TimeSpan Retention = TimeSpan.FromDays(30);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = sp.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IdentityService>();
                await svc.PurgeOldTokensAsync(Retention);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error durante la purga de tokens");
            }
            
            try { await Task.Delay(Period, stoppingToken); }
            catch (TaskCanceledException) { /* shutdown */ }
        }
    }
}

