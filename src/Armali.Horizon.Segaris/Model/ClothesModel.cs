using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Armali.Horizon.Blazor;

namespace Armali.Horizon.Segaris.Model;

public class ClothesEntity : Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public DateTime Date { get; set; }
    
    public string GarmentCode { get; set; } = string.Empty;
    
    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public ClothesCategory? Category { get; set; }

    public int StatusId { get; set; }
    [ForeignKey("StatusId")]
    public ClothesStatus? Status { get; set; }
    
    public int WashTypeId { get; set; }
    [ForeignKey("CategoryId")]
    public ClothesWashType? WashType { get; set; }
    
    public bool IsPrivate { get; set; }
    
    public string Creator { get; set; } = string.Empty;
}

public class ClothesCategory : Identifiable, Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class ClothesStatus : Identifiable, Nameable, Colorable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Color { get; set; } = string.Empty;
}

public class ClothesWashType : Identifiable, Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
}