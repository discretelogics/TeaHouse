using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TeaTime.Data
{
    public class TeaFileWatcher
    {
        #region state

        readonly object registrationSync;
        readonly Dictionary<string, FileSystemWatcher> fileSystemWatchers;
        readonly ConcurrentDictionary<string, ITeaFileEditor> subscribers;
        readonly ConcurrentDictionary<string, object> updateWorkers;
        readonly TimeSpan writeTimeBuffer;

        #endregion

        #region core
        public IDisposable Subscribe(string fullname, ITeaFileEditor editor)
        {
            Register(fullname, editor);

            return new Subscription(this, fullname);
        }
        #endregion

        #region int
        private TeaFileWatcher()
        {
            this.registrationSync = new object();
            fileSystemWatchers = new Dictionary<string, FileSystemWatcher>();
            subscribers = new ConcurrentDictionary<string, ITeaFileEditor>();
            updateWorkers = new ConcurrentDictionary<string, object>();
            writeTimeBuffer = TimeSpan.FromMilliseconds(500);
        }

        void FswChanged(object sender, FileSystemEventArgs e)
        {
            var fullname = IOUtils.GetComparablePath(e.FullPath);
            if (IOUtils.IsFile(fullname) && subscribers.ContainsKey(fullname) && updateWorkers.TryAdd(fullname, null))
            {
                Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            while (subscribers.ContainsKey(fullname))
                            {
                                try
                                {
                                    var fi = new FileInfo(fullname);
                                    if (DateTime.UtcNow - fi.LastWriteTimeUtc > writeTimeBuffer)
                                        break;
                                    Thread.Sleep(1000);
                                }
                                catch (FileNotFoundException)
                                {
                                    return;
                                }
                                catch (IOException)
                                {
                                }
                            }

                            ITeaFileEditor subscriber;
                            if (subscribers.TryGetValue(fullname, out subscriber))
                            {
                                subscriber.Update(new Change());
                            }
                        }
                        finally
                        {
                            object dummy;
                            updateWorkers.TryRemove(fullname, out dummy);
                        }
                    }, TaskCreationOptions.LongRunning);
            }
        }

        void Register(string fullname, ITeaFileEditor editor)
        {
            lock (registrationSync)
            {
                fullname = IOUtils.GetComparablePath(fullname);
                subscribers.TryAdd(fullname, editor);

                string dir = IOUtils.GetComparablePath(Path.GetDirectoryName(fullname));
                if (!fileSystemWatchers.ContainsKey(dir))
                {
                    var fsw = new FileSystemWatcher();
                    fsw.Path = dir;
                    fsw.NotifyFilter = NotifyFilters.LastWrite;
                    fsw.Changed += this.FswChanged;
                    fileSystemWatchers.Add(dir, fsw);
                    fsw.EnableRaisingEvents = true;
                }

            }
        }
        void Unregister(string fullname)
        {
            lock (registrationSync)
            {
                fullname = IOUtils.GetComparablePath(fullname);
                ITeaFileEditor dummy;
                subscribers.TryRemove(fullname, out dummy);

                string dir = IOUtils.GetComparablePath(Path.GetDirectoryName(fullname));
                if (!subscribers.Any(f => IOUtils.AreEqualPaths(Path.GetDirectoryName(f.Key), dir)))
                {
                    fileSystemWatchers[dir].EnableRaisingEvents = false;
                    fileSystemWatchers[dir].Dispose();  // hopefully not hanging like on the tree...
                    fileSystemWatchers.Remove(dir);
                }
            }
        }

        class Change : IChange
        {

        }
        class Subscription : IDisposable
        {
            readonly TeaFileWatcher parent;
            readonly string fullname;

            public Subscription(TeaFileWatcher parent, string fullname)
            {
                this.parent = parent;
                this.fullname = fullname;
            }

            public void Dispose()
            {
                parent.Unregister(fullname);
            }
        }
        #endregion

        #region Singleton
        public static TeaFileWatcher Instance { get { return Singleton.Instance; } }

        static class Singleton
        {
            static Singleton() { }
            internal static readonly TeaFileWatcher Instance = new TeaFileWatcher();
        }
        #endregion
    }
}
