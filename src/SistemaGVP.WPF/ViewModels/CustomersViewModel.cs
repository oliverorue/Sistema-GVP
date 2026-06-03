using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.WPF.Services;

namespace SistemaGVP.WPF.ViewModels;

public partial class CustomersViewModel : BaseViewModel
{
    private readonly ICustomerService _customerService;
    private readonly IDialogService _dialogService;
    private readonly ICurrentUserService _currentUserService;

    [ObservableProperty]
    private ObservableCollection<CustomerDto> _items = new();

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private CustomerDto _editingCustomer = new();

    public CustomersViewModel(
        ICustomerService customerService,
        IDialogService dialogService,
        ICurrentUserService currentUserService,
        ILogger<BaseViewModel>? logger = null)
        : base(logger)
    {
        _customerService = customerService;
        _dialogService = dialogService;
        _currentUserService = currentUserService;
        ViewTitle = "Clientes";
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
            var result = await _customerService.SearchAsync(SearchTerm, _currentUserService.CompanyId);
            if (result.IsSuccess)
            {
                Items = new ObservableCollection<CustomerDto>(result.Data ?? new List<CustomerDto>());
            }
        }, "Buscar clientes");
    }

    [RelayCommand]
    private void NewCustomer()
    {
        EditingCustomer = new CustomerDto { IsActive = true, CompanyId = _currentUserService.CompanyId };
        ClearError();
        IsEditing = true;
    }

    [RelayCommand]
    private void EditCustomer(object? param)
    {
        if (param is not CustomerDto customer) return;
        EditingCustomer = new CustomerDto
        {
            Id = customer.Id,
            CompanyId = customer.CompanyId,
            Name = customer.Name,
            TaxId = customer.TaxId,
            Phone = customer.Phone,
            Email = customer.Email,
            Address = customer.Address,
            CreditLimit = customer.CreditLimit,
            Balance = customer.Balance,
            IsActive = customer.IsActive
        };
        ClearError();
        IsEditing = true;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        EditingCustomer = new CustomerDto();
        ClearError();
    }

    [RelayCommand]
    private async Task SaveCustomerAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            if (string.IsNullOrWhiteSpace(EditingCustomer.Name))
            {
                HasError = true;
                ErrorMessage = "El nombre del cliente es obligatorio.";
                return;
            }

            if (EditingCustomer.Id == 0)
            {
                var result = await _customerService.CreateAsync(EditingCustomer);
                if (!result.IsSuccess)
                {
                    HasError = true;
                    ErrorMessage = result.Message;
                    return;
                }
            }
            else
            {
                var result = await _customerService.UpdateAsync(EditingCustomer);
                if (!result.IsSuccess)
                {
                    HasError = true;
                    ErrorMessage = result.Message;
                    return;
                }
            }

            IsEditing = false;
            EditingCustomer = new CustomerDto();
            await LoadAsync();
            await SetTemporaryStatusAsync("Cliente guardado correctamente");
        }, "Guardar cliente");
    }

    [RelayCommand]
    private async Task DeleteCustomerAsync(object? param)
    {
        if (param is not CustomerDto customer) return;

        var confirm = await _dialogService.ShowConfirmAsync(
            $"¿Eliminar el cliente '{customer.Name}'?", "Confirmar Eliminación");
        if (!confirm) return;

        await ExecuteSafeAsync(async () =>
        {
            var result = await _customerService.DeleteAsync(customer.Id);
            if (result.IsSuccess)
            {
                await LoadAsync();
                await SetTemporaryStatusAsync("Cliente eliminado correctamente");
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Message;
            }
        }, "Eliminar cliente");
    }
}
