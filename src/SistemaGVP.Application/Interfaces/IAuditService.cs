using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;

namespace SistemaGVP.Application.Interfaces;

public interface IAuditService
{
    Task<ServiceResult<PagedResult<AuditLogDto>>> GetAuditLogsAsync(
        int companyId,
        PaginationFilter filter,
        string? actionFilter = null,
        string? entityFilter = null,
        DateTime? startDate = null,
        DateTime? endDate = null);
    Task<ServiceResult<List<AuditLogDto>>> GetRecentAuditLogsAsync(int companyId, int count = 50);
    Task<ServiceResult<AuditLogDto>> GetAuditLogDetailAsync(int logId);
}
