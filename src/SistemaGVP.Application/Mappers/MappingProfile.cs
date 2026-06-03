using AutoMapper;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Domain.Entities;

namespace SistemaGVP.Application.Mappers;

/// <summary>
/// Perfil de AutoMapper con todos los mapeos entre entidades y DTOs.
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // ── Product ──────────────────────────────────────
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
            .ForMember(d => d.SupplierName, o => o.MapFrom(s => s.Supplier != null ? s.Supplier.Name : null));

        CreateMap<ProductDto, Product>()
            .ForMember(d => d.CompanyId, o => o.MapFrom(s => s.CompanyId))
            .ForMember(d => d.Category, o => o.Ignore())
            .ForMember(d => d.Supplier, o => o.Ignore())
            .ForMember(d => d.Company, o => o.Ignore())
            .ForMember(d => d.SaleDetails, o => o.Ignore())
            .ForMember(d => d.InventoryMovements, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore());

        // ── Category ─────────────────────────────────────
        CreateMap<Category, CategoryDto>();
        CreateMap<CategoryDto, Category>()
            .ForMember(d => d.Company, o => o.Ignore())
            .ForMember(d => d.Products, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore());

        // ── Customer ─────────────────────────────────────
        CreateMap<Customer, CustomerDto>();
        CreateMap<CustomerDto, Customer>()
            .ForMember(d => d.Company, o => o.Ignore())
            .ForMember(d => d.Sales, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore());

        // ── User ─────────────────────────────────────────
        CreateMap<User, UserDto>()
            .ForMember(d => d.Password, o => o.Ignore());

        CreateMap<UserDto, User>()
            .ForMember(d => d.PasswordHash, o => o.Ignore())
            .ForMember(d => d.Company, o => o.Ignore())
            .ForMember(d => d.Sales, o => o.Ignore())
            .ForMember(d => d.InventoryMovements, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore())
            .ForMember(d => d.LastLogin, o => o.Ignore());

        // ── Sale ─────────────────────────────────────────
        CreateMap<Sale, SaleDto>()
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.CustomerName, o => o.MapFrom(s => s.Customer != null ? s.Customer.Name : null))
            .ForMember(d => d.Items, o => o.MapFrom(s => s.SaleDetails))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.PaymentMethod, o => o.MapFrom(s => s.PaymentMethod.ToString()));

        CreateMap<SaleDetail, SaleDetailDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.ProductName))
            .ForMember(d => d.Barcode, o => o.MapFrom(s => s.Barcode));

        // ── InventoryMovement ────────────────────────────
        CreateMap<InventoryMovement, InventoryMovementDto>()
            .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product.Name))
            .ForMember(d => d.ProductBarcode, o => o.MapFrom(s => s.Product.Barcode))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.FullName))
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));

        // ── Supplier ──────────────────────────────────────
        CreateMap<Supplier, SupplierDto>();
        CreateMap<SupplierDto, Supplier>()
            .ForMember(d => d.Company, o => o.Ignore())
            .ForMember(d => d.Products, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore());

        // ── Company ───────────────────────────────────────
        CreateMap<Company, CompanyDto>();
        CreateMap<CompanyDto, Company>()
            .ForMember(d => d.Logo, o => o.Ignore())
            .ForMember(d => d.Users, o => o.Ignore())
            .ForMember(d => d.Products, o => o.Ignore())
            .ForMember(d => d.Categories, o => o.Ignore())
            .ForMember(d => d.Customers, o => o.Ignore())
            .ForMember(d => d.Sales, o => o.Ignore())
            .ForMember(d => d.InventoryMovements, o => o.Ignore())
            .ForMember(d => d.Suppliers, o => o.Ignore())
            .ForMember(d => d.CreatedAt, o => o.Ignore())
            .ForMember(d => d.UpdatedAt, o => o.Ignore());

        // ── AuditLog ──────────────────────────────────────
        CreateMap<AuditLog, AuditLogDto>()
            .ForMember(d => d.Action, o => o.MapFrom(s => s.Action.ToString()));
    }
}
