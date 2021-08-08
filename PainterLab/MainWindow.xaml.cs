using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TeaTime.Chart.Painters;
using TeaTime.Data;
using Microsoft.Win32;
using System.Reflection;

namespace TeaTime.PainterLab
{
    public partial class MainWindow : Window
    {
        #region commands

        #endregion

        #region ctor
        public MainWindow()
        {
            InitializeComponent();
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;
        }
        #endregion

        #region private methods
        private void OpenTs(string file)
        {
            TryCloseTs();

            try
            {
                ts = TeaFactory.Instance.OpenReadTyped(file);
                chart.Add(ts);
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format(@"Failed to open {0}.
{1}", file, ex.Message), "Cannot open file", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void TryCloseTs()
        {
            if (ts != null)
            {
                try
                {
                    chart.Clear();
                    ts.Dispose();
                    ts = null;
                }
                catch { }
            }
        }
        #endregion

        #region eventhandler
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region initialize chart
            chart.Initialize(null);
            #endregion

            #region load painters
            try
            {
                var catalog = new AggregateCatalog();
                var fileToIgnore = Path.GetFileName(Assembly.GetAssembly(typeof(PainterManager)).Location);

                foreach (string file in Directory.GetFiles(Environment.CurrentDirectory, "*.dll", SearchOption.TopDirectoryOnly))
                {
                    if (!fileToIgnore.Equals(Path.GetFileName(file), StringComparison.InvariantCultureIgnoreCase))
                    {
                        catalog.Catalogs.Add(new AssemblyCatalog(Assembly.LoadFile(file)));
                    }
                }

                var container = new CompositionContainer(catalog);
                CompositionBatch batch = new CompositionBatch();
                container.Compose(batch);

                PainterManager.Instance.ImportPainters(container);
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format(@"Failed to load Painters in {0}.
{1}", Environment.CurrentDirectory, ex.Message), "Load Painters failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            #endregion

            #region load ts
            if (String.IsNullOrEmpty(App.StartupArgs.TimeSeries))
            {
                ApplicationCommands.Open.Execute(null, this);
            }
            else
            {
                OpenTs(App.StartupArgs.TimeSeries);
            }
            #endregion
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TryCloseTs();
        }

        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(String.Format(@"Unexpected Exception in Dispatcher encountered.
{0}", e.Exception.Message), "Dispatcher UnhandledException", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void OpenCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void OpenExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Multiselect = false;
            dlg.Filter = "Tea Files (*.tea)|*.tea|All Files|*.*";
            if (dlg.ShowDialog() == true)
            {
                OpenTs(dlg.FileName);
            }
        }

        private void CloseCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }
        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region fields
        ITeaFile ts;
        #endregion
    }
}
