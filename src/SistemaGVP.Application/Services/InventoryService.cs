using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.Domain.Entities;
using SistemaGVP.Domain.Enums;
using SistemaGVP.Domain.Interfaces;

namespace SistemaGVP.Application.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryMovementRepository _movementRepository;
    private readonly IProductRepository _productRepository;
    private readonly IRepository<Company> _companyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateInventoryMovementDto> _movementValidator;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(
        IInventoryMovementRepository movementRepository,
        IProductRepository productRepository,
        IRepository<Company> companyRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateInventoryMovementDto> movementValidator,
        ILogger<InventoryService> logger)
    {
        _movementRepository = movementRepository;
        _productRepository = productRepository;
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _movementValidator = movementValidator;
        _logger = logger;
    }

    public async Task<ServiceResult<List<InventoryMovementDto>>> GetMovementsByProductAsync(int productId, int companyId)
    {
        try
        {
            var movements = await _movementRepository.GetByProductIdAsync(productId, companyId);
            var dtos = _mapper.Map<List<InventoryMovementDto>>(movements);
            return ServiceResult<List<InventoryMovementDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener movimientos del producto {ProductId}", productId);
            return ServiceResult<List<InventoryMovementDto>>.Failure("Error al cargar movimientos.");
        }
    }

    public async Task<ServiceResult<InventoryMovementDto>> AdjustStockAsync(CreateInventoryMovementDto dto)
    {
        try
        {
            var validationResult = await _movementValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ServiceResult<InventoryMovementDto>.Failure(errors);
            }

            var product = await _productRepository.GetByIdAsync(dto.ProductId);
            if (product == null)
                return ServiceResult<InventoryMovementDto>.Failure("Producto no encontrado.");

            if (!Enum.TryParse<MovementType>(dto.Type, out var movementType))
                return ServiceResult<InventoryMovementDto>.Failure("Tipo de movimiento inválido.");

            decimal stockBefore = product.CurrentStock;
            decimal stockAfter;

            switch (movementType)
            {
                case MovementType.IN:
                    stockAfter = stockBefore + dto.Quantity;
                    break;
                case MovementType.OUT:
                    if (stockBefore < dto.Quantity)
                        return ServiceResult<InventoryMovementDto>.Failure(
                            $"Stock insuficiente. Disponible: {stockBefore}, solicitado: {dto.Quantity}");
                    stockAfter = stockBefore - dto.Quantity;
                    break;
                case MovementType.ADJUSTMENT:
                    stockAfter = dto.Quantity;  // Ajuste: quantity es el nuevo stock
                    break;
                default:
                    return ServiceResult<InventoryMovementDto>.Failure("Tipo de movimiento inválido.");
            }

            var movement = new InventoryMovement
            {
                ProductId = product.Id,
                UserId = dto.UserId,
                CompanyId = dto.CompanyId,
                RelatedSaleId = dto.RelatedSaleId,
                Type = movementType,
                Quantity = Math.Abs(stockAfter - stockBefore),
                StockBefore = stockBefore,
                StockAfter = stockAfter,
                Reason = dto.Reason,
                Notes = dto.Notes
            };

            product.CurrentStock = stockAfter;
            _productRepository.Update(product);
            await _movementRepository.AddAsync(movement);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation(
                "Ajuste de stock: Producto {ProductId} | {Type} | {Quantity} | Stock: {Before}→{After}",
                product.Id, dto.Type, dto.Quantity, stockBefore, stockAfter);

            var resultDto = _mapper.Map<InventoryMovementDto>(movement);
            return ServiceResult<InventoryMovementDto>.Success(resultDto, "Movimiento registrado exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al ajustar stock del producto {ProductId}", dto.ProductId);
            return ServiceResult<InventoryMovementDto>.Failure("Error al registrar movimiento.");
        }
    }
    public async Task<ServiceResult<List<InventoryMovementDto>>> GetRecentMovementsAsync(int companyId, int count = 50)
    {
        try
        {
            var movements = await _movementRepository.GetByDateRangeAsync(
                companyId, DateTime.UtcNow.AddYears(-1), DateTime.UtcNow);
            var recent = movements.Take(count).ToList();
            var dtos = _mapper.Map<List<InventoryMovementDto>>(recent);
            return ServiceResult<List<InventoryMovementDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener movimientos recientes para empresa {CompanyId}", companyId);
            return ServiceResult<List<InventoryMovementDto>>.Failure("Error al cargar movimientos de inventario.");
        }
    }

    public async Task<ServiceResult<List<ProductDto>>> GetLowStockProductsAsync(int companyId)
    {
        try
        {
            var company = await _companyRepository.GetByIdAsync(companyId);
            var threshold = company?.LowStockThreshold ?? 10;

            var lowStockProducts = await _productRepository.GetLowStockAsync(companyId, threshold);

            var dtos = _mapper.Map<List<ProductDto>>(lowStockProducts);

            // Calculate difference for each product
            foreach (var dto in dtos)
            {
                dto.Difference = (int)(threshold - dto.CurrentStock);
            }

            _logger.LogInformation(
                "Productos con stock bajo: {Count} (umbral: {Threshold}) para empresa {CompanyId}",
                dtos.Count, threshold, companyId);

            return ServiceResult<List<ProductDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener productos con stock bajo para empresa {CompanyId}", companyId);
            return ServiceResult<List<ProductDto>>.Failure("Error al cargar alertas de stock bajo.");
        }
    }

    public async Task<ServiceResult<int>> GetLowStockCountAsync(int companyId)
    {
        try
        {
            var company = await _companyRepository.GetByIdAsync(companyId);
            var threshold = company?.LowStockThreshold ?? 10;

            var lowStockProducts = await _productRepository.GetLowStockAsync(companyId, threshold);

            return ServiceResult<int>.Success(lowStockProducts.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al contar productos con stock bajo para empresa {CompanyId}", companyId);
            return ServiceResult<int>.Failure("Error al contar productos con stock bajo.");
        }
    }
}
