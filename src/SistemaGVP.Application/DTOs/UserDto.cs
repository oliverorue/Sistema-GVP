namespace SistemaGVP.Application.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public int CompanyId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? Password { get; set; } // Solo para creación/actualización
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Cashier";
    public bool IsActive { get; set; } = true;
    public bool MustChangePassword { get; set; }
    public DateTime? LastLogin { get; set; }
}

public class LoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public int CompanyId { get; set; }
}
