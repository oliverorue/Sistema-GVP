using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.Domain.Entities;
using SistemaGVP.Domain.Enums;
using SistemaGVP.Domain.Exceptions;
using SistemaGVP.Domain.Interfaces;

namespace SistemaGVP.Application.Services;

public class SaleService : ISaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IProductRepository _productRepository;
    private readonly IInventoryMovementRepository _movementRepository;
    private readonly IInvoiceCounterRepository _invoiceCounterRepository;
    private readonly IRepository<Company> _companyRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateSaleDto> _saleValidator;
    private readonly ILogger<SaleService> _logger;

    public SaleService(
        ISaleRepository saleRepository,
        IProductRepository productRepository,
        IInventoryMovementRepository movementRepository,
        IInvoiceCounterRepository invoiceCounterRepository,
        IRepository<Company> companyRepository,
        IRepository<Customer> customerRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CreateSaleDto> saleValidator,
        ILogger<SaleService> logger)
    {
        _saleRepository = saleRepository;
        _productRepository = productRepository;
        _movementRepository = movementRepository;
        _invoiceCounterRepository = invoiceCounterRepository;
        _companyRepository = companyRepository;
        _customerRepository = customerRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _saleValidator = saleValidator;
        _logger = logger;
    }

    // ========================================================================
    // Hold / Resume (Ventas en Espera) - Persistidas en Base de Datos
    // ========================================================================

    public async Task<ServiceResult<HeldSaleDto>> HoldSaleAsync(CreateSaleDto saleDto, int companyId, int userId)
    {
        try
        {
            var validationResult = await _saleValidator.ValidateAsync(saleDto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ServiceResult<HeldSaleDto>.Failure(errors);
            }

            // Crear los detalles de la venta en espera
            var saleDetails = saleDto.Items.Select(i => new SaleDetail
            {
                ProductId = i.ProductId,
                ProductName = string.Empty, // Se completa al reanudar
                Barcode = string.Empty,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Cost = 0,
                Discount = i.Discount,
                Subtotal = (i.Quantity * i.UnitPrice) - i.Discount
            }).ToList();

            var subtotal = saleDetails.Sum(d => d.Subtotal + d.Discount);
            var totalDiscount = saleDetails.Sum(d => d.Discount);
            var total = subtotal - totalDiscount;

            // Persistir la venta en espera como una entidad Sale con Status = HeldSale
            var sale = new Sale
            {
                CompanyId = companyId,
                UserId = userId,
                CustomerId = saleDto.CustomerId,
                InvoiceNumber = $"HELD-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
                Subtotal = subtotal,
                Tax = 0,
                Discount = totalDiscount,
                Total = total,
                Status = SaleStatus.HeldSale,
                PaymentMethod = Enum.Parse<PaymentMethod>(saleDto.PaymentMethod),
                CashAmount = saleDto.CashAmount,
                ChangeAmount = 0,
                Notes = saleDto.Notes,
                SaleDetails = saleDetails
            };

            await _saleRepository.AddAsync(sale);
            await _unitOfWork.CompleteAsync();

            var heldSaleDto = new HeldSaleDto
            {
                Id = sale.Id,
                CustomerName = saleDto.CustomerId?.ToString(),
                Items = saleDetails.Select(d => new SaleDetailDto
                {
                    ProductId = d.ProductId,
                    ProductName = d.ProductName,
                    Barcode = d.Barcode,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    Discount = d.Discount,
                    Subtotal = d.Subtotal
                }).ToList(),
                HeldAt = sale.CreatedAt,
                SaleTotal = sale.Total
            };

            _logger.LogInformation("Venta en espera #{Id} guardada en BD | Items: {Items}", sale.Id, saleDetails.Count);

            return ServiceResult<HeldSaleDto>.Success(heldSaleDto, $"Venta en espera #{sale.Id} guardada.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al pausar venta");
            return ServiceResult<HeldSaleDto>.Failure($"Error al pausar la venta: {ex.Message}");
        }
    }

    public async Task<ServiceResult<HeldSaleDto?>> ResumeSaleAsync(int heldSaleId, int companyId)
    {
        try
        {
            var sale = await _saleRepository.GetWithDetailsAsync(heldSaleId);
            if (sale == null || sale.Status != SaleStatus.HeldSale || sale.CompanyId != companyId)
                return ServiceResult<HeldSaleDto?>.Failure("Venta en espera no encontrada.");

            var heldSaleDto = new HeldSaleDto
            {
                Id = sale.Id,
                CustomerName = sale.Customer?.Name ?? sale.CustomerId?.ToString(),
                Items = sale.SaleDetails.Select(d => new SaleDetailDto
                {
                    ProductId = d.ProductId,
                    ProductName = d.ProductName,
                    Barcode = d.Barcode,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    Discount = d.Discount,
                    Subtotal = d.Subtotal
                }).ToList(),
                HeldAt = sale.CreatedAt,
                SaleTotal = sale.Total
            };

            // Marcar la venta original como cancelada para que no aparezca
            // como pendiente ni se duplique en reportes
            sale.Status = SaleStatus.Cancelled;
            sale.Notes = $"Reanudada y convertida en una nueva venta — {DateTime.UtcNow:yyyy-MM-dd HH:mm}";
            _saleRepository.Update(sale);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Venta en espera #{Id} reanudada y marcada como cancelada en BD", heldSaleId);
            return ServiceResult<HeldSaleDto?>.Success(heldSaleDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al reanudar venta {Id}", heldSaleId);
            return ServiceResult<HeldSaleDto?>.Failure("Error al reanudar la venta.");
        }
    }

    public async Task<ServiceResult<List<HeldSaleDto>>> GetHeldSalesAsync(int companyId)
    {
        try
        {
            var heldSales = await _saleRepository.GetHeldSalesAsync(companyId);

            var dtos = heldSales.Select(s => new HeldSaleDto
            {
                Id = s.Id,
                CustomerName = s.Customer?.Name ?? s.CustomerId?.ToString(),
                Items = s.SaleDetails.Select(d => new SaleDetailDto
                {
                    ProductId = d.ProductId,
                    ProductName = d.ProductName,
                    Barcode = d.Barcode,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    Discount = d.Discount,
                    Subtotal = d.Subtotal
                }).ToList(),
                HeldAt = s.CreatedAt,
                SaleTotal = s.Total
            }).OrderByDescending(h => h.HeldAt).ToList();

            return ServiceResult<List<HeldSaleDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar ventas en espera");
            return ServiceResult<List<HeldSaleDto>>.Failure("Error al cargar ventas en espera.");
        }
    }

    public async Task<ServiceResult<bool>> RemoveHeldSaleAsync(int heldSaleId, int companyId)
    {
        try
        {
            var sale = await _saleRepository.GetWithDetailsAsync(heldSaleId);
            if (sale == null || sale.Status != SaleStatus.HeldSale || sale.CompanyId != companyId)
                return ServiceResult<bool>.Failure("Venta en espera no encontrada.");

            // Marcar como cancelada en lugar de eliminar físicamente
            sale.Status = SaleStatus.Cancelled;
            sale.Notes = "Eliminada por el usuario desde ventas en espera";
            _saleRepository.Update(sale);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Venta en espera #{Id} eliminada (cancelada) desde BD", heldSaleId);
            return ServiceResult<bool>.Success(true, "Venta en espera eliminada.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar venta en espera {Id}", heldSaleId);
            return ServiceResult<bool>.Failure("Error al eliminar venta en espera.");
        }
    }

    // ========================================================================
    // Historial de Ventas
    // ========================================================================

    public async Task<ServiceResult<PagedResult<SaleHistoryDto>>> GetSalesHistoryAsync(
        int companyId, string? searchTerm, DateTime? startDate, DateTime? endDate,
        string? paymentMethod, int page, int pageSize)
    {
        try
        {
            var (items, totalCount) = await _saleRepository.GetSalesWithFilterAsync(
                companyId, searchTerm, startDate, endDate, paymentMethod, page, pageSize);

            var dtos = items.Select(s => new SaleHistoryDto
            {
                Id = s.Id,
                InvoiceNumber = s.InvoiceNumber,
                CustomerName = s.Customer?.Name ?? "Consumidor Final",
                Total = s.Total,
                PaymentMethod = s.PaymentMethod.ToString(),
                Status = s.Status.ToString(),
                CreatedAt = s.CreatedAt,
                ItemCount = s.SaleDetails?.Sum(d => (int)d.Quantity) ?? 0,
                CashAmount = s.CashAmount,
                ChangeAmount = s.ChangeAmount
            }).ToList();

            var result = new PagedResult<SaleHistoryDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = pageSize
            };

            return ServiceResult<PagedResult<SaleHistoryDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener historial de ventas");
            return ServiceResult<PagedResult<SaleHistoryDto>>.Failure("Error al cargar historial de ventas.");
        }
    }

    public async Task<ServiceResult<SaleDetailDto>> GetSaleDetailAsync(int saleId, int companyId)
    {
        try
        {
            var sale = await _saleRepository.GetWithDetailsAsync(saleId);
            if (sale == null)
                return ServiceResult<SaleDetailDto>.Failure("Venta no encontrada.");

            // Devolvemos el primer detalle como representación (o podemos crear un DTO específico)
            var detail = sale.SaleDetails?.FirstOrDefault();
            if (detail == null)
                return ServiceResult<SaleDetailDto>.Failure("Venta sin detalles.");

            var dto = _mapper.Map<SaleDetailDto>(detail);
            return ServiceResult<SaleDetailDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener detalle de venta {Id}", saleId);
            return ServiceResult<SaleDetailDto>.Failure("Error al obtener detalle de venta.");
        }
    }

    public async Task<ServiceResult<bool>> CancelSaleAsync(int saleId, int companyId, int userId, string reason)
    {
        try
        {
            var sale = await _saleRepository.GetWithDetailsAsync(saleId);
            if (sale == null)
                return ServiceResult<bool>.Failure("Venta no encontrada.");

            if (sale.Status == SaleStatus.Cancelled)
                return ServiceResult<bool>.Failure("La venta ya está anulada.");

            if (sale.CompanyId != companyId)
                return ServiceResult<bool>.Failure("La venta no pertenece a esta empresa.");

            // Restaurar stock: crear movimientos inversos
            foreach (var detail in sale.SaleDetails)
            {
                var product = await _productRepository.GetByIdAsync(detail.ProductId);
                if (product != null)
                {
                    var movement = new InventoryMovement
                    {
                        ProductId = product.Id,
                        UserId = userId,
                        CompanyId = sale.CompanyId,
                        RelatedSaleId = sale.Id,
                        Type = MovementType.IN,
                        Quantity = detail.Quantity,
                        StockBefore = product.CurrentStock,
                        StockAfter = product.CurrentStock + detail.Quantity,
                        Reason = "return",
                        Notes = $"Anulación de venta #{sale.InvoiceNumber}: {reason}"
                    };

                    product.CurrentStock += detail.Quantity;
                    _productRepository.Update(product);
                    await _movementRepository.AddAsync(movement);
                }
            }

            sale.Status = SaleStatus.Cancelled;
            sale.Notes = reason;
            _saleRepository.Update(sale);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Venta #{InvoiceNumber} anulada por usuario {UserId}. Motivo: {Reason}",
                sale.InvoiceNumber, userId, reason);

            return ServiceResult<bool>.Success(true, $"Venta #{sale.InvoiceNumber} anulada exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al anular venta {SaleId}", saleId);
            return ServiceResult<bool>.Failure("Error al anular la venta.");
        }
    }

    public async Task<ServiceResult<SaleDto>> GetByIdAsync(int id)
    {
        try
        {
            var sale = await _saleRepository.GetWithDetailsAsync(id);
            if (sale == null)
                return ServiceResult<SaleDto>.Failure("Venta no encontrada.");

            var dto = _mapper.Map<SaleDto>(sale);
            return ServiceResult<SaleDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener venta {Id}", id);
            return ServiceResult<SaleDto>.Failure("Error al obtener venta.");
        }
    }

    public async Task<ServiceResult<SaleDto>> CreateSaleAsync(CreateSaleDto dto)
    {
        try
        {
            var validationResult = await _saleValidator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ServiceResult<SaleDto>.Failure(errors);
            }

            // Ejecutar toda la operación dentro de una transacción Serializable
            // para evitar race conditions en el stock (Tarea 2)
            return await _saleRepository.ExecuteWithSerializableTransactionAsync(async () =>
            {
                // 1. Generar número de factura (atómico)
                var invoiceNumber = await GenerateInvoiceNumberAsync(dto.CompanyId);

                // 2. Obtener tasa de IVA desde la compañía y preferir la del DTO
                var company = await _companyRepository.GetByIdAsync(dto.CompanyId);
                var taxRate = dto.TaxRate > 0 ? dto.TaxRate : (company?.TaxRate ?? 0.10m);
                var ivaIncluido = dto.IvaIncluido ?? company?.IvaIncluido ?? true;

                // 3. Calcular totales y preparar detalles
                decimal subtotal = 0;
                decimal totalCost = 0;
                var saleDetails = new List<SaleDetail>();
                var movements = new List<InventoryMovement>();

                foreach (var itemDto in dto.Items)
                {
                    var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                    if (product == null)
                        return ServiceResult<SaleDto>.Failure($"Producto ID {itemDto.ProductId} no encontrado.");

                    if (!product.IsActive)
                        return ServiceResult<SaleDto>.Failure($"El producto '{product.Name}' está desactivado.");

                    if (product.CurrentStock < itemDto.Quantity)
                        throw new InsufficientStockException(product.Name, product.CurrentStock, itemDto.Quantity);

                    var itemSubtotal = itemDto.Quantity * itemDto.UnitPrice;

                    saleDetails.Add(new SaleDetail
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,     // Snapshot
                        Barcode = product.Barcode,       // Snapshot
                        Unit = product.Unit,             // Snapshot (kg, m, litro, pz...)
                        Quantity = itemDto.Quantity,
                        UnitPrice = itemDto.UnitPrice,
                        Cost = product.Cost,             // Snapshot del costo
                        Discount = itemDto.Discount,
                        Subtotal = itemSubtotal - itemDto.Discount
                    });

                    // Registrar movimiento de stock OUT
                    movements.Add(new InventoryMovement
                    {
                        ProductId = product.Id,
                        UserId = dto.UserId,
                        CompanyId = dto.CompanyId,
                        Type = MovementType.OUT,
                        Quantity = itemDto.Quantity,
                        StockBefore = product.CurrentStock,
                        StockAfter = product.CurrentStock - itemDto.Quantity,
                        Reason = "sale",
                        Notes = $"Venta #{invoiceNumber}"
                    });

                    // Actualizar stock del producto
                    product.CurrentStock -= itemDto.Quantity;
                    _productRepository.Update(product);

                    subtotal += itemSubtotal;
                    totalCost += product.Cost * itemDto.Quantity;
                }

                // 4. Calcular impuestos e incluir descuento global
                var totalDiscount = dto.Items.Sum(i => i.Discount) + dto.Discount;
                decimal tax;
                decimal total;

                if (ivaIncluido)
                {
                    // IVA incluido: los precios ya contienen IVA
                    // IVA = total_con_iva / (1 + taxRate) * taxRate
                    var baseAmount = (subtotal - totalDiscount) / (1 + taxRate);
                    tax = (subtotal - totalDiscount) - baseAmount;
                    total = subtotal - totalDiscount;
                }
                else
                {
                    // IVA discriminado: se suma al subtotal
                    tax = (subtotal - totalDiscount) * taxRate;
                    total = subtotal - totalDiscount + tax;
                }

                // 4.1 Validar límite de crédito del cliente
                if (dto.CustomerId.HasValue)
                {
                    var customer = await _customerRepository.GetByIdAsync(dto.CustomerId.Value);
                    if (customer != null && customer.CreditLimit > 0
                        && (total + customer.Balance > customer.CreditLimit))
                    {
                        return ServiceResult<SaleDto>.Failure(
                            $"El cliente '{customer.Name}' supera su límite de crédito. Saldo: {customer.Balance:N0}, Límite: {customer.CreditLimit:N0}");
                    }
                }

                // 5. Calcular cambio si es efectivo
                decimal changeAmount = 0;
                if (dto.PaymentMethod == "Cash")
                {
                    changeAmount = dto.CashAmount - total;
                    if (changeAmount < 0)
                        return ServiceResult<SaleDto>.Failure("El monto en efectivo debe cubrir el total.");
                }

                // 6. Crear la venta
                var sale = new Sale
                {
                    CompanyId = dto.CompanyId,
                    UserId = dto.UserId,
                    CustomerId = dto.CustomerId,
                    InvoiceNumber = invoiceNumber,
                    Subtotal = subtotal,
                    Tax = tax,
                    Discount = totalDiscount,
                    Total = total,
                    Status = SaleStatus.Completed,
                    PaymentMethod = Enum.Parse<PaymentMethod>(dto.PaymentMethod),
                    CashAmount = dto.CashAmount,
                    ChangeAmount = changeAmount,
                    Notes = dto.Notes,
                    SaleDetails = saleDetails,
                    InventoryMovements = movements
                };

                await _saleRepository.AddAsync(sale);
                await _unitOfWork.CompleteAsync();

                // Update customer balance for credit sales
                if (dto.PaymentMethod == "Credit" && dto.CustomerId.HasValue)
                {
                    var customerEntity = await _customerRepository.GetByIdAsync(dto.CustomerId.Value);
                    if (customerEntity != null)
                    {
                        customerEntity.Balance += total;
                        _customerRepository.Update(customerEntity);
                        await _unitOfWork.CompleteAsync();
                        _logger.LogInformation("Saldo cliente #{CustomerId}: +{Amount}, nuevo saldo = {Balance}",
                            dto.CustomerId, total, customerEntity.Balance);
                    }
                }

                _logger.LogInformation(
                    "Venta #{InvoiceNumber} creada | Total: {Total} | Items: {ItemCount} | IVA: {TaxRate:P}",
                    invoiceNumber, total, dto.Items.Count, taxRate);

                var resultDto = _mapper.Map<SaleDto>(sale);
                return ServiceResult<SaleDto>.Success(resultDto, $"Venta #{invoiceNumber} completada.");
            });
        }
        catch (InsufficientStockException ex)
        {
            _logger.LogWarning(ex, "Stock insuficiente al crear venta");
            return ServiceResult<SaleDto>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear venta");
            return ServiceResult<SaleDto>.Failure("Error al procesar la venta.");
        }
    }

    public async Task<ServiceResult<bool>> CancelSaleAsync(int saleId, int userId)
    {
        try
        {
            var sale = await _saleRepository.GetWithDetailsAsync(saleId);
            if (sale == null)
                return ServiceResult<bool>.Failure("Venta no encontrada.");

            if (sale.Status == SaleStatus.Cancelled)
                return ServiceResult<bool>.Failure("La venta ya está anulada.");

            // Restaurar stock: crear movimientos inversos
            foreach (var detail in sale.SaleDetails)
            {
                var product = await _productRepository.GetByIdAsync(detail.ProductId);
                if (product != null)
                {
                    var movement = new InventoryMovement
                    {
                        ProductId = product.Id,
                        UserId = userId,
                        CompanyId = sale.CompanyId,
                        RelatedSaleId = sale.Id,
                        Type = MovementType.IN,
                        Quantity = detail.Quantity,
                        StockBefore = product.CurrentStock,
                        StockAfter = product.CurrentStock + detail.Quantity,
                        Reason = "return",
                        Notes = $"Anulación de venta #{sale.InvoiceNumber}"
                    };

                    product.CurrentStock += detail.Quantity;
                    _productRepository.Update(product);
                    await _movementRepository.AddAsync(movement);
                }
            }

            sale.Status = SaleStatus.Cancelled;
            _saleRepository.Update(sale);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Venta #{InvoiceNumber} anulada por usuario {UserId}", sale.InvoiceNumber, userId);

            return ServiceResult<bool>.Success(true, $"Venta #{sale.InvoiceNumber} anulada exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al anular venta {SaleId}", saleId);
            return ServiceResult<bool>.Failure("Error al anular la venta.");
        }
    }

    public async Task<ServiceResult<List<SaleDto>>> GetSalesByDateAsync(int companyId, DateTime from, DateTime to)
    {
        try
        {
            var sales = await _saleRepository.GetByDateRangeAsync(companyId, from, to);
            var dtos = _mapper.Map<List<SaleDto>>(sales);
            return ServiceResult<List<SaleDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener ventas por fecha");
            return ServiceResult<List<SaleDto>>.Failure("Error al cargar ventas.");
        }
    }

    private async Task<string> GenerateInvoiceNumberAsync(int companyId)
    {
        // Formato: INV-{YYYYMMDD}-{XXXX}
        // Usa el contador atómico para evitar números duplicados bajo concurrencia
        var datePart = DateTime.UtcNow.ToString("yyyyMMdd");
        var sequentialNumber = await _invoiceCounterRepository.GetNextNumberAsync(companyId, datePart);
        return $"INV-{datePart}-{sequentialNumber:D4}";
    }
}
