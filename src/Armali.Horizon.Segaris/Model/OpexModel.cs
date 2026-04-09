using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Armali.Horizon.Blazor;

namespace Armali.Horizon.Segaris.Model;

public class OpexEntity : Nameable
{
    public int Id { get; set; }

    [Required] 
    public string Name { get; set; } = string.Empty;
    
    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public OpexCategory? Category { get; set; }

    public int StatusId { get; set; }
    [ForeignKey("StatusId")]
    public OpexStatus? Status { get; set; }
    
    /// <summary>
    /// Enlace opcional a un proyecto.
    /// </summary>
    public int? ProjectId { get; set; }
    
    public bool IsPrivate { get; set; }
    
    public string Creator { get; set; } = string.Empty;
}

public class OpexSubEntity
{
    public int Id { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    public double Amount { get; set; }
    
    public int ContractId { get; set; }
    [ForeignKey("ContractId")]
    public OpexEntity? Contract { get; set; }
}

public class OpexCategory : Identifiable, Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class OpexStatus : Identifiable, Nameable, Colorable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Color { get; set; } = string.Empty;
}

public class OpexStats
{
    public int SubEntityCount { get; set; }
    public double TotalAmount { get; set; }
}