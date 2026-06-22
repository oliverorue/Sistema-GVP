using AutoMapper;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.Application.Services;
using SistemaGVP.Domain.Entities;
using SistemaGVP.Domain.Enums;
using SistemaGVP.Domain.Interfaces;

namespace SistemaGVP.Application.Tests.Services;

public class SaleServiceTests
{
    private readonly Mock<ISaleRepository> _saleRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IInventoryMovementRepository> _movementRepoMock;
    private readonly Mock<IInvoiceCounterRepository> _invoiceCounterRepoMock;
    private readonly Mock<IRepository<Company>> _companyRepoMock;
    private readonly Mock<IRepository<Customer>> _customerRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<CreateSaleDto>> _saleValidatorMock;
    private readonly Mock<ILogger<SaleService>> _loggerMock;
    private readonly SaleService _sut;

    public SaleServiceTests()
    {
        _saleRepoMock = new Mock<ISaleRepository>();
        _productRepoMock = new Mock<IProductRepository>();
        _movementRepoMock = new Mock<IInventoryMovementRepository>();
        _invoiceCounterRepoMock = new Mock<IInvoiceCounterRepository>();
        _companyRepoMock = new Mock<IRepository<Company>>();
        _customerRepoMock = new Mock<IRepository<Customer>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _saleValidatorMock = new Mock<IValidator<CreateSaleDto>>();
        _loggerMock = new Mock<ILogger<SaleService>>();

        _sut = new SaleService(
            _saleRepoMock.Object,
            _productRepoMock.Object,
            _movementRepoMock.Object,
            _invoiceCounterRepoMock.Object,
            _companyRepoMock.Object,
            _customerRepoMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _saleValidatorMock.Object,
            _loggerMock.Object);
    }

    private static CreateSaleDto CreateValidSaleDto() => new()
    {
        CompanyId = 1,
        UserId = 1,
        PaymentMethod = "Cash",
        CashAmount = 200m,
        Items = new List<CreateSaleDetailDto>
        {
            new() { ProductId = 1, Quantity = 2, UnitPrice = 50m, Discount = 0 }
        }
    };

    private static Product CreateProduct(decimal stock = 10) => new()
    {
        Id = 1,
        CompanyId = 1,
        Name = "Producto Test",
        Barcode = "123",
        Price = 50m,
        Cost = 25m,
        CurrentStock = stock,
        IsActive = true
    };

    private static Company CreateCompany() => new()
    {
        Id = 1,
        Name = "Test Company",
        TaxRate = 0.10m
    };

    // ========================
    // HoldSaleAsync
    // ========================

    [Fact]
    public async Task HoldSaleAsync_WhenValid_ReturnsSuccess()
    {
        var dto = CreateValidSaleDto();
        var validationResult = new ValidationResult();

        _saleValidatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);
        _saleRepoMock.Setup(r => r.AddAsync(It.IsAny<Sale>())).ReturnsAsync((Sale s) => s);
        _unitOfWorkMock.Setup(u => u.CompleteAsync(default)).ReturnsAsync(1);

        var result = await _sut.HoldSaleAsync(dto, 1, 1);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task HoldSaleAsync_WhenValidationFails_ReturnsFailure()
    {
        var dto = CreateValidSaleDto();
        dto.PaymentMethod = "INVALID";
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("PaymentMethod", "Método de pago inválido.")
        });

        _saleValidatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);

        var result = await _sut.HoldSaleAsync(dto, 1, 1);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Método de pago inválido.");
    }

    [Fact]
    public async Task HoldSaleAsync_WhenEmptyItems_ReturnsFailure()
    {
        var dto = CreateValidSaleDto();
        dto.Items.Clear();
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Items", "La venta debe tener al menos un producto.")
        });

        _saleValidatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);

        var result = await _sut.HoldSaleAsync(dto, 1, 1);

        result.IsSuccess.Should().BeFalse();
    }

    // ========================
    // CancelSaleAsync (with reason)
    // ========================

    [Fact]
    public async Task CancelSaleWithReason_WhenValid_ReturnsSuccess()
    {
        var product = CreateProduct(10);
        var sale = new Sale
        {
            Id = 1,
            CompanyId = 1,
            InvoiceNumber = "INV-001",
            Status = SaleStatus.Completed,
            SaleDetails = new List<SaleDetail>
            {
                new() { ProductId = 1, Quantity = 2, ProductName = "P1" }
            }
        };

        _saleRepoMock.Setup(r => r.GetWithDetailsAsync(1)).ReturnsAsync(sale);
        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
        _movementRepoMock.Setup(r => r.AddAsync(It.IsAny<InventoryMovement>())).ReturnsAsync((InventoryMovement m) => m);
        _productRepoMock.Setup(r => r.Update(product));
        _saleRepoMock.Setup(r => r.Update(sale));
        _unitOfWorkMock.Setup(u => u.CompleteAsync(default)).ReturnsAsync(1);

        var result = await _sut.CancelSaleAsync(1, 1, 1, "Error en producto");

        result.IsSuccess.Should().BeTrue();
        product.CurrentStock.Should().Be(12);
        sale.Status.Should().Be(SaleStatus.Cancelled);
    }

    [Fact]
    public async Task CancelSaleWithReason_WhenNotFound_ReturnsFailure()
    {
        _saleRepoMock.Setup(r => r.GetWithDetailsAsync(99)).ReturnsAsync((Sale?)null);

        var result = await _sut.CancelSaleAsync(99, 1, 1, "razon");

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Venta no encontrada.");
    }

    [Fact]
    public async Task CancelSaleWithReason_WhenAlreadyCancelled_ReturnsFailure()
    {
        var sale = new Sale { Id = 1, CompanyId = 1, Status = SaleStatus.Cancelled };
        _saleRepoMock.Setup(r => r.GetWithDetailsAsync(1)).ReturnsAsync(sale);

        var result = await _sut.CancelSaleAsync(1, 1, 1, "razon");

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("La venta ya está anulada.");
    }

    [Fact]
    public async Task CancelSaleWithReason_WhenWrongCompany_ReturnsFailure()
    {
        var sale = new Sale { Id = 1, CompanyId = 2, Status = SaleStatus.Completed };
        _saleRepoMock.Setup(r => r.GetWithDetailsAsync(1)).ReturnsAsync(sale);

        var result = await _sut.CancelSaleAsync(1, 1, 1, "razon");

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("La venta no pertenece a esta empresa.");
    }

    // ========================
    // CreateSaleAsync
    // ========================

    [Fact]
    public async Task CreateSaleAsync_WhenValidationFails_ReturnsFailure()
    {
        var dto = CreateValidSaleDto();
        dto.Items.Clear();
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Items", "Debe tener al menos un producto.")
        });

        _saleValidatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);

        var result = await _sut.CreateSaleAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("Debe tener al menos un producto.");
    }

    [Fact]
    public async Task CreateSaleAsync_WhenInsufficientStock_ReturnsFailure()
    {
        var dto = CreateValidSaleDto();
        var product = CreateProduct(0);
        var company = CreateCompany();
        var validationResult = new ValidationResult();

        _saleValidatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);
        _saleRepoMock
            .Setup(r => r.ExecuteWithSerializableTransactionAsync(It.IsAny<Func<Task<ServiceResult<SaleDto>>>>()))
            .Returns((Func<Task<ServiceResult<SaleDto>>> f) => f());
        _invoiceCounterRepoMock.Setup(r => r.GetNextNumberAsync(1, It.IsAny<string>())).ReturnsAsync(1);
        _companyRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(company);
        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        var result = await _sut.CreateSaleAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Stock insuficiente");
    }

    [Fact]
    public async Task ResumeSaleAsync_WhenNotFound_ReturnsFailure()
    {
        _saleRepoMock.Setup(r => r.GetWithDetailsAsync(99)).ReturnsAsync((Sale?)null);

        var result = await _sut.ResumeSaleAsync(99, 1);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task ResumeSaleAsync_WhenNotHeldSale_ReturnsFailure()
    {
        var sale = new Sale { Id = 1, CompanyId = 1, Status = SaleStatus.Completed };
        _saleRepoMock.Setup(r => r.GetWithDetailsAsync(1)).ReturnsAsync(sale);

        var result = await _sut.ResumeSaleAsync(1, 1);

        result.IsSuccess.Should().BeFalse();
    }
}
