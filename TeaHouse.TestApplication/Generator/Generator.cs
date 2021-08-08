using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TeaTime;
using TeaTime.Data;

namespace TeaHouse.TestApplication
{
    public class Generator
    {
        bool stopped;
        GeneratorParameters parameters;
        public event Action<string> OnFileUpdate;

        public void Start(GeneratorParameters p)
        {
            this.parameters = p;
            stopped = false;
            int ms = (int)((1.0 / p.UpdatesPerMinute) * 60 * 1000);
            Task.Factory.StartNew(() => Process(ms));
        }

        void Process(int ms)
        {
            while (!this.stopped)
            {                
                CreateRandomTeaFile();
                if (OnFileUpdate != null) OnFileUpdate("filename");
                Thread.Sleep(ms);
            }
        }

        public void Stop()
        {
            stopped = true;
        }

        void CreateRandomTeaFile()
        {
            Console.WriteLine("generate file");
            var p = this.parameters;

            using (var fs = new FileStream(p.Filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read))
            using (var tf = TeaFile<Event<Tick>>.Create(fs))
            {
                int count = p.ValueCount;
                DateTime t = new DateTime(2000, 1, 1);
                if(p.VaryValueCountAndTime)
                {
                    Random r = new Random();
                    count += r.Next(0, p.ValueCount * 3);
                    t = t.AddDays(r.Next(0, 100));
                }
                for (int i = 0; i < count; i++)
                {
                    double price = Math.Sin(i / 10.0) * 20 + 40;
                    tf.Write(new Event<Tick> { Time = t.AddHours(i), Value = new Tick() { Id = 700 + i, Price = price, Volume = i * 300 + 3000 } });
                }
            }
        }

        #region singleton

        public static Generator Instance { get { return Singleton.instance; } }

        // ReSharper disable ClassNeverInstantiated.Local
        class Singleton
        // ReSharper restore ClassNeverInstantiated.Local
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            // ReSharper disable EmptyConstructor
            static Singleton()
            // ReSharper restore EmptyConstructor
            {
            }

            internal static readonly Generator instance = new Generator();
        }

        #endregion
    }

    public class GeneratorParameters
    {
        public int UpdatesPerMinute;
        public int ValueCount;
        public bool VaryValueCountAndTime;
        public string Filename;
    }
}
