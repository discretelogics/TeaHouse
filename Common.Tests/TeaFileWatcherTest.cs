using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TeaTime.Data;

namespace TeaTime
{
    [TestClass]
    [Ignore] // tbd
    public class TeaFileWatcherTest
    {
        string rootDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "UnitTests");

        [TestInitialize]
        public void Initialize()
        {
            if (Directory.Exists(rootDirectory))
                Directory.Delete(rootDirectory, true);
            Directory.CreateDirectory(rootDirectory);
        }

        [TestMethod]
        public void Update_Fired()
        {
            var file = this.Create(null, "file1");
            var editor = new TestEditor(file);

            this.AppendItem(file);
            editor.AssertUpdated();

            this.Truncate(file);
            editor.AssertUpdated();
        }

        [TestMethod]
        public void Register_one_and_unregister()
        {
            var file = this.Create(null, "file1");
            var editor = new TestEditor(file);

            this.AppendItem(file);
            editor.AssertUpdated();

            editor.Dispose();

            this.AppendItem(file);
            editor.AssertNotUpdated();
        }

        [TestMethod]
        public void Register_two_in_same_folder_and_unregister_one()
        {
            var file1 = this.Create("folder1", "file1");
            var file2 = this.Create("folder1", "file2");

            var editor1 = new TestEditor(file1);
            var editor2 = new TestEditor(file2);

            editor1.Dispose();

            this.AppendItem(file1);
            editor1.AssertNotUpdated();
            editor2.AssertNotUpdated();

            this.AppendItem(file2);
            editor2.AssertUpdated();
        }

        [TestMethod]
        public void Register_two_in_different_folders_and_unregister_one()
        {
            var file1 = this.Create("folder1", "file1");
            var editor1 = new TestEditor(file1);

            var file2 = this.Create("folder2", "file2");
            var editor2 = new TestEditor(file2);

            editor1.Dispose();

            this.AppendItem(file1);
            editor2.AssertNotUpdated();

            this.AppendItem(file2);
            editor2.AssertUpdated();
        }

        string Create(string folder, string file)
        {
            string directory = rootDirectory;
            if (folder.IsSet())
            {
                directory = Path.Combine(rootDirectory, folder);
                Directory.CreateDirectory(directory);
            }
            string fullname = Path.Combine(directory, file + ".tea");
            using (var tf = TeaFile<Event<OHLC>>.Create(fullname))
            {
                tf.Write(new Event<OHLC>(DateTime.UtcNow, new OHLC { Open = 10, High = 11, Low=9, Close = 9.5}));
            }
            return fullname;
        }
        void AppendItem(string fullname)
        {
            using (var tf = TeaFile<Event<OHLC>>.OpenWrite(fullname))
            {
                tf.Write(new Event<OHLC>(DateTime.UtcNow, new OHLC { Open = 12, High = 13, Low=11, Close = 11.5}));
            }
        }
        void Truncate(string fullname)
        {
            using (var tf = TeaFile<Event<OHLC>>.OpenWrite(fullname))
            {
                tf.Truncate();
            }
        }

        class TestEditor : ITeaFileEditor, IDisposable
        {
            string fullname;
            IDisposable subscription;
            public TestEditor(string fullname)
            {
                this.fullname = fullname;
                subscription = TeaFileWatcher.Instance.Subscribe(fullname, this);
            }

            public void SetTeaFileIndex(object sender, long tsi)
            {
                throw new NotImplementedException();
            }

            readonly ManualResetEvent updated = new ManualResetEvent(false);
            public void Update(IChange change)
            {
                updated.Set();
            }
            public void AssertUpdated()
            {
                try
                {
                    if (!updated.WaitOne(TimeSpan.FromSeconds(3)))
                    {
                        Assert.Fail("No update event was raised for {0} in time.", fullname);
                    }
                }
                finally
                {
                    updated.Reset();
                }
            }
            public void AssertNotUpdated()
            {
                try
                {
                    if (updated.WaitOne(TimeSpan.FromSeconds(3)))
                    {
                        Assert.Fail("Unexpected update event was raised for {0}.", fullname);
                    }
                }
                finally
                {
                    updated.Reset();
                }
            }


            public void Dispose()
            {
                subscription.Dispose();
            }
        }
    }
}
