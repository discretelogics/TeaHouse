// copyright discretelogics 2012.

using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using TeaTime.CommonUI;
using TeaTime.Data;
using TeaTime.UI;

namespace TeaTime.Data
{
    public class CSVExportVM : NotifyPropertyChanged
    {
        readonly ITextReporter textRetporter;
        readonly IProgressReporter progressReporter;

        #region state

        ConfirmMessageBoxResult? confirmOverwrite;
        CSVExportParameters parameters;
        string preview;
        string previewFile;

        #endregion

        #region life

        public CSVExportVM(ITextReporter textRetporter, IProgressReporter progressReporter)
        {
            this.textRetporter = textRetporter;
            this.progressReporter = progressReporter;
        }

        #endregion

        #region core

        public CSVExportParameters Parameters
        {
            get { return this.parameters; }
            set
            {
                Guard.ArgumentNotNull(value, "value");
                this.SetProperty(ref this.parameters, value);
                this.parameters.PropertyChanged += this.ParametersPropertyChanged;

                this.UpdatePreview();
            }
        }

        public ObservableMruCollection<MruItem> MruTargetFolders { get; set; }

        public string Preview { get { return this.preview; } set { this.SetProperty(ref this.preview, value); } }
        public string PreviewFile { get { return this.previewFile; } set { this.SetProperty(ref this.previewFile, value); } }        

        public bool PreviewOk { get; private set; }

        public void Export()
        {
            this.confirmOverwrite = null;

            if (!Directory.Exists(this.Parameters.TargetFolder))
            {
                try
                {
                    Directory.CreateDirectory(this.Parameters.TargetFolder);
                }
                catch (Exception)
                {
                    MessageBox.Show("Could not create target folder.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (this.Parameters.SourceIsFolder)
            {
                SearchOption so = this.Parameters.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                Directory.EnumerateFiles(this.Parameters.SourceFileOrFolder, TeaTime.CommonUI.Constants.TeaFileSearchPattern, so)
                    .ForEach(this.ExportFile);
            }
            else
            {
                this.ExportFile(this.Parameters.SourceFileOrFolder);
            }            
            this.MruTargetFolders.Add(new MruItem(this.Parameters.TargetFolder));                
        }

        #endregion

        #region int

        void ExportFile(string sourceFile)
        {
            try
            {
                var targetCsvFile = this.GetTargetCSVFileName(sourceFile);
                if (!this.FileCollisionConfirmed(targetCsvFile)) return; // unconfirmed file collisions, abort 

                Data.Export.ExportToCSV(
                    sourceFile,
                    targetCsvFile,
                    this.parameters,
                    this.textRetporter,
                    this.progressReporter
                    );
                textRetporter.WriteLine("Export Completed.");
            }
            catch (Exception ex)
            {
                textRetporter.WriteLine("Export to csv failed. " + ex.Message);
            }
        }

        bool FileCollisionConfirmed(string targetCsvFile)
        {
            // user confirmed everything?
            if (this.confirmOverwrite == ConfirmMessageBoxResult.YesToAll) return true;

            // file collision?
            if (!File.Exists(targetCsvFile)) return true;

            // abort everything?
            if (this.confirmOverwrite == ConfirmMessageBoxResult.NoToAll) return false;

            // ask
            this.confirmOverwrite =
                ConfirmMessageBox.Show(
                    String.Format(@"The file {0} ({1}) exists already.
When you overwrite the file, data might be lost permanently.

Are you sure you want to overwrite the target file?",
                                  Path.GetFileName(targetCsvFile), Path.GetDirectoryName(targetCsvFile)),
                    "Confirm Overwrite",
                    new BitmapImage(new Uri(@"pack://application:,,,/DiscreteLogics.TeaHouse;component/Resources/TeaFile.Export.64.png")),
                    BitmapFrame.Create(new Uri(@"pack://application:,,,/DiscreteLogics.TeaHouse;component/Resources/TeaFile.Export.ico")));

            if (this.confirmOverwrite == ConfirmMessageBoxResult.Yes) return true;
            if (this.confirmOverwrite == ConfirmMessageBoxResult.YesToAll) return true;
            if (this.confirmOverwrite == ConfirmMessageBoxResult.No) return false;
            if (this.confirmOverwrite == ConfirmMessageBoxResult.NoToAll) return false;
            
            // unreachable code
            return false;
        }

        string GetTargetCSVFileName(string teaFileName)
        {
            return Path.Combine(this.Parameters.TargetFolder, Path.GetFileNameWithoutExtension(teaFileName) + ".csv");
        }    

        void ParametersPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
           this.UpdatePreview();
        }

        public void UpdatePreview()
        {
            if (this.Parameters.SourceFileOrFolder.IsSet())
            {
                string filename = null;
                if (this.Parameters.SourceIsFolder)
                {
                    filename = Directory.EnumerateFiles(
                        this.Parameters.SourceFileOrFolder,
                        Constants.TeaFileSearchPattern).FirstOrDefault();
                }
                else
                {
                    filename = this.Parameters.SourceFileOrFolder;
                }

                if (filename.IsSet())
                {
                    try
                    {
                        this.Preview = Data.Export.GetExportedCSVPreview(filename, this.Parameters);
                        this.PreviewOk = true;
                        if (this.Parameters.TargetFolder.IsSet())
                        {
                            this.PreviewFile = GetTargetCSVFileName(filename);
                        }
                        else
                        {
                            this.PreviewFile = null;
                        }
                        return;
                    }
                    catch (Exception ex)
                    {
                        textRetporter.WriteLine(ex.Message);
                        this.Preview = ex.Message;
                        this.PreviewOk = false;
                    }
                }
            }
            this.PreviewFile = null;
            this.Preview = null;
            this.PreviewOk = false;
        }

        #endregion

        public void LoadSettings()
        {
            this.Parameters = SettingsManager.Instance.Read<CSVExportParameters>("Csv", "ExportParameters", () => new CSVExportParameters());
            this.MruTargetFolders = SettingsManager.Instance.Read<ObservableMruCollection<MruItem>>("Csv", "ExportFolders", () => new ObservableMruCollection<MruItem>());
        }

        public void SaveSettings()
        {
            SettingsManager.Instance.Store("Csv", "ExportParameters", this.Parameters);
            SettingsManager.Instance.Store("Csv", "ExportFolders", this.MruTargetFolders);
        }
    }
}
