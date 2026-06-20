using Microsoft.AspNetCore.Mvc;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;

namespace SistemaGVP.API.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").RequireAuthorization().RequireAuthorization("Admin");

        group.MapGet("/", async ([AsParameters] PaginationFilter filter, IUserService service, ICurrentUserService currentUser) =>
        {
            var result = await service.GetAllAsync(filter, currentUser.CompanyId);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapPost("/", async ([FromBody] UserDto dto, IUserService service, ICurrentUserService currentUser) =>
        {
            dto.CompanyId = currentUser.CompanyId;
            var result = await service.CreateAsync(dto);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapPut("/{id:int}", async (int id, [FromBody] UserDto dto, IUserService service) =>
        {
            dto.Id = id;
            var result = await service.UpdateAsync(dto);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapDelete("/{id:int}", async (int id, IUserService service) =>
        {
            var result = await service.DeleteAsync(id);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapPost("/{id:int}/reset-password", async (int id, IUserService service) =>
        {
            var userResult = await service.GetByIdAsync(id);
            if (!userResult.IsSuccess)
                return Results.Ok(new { isSuccess = false, data = (object?)null, message = userResult.Message, errors = userResult.Errors });

            var user = userResult.Data!;
            user.MustChangePassword = true;
            var result = await service.UpdateAsync(user);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = "Contrasena restablecida. El usuario debera cambiarla al iniciar sesion.", errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        return app;
    }
}
