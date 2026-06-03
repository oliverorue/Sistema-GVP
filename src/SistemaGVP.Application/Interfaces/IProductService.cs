using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;

namespace SistemaGVP.Application.Interfaces;

public interface IProductService
{
    Task<ServiceResult<ProductDto>> GetByIdAsync(int id);
    Task<ServiceResult<ProductDto>> GetByBarcodeAsync(string barcode, int companyId);
    Task<ServiceResult<PagedResult<ProductDto>>> GetAllAsync(PaginationFilter filter, int companyId);
    Task<ServiceResult<ProductDto>> CreateAsync(ProductDto dto);
    Task<ServiceResult<ProductDto>> UpdateAsync(ProductDto dto);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
