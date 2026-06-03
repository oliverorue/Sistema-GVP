using SistemaGVP.Domain.Entities;

namespace SistemaGVP.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetByBarcodeAsync(string barcode, int companyId);
    Task<Product?> GetBySkuAsync(string sku, int companyId);
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, string? searchTerm, int companyId);
    Task<IReadOnlyList<Product>> GetLowStockAsync(int companyId, decimal threshold);
    Task<IReadOnlyList<Product>> GetByCompanyAsync(int companyId);
}
