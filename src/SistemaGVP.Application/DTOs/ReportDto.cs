namespace SistemaGVP.Application.DTOs;

public class DailySalesSummaryDto
{
    public int TotalSales { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalTax { get; set; }
    public int TotalItems { get; set; }
    public decimal AverageTicket { get; set; }
}

public class TopProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class LowStockProductDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal MinStock { get; set; }
    public decimal Difference => CurrentStock - MinStock;
}

// ========================================================================
// DTOs para Reportes Avanzados (Sub-fase 2.2)
// ========================================================================

public class SalesByPeriodDto
{
    public DateTime Date { get; set; }
    public int TotalSales { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
}

public class ProfitMarginDto
{
    public string Period { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal Profit { get; set; }
    public decimal Margin { get; set; }
}

public class InventoryValuationDto
{
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal CurrentStock { get; set; }
    public decimal UnitCost { get; set; }
    public decimal TotalValue { get; set; }
}
