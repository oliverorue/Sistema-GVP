using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;

namespace SistemaGVP.Application.Interfaces;

public interface ISettingsService
{
    Task<ServiceResult<CompanyDto>> GetCompanyAsync(int companyId);
    Task<ServiceResult<CompanyDto>> UpdateCompanyAsync(CompanyDto dto);
}
