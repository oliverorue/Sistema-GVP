using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Controls;

namespace SistemaGVP.WPF.Controls;

public static class PasswordBoxHelper
{
    private static readonly DependencyProperty IsUpdatingProperty =
        DependencyProperty.RegisterAttached("IsUpdating", typeof(bool), typeof(PasswordBoxHelper));

    public static readonly DependencyProperty IsMonitoringProperty =
        DependencyProperty.RegisterAttached(
            "IsMonitoring",
            typeof(bool),
            typeof(PasswordBoxHelper),
            new PropertyMetadata(false, OnIsMonitoringChanged));

    public static readonly DependencyProperty BoundPasswordProperty =
        DependencyProperty.RegisterAttached(
            "BoundPassword",
            typeof(string),
            typeof(PasswordBoxHelper),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnBoundPasswordChanged));

    public static readonly DependencyProperty SecurePasswordProperty =
        DependencyProperty.RegisterAttached(
            "SecurePassword",
            typeof(SecureString),
            typeof(PasswordBoxHelper),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSecurePasswordChanged));

    public static bool GetIsMonitoring(DependencyObject obj) => (bool)obj.GetValue(IsMonitoringProperty);
    public static void SetIsMonitoring(DependencyObject obj, bool value) => obj.SetValue(IsMonitoringProperty, value);

    public static string GetBoundPassword(DependencyObject obj) => (string)obj.GetValue(BoundPasswordProperty);
    public static void SetBoundPassword(DependencyObject obj, string value) => obj.SetValue(BoundPasswordProperty, value);

    public static SecureString? GetSecurePassword(DependencyObject obj) => (SecureString)obj.GetValue(SecurePasswordProperty);
    public static void SetSecurePassword(DependencyObject obj, SecureString? value) => obj.SetValue(SecurePasswordProperty, value);

    public static string SecureStringToString(SecureString? secureString)
    {
        if (secureString is null || secureString.Length == 0)
            return string.Empty;

        var ptr = Marshal.SecureStringToBSTR(secureString);
        try
        {
            return Marshal.PtrToStringBSTR(ptr);
        }
        finally
        {
            Marshal.ZeroFreeBSTR(ptr);
        }
    }

    private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox pb) return;

        if ((bool)e.NewValue)
            pb.PasswordChanged += OnPasswordChanged;
        else
            pb.PasswordChanged -= OnPasswordChanged;
    }

    private static void OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is not PasswordBox pb) return;

        SetIsUpdating(pb, true);

        SetBoundPassword(pb, pb.Password);

        var copy = pb.SecurePassword?.Copy();
        var old = GetSecurePassword(pb);
        SetSecurePassword(pb, copy);
        old?.Dispose();

        SetIsUpdating(pb, false);
    }

    private static void OnBoundPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox pb || GetIsUpdating(pb)) return;

        pb.Password = e.NewValue as string ?? string.Empty;
    }

    private static void OnSecurePasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PasswordBox pb || GetIsUpdating(pb)) return;

        var secure = e.NewValue as SecureString;
        pb.Password = secure is { Length: > 0 } ? SecureStringToString(secure) : string.Empty;
    }

    private static bool GetIsUpdating(DependencyObject obj) => (bool)obj.GetValue(IsUpdatingProperty);
    private static void SetIsUpdating(DependencyObject obj, bool value) => obj.SetValue(IsUpdatingProperty, value);
}
