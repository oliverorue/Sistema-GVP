using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;

namespace SistemaGVP.Application.Interfaces;

public interface IInventoryService
{
    Task<ServiceResult<List<InventoryMovementDto>>> GetMovementsByProductAsync(int productId, int companyId);
    Task<ServiceResult<InventoryMovementDto>> AdjustStockAsync(CreateInventoryMovementDto dto);
    Task<ServiceResult<List<ProductDto>>> GetLowStockProductsAsync(int companyId);
    Task<ServiceResult<int>> GetLowStockCountAsync(int companyId);
    Task<ServiceResult<List<InventoryMovementDto>>> GetRecentMovementsAsync(int companyId, int count = 50);
}
