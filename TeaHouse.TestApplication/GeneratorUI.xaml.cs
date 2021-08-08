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

namespace TeaHouse.TestApplication
{
    /// <summary>
    /// Interaction logic for Generator.xaml
    /// </summary>
    public partial class GeneratorUI : UserControl
    {
        public GeneratorUI()
        {
            InitializeComponent();
        }

        public GeneratorParameters GetParameters()
        {
            GeneratorParameters args = new GeneratorParameters();
            args.UpdatesPerMinute = int.Parse(this.txtUpdatesPerMinute.Text);
            args.ValueCount = int.Parse(this.txtValueCount.Text);
            args.VaryValueCountAndTime = cbChangeValueAndTimeRange.IsChecked.Value;
            args.Filename = txtFileName.Text;
            return args;
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            var p = this.GetParameters();
            Generator.Instance.Start(p);
            Generator.Instance.OnFileUpdate += this.OnFileUpdate;
            lblStatus.Content = "started";
        }

        void OnFileUpdate(string filename)
        {
            Dispatcher.Invoke(() =>
                {
                    lblStatus.Content = "last updated at " + DateTime.Now.ToString("HH:mm:sss.fff");
                }
            );
        }

        private void Stop(object sender, RoutedEventArgs e)
        {
            Generator.Instance.Stop();
            lblStatus.Content = "stopped";
        }
    }
}
