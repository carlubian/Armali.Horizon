using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Armali.Horizon.Blazor;

namespace Armali.Horizon.Segaris.Model;

public class AssetEntity : Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string AssetCode { get; set; } = string.Empty;
    
    public string Location { get; set; } = string.Empty;
    
    [Required]
    public DateTime Date { get; set; }
    
    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public AssetCategory? Category { get; set; }

    public int StatusId { get; set; }
    [ForeignKey("StatusId")]
    public AssetStatus? Status { get; set; }
    
    /// <summary>
    /// Enlace opcional a un proyecto.
    /// </summary>
    public int? ProjectId { get; set; }
    
    public bool IsPrivate { get; set; }
    
    public string Creator { get; set; } = string.Empty;
}

public class AssetCategory : Identifiable, Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class AssetStatus : Identifiable, Nameable, Colorable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Color { get; set; } = string.Empty;
}