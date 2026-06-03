using SistemaGVP.Domain.Enums;

namespace SistemaGVP.Domain.Entities;

/// <summary>
/// Cabecera de venta. Principal documento transaccional del sistema.
/// </summary>
public class Sale : BaseEntity
{
    public int CompanyId { get; set; }
    public int UserId { get; set; }
    public int? CustomerId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public SaleStatus Status { get; set; } = SaleStatus.Completed;
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public decimal CashAmount { get; set; }
    public decimal ChangeAmount { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public Company Company { get; set; } = null!;
    public User User { get; set; } = null!;
    public Customer? Customer { get; set; }
    public ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
    public ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
}
