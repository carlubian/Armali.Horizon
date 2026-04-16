using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Armali.Horizon.Blazor;

namespace Armali.Horizon.Segaris.Model;

public class AdminEntity : Nameable
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public AdminCategory? Category { get; set; }

    public bool IsPrivate { get; set; }

    public string Creator { get; set; } = string.Empty;
}

public class AdminSubEntity
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime DueDate { get; set; }

    public bool IsCompleted { get; set; }

    public int ProcessId { get; set; }
    [ForeignKey("ProcessId")]
    public AdminEntity? Process { get; set; }
}

public class AdminCategory : Identifiable, Nameable
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Estado calculado de un paso/etapa de un proceso administrativo.
/// </summary>
public enum AdminStepStatus
{
    /// <summary>Completado (sin importar fechas).</summary>
    Finished,
    /// <summary>Fecha de inicio en el futuro, no completado.</summary>
    NotStarted,
    /// <summary>En curso y dentro de plazo (DueDate futuro).</summary>
    OnTime,
    /// <summary>Fuera de plazo y no completado.</summary>
    Delayed
}

/// <summary>
/// Resumen agregado de los pasos de un AdminEntity.
/// </summary>
public class AdminStats
{
    public int Finished { get; set; }
    public int NotStarted { get; set; }
    public int OnTime { get; set; }
    public int Delayed { get; set; }

    public int Total => Finished + NotStarted + OnTime + Delayed;

    /// <summary>
    /// Color general del proceso: verde si todos completados, rojo si hay algún
    /// paso retrasado, amarillo si hay pendientes sin retraso, gris si no hay pasos.
    /// </summary>
    public string OverallColor
    {
        get
        {
            if (Total == 0) return "gray";
            if (Delayed > 0) return "red";
            if (Finished == Total) return "green";
            return "gold";
        }
    }

    /// <summary>
    /// Nombre descriptivo del estado general.
    /// </summary>
    public string OverallName
    {
        get
        {
            if (Total == 0) return "Empty";
            if (Delayed > 0) return "Delayed";
            if (Finished == Total) return "Finished";
            return "In Progress";
        }
    }

    /// <summary>
    /// Resumen legible: "2 finished, 1 on time, 1 delayed".
    /// </summary>
    public string Summary
    {
        get
        {
            if (Total == 0) return "No steps";
            var parts = new List<string>();
            if (Finished > 0) parts.Add($"{Finished} finished");
            if (NotStarted > 0) parts.Add($"{NotStarted} not started");
            if (OnTime > 0) parts.Add($"{OnTime} on time");
            if (Delayed > 0) parts.Add($"{Delayed} delayed");
            return string.Join(", ", parts);
        }
    }
}

/// <summary>
/// Pseudo-estado calculado compatible con HorizonCellStatus (Identifiable, Nameable, Colorable).
/// </summary>
public class AdminComputedStatus : Identifiable, Nameable, Colorable
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;

    public static AdminComputedStatus FromStats(AdminStats stats) => new()
    {
        Id = 0,
        Name = stats.OverallName,
        Color = stats.OverallColor
    };
}

