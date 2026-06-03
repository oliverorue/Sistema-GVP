using SistemaGVP.WPF.ViewModels;
using Wpf.Ui.Controls;

namespace SistemaGVP.WPF.Views.Dialogs;

public partial class CustomerSelectionDialog : ContentDialog
{
    public CustomerSelectionDialog(ContentDialogHost dialogHost, SalesViewModel viewModel) : base(dialogHost)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
