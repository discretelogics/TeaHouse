// copyright discretelogics © 2011
using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TeaTime.Chart.Core;

namespace TeaTime
{
    [TestClass]
    public class SimpleTapeViewTest
    {
        #region Initialize/Cleanup

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDirectory);

            tsFullname = CreateMinuteTs("ts", 1000, new DateTime(2000, 1, 1, 0, 0, 0));
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            if (Directory.Exists(tempDirectory))
            {
                Directory.Delete(tempDirectory, true);
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            ts = TeaFactory.Instance.OpenReadTyped(tsFullname);
            tape = new SimpleTapeView();
            tape.Add(new [] {ts});
        }

        [TestCleanup]
        public void TestCleanup()
        {
            ts.Dispose();
        }

        #endregion

        [TestMethod]
        public void IndexAtForAllTsTimes()
        {
            for (int i = 0; i < ts.Count; i++)
            {
                IndexAt(ts.TimeAt(i), (int) ts.Count - 1 - i);
            }
        }

        #region private methods

        private static string CreateMinuteTs(string name, int length, DateTime startTime)
        {
            string fullname = Path.Combine(tempDirectory, name + ".tea");
            using (var tf = TeaFile<Event<int>>.Create(fullname))
            {
                for (int i = 0; i < length; i++)
                {
                    tf.Write(new Event<int>(startTime.AddMinutes(i), i));
                }
            }

            return fullname;
        }

        private void IndexAt(DateTime t, long expectedTi)
        {
            long ti = tape.IndexAt(t, new RangeL(0, tape.Length - 1));
            Assert.AreEqual(expectedTi, ti, "IndexAt(DateTime) returned an unexpected result");
        }

        #endregion

        #region fields

        private static string tempDirectory;
        private static string tsFullname;

        private SimpleTapeView tape;
        private ITeaFile ts;

        #endregion
    }
}