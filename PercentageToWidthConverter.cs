using System;
using System.Globalization;
using System.Windows.Data;

namespace ResourceMonitor.Converters
{
    public class PercentageToWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float percentage)
            {
                // Calculate actual width based on percentage
                double width = 80.0 * (Math.Min(percentage, 100) / 100.0);
                return width;
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}