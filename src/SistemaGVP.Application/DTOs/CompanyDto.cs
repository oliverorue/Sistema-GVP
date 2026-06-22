namespace SistemaGVP.Application.DTOs;

public class CompanyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TaxId { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public decimal TaxRate { get; set; } = 0.10m;
    public bool IvaIncluido { get; set; } = true;
    public string Currency { get; set; } = "Gs.";
    public int LowStockThreshold { get; set; } = 10;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
}
