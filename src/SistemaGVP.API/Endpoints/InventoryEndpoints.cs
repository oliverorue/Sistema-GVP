using Microsoft.AspNetCore.Mvc;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;

namespace SistemaGVP.API.Endpoints;

public static class InventoryEndpoints
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/inventory").RequireAuthorization();

        group.MapGet("/movements", async (IInventoryService service, ICurrentUserService currentUser) =>
        {
            var result = await service.GetRecentMovementsAsync(currentUser.CompanyId);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapPost("/movements", async ([FromBody] CreateInventoryMovementDto dto, IInventoryService service, ICurrentUserService currentUser) =>
        {
            dto.CompanyId = currentUser.CompanyId;
            dto.UserId = currentUser.UserId;
            var result = await service.AdjustStockAsync(dto);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        }).RequireAuthorization("Admin");

        group.MapGet("/low-stock", async (IInventoryService service, ICurrentUserService currentUser) =>
        {
            var result = await service.GetLowStockProductsAsync(currentUser.CompanyId);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        return app;
    }
}
