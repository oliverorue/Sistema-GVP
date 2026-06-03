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
using SistemaGVP.Domain.Interfaces;

namespace SistemaGVP.Application.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IValidator<ProductDto>> _validatorMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly Mock<ILogger<ProductService>> _loggerMock;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _productRepoMock = new Mock<IProductRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _validatorMock = new Mock<IValidator<ProductDto>>();
        _cacheMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<ProductService>>();

        _sut = new ProductService(
            _productRepoMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _validatorMock.Object,
            _cacheMock.Object,
            _loggerMock.Object);
    }

    private static ProductDto CreateValidDto() => new()
    {
        Id = 1,
        CompanyId = 1,
        Name = "Producto Test",
        Barcode = "123456789",
        Sku = "SKU-001",
        Price = 100m,
        Cost = 50m,
        CategoryId = 1,
        MinStock = 5,
        CurrentStock = 0,
        Unit = "pz"
    };

    private static Product CreateProduct() => new()
    {
        Id = 1,
        CompanyId = 1,
        Name = "Producto Test",
        Barcode = "123456789",
        Sku = "SKU-001",
        Price = 100m,
        Cost = 50m,
        CategoryId = 1,
        MinStock = 5,
        Unit = "pz"
    };

    // ========================
    // CreateAsync
    // ========================

    [Fact]
    public async Task CreateAsync_WhenValid_ReturnsSuccess()
    {
        var dto = CreateValidDto();
        var product = CreateProduct();
        var validationResult = new ValidationResult();

        _validatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);
        _productRepoMock.Setup(r => r.GetByBarcodeAsync(dto.Barcode, dto.CompanyId)).ReturnsAsync((Product?)null);
        _productRepoMock.Setup(r => r.GetBySkuAsync(dto.Sku, dto.CompanyId)).ReturnsAsync((Product?)null);
        _mapperMock.Setup(m => m.Map<Product>(dto)).Returns(product);
        _productRepoMock.Setup(r => r.AddAsync(product)).ReturnsAsync(product);
        _unitOfWorkMock.Setup(u => u.CompleteAsync(default)).ReturnsAsync(1);
        _cacheMock.Setup(c => c.RemoveByPrefix("product:barcode"));
        _mapperMock.Setup(m => m.Map<ProductDto>(product)).Returns(dto);

        var result = await _sut.CreateAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(dto);
        result.Message.Should().Be("Producto creado exitosamente.");
        _unitOfWorkMock.Verify(u => u.CompleteAsync(default), Times.Once);
        _cacheMock.Verify(c => c.RemoveByPrefix("product:barcode"), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenValidationFails_ReturnsFailure()
    {
        var dto = CreateValidDto();
        dto.Name = string.Empty;
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Name", "El nombre es obligatorio.")
        });

        _validatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);

        var result = await _sut.CreateAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("El nombre es obligatorio.");
        _unitOfWorkMock.Verify(u => u.CompleteAsync(default), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenBarcodeExists_ReturnsFailure()
    {
        var dto = CreateValidDto();
        var existing = CreateProduct();
        var validationResult = new ValidationResult();

        _validatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);
        _productRepoMock.Setup(r => r.GetByBarcodeAsync(dto.Barcode, dto.CompanyId)).ReturnsAsync(existing);

        var result = await _sut.CreateAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Ya existe un producto con el código");
        _unitOfWorkMock.Verify(u => u.CompleteAsync(default), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenSkuExists_ReturnsFailure()
    {
        var dto = CreateValidDto();
        var existing = CreateProduct();
        var validationResult = new ValidationResult();

        _validatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);
        _productRepoMock.Setup(r => r.GetByBarcodeAsync(dto.Barcode, dto.CompanyId)).ReturnsAsync((Product?)null);
        _productRepoMock.Setup(r => r.GetBySkuAsync(dto.Sku, dto.CompanyId)).ReturnsAsync(existing);

        var result = await _sut.CreateAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Ya existe un producto con el SKU");
    }

    [Fact]
    public async Task CreateAsync_WhenRepositoryThrows_ReturnsFailure()
    {
        var dto = CreateValidDto();
        var product = CreateProduct();
        var validationResult = new ValidationResult();

        _validatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);
        _productRepoMock.Setup(r => r.GetByBarcodeAsync(dto.Barcode, dto.CompanyId)).ReturnsAsync((Product?)null);
        _productRepoMock.Setup(r => r.GetBySkuAsync(dto.Sku, dto.CompanyId)).ReturnsAsync((Product?)null);
        _mapperMock.Setup(m => m.Map<Product>(dto)).Returns(product);
        _productRepoMock.Setup(r => r.AddAsync(product)).ThrowsAsync(new Exception("DB error"));

        var result = await _sut.CreateAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Error al crear producto.");
    }

    // ========================
    // GetByBarcodeAsync
    // ========================

    [Fact]
    public async Task GetByBarcodeAsync_WhenFound_ReturnsSuccess()
    {
        var barcode = "123456789";
        var companyId = 1;
        var product = CreateProduct();
        var dto = CreateValidDto();
        var cacheKey = $"product:barcode:{companyId}:{barcode}";

        _cacheMock
            .Setup(c => c.GetOrCreateAsync(cacheKey, It.IsAny<Func<Task<ProductDto?>>>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync(dto);

        var result = await _sut.GetByBarcodeAsync(barcode, companyId);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(dto);
    }

    [Fact]
    public async Task GetByBarcodeAsync_WhenBarcodeEmpty_ReturnsFailure()
    {
        var result = await _sut.GetByBarcodeAsync("", 1);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("El código de barras es requerido.");
    }

    [Fact]
    public async Task GetByBarcodeAsync_WhenNotFound_ReturnsFailure()
    {
        var barcode = "999999";
        var companyId = 1;
        var cacheKey = $"product:barcode:{companyId}:{barcode}";

        _cacheMock
            .Setup(c => c.GetOrCreateAsync(cacheKey, It.IsAny<Func<Task<ProductDto?>>>(), It.IsAny<TimeSpan>()))
            .ReturnsAsync((ProductDto?)null);

        var result = await _sut.GetByBarcodeAsync(barcode, companyId);

        result.IsSuccess.Should().BeFalse();
    }

    // ========================
    // UpdateAsync
    // ========================

    [Fact]
    public async Task UpdateAsync_WhenValid_ReturnsSuccess()
    {
        var dto = CreateValidDto();
        var existing = CreateProduct();
        var validationResult = new ValidationResult();

        _validatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);
        _productRepoMock.Setup(r => r.GetByIdAsync(dto.Id)).ReturnsAsync(existing);
        _mapperMock.Setup(m => m.Map(dto, existing));
        _productRepoMock.Setup(r => r.Update(existing));
        _unitOfWorkMock.Setup(u => u.CompleteAsync(default)).ReturnsAsync(1);
        _cacheMock.Setup(c => c.RemoveByPrefix("product:barcode"));
        _mapperMock.Setup(m => m.Map<ProductDto>(existing)).Returns(dto);

        var result = await _sut.UpdateAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Producto actualizado exitosamente.");
        _cacheMock.Verify(c => c.RemoveByPrefix("product:barcode"), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsFailure()
    {
        var dto = CreateValidDto();
        var validationResult = new ValidationResult();

        _validatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);
        _productRepoMock.Setup(r => r.GetByIdAsync(dto.Id)).ReturnsAsync((Product?)null);

        var result = await _sut.UpdateAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Producto no encontrado.");
    }

    [Fact]
    public async Task UpdateAsync_WhenBarcodeChangedAndExists_ReturnsFailure()
    {
        var dto = CreateValidDto();
        dto.Barcode = "newbarcode";
        var existing = CreateProduct();
        var existingOther = CreateProduct();
        existingOther.Id = 2;
        var validationResult = new ValidationResult();

        _validatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);
        _productRepoMock.Setup(r => r.GetByIdAsync(dto.Id)).ReturnsAsync(existing);
        _productRepoMock.Setup(r => r.GetByBarcodeAsync(dto.Barcode, dto.CompanyId)).ReturnsAsync(existingOther);

        var result = await _sut.UpdateAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("Ya existe otro producto con el código");
    }

    // ========================
    // DeleteAsync
    // ========================

    [Fact]
    public async Task DeleteAsync_WhenValid_ReturnsSuccess()
    {
        var product = CreateProduct();

        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
        _productRepoMock.Setup(r => r.Update(product));
        _unitOfWorkMock.Setup(u => u.CompleteAsync(default)).ReturnsAsync(1);
        _cacheMock.Setup(c => c.RemoveByPrefix("product:barcode"));

        var result = await _sut.DeleteAsync(1);

        result.IsSuccess.Should().BeTrue();
        product.IsActive.Should().BeFalse();
        _cacheMock.Verify(c => c.RemoveByPrefix("product:barcode"), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsFailure()
    {
        _productRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);

        var result = await _sut.DeleteAsync(99);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Producto no encontrado.");
    }

    [Fact]
    public async Task DeleteAsync_WhenHasStock_ReturnsFailure()
    {
        var product = CreateProduct();
        product.CurrentStock = 10;

        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);

        var result = await _sut.DeleteAsync(1);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Contain("stock positivo");
        _unitOfWorkMock.Verify(u => u.CompleteAsync(default), Times.Never);
    }

    // ========================
    // GetByIdAsync
    // ========================

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsSuccess()
    {
        var product = CreateProduct();
        var dto = CreateValidDto();

        _productRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(product);
        _mapperMock.Setup(m => m.Map<ProductDto>(product)).Returns(dto);

        var result = await _sut.GetByIdAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(dto);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsFailure()
    {
        _productRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);

        var result = await _sut.GetByIdAsync(99);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Producto no encontrado.");
    }
}
