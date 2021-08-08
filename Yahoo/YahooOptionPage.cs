// copyright discretelogics 2013.

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Windows.Forms.Design;
using Microsoft.VisualStudio.Shell;

namespace TeaTime.Yahoo
{
    public class YahooOptionPage : DialogPage //, INotifyPropertyChanged
    {
        string downloadDirectory;
        string symbolAliases;
        bool defaultsHaveBeenSet;

        #region core

        [Category("Yahoo Downloader")]
        [DisplayName("Yahoo Downloader Root Directory")]
        [Description("The directory where newly subscribed files will be placed.")]
        [Editor(typeof (FolderNameEditor), typeof (UITypeEditor))]
        public string DownloadDirectory
        {
            get { return this.downloadDirectory; }
            set
            {
                this.downloadDirectory = value.Trim();
            }
        }

        [Category("Yahoo Downloader")]
        [DisplayName("Symbol Aliases")]
        [Description("Yahoo symbols for data downloads sometimes deviate from symbols at yahoo.com.")]
        public string SymbolAliases
        {
            get
            {
                return this.symbolAliases;
            }
            set
            {
                this.symbolAliases = value.Trim();
            }
        }

        #endregion

        public bool DefaultsHaveBeenSet { get { return this.defaultsHaveBeenSet; } set { this.defaultsHaveBeenSet = value; } }

        private const string defaultFolderName = "YahooDownloads";
        public string GetDownloadDirectoryEnsured()
        {
            if (!this.downloadDirectory.IsSet())
            {
                this.downloadDirectory = Path.Combine(TeaTimeConstants.GetDefaultWarehousePath(), defaultFolderName);
            }
            return this.downloadDirectory;
        }

        public void EnsureDefaultSettings()
        {
            if (this.DefaultsHaveBeenSet) return;

            //  aliases
            this.symbolAliases = "^DJI=DJIA";
            
            // directory
            var root = Environment.GetEnvironmentVariable(TeaTimeConstants.WarehouseEnvironmentVariable, EnvironmentVariableTarget.User);
            if (root.IsSet())
            {
                if (Directory.Exists(root))
                {
                    this.downloadDirectory = Path.Combine(root, defaultFolderName);
                    Directory.CreateDirectory(this.downloadDirectory);
                }
            }
            if (!this.downloadDirectory.IsSet()) // if still not set, point into user directory
            {
                if (!this.downloadDirectory.IsSet())
                {
                    this.downloadDirectory = Path.Combine(TeaTimeConstants.GetDefaultWarehousePath(), defaultFolderName);
                }
            }

            this.defaultsHaveBeenSet = true;
        }
    }
}
