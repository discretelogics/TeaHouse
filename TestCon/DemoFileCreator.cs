#if false

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeaTime;
using TeaTime.Data;

namespace TestCon
{
    static class Extensions
    {
        public static double Rounded(this double value)
        {
            return Math.Round(value, 2);
        }
    }

    class DemoFileCreator
    {
        public void WriteFile(string filename, int itemCount)
        {
            using(var tf = TeaFile<Event<OHLCV>>.Create(filename, "some demo content"))
            {
                Random r = new Random();
                Time t = new Time(2000, 1, 1);
                for (int i = 0; i < itemCount; i++)
                {
                    var value = Math.Sin(i * Math.PI * 2 / 100) * 1000;
                    Event<OHLCV> bar = new Event<OHLCV>();
                    bar.Value.Open = value;
                    bar.Value.Close = value + r.NextDouble() * 10;
                    bar.Value.High = bar.Value.Open + 15;
                    bar.Value.Low = bar.Value.Open - 15;

                    bar.Value.Open = bar.Value.Open.Rounded();
                    bar.Value.High = bar.Value.High.Rounded();
                    bar.Value.Low = bar.Value.Low.Rounded();
                    bar.Value.Close= bar.Value.Close.Rounded();
                    bar.Time = t;
                    t = t.AddDays(1);

                    tf.Write(bar);
                }
            }
        }

        public void WriteFoldersWithDemoFiles(string root, int filesperfolder)
        {
//            Directory.Delete(root, true);
            DirectoryInfo d = Directory.CreateDirectory(root);
            5.Times(i =>
                {
                    DirectoryInfo subdir = d.CreateSubdirectory("folder {0}".Formatted(i));
                    filesperfolder.Times(j => this.WriteFile(Path.Combine(subdir.FullName, "file {0}.tea".Formatted(j)), 5000));
                }
                );
        }
    }
}

#endif