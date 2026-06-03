using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;

namespace SistemaGVP.Application.Interfaces;

public interface ICustomerService
{
    Task<ServiceResult<List<CustomerDto>>> SearchAsync(string searchTerm, int companyId);
    Task<ServiceResult<CustomerDto>> GetByIdAsync(int id);
    Task<ServiceResult<CustomerDto>> CreateAsync(CustomerDto dto);
    Task<ServiceResult<CustomerDto>> UpdateAsync(CustomerDto dto);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
