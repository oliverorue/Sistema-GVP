using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SistemaGVP.WPF.Converters;

/// <summary>
/// Convierte bool a Visibility. Soporta parámetro "Invert" para invertir.
/// true → Visible, false → Collapsed (default).
/// "Invert" → true → Collapsed, false → Visible.
/// </summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var boolValue = value is bool b && b;
        var invert = parameter?.ToString()?.ToLower() == "invert";
        var result = invert ? !boolValue : boolValue;
        return result ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility v && v == Visibility.Visible;
    }
}
