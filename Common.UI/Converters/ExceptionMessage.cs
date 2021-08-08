using System;
using System.Windows.Data;

namespace TeaTime.Converters
{
    public class ExceptionMessage : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Exception ex = value as Exception;
            if (ex == null)
            {
                return null;
            }

            string message;
            do
            {
                message = ex.Message;
                ex = ex.InnerException;
            } while (ex != null);
            return message;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
