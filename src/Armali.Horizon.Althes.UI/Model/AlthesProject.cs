using System.ComponentModel.DataAnnotations;

namespace Armali.Horizon.Althes.UI.Model;

/// <summary>
/// Conexión persistida a una instancia de Althes. Cada proyecto Althes
/// comparte el mismo Redis que el resto del stack Horizon, por lo que la
/// "conexión" es simplemente el <see cref="ProjectId"/> que identifica los
/// canales del bus IO, más la API key con la que la UI se autentica.
/// </summary>
public class AlthesProject
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>Nombre para mostrar en la UI.</summary>
    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// ProjectId del contenedor Althes (valor de <c>ALTHES_PROJECT_ID</c>).
    /// Determina el prefijo de los canales IO: <c>althes:{ProjectId}</c>.
    /// </summary>
    [Required, MaxLength(100)]
    public string ProjectId { get; set; } = string.Empty;
    
    /// <summary>
    /// API key Identity para autenticar las peticiones al bus IO de esta instancia.
    /// Debe pertenecer a un usuario con rol <c>althes.user</c>.
    /// Edición restringida a rol <c>admin</c> en la UI.
    /// </summary>
    [Required]
    public string ApiKey { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

