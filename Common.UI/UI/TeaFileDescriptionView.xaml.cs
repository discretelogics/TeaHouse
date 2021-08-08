using System.Windows;
using System.Windows.Controls;
using TeaTime.Data;

namespace TeaTime
{
    public partial class TeaFileDescriptionView : UserControl
    {
        public static readonly DependencyProperty IsStoppedProperty = DependencyProperty<TeaFileDescriptionView, bool>.Register("IsStopped", false);
        public bool IsStopped
        {
            get { return (bool)GetValue(IsStoppedProperty); }
            set { SetValue(IsStoppedProperty, value); }
        }

        public TeaFileDescriptionView()
        {
            InitializeComponent();
        }

        public ITeaFile TeaFile
        {
            get { return this.DataContext as ITeaFile; }
            set
            {
                var safeTeaFile = new SafeTeaFileAccessor(value);
                safeTeaFile.DataAccessFailed += (sender, exception) => IsStopped = true;
                this.DataContext = safeTeaFile;
            }
        }
    }
}
