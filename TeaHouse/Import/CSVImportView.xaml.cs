// copyright discretelogics 2013.

using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;
using TeaTime.Data;
using TeaTime.VSX;

namespace TeaTime.UI
{
    public partial class CSVImportView : UserControl
    {
        #region commands

        public static RoutedCommand BrowseSourceFileCommand = new RoutedCommand();
        public static RoutedCommand BrowseTargetFileCommand = new RoutedCommand();
        public static RoutedCommand ImportCommand = new RoutedCommand();

        #endregion

        #region state

        CSVImportVM model;

        // for easy testing in devapp this should be public
        public CSVImportVM Model { get { return this.model; } }

        #endregion

        #region life

        public CSVImportView()
        {
            this.InitializeComponent();

            this.model = new CSVImportVM();
            this.model.Preview.CollectionChanged += this.UpdatePreview;
            this.DataContext = this.model;
        }

        #endregion

        #region events

        void BrowseSourceFileCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (this.model != null);
        }

        void BrowseSourceFileExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = false;
            ofd.FileName = this.model.Parameters.CSVFileName;
            ofd.Filter = "Text files (*.csv, *.txt)|*.csv;*.txt|All files (*.*)|*.*";
            ofd.Title = "Import File";
            if (ofd.ShowDialog() == true)
            {
                this.model.Parameters.CSVFileName = ofd.FileName;
                this.model.MruFileNames.Add(new MruItem(ofd.FileName));
            }
        }

        void BrowseTargetFileCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (this.model != null);
        }

        void BrowseTargetFileExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.CheckFileExists = false;
            sfd.AddExtension = true;
            sfd.CreatePrompt = false;
            sfd.DefaultExt = TeaTime.CommonUI.Constants.TeaFileExtension;
            if (String.IsNullOrWhiteSpace(this.model.Parameters.TargetFileName))
            {
                sfd.InitialDirectory = TeaHousePackage.Instance.Options.TeaHouseRootDirectory;
            }
            else
            {
                sfd.FileName = this.model.Parameters.TargetFileName;
            }
            sfd.Filter = TeaTime.CommonUI.Constants.TeaFileFilter;
            sfd.OverwritePrompt = false;
            sfd.Title = "Import Target File";
            sfd.ValidateNames = true;
            if (sfd.ShowDialog() == true)
            {
                this.model.Parameters.TargetFileName = sfd.FileName;
                this.model.MruTargetFileNames.Add(new MruItem(sfd.FileName));
            }
        }

        void ImportCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (this.model != null) && this.model.IsConfigured;
        }

        void ImportExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.model.Parameters.OverwriteExistingFile && File.Exists(this.model.Parameters.TargetFileName))
            {
                if (MessageBox.Show("Are you sure you want to overwrite the existing file?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                    == MessageBoxResult.Yes)
                {
                    TeaHousePackage.Instance.EnsureFileClosed(this.model.Parameters.TargetFileName);
                }
                else
                {
                    return;
                }
            }
            this.model.MruFileNames.Add(new MruItem(this.model.Parameters.CSVFileName));
            this.model.MruTargetFileNames.Add(new MruItem(this.model.Parameters.TargetFileName));

            IntPtr animation = AnimationHelper.GetAnimation("TeaFile.Import.Animation.16.png");
            TeaHousePackage.Instance.LengthyOperationStarted("Import started...", animation);
            try
            {
                this.model.Import(TeaHousePackage.Instance.TextReporter);
            }
            catch (Exception ex)
            {
                if (TeaHousePackage.Instance != null) TeaHousePackage.Instance.WriteError(ex.Message, null);
            }
            if (animation != IntPtr.Zero)
            {
                TeaHousePackage.Instance.LengthyOperationStopped("Import completed", animation);
            }
        }

        void UpdatePreview(object sender, EventArgs e)
        {
            this.previewGrid.Columns.Clear();

            if (this.model.Preview.Any())
            {
                for (int i = 0; i < this.model.Preview.First().Length; i++)
                {
                    DataGridTextColumn column = new DataGridTextColumn();
                    column.Binding = new Binding(String.Format("[{0}]", i));
                    column.Header = this.model.Parameters.TeaFileFields[i];
                    column.IsReadOnly = true;
                    column.CanUserSort = false;
                    column.CanUserReorder = false;
                    column.Width = new DataGridLength(1, DataGridLengthUnitType.Star);
                    this.previewGrid.Columns.Add(column);
                }
            }
        }

        void FieldTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            Console.WriteLine(e);
            this.model.UpdateParsing();
        }

        #endregion
    }
}
