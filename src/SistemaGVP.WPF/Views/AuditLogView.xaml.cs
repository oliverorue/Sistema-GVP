using SistemaGVP.WPF.ViewModels;
using System.Windows.Controls;

namespace SistemaGVP.WPF.Views;

public partial class AuditLogView : UserControl
{
    public AuditLogView(AuditLogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        Loaded += async (s, e) => await viewModel.LoadAsync();
    }
}
