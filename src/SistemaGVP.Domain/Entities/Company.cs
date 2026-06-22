namespace SistemaGVP.Domain.Entities;

public class Company : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string? LogoUrl { get; set; }
    public decimal TaxRate { get; set; } = 0.10m;
    public bool IvaIncluido { get; set; } = true;
    public string Currency { get; set; } = "Gs.";
    public int LowStockThreshold { get; set; } = 10;

    // Navigation properties
    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
    public ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();
}
