using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Armali.Horizon.Blazor;

namespace Armali.Horizon.Segaris.Model;

public class ProjectEntity : Nameable
{
    public int Id { get; set; }

    [Required] 
    public string Name { get; set; } = string.Empty;
    
    public string Code { get; set; } = string.Empty;
    
    public int ProgramId { get; set; }
    [ForeignKey("ProgramId")]
    public ProjectProgram? Program { get; set; }
    
    public int AxisId { get; set; }
    [ForeignKey("AxisId")]
    public ProjectAxis? Axis { get; set; }

    public int StatusId { get; set; }
    [ForeignKey("StatusId")]
    public ProjectStatus? Status { get; set; }
    
    public bool IsPrivate { get; set; }
    
    public string Creator { get; set; } = string.Empty;
}

public class ProjectSubEntity
{
    public int Id { get; set; }
    
    [Required] 
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public DateTime Date { get; set; }
    
    public string File { get; set; } = string.Empty;
    
    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public ProjectSubEntityCategory? Category { get; set; }
    
    public int ProjectId { get; set; }
    [ForeignKey("ProjectId")]
    public ProjectEntity? Project { get; set; }
}

public class ProjectSubEntityCategory : Identifiable, Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class ProjectProgram : Identifiable, Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class ProjectAxis : Identifiable, Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public int ProgramId { get; set; }
    //[ForeignKey("ProgramId")] EF seems confused by this relationship, so we will handle it manually in the service layer
    //public ProjectEntity? Program { get; set; }
}

public class ProjectStatus : Identifiable, Nameable, Colorable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Color { get; set; } = string.Empty;
}

public class ProjectRiskCategory : Identifiable, Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class ProjectRiskElement
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public ProjectRiskCategory? Category { get; set; }
    
    [Required]
    [Range(1, 10)]
    public int Probability { get; set; } = 1;
    
    [Required]
    [Range(1, 10)]
    public int Severity { get; set; } = 1;
    
    [Required]
    [Range(1, 10)]
    public int Mitigation { get; set; } = 1;
    
    public int ProjectId { get; set; }
    [ForeignKey("ProjectId")]
    public ProjectEntity? Project { get; set; }
    
    /// <summary>
    /// Score calculado: Probability × Severity × Mitigation. No se persiste en BD.
    /// </summary>
    [NotMapped]
    public int Score => Probability * Severity * Mitigation;
}

public class ProjectBudget
{
    public int Id { get; set; }
    
    [Required]
    [Range(2000, 2200)]
    public int Year { get; set; } = DateTime.Today.Year;
    
    [Required]
    public double Estimated { get; set; }
    
    [Required]
    public double Actual { get; set; }
    
    public int ProjectId { get; set; }
    [ForeignKey("ProjectId")]
    public ProjectEntity? Project { get; set; }
    
    /// <summary>
    /// Porcentaje del presupuesto estimado ya gastado: (Actual / Estimated) × 100.
    /// Devuelve 0 si Estimated es 0 para evitar división por cero.
    /// </summary>
    [NotMapped]
    public double SpentPercent => Estimated == 0 ? 0 : (Actual / Estimated) * 100;
}

