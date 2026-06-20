using Microsoft.AspNetCore.Mvc;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;

namespace SistemaGVP.API.Endpoints;

public static class ProductEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products").RequireAuthorization();

        group.MapGet("/", async ([AsParameters] PaginationFilter filter, IProductService service, ICurrentUserService currentUser) =>
        {
            var result = await service.GetAllAsync(filter, currentUser.CompanyId);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapGet("/{id:int}", async (int id, IProductService service) =>
        {
            var result = await service.GetByIdAsync(id);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapPost("/", async ([FromBody] ProductDto dto, IProductService service) =>
        {
            var result = await service.CreateAsync(dto);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        }).RequireAuthorization("Admin");

        group.MapPut("/{id:int}", async (int id, [FromBody] ProductDto dto, IProductService service) =>
        {
            dto.Id = id;
            var result = await service.UpdateAsync(dto);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        }).RequireAuthorization("Admin");

        group.MapDelete("/{id:int}", async (int id, IProductService service) =>
        {
            var result = await service.DeleteAsync(id);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        }).RequireAuthorization("Admin");

        group.MapGet("/search", async (string q, IProductService service, ICurrentUserService currentUser) =>
        {
            var filter = new PaginationFilter(1, 100, q);
            var result = await service.GetAllAsync(filter, currentUser.CompanyId);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapGet("/barcode/{code}", async (string code, IProductService service, ICurrentUserService currentUser) =>
        {
            var result = await service.GetByBarcodeAsync(code, currentUser.CompanyId);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapGet("/low-stock", async (IProductService service, ICurrentUserService currentUser) =>
        {
            var filter = new PaginationFilter(1, 1000);
            var result = await service.GetAllAsync(filter, currentUser.CompanyId);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        return app;
    }
}
