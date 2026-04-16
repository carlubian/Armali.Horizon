using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Armali.Horizon.Blazor;

namespace Armali.Horizon.Autoconfig.Model;

public class AutoconfigNode : Nameable, Identifiable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string IconHint { get; set; } = string.Empty;
    
    public string Uri { get; set; } = string.Empty;
}

public class NodeStats
{
    public int AppCount { get; set; }
    public double TotalKbSize { get; set; }
}

public class AutoconfigApp : Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string IconHint { get; set; } = string.Empty;
    
    public int NodeId { get; set; }
    [ForeignKey("NodeId")]
    public AutoconfigNode? Node { get; set; }
}

public class AppStats
{
    public int VersionCount { get; set; }
    public double TotalKbSize { get; set; }
}

public class AutoconfigVersion : Nameable
{
    public int Id { get; set; }
    
    [Required]
    public int Major { get; set; }
    [Required]
    public int Minor { get; set; }
    [Required]
    public int Patch { get; set; }
    
    [Required]
    public DateTime Date { get; set; }

    public string Name => $"{Major}.{Minor}.{Patch}";

    public int AppId { get; set; }
    [ForeignKey("AppId")]
    public AutoconfigApp? App { get; set; }
}

public class AutoconfigFile : Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string IconHint { get; set; } = string.Empty;
    
    public int KbSize { get; set; }
    
    public int VersionId { get; set; }
    [ForeignKey("VersionId")]
    public AutoconfigVersion? Version { get; set; }
}

/// <summary>
/// DTO con los nombres resueltos de la jerarquía Nodo → App → Versión.
/// Se usa para construir las rutas del Datalake.
/// </summary>
public class VersionContext
{
    public string NodeName { get; set; } = string.Empty;
    public string AppName { get; set; } = string.Empty;
    public string VersionName { get; set; } = string.Empty;
}

/// <summary>
/// Resultado de <see cref="Services.AutoconfigService.FindValidFile"/>.
/// Contiene el contenido del fichero y la versión que lo proporcionó.
/// </summary>
public class FindFileResult
{
    public string Content { get; set; } = string.Empty;
    public string ResolvedVersion { get; set; } = string.Empty;
}
