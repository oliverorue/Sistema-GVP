using System.Globalization;
using System.Windows.Data;

namespace SistemaGVP.WPF.Converters;

/// <summary>
/// Convierte int > 0 a true. Útil para habilitar botones cuando hay items.
/// Soporta parámetro numérico para comparación específica.
/// </summary>
public class IntToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var intValue = value is int i ? i : 0;

        if (parameter is string paramStr && int.TryParse(paramStr, out var compareValue))
            return intValue > compareValue;

        return intValue > 0;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is bool b && b ? 1 : 0;
    }
}
