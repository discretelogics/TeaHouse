using System;
using System.Windows.Data;

namespace TeaTime.Converters
{
    public class IsNullOrWhiteSpace : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            return String.IsNullOrWhiteSpace(value as string);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            throw new NotSupportedException();
		}
	}
}
