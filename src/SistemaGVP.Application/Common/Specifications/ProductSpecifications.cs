using SistemaGVP.Domain.Entities;

namespace SistemaGVP.Application.Common.Specifications;

public class ActiveProductsByCompanySpec : BaseSpecification<Product>
{
    public ActiveProductsByCompanySpec(int companyId, string? searchTerm = null, int? pageNumber = null, int? pageSize = null)
    {
        AddCriteria(p => p.CompanyId == companyId && p.IsActive);
        AddInclude(p => p.Category!);
        AddInclude(p => p.Supplier!);
        SetAsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            AddCriteria(p =>
                p.Name.ToLower().Contains(term) ||
                p.Barcode.ToLower().Contains(term) ||
                p.Sku.ToLower().Contains(term));
        }

        if (pageNumber.HasValue && pageSize.HasValue)
        {
            ApplyPaging((pageNumber.Value - 1) * pageSize.Value, pageSize.Value);
        }

        ApplyOrderBy(p => p.Name);
    }
}

public class ProductByBarcodeSpec : BaseSpecification<Product>
{
    public ProductByBarcodeSpec(string barcode, int companyId)
    {
        AddCriteria(p => p.Barcode == barcode && p.CompanyId == companyId && p.IsActive);
        AddInclude(p => p.Category!);
        AddInclude(p => p.Supplier!);
        SetAsNoTracking();
    }
}

public class ProductBySkuSpec : BaseSpecification<Product>
{
    public ProductBySkuSpec(string sku, int companyId)
    {
        AddCriteria(p => p.Sku == sku && p.CompanyId == companyId && p.IsActive);
        AddInclude(p => p.Category!);
        AddInclude(p => p.Supplier!);
        SetAsNoTracking();
    }
}

public class LowStockProductsSpec : BaseSpecification<Product>
{
    public LowStockProductsSpec(int companyId, decimal threshold)
    {
        AddCriteria(p => p.CompanyId == companyId && p.IsActive && p.CurrentStock <= threshold);
        AddInclude(p => p.Category!);
        SetAsNoTracking();
        ApplyOrderBy(p => p.CurrentStock);
    }
}
