using System;
using System.Windows.Data;

namespace TeaTime.Converters
{
	public class Suffix : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            return String.Format("{0}{1}", value, parameter);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            if (value == null)
            {
                return null;
            }
            if (parameter == null)
            {
                return value;
            }
            return ((string)value).TrimEnd(((string)parameter).ToCharArray());
		}
	}
}
