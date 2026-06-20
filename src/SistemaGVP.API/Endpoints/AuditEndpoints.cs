using SistemaGVP.Application.Common;
using SistemaGVP.Application.Interfaces;

namespace SistemaGVP.API.Endpoints;

public static class AuditEndpoints
{
    public static IEndpointRouteBuilder MapAuditEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/audit").RequireAuthorization().RequireAuthorization("Admin");

        group.MapGet("/logs", async (
            [AsParameters] PaginationFilter filter,
            string? actionFilter,
            string? entityFilter,
            DateTime? startDate,
            DateTime? endDate,
            IAuditService service,
            ICurrentUserService currentUser) =>
        {
            var result = await service.GetAuditLogsAsync(currentUser.CompanyId, filter, actionFilter, entityFilter, startDate, endDate);
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
