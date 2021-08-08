using System;
using System.Windows.Data;
using System.Windows.Controls;
using TeaTime.Data;

namespace TeaTime.UI
{
    class RowToCellConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var cell = (DataGridCell)value;
            var row = (PreviewCell[])cell.DataContext;
            return row.Length > cell.Column.DisplayIndex ? row[cell.Column.DisplayIndex] : null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
