using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace TeaTime
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string warehouse = Environment.GetEnvironmentVariable("TeaTimeWarehousePath", EnvironmentVariableTarget.User);
                if (String.IsNullOrEmpty(warehouse))
                {
                    warehouse = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "DiscreteLogics", "Data", "Test");
                    Console.WriteLine("Warehouse is not configured. Defaulting to {0}", warehouse);
                }
                else
                {
                    warehouse = Path.Combine(warehouse, "Test");
                    Console.WriteLine("File will be created in {0}", warehouse);
                }
                Console.Write("FileName (default Test.tea): ");
                var name = Console.ReadLine();
                if (String.IsNullOrWhiteSpace(name))
                {
                    name = "Test.tea";
                }
                Console.Write("ItemsCount (default 250,000,000): ");
                var cs = Console.ReadLine();
                long count = 250000000;
                if (!String.IsNullOrWhiteSpace(cs))
                {
                    count = long.Parse(cs);
                }
                
                Console.Write("Are you sure you want to create the test file (y/n)? ");
                string confirm = null;
                do
                {
                    confirm = Console.ReadLine();
                } while (!String.Equals(confirm, "y", StringComparison.InvariantCultureIgnoreCase) && !String.Equals(confirm, "yes", StringComparison.InvariantCultureIgnoreCase));

                if (!Directory.Exists(warehouse))
                    Directory.CreateDirectory(warehouse);

                var fullname = Path.Combine(warehouse, name);

                var sw = new Stopwatch();
                sw.Start();

                using (var s = File.Create(fullname))
                using (var tf = TeaFile<Event<double>>.Create(s, false))
                {
                    var start = new DateTime(1980, 1, 1);
                    for (long l = 0; l < count; l++)
                    {
                        tf.Write(new Event<double>(start.AddSeconds(l), Math.Sin(l / 10.0) * 20 + 40));
                        if (l > 0 && l % 1000000 == 0)
                        {
                            Console.WriteLine("Written {0}/{1} items.", l, count);
                        }
                    }
                }

                sw.Stop();
                Console.WriteLine("Written the test file ({0} items) in {1} successfully.", count, sw.Elapsed);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create test file: {0}", ex.Message);
            }
        }
    }
}
