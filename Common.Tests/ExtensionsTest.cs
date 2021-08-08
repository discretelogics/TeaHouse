using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;
using TeaTime.Data;

namespace TeaTime
{
	[TestClass]
	public class ExtensionsTest
	{
		[TestMethod]
		public void StreamReaderReadLinesTest()
		{
			using (var f = File.CreateText("test.txt"))
			{
				10.Times(n => f.WriteLine(n.ToString()));
			}

			using (var f = File.OpenRead("test.txt"))
			using (var sr = new StreamReader(f))
			{
				sr.ReadLines(5).Joined("-").Should().Be("0-1-2-3-4");
			}

			using (var f = File.OpenRead("test.txt"))
			using (var sr = new StreamReader(f))
			{
				sr.ReadLines(int.MaxValue).Joined("-").Should().Be("0-1-2-3-4-5-6-7-8-9");
			}
		}

        [TestMethod]
        public void IsEventOfPrimitiveTest()
        {
            var t = TeaFactory.Instance.CreateType(ItemDescription.FromAnalysis<Event<double>>());
            t.IsEventOfPrimitive().Should().Be.True();
            t = TeaFactory.Instance.CreateType(ItemDescription.FromAnalysis<Event<long>>());
            t.IsEventOfPrimitive().Should().Be.True();
            t = TeaFactory.Instance.CreateType(ItemDescription.FromAnalysis<Event<OHLCV>>());
            t.IsEventOfPrimitive().Should().Be.False();
        }
	}
}
