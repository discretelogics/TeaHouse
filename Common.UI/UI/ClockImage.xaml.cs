using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TeaTime.UI
{
    public partial class ClockImage : UserControl
    {
        public static readonly DependencyProperty StrokeProperty = DependencyProperty<ClockImage, Brush>.Register("Stroke", new SolidColorBrush(Color.FromRgb(85, 85, 85)));
        public Brush Stroke
        {
            get { return (Brush)GetValue(StrokeProperty); }
            set { SetValue(StrokeProperty, value); }
        }

        public static readonly DependencyProperty FillProperty = DependencyProperty<ClockImage, Brush>.Register("Fill", new SolidColorBrush(Colors.White));
        public Brush Fill
        {
            get { return (Brush)GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public ClockImage()
        {
            InitializeComponent();
        }
    }
}
