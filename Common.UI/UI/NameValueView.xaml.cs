using System.Windows;
using System.Windows.Controls;

namespace TeaTime.UI
{
	public partial class NameValueView : UserControl
    {
        #region properties
        public static readonly DependencyProperty NameTextProperty = DependencyProperty<NameValueView, string>.Register("NameText", null);
        public static readonly DependencyProperty NameWidthProperty = DependencyProperty<NameValueView, double>.Register("NameWidth", 80.0);
        public static readonly DependencyProperty ValueTextProperty = DependencyProperty<NameValueView, string>.Register("ValueText", null);
        
        public string NameText
        {
            get
            {
                return (string)GetValue(NameTextProperty);
            }
            set
            {
                SetValue(NameTextProperty, value);
            }
        }
        public double NameWidth
        {
            get
            {
                return (double)GetValue(NameWidthProperty);
            }
            set
            {
                SetValue(NameWidthProperty, value);
            }
        }
        public string ValueText
        {
            get
            {
                return (string)GetValue(ValueTextProperty);
            }
            set
            {
                SetValue(ValueTextProperty, value);
            }
        }
        #endregion

        #region ctor
        public NameValueView()
		{
			InitializeComponent();
		}
		#endregion
    }
}
