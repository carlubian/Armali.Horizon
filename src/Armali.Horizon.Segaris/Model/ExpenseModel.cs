namespace Armali.Horizon.Segaris.Model;

/// <summary>
/// Agregación mensual de ingresos y gastos para un módulo financiero.
/// Income = suma de importes > 0; Expense = suma de |importes &lt; 0|.
/// </summary>
public class MonthlyAggregate
{
    public int Month { get; set; }
    public double Income { get; set; }
    public double Expense { get; set; }
}

/// <summary>
/// Agregación por nombre de categoría (o vendedor) con el importe total en valor absoluto.
/// </summary>
public class CategoryAggregate
{
    public string Name { get; set; } = string.Empty;
    public double Total { get; set; }
}
