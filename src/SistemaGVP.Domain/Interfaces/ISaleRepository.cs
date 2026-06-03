using SistemaGVP.Domain.Entities;

namespace SistemaGVP.Domain.Interfaces;

public interface ISaleRepository : IRepository<Sale>
{
    Task<Sale?> GetByInvoiceNumberAsync(string invoiceNumber, int companyId);
    Task<IReadOnlyList<Sale>> GetByDateRangeAsync(int companyId, DateTime from, DateTime to);
    Task<Sale?> GetWithDetailsAsync(int saleId);
    Task<IReadOnlyList<Sale>> GetHeldSalesAsync(int companyId);
    Task<(IReadOnlyList<Sale> Items, int TotalCount)> GetSalesWithFilterAsync(
        int companyId,
        string? searchTerm,
        DateTime? startDate,
        DateTime? endDate,
        string? paymentMethod,
        int page,
        int pageSize);

    /// <summary>
    /// Ejecuta una acción dentro de una transacción Serializable con ExecutionStrategy.
    /// Útil para operaciones críticas como ventas donde se necesita evitar race conditions.
    /// </summary>
    Task<T> ExecuteWithSerializableTransactionAsync<T>(Func<Task<T>> action);
}
