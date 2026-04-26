namespace Armali.Horizon.Core.Model;

/// <summary>
/// Identidad ligera del usuario autenticado, distribuida por el sistema de eventos.
/// <para>
/// La emite el servicio Horizon.Identity tras un login o validación de token, y se
/// almacena del lado cliente (en localStorage para apps Blazor o en memoria para
/// apps headless). Consumidores como Segaris o Autoconfig deciden permisos
/// consultando <see cref="Roles"/> mediante <see cref="HasRole"/>.
/// </para>
/// </summary>
public class HorizonIdentity
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    /// <summary>Roles asignados al usuario (vacío si no tiene ninguno).</summary>
    public string[] Roles { get; set; } = [];

    /// <summary>
    /// Token bearer opaco usado para autenticar peticiones IO.
    /// Se rellena tras un login y debe enviarse en cada <c>AuthenticatedRequest</c>.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>Indica si el usuario tiene el rol indicado (case-insensitive).</summary>
    public bool HasRole(string role) =>
        Roles.Any(r => string.Equals(r, role, StringComparison.OrdinalIgnoreCase));
}