using AutoMapper;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;

namespace SistemaGVP.WPF.ViewModels;

public partial class SettingsViewModel : BaseViewModel
{
    private readonly ISettingsService _settingsService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    [ObservableProperty]
    private CompanyDto? _editingCompany;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isSaved;

    public SettingsViewModel(
        ISettingsService settingsService,
        ICurrentUserService currentUserService,
        IMapper mapper,
        ILogger<BaseViewModel>? logger = null)
        : base(logger)
    {
        _settingsService = settingsService;
        _currentUserService = currentUserService;
        _mapper = mapper;
        ViewTitle = "Configuración de Empresa";
    }

    public override async Task LoadAsync()
    {
        IsLoading = true;
        await ExecuteSafeAsync(async () =>
        {
            var companyId = _currentUserService.CompanyId;
            var result = await _settingsService.GetCompanyAsync(companyId);
            if (result.IsSuccess && result.Data != null)
                EditingCompany = result.Data;
            else
                EditingCompany = new CompanyDto();
        }, "Cargar configuración");
        IsLoading = false;
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        if (EditingCompany is null) return;

        IsLoading = true;
        IsSaved = false;
        await ExecuteSafeAsync(async () =>
        {
            var result = await _settingsService.UpdateCompanyAsync(EditingCompany);
            if (result.IsSuccess)
            {
                IsSaved = true;
                await SetTemporaryStatusAsync("Configuración guardada exitosamente");
            }
            else
            {
                ErrorMessage = result.Message ?? "Error al guardar la configuración";
            }
        }, "Guardar configuración");
        IsLoading = false;
    }
}
