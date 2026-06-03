using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.Domain.Entities;
using SistemaGVP.Domain.Interfaces;

namespace SistemaGVP.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CategoryDto> _validator;
    private readonly ICacheService _cache;
    private readonly ILogger<CategoryService> _logger;

    private static readonly TimeSpan CategoriesCacheDuration = TimeSpan.FromMinutes(30);

    public CategoryService(
        IRepository<Category> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CategoryDto> validator,
        ICacheService cache,
        ILogger<CategoryService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _validator = validator;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ServiceResult<List<CategoryDto>>> GetAllAsync(int companyId)
    {
        try
        {
            var cacheKey = $"categories:company_{companyId}";

            var dtos = await _cache.GetOrCreateAsync(cacheKey, async () =>
            {
                var categories = await _repository.GetAllNoTrackingAsync();
                var filtered = categories.Where(c => c.CompanyId == companyId).ToList();
                return _mapper.Map<List<CategoryDto>>(filtered);
            }, CategoriesCacheDuration);

            return ServiceResult<List<CategoryDto>>.Success(dtos!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener categorías");
            return ServiceResult<List<CategoryDto>>.Failure("Error al cargar categorías.");
        }
    }

    public async Task<ServiceResult<CategoryDto>> CreateAsync(CategoryDto dto)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ServiceResult<CategoryDto>.Failure(errors);
            }

            var category = _mapper.Map<Category>(dto);
            await _repository.AddAsync(category);
            await _unitOfWork.CompleteAsync();

            _cache.RemoveByPrefix("categories");

            var resultDto = _mapper.Map<CategoryDto>(category);
            return ServiceResult<CategoryDto>.Success(resultDto, "Categoría creada exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear categoría");
            return ServiceResult<CategoryDto>.Failure("Error al crear categoría.");
        }
    }

    public async Task<ServiceResult<CategoryDto>> UpdateAsync(CategoryDto dto)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ServiceResult<CategoryDto>.Failure(errors);
            }

            var category = await _repository.GetByIdAsync(dto.Id);
            if (category == null)
                return ServiceResult<CategoryDto>.Failure("Categoría no encontrada.");

            _mapper.Map(dto, category);
            _repository.Update(category);
            await _unitOfWork.CompleteAsync();

            _cache.RemoveByPrefix("categories");

            var resultDto = _mapper.Map<CategoryDto>(category);
            return ServiceResult<CategoryDto>.Success(resultDto, "Categoría actualizada exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar categoría");
            return ServiceResult<CategoryDto>.Failure("Error al actualizar categoría.");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        try
        {
            var category = await _repository.GetByIdAsync(id);
            if (category == null)
                return ServiceResult<bool>.Failure("Categoría no encontrada.");

            category.IsActive = false;
            _repository.Update(category);
            await _unitOfWork.CompleteAsync();

            _cache.RemoveByPrefix("categories");

            return ServiceResult<bool>.Success(true, "Categoría desactivada exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar categoría");
            return ServiceResult<bool>.Failure("Error al desactivar categoría.");
        }
    }
}
