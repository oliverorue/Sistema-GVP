using Microsoft.AspNetCore.Mvc;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;

namespace SistemaGVP.API.Endpoints;

public static class SettingsEndpoints
{
    public static IEndpointRouteBuilder MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/settings").RequireAuthorization();

        group.MapGet("/company", async (ISettingsService service, ICurrentUserService currentUser) =>
        {
            var result = await service.GetCompanyAsync(currentUser.CompanyId);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapPut("/company", async ([FromBody] CompanyDto dto, ISettingsService service, ICurrentUserService currentUser) =>
        {
            dto.Id = currentUser.CompanyId;
            var result = await service.UpdateCompanyAsync(dto);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        }).RequireAuthorization("Admin");

        return app;
    }
}
