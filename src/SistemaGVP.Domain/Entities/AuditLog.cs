using SistemaGVP.Domain.Enums;

namespace SistemaGVP.Domain.Entities;

public class AuditLog : BaseEntity
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public AuditAction Action { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? OldValues { get; set; }  // JSON
    public string? NewValues { get; set; }  // JSON
    public string? IpAddress { get; set; }
    public int CompanyId { get; set; }

    // Navigation
    public Company? Company { get; set; }
}
