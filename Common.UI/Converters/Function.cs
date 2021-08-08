using System;
using System.Globalization;
using System.Windows.Data;

namespace TeaTime.Converters
{
	public class Function<TSource, TTarget> : IValueConverter
	{
		private readonly Func<TTarget, TSource> convertBack;
		private readonly Func<TSource, TTarget> func;

		public Function(Func<TSource, TTarget> func, Func<TTarget, TSource> convertBack)
		{
			this.func = func;
			this.convertBack = convertBack;
		}

		#region IValueConverter Members

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return func((TSource) value);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return convertBack((TTarget) value);
		}

		#endregion
	}
}