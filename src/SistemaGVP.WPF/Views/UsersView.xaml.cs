using System.Windows.Controls;
using SistemaGVP.WPF.ViewModels;

namespace SistemaGVP.WPF.Views;

public partial class UsersView : UserControl
{
    private readonly UsersViewModel _viewModel;

    public UsersView(UsersViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        Loaded += async (s, e) => await viewModel.LoadAsync();
    }
}
