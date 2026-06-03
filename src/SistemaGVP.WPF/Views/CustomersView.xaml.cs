using SistemaGVP.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SistemaGVP.WPF.Views;

public partial class CustomersView : UserControl
{
    public CustomersView(CustomersViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        Loaded += async (s, e) => await viewModel.LoadAsync();
    }
}
