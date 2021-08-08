using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using TeaTime.Base;
using TeaTime.Chart;
using TeaTime.Data;
using TeaTime.VSX;

namespace TeaTime.Editors
{
    class HandledException : Exception
    {
    }

    public partial class TeaFileEditor : UserControl, IDisposable, ITeaFileEditor
    {
        #region properties
        public ChartControl Chart
        {
            get
            {
                return chart;
            }
        }
        #endregion

        #region ctor, Initialize, Dispose

        public TeaFileEditor()
        {
            InitializeComponent();
        }

        public void Initialize(string fullname)
        {
            Guard.ArgumentNotNull(fullname, "fullname");

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            this.chart.Initialize(this);
            this.grid.Initialize(this);

            this.fullname = fullname;
            this.stream = new FileStream(fullname, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            this.tabControl.SelectedIndex = SettingsManager.Instance.Read("Chart", "SelectedTab", () => new Setting<int>(0)).Value;
            this.InternalUpdate(null);

            try
            {
                stopwatch.Stop();
                TeaHousePackage.Instance.WriteMessage(false, String.Format("Opened {0} ({1} items, {2}) in {3:0.###} seconds.",
                                                      Path.GetFileName(fullname), this.teaFile.Count, IOUtils.GetFileSizeString(fullname), stopwatch.Elapsed.TotalSeconds));
            }
            catch { }

            this.watcher = TeaFileWatcher.Instance.Subscribe(fullname, this);

            this.isInitialized = true;
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                if (this.watcher != null)
                    this.watcher.Dispose();

                this.chart.Dispose();

                if (this.teaFile != null)
                    this.teaFile.Dispose();

                if(this.stream != null)
                    this.stream.Dispose();

                isDisposed = true;
            }            
        }

        #endregion

        #region ITeaFileEditor Members

        public void SetTeaFileIndex(object sender, long tsi)
        {
            this.pendingSender = sender;
            this.pendingIndex = tsi;
        }

        public void Update(IChange change)
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action<IChange>(this.InternalUpdate), change);
        }
        private void InternalUpdate(IChange change)
        {
            this.ResetPendingIndex();

            if (this.teaFile != null)
                this.teaFile.Dispose();

            this.teaFile = TeaFactory.Instance.OpenReadTyped(this.stream, false);
            this.teaFile.Name = this.fullname;
            if (!this.teaFile.Description.IsSet()) return;
            if (!this.teaFile.Description.ItemDescription.IsSet()) return;

            if (this.teaFile.Description.Timescale != TeaHousePackage.Instance.Options.Timescale)
            {
                string message = "The time scale of this file does not match the current settings.\n\nTime scale in this file: {0}\nCurrent time scale: {1}.\n\nYou can change the current time scale in Tools -> Options -> TeaHouse"
                    .Formatted(this.teaFile.Description.Timescale.ToString(), TeaHousePackage.Instance.Options.Timescale);
                MessageBox.Show(message);
                this.teaFile.Dispose(); // not really necssary, since the TeaFile does not own the stream anyway
                throw new HandledException();
            }

            this.chart.IsStopped = false;
            this.grid.IsStopped = false;
            this.description.IsStopped = false;

            this.chart.Clear();
            if (this.teaFile.Description.ItemDescription.HasEventTime)
            {
                this.chart.Add(this.teaFile);
            }
            else
            {
                this.chartTab.Visibility = Visibility.Collapsed;
            }

            this.grid.TeaFile = this.teaFile;
            this.description.TeaFile = this.teaFile;
        }

        #endregion

        void ResetPendingIndex()
        {
            this.pendingSender = null;
            this.pendingIndex = null;
        }

        #region eventhandler

        private void TabControl_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isDisposed && isInitialized)
            {
                var item = (FrameworkElement)tabControl.SelectedItem;
                // we assume that the ti and the tsi are the same for now
                if (item == chartTab)
                {
                    if (this.pendingIndex.HasValue && this.pendingSender != chart)
                    {
                        chart.SetTapeIndex(chart.TapeView.Length - this.pendingIndex.Value - 1);
                        ResetPendingIndex();
                    }
                }
                else if (item == gridTab)
                {
                    if (this.pendingIndex.HasValue && this.pendingSender != grid)
                    {
                        grid.SelectedIndex = this.pendingIndex.Value;
                        ResetPendingIndex();
                    }
                }
                SettingsManager.Instance.Store("Chart", "SelectedTab", new Setting<int>(this.tabControl.SelectedIndex));
            }
        }

        private FrameworkElement lastFocusedContent;
        private void TabControl_OnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            if (TeaHousePackage.Instance.ResetFocusForProvisionalTab(this.fullname))
            {
                return;
            }
            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
                                        {
                                            UpdateLayout();
                                            var tabItem = (TabItem)tabControl.SelectedItem;

                                            var content = (FrameworkElement)tabItem.Content;
                                            if (lastFocusedContent != content)
                                            {
                                                lastFocusedContent = content;
                                                content.Focus();
                                                Keyboard.Focus(content);
                                            }
                                        }));
            e.Handled = true;
        }

        #endregion

        #region fields
                
        bool isDisposed;
        bool isInitialized;

        object pendingSender;
        long? pendingIndex;
        
        Stream stream;
        IDisposable watcher;
        string fullname;
        ITeaFile teaFile;

        #endregion
    }
}
