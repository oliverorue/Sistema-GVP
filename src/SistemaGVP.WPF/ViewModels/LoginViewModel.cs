using System.Runtime.InteropServices;
using System.Security;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.WPF.Messages;

namespace SistemaGVP.WPF.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private SecureString? _securePassword;

    [ObservableProperty]
    private bool _showPassword;

    [ObservableProperty]
    private int _companyId = 1;

    [Obsolete("Usar WeakReferenceMessenger con LoginSucceededMessage en su lugar")]
    public event EventHandler<UserDto>? LoginSucceeded;

    public LoginViewModel(IAuthService authService, ILogger<BaseViewModel>? logger = null)
        : base(logger)
    {
        _authService = authService;
        ViewTitle = "Iniciar Sesión";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        await ExecuteSafeAsync(async () =>
        {
            var plainPassword = GetPlainPassword();

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(plainPassword))
            {
                HasError = true;
                ErrorMessage = "Usuario y contraseña son requeridos";
                return;
            }

            var result = await _authService.LoginAsync(new LoginDto
            {
                Username = Username,
                Password = plainPassword,
                CompanyId = CompanyId
            });

            ClearPassword();

            if (result.IsSuccess && result.Data != null)
            {
                if (result.RequiresPasswordChange)
                {
                    _ = SetTemporaryStatusAsync("Debe cambiar su contraseña por motivos de seguridad. Vaya a Configuración > Usuarios para cambiarla.", 5000)
                        .ContinueWith(t =>
                        {
                            if (t.IsFaulted && t.Exception != null)
                                _logger?.LogError(t.Exception, "Error en SetTemporaryStatusAsync");
                        }, TaskContinuationOptions.OnlyOnFaulted);
                }

                WeakReferenceMessenger.Default.Send(new LoginSucceededMessage(result.Data));

                LoginSucceeded?.Invoke(this, result.Data);
            }
            else
            {
                HasError = true;
                ErrorMessage = result.Message;
            }
        }, "Login");
    }

    [RelayCommand]
    private void ClearFields()
    {
        Username = string.Empty;
        Password = string.Empty;
        ClearPassword();
        HasError = false;
        ErrorMessage = string.Empty;
    }

    private string GetPlainPassword()
    {
        if (SecurePassword is { Length: > 0 })
        {
            var ptr = Marshal.SecureStringToBSTR(SecurePassword);
            try
            {
                return Marshal.PtrToStringBSTR(ptr) ?? string.Empty;
            }
            finally
            {
                Marshal.ZeroFreeBSTR(ptr);
            }
        }
        return Password;
    }

    private void ClearPassword()
    {
        Password = string.Empty;
        SecurePassword?.Dispose();
        SecurePassword = null;
    }
}
