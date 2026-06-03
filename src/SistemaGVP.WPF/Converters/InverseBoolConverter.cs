using System.Globalization;
using System.Windows.Data;

namespace SistemaGVP.WPF.Converters;

/// <summary>
/// Invierte un valor booleano.
/// </summary>
public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && !b;
    }
}
