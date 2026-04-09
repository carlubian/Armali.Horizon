using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Armali.Horizon.Blazor;

namespace Armali.Horizon.Segaris.Model;

public class TravelEntity : Nameable
{
    public int Id { get; set; }

    [Required] 
    public string Name { get; set; } = string.Empty;
    
    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public TravelCategory? Category { get; set; }

    public int StatusId { get; set; }
    [ForeignKey("StatusId")]
    public TravelStatus? Status { get; set; }
    
    public int CostCenterId { get; set; }
    [ForeignKey("CostCenterId")]
    public TravelCostCenter? CostCenter { get; set; }
    
    public string Destination { get; set; } = string.Empty;
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    public int Pax { get; set; }
    
    /// <summary>
    /// Enlace opcional a un proyecto.
    /// </summary>
    public int? ProjectId { get; set; }
    
    public bool IsPrivate { get; set; }
    
    public string Creator { get; set; } = string.Empty;
}

public class TravelSubEntity
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public TravelSubEntityCategory? Category { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    public double Amount { get; set; }
    
    public int TravelId { get; set; }
    [ForeignKey("TravelId")]
    public TravelEntity? Travel { get; set; }
}

public class TravelCategory : Identifiable, Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class TravelStatus : Identifiable, Nameable, Colorable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Color { get; set; } = string.Empty;
}

public class TravelCostCenter : Identifiable, Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class TravelSubEntityCategory : Identifiable, Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
}