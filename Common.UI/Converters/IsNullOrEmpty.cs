using System;
using System.Linq;
using System.Collections;
using System.Windows.Data;

namespace TeaTime.Converters
{
    public class IsNullOrEmpty : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
		    if (value == null)
		        return true;
            if (value is string)
                return String.IsNullOrEmpty(value as string);
		    if (value is IEnumerable)
		        return !(value as IEnumerable).GetEnumerator().MoveNext();
		    return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
            throw new NotSupportedException();
		}
	}
}
