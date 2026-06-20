using Microsoft.AspNetCore.Mvc;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;

namespace SistemaGVP.API.Endpoints;

public static class SupplierEndpoints
{
    public static IEndpointRouteBuilder MapSupplierEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/suppliers").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] PaginationFilter filter, ISupplierService service, ICurrentUserService currentUser) =>
        {
            var result = await service.GetAllAsync(filter, currentUser.CompanyId);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapPost("/", async ([FromBody] SupplierDto dto, ISupplierService service, ICurrentUserService currentUser) =>
        {
            dto.CompanyId = currentUser.CompanyId;
            var result = await service.CreateAsync(dto);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        }).RequireAuthorization("Admin");

        group.MapPut("/{id:int}", async (int id, [FromBody] SupplierDto dto, ISupplierService service) =>
        {
            dto.Id = id;
            var result = await service.UpdateAsync(dto);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        }).RequireAuthorization("Admin");

        group.MapDelete("/{id:int}", async (int id, ISupplierService service) =>
        {
            var result = await service.DeleteAsync(id);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        }).RequireAuthorization("Admin");

        group.MapGet("/search", async (string q, ISupplierService service, ICurrentUserService currentUser) =>
        {
            var result = await service.SearchAsync(q, currentUser.CompanyId);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        return app;
    }
}
