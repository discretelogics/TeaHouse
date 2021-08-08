using System;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace TeaTime.Converters
{
    public class VisibleIfGreaterThan : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int && parameter is int)
            {
                if ((int)value > (int)parameter)
                    return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
