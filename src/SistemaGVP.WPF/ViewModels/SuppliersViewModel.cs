using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.WPF.Services;

namespace SistemaGVP.WPF.ViewModels;

public partial class SuppliersViewModel : BaseViewModel
{
    private readonly ISupplierService _supplierService;
    private readonly IDialogService _dialogService;
    private readonly ICurrentUserService _currentUserService;

    [ObservableProperty]
    private ObservableCollection<SupplierDto> _items = new();

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private SupplierDto _editingSupplier = new();

    public SuppliersViewModel(
        ISupplierService supplierService,
        IDialogService dialogService,
        ICurrentUserService currentUserService,
        ILogger<BaseViewModel>? logger = null)
        : base(logger)
    {
        _supplierService = supplierService;
        _dialogService = dialogService;
        _currentUserService = currentUserService;
        ViewTitle = "Proveedores";
    }

    public override async Task LoadAsync()
    {
        await SearchAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            var filter = new PaginationFilter(1, 50, SearchTerm);
            var result = await _supplierService.GetAllAsync(filter, _currentUserService.CompanyId);
            if (result.IsSuccess && result.Data != null)
            {
                Items = new ObservableCollection<SupplierDto>(result.Data.Items ?? new List<SupplierDto>());
            }
        }, "Buscar proveedores");
    }

    [RelayCommand]
    private void NewSupplier()
    {
        EditingSupplier = new SupplierDto { IsActive = true, CompanyId = _currentUserService.CompanyId };
        ClearError();
        IsEditing = true;
    }

    [RelayCommand]
    private void EditSupplier(object? param)
    {
        if (param is not SupplierDto supplier) return;
        EditingSupplier = new SupplierDto
        {
            Id = supplier.Id,
            CompanyId = supplier.CompanyId,
            Name = supplier.Name,
            ContactName = supplier.ContactName,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Address = supplier.Address,
            TaxId = supplier.TaxId,
            IsActive = supplier.IsActive
        };
        ClearError();
        IsEditing = true;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        EditingSupplier = new SupplierDto();
        ClearError();
    }

    [RelayCommand]
    private async Task SaveSupplierAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(EditingSupplier.Name))
            {
                HasError = true;
                ErrorMessage = "El nombre del proveedor es obligatorio.";
                return;
            }

            if (EditingSupplier.Id == 0)
            {
                var result = await _supplierService.CreateAsync(EditingSupplier);
                if (!result.IsSuccess)
                {
                    HasError = true;
                    ErrorMessage = result.Message;
                    return;
                }
            }
            else
            {
                var result = await _supplierService.UpdateAsync(EditingSupplier);
                if (!result.IsSuccess)
                {
                    HasError = true;
                    ErrorMessage = result.Message;
                    return;
                }
            }

            IsEditing = false;
            EditingSupplier = new SupplierDto();
            await LoadAsync();
            await SetTemporaryStatusAsync("Proveedor guardado correctamente");
        }, "Guardar proveedor");
    }

    [RelayCommand]
    private async Task DeleteSupplierAsync(object? param)
    {
        if (param is not SupplierDto supplier) return;

        var confirm = await _dialogService.ShowConfirmAsync(
            $"¿Eliminar el proveedor '{supplier.Name}'?", "Confirmar Eliminación");
        if (!confirm) return;

        await ExecuteSafeAsync(async () =>
        {
            var result = await _supplierService.DeleteAsync(supplier.Id);
            if (result.IsSuccess)
            {
                await LoadAsync();
                await SetTemporaryStatusAsync("Proveedor eliminado correctamente");
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Message;
            }
        }, "Eliminar proveedor");
    }
}
