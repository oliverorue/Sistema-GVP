using SistemaGVP.WPF.ViewModels;
using System.Windows.Controls;

namespace SistemaGVP.WPF.Views;

public partial class BackupView : UserControl
{
    public BackupView(BackupViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        Loaded += async (s, e) => await viewModel.LoadAsync();
    }
}
