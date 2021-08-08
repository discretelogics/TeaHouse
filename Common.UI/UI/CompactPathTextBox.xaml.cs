using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TeaTime.UI
{
    public partial class CompactPathTextBox : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty<CompactPathTextBox, string>.Register("Text", null, TextPropertyChanged);
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

        public static readonly DependencyProperty TextBoxStyleProperty = DependencyProperty<CompactPathTextBox, Style>.Register("TextBoxStyle", null);
        public Style TextBoxStyle
        {
            get
            {
                return (Style)GetValue(TextBoxStyleProperty);
            }
            set
            {
                SetValue(TextBoxStyleProperty, value);
            }
        }

        public CompactPathTextBox()
        {
            InitializeComponent();
        }

        private void UpdateText()
        {
            var text = Text;
            if (text == null)
            {
                textBox.Text = null;
                return;
            }

            if (!String.IsNullOrEmpty(text))
            {
                var textWidth = textBox.MeasureDesiredWidth(text);
                if (textBox.ActualWidth < textWidth && textWidth > 0)
                {
                    int length = (int)(textBox.ActualWidth / textWidth * text.Length);
                    textBox.Text = IOUtils.GetCompactPath(text, length);
                    return;
                }
            }
            textBox.Text = text;
        }

        private static void TextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (CompactPathTextBox)sender;
            control.UpdateText();
        }

        private void textBox_SizeChanged_1(object sender, SizeChangedEventArgs e)
        {
            UpdateText();
        }
    }
}
