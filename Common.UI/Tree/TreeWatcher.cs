// copyright discretelogics 2013.

using System;
using System.IO;
using System.Linq;
using NLog;

namespace TeaTime.Tree
{
    class TreeWatcher : IDisposable
    {
        #region state

        FileSystemWatcher watcher;
        Folder root;
        static Logger logger = LogManager.GetCurrentClassLogger();

        #endregion

        #region life

        public TreeWatcher(Folder root)
        {
            this.watcher = new FileSystemWatcher(root.FullPath);
            this.watcher.IncludeSubdirectories = true;
            this.watcher.Created += this.Created;
            this.watcher.Deleted += this.Deleted;
            this.watcher.Renamed += this.Renamed;
            this.watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName; // | NotifyFilters.LastWrite;
            // msdn:
            // You can set the buffer to 4 KB or larger, but it must not exceed 64 KB. 
            // If you try to set the InternalBufferSize property to less than 4096 bytes, 
            // your value is discarded and the InternalBufferSize property is set to 4096 bytes. 
            // For best performance, use a multiple of 4 KB on Intel-based computers.
            this.watcher.InternalBufferSize = 64 * 1024; // "must not exceed 64 KB"
            this.watcher.EnableRaisingEvents = true;
            this.root = root;
        }

        public void Dispose()
        {
            this.watcher.EnableRaisingEvents = false;
            // this.watcher.Dispose(); // this line hangs the closing of VS, presumably causing a deadlock
        }

        #endregion

        #region watching

        void Renamed(object sender, RenamedEventArgs e)
        {            
            Folder parent;
            INode n = Folder.FindNode(new DirectoryInfo(e.OldFullPath), this.root, out parent);
            if (n == null)
            {
                logger.Error("file renaming could not be processed since the node was not found. previous path:'{0}'", e.OldFullPath);
                return;
            }
            parent.DispatchedFileSync(() => n.UpdatePath(e.FullPath, parent));
        }

        void Created(object sender, FileSystemEventArgs e)
        {
            Folder parent;
            Folder.FindNode(new DirectoryInfo(e.FullPath), this.root, out parent);
            if (parent == null)
            {
                logger.Error("file renaming could not be processed since the node was not found. new path:'{0}'", e.FullPath);
                return;
            }
            parent.DispatchedFileSync(() => parent.AddItem(e.FullPath));
        }

        void Deleted(object sender, FileSystemEventArgs e)
        {
            Folder parent;
            var n = Folder.FindNode(new DirectoryInfo(e.FullPath), this.root, out parent);
            if (parent == null)
            {
                logger.Error("file renaming could not be processed since the node was not found. new path:'{0}'", e.FullPath);
                return;
            }
            parent.DispatchedFileSync(() => parent.RemoveItem(n));
        }

        #endregion
    }
}
