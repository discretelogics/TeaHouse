using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using TeaTime.VSX;
using TeaTime.Yahoo.Commands;

namespace TeaTime.Yahoo
{
    /// <summary>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>

    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]

    // This attribute is used to register the information needed to show this package in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]

    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]

    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(YahooBrowserPane), Style = VsDockStyle.Tabbed, Width = 1000, Height = 500, DocumentLikeTool = true)]

    [ProvideOptionPage(typeof(YahooOptionPage), "Yahoo Downloader", "Yahoo Downloader Options", 111, 114, true)]

    [Guid(YahooConstants.guidYahooPkgString)]
    public sealed class YahooPackage : Package
    {
        private static YahooPackage instance;
        public static YahooPackage Instance { get { return instance; } }

        #region life

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {            
            base.Initialize();

            this.RegisterMenuCommand(YahooCommands.ViewYahooBrowser, YahooConstants.guidYahooViewCmdSet, YahooConstants.idShowYahooBrowserCmd);
            this.RegisterMenuCommand(YahooCommands.UpdateYahooData, YahooConstants.guidYahooToolsCmdSet, YahooConstants.idUpdateYahooData);

            this.Options.EnsureDefaultSettings();

            instance = this;
        }

        #endregion

        public YahooOptionPage Options
        {
            get { return (YahooOptionPage)this.GetDialogPage(typeof(YahooOptionPage)); }
        }

        public void ShowOptionPage()
        {            
            Instance.ShowOptionPage(typeof(YahooOptionPage));
        }

        public void ShowYahooToolWindow()
        {
            ToolWindowPane window = this.FindToolWindow(typeof(YahooBrowserPane), 0, true);
            if ((window == null) || (window.Frame == null))
            {
                throw new COMException("Failed to create YahooBrowser window.");
            }
            var windowFrame = (IVsWindowFrame)window.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        #region int

        void RegisterMenuCommand(CommandBase<YahooPackage> command, Guid menuGroup, int commandId)
        {
            command.Initialize(this);

            var menuCommandService = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            var menuCommandID = new CommandID(menuGroup, commandId);
            var menuItem = new OleMenuCommand((s, e) => command.Execute(null), menuCommandID);
            if (menuCommandService != null) menuCommandService.AddCommand(menuItem);
            else
            {
                // log error                
            }
        }

        #endregion
    }
}
