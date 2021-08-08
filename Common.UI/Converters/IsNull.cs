using System;
using System.Windows.Data;

namespace TeaTime.Converters
{
    /// <summary>
    /// This Converter is only required, because there's a bug in WPF that does not allow Markup-extensions (e.g.:"{x:Null}") within condition-values of MultiDataTriggers
    /// </summary>
    public class IsNull : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value == null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsNotNull : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (value != null);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
