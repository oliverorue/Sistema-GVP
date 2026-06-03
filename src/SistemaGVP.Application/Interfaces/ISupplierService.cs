using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;

namespace SistemaGVP.Application.Interfaces;

public interface ISupplierService
{
    Task<ServiceResult<PagedResult<SupplierDto>>> GetAllAsync(PaginationFilter filter, int companyId);
    Task<ServiceResult<SupplierDto>> GetByIdAsync(int id);
    Task<ServiceResult<List<SupplierDto>>> SearchAsync(string term, int companyId);
    Task<ServiceResult<SupplierDto>> CreateAsync(SupplierDto dto);
    Task<ServiceResult<SupplierDto>> UpdateAsync(SupplierDto dto);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
