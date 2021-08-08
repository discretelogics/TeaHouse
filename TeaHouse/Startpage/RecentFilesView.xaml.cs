using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TeaTime.VSX;

namespace TeaTime.StartPage
{
    public partial class RecentFilesView : ItemsControl
    {
        private readonly ObservableCollection<MruFileCommand> mruFiles = new ObservableCollection<MruFileCommand>();

        public RecentFilesView()
        {
            InitializeComponent();
        }

        private void ItemsControl_Initialized_1(object sender, System.EventArgs e)
        {
            if (TeaHousePackage.Instance != null)
            {
                mruFiles.Add(TeaHousePackage.Instance.GetRecentFiles());
                ItemsSource = mruFiles;
            }
        }

        private void itemLink_Click(object sender, RoutedEventArgs e)
        {
            var command = (MruFileCommand)((Button)sender).DataContext;
            if (!TeaHousePackage.Instance.OpenRecentFile(command))
            {
                var files = TeaHousePackage.Instance.GetRecentFiles();
                if (!files.Any(f => f.Text == command.Text))
                {
                    mruFiles.Remove(command);
                }
            }
        }
    }
}
