// copyright discretelogics 2013.

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace TeaTime.Data
{
    [TestClass]
    public class ToolsTest : TestBase
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
                tf.Write(new Event<OHLCV>(new Time(2000, 3, 26, 11, 22, 33), new OHLCV {Open = 11.7d, High = 22, Low = 5, Close = 12, Volume = 3333}));
            }

            var p = new CSVExportParameters();
            p.DateTimeFormat = "d.M.yyyy";
            p.Delimeter = "$";
            p.DecimalSeparator = "+";
            Export.ExportToCSV(teafile, csvfile, p, null, null);

            var csvlines = File.ReadAllLines(csvfile);
            csvlines.Should().Have.Count.EqualTo(2);
            csvlines[0].Should().Be("Time$Open$High$Low$Close$Volume");
            csvlines[1].Should().Be("26.3.2000$11+7$22$5$12$3333");
        }

        [TestMethod]
        public void ExportImportRoundTrip()
        {
            // create teafile
            var teafile = base.GetFileName("tea");
            using (var tf = TeaFile<Event<Tick>>.Create(teafile))
            {
                Time t = new Time(2000, 3, 16);
                Enumerable.Range(10, 5).ForEach(n => tf.Write(new Event<Tick>(t.AddDays(n), new Tick() {Id = n, Price = n * 1.1, Volume = n * 222})));
            }
            Console.WriteLine(TeaFileSnapshot.Get(teafile));

            // export to csv
            var csvfile = base.GetFileName("csv");
            var pe = new CSVExportParameters();
            pe.DateTimeFormat = "d.M.yyyy";
            Export.ExportToCSV(teafile, csvfile, pe, null, null);

            Console.WriteLine(File.ReadAllText(csvfile));

            // import csv
            var teafile2 = base.GetFileName("tea", true, "import");
            var p = new CSVImportParameters();
            p.DateTimeFormat = "d.M.yyyy";            
            p.TeaFileFields.Add(new CSVFieldMapping(FieldTypeDescriptionManager.Instance.Get("time"), "time"));
            p.TeaFileFields.Add(new CSVFieldMapping(FieldTypeDescriptionManager.Instance.Get("double"), "price"));
            Import.ImportCSV(csvfile, teafile2, p, Timescale.Java, null);

            // display the imported file
            Console.WriteLine(teafile2);
            var snapshot = TeaFileSnapshot.Get(teafile2);
            Console.WriteLine(snapshot);
            snapshot.Description.Timescale.Value.Should().Be(Timescale.Java);
        }
    }
}
