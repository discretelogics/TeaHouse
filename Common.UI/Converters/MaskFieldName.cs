using System;
using System.Linq;
using System.Windows.Data;

namespace TeaTime.Converters
{
    public class MaskFieldName : IMultiValueConverter
	{
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length == 2)
            {
                var type = values[1] as Type;
                if (type != null && (type.IsPrimitive || type.IsEventOfPrimitive()))
                {
                    var name = values[0] as string;
                    if (name == "m_value")
                        return "Value";
                }
            }
            return values.FirstOrDefault();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
