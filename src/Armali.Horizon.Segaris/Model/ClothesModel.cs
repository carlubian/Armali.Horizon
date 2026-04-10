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
    [ForeignKey("WashTypeId")]
    public ClothesWashType? WashType { get; set; }
    
    public bool IsPrivate { get; set; }
    
    public string Creator { get; set; } = string.Empty;
}

/// <summary>
/// Sub-entidad que asigna un color a una prenda con un nivel de importancia.
/// </summary>
public class ClothesColorAssignment
{
    public int Id { get; set; }
    
    public int GarmentId { get; set; }
    [ForeignKey("GarmentId")]
    public ClothesEntity? Garment { get; set; }
    
    public int ColorId { get; set; }
    [ForeignKey("ColorId")]
    public ClothesColor? Color { get; set; }
    
    public int StyleId { get; set; }
    [ForeignKey("StyleId")]
    public ClothesColorStyle? Style { get; set; }
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

/// <summary>
/// Catálogo de colores predefinidos con nombre y código hexadecimal.
/// </summary>
public class ClothesColor : Identifiable, Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Código hexadecimal del color (e.g. "#FF0000").
    /// </summary>
    [Required]
    public string Reference { get; set; } = string.Empty;
}

/// <summary>
/// Nivel de importancia del color en la prenda (Primary, Secondary, Details).
/// </summary>
public class ClothesColorStyle : Identifiable, Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
}
