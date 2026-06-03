using SistemaGVP.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SistemaGVP.WPF.Views;

public partial class DashboardView : UserControl
{
    public DashboardView(DashboardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (_, _) => await viewModel.LoadAsync();
    }
}
