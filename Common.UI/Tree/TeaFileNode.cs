// copyright discretelogics 2012.

using System;
using System.ComponentModel;
using System.IO;

namespace TeaTime.Tree
{
    class TeaFileNode : INode, INotifyPropertyChanged
    {
        #region state

        string fullPath;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region life

        public TeaFileNode(string fullPath) { this.fullPath = fullPath; }

        #endregion

        #region core

        public bool IsFolder { get { return false; } }
        public bool IsExpanded { get { return false; } set { } }
        public string Name { get { return Path.GetFileName(this.fullPath); } }
        public string FullPath { get { return this.fullPath; } }

        #endregion

        #region sync

        public void UpdatePath(string newpath, Folder parent)
        {
            this.fullPath = newpath;
            if (this.PropertyChanged != null) this.PropertyChanged(this, new PropertyChangedEventArgs("Name"));
        }

        #endregion

        #region diag

        public override string ToString()
        {
            return this.fullPath;
        }

        #endregion
    }
}
