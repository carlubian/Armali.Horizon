using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Armali.Horizon.Blazor;

namespace Armali.Horizon.Segaris.Model;

public class InvVendorEntity : Nameable, Identifiable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;

    public int StatusId { get; set; }
    [ForeignKey("StatusId")]
    public InvVendorStatus? Status { get; set; }
    
    public bool IsPrivate { get; set; }
    
    public string Creator { get; set; } = string.Empty;
}

public class InvVendorStatus : Identifiable, Nameable, Colorable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Color { get; set; } = string.Empty;
}

public class InvItemEntity : Identifiable, Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public int CurrentStock { get; set; }
    
    public int MinStock { get; set; }
    
    public int CategoryId { get; set; }
    [ForeignKey("CategoryId")]
    public InvItemCategory? Category { get; set; }

    public int StatusId { get; set; }
    [ForeignKey("StatusId")]
    public InvItemStatus? Status { get; set; }
    
    public int VendorId { get; set; }
    [ForeignKey("VendorId")]
    public InvVendorEntity? Vendor { get; set; }
    
    public bool IsPrivate { get; set; }
    
    public string Creator { get; set; } = string.Empty;
}

public class InvItemCategory : Identifiable, Nameable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class InvItemStatus : Identifiable, Nameable, Colorable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Color { get; set; } = string.Empty;
}

public class InvOrderEntity
{
    public int Id { get; set; }

    public DateTime PurchaseDate { get; set; }
    
    public DateTime ReceptionDate { get; set; }
    
    public int StatusId { get; set; }
    [ForeignKey("StatusId")]
    public InvOrderStatus? Status { get; set; }

    public int VendorId { get; set; }
    [ForeignKey("VendorId")]
    public InvVendorEntity? Vendor { get; set; }
    
    public bool IsPrivate { get; set; }
    
    public string Creator { get; set; } = string.Empty;
}

public class InvOrderSubEntity
{
    public int Id { get; set; }
    
    public int ItemId { get; set; }
    [ForeignKey("ItemId")]
    public InvItemEntity? Item { get; set; }
    
    [Required]
    public int ItemCount { get; set; }
    
    [Required]
    public double Amount { get; set; }
    
    public int OrderId { get; set; }
    [ForeignKey("OrderId")]
    public InvOrderEntity? Order { get; set; }
}

public class InvOrderStatus : Identifiable, Nameable, Colorable
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Color { get; set; } = string.Empty;
}

public class InvVendorStats
{
    public int OrderCount { get; set; }
    public double TotalAmount { get; set; }
}

public class InvOrderStats
{
    public int ItemCount { get; set; }
    public double TotalAmount { get; set; }
}