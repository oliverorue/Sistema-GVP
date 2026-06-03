using SistemaGVP.WPF.ViewModels;
using System.Windows.Controls;

namespace SistemaGVP.WPF.Views;

public partial class SettingsView : UserControl
{
    public SettingsView(SettingsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        Loaded += async (s, e) => await viewModel.LoadAsync();
    }
}
