using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.Domain.Interfaces;

namespace SistemaGVP.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IRepository<Domain.Entities.Customer> _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IValidator<CustomerDto> _validator;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        IRepository<Domain.Entities.Customer> repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IValidator<CustomerDto> validator,
        ILogger<CustomerService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _validator = validator;
        _logger = logger;
    }

    public async Task<ServiceResult<List<CustomerDto>>> SearchAsync(string searchTerm, int companyId)
    {
        try
        {
            var all = await _repository.GetAllAsync();
            var filtered = all.Where(c => c.CompanyId == companyId && c.IsActive);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                filtered = filtered.Where(c =>
                    c.Name.ToLower().Contains(term) ||
                    (c.TaxId != null && c.TaxId.Contains(term)) ||
                    (c.Phone != null && c.Phone.Contains(term)));
            }

            var dtos = _mapper.Map<List<CustomerDto>>(filtered.Take(50).ToList());
            return ServiceResult<List<CustomerDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar clientes");
            return ServiceResult<List<CustomerDto>>.Failure("Error al buscar clientes.");
        }
    }

    public async Task<ServiceResult<CustomerDto>> GetByIdAsync(int id)
    {
        try
        {
            var customer = await _repository.GetByIdAsync(id);
            if (customer == null)
                return ServiceResult<CustomerDto>.Failure("Cliente no encontrado.");

            var dto = _mapper.Map<CustomerDto>(customer);
            return ServiceResult<CustomerDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener cliente {Id}", id);
            return ServiceResult<CustomerDto>.Failure("Error al obtener cliente.");
        }
    }

    public async Task<ServiceResult<CustomerDto>> CreateAsync(CustomerDto dto)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ServiceResult<CustomerDto>.Failure(errors);
            }

            var customer = _mapper.Map<Domain.Entities.Customer>(dto);
            await _repository.AddAsync(customer);
            await _unitOfWork.CompleteAsync();

            var resultDto = _mapper.Map<CustomerDto>(customer);
            return ServiceResult<CustomerDto>.Success(resultDto, "Cliente creado exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear cliente");
            return ServiceResult<CustomerDto>.Failure("Error al crear cliente.");
        }
    }

    public async Task<ServiceResult<CustomerDto>> UpdateAsync(CustomerDto dto)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(dto);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return ServiceResult<CustomerDto>.Failure(errors);
            }

            var customer = await _repository.GetByIdAsync(dto.Id);
            if (customer == null)
                return ServiceResult<CustomerDto>.Failure("Cliente no encontrado.");

            _mapper.Map(dto, customer);
            _repository.Update(customer);
            await _unitOfWork.CompleteAsync();

            var resultDto = _mapper.Map<CustomerDto>(customer);
            return ServiceResult<CustomerDto>.Success(resultDto, "Cliente actualizado exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cliente");
            return ServiceResult<CustomerDto>.Failure("Error al actualizar cliente.");
        }
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        try
        {
            var customer = await _repository.GetByIdAsync(id);
            if (customer == null)
                return ServiceResult<bool>.Failure("Cliente no encontrado.");

            customer.IsActive = false;
            _repository.Update(customer);
            await _unitOfWork.CompleteAsync();

            return ServiceResult<bool>.Success(true, "Cliente desactivado exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar cliente");
            return ServiceResult<bool>.Failure("Error al desactivar cliente.");
        }
    }

    public async Task<ServiceResult<CustomerDto>> RegisterPaymentAsync(int customerId, decimal amount, string? notes)
    {
        try
        {
            if (amount <= 0)
                return ServiceResult<CustomerDto>.Failure("El monto del pago debe ser mayor a cero.");

            var customer = await _repository.GetByIdAsync(customerId);
            if (customer == null)
                return ServiceResult<CustomerDto>.Failure("Cliente no encontrado.");

            if (amount > customer.Balance)
                return ServiceResult<CustomerDto>.Failure(
                    $"El monto ingresado ({amount:N0}) supera el saldo pendiente ({customer.Balance:N0}).");

            customer.Balance -= amount;
            _repository.Update(customer);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Pago registrado para cliente #{Id} {Name}: -{Amount:N0}, nuevo saldo = {Balance:N0}{Notes}",
                customerId, customer.Name, amount, customer.Balance,
                !string.IsNullOrWhiteSpace(notes) ? $", notas: {notes}" : "");

            var dto = _mapper.Map<CustomerDto>(customer);
            return ServiceResult<CustomerDto>.Success(dto, $"Pago de {amount:N0} registrado. Saldo restante: {customer.Balance:N0}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar pago para cliente {Id}", customerId);
            return ServiceResult<CustomerDto>.Failure("Error al registrar el pago.");
        }
    }
}
