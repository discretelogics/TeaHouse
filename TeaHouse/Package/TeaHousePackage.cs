// copyright discretelogics 2012.

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Shell.Settings;
using TeaTime.Chart.Settings;
using TeaTime.Commands;
using TeaTime.Data;
using TeaTime.Editors;
using TeaTime.UI;
using TeaTime.VSX;
using Constants = EnvDTE.Constants;
using Window = EnvDTE.Window;

namespace TeaTime
{
    [ProvideBindingPath]
    [ProvideAutoLoad(UIContextGuids80.DataSourceWindowSupported)] // ???
    [ProvideAutoLoad(UIContextGuids80.EmptySolution)]
    [ProvideAutoLoad(UIContextGuids80.NoSolution)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [InstalledProductRegistration("#111", "#114", "1.0", IconResourceID = 401)]
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [Guid(TeaHouseConstants.guidTeaHousePackageString)]

    [ProvideOptionPage(typeof(TeaHouseOptionPage), "TeaHouse", "TeaHouse Options", 111, 113, true)]
    [ProvideProfile(typeof(TeaHouseOptionPage), "TeaHouse", "TeaHouse Options", 111, 113, true, DescriptionResourceID = 112)]
    
    // 7e574ed2-97f6-4594-b7f3-a403f8056443 == typeof(TeaFileEditor).GUID
    // this MUST be used, otherwise Obfuscator translates this attribute to a ctor call with typeof(TeaFileEditor).ToString() == "TeaTime.Editors.TeaFileEditor" 
    // which in turn is NOT a valid call of the ctor and causes CreatePkgDef to fail. And most likely the package would fail toload later as well.
    [ProvideEditorExtension("7e574ed2-97f6-4594-b7f3-a403f8056443", CommonUI.Constants.TeaFileExtension, 50, NameResourceID = 500)]
    [ProvideEditorFactory(typeof(TeaFileEditorFactory), 500, CommonPhysicalViewAttributes = (int)__VSPHYSICALVIEWATTRIBUTES.PVA_SupportsPreview, TrustLevel = __VSEDITORTRUSTLEVEL.ETL_AlwaysTrusted)]
    
    [ProvideToolWindow(typeof(TeaHouseTreePane), Style = VsDockStyle.Tabbed, Window = "{3AE79031-E1BC-11D0-8F78-00A0C9110057}")]
    [ProvideToolWindow(typeof(CSVImportPane), Style = VsDockStyle.Float, Width = 600, Height = 480)]
    [ProvideToolWindow(typeof(CSVExportPane), Style = VsDockStyle.Float, Width = 480, Height = 320)]

    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class TeaHousePackage : Package
    {
        #region const

        const string collectionRoot = "TeaTime";

        #endregion

        #region state

        ShellSettingsManager shellSettingsManager;

        #endregion

        #region life

        static TeaHousePackage instance = null;

        public static TeaHousePackage Instance { get { return instance; } }

        protected override void Initialize()
        {
            base.Initialize();

            Dispatcher.CurrentDispatcher.UnhandledException += (s, e) =>
                {
                    this.WriteError(e.Exception.Message);
                    e.Handled = true;
                };

            // statusbar
            this.statusBar = (IVsStatusbar)this.GetService(typeof(SVsStatusbar));

            // settings
            this.shellSettingsManager = new ShellSettingsManager(this);
            Func<string, string> getCollectionPath = (collectionName) => collectionRoot + "\\" + collectionName;
            SettingsManager.Instance.Configure(
                (collectionName, name, value) =>
                {
                    string collectionPath = getCollectionPath(collectionName);
                    var store = this.shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
                    store.CreateCollection(collectionPath);
                    store.SetString(collectionPath, name, value);
                },
                (collectionName, name) =>
                {
                    string collectionPath = getCollectionPath(collectionName);
                    var store = this.shellSettingsManager.GetReadOnlySettingsStore(SettingsScope.UserSettings);
                    if (store.CollectionExists(collectionPath))
                    {
                        return store.GetString(collectionPath, name, null);
                    }
                    return null;
                });
            this.EnsureDefaultSettings();

            //  Create Editor Factory            
            base.RegisterEditorFactory(new TeaFileEditorFactory());

            // menu commands
            this.RegisterMenuCommand(TeaHouseCommands.ViewTeaHouseExplorer, TeaHouseConstants.guidTeaHouseViewCmdSet, TeaHouseConstants.idTeaHouseTreeCmd);
            this.RegisterMenuCommand(TeaHouseCommands.ImportCsvCmd, TeaHouseConstants.guidTeaHouseToolsCmdSet, TeaHouseConstants.idImportCsvFileCmd);
            this.RegisterMenuCommand(TeaHouseCommands.ExportCsvCmd, TeaHouseConstants.guidTeaHouseToolsCmdSet, TeaHouseConstants.idExportCsvFileCmd);
            this.RegisterMenuCommand(TeaHouseCommands.ShowAboutDialogCmd, TeaHouseConstants.guidTeaHouseHelpCmdSet, TeaHouseConstants.idShowAboutDialogCmd);

            // other commands
            TeaHouseCommands.ViewTeaHouseOptions.Initialize(this);

            // tbd
            Time.ScaleCollisionBehavior = ScaleCollisionBehavior.Ignore;

            instance = this;
        }

        #endregion

        #region core

        public TeaHouseOptionPage Options { get { return (TeaHouseOptionPage)this.GetDialogPage(typeof(TeaHouseOptionPage)); } }

        #endregion

        #region dte util

        internal void EnsureFileClosed(string file)
        {
            DTE2 dte = (DTE2)this.GetService(typeof(SDTE));
            if (dte.ItemOperations.IsFileOpen(file))
            {
                Window window = dte.ItemOperations.OpenFile(file, Constants.vsViewKindPrimary);
                window.Close();
            }
        }

        #endregion

        #region recent

        public IEnumerable<MruFileCommand> GetRecentFiles()
        {
            IOleCommandTarget shellCommandTarget = (IOleCommandTarget)this.GetService(typeof(SUIHostCommandDispatcher));

            VsCommandTextRetriever textRetriever = new VsCommandTextRetriever(shellCommandTarget, VSConstants.GUID_VSStandardCommandSet97, (uint)VSConstants.VSStd97CmdID.MRUFile1);
            string text = textRetriever.RetrieveText();
            var files = new List<MruFileCommand>();
            while (text != null)
            {
                files.Add(new MruFileCommand(textRetriever.CommandGuid, textRetriever.CommandId, text));
                textRetriever.CommandId++;

                text = textRetriever.RetrieveText();
            }
            return files;
        }

        public bool OpenRecentFile(MruFileCommand command)
        {
            IOleCommandTarget shellCommandTarget = (IOleCommandTarget)this.GetService(typeof(SUIHostCommandDispatcher));
            shellCommandTarget.Exec(ref command.CommandGuid, command.CommandId, (uint)OLECMDEXECOPT.OLECMDEXECOPT_DODEFAULT, IntPtr.Zero, IntPtr.Zero);
            return false; // there is no nice way to determine that a file was removed from the mru list
        }

        #endregion

        #region statusbar

        IVsStatusbar statusBar;

        internal void UpdateStatus(string label, uint progress, uint total)
        {
            uint cookie = 0;
            this.statusBar.Progress(ref cookie, 1, label, progress, total);
        }

        internal void LengthyOperationStarted(string label, object icon)
        {
            this.UpdateStatus(label, 0, 0);
            this.statusBar.Animation(1, ref icon);
        }

        internal void LengthyOperationStopped(string label, object icon)
        {
            this.UpdateStatus(label, 0, 0);
            this.statusBar.Animation(0, ref icon);
        }

        #endregion

        #region commands

        void RegisterMenuCommand(CommandBase<TeaHousePackage> command, Guid menuGroup, int commandId)
        {
            command.Initialize(this);

            var menuCommandService = this.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            var menuCommandID = new CommandID(menuGroup, commandId);
            var menuItem = new OleMenuCommand((s, e) => command.Execute(null), menuCommandID);
            menuCommandService.AddCommand(menuItem);
        }

        #endregion

        #region settings

        void EnsureDefaultSettings()
        {
            try
            {
                this.EnsureDefaultChartSettings();
                this.EnsureDefaultWarehouse();
                this.EnsureDefaultStartPage();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to set default settings." + Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        void EnsureDefaultChartSettings()
        {
            var builtinTypes = new[]
                {
                    ItemDescription.FromAnalysis<Event<OHLC>>(),
                    ItemDescription.FromAnalysis<Event<OHLCV>>(),
                    ItemDescription.FromAnalysis<Event<OHLCVI>>()
                };
            foreach (var id in builtinTypes)
            {
                var t = TeaFactory.Instance.CreateType(id);
                var setting = ItemTypeSettings.Read(t);
                if (setting.IsProposal)
                    setting.Store();
            }
        }

        void EnsureDefaultWarehouse()
        {
            if (!Directory.Exists(this.Options.TeaHouseRootDirectory))
            {
                string warehouse = Environment.GetEnvironmentVariable(TeaTimeConstants.WarehouseEnvironmentVariable, EnvironmentVariableTarget.User);
                if (!warehouse.IsSet())
                {
                    warehouse = TeaTimeConstants.GetDefaultWarehousePath();
                }
                if (!Directory.Exists(warehouse))
                {
                    Directory.CreateDirectory(warehouse);
                }
                string dow30 = Path.Combine(warehouse, "Dow30");
                if (!Directory.Exists(dow30))
                {
                    Directory.CreateDirectory(dow30);
                    var asm = Assembly.GetCallingAssembly();
                    const string resourceFolder = "TeaTime.Resources.Dow30.";
                    foreach (var name in asm.GetManifestResourceNames().Where(s => s.StartsWith(resourceFolder)))
                    {
                        using (var stream = asm.GetManifestResourceStream(name))
                        {
                            using (var file = File.Create(Path.Combine(dow30, name.Remove(0, resourceFolder.Length))))
                            {
                                byte[] buffer = new byte[8 * 1024];
                                int len;
                                while ((len = stream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    file.Write(buffer, 0, len);
                                }
                            }
                        }
                    }
                }
                this.Options.TeaHouseRootDirectory = warehouse;
            }
        }

        void EnsureDefaultStartPage()
        {
            var readOnlyStore = this.shellSettingsManager.GetReadOnlySettingsStore(SettingsScope.UserSettings);
            if (!readOnlyStore.GetBoolean(collectionRoot, "IsDefaultStartPageSet", false))
            {
                var store = this.shellSettingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);

                FileInfo asmFile = new FileInfo(Assembly.GetCallingAssembly().Location);
                string startPageUri = Path.Combine(asmFile.DirectoryName, "StartPage", "TeaHouseStartPage.xaml");
                if (File.Exists(startPageUri))
                {
                    const string startPageCollection = "StartPage\\Default";
                    store.CreateCollection(startPageCollection);
                    store.SetString(startPageCollection, "Uri", startPageUri);

                    store.CreateCollection(collectionRoot);
                    store.SetBoolean(collectionRoot, "IsDefaultStartPageSet", true);
                }
            }
        }

        #endregion

        #region selection

        StringSelection selectedTimeSeriesPath;

        internal StringSelection SelectedTimeSeriesPath
        {
            get { return this.selectedTimeSeriesPath ?? (this.selectedTimeSeriesPath = new StringSelection()); }
        }

        #endregion

        #region report

        ITextReporter textReporter;
        public ITextReporter TextReporter
        {
            get
            {
                return this.textReporter ?? (this.textReporter = new TextReporter(this));
            }
        }

        #endregion

        #region provisional

        string provisionalFocusToIgnore;
        readonly object provisionalFocusSync = new object();
        public void IgnoreFocusForProvisionalTab(string fullname)
        {
            lock (provisionalFocusSync)
            {
                provisionalFocusToIgnore = fullname;
            }
        }
        public bool ResetFocusForProvisionalTab(string fullname)
        {
            lock (provisionalFocusSync)
            {
                if (provisionalFocusToIgnore == fullname)
                {
                    provisionalFocusToIgnore = null;
                    return true;
                }
                return false;
            }
        }

        #endregion
    }
}
