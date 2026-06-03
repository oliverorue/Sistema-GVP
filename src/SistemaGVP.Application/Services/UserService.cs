using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.Domain.Interfaces;

namespace SistemaGVP.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<UserDto> _validator;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IPasswordHasher passwordHasher,
        IValidator<UserDto> validator,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _passwordHasher = passwordHasher;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<UserDto>>> GetAllAsync(PaginationFilter filter, int companyId)
    {
        try
        {
            var allUsers = await _userRepository.GetAllAsync();
            var filtered = allUsers
                .Where(u => u.CompanyId == companyId)
                .AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                filtered = filtered.Where(u =>
                    u.Username.ToLower().Contains(term) ||
                    u.FullName.ToLower().Contains(term) ||
                    u.Email.ToLower().Contains(term));
            }

            var totalCount = filtered.Count();

            var pagedItems = filtered
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var dtos = _mapper.Map<List<UserDto>>(pagedItems);

            var pagedResult = new PagedResult<UserDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };

            return ServiceResult<PagedResult<UserDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios");
            return ServiceResult<PagedResult<UserDto>>.Failure("Error al cargar usuarios.");
        }
    }

    public async Task<ServiceResult<UserDto>> GetByIdAsync(int id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return ServiceResult<UserDto>.Failure("Usuario no encontrado.");

            var dto = _mapper.Map<UserDto>(user);
            return ServiceResult<UserDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario {Id}", id);
            return ServiceResult<UserDto>.Failure("Error al obtener usuario.");
        }
    }

    public async Task<ServiceResult<List<UserDto>>> SearchAsync(string term, int companyId)
    {
        try
        {
            var allUsers = await _userRepository.GetAllAsync();
            var lowerTerm = term.ToLower();

            var filtered = allUsers
                .Where(u => u.CompanyId == companyId &&
                    (u.Username.ToLower().Contains(lowerTerm) ||
                     u.FullName.ToLower().Contains(lowerTerm) ||
                     u.Email.ToLower().Contains(lowerTerm)))
                .ToList();

            var dtos = _mapper.Map<List<UserDto>>(filtered);
            return ServiceResult<List<UserDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar usuarios con término {Term}", term);
            return ServiceResult<List<UserDto>>.Failure("Error al buscar usuarios.");
        }
    }

    public async Task<ServiceResult<UserDto>> CreateAsync(UserDto dto)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ServiceResult<UserDto>.Failure(errors);
            }

            var existing = await _userRepository.GetByUsernameAsync(dto.Username, dto.CompanyId);
            if (existing != null)
                return ServiceResult<UserDto>.Failure("El nombre de usuario ya existe.");

            var user = _mapper.Map<Domain.Entities.User>(dto);
            user.PasswordHash = _passwordHasher.Hash(dto.Password!);

            await _userRepository.AddAsync(user);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Usuario creado: {Username}", user.Username);

            var resultDto = _mapper.Map<UserDto>(user);
            return ServiceResult<UserDto>.Success(resultDto, "Usuario creado exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear usuario");
            return ServiceResult<UserDto>.Failure("Error al crear usuario.");
        }
    }

    public async Task<ServiceResult<UserDto>> UpdateAsync(UserDto dto)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ServiceResult<UserDto>.Failure(errors);
            }

            var user = await _userRepository.GetByIdAsync(dto.Id);
            if (user == null)
                return ServiceResult<UserDto>.Failure("Usuario no encontrado.");

            _mapper.Map(dto, user);

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
            user.PasswordHash = _passwordHasher.Hash(dto.Password!);
            }

            _userRepository.Update(user);
            await _unitOfWork.CompleteAsync();

            var resultDto = _mapper.Map<UserDto>(user);
            return ServiceResult<UserDto>.Success(resultDto, "Usuario actualizado exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario");
            return ServiceResult<UserDto>.Failure("Error al actualizar usuario.");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
                return ServiceResult<bool>.Failure("Usuario no encontrado.");

            user.IsActive = false;
            _userRepository.Update(user);
            await _unitOfWork.CompleteAsync();

            return ServiceResult<bool>.Success(true, "Usuario desactivado exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar usuario");
            return ServiceResult<bool>.Failure("Error al desactivar usuario.");
        }
    }
}
