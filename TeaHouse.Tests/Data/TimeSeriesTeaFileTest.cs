#if false

// copyright discretelogics © 2011
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;
using TeaTime.IO;

namespace TeaTime.Data
{
    [TestClass]
    public unsafe class TimeSeriesTeaFileTest
    {
        #region Initialize/CleanUp

        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            if (!Directory.Exists(tempDirectory))
            {
                tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDirectory);
            }
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }

        #endregion

        [TestMethod]
        public void OpenReadAndDispose()
        {
            string file = CreateOHLVCSampleFile("OpenRead");
            using (var ts = TeaFactory.Instance.OpenReadTyped(file))
            {
            }
        }

        [TestMethod]
        [ExpectedException(typeof (InvalidOperationException))]
        public void OpenReadNoTimeSeries()
        {
            string file = tempDirectory.PathCombine("OpenReadNoTimeSeries.tea");
            using (TeaFile<NoTimeStamp>.Create(file))
            {
            }
            using (TimeSeriesTeaFile.OpenRead(file))
            {
            }
        }

        [TestMethod]
        public void OpenReadNoTimeSeriesCleansUpResources()
        {
            string file = tempDirectory.PathCombine("OpenReadNoTimeSeriesCleansUpResources.tea");
            using (TeaFile<NoTimeStamp>.Create(file))
            {
            }
            try
            {
                using (TimeSeriesTeaFile.OpenRead(file))
                {
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                IOUtils.IsFileInUse(file).Should().Be.False();
            }
        }

        [TestMethod]
        public void ReadItemsForward()
        {
            string file = CreateOHLVCSampleFile("ReadItemsForward");
            using (TimeSeriesTeaFile ts = TimeSeriesTeaFile.OpenRead(file))
            {
                for (int i = 0; i < sampleFileItemsCount; i++)
                {
                    var value = *(OHLCV*) ts.EventValueAt(i);
                    var time = ts.TimeAt(i);
                    AssertSampleOHLCVAt(value, time, i);
                }
            }
        }

        [TestMethod]
        public void ReadItemsBackward()
        {
            string file = CreateOHLVCSampleFile("ReadItemsBackward");
            using (TimeSeriesTeaFile ts = TimeSeriesTeaFile.OpenRead(file))
            {
                for (int i = sampleFileItemsCount - 1; i >= 0; i--)
                {
                    var value = *(OHLCV*) ts.EventValueAt(i);
                    var time = ts.TimeAt(i);
                    AssertSampleOHLCVAt(value, time, i);
                }
            }
        }

        //[TestMethod]
        //public void ReadItemT()
        //{
        //    string file = CreateOHLVCSampleFile("ReadItemT");
        //    using (TimeSeriesTeaFile ts = TimeSeriesTeaFile.OpenRead(file))
        //    {
        //        for (int i = 0; i < sampleFileItemsCount; i++)
        //        {
        //            var item = ts.ItemAt<Event<OHLCV>>(i);
        //            AssertSampleOHLCVAt(item.Value, item.Time, i);
        //        }
        //    }
        //}

        #region private methods

        private string CreateOHLVCSampleFile(string name)
        {
            string file = Path.Combine(tempDirectory, name + ".tea");
            using (var tf = TeaFile<Event<OHLCV>>.Create(file))
            {
                DateTime startTime = new DateTime(2000, 1, 1, 0, 0, 0);
                double offset = 888;
                for (int i = 0; i < sampleFileItemsCount; i++)
                {
                    var value = new OHLCV();
                    value.Close = i + offset;
                    value.Open = value.Close + 0.1;
                    value.High = value.Close + 0.8;
                    value.Low = value.Close - 0.8;
                    value.Volume = i;

                    var ohlcvEvent = new Event<OHLCV>(startTime.AddDays(i), value);

                    tf.Write(ohlcvEvent);
                }
            }
            return file;
        }

        private void AssertSampleOHLCVAt(OHLCV actualValue, DateTime actualTime, int i)
        {
            Assert.AreEqual(i + 888, actualValue.Close);
            Assert.AreEqual(actualValue.Close + 0.1, actualValue.Open);
            Assert.AreEqual(actualValue.Close + 0.8, actualValue.High);
            Assert.AreEqual(actualValue.Close - 0.8, actualValue.Low);
            Assert.AreEqual<double>(i, actualValue.Volume);
            Assert.AreEqual(new DateTime(2000, 1, 1, 0, 0, 0).AddDays(i), actualTime);
        }

        #endregion

        #region fields

        private const int sampleFileItemsCount = 100;
        private static string tempDirectory;

        #endregion

        #region embedded Types

        public struct NoTimeStamp
        {
            public double Value;
        }

        #endregion
    }
}

#endif