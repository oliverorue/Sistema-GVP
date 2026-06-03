using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MahApps.Metro.IconPacks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SistemaGVP.Application.DTOs;
using SistemaGVP.Application.Interfaces;
using SistemaGVP.Domain.Entities;
using SistemaGVP.Domain.Enums;
using SistemaGVP.Infrastructure.Services;
using SistemaGVP.WPF.Messages;
using SistemaGVP.WPF.Services;
using SistemaGVP.WPF.Views;
using System;
using System.Windows;
using System.Windows.Controls;
using WpfUi = Wpf.Ui;

namespace SistemaGVP.WPF.ViewModels;

public partial class MainWindowViewModel : BaseViewModel
{
    private readonly Services.INavigationService _navigationService;
    private readonly IAuthService _authService;
    private readonly Services.ThemeService _themeService;
    private readonly WpfUi.IThemeService _wpfUiThemeService;
    private readonly IServiceProvider _serviceProvider;
    private readonly new ILogger<MainWindowViewModel> _logger;

    private IServiceScope? _currentLoginScope;

    [ObservableProperty]
    private string _activeModule = "Dashboard";

    [ObservableProperty]
    private bool _isLoggedIn;

    [ObservableProperty]
    private string _userDisplayName = string.Empty;

    [ObservableProperty]
    private bool _isAdmin;

    [ObservableProperty]
    private bool _isCashier;

    [ObservableProperty]
    private string _currentModuleName = "POS - Sistema de Ventas";

    [ObservableProperty]
    private string _userRole = "Cajero";

    [ObservableProperty]
    private string _userInitials = "U";

    [ObservableProperty]
    private PackIconMaterialKind _themeIconKind = PackIconMaterialKind.WeatherNight;

    [ObservableProperty]
    private string _themeTooltip = "Cambiar a tema oscuro";

    public ContentControl? LoginArea { get; set; }

    public bool ShowUserManagement => IsAdmin;

    public MainWindowViewModel(
        Services.INavigationService navigationService,
        IAuthService authService,
        Services.ThemeService themeService,
        WpfUi.IThemeService wpfUiThemeService,
        IServiceProvider serviceProvider,
        ILogger<MainWindowViewModel>? logger = null)
        : base(logger)
    {
        _navigationService = navigationService;
        _authService = authService;
        _themeService = themeService;
        _wpfUiThemeService = wpfUiThemeService;
        _serviceProvider = serviceProvider;
        _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<MainWindowViewModel>.Instance;

        ViewTitle = "Sistema GVP - Principal";

        WeakReferenceMessenger.Default.Register<LoginSucceededMessage>(this, (r, m) =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                HandleLoginSucceeded(m.Value);
            });
        });

        UpdateThemeIcon();
    }

    public void Initialize(ContentControl mainContentArea)
    {
        if (_navigationService is NavigationService navSvc)
            navSvc.Initialize(mainContentArea);

        ShowLoginView();
    }

    public static string ExtractInitials(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "U";

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
            return parts[0].Substring(0, 1).ToUpperInvariant();

        return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
    }

    private void ShowLoginView()
    {
        if (LoginArea == null) return;

        _currentLoginScope?.Dispose();
        _currentLoginScope = _serviceProvider.CreateScope();
        var loginViewModel = _currentLoginScope.ServiceProvider.GetRequiredService<LoginViewModel>();
        var loginView = new LoginView(loginViewModel);

        LoginArea.Content = loginView;
    }

    private void HandleLoginSucceeded(UserDto currentUserDto)
    {
        _logger.LogInformation("Login exitoso para usuario: {Username}, Role={Role}", currentUserDto.Username, currentUserDto.Role);

        var currentUserService = _serviceProvider.GetRequiredService<ICurrentUserService>();
        var user = new User
        {
            Id = currentUserDto.Id,
            CompanyId = currentUserDto.CompanyId,
            Username = currentUserDto.Username,
            FullName = currentUserDto.FullName,
            Email = currentUserDto.Email,
            Role = currentUserDto.Role == "Admin" ? Domain.Enums.UserRole.Admin : Domain.Enums.UserRole.Cashier,
            IsActive = currentUserDto.IsActive
        };
        currentUserService.SetCurrentUser(user);

        UserDisplayName = currentUserDto.FullName;
        IsAdmin = currentUserDto.Role == "Admin";
        IsCashier = currentUserDto.Role != "Admin";
        UserRole = currentUserDto.Role == "Admin" ? "Administrador" : "Cajero";
        UserInitials = ExtractInitials(currentUserDto.FullName);

        IsLoggedIn = true;

        _ = NavigateToDashboardAsync();
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        IsLoggedIn = false;

        _currentLoginScope?.Dispose();
        _currentLoginScope = null;
        ShowLoginView();

        await Task.CompletedTask;
    }

    [RelayCommand]
    private void ToggleTheme()
    {
        _themeService.ToggleTheme();
        var isDark = _themeService.IsDarkTheme;
        _wpfUiThemeService.SetTheme(isDark ? Wpf.Ui.Appearance.ApplicationTheme.Dark : Wpf.Ui.Appearance.ApplicationTheme.Light);
        UpdateThemeIcon();
    }

    private void UpdateThemeIcon()
    {
        ThemeIconKind = _themeService.IsDarkTheme
            ? PackIconMaterialKind.WhiteBalanceSunny
            : PackIconMaterialKind.WeatherNight;
        ThemeTooltip = _themeService.GetThemeTooltip();
    }

    private async Task NavigateToDashboardAsync()
    {
        try
        {
            await _navigationService.NavigateTo<DashboardView>();
            CurrentModuleName = "Dashboard";
            ActiveModule = "Dashboard";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al navegar al Dashboard después del login");
        }
    }

    [RelayCommand]
    private async Task NavigateToDashboard()
    {
        await _navigationService.NavigateTo<DashboardView>();
        CurrentModuleName = "Dashboard";
        ActiveModule = "Dashboard";
    }

    [RelayCommand]
    private async Task NavigateToSales()
    {
        await _navigationService.NavigateTo<SalesView>();
        CurrentModuleName = "Punto de Venta";
        ActiveModule = "Sales";
    }

    [RelayCommand]
    private async Task NavigateToSalesHistory()
    {
        await _navigationService.NavigateTo<SalesHistoryView>();
        CurrentModuleName = "Historial de Ventas";
        ActiveModule = "SalesHistory";
    }

    [RelayCommand]
    private async Task NavigateToProducts()
    {
        await _navigationService.NavigateTo<ProductsView>();
        CurrentModuleName = "Productos";
        ActiveModule = "Products";
    }

    [RelayCommand]
    private async Task NavigateToCategories()
    {
        await _navigationService.NavigateTo<CategoriesView>();
        CurrentModuleName = "Categorías";
        ActiveModule = "Categories";
    }

    [RelayCommand]
    private async Task NavigateToCustomers()
    {
        await _navigationService.NavigateTo<CustomersView>();
        CurrentModuleName = "Clientes";
        ActiveModule = "Customers";
    }

    [RelayCommand]
    private async Task NavigateToInventory()
    {
        await _navigationService.NavigateTo<InventoryView>();
        CurrentModuleName = "Inventario";
        ActiveModule = "Inventory";
    }

    [RelayCommand]
    private async Task NavigateToReports()
    {
        await _navigationService.NavigateTo<ReportsView>();
        CurrentModuleName = "Reportes";
        ActiveModule = "Reports";
    }

    [RelayCommand]
    private async Task NavigateToSuppliers()
    {
        await _navigationService.NavigateTo<SuppliersView>();
        CurrentModuleName = "Proveedores";
        ActiveModule = "Suppliers";
    }

    [RelayCommand]
    private async Task NavigateToUsers()
    {
        await _navigationService.NavigateTo<UsersView>();
        CurrentModuleName = "Usuarios";
        ActiveModule = "Users";
    }

    [RelayCommand]
    private async Task NavigateToSettings()
    {
        await _navigationService.NavigateTo<SettingsView>();
        CurrentModuleName = "Configuración";
        ActiveModule = "Settings";
    }

    [RelayCommand]
    private async Task NavigateToBackup()
    {
        await _navigationService.NavigateTo<BackupView>();
        CurrentModuleName = "Backup y Restauración";
        ActiveModule = "Backup";
    }

    [RelayCommand]
    private async Task NavigateToAuditLog()
    {
        await _navigationService.NavigateTo<AuditLogView>();
        CurrentModuleName = "Auditoría";
        ActiveModule = "AuditLog";
    }

    partial void OnIsAdminChanged(bool value)
    {
        OnPropertyChanged(nameof(ShowUserManagement));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            WeakReferenceMessenger.Default.Unregister<LoginSucceededMessage>(this);
            _currentLoginScope?.Dispose();
        }
        base.Dispose(disposing);
    }
}
