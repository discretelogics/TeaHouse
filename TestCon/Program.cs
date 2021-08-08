// copyright discretelogics © 2011
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using TeaTime;
using TeaTime.Data;

namespace TestCon
{
    struct Measurement
    {
        public double Min;
        public double Max;
        public double Level;
    }

    public class Program
    {
        public static void Main()
        {
            try
            {
                File.Delete("m:/measurements.tea");
                using (var tf = TeaFile<Event<Measurement>>.Create("m:/measurements.tea", "Load at machine xenos 72"))
                {
                    DateTime t = new DateTime(2000, 1, 1);
                    for (int i = 0; i < 1000000000; i++)
                    {
                        double v = Math.Sin(i / 10.0) * 20 + 40;
                        tf.Write(new Event<Measurement>(t.AddSeconds(i), new Measurement { Min = v, Max = v + 3, Level = v % 7 }));
                        if (tf.Stream.Position > 3500000000) break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return;

            using (var f = new StreamWriter("out.log"))
            {
                Console.SetOut(f);
                try
                {
                    var ass = Assembly.LoadFrom(@"C:\Users\hase\AppData\Local\Microsoft\VisualStudio\11.0Exp\Extensions\DiscreteLogics\TeaHouse\1.0\DiscreteLogics.TeaHouse.dll");
                    var t = ass.GetType("TeaTime.TeaHousePackage", true);
                    Console.WriteLine(t.FullName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    Console.WriteLine("done");
                }
            }
        }

        struct ABC
        {
            public double A;
            public double B;
            public double C;
        }

        struct Longibert
        {
            public double Loooooooooooooooooooooong;
        }
    }
}
