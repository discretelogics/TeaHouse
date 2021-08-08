// copyright discretelogics 2013.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Threading;

namespace TeaTime.Tree
{
    class Folder : NotifyPropertyChanged, INode
    {      
        #region state

        bool isExpanded;
        string fullPath;
        Dispatcher dispatcher;
        ObservableCollection<INode> items; // the collection we maintain
        CollectionView sortedView; // the view that does the sorting for us, automatically, we dont mess with this view        

        // this property exists for unit testing only, it allos mocking the file system structure
        public static Func<string, IEnumerable<INode>> ItemFactory = GetItemEnumerator;

        #endregion

        #region life

        public Folder(string fullPath)
        {
            this.fullPath = fullPath;
            this.items = new ObservableCollection<INode>();
            this.dispatcher = Dispatcher.CurrentDispatcher;

            this.UpdateItems();
            this.sortedView = (CollectionView)CollectionViewSource.GetDefaultView(this.items);
            this.sortedView.SortDescriptions.Add(new SortDescription("IsFolder", ListSortDirection.Descending));
            this.sortedView.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
        }

        #endregion

        #region core

        public string Name { get { return new DirectoryInfo(this.fullPath).Name; } }
        public string FullPath { get { return this.fullPath; } }
        public bool IsFolder { get { return true; } }

        public bool IsExpanded
        {
            get { return this.isExpanded; }
            set
            {
                if (this.SetProperty(ref this.isExpanded, value))
                {
                    this.UpdateItems();
                }
            }
        }

        public CollectionView Items { get { return this.sortedView; } }

        #endregion

        #region item factory

        void UpdateItems()
        {
            var enumeration = ItemFactory(this.fullPath); // by default = GetChildren(this.fullPath)
            this.items.Clear();
            if (this.isExpanded)
            {
                this.items.Add(enumeration);
            }
            else
            {
                this.items.Add(enumeration.Take(1));
            }
        }

        static IEnumerable<INode> GetItemEnumerator(string path)
        {
            if (!IOUtils.CanDiscover(path)) return new List<INode>();

            var enumeration =
                Directory.EnumerateDirectories(path)
                    .Where(IOUtils.CanDiscover)
                    .Select(p => new Folder(p)) // no sort guarantee given, so need the collectionview around it
                    .Union<INode>(Directory.EnumerateFiles(path).Select(p => new TeaFileNode(p))); // detto
            return enumeration;
        }

        #endregion

        #region sync

        public void DispatchedFileSync(Action a)
        {
            try
            {
                this.dispatcher.Invoke(a);
            }
            catch (TaskCanceledException)
            {
            } // during shutdown, a task might get cancelled, we do not care however
            catch (FileNotFoundException)
            {
            } // the file can easily be removed meanwhile
        }

        public void AddItem(string fullPath)
        {
            if (IOUtils.IsFolder(fullPath))
            {
                var f = new Folder(fullPath);
                this.items.Add(f);
            }
            else
            {
                var ts = new TeaFileNode(fullPath);
                this.items.Add(ts);
            }
        }

        public void RemoveItem(INode n)
        {
            this.items.Remove(n);
        }

        public void UpdatePath(string newFullPath, Folder parent)
        {
            this.fullPath = newFullPath;
            this.Changed("Name");
            parent.sortedView.Refresh();
        }

        #endregion

        #region int

        public INode GetChild(string name)
        {
            return this.Items.OfType<INode>().FirstOrDefault(n => n.Name == name);
        }

        internal static INode FindNode(DirectoryInfo d, Folder root, out Folder parent)
        {
            var names = d.FullName.Split(IOUtils.DirectorySeparators);
            var rootnames = root.FullPath.Split(IOUtils.DirectorySeparators);
            names = names.Skip(rootnames.Length).ToArray();

            INode n = root;
            parent = null;
            foreach (string name in names)
            {
                var f = (Folder)n;
                parent = f;
                n = f.GetChild(name);

                if (n == null)
                    break;
            }
            return n;
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
