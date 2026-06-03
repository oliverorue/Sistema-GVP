using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SistemaGVP.WPF.Converters;

/// <summary>
/// Convierte un valor numérico de stock a un color:
/// - Menor a 10 → Rojo (bajo)
/// - Menor a 20 → Naranja (medio)
/// - Mayor o igual a 20 → Verde (normal)
/// </summary>
public class StockToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            if (decimalValue < 10) return new SolidColorBrush(Color.FromArgb(255, 211, 47, 47));   // Rojo
            if (decimalValue < 20) return new SolidColorBrush(Color.FromArgb(255, 245, 124, 0));  // Naranja
            return new SolidColorBrush(Color.FromArgb(255, 56, 142, 60));                          // Verde
        }

        if (value is int intValue)
        {
            if (intValue < 10) return new SolidColorBrush(Color.FromArgb(255, 211, 47, 47));
            if (intValue < 20) return new SolidColorBrush(Color.FromArgb(255, 245, 124, 0));
            return new SolidColorBrush(Color.FromArgb(255, 56, 142, 60));
        }

        return new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
