using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SistemaGVP.WPF.Converters;

/// <summary>
/// Convierte bool a Visibility de forma inversa.
/// true → Collapsed, false → Visible.
/// </summary>
public class InverseBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var boolValue = value is bool b && b;
        return boolValue ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility v && v != Visibility.Visible;
    }
}
