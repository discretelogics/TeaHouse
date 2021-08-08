// copyright discretelogics © 2011
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TeaTime.Chart.Core;

namespace TeaTime
{
	[TestClass]
	public class TapeViewTest
	{
		#region Initialize/Cleanup

		[ClassInitialize]
		public static void ClassInitialize(TestContext testContext)
		{
			tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			Directory.CreateDirectory(tempDirectory);

			ts1Fullname = CreateMinuteTs("ts1", 2001, new DateTime(2000, 1, 1, 0, 0, 0));
			ts2Fullname = CreateMinuteTs("ts2", 201, new DateTime(2000, 1, 1, 0, 0, 30));
			ts3Fullname = CreateMinuteTs("ts3", 101, new DateTime(1900, 1, 1, 0, 0, 0));
			ts4Fullname = CreateMinuteTs("ts4", 2001, new DateTime(2000, 1, 1, 0, 10, 0));
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
			lengthChanged = new List<long>();
			tape = new TapeView();
			tape.LengthChanged += TapeLengthChanged;

			ts1 = TeaFactory.Instance.OpenReadTyped(ts1Fullname);
			ts2 = TeaFactory.Instance.OpenReadTyped(ts2Fullname);
			ts3 = TeaFactory.Instance.OpenReadTyped(ts3Fullname);
			ts4 = TeaFactory.Instance.OpenReadTyped(ts4Fullname);
		}

		[TestCleanup]
		public void TestCleanup()
		{
			ts1.Dispose();
			ts2.Dispose();
			ts3.Dispose();
			ts4.Dispose();
		}

		#endregion

		[TestMethod]
		public void AddTs1AndGetStartAndEnd()
		{
			AddTss(2001, ts1);
			Get(0, new DateTime(2000, 1, 1, 0, 0, 0).AddMinutes(2000), null);
			Get(2000, new DateTime(2000, 1, 1, 0, 0, 0), null);
		}

		[TestMethod]
		public void AddTs1Ts2AndGetStartAnd2001AndEnd()
		{
			AddTss(2202, ts1, ts2);
			Get(0, new DateTime(2000, 1, 1, 0, 0, 0).AddMinutes(2000), null);
			Get(2000, new DateTime(2000, 1, 1, 0, 0, 30).AddMinutes(100), null);
			Get(2201, new DateTime(2000, 1, 1, 0, 0, 0), null);
		}

		[TestMethod]
		public void AddTs1Ts3AndGetStartAndEnd()
		{
			AddTss(2102, ts1, ts3);
			Get(0, new DateTime(2000, 1, 1, 0, 0, 0).AddMinutes(2000), null);
			Get(2101, new DateTime(1900, 1, 1, 0, 0, 0), null);
		}

		[TestMethod]
		public void AddTs1Ts4AndGetStartAndEnd()
		{
			AddTss(4002, ts1, ts4);
			Get(0, new DateTime(2000, 1, 1, 0, 10, 0).AddMinutes(2000), 3011);
			Get(3010, new DateTime(2000, 1, 1, 0, 0, 0), 2011);
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

        void TapeLengthChanged(object sender, EventArgs<long> e)
		{
			lengthChanged.Add(e.Value);
		}

		private void AddTss(int expectedLength, params ITeaFile[] tss)
		{
			int eventsBefore = lengthChanged.Count;
			tape.Add(tss);

			Assert.AreEqual(expectedLength, tape.Length, "TapeView.Length was unexpected.");
			Assert.AreEqual(eventsBefore + 1, lengthChanged.Count, "Expected to receive 1 LengthChanged event.");
			Assert.AreEqual(expectedLength, lengthChanged.Last(), "The received LengthChanged event did publish an unexpected value.");
		}

        private void Get(long index, DateTime expectedTime, long? expectedLengthChanged)
		{
			int eventsBefore = lengthChanged.Count;
			index = tape.ComputeTapeRange(new RangeL(index, index)).Start;
			DateTime time = tape.TimeAt(index);

			Assert.AreEqual(expectedTime, time, "TapeView.TimeAt did not return the expected time.");
			if (expectedLengthChanged.HasValue)
			{
				Assert.AreEqual(expectedLengthChanged.Value, tape.Length, "TapeView.Length was unexpected.");
				Assert.AreEqual(eventsBefore + 1, lengthChanged.Count, "Expected to receive 1 LengthChanged event.");
				Assert.AreEqual(expectedLengthChanged.Value, lengthChanged.Last(), "The received LengthChanged event did publish an unexpected value.");
			}
			else
			{
				Assert.AreEqual(eventsBefore, lengthChanged.Count, "Did not expect a LengthChanged event.");
			}
		}

		#endregion

		#region fields

		private static string tempDirectory;
		private static string ts1Fullname;
		private static string ts2Fullname;
		private static string ts3Fullname;
		private static string ts4Fullname;
		private List<long> lengthChanged;
		private TapeView tape;

		private ITeaFile ts1;
		private ITeaFile ts2;
		private ITeaFile ts3;
		private ITeaFile ts4;

		#endregion
	}
}