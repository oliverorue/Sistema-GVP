using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SistemaGVP.Infrastructure;
using SistemaGVP.WPF.Services;
using SistemaGVP.WPF.ViewModels;
using SistemaGVP.WPF.Views;
using System.IO;
using System.Windows;
using WpfUi = Wpf.Ui;
using Wpf.Ui.Appearance;

namespace SistemaGVP.WPF;

public partial class App : System.Windows.Application
{
    private IServiceProvider? _serviceProvider;
    private GlobalExceptionHandler? _globalExceptionHandler;

    public App()
    {
        InitializeComponent();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            var configuration = builder.Build();

            var services = new ServiceCollection();
            ConfigureServices(services, configuration);

            _serviceProvider = services.BuildServiceProvider();

            var logger = _serviceProvider.GetRequiredService<ILogger>();
            logger.Information("=== Sistema GVP WPF iniciado ===");

            _globalExceptionHandler = _serviceProvider.GetRequiredService<GlobalExceptionHandler>();
            _globalExceptionHandler.Attach();

            await DependencyInjection.InitializeDatabaseAsync(_serviceProvider);

            logger.Information("Creando MainWindow...");
            var mainWindow = _serviceProvider.GetRequiredService<Views.MainWindow>();
            logger.Information("MainWindow creada. Aplicando tema y mostrando...");
            SystemThemeWatcher.Watch(mainWindow);
            MainWindow = mainWindow;
            mainWindow.Show();
            logger.Information("MainWindow.Show() ejecutado. Ventana visible.");
        }
        catch (Exception ex)
        {
            try
            {
                var logger = _serviceProvider?.GetService<ILogger>();
                logger?.Error(ex, "Error al iniciar la aplicación");
            }
            catch
            {
            }

            MessageBox.Show(
                $"Error crítico al iniciar la aplicación:\n\n{ex.Message}\n\nRevise los logs para más detalles.",
                "Error de Inicio",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(configuration);

        services.AddInfrastructure(configuration);

        services.AddSingleton<GlobalExceptionHandler>();
        services.AddSingleton<Services.ThemeService>();
        services.AddSingleton<Services.INavigationService, Services.NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        services.AddSingleton<BarcodeHttpServer>();
        services.AddSingleton<BarcodeGeneratorService>();
        services.AddSingleton<QrCodeService>();

        services.AddSingleton<WpfUi.IThemeService, WpfUi.ThemeService>();
        services.AddSingleton<WpfUi.ISnackbarService, WpfUi.SnackbarService>();
        services.AddSingleton<INotificationService, NotificationService>();

        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<SalesViewModel>();
        services.AddTransient<ProductsViewModel>();
        services.AddTransient<CategoriesViewModel>();
        services.AddTransient<CustomersViewModel>();
        services.AddTransient<InventoryViewModel>();
        services.AddTransient<ReportsViewModel>();
        services.AddTransient<SuppliersViewModel>();
        services.AddTransient<UsersViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<BackupViewModel>();
        services.AddTransient<AuditLogViewModel>();
        services.AddTransient<SalesHistoryViewModel>();

        services.AddTransient<Views.MainWindow>();
        services.AddTransient<LoginView>();
        services.AddTransient<DashboardView>();
        services.AddTransient<SalesView>();
        services.AddTransient<ProductsView>();
        services.AddTransient<CategoriesView>();
        services.AddTransient<CustomersView>();
        services.AddTransient<InventoryView>();
        services.AddTransient<ReportsView>();
        services.AddTransient<SuppliersView>();
        services.AddTransient<UsersView>();
        services.AddTransient<SettingsView>();
        services.AddTransient<SalesHistoryView>();
        services.AddTransient<BackupView>();
        services.AddTransient<AuditLogView>();
    }
}
