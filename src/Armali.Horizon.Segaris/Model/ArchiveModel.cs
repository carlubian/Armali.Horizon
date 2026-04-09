using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Armali.Horizon.Blazor;

namespace Armali.Horizon.Segaris.Model;

public class ArchiveEntity : Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public DateTime Date { get; set; }
    
    public string File { get; set; } = string.Empty;
    
    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public ArchiveCategory? Category { get; set; }

    public int StatusId { get; set; }
    [ForeignKey("StatusId")]
    public ArchiveStatus? Status { get; set; }
    
    public bool IsPrivate { get; set; }
    
    public string Creator { get; set; } = string.Empty;
}

public class ArchiveCategory : Identifiable, Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class ArchiveStatus : Identifiable, Nameable, Colorable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Color { get; set; } = string.Empty;
}