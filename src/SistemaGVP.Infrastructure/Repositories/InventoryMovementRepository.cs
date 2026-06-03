using Microsoft.EntityFrameworkCore;
using SistemaGVP.Domain.Entities;
using SistemaGVP.Domain.Interfaces;
using SistemaGVP.Infrastructure.Data;

namespace SistemaGVP.Infrastructure.Repositories;

public class InventoryMovementRepository : BaseRepository<InventoryMovement>, IInventoryMovementRepository
{
    public InventoryMovementRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<InventoryMovement>> GetByProductIdAsync(int productId, int companyId)
    {
        return await _dbSet
            .Include(m => m.User)
            .Include(m => m.RelatedSale)
            .Where(m => m.ProductId == productId && m.CompanyId == companyId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<InventoryMovement>> GetByDateRangeAsync(int companyId, DateTime from, DateTime to)
    {
        return await _dbSet
            .Include(m => m.Product)
            .Include(m => m.User)
            .Where(m => m.CompanyId == companyId
                     && m.CreatedAt >= from
                     && m.CreatedAt <= to)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<decimal> GetCurrentStockAsync(int productId)
    {
        var lastMovement = await _dbSet
            .Where(m => m.ProductId == productId)
            .OrderByDescending(m => m.Id)
            .FirstOrDefaultAsync();

        return lastMovement?.StockAfter ?? 0;
    }
}
