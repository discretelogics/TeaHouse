using System.Windows.Controls;
using System.Windows;

namespace TeaTime.UI
{
    public class BindableScrollViewer : ScrollViewer
    {
        public static readonly DependencyProperty HorizontalOffsetBoundProperty = DependencyProperty<BindableScrollViewer, double>.Register("HorizontalOffsetBound", 0.0, HorizontalOffsetBoundChanged);
        public static readonly DependencyProperty VerticalOffsetBoundProperty = DependencyProperty<BindableScrollViewer, double>.Register("VerticalOffsetBound", 0.0, VerticalOffsetBoundChanged);

        private static void HorizontalOffsetBoundChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var sv = (BindableScrollViewer)sender;
            sv.ScrollToHorizontalOffset((double)e.NewValue);
        }
        private static void VerticalOffsetBoundChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var sv = (BindableScrollViewer)sender;
            sv.ScrollToVerticalOffset((double)e.NewValue);
        }
    }
}
