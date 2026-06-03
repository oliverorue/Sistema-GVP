using AutoMapper;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.Domain.Entities;
using SistemaGVP.Domain.Interfaces;

namespace SistemaGVP.Application.Services;

public class SettingsService : ISettingsService
{
    private readonly IRepository<Company> _companyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SettingsService> _logger;

    public SettingsService(
        IRepository<Company> companyRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SettingsService> logger)
    {
        _companyRepository = companyRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ServiceResult<CompanyDto>> GetCompanyAsync(int companyId)
    {
        try
        {
            var company = await _companyRepository.GetByIdAsync(companyId);
            if (company == null)
                return ServiceResult<CompanyDto>.Failure("Empresa no encontrada.");

            var dto = _mapper.Map<CompanyDto>(company);
            return ServiceResult<CompanyDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empresa {Id}", companyId);
            return ServiceResult<CompanyDto>.Failure("Error al cargar configuración de la empresa.");
        }
    }

    public async Task<ServiceResult<CompanyDto>> UpdateCompanyAsync(CompanyDto dto)
    {
        try
        {
            var company = await _companyRepository.GetByIdAsync(dto.Id);
            if (company == null)
                return ServiceResult<CompanyDto>.Failure("Empresa no encontrada.");

            _mapper.Map(dto, company);
            _companyRepository.Update(company);
            await _unitOfWork.CompleteAsync();

            _logger.LogInformation("Configuración de empresa actualizada: {Id} | {Name}", company.Id, company.Name);

            var resultDto = _mapper.Map<CompanyDto>(company);
            return ServiceResult<CompanyDto>.Success(resultDto, "Configuración actualizada exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar empresa {Id}", dto.Id);
            return ServiceResult<CompanyDto>.Failure("Error al actualizar configuración.");
        }
    }
}
