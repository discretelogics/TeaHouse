// copyright discretelogics 2013.

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TeaTime.Data
{
    [TestClass]
    public class ExportCSVTest
    {
        [TestMethod]
        public void ExportedCVSFileHasCorrectFormat()
        {
            const string teafile = "ExportCSVTest.ExportedCVSFileHasCorrectFormat.tea";
            const string csvfile = "ExportCSVTest.ExportedCVSFileHasCorrectFormat.csv";
            File.Delete(teafile);
            File.Delete(csvfile);
            using (var tf = TeaFile<Event<OHLCV>>.Create(teafile))
            {
                tf.Write(new Event<OHLCV>(new Time(2000, 2, 3, 11, 22, 33), new OHLCV {Open = 11, High = 22, Low = 5, Close = 12}));
            }

            var p = new CSVExportParameters();
            p.DateFormat = "dd.MM.yyyy";
            p.Delimeter = ";";
            p.TimeFormat = "";
            Tools.ExportToCSV(teafile, csvfile, p, null, null);

            string csv = File.ReadAllText(csvfile);
            Console.WriteLine(csv);
        }
    }
}
