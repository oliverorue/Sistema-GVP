using Microsoft.AspNetCore.Mvc;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;

namespace SistemaGVP.API.Endpoints;

public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers").RequireAuthorization();

        group.MapGet("/", async (ICustomerService service, ICurrentUserService currentUser) =>
        {
            var result = await service.SearchAsync("", currentUser.CompanyId);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapGet("/{id:int}", async (int id, ICustomerService service) =>
        {
            var result = await service.GetByIdAsync(id);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapPost("/", async ([FromBody] CustomerDto dto, ICustomerService service, ICurrentUserService currentUser) =>
        {
            dto.CompanyId = currentUser.CompanyId;
            var result = await service.CreateAsync(dto);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        }).RequireAuthorization("Admin");

        group.MapPut("/{id:int}", async (int id, [FromBody] CustomerDto dto, ICustomerService service) =>
        {
            dto.Id = id;
            var result = await service.UpdateAsync(dto);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        }).RequireAuthorization("Admin");

        group.MapDelete("/{id:int}", async (int id, ICustomerService service) =>
        {
            var result = await service.DeleteAsync(id);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        }).RequireAuthorization("Admin");

        group.MapGet("/search", async (string q, ICustomerService service, ICurrentUserService currentUser) =>
        {
            var result = await service.SearchAsync(q, currentUser.CompanyId);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        group.MapPost("/{id:int}/payment", async (int id, [FromBody] RegisterPaymentRequest request, ICustomerService service) =>
        {
            var result = await service.RegisterPaymentAsync(id, request.Amount, request.Notes);
            return result.IsSuccess
                ? Results.Ok(new { isSuccess = true, data = result.Data, message = result.Message, errors = result.Errors })
                : Results.Ok(new { isSuccess = false, data = (object?)null, message = result.Message, errors = result.Errors });
        });

        return app;
    }
}

public class RegisterPaymentRequest
{
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}
