using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.Domain.Entities;
using SistemaGVP.Domain.Interfaces;

namespace SistemaGVP.Application.Services;

public class SupplierService : ISupplierService
{
    private readonly IRepository<Supplier> _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<SupplierDto> _validator;
    private readonly ILogger<SupplierService> _logger;

    public SupplierService(
        IRepository<Supplier> supplierRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<SupplierDto> validator,
        ILogger<SupplierService> logger)
    {
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<SupplierDto>>> GetAllAsync(PaginationFilter filter, int companyId)
    {
        try
        {
            var allSuppliers = await _supplierRepository.GetAllAsync();
            var filtered = allSuppliers
                .Where(s => s.CompanyId == companyId)
                .AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower();
                filtered = filtered.Where(s =>
                    s.Name.ToLower().Contains(term) ||
                    (s.ContactName != null && s.ContactName.ToLower().Contains(term)) ||
                    (s.TaxId != null && s.TaxId.ToLower().Contains(term)) ||
                    (s.Phone != null && s.Phone.Contains(term)));
            }

            var totalCount = filtered.Count();

            var pagedItems = filtered
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var dtos = _mapper.Map<List<SupplierDto>>(pagedItems);

            var pagedResult = new PagedResult<SupplierDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };

            return ServiceResult<PagedResult<SupplierDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lista de proveedores");
            return ServiceResult<PagedResult<SupplierDto>>.Failure("Error al cargar proveedores.");
        }
    }

    public async Task<ServiceResult<SupplierDto>> GetByIdAsync(int id)
    {
        try
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);
            if (supplier == null)
                return ServiceResult<SupplierDto>.Failure("Proveedor no encontrado.");

            var dto = _mapper.Map<SupplierDto>(supplier);
            return ServiceResult<SupplierDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener proveedor {Id}", id);
            return ServiceResult<SupplierDto>.Failure("Error al obtener proveedor.");
        }
    }

    public async Task<ServiceResult<List<SupplierDto>>> SearchAsync(string term, int companyId)
    {
        try
        {
            var allSuppliers = await _supplierRepository.GetAllAsync();
            var lowerTerm = term.ToLower();

            var filtered = allSuppliers
                .Where(s => s.CompanyId == companyId &&
                    (s.Name.ToLower().Contains(lowerTerm) ||
                     (s.ContactName != null && s.ContactName.ToLower().Contains(lowerTerm)) ||
                     (s.TaxId != null && s.TaxId.ToLower().Contains(lowerTerm))))
                .ToList();

            var dtos = _mapper.Map<List<SupplierDto>>(filtered);
            return ServiceResult<List<SupplierDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar proveedores con término {Term}", term);
            return ServiceResult<List<SupplierDto>>.Failure("Error al buscar proveedores.");
        }
    }

    public async Task<ServiceResult<SupplierDto>> CreateAsync(SupplierDto dto)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ServiceResult<SupplierDto>.Failure(errors);
            }

            var supplier = _mapper.Map<Supplier>(dto);

            await _supplierRepository.AddAsync(supplier);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Proveedor creado: {Name}", supplier.Name);

            var resultDto = _mapper.Map<SupplierDto>(supplier);
            return ServiceResult<SupplierDto>.Success(resultDto, "Proveedor creado exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear proveedor {Name}", dto.Name);
            return ServiceResult<SupplierDto>.Failure("Error al crear proveedor.");
        }
    }

    public async Task<ServiceResult<SupplierDto>> UpdateAsync(SupplierDto dto)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ServiceResult<SupplierDto>.Failure(errors);
            }

            var supplier = await _supplierRepository.GetByIdAsync(dto.Id);
            if (supplier == null)
                return ServiceResult<SupplierDto>.Failure("Proveedor no encontrado.");

            _mapper.Map(dto, supplier);
            _supplierRepository.Update(supplier);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Proveedor actualizado: {Id} | {Name}", supplier.Id, supplier.Name);

            var resultDto = _mapper.Map<SupplierDto>(supplier);
            return ServiceResult<SupplierDto>.Success(resultDto, "Proveedor actualizado exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar proveedor {Id}", dto.Id);
            return ServiceResult<SupplierDto>.Failure("Error al actualizar proveedor.");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        try
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);
            if (supplier == null)
                return ServiceResult<bool>.Failure("Proveedor no encontrado.");

            supplier.IsActive = false;
            _supplierRepository.Update(supplier);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Proveedor desactivado: {Id} | {Name}", id, supplier.Name);

            return ServiceResult<bool>.Success(true, "Proveedor desactivado exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar proveedor {Id}", id);
            return ServiceResult<bool>.Failure("Error al desactivar proveedor.");
        }
    }
}
