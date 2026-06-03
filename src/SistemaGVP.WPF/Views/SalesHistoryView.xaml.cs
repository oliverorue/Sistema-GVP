using SistemaGVP.WPF.ViewModels;
using System.Windows.Controls;

namespace SistemaGVP.WPF.Views;

public partial class SalesHistoryView : UserControl
{
    public SalesHistoryView(SalesHistoryViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        Loaded += async (s, e) => await viewModel.LoadAsync();
    }
}
