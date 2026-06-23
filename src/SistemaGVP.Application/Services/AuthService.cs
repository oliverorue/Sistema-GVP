using AutoMapper;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.Domain.Interfaces;

namespace SistemaGVP.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthService> _logger;

    // Almacenamiento en memoria del usuario actual (mejorar con JWT en producción)
    private UserDto? _currentUser;

    public UserDto? CurrentUser => _currentUser;
    public bool IsAuthenticated => _currentUser != null;
    public bool IsAdmin => _currentUser?.Role == "Admin";

    public AuthService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPasswordHasher passwordHasher,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<ServiceResult<UserDto>> LoginAsync(LoginDto loginDto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(loginDto.Username))
                return ServiceResult<UserDto>.Failure("El usuario es requerido.");

            if (string.IsNullOrWhiteSpace(loginDto.Password))
                return ServiceResult<UserDto>.Failure("La contraseña es requerida.");

            var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
            if (user == null)
                return ServiceResult<UserDto>.Failure("Usuario o contraseña incorrectos.");

            if (!user.IsActive)
                return ServiceResult<UserDto>.Failure("La cuenta está desactivada.");

            // Verificar contraseña
            if (!_passwordHasher.Verify(loginDto.Password, user.PasswordHash))
                return ServiceResult<UserDto>.Failure("Usuario o contraseña incorrectos.");

            // Actualizar último login
            user.LastLogin = DateTime.UtcNow;
            _userRepository.Update(user);
            await _unitOfWork.CompleteAsync();

            _currentUser = _mapper.Map<UserDto>(user);
            _logger.LogInformation("Usuario {Username} inició sesión", loginDto.Username);

            // Si el usuario debe cambiar su contraseña, devolver resultado especial
            if (user.MustChangePassword)
            {
                _logger.LogWarning("Usuario {Username} debe cambiar su contraseña", loginDto.Username);
                return ServiceResult<UserDto>.SuccessRequiresPasswordChange(
                    _currentUser,
                    "Debe cambiar su contraseña por motivos de seguridad.");
            }

            return ServiceResult<UserDto>.Success(_currentUser, $"Bienvenido {user.FullName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al iniciar sesión para {Username}", loginDto.Username);
            return ServiceResult<UserDto>.Failure("Error al iniciar sesión.");
        }
    }

    public void Logout()
    {
        if (_currentUser != null)
        {
            _logger.LogInformation("Usuario {Username} cerró sesión", _currentUser.Username);
            _currentUser = null;
        }
    }
}
