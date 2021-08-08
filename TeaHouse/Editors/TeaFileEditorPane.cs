// copyright discretelogics © 2011
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell.Interop;
using TeaTime.Data;
using TeaTime.Special;

namespace TeaTime.Editors
{
    [ComVisible(true)]
    public sealed class TeaFileEditorPane : Microsoft.VisualStudio.Shell.WindowPane, IVsPersistDocData
    {
        #region WindowPane

        protected override void Initialize()
        {
            base.Content = GoodFactory.GetContent<TeaFileEditor>(out this.editor); // checks license and returns "license exhausted" control if not valid            
        }

        #endregion

        #region IVsPersistDocData Members

        public int Close()
        {
            if (editor == null) return VSConstants.S_OK; // license expired case
            editor.Dispose();

            if (teaFile != null)
            {
                teaFile.Dispose();
                teaFile = null;
            }

            return VSConstants.S_OK;
        }

        public int GetGuidEditorType(out Guid pClassID)
        {
            // todo: handle this
            //throw new NotImplementedException();
            pClassID = Guid.Empty;
            return VSConstants.E_FAIL;
        }

        public int IsDocDataDirty(out int pfDirty)
        {
            pfDirty = 0;
            return VSConstants.S_OK;
        }

        public int IsDocDataReloadable(out int pfReloadable)
        {
            throw new NotImplementedException();
        }

        public int LoadDocData(string filename)
        {
            try
            {
                if (editor == null) return VSConstants.S_OK; // if the license is expired, the editor is null
                editor.Initialize(filename);
            }
            catch (HandledException)
            {
                return VSConstants.E_FAIL;
            }
            catch (Exception)
            {
                return VSConstants.S_FALSE;
            }
            return VSConstants.S_OK;
        }

        public int OnRegisterDocData(uint docCookie, IVsHierarchy pHierNew, uint itemidNew)
        {
            return VSConstants.S_OK;
        }

        public int ReloadDocData(uint grfFlags)
        {
            throw new NotImplementedException();
        }

        public int RenameDocData(uint grfAttribs, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
        {
            throw new NotImplementedException();
        }

        public int SaveDocData(VSSAVEFLAGS dwSave, out string pbstrMkDocumentNew, out int pfSaveCanceled)
        {
            throw new NotImplementedException();
        }

        public int SetUntitledDocPath(string pszDocDataPath)
        {
            // throw new NotImplementedException();
            this.xname = pszDocDataPath;
            return VSConstants.S_OK;
        }

        #endregion

        #region fields

        private TeaFileEditor editor;
        private TeaFile teaFile;
        string xname;

        #endregion

        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1816:CallGCSuppressFinalizeCorrectly")]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public string StreamId { get; set; }
    }
}