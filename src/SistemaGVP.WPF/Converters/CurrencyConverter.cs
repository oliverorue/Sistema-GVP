using System.Globalization;
using System.Windows.Data;

namespace SistemaGVP.WPF.Converters;

/// <summary>
/// Convierte valores decimales a formato moneda (Gs.).
/// </summary>
public class CurrencyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
            return $"Gs. {decimalValue:N0}";

        if (value is double doubleValue)
            return $"Gs. {doubleValue:N0}";

        if (value is int intValue)
            return $"Gs. {intValue:N0}";

        return "Gs. 0";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            var cleaned = str.Replace("Gs. ", "").Replace(",", "").Trim();
            if (decimal.TryParse(cleaned, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;
        }

        return 0m;
    }
}
