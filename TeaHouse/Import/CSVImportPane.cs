// copyright discretelogics 2013.

using System.Runtime.InteropServices;
using System.Windows.Controls;
using Microsoft.VisualStudio.Shell;
using TeaTime.Data;
using TeaTime.Special;

namespace TeaTime.UI
{
    [Guid("65F52977-93A1-4480-8858-7B83996D3EEE")]
    class CSVImportPane : ToolWindowPane
    {
        #region life

        public CSVImportPane()
        {
            this.Caption = "TeaFile Import";
            this.BitmapResourceID = 304;
            this.BitmapIndex = 0;

            CSVImportView view;
            base.Content = GoodFactory.GetContent<CSVImportView>(out view);
        }

        protected override void Initialize()
        {
            var view = base.Content as CSVImportView;
            if (view != null)
            {
                var model = view.Model;
                model.Parameters = SettingsManager.Instance.Read<CSVImportParameters>("Csv", "ImportParameters", () => new CSVImportParameters());
                model.MruFileNames = SettingsManager.Instance.Read<ObservableMruCollection<MruItem>>("Csv", "ImportSourceFiles", () => new ObservableMruCollection<MruItem>());
                model.MruTargetFileNames = SettingsManager.Instance.Read<ObservableMruCollection<MruItem>>("Csv", "ImportTargetFiles", () => new ObservableMruCollection<MruItem>());
                model.WarehouseRoot = TeaHousePackage.Instance.Options.TeaHouseRootDirectory;
                model.Timescale = TeaHousePackage.Instance.Options.Timescale;
            }
            base.Initialize();
        }

        protected override void OnClose()
        {
            var view = (CSVImportView)base.Content;
            var model = view.Model;
            SettingsManager.Instance.Store("Csv", "ImportParameters", model.Parameters);
            SettingsManager.Instance.Store("Csv", "ImportSourceFiles", model.MruFileNames);
            SettingsManager.Instance.Store("Csv", "ImportTargetFiles", model.MruTargetFileNames);

            base.OnClose();
        }

        #endregion
    }
}
