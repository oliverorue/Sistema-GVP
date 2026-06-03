using SistemaGVP.WPF.ViewModels;
using Wpf.Ui.Controls;

namespace SistemaGVP.WPF.Views.Dialogs;

public partial class PaymentDialog : ContentDialog
{
    public PaymentDialog(ContentDialogHost dialogHost, SalesViewModel viewModel) : base(dialogHost)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
