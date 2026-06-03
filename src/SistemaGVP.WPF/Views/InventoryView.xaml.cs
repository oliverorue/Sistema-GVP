using SistemaGVP.WPF.ViewModels;
using System.Windows.Controls;

namespace SistemaGVP.WPF.Views;

public partial class InventoryView : UserControl
{
    public InventoryView(InventoryViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        Loaded += async (s, e) => await viewModel.LoadAsync();
    }
}
