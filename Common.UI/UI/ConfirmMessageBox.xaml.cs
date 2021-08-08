using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace TeaTime.UI
{
    public partial class ConfirmMessageBox : Window
    {
        #region commands
        public static RoutedCommand NoCommand = new RoutedCommand();
        public static RoutedCommand NoToAllCommand = new RoutedCommand();
        public static RoutedCommand YesCommand = new RoutedCommand();
        public static RoutedCommand YesToAllCommand = new RoutedCommand();
        #endregion

        #region properties
        public static readonly DependencyProperty ImageProperty = DependencyProperty<ConfirmMessageBox, ImageSource>.Register("Image", null);
        public static readonly DependencyProperty TextProperty = DependencyProperty<ConfirmMessageBox, string>.Register("Text", null);

        public ImageSource Image
        {
            get
            {
                return (ImageSource)GetValue(ImageProperty);
            }
            set
            {
                SetValue(ImageProperty, value);
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

        public new ConfirmMessageBoxResult? DialogResult
        {
            get
            {
                return dialogResult;
            }
        }
        #endregion

        #region ctor
        public ConfirmMessageBox()
        {
            InitializeComponent();
        }
        #endregion

        #region public methods
        public new ConfirmMessageBoxResult? ShowDialog()
        {
            base.ShowDialog();
            return dialogResult;
        }

        public static ConfirmMessageBoxResult? Show(string text, string title, ImageSource image, ImageSource icon)
        {
            ConfirmMessageBox messageBox = new ConfirmMessageBox();
            messageBox.Text = text;
            messageBox.Title = title;
            messageBox.Image = image;
            messageBox.Icon = icon;
            return messageBox.ShowDialog();
        }
        #endregion

        #region eventhandler
        private void NoCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void NoCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            dialogResult = ConfirmMessageBoxResult.No;
            this.Close();
        }
        private void NoToAllCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void NoToAllCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            dialogResult = ConfirmMessageBoxResult.NoToAll;
            this.Close();
        }
        private void YesCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void YesCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            dialogResult = ConfirmMessageBoxResult.Yes;
            this.Close();
        }
        private void YesToAllCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void YesToAllCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            dialogResult = ConfirmMessageBoxResult.YesToAll;
            this.Close();
        }
        #endregion

        #region fields
        private ConfirmMessageBoxResult? dialogResult;
        #endregion
    }

    public enum ConfirmMessageBoxResult
    {
        No,
        NoToAll,
        Yes,
        YesToAll
    }
}
