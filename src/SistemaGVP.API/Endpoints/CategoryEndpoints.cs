using Microsoft.AspNetCore.Mvc;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;

namespace SistemaGVP.API.Endpoints;

public static class CategoryEndpoints
{
    public static IEndpointRouteBuilder MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/categories").RequireAuthorization();

        group.MapGet("/", async (ICategoryService service, ICurrentUserService currentUser) =>
        {
            var result = await service.GetAllAsync(currentUser.CompanyId);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapPost("/", async ([FromBody] CategoryDto dto, ICategoryService service, ICurrentUserService currentUser) =>
        {
            dto.CompanyId = currentUser.CompanyId;
            var result = await service.CreateAsync(dto);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        }).RequireAuthorization("Admin");

        group.MapPut("/{id:int}", async (int id, [FromBody] CategoryDto dto, ICategoryService service) =>
        {
            dto.Id = id;
            var result = await service.UpdateAsync(dto);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        }).RequireAuthorization("Admin");

        group.MapDelete("/{id:int}", async (int id, ICategoryService service) =>
        {
            var result = await service.DeleteAsync(id);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        }).RequireAuthorization("Admin");

        return app;
    }
}
