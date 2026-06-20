namespace SistemaGVP.Domain.Entities;

/// <summary>
/// Contador atómico para números de factura por compañía y fecha.
/// Permite generar números de factura secuenciales sin race conditions
/// usando el bloqueo optimista de la base de datos.
/// </summary>
public class InvoiceCounter : BaseEntity
{
    public int CompanyId { get; set; }
    public string DatePrefix { get; set; } = string.Empty; // Ej: "20260509"
    public int LastNumber { get; set; }

    // Navigation property
    public Company Company { get; set; } = null!;
}
