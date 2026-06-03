using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.Domain.Entities;
using SistemaGVP.Domain.Interfaces;

namespace SistemaGVP.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<ProductDto> _validator;
    private readonly ICacheService _cache;
    private readonly ILogger<ProductService> _logger;

    private static readonly TimeSpan BarcodeCacheDuration = TimeSpan.FromMinutes(10);

    public ProductService(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<ProductDto> validator,
        ICacheService cache,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _validator = validator;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ServiceResult<ProductDto>> GetByIdAsync(int id)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return ServiceResult<ProductDto>.Failure("Producto no encontrado.");

            var dto = _mapper.Map<ProductDto>(product);
            return ServiceResult<ProductDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener producto {Id}", id);
            return ServiceResult<ProductDto>.Failure("Error al obtener producto.");
        }
    }

    public async Task<ServiceResult<ProductDto>> GetByBarcodeAsync(string barcode, int companyId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(barcode))
                return ServiceResult<ProductDto>.Failure("El código de barras es requerido.");

            var trimmedBarcode = barcode.Trim();
            var cacheKey = $"product:barcode:{companyId}:{trimmedBarcode}";

            var dto = await _cache.GetOrCreateAsync(cacheKey, async () =>
            {
                var product = await _productRepository.GetByBarcodeAsync(trimmedBarcode, companyId);
                if (product == null || !product.IsActive)
                    return null!;

                return _mapper.Map<ProductDto>(product);
            }, BarcodeCacheDuration);

            if (dto == null)
                return ServiceResult<ProductDto>.Failure("Producto no encontrado.");

            return ServiceResult<ProductDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar producto por código {Barcode}", barcode);
            return ServiceResult<ProductDto>.Failure("Error al buscar producto.");
        }
    }

    public async Task<ServiceResult<PagedResult<ProductDto>>> GetAllAsync(PaginationFilter filter, int companyId)
    {
        try
        {
            _logger.LogInformation("GetAllAsync: PageNumber={Page}, PageSize={Size}, SearchTerm='{Search}', CompanyId={Company}",
                filter.PageNumber, filter.PageSize, filter.SearchTerm, companyId);

            var (items, totalCount) = await _productRepository.GetPagedAsync(
                filter.PageNumber, filter.PageSize, filter.SearchTerm, companyId);

            _logger.LogInformation("GetAllAsync: Items={Count}, TotalCount={Total}", items.Count, totalCount);

            var dtos = _mapper.Map<List<ProductDto>>(items);

            var pagedResult = new PagedResult<ProductDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };

            return ServiceResult<PagedResult<ProductDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lista de productos");
            return ServiceResult<PagedResult<ProductDto>>.Failure("Error al cargar productos.");
        }
    }

    public async Task<ServiceResult<ProductDto>> CreateAsync(ProductDto dto)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ServiceResult<ProductDto>.Failure(errors);
            }

            var existingBarcode = await _productRepository.GetByBarcodeAsync(dto.Barcode, dto.CompanyId);
            if (existingBarcode != null)
                return ServiceResult<ProductDto>.Failure($"Ya existe un producto con el código '{dto.Barcode}'.");

            var existingSku = await _productRepository.GetBySkuAsync(dto.Sku, dto.CompanyId);
            if (existingSku != null)
                return ServiceResult<ProductDto>.Failure($"Ya existe un producto con el SKU '{dto.Sku}'.");

            var product = _mapper.Map<Product>(dto);
            product.CurrentStock = 0;

            await _productRepository.AddAsync(product);
            await _unitOfWork.CompleteAsync();

            _cache.RemoveByPrefix("product:barcode");

            _logger.LogInformation("Producto creado: {Name} | Barcode: {Barcode}", product.Name, product.Barcode);

            var resultDto = _mapper.Map<ProductDto>(product);
            return ServiceResult<ProductDto>.Success(resultDto, "Producto creado exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear producto {Name}", dto.Name);
            return ServiceResult<ProductDto>.Failure("Error al crear producto.");
        }
    }

    public async Task<ServiceResult<ProductDto>> UpdateAsync(ProductDto dto)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ServiceResult<ProductDto>.Failure(errors);
            }

            var existingProduct = await _productRepository.GetByIdAsync(dto.Id);
            if (existingProduct == null)
                return ServiceResult<ProductDto>.Failure("Producto no encontrado.");

            if (existingProduct.Barcode != dto.Barcode)
            {
                var barcodeExists = await _productRepository.GetByBarcodeAsync(dto.Barcode, dto.CompanyId);
                if (barcodeExists != null)
                    return ServiceResult<ProductDto>.Failure($"Ya existe otro producto con el código '{dto.Barcode}'.");
            }

            _mapper.Map(dto, existingProduct);
            _productRepository.Update(existingProduct);
            await _unitOfWork.CompleteAsync();

            _cache.RemoveByPrefix("product:barcode");

            _logger.LogInformation("Producto actualizado: {Id} | {Name}", existingProduct.Id, existingProduct.Name);

            var resultDto = _mapper.Map<ProductDto>(existingProduct);
            return ServiceResult<ProductDto>.Success(resultDto, "Producto actualizado exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar producto {Id}", dto.Id);
            return ServiceResult<ProductDto>.Failure("Error al actualizar producto.");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
                return ServiceResult<bool>.Failure("Producto no encontrado.");

            if (product.CurrentStock > 0)
                return ServiceResult<bool>.Failure(
                    "No se puede desactivar un producto con stock positivo. Ajuste el inventario primero.");

            product.IsActive = false;
            _productRepository.Update(product);
            await _unitOfWork.CompleteAsync();

            _cache.RemoveByPrefix("product:barcode");

            _logger.LogInformation("Producto desactivado: {Id} | {Name}", id, product.Name);

            return ServiceResult<bool>.Success(true, "Producto desactivado exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar producto {Id}", id);
            return ServiceResult<bool>.Failure("Error al desactivar producto.");
        }
    }
}
