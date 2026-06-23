using Microsoft.AspNetCore.Mvc;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.Interfaces;

namespace SistemaGVP.API.Endpoints;

public static class AuditEndpoints
{
    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/audit").RequireAuthorization().RequireAuthorization("Admin");

        group.MapGet("/logs", async (
            [FromQuery] int pageNumber,
            [FromQuery] int pageSize,
            [FromQuery] string? actionFilter,
            [FromQuery] string? entityFilter,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            IAuditService service,
            ICurrentUserService currentUser) =>
        {
            var filter = new PaginationFilter(pageNumber, pageSize);
            var from = startDate?.Date ?? DateTime.UtcNow.Date.AddMonths(-1);
            var to = (endDate?.Date ?? DateTime.UtcNow.Date).AddDays(1).AddSeconds(-1);
            var result = await service.GetAuditLogsAsync(currentUser.CompanyId, filter, actionFilter, entityFilter, from, to);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapGet("/logs/{entityName}/{entityId:int}", async (
            string entityName,
            int entityId,
            IAuditService service,
            ICurrentUserService currentUser) =>
        {
            var result = await service.GetRecentAuditLogsAsync(currentUser.CompanyId);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data?.Where(l => l.EntityName == entityName && l.EntityId == entityId), message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        return app;
    }
}
