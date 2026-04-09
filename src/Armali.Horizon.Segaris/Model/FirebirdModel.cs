using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Armali.Horizon.Blazor;

namespace Armali.Horizon.Segaris.Model;

public class FirebirdEntity : Nameable
{
    public int Id { get; set; }

    [Required] 
    public string Name { get; set; } = string.Empty;
    
    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public FirebirdCategory? Category { get; set; }

    public int StatusId { get; set; }
    [ForeignKey("StatusId")]
    public FirebirdStatus? Status { get; set; }
    
    public string Location { get; set; } = string.Empty;
    
    public DateTime Birthday { get; set; }
    
    public bool IsAware { get; set; }
    
    public bool IsPrivate { get; set; }
    
    public string Creator { get; set; } = string.Empty;
}

public class FirebirdSubEntity
{
    public int Id { get; set; }
    
    [Required]
    public DateTime Date { get; set; }

    [Required] 
    public string Description { get; set; } = string.Empty;
    
    public int FirebirdId { get; set; }
    [ForeignKey("FirebirdId")]
    public FirebirdEntity? Contract { get; set; }
}

public class FirebirdCategory : Identifiable, Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class FirebirdStatus : Identifiable, Nameable, Colorable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Color { get; set; } = string.Empty;
}