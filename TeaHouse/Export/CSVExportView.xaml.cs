// copyright discretelogics © 2011
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TeaTime.Converters;
using TeaTime.Data;
using TeaTime.VSX;

namespace TeaTime.UI
{
	public partial class CSVExportView : UserControl
	{
		#region commands

		public static RoutedCommand BrowseTargetFolderCommand = new RoutedCommand();
		public static RoutedCommand ExportCommand = new RoutedCommand();

		#endregion

		#region properties

		private CSVExportVM model;
		public CSVExportVM Model
		{
			get { return this.model; }
            set
            {
                this.model = value;
                DataContext = this.model;
            }
		}

		#endregion

		#region ctor

		public CSVExportView()
		{
			InitializeComponent();

            Binding vb = new Binding("Visibility");
            vb.Path = new PropertyPath("Parameters.SourceFileOrFolder");
            vb.Converter = new Function<string, Visibility>(s => string.IsNullOrWhiteSpace(s) ? Visibility.Visible : Visibility.Collapsed, null);
            watermark.SetBinding(TextBlock.VisibilityProperty, vb);
		}
        
        #endregion
        
        #region eventhandler

        private void BrowseTargetFolderCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (this.model != null);
		}

		private void BrowseTargetFolderExecuted(object sender, ExecutedRoutedEventArgs e)
		{
			var fbd = new System.Windows.Forms.FolderBrowserDialog();
			fbd.RootFolder = Environment.SpecialFolder.MyComputer;
			if (Directory.Exists(this.model.Parameters.TargetFolder))
			{
				fbd.SelectedPath = this.model.Parameters.TargetFolder;
			}
			fbd.ShowNewFolderButton = true;
			fbd.Description = "Choose the Target location for the exported TeaFiles";
			if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				this.model.Parameters.TargetFolder = fbd.SelectedPath;
				this.model.MruTargetFolders.Add(new MruItem(fbd.SelectedPath));
			}
		}

		private void ExportCanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (this.model != null) && this.model.Parameters.IsConfigured && this.model.PreviewOk;
		}

		private void ExportExecuted(object sender, ExecutedRoutedEventArgs e)
		{
		    IntPtr animation = IntPtr.Zero;
            if(TeaHousePackage.Instance != null) // somewhat lousy decoupling
            {
                animation = AnimationHelper.GetAnimation("TeaFile.Export.Animation.16.png");
                TeaHousePackage.Instance.LengthyOperationStarted("Export started...", animation);
            }
			
            this.model.Export();

            if (TeaHousePackage.Instance != null)
            {
                TeaHousePackage.Instance.LengthyOperationStopped("Export succeeded", animation);
            }
		}
        #endregion
    }
}