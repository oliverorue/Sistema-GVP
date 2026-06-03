namespace SistemaGVP.Domain.Entities;

public class Supplier : BaseEntity
{
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxId { get; set; }

    // Navigation properties
    public Company Company { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
