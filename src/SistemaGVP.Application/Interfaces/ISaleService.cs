using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;

namespace SistemaGVP.Application.Interfaces;

public interface ISaleService
{
    Task<ServiceResult<SaleDto>> GetByIdAsync(int id);
    Task<ServiceResult<SaleDto>> CreateSaleAsync(CreateSaleDto dto);
    Task<ServiceResult<bool>> CancelSaleAsync(int saleId, int userId);
    Task<ServiceResult<List<SaleDto>>> GetSalesByDateAsync(int companyId, DateTime from, DateTime to);

    // ---- Hold / Resume (Ventas en Espera) ----
    Task<ServiceResult<HeldSaleDto>> HoldSaleAsync(CreateSaleDto saleDto, int companyId, int userId);
    Task<ServiceResult<HeldSaleDto?>> ResumeSaleAsync(int heldSaleId, int companyId);
    Task<ServiceResult<List<HeldSaleDto>>> GetHeldSalesAsync(int companyId);
    Task<ServiceResult<bool>> RemoveHeldSaleAsync(int heldSaleId, int companyId);

    // ---- Historial de Ventas + Anulación ----
    Task<ServiceResult<PagedResult<SaleHistoryDto>>> GetSalesHistoryAsync(int companyId, string? searchTerm, DateTime? startDate, DateTime? endDate, string? paymentMethod, int page, int pageSize);
    Task<ServiceResult<SaleDetailDto>> GetSaleDetailAsync(int saleId, int companyId);
    Task<ServiceResult<bool>> CancelSaleAsync(int saleId, int companyId, int userId, string reason);
}
