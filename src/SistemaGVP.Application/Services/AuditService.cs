using AutoMapper;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.Domain.Entities;
using SistemaGVP.Domain.Interfaces;

namespace SistemaGVP.Application.Services;

public class AuditService : IAuditService
{
    private readonly IRepository<AuditLog> _auditLogRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IRepository<AuditLog> auditLogRepository,
        IMapper mapper,
        ILogger<AuditService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<AuditLogDto>>> GetAuditLogsAsync(
        int companyId,
        PaginationFilter filter,
        string? actionFilter = null,
        string? entityFilter = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            // Since we're using a generic IRepository<AuditLog>, we'll get all logs
            // and apply filters in memory. In a production scenario, you'd want a
            // specialized repository with queryable support.
            var allLogs = await _auditLogRepository.GetAllAsync();

            var query = allLogs.Where(l => l.CompanyId == companyId).AsQueryable();

            if (!string.IsNullOrEmpty(actionFilter) && actionFilter != "Todas")
                query = query.Where(l => l.Action.ToString() == actionFilter);

            if (!string.IsNullOrEmpty(entityFilter) && entityFilter != "Todas")
                query = query.Where(l => l.EntityName == entityFilter);

            if (startDate.HasValue)
                query = query.Where(l => l.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.CreatedAt <= endDate.Value);

            // Order by newest first
            var orderedQuery = query.OrderByDescending(l => l.CreatedAt);

            var totalCount = orderedQuery.Count();

            var pagedLogs = orderedQuery
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var dtos = _mapper.Map<List<AuditLogDto>>(pagedLogs);

            var result = new PagedResult<AuditLogDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };

            return ServiceResult<PagedResult<AuditLogDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener registros de auditoría para empresa {CompanyId}", companyId);
            return ServiceResult<PagedResult<AuditLogDto>>.Failure("Error al cargar registros de auditoría.");
        }
    }

    public async Task<ServiceResult<List<AuditLogDto>>> GetRecentAuditLogsAsync(int companyId, int count = 50)
    {
        try
        {
            var allLogs = await _auditLogRepository.GetAllAsync();
            var recentLogs = allLogs
                .Where(l => l.CompanyId == companyId)
                .OrderByDescending(l => l.CreatedAt)
                .Take(count)
                .ToList();

            var dtos = _mapper.Map<List<AuditLogDto>>(recentLogs);
            return ServiceResult<List<AuditLogDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener auditorías recientes para empresa {CompanyId}", companyId);
            return ServiceResult<List<AuditLogDto>>.Failure("Error al cargar auditorías recientes.");
        }
    }

    public async Task<ServiceResult<AuditLogDto>> GetAuditLogDetailAsync(int logId)
    {
        try
        {
            var log = await _auditLogRepository.GetByIdAsync(logId);
            if (log == null)
                return ServiceResult<AuditLogDto>.Failure("Registro de auditoría no encontrado.");

            var dto = _mapper.Map<AuditLogDto>(log);
            return ServiceResult<AuditLogDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle de auditoría {LogId}", logId);
            return ServiceResult<AuditLogDto>.Failure("Error al cargar detalle de auditoría.");
        }
    }
}
