// copyright discretelogics 2013.

using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SharpTestsEx;
using TeaTime.Data;

namespace TeaTime
{    
    [TestClass]
    public class CsvImporterTest : TestBase
    {
        [TestMethod]
        public void CreateInstanceOfCSVImporter()
        {
            var c = new CSVImportVM();
        }

        [TestMethod]
        public void AnalyzeFileDetectsFirstLineShallNotBeSkipped()
        {
            const string fileName = "csv1.txt";
            using (var f = File.CreateText(fileName))
            {
                5.Times((i) => f.WriteLine(SampleValuesFactory.GetOHLCV(i).CSV()));
            }
            CSVImportVM.Analyze(fileName).Should().Be.False();
        }

        [TestMethod]
        public void AnalyzeFileDetectsFirstLineShallBeSkipped()
        {
            const string fileName = "csv2.txt";
            using (var f = File.CreateText(fileName))
            {
                f.WriteLine("Time;Open;High;Low;Close;Volume");
                5.Times((i) => f.WriteLine(SampleValuesFactory.GetOHLCV(i).CSV()));
            }                        
            CSVImportVM.Analyze(fileName).Should().Be.True();
        }

        [TestMethod]
        public void ImportParametersJsonSerRoundTrip()
        {
            var p = new CSVImportParameters();
            p.DateTimeFormat = "a";
            p.DecimalSeparator = "b";
            p.FieldDelimeters = "c";
            p.FirstLineHoldsFieldNames = true;
            p.OverwriteExistingFile = true;
            p.TargetTypeName = "d";
            p.TeaFileFields.Add(new CSVFieldMapping(FieldTypeDescriptionManager.Instance.Get("time"), "timefield"));
            p.TeaFileFields.Add(new CSVFieldMapping(FieldTypeDescriptionManager.Instance.Get("double"), "openfield"));

            var json = JsonConvert.SerializeObject(p);
            Console.WriteLine(json);
            var p2 = JsonConvert.DeserializeObject<CSVImportParameters>(json);

            p2.DateTimeFormat.Should().Be(p.DateTimeFormat);
            p2.DecimalSeparator.Should().Be(p.DecimalSeparator);
            p2.FieldDelimeters.Should().Be(p.FieldDelimeters);
            p2.FirstLineHoldsFieldNames.Should().Be(p.FirstLineHoldsFieldNames);
            p2.OverwriteExistingFile.Should().Be(p.OverwriteExistingFile);
            p2.TargetTypeName.Should().Be(p.TargetTypeName);
            p2.TeaFileFields.Select(f => f.FieldTypeDesc.ToString() + f.Name).Should().Have.SameSequenceAs(
             p.TeaFileFields.Select(f => f.FieldTypeDesc.ToString() + f.Name));
        }

        /*[TestMethod]
  public void ParseLine()
  {
  	var c = new CSVImporter();
  	c.Parameters.Delimeter = ";";
  	var fields = c.Parse("aaa;bbb;ccc;ddd");

  	Assert.AreEqual(4, fields.Length);
  	Assert.AreEqual("aaa", fields[0]);
  	Assert.AreEqual("bbb", fields[1]);
  	Assert.AreEqual("ccc", fields[2]);
  	Assert.AreEqual("ddd", fields[3]);
  }*/

        //[TestMethod]
        //public void Preview()
        //{
        //    const string fileName = "csv3.txt";
        //    using (var f = File.CreateText(fileName))
        //    {
        //        f.WriteLine("Time;Open;High;Low;Close;Volume");
        //        int v = 70;
        //        5.Times(() => f.WriteLine(new Event<OHLCV>(new DateTime(2000, 1, 2), new OHLCV {Open = 200, High = 210, Low = 190, Close = 200, Volume = v++}).CSV()));
        //    }

        //    var c = new CSVImportVM();
        //    c.Parameters.Delimeter = ";";
        //    c.FileName = "csv3.txt";
        //    c.UpdatePreview();

        //    var p = c.Preview;

        //    Assert.IsNotNull(p);
        //}

#if false
  [TestMethod]
  public void ImportOHLCVData()
  {
    const string fileName = "csv4.txt";
    var bars = new List<Event<OHLCV>>();
    5.Times(() => bars.Add(SampleValuesFactory.GetOHLCV()));
    using (var f = File.CreateText(fileName))
    {
      bars.ForEach(ohlcv => f.WriteLine(ohlcv.CSV()));
    }

    var c = new CSVImportVM();
    c.Parameters.TargetType = typeof (Event<OHLCV>);
    c.FileName = fileName;
    c.Parameters.Delimeter = ";";
    c.Parameters.DateTimeFormat = "d.MM.yyyy HH:mm:ss";
    c.UpdatePreview();
    c.Import();

    using (var tf = TeaFile<Event<OHLCV>>.OpenRead(c.TargetFileName))
    {
      Assert.AreEqual(5, tf.Items.Count);
      bars.ForEachIndex((i, item) =>
      {
        var ohlcv = tf.Read();
        Assert.IsTrue(ohlcv.Equals(bars[i]));
      });
    }
  }

  [TestMethod]
  public void ImportOHLCVDataWithSpecialFormatting()
  {
    const string fileName = "csv5.txt";
    using (var f = File.CreateText(fileName))
    {
      f.WriteLine("08.11.2010 20:25:48;1537.47;1539.47;1534.47;1537.97;0");
      f.WriteLine("09.11.2010 21:26:48;1688.97;1690.97;1685.97;1689.47;0");
      f.WriteLine("10.11.2010 22:27:48;1355.85;1357.85;1352.85;1356.35;0");
      f.WriteLine("11.11.2010 23:28:48;1183.69;1185.69;1180.69;1184.19;0");
      f.WriteLine("13.11.2010 00:29:48;1496.22;1498.22;1493.22;1496.72;0");
    }

    var c = new CSVImportVM();
    c.Parameters.TargetType = typeof (OHLCV);
    c.FileName = fileName;
    c.Parameters.Delimeter = ";";
    c.Parameters.DecimalSeparator = ".";
    c.Parameters.DateTimeFormat = "d.MM.yyyy HH:mm:ss";
    c.UpdatePreview();
    c.Import();

    using (var tf = TeaFile<OHLCV>.OpenRead(c.TargetFileName))
    {
      Assert.AreEqual(5, tf.Items.Count);
      OHLCV ohlcv = tf.Read();
      Assert.AreEqual(1537.47, ohlcv.Open);
    }
  }

  [TestMethod]
  public void TargetFieldsAreSetOnTargetTypeSelection()
  {
    var c = new CSVImportVM();
    c.Parameters.TargetType = typeof (OHLCV);
    Assert.AreEqual(6, c.Parameters.TargetFieldNames.Count);
    Assert.AreEqual(1, c.Parameters.TargetFieldNames.Count(s => s == "Time"));
    Assert.AreEqual(1, c.Parameters.TargetFieldNames.Count(s => s == "Open"));
    Assert.AreEqual(1, c.Parameters.TargetFieldNames.Count(s => s == "High"));
    Assert.AreEqual(1, c.Parameters.TargetFieldNames.Count(s => s == "Low"));
    Assert.AreEqual(1, c.Parameters.TargetFieldNames.Count(s => s == "Close"));
    Assert.AreEqual(1, c.Parameters.TargetFieldNames.Count(s => s == "Volume"));

    c.Parameters.TargetType = typeof (Tick);
    Assert.AreEqual(4, c.Parameters.TargetFieldNames.Count);
    Assert.AreEqual(1, c.Parameters.TargetFieldNames.Count(s => s == "Time"));
    Assert.AreEqual(1, c.Parameters.TargetFieldNames.Count(s => s == "Price"));
    Assert.AreEqual(1, c.Parameters.TargetFieldNames.Count(s => s == "Volume"));
    Assert.AreEqual(1, c.Parameters.TargetFieldNames.Count(s => s == "Id"));

    c.Parameters.TargetType = typeof (OHLCV);
    Assert.AreEqual(6, c.Parameters.TargetFieldNames.Count);
    Assert.AreEqual(1, c.Parameters.TargetFieldNames.Count(s => s == "Time"));
    Assert.AreEqual(1, c.Parameters.TargetFieldNames.Count(s => s == "Open"));
    Assert.AreEqual(1, c.Parameters.TargetFieldNames.Count(s => s == "High"));
    Assert.AreEqual(1, c.Parameters.TargetFieldNames.Count(s => s == "Low"));
    Assert.AreEqual(1, c.Parameters.TargetFieldNames.Count(s => s == "Close"));
    Assert.AreEqual(1, c.Parameters.TargetFieldNames.Count(s => s == "Volume"));
  }
#endif

        [TestMethod]
        public void GetTeaFileNameFromTextFileNameTest()
        {
            var c = new CSVImportVM();
            var teafilename = c.GetTeaFileNameFromTextFileName(@"c:\folder\testfile.txt");
            Assert.AreEqual(@"c:\folder\testfile.tea", teafilename);
        }
    }

    static class CsvExtensions
    {
        public static string CSV(this Event<OHLCV> ohlcvEvent)
        {
            return String.Join(";", ohlcvEvent.Time, ohlcvEvent.Value.Open, ohlcvEvent.Value.High, ohlcvEvent.Value.Low, ohlcvEvent.Value.Close, ohlcvEvent.Value.Volume);
        }
    }
}
