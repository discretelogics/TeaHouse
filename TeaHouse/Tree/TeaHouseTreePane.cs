// copyright discretelogics 2012.

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Threading;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using NLog;
using TeaTime.Tree;
using TeaTime.VSX;
using Constants = EnvDTE.Constants;
using SelectionContainer = Microsoft.VisualStudio.Shell.SelectionContainer;

namespace TeaTime.UI
{
    [Guid("d41b2e08-5b88-4f91-9ab1-f864ada807ea")]
    class TeaHouseTreePane : ToolWindowPane
    {
        static Logger logger = LogManager.GetCurrentClassLogger();

        #region Properties

        TeaHouseTreeView TeaHouseTreeView
        {
            get { return (TeaHouseTreeView)base.Content; }
        }

        public new TeaHousePackage Package
        {
            get { return (TeaHousePackage)base.Package; }
        }

        #endregion

        #region ctor

        public TeaHouseTreePane() : base(null)
        {
            this.Caption = "TeaHouse Explorer";
            this.BitmapResourceID = 301;
            this.BitmapIndex = 0;

            var treeview = new TeaHouseTreeView();
            treeview.SelectedItemChanged += treeview_SelectedItemChanged;
            treeview.OnTimeSeriesOpen += treeview_OnTimeSeriesOpen;
            
            base.Content = treeview;
        }

        #endregion

        #region event handler
        
        void treeview_OnTimeSeriesOpen(string obj)
        {
            TeaHousePackage.Instance.ResetFocusForProvisionalTab(obj);
            this.OpenTimeSeries(obj);
        }
        void treeview_SelectedItemChanged(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            var treeView = TeaHouseTreeView;
            var selectedPath = treeView.SelectedItemPath;
            TeaHousePackage.Instance.SelectedTimeSeriesPath.Current = selectedPath;
            if (!String.IsNullOrEmpty(selectedPath) && IOUtils.IsFile(selectedPath))
            {
                System.Threading.Tasks.Task.Factory.StartNew(state =>
                {
                    System.Threading.Thread.Sleep(200);
                    var fullname = (string)state;

                    treeView.Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() =>
                    {
                        if (treeView.SelectedItemPath == fullname) // open only, if the file is still selected
                        {
                            using (new NewDocumentStateScope(__VSNEWDOCUMENTSTATE.NDS_Provisional, VSConstants.NewDocumentStateReason.Navigation))
                            {
                                TeaHousePackage.Instance.IgnoreFocusForProvisionalTab(fullname); // avoid forcing the focus on the chart
                                this.OpenTimeSeries(fullname); // if the file is already open in a persistent tab, it is focused and not opened in the provisional tab
                            }
                            // focus the tree again, since the chart is focused by default
                            treeView.Focus();
                            Keyboard.Focus(treeView);
                        }
                    }));
                }, selectedPath);
            }
        }

        public override void OnToolWindowCreated()
        {
            base.OnToolWindowCreated();

            #region Selection Management

            this.selectionContainer = new SelectionContainer();
            this.selectionContainer.SelectedObjects = this.selectionContainer.SelectableObjects = null;
            this.trackSelection = this.GetService(typeof(SVsTrackSelectionEx)) as IVsTrackSelectionEx;
            if (this.trackSelection != null)
            {
                this.trackSelection.OnSelectChange(this.selectionContainer);
            }
            else
            {
                // tbd - log warning
            }
            #endregion

            SetTeaHouseRoot();
            this.Package.Options.OnTeaHouseRootChanged += this.SetTeaHouseRoot;
        }

        protected override void OnClose()
        {
            base.OnClose();
            this.TeaHouseTreeView.Dispose();
        }

        void SetTeaHouseRoot()
        {
            this.TeaHouseTreeView.SetRootPath(this.Package.Options.TeaHouseRootDirectory);
        }

        void OpenTimeSeries(string fullpath)
        {
            try
            {
                var dte = (DTE)this.GetService(typeof(SDTE));
                dte.ItemOperations.OpenFile(fullpath, Constants.vsViewKindAny); // vsViewKindAny is VERY IMPORTANT, using vsViewKindPrimary causes the Editor to be instantiated twice!
            }
            catch(COMException ce)
            {
                if(ce.HResult == VSConstants.E_INVALIDARG)
                {
                    // E_INVALIDARG means that the error has already been reported to the user
                }
                else
                {
                    TeaHousePackage.Instance.WriteError(ce.Message);
                    logger.Error(ce);
                }
            }
            catch (Exception ex)
            {
                TeaHousePackage.Instance.WriteError(ex.Message);
                logger.Error(ex);
            }
        }

        #endregion

        #region fields

        SelectionContainer selectionContainer;
        ITrackSelection trackSelection;

        #endregion
    }
}
