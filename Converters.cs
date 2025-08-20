using Microsoft.Maui.Graphics;
using System.Globalization;

namespace ServerMonitor
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isAlarm)
            {
                var baseColor = isAlarm ? Colors.DarkRed : Colors.DarkGreen;
                if (parameter is string opacityStr && float.TryParse(opacityStr, NumberStyles.Any, CultureInfo.InvariantCulture, out float opacity) && opacity >= 0 && opacity <= 1)
                {
                    return baseColor.WithAlpha(opacity);
                }
                return baseColor;
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value ? "SI" : "NO";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}