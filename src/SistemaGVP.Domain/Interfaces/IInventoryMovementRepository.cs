using SistemaGVP.Domain.Entities;

namespace SistemaGVP.Domain.Interfaces;

public interface IInventoryMovementRepository : IRepository<InventoryMovement>
{
    Task<IReadOnlyList<InventoryMovement>> GetByProductIdAsync(int productId, int companyId);
    Task<IReadOnlyList<InventoryMovement>> GetByDateRangeAsync(int companyId, DateTime from, DateTime to);
}
