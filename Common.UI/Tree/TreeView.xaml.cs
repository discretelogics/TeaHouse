// copyright discretelogics 2012.
using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace TeaTime.Tree
{
    public partial class TeaHouseTreeView : TreeView
    {
        #region state

        TreeWatcher treeWatcher;

        #endregion

        #region life

        public TeaHouseTreeView()
        {
            this.InitializeComponent();
        }

        public void SetRootPath(string rootPath)
        {
            var root = new Folder(rootPath);
            root.IsExpanded = true;
            this.ItemsSource = root.ToEnumerable();
            this.treeWatcher = new TreeWatcher(root);
        }

        public void Dispose()
        {
            if(this.treeWatcher == null) return;
            this.treeWatcher.Dispose();
        }

        #endregion

        #region core

        public event Action<string> OnTimeSeriesOpen;

        public string SelectedItemPath
        {
            get 
            {
                if (this.SelectedItem == null) return null;
                return ((INode)this.SelectedItem).FullPath;
            }
        }

        #endregion

        #region ui events

        void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.OnTimeSeriesOpen == null) return;

            var item = this.SelectedItem as TeaFileNode;
            if (item == null) return;

            this.OnTimeSeriesOpen(item.FullPath);
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
            {
                var item = this.SelectedItem as TeaFileNode;
                if (item == null) return;
                this.OnTimeSeriesOpen(item.FullPath);
            }
        }

        #endregion
    }
}
