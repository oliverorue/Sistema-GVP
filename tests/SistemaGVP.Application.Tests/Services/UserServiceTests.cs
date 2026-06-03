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
using SistemaGVP.Domain.Interfaces;

namespace SistemaGVP.Application.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IValidator<UserDto>> _validatorMock;
    private readonly Mock<ILogger<UserService>> _loggerMock;
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _userRepoMock = new Mock<IUserRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _validatorMock = new Mock<IValidator<UserDto>>();
        _loggerMock = new Mock<ILogger<UserService>>();

        _sut = new UserService(
            _userRepoMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object,
            _passwordHasherMock.Object,
            _validatorMock.Object,
            _loggerMock.Object);
    }

    private static UserDto CreateValidDto() => new()
    {
        Id = 1,
        CompanyId = 1,
        Username = "testuser",
        FullName = "Test User",
        Email = "test@example.com",
        Password = "password123",
        Role = "Cashier"
    };

    private static Domain.Entities.User CreateUser() => new()
    {
        Id = 1,
        CompanyId = 1,
        Username = "testuser",
        FullName = "Test User",
        Email = "test@example.com",
        PasswordHash = "$2a$12$hashed",
        Role = Domain.Enums.UserRole.Cashier
    };

    // ========================
    // CreateAsync
    // ========================

    [Fact]
    public async Task CreateAsync_WhenValid_ReturnsSuccess()
    {
        var dto = CreateValidDto();
        var user = CreateUser();
        var validationResult = new ValidationResult();

        _validatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);
        _userRepoMock.Setup(r => r.GetByUsernameAsync(dto.Username, dto.CompanyId))
            .ReturnsAsync((Domain.Entities.User?)null);
        _mapperMock.Setup(m => m.Map<Domain.Entities.User>(dto)).Returns(user);
        _passwordHasherMock.Setup(p => p.Hash(dto.Password!)).Returns("$2a$12$hashed");
        _userRepoMock.Setup(r => r.AddAsync(user)).ReturnsAsync(user);
        _unitOfWorkMock.Setup(u => u.CompleteAsync(default)).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<UserDto>(user)).Returns(dto);

        var result = await _sut.CreateAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(dto);
        result.Message.Should().Be("Usuario creado exitosamente.");
        user.PasswordHash.Should().Be("$2a$12$hashed");
        _passwordHasherMock.Verify(p => p.Hash(dto.Password!), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenValidationFails_ReturnsFailure()
    {
        var dto = CreateValidDto();
        dto.Username = "ab";
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Username", "El nombre de usuario debe tener al menos 3 caracteres.")
        });

        _validatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);

        var result = await _sut.CreateAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("El nombre de usuario debe tener al menos 3 caracteres.");
        _unitOfWorkMock.Verify(u => u.CompleteAsync(default), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenDuplicateUsername_ReturnsFailure()
    {
        var dto = CreateValidDto();
        var existingUser = CreateUser();
        var validationResult = new ValidationResult();

        _validatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);
        _userRepoMock.Setup(r => r.GetByUsernameAsync(dto.Username, dto.CompanyId))
            .ReturnsAsync(existingUser);

        var result = await _sut.CreateAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("El nombre de usuario ya existe.");
        _unitOfWorkMock.Verify(u => u.CompleteAsync(default), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenRepositoryThrows_ReturnsFailure()
    {
        var dto = CreateValidDto();
        var user = CreateUser();
        var validationResult = new ValidationResult();

        _validatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);
        _userRepoMock.Setup(r => r.GetByUsernameAsync(dto.Username, dto.CompanyId))
            .ReturnsAsync((Domain.Entities.User?)null);
        _mapperMock.Setup(m => m.Map<Domain.Entities.User>(dto)).Returns(user);
        _passwordHasherMock.Setup(p => p.Hash(dto.Password!)).Returns("hashed");
        _userRepoMock.Setup(r => r.AddAsync(user)).ThrowsAsync(new Exception("DB failure"));

        var result = await _sut.CreateAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Error al crear usuario.");
    }

    [Fact]
    public async Task CreateAsync_HashesPasswordCorrectly()
    {
        var dto = CreateValidDto();
        dto.Password = "SecurePass99!";
        var user = CreateUser();
        var validationResult = new ValidationResult();

        _validatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);
        _userRepoMock.Setup(r => r.GetByUsernameAsync(dto.Username, dto.CompanyId)).ReturnsAsync((Domain.Entities.User?)null);
        _mapperMock.Setup(m => m.Map<Domain.Entities.User>(dto)).Returns(user);
        _passwordHasherMock.Setup(p => p.Hash("SecurePass99!")).Returns("$2a$12$customhash");
        _userRepoMock.Setup(r => r.AddAsync(user)).ReturnsAsync(user);
        _unitOfWorkMock.Setup(u => u.CompleteAsync(default)).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<UserDto>(user)).Returns(dto);

        await _sut.CreateAsync(dto);

        _passwordHasherMock.Verify(p => p.Hash("SecurePass99!"), Times.Once);
    }

    // ========================
    // UpdateAsync
    // ========================

    [Fact]
    public async Task UpdateAsync_WhenValid_ReturnsSuccess()
    {
        var dto = CreateValidDto();
        var user = CreateUser();
        var validationResult = new ValidationResult();

        _validatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);
        _userRepoMock.Setup(r => r.GetByIdAsync(dto.Id)).ReturnsAsync(user);
        _mapperMock.Setup(m => m.Map(dto, user));
        _userRepoMock.Setup(r => r.Update(user));
        _unitOfWorkMock.Setup(u => u.CompleteAsync(default)).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<UserDto>(user)).Returns(dto);

        var result = await _sut.UpdateAsync(dto);

        result.IsSuccess.Should().BeTrue();
        result.Message.Should().Be("Usuario actualizado exitosamente.");
    }

    [Fact]
    public async Task UpdateAsync_WhenNotFound_ReturnsFailure()
    {
        var dto = CreateValidDto();
        var validationResult = new ValidationResult();

        _validatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);
        _userRepoMock.Setup(r => r.GetByIdAsync(dto.Id)).ReturnsAsync((Domain.Entities.User?)null);

        var result = await _sut.UpdateAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Usuario no encontrado.");
    }

    [Fact]
    public async Task UpdateAsync_WhenValidationFails_ReturnsFailure()
    {
        var dto = CreateValidDto();
        dto.Email = "invalid";
        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("Email", "El formato del email no es válido.")
        });

        _validatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);

        var result = await _sut.UpdateAsync(dto);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().Contain("El formato del email no es válido.");
    }

    [Fact]
    public async Task UpdateAsync_WhenPasswordProvided_RehashesPassword()
    {
        var dto = CreateValidDto();
        dto.Password = "NewPassword456";
        var user = CreateUser();
        var validationResult = new ValidationResult();

        _validatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);
        _userRepoMock.Setup(r => r.GetByIdAsync(dto.Id)).ReturnsAsync(user);
        _mapperMock.Setup(m => m.Map(dto, user));
        _passwordHasherMock.Setup(p => p.Hash("NewPassword456")).Returns("$2a$12$newhash");
        _userRepoMock.Setup(r => r.Update(user));
        _unitOfWorkMock.Setup(u => u.CompleteAsync(default)).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<UserDto>(user)).Returns(dto);

        await _sut.UpdateAsync(dto);

        _passwordHasherMock.Verify(p => p.Hash("NewPassword456"), Times.Once);
        user.PasswordHash.Should().Be("$2a$12$newhash");
    }

    [Fact]
    public async Task UpdateAsync_WhenPasswordNotProvided_KeepsOriginalHash()
    {
        var dto = CreateValidDto();
        dto.Password = null;
        var user = CreateUser();
        var originalHash = user.PasswordHash;
        var validationResult = new ValidationResult();

        _validatorMock.Setup(v => v.ValidateAsync(dto, default)).ReturnsAsync(validationResult);
        _userRepoMock.Setup(r => r.GetByIdAsync(dto.Id)).ReturnsAsync(user);
        _mapperMock.Setup(m => m.Map(dto, user));
        _userRepoMock.Setup(r => r.Update(user));
        _unitOfWorkMock.Setup(u => u.CompleteAsync(default)).ReturnsAsync(1);
        _mapperMock.Setup(m => m.Map<UserDto>(user)).Returns(dto);

        await _sut.UpdateAsync(dto);

        _passwordHasherMock.Verify(p => p.Hash(It.IsAny<string>()), Times.Never);
        user.PasswordHash.Should().Be(originalHash);
    }

    // ========================
    // DeleteAsync
    // ========================

    [Fact]
    public async Task DeleteAsync_WhenValid_ReturnsSuccess()
    {
        var user = CreateUser();

        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _userRepoMock.Setup(r => r.Update(user));
        _unitOfWorkMock.Setup(u => u.CompleteAsync(default)).ReturnsAsync(1);

        var result = await _sut.DeleteAsync(1);

        result.IsSuccess.Should().BeTrue();
        user.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_ReturnsFailure()
    {
        _userRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Domain.Entities.User?)null);

        var result = await _sut.DeleteAsync(99);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Usuario no encontrado.");
    }

    // ========================
    // GetByIdAsync
    // ========================

    [Fact]
    public async Task GetByIdAsync_WhenFound_ReturnsSuccess()
    {
        var user = CreateUser();
        var dto = CreateValidDto();

        _userRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
        _mapperMock.Setup(m => m.Map<UserDto>(user)).Returns(dto);

        var result = await _sut.GetByIdAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(dto);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotFound_ReturnsFailure()
    {
        _userRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Domain.Entities.User?)null);

        var result = await _sut.GetByIdAsync(99);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Usuario no encontrado.");
    }
}
