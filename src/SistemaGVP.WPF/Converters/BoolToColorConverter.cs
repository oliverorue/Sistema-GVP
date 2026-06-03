using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SistemaGVP.WPF.Converters;

/// <summary>
/// Converts a boolean to a foreground color brush.
/// Parameter "greenRed" = true → green, false → red (default).
/// Parameter "redGreen" = true → red, false → green (inverted).
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    private static readonly SolidColorBrush GreenBrush = new SolidColorBrush(Color.FromArgb(255, 46, 125, 50));
    private static readonly SolidColorBrush RedBrush = new SolidColorBrush(Color.FromArgb(255, 198, 40, 40));

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var boolValue = value is bool b && b;
        var param = parameter?.ToString()?.ToLower();

        if (param == "redgreen")
            return boolValue ? RedBrush : GreenBrush;

        // default / greenRed
        return boolValue ? GreenBrush : RedBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
