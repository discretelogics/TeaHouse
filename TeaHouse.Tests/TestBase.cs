// copyright discretelogics 2013.

using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TeaTime
{
    public abstract class TestBase
    {
        [ThreadStatic]
        string lastCleanedDirectory;

        public TestContext TestContext { get; set; }

        public string GetFileName(string extension = "tmp", bool autoclean = true, [CallerMemberName] string testclassname = null, [CallerMemberName] string testsubdir = null)
        {
            var testoutdir = this.TestContext.TestDir;
            if (Directory.Exists("m:")) testoutdir = "m:/testout"; // this is the RAM on my local machine

            Console.WriteLine(1);

            var d = Path.Combine(testoutdir, testsubdir);
            if (!Directory.Exists(d))
            {
                Console.WriteLine(2);
                Directory.CreateDirectory(d);
                this.lastCleanedDirectory = d;
            }
            else if (autoclean)
            {
                Console.WriteLine(3);
                if (this.lastCleanedDirectory != d)
                {
                    Console.WriteLine(4);
                    Directory.EnumerateFiles(d).ForEach(File.Delete);
                    this.lastCleanedDirectory = d;
                    Console.WriteLine("where is the diretory?");
                    Console.WriteLine(Directory.Exists(d));
                }
            }

            var f = Path.Combine(d, testclassname);
            var filepath = Path.ChangeExtension(f, extension);
            return filepath;
        }
    }
}
