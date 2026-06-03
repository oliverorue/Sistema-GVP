namespace SistemaGVP.Domain.Entities;

/// <summary>
/// Detalle de venta. Guarda snapshots del producto al momento de la venta
/// para preservar el histórico aunque el producto cambie después.
/// </summary>
public class SaleDetail : BaseEntity
{
    public int SaleId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;  // Snapshot
    public string Barcode { get; set; } = string.Empty;       // Snapshot
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Cost { get; set; }           // Snapshot del costo (para calcular margen histórico)
    public decimal Discount { get; set; }
    public decimal Subtotal { get; set; }

    // Navigation properties
    public Sale Sale { get; set; } = null!;
    public Product Product { get; set; } = null!;
}
