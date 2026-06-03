using System.Globalization;
using System.Windows.Data;

namespace SistemaGVP.WPF.Converters;

/// <summary>
/// Retorna true si el valor no es null.
/// </summary>
public class IsNotNullConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return false;
    }
}
