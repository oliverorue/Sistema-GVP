using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using MahApps.Metro.IconPacks;
using SistemaGVP.WPF.ViewModels;

namespace SistemaGVP.WPF.Views;

public partial class LoginView : UserControl
{
    private readonly LoginViewModel _viewModel;

    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        PasswordBox.KeyDown += (sender, args) =>
        {
            if (args.Key == Key.Enter)
                _viewModel.LoginCommand.Execute(null);
        };

        PasswordVisibleBox.KeyDown += (sender, args) =>
        {
            if (args.Key == Key.Enter)
                _viewModel.LoginCommand.Execute(null);
        };

        TogglePasswordBtn.Checked += (_, _) =>
        {
            TogglePasswordIcon.Kind = PackIconMaterialKind.EyeOff;
            TogglePasswordBtn.ToolTip = "Ocultar contraseña";
            PasswordVisibleBox.Focus();
        };

        TogglePasswordBtn.Unchecked += (_, _) =>
        {
            TogglePasswordIcon.Kind = PackIconMaterialKind.Eye;
            TogglePasswordBtn.ToolTip = "Mostrar contraseña";
            PasswordBox.Focus();
        };

        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(LoginViewModel.ShowPassword))
            {
                TogglePasswordIcon.Kind = _viewModel.ShowPassword
                    ? PackIconMaterialKind.EyeOff
                    : PackIconMaterialKind.Eye;
            }
        };

        UsernameBox.KeyDown += (sender, args) =>
        {
            if (args.Key == Key.Enter)
                PasswordBox.Focus();
        };

        BrandingPanel.MouseMove += OnBrandingPanelMouseMove;
    }

    private void OnBrandingPanelMouseMove(object sender, MouseEventArgs e)
    {
        var pos = e.GetPosition(BrandingPanel);
        var width = BrandingPanel.ActualWidth;
        var height = BrandingPanel.ActualHeight;

        if (width <= 0 || height <= 0) return;

        var x = (pos.X / width) - 0.5;
        var y = (pos.Y / height) - 0.5;

        Blob1.RenderTransform = new TranslateTransform(x * 20, y * 20);
        Blob2.RenderTransform = new TranslateTransform(x * -15, y * -15);
    }
}