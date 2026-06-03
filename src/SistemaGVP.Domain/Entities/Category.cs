namespace SistemaGVP.Domain.Entities;

public class Category : BaseEntity
{
    public int CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public Company Company { get; set; } = null!;
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
