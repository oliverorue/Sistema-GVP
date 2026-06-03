namespace SistemaGVP.Domain.Entities;

/// <summary>
/// Producto del catálogo. Es el corazón del sistema.
/// CurrentStock se mantiene sincronizado mediante InventoryMovements.
/// </summary>
public class Product : BaseEntity
{
    public int CategoryId { get; set; }
    public int? SupplierId { get; set; }
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
    public decimal MinStock { get; set; }
    public decimal CurrentStock { get; set; }
    public string Unit { get; set; } = "pz";
    public string? ImagePath { get; set; }

    // Navigation properties
    public Category Category { get; set; } = null!;
    public Supplier? Supplier { get; set; }
    public Company Company { get; set; } = null!;
    public ICollection<SaleDetail> SaleDetails { get; set; } = new List<SaleDetail>();
    public ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
}
