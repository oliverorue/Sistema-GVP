using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;

namespace SistemaGVP.Application.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<UserDto>> LoginAsync(LoginDto loginDto);
    void Logout();
    UserDto? CurrentUser { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
}
