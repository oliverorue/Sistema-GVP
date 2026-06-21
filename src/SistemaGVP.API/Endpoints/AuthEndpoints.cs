using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using SistemaGVP.API.Middleware;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.Domain.Interfaces;

namespace SistemaGVP.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/login", async (
            [FromBody] LoginDto loginDto,
            IAuthService authService,
            JwtTokenService tokenService) =>
        {
            var result = await authService.LoginAsync(loginDto);

            if (!result.IsSuccess)
                return Results.Ok(new
                {
                    isSuccess = false,
                    data = (object?)null,
                    message = result.Message,
                    errors = result.Errors
                });

            var user = result.Data!;
            var (accessToken, expiresAt) = tokenService.GenerateAccessToken(user);
            var refreshToken = tokenService.GenerateRefreshToken();

            var response = new
            {
                isSuccess = true,
                data = new
                {
                    token = accessToken,
                    refreshToken,
                    expiresAt,
                    requiresPasswordChange = result.RequiresPasswordChange,
                    user = new
                    {
                        user.Id,
                        user.Username,
                        user.FullName,
                        user.Email,
                        user.Role,
                        user.CompanyId,
                        user.MustChangePassword
                    }
                },
                message = result.Message
            };

            return Results.Ok(response);
        });

        group.MapPost("/logout", () =>
        {
            return Results.Ok(new
            {
                isSuccess = true,
                data = (object?)null,
                message = "Sesion cerrada exitosamente."
            });
        });

        group.MapGet("/me", (ICurrentUserService currentUser) =>
        {
            return Results.Ok(new
            {
                isSuccess = true,
                data = new
                {
                    userId = currentUser.UserId,
                    userName = currentUser.UserName,
                    companyId = currentUser.CompanyId,
                    isAuthenticated = currentUser.IsAuthenticated,
                    isAdmin = currentUser.IsAdmin
                },
                message = ""
            });
        }).RequireAuthorization();

        group.MapPost("/change-password", async (
            [FromBody] ChangePasswordRequest request,
            IUserService userService,
            ICurrentUserService currentUser,
            IUserRepository userRepository,
            IMapper mapper) =>
        {
            // Fetch the current user to get all required fields
            var user = await userRepository.GetByIdAsync(currentUser.UserId);
            if (user == null)
                return Results.Ok(new
                {
                    isSuccess = false,
                    data = (object?)null,
                    message = "Usuario no encontrado.",
                    errors = Array.Empty<string>()
                });

            // Create a complete UserDto from the existing user
            var userDto = mapper.Map<UserDto>(user);
            userDto.Password = request.NewPassword;
            userDto.MustChangePassword = false;

            var result = await userService.UpdateAsync(userDto);

            return Results.Ok(new
            {
                isSuccess = result.IsSuccess,
                data = result.Data,
                message = result.Message,
                errors = result.Errors
            });
        }).RequireAuthorization();

        return app;
    }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
