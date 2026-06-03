using SistemaGVP.Domain.Enums;

namespace SistemaGVP.Domain.Entities;

/// <summary>
/// Movimiento de inventario. Corazón del control de stock.
/// Cada entrada/salida/ajuste queda registrado con saldo anterior y posterior.
/// Esto permite auditoría completa y reconstrucción del historial.
/// </summary>
public class InventoryMovement : BaseEntity
{
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public int CompanyId { get; set; }
    public int? RelatedSaleId { get; set; }
    public MovementType Type { get; set; }
    public decimal Quantity { get; set; }
    public decimal StockBefore { get; set; }
    public decimal StockAfter { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }

    // Navigation properties
    public Product Product { get; set; } = null!;
    public User User { get; set; } = null!;
    public Company Company { get; set; } = null!;
    public Sale? RelatedSale { get; set; }
}
