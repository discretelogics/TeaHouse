using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TeaTime.UI
{
    public partial class Header : UserControl
    {
        #region properties
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty<Header,ImageSource>.Register("ImageSource", null);
        public static readonly DependencyProperty ImageSizeProperty = DependencyProperty<Header, double>.Register("ImageSize", 24.0);
        public static readonly DependencyProperty TextProperty = DependencyProperty<Header, string>.Register("Text", null);
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty<Header, bool>.Register("IsSelected", false);

        public ImageSource ImageSource
        {
            get
            {
                return (ImageSource)GetValue(ImageSourceProperty);
            }
            set
            {
                SetValue(ImageSourceProperty, value);
            }
        }
        public double ImageSize
        {
            get
            {
                return (double)GetValue(ImageSizeProperty);
            }
            set
            {
                SetValue(ImageSizeProperty, value);
            }
        }
        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }
        public bool IsSelected
        {
            get
            {
                return (bool)GetValue(IsSelectedProperty);
            }
            set
            {
                SetValue(IsSelectedProperty, value);
            }
        }
        #endregion

        #region ctor
        public Header()
        {
            InitializeComponent();
        }
        #endregion
    }
}
