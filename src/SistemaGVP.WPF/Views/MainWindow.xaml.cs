using Microsoft.Extensions.DependencyInjection;
using SistemaGVP.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui;

namespace SistemaGVP.WPF.Views;

public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;

    public MainWindow(MainWindowViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;
        DataContext = viewModel;

        _viewModel.LoginArea = LoginArea;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        _viewModel.Initialize(MainContentArea);

        var snackbarService = _serviceProvider.GetRequiredService<ISnackbarService>();
        snackbarService.SetSnackbarPresenter(SnackbarPresenter);
    }
}
