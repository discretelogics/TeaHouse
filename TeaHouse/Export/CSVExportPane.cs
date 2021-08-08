// copyright discretelogics 2012.

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using TeaTime.Data;
using TeaTime.Special;
using TeaTime.VSX;

namespace TeaTime.UI
{
    [Guid("6A58D720-6150-41C6-841D-51478DFF3590")]
    public class CSVExportPane : ToolWindowPane
    {
        #region state

        readonly CSVExportVM model;

        #endregion

        #region life

        public CSVExportPane()
        {
            this.Caption = "TeaFile Export";
            this.BitmapResourceID = 305;
            this.BitmapIndex = 0;
            CSVExportView view;
            var c = GoodFactory.GetContent<CSVExportView>(out view);
            if (view != null)
            {
                view.Model = this.model = new CSVExportVM(TeaHousePackage.Instance.TextReporter, new ExportProgressReporter());
            }
            base.Content = c;
        }

        protected override void Initialize()
        {
            if (this.model != null)
            {
                this.model.LoadSettings();
                TeaHousePackage.Instance.SelectedTimeSeriesPath.OnChanged +=
                    (s, newPath) => this.model.Parameters.SourceFileOrFolder = newPath;
            }
            base.Initialize();
        }

        protected override void OnClose()
        {
            if(this.model != null)
            {
                this.model.SaveSettings();
            }
            base.OnClose();
        }

        #endregion

        #region int

        internal void Evaluate()
        {
            if (this.model == null) return;
            
            var p = this.model.Parameters;
            var current = TeaHousePackage.Instance.SelectedTimeSeriesPath.Current;
            if (p.SourceFileOrFolder != current)
            {
                p.SourceFileOrFolder = current; // this will call UpdatePreview()
            }
            else
            {
                this.model.UpdatePreview(); //  in this case we call it directly
            }
        }

        #endregion
    }

    class ExportProgressReporter : IProgressReporter
    {
        public void ReportProgress(string message, uint completed, uint total)
        {
            TeaHousePackage.Instance.UpdateStatus(message, completed, total);
        }
    }
}
