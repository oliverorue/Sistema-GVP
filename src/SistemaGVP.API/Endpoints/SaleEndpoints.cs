using Microsoft.AspNetCore.Mvc;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;

namespace SistemaGVP.API.Endpoints;

public static class SaleEndpoints
{
    public static IEndpointRouteBuilder MapSaleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sales").RequireAuthorization();

        group.MapPost("/", async ([FromBody] CreateSaleDto dto, ISaleService service, ICurrentUserService currentUser) =>
        {
            dto.CompanyId = currentUser.CompanyId;
            dto.UserId = currentUser.UserId;
            var result = await service.CreateSaleAsync(dto);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapGet("/", async (
            int? page,
            int? pageSize,
            string? searchTerm,
            DateTime? startDate,
            DateTime? endDate,
            string? paymentMethod,
            ISaleService service,
            ICurrentUserService currentUser) =>
        {
            var result = await service.GetSalesHistoryAsync(
                currentUser.CompanyId,
                searchTerm,
                startDate,
                endDate,
                paymentMethod,
                page ?? 1,
                pageSize ?? 25);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapGet("/{id:int}", async (int id, ISaleService service, ICurrentUserService currentUser) =>
        {
            var result = await service.GetByIdAsync(id);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapPut("/{id:int}/void", async (int id, [FromBody] VoidSaleRequest request, ISaleService service, ICurrentUserService currentUser) =>
        {
            var result = await service.CancelSaleAsync(id, currentUser.CompanyId, currentUser.UserId, request.Reason);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        }).RequireAuthorization("Admin");

        group.MapPost("/hold", async ([FromBody] CreateSaleDto dto, ISaleService service, ICurrentUserService currentUser) =>
        {
            dto.CompanyId = currentUser.CompanyId;
            dto.UserId = currentUser.UserId;
            var result = await service.HoldSaleAsync(dto, currentUser.CompanyId, currentUser.UserId);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapGet("/held", async (ISaleService service, ICurrentUserService currentUser) =>
        {
            var result = await service.GetHeldSalesAsync(currentUser.CompanyId);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapPost("/{id:int}/resume", async (int id, ISaleService service, ICurrentUserService currentUser) =>
        {
            var result = await service.ResumeSaleAsync(id, currentUser.CompanyId);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        return app;
    }
}

public class VoidSaleRequest
{
    public string Reason { get; set; } = string.Empty;
}
