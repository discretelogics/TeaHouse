using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.VisualStudio.Shell;
using TeaTime;
using TeaTime.Data;
using TeaTime.Special;

namespace TeaHouse.TestApplication
{
    public struct OHLCV
    {
        public double Open;
        public double High;
        public double Low;
        public double Close;
        public long Volume;

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3} {4}", this.Open, this.High, this.Low, this.Close, this.Volume);
        }
    }

    public partial class MainWindow : Window, ITeaFileEditor
    {
        public MainWindow()
        {
            InitializeComponent();

            var b = ((SolidColorBrush)this.FindResource(VsBrushes.HighlightKey));
        }

        #region TsEditor
        private void tsEditorInitialize_Click(object sender, RoutedEventArgs e)
        {
            tsEditor.Initialize("some stream name");
        }
        #endregion        

        #region Grid Wpf
        private void gridInitialize_Click(object sender, RoutedEventArgs e)
        {
            //var tf = TeaFile.OpenRead(gridFile.Text);
            //openedTss.Add(tf);
            //grid.ItemSource = tf.ItemLines;
            //grid.ItemSource = new DemoItems();
        }
        #endregion

        #region Chart
        private void chartAdd_Click(object sender, RoutedEventArgs e)
        {
            var ss = chartFile.Text.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            int index = 0;
            if (ss.Length > 1)
            {
                index = int.Parse(ss[1]);
            }
            string fileName = ss[0];
            if (!Path.HasExtension(fileName))
            {
                fileName += ".tea";
            }
            
            //TeaPlant plant = new TeaPlant("127.0.0.1", "duffy");
            //var stream = plant.GetStream(@"e:\finance\Dow30\lab\b.tea");
            ////var stream = File.OpenRead(@"E:\Finance\Dow30\lab\b.tea");
            
            //var ccs = new ChunkCachingStream(262144, stream);
            //var ts = TeaFactory.Instance.OpenReadTyped(ccs, true);

            var ts = TeaFactory.Instance.OpenReadTyped(@"E:\Finance\Dow30\lab\b.tea");
            
            //var stream = new DiagnosticStream(fs);
            //TeaFile<Event<OHLCV>> ts = TeaFile<Event<OHLCV>>.OpenRead(stream);
            //TeaFile<Event<OHLCV>>.getTime = (eohlcv) => eohlcv.Time;

            chart.Add(ts);
        }

        private void chartInitialize_Click(object sender, RoutedEventArgs e)
        {
            chartTapeIndex.ValueText = null;
            chartSelectedTss.ValueText = null;

            chart.Initialize(this);
        }

        public void SetTeaFileIndex(object sender, long ti)
        {
            if (sender == chart)
            {
                chartTapeIndex.ValueText = ti.ToString();
            }
        }
        #endregion

        #region Description
        private void DescriptionBindClick(object sender, RoutedEventArgs e)
        {
            var ts = TeaFactory.Instance.OpenReadTyped(descriptionFile.Text);
            description.TeaFile = ts;
        }
        #endregion        

        void InitTree(object sender, RoutedEventArgs e)
        {
            this.tree.SetRootPath("m:/test");
        }

        private void ScrollBar_ValueChanged_1(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.grid == null) return;
            this.grid.SelectedIndex = (int)e.NewValue;
        }

        void TreeSelectionTextBlockClick(object sender, MouseButtonEventArgs e)
        {
            this.treeSelection.Text = this.tree.SelectedItemPath;
        }

        void CsvImportLoaded(object sender, RoutedEventArgs e)
        {
            this.csvImportView.Model.WarehouseRoot = "m:/test";
            this.csvImportView.Model.MruFileNames = new ObservableMruCollection<MruItem>();
            this.csvImportView.Model.MruTargetFileNames = new ObservableMruCollection<MruItem>();
            this.csvImportView.Model.Parameters = new CSVImportParameters();
            this.csvImportView.Model.Timescale = Timescale.Java;
        }

        void exportLoaded(object sender, RoutedEventArgs e)
        {
            this.csvExportView.Model = new CSVExportVM(null, null);
            this.csvExportView.Model.Parameters = new CSVExportParameters();
            this.csvExportView.Model.Parameters.SourceFileOrFolder = "e:/datalab/aa.tea";
            this.csvExportView.Model.MruTargetFolders = new ObservableMruCollection<MruItem>();
        }


        public void Update(IChange change)
        {
            throw new NotImplementedException();
        }

        void OpenLicenseDialog(object sender, RoutedEventArgs e)
        {
            AboutDialog.ShowModal();
        }
    }

    class DemoItems : IItemLines
    {
        public string GetLineText(long index)
        {
            if(index > 99) throw new Exception("invalid index");
            return "{0}".Formatted(index).PadLeft(9);
        }

        public string GetHeader()
        {
            return "demo line";
        }
    }
}
