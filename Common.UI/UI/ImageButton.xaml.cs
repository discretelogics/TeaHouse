using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TeaTime.UI
{
    public partial class ImageButton : Button
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty<ImageButton, ImageSource>.Register("ImageSource", null);
        public static readonly DependencyProperty ImageHeightProperty = DependencyProperty<ImageButton, double>.Register("ImageHeight", 16);
        public static readonly DependencyProperty ImageWidthProperty = DependencyProperty<ImageButton, double>.Register("ImageWidth", 16);

        public ImageSource ImageSource
        {
            get
            {
                return (ImageSource)this.GetValue(ImageSourceProperty);
            }
            set
            {
                this.SetValue(ImageSourceProperty, value);
            }
        }
        public double ImageHeight
        {
            get
            {
                return (double)this.GetValue(ImageHeightProperty);
            }
            set
            {
                this.SetValue(ImageHeightProperty, value);
            }
        }
        public double ImageWidth
        {
            get
            {
                return (double)this.GetValue(ImageWidthProperty);
            }
            set
            {
                this.SetValue(ImageWidthProperty, value);
            }
        }

        public ImageButton()
        {
            this.InitializeComponent();
        }
    }
}
