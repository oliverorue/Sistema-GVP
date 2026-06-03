using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;

namespace SistemaGVP.Application.Interfaces;

public interface IUserService
{
    Task<ServiceResult<PagedResult<UserDto>>> GetAllAsync(PaginationFilter filter, int companyId);
    Task<ServiceResult<UserDto>> GetByIdAsync(int id);
    Task<ServiceResult<List<UserDto>>> SearchAsync(string term, int companyId);
    Task<ServiceResult<UserDto>> CreateAsync(UserDto dto);
    Task<ServiceResult<UserDto>> UpdateAsync(UserDto dto);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
