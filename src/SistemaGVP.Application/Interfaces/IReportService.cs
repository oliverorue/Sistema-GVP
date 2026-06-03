using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;

namespace SistemaGVP.Application.Interfaces;

public interface IReportService
{
    Task<ServiceResult<DailySalesSummaryDto>> GetDailySummaryAsync(int companyId, DateTime date);
    Task<ServiceResult<List<TopProductDto>>> GetTopProductsAsync(int companyId, int topCount = 10);
    Task<ServiceResult<List<LowStockProductDto>>> GetLowStockProductsAsync(int companyId);

    // ---- Reportes Avanzados (Sub-fase 2.2) ----
    Task<ServiceResult<List<SalesByPeriodDto>>> GetSalesByPeriodAsync(int companyId, DateTime startDate, DateTime endDate);
    Task<ServiceResult<ProfitMarginDto>> GetProfitMarginAsync(int companyId, DateTime startDate, DateTime endDate);
    Task<ServiceResult<List<InventoryValuationDto>>> GetInventoryValuationAsync(int companyId);
    Task<ServiceResult<byte[]>> ExportReportToExcelAsync<T>(List<T> data, string reportName);
    Task<ServiceResult<byte[]>> ExportReportToPdfAsync<T>(List<T> data, string reportName);
}
