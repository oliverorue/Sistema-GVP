using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Security;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.Common;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;

namespace SistemaGVP.WPF.ViewModels;

public partial class UsersViewModel : BaseViewModel
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    [ObservableProperty]
    private ObservableCollection<UserDto> _items = new();

    [ObservableProperty]
    private string _searchTerm = string.Empty;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private UserDto? _editingUser;

    [ObservableProperty]
    private bool _isNew;

    [ObservableProperty]
    private string _editingPassword = string.Empty;

    [ObservableProperty]
    private SecureString? _secureEditingPassword;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private string _paginationText = "Página 1 de 1";

    [ObservableProperty]
    private bool _hasPreviousPage;

    [ObservableProperty]
    private bool _hasNextPage;

    private const int PageSize = 30;

    public static List<string> RoleOptions { get; } = new() { "Admin", "Cashier", "Supervisor" };

    public UsersViewModel(
        IUserService userService,
        ICurrentUserService currentUserService,
        ILogger<BaseViewModel>? logger = null)
        : base(logger)
    {
        _userService = userService;
        _currentUserService = currentUserService;
        ViewTitle = "Usuarios";
    }

    public override async Task LoadAsync()
    {
        await LoadPageAsync(1);
    }

    private async Task LoadPageAsync(int page)
    {
        await ExecuteSafeAsync(async () =>
        {
            var companyId = _currentUserService.CompanyId;
            var filter = new PaginationFilter(page, PageSize);
            var result = await _userService.GetAllAsync(filter, companyId);

            if (result.IsSuccess && result.Data is not null)
            {
                Items = new ObservableCollection<UserDto>(result.Data.Items);
                CurrentPage = result.Data.PageNumber;
                TotalPages = result.Data.TotalPages;
                PaginationText = $"Página {CurrentPage} de {TotalPages}";
                HasPreviousPage = CurrentPage > 1;
                HasNextPage = CurrentPage < TotalPages;
            }
        }, "Cargar usuarios");
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            var companyId = _currentUserService.CompanyId;
            if (string.IsNullOrWhiteSpace(SearchTerm))
            {
                await LoadPageAsync(1);
                return;
            }
            var result = await _userService.SearchAsync(SearchTerm, companyId);
            if (result.IsSuccess && result.Data is not null)
            {
                Items = new ObservableCollection<UserDto>(result.Data);
                PaginationText = $"{result.Data.Count} resultados";
                HasPreviousPage = false;
                HasNextPage = false;
            }
        }, "Buscar usuarios");
    }

    [RelayCommand]
    private void NewUser()
    {
        EditingUser = new UserDto
        {
            CompanyId = _currentUserService.CompanyId,
            IsActive = true,
            Role = "Cashier",
            MustChangePassword = true
        };
        EditingPassword = string.Empty;
        ClearSecurePassword();
        IsNew = true;
        IsEditing = true;
        ClearError();
    }

    [RelayCommand]
    private void EditUser(object? param)
    {
        if (param is not UserDto user) return;
        EditingUser = new UserDto
        {
            Id = user.Id,
            CompanyId = user.CompanyId,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            MustChangePassword = user.MustChangePassword
        };
        EditingPassword = string.Empty;
        ClearSecurePassword();
        IsNew = false;
        IsEditing = true;
        ClearError();
    }

    [RelayCommand]
    private async Task SaveUserAsync()
    {
        if (EditingUser is null) return;
        await ExecuteSafeAsync(async () =>
        {
            var plainPassword = GetPlainEditingPassword();

            if (!string.IsNullOrWhiteSpace(plainPassword))
                EditingUser.Password = plainPassword;
            else if (!IsNew)
                EditingUser.Password = null;

            if (IsNew)
            {
                var result = await _userService.CreateAsync(EditingUser);
                if (result.IsSuccess)
                {
                    await SetTemporaryStatusAsync("Usuario creado exitosamente");
                    IsEditing = false;
                    await LoadPageAsync(CurrentPage);
                }
                else
                {
                    ErrorMessage = result.Message ?? "Error al crear el usuario";
                }
            }
            else
            {
                var result = await _userService.UpdateAsync(EditingUser);
                if (result.IsSuccess)
                {
                    await SetTemporaryStatusAsync("Usuario actualizado exitosamente");
                    IsEditing = false;
                    await LoadPageAsync(CurrentPage);
                }
                else
                {
                    ErrorMessage = result.Message ?? "Error al actualizar el usuario";
                }
            }

            ClearEditingPassword();
        }, IsNew ? "Crear usuario" : "Actualizar usuario");
    }

    [RelayCommand]
    private async Task DeleteUserAsync(object? param)
    {
        if (param is not UserDto user) return;
        await ExecuteSafeAsync(async () =>
        {
            var result = await _userService.DeleteAsync(user.Id);
            if (result.IsSuccess)
            {
                await SetTemporaryStatusAsync("Usuario eliminado exitosamente");
                await LoadPageAsync(CurrentPage);
            }
            else
            {
                ErrorMessage = result.Message ?? "Error al eliminar el usuario";
            }
        }, "Eliminar usuario");
    }

    [RelayCommand]
    private void CancelEdit()
    {
        IsEditing = false;
        EditingUser = null;
        EditingPassword = string.Empty;
        ClearSecurePassword();
        ClearError();
    }

    private string GetPlainEditingPassword()
    {
        if (SecureEditingPassword is { Length: > 0 })
        {
            var ptr = Marshal.SecureStringToBSTR(SecureEditingPassword);
            try
            {
                return Marshal.PtrToStringBSTR(ptr) ?? string.Empty;
            }
            finally
            {
                Marshal.ZeroFreeBSTR(ptr);
            }
        }
        return EditingPassword;
    }

    private void ClearSecurePassword()
    {
        SecureEditingPassword?.Dispose();
        SecureEditingPassword = null;
    }

    private void ClearEditingPassword()
    {
        EditingPassword = string.Empty;
        ClearSecurePassword();
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (HasNextPage) await LoadPageAsync(CurrentPage + 1);
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (HasPreviousPage) await LoadPageAsync(CurrentPage - 1);
    }
}
