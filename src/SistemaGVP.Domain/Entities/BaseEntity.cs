namespace SistemaGVP.Domain.Entities;

/// <summary>
/// Entidad base con propiedades comunes para todas las entidades del sistema.
/// </summary>
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}
