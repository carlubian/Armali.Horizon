namespace Armali.Horizon.Contracts.Segaris;

// ─────────────────────────────────────────────────────────────────────────────
// DTOs ligeros para cruzar el bus IO sin arrastrar referencias a Segaris.Model
// ni nav properties de EF. Se mapean uno-a-uno desde las entidades del dominio.
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Catálogo simple identificable por nombre (categorías, ejes, programas, etc.).</summary>
public class SegarisRefDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>Catálogo coloreado (estados, prioridades).</summary>
public class SegarisStatusDto : SegarisRefDto
{
    public string Color { get; set; } = string.Empty;
}

// ── Project ─────────────────────────────────────────────────────────────────

public class ProjectAxisDto : SegarisRefDto
{
    public int ProgramId { get; set; }
}

public class ProjectEntityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int ProgramId { get; set; }
    public int AxisId { get; set; }
    public int StatusId { get; set; }
    public bool IsPrivate { get; set; }
    public string Creator { get; set; } = string.Empty;
}

public class ProjectSubEntityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string File { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int ProjectId { get; set; }
}

public class ProjectRiskElementDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int Probability { get; set; }
    public int Severity { get; set; }
    public int Mitigation { get; set; }
    public int ProjectId { get; set; }
    public int Score { get; set; }
}

public class ProjectBudgetDto
{
    public int Id { get; set; }
    public int Year { get; set; }
    public double Estimated { get; set; }
    public double Actual { get; set; }
    public int ProjectId { get; set; }
    public double SpentPercent { get; set; }
}

// ── Asset ───────────────────────────────────────────────────────────────────

public class AssetEntityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string AssetCode { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int CategoryId { get; set; }
    public int StatusId { get; set; }
    public int? ProjectId { get; set; }
    public bool IsPrivate { get; set; }
    public string Creator { get; set; } = string.Empty;
}

// ── Capex ───────────────────────────────────────────────────────────────────

public class CapexEntityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public double Amount { get; set; }
    public int CategoryId { get; set; }
    public int StatusId { get; set; }
    public int? ProjectId { get; set; }
    public bool IsPrivate { get; set; }
    public string Creator { get; set; } = string.Empty;
}

// ── Opex ────────────────────────────────────────────────────────────────────

public class OpexEntityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int StatusId { get; set; }
    public int? ProjectId { get; set; }
    public bool IsPrivate { get; set; }
    public string Creator { get; set; } = string.Empty;
}

public class OpexSubEntityDto
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public double Amount { get; set; }
    public int ContractId { get; set; }
}

public class OpexStatsDto
{
    public int SubEntityCount { get; set; }
    public double TotalAmount { get; set; }
}

// ── Travel ──────────────────────────────────────────────────────────────────

public class TravelEntityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int StatusId { get; set; }
    public int CostCenterId { get; set; }
    public string Destination { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Pax { get; set; }
    public int? ProjectId { get; set; }
    public bool IsPrivate { get; set; }
    public string Creator { get; set; } = string.Empty;
}

public class TravelSubEntityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public DateTime Date { get; set; }
    public double Amount { get; set; }
    public int TravelId { get; set; }
}

// ── Maintenance ─────────────────────────────────────────────────────────────

public class MaintEntityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Details { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int StatusId { get; set; }
    public int? AssetId { get; set; }
    public bool IsPrivate { get; set; }
    public string Creator { get; set; } = string.Empty;
}

// ── Inventory ───────────────────────────────────────────────────────────────

public class InvVendorEntityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StatusId { get; set; }
    public bool IsPrivate { get; set; }
    public string Creator { get; set; } = string.Empty;
}

public class InvItemEntityDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int MinStock { get; set; }
    public int CategoryId { get; set; }
    public int StatusId { get; set; }
    public int VendorId { get; set; }
    public bool IsPrivate { get; set; }
    public string Creator { get; set; } = string.Empty;
}

public class InvOrderEntityDto
{
    public int Id { get; set; }
    public DateTime PurchaseDate { get; set; }
    public DateTime ReceptionDate { get; set; }
    public int StatusId { get; set; }
    public int VendorId { get; set; }
    public bool IsPrivate { get; set; }
    public string Creator { get; set; } = string.Empty;
}

public class InvOrderSubEntityDto
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public int ItemCount { get; set; }
    public double Amount { get; set; }
    public int OrderId { get; set; }
    public int? ProjectId { get; set; }
}

public class InvVendorStatsDto
{
    public int OrderCount { get; set; }
    public double TotalAmount { get; set; }
}

public class InvOrderStatsDto
{
    public int ItemCount { get; set; }
    public double TotalAmount { get; set; }
}

public class InvItemPriceHistoryDto
{
    public int Id { get; set; }
    public DateTime PurchaseDate { get; set; }
    public int VendorId { get; set; }
    public int ItemCount { get; set; }
    public double TotalAmount { get; set; }
    public double UnitPrice { get; set; }
}

