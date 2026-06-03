using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;

namespace SistemaGVP.Application.Interfaces;

public interface ICategoryService
{
    Task<ServiceResult<List<CategoryDto>>> GetAllAsync(int companyId);
    Task<ServiceResult<CategoryDto>> CreateAsync(CategoryDto dto);
    Task<ServiceResult<CategoryDto>> UpdateAsync(CategoryDto dto);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
