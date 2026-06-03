using SistemaGVP.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SistemaGVP.WPF.Views;

public partial class ProductsView : UserControl
{
    public ProductsView(ProductsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        Loaded += async (s, e) => await viewModel.LoadAsync();
    }
}
