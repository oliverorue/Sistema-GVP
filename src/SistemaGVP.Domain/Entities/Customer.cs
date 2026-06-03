namespace SistemaGVP.Domain.Entities;

public class Customer : BaseEntity
{
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? TaxId { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal Balance { get; set; }

    // Navigation properties
    public Company Company { get; set; } = null!;
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}
