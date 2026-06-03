using SistemaGVP.Domain.Entities;

namespace SistemaGVP.Domain.Interfaces;

/// <summary>
/// Repositorio para el manejo atómico de contadores de facturas.
/// Garantiza que no se generen números de factura duplicados
/// incluso bajo concurrencia.
/// </summary>
public interface IInvoiceCounterRepository
{
    /// <summary>
    /// Obtiene o crea el contador para una compañía y fecha específicas,
    /// e incrementa el número secuencial de forma atómica.
    /// </summary>
    /// <param name="companyId">ID de la compañía</param>
    /// <param name="datePrefix">Prefijo de fecha (yyyyMMdd)</param>
    /// <returns>El siguiente número secuencial</returns>
    Task<int> GetNextNumberAsync(int companyId, string datePrefix);
}
