using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Armali.Horizon.Blazor;

namespace Armali.Horizon.Segaris.Model;

public class MaintEntity : Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    public string Details { get; set; } = string.Empty;
    
    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public MaintCategory? Category { get; set; }

    public int StatusId { get; set; }
    [ForeignKey("StatusId")]
    public MaintStatus? Status { get; set; }
    
    /// <summary>
    /// Enlace opcional a un activo.
    /// </summary>
    public int? AssetId { get; set; }
    
    public bool IsPrivate { get; set; }
    
    public string Creator { get; set; } = string.Empty;
}

public class MaintCategory : Identifiable, Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class MaintStatus : Identifiable, Nameable, Colorable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Color { get; set; } = string.Empty;
}