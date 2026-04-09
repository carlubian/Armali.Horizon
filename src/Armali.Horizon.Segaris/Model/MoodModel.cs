using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Armali.Horizon.Blazor;

namespace Armali.Horizon.Segaris.Model;

public class MoodEntity
{
    public int Id { get; set; }
    
    [Required]
    public DateTime Date { get; set; }
    
    [Required]
    public int Score { get; set; }
    
    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public MoodCategory? Category { get; set; }
    
    public bool IsPrivate { get; set; }
    
    public string Creator { get; set; } = string.Empty;
}

public class MoodCategory : Identifiable, Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string PrimaryColor { get; set; } = string.Empty;
    
    public string SecondaryColor { get; set; } = string.Empty;
}

public class MoodDailyStats
{
    public DateTime Date { get; set; }

    public double Min { get; set; }

    public double Avg { get; set; }

    public double Max { get; set; }
}
