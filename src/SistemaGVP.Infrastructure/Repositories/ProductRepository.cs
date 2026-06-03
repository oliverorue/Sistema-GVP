using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaGVP.Domain.Entities;
using SistemaGVP.Domain.Interfaces;
using SistemaGVP.Infrastructure.Data;

namespace SistemaGVP.Infrastructure.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    private readonly ILogger<ProductRepository>? _logger;

    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    public ProductRepository(AppDbContext context, ILogger<ProductRepository>? logger) : base(context)
    {
        _logger = logger;
    }

    public async Task<Product?> GetByBarcodeAsync(string barcode, int companyId)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Barcode == barcode
                                   && p.CompanyId == companyId
                                   && p.IsActive);
    }

    public async Task<Product?> GetBySkuAsync(string sku, int companyId)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.Sku == sku
                                   && p.CompanyId == companyId
                                   && p.IsActive);
    }

    public async Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize, string? searchTerm, int companyId)
    {
        var query = _dbSet
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.IsActive && p.CompanyId == companyId);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var search = $"%{searchTerm.ToLower()}%";
            query = query.Where(p =>
                EF.Functions.Like(p.Name.ToLower(), search) ||
                EF.Functions.Like(p.Barcode.ToLower(), search) ||
                EF.Functions.Like(p.Sku.ToLower(), search));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<Product>> GetLowStockAsync(int companyId, decimal threshold)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.CompanyId == companyId
                     && p.IsActive
                     && p.CurrentStock <= threshold)
            .OrderBy(p => (double)p.CurrentStock)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Product>> SearchByBarcodePartialAsync(string partialBarcode, int companyId)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.CompanyId == companyId
                     && p.IsActive
                     && p.Barcode.Contains(partialBarcode))
            .OrderBy(p => p.Barcode)
            .Take(10)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Product>> GetByCompanyAsync(int companyId)
    {
        return await _dbSet
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.CompanyId == companyId)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }
}
