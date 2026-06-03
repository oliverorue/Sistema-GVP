using SistemaGVP.Domain.Enums;

namespace SistemaGVP.Domain.Entities;

public class User : BaseEntity
{
    public int CompanyId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Cashier;
    public DateTime? LastLogin { get; set; }
    public bool MustChangePassword { get; set; } = true;

    // Navigation properties
    public Company Company { get; set; } = null!;
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
    public ICollection<InventoryMovement> InventoryMovements { get; set; } = new List<InventoryMovement>();
}
