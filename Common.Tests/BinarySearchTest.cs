// copyright discretelogics © 2011
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace TeaTime
{
	/// <summary>
	///     Summary description for BinarySearchTest
	/// </summary>
	[TestClass]
	public class BinarySearchTest
	{
		public TestContext TestContext { get; set; }

		[TestMethod]
		public void BinarySearchTest1()
		{
			var values = new List<long>();
			Algorithms.BinarySearch(i => values[(int)i], 0, -1, 0, RoundMode.Up).Should().Be(-1);
            Algorithms.BinarySearch(i => values[(int)i], 0, -1, 1, RoundMode.Up).Should().Be(-1);
            Algorithms.BinarySearch(i => values[(int)i], 0, -1, 1, RoundMode.Up).Should().Be(-1);
		}

		[TestMethod]
		[ExpectedException(typeof (ArgumentOutOfRangeException))]
		public void BinarySearchThrowsExceptionWhenIndicesAreSetWrongTest()
		{
            var values = new List<long>();
			Algorithms.BinarySearch(i => values[(int)i], 0, 3, 1, RoundMode.Up);
		}

		[TestMethod]
		public void BinarySearchReturnsNegativeValueIfNothingFound()
		{
            var values = new List<long>();
			values.Add(40);
            Algorithms.BinarySearch(i => values[(int)i], 0, values.Count - 1, 0).Should().Be.LessThan(0);
            Algorithms.BinarySearch(i => values[(int)i], 0, values.Count - 1, 1).Should().Be.LessThan(0);
            Algorithms.BinarySearch(i => values[(int)i], 0, values.Count - 1, 1).Should().Be.LessThan(0);
		}

		[TestMethod]
		public void BinarySearchReturns0IfNothingFoundAndRoundUp()
		{
            var values = new List<long>();
			values.Add(40);
            Algorithms.BinarySearch(i => values[(int)i], 0, values.Count - 1, 0, RoundMode.Up).Should().Be(0);
            Algorithms.BinarySearch(i => values[(int)i], 0, values.Count - 1, 1, RoundMode.Up).Should().Be(0);
            Algorithms.BinarySearch(i => values[(int)i], 0, values.Count - 1, 1, RoundMode.Up).Should().Be(0);
		}

		[TestMethod]
		public void BinarySearchReturns0IfNothingFoundAndRoundDown()
		{
            var values = new List<long>();
			values.Add(40);
            Algorithms.BinarySearch(i => values[(int)i], 0, values.Count - 1, 0, RoundMode.Down).Should().Be(-1);
            Algorithms.BinarySearch(i => values[(int)i], 0, values.Count - 1, 1, RoundMode.Down).Should().Be(-1);
            Algorithms.BinarySearch(i => values[(int)i], 0, values.Count - 1, 1, RoundMode.Down).Should().Be(-1);
		}

		[TestMethod]
		public void BinarySearchTestFindOnSIngleValueCollectionSucceedsTest()
		{
            var values = new List<long>();
			values.Add(40);
			Algorithms.BinarySearch(i => values[(int)i], 0, values.Count - 1, 40, RoundMode.Up).Should().Be(0);
		}

		[TestMethod]
		public void BinarySearchTestFindFailsTest()
		{
            var values = new List<long>();
			values.Add(40);
			Algorithms.BinarySearch(i => values[(int)i], 0, values.Count - 1, 33).Should().Be(-1);
            Algorithms.BinarySearch(i => values[(int)i], 0, values.Count - 1, 55).Should().Be(-1);
		}

		[TestMethod]
		public void RandomTest()
		{
			var r = new Random(1777);

			for (int i = 0; i < 100; i++)
			{
				var n = r.Next(0, 172);
				var values = new SortedSet<int>();
				var min = r.Next(-10000, 2000);
				var max = r.Next(min+1, 10000);
				n.Times(() => values.Add(r.Next(min, max))); // sorted set ignores duplicates
				Console.WriteLine(values.Select(v => v.ToString()).Joined(","));
				var list = values.ToList();
				var x = r.Next(min, max);

				CompareBinarySearchAgainstListBinarySearch(list, x);
			}
		}

		[TestMethod]
		public void SingleValuesSearch()
		{
			var list = new List<int>();
			list.Add(2);
			CompareBinarySearchAgainstListBinarySearch(list, 7);
		}

		[TestMethod]
		public void SampleValueTest()
		{
			var list = new List<int>();
			CompareBinarySearchAgainstListBinarySearch(list, 7);
			list.Add(2);
			CompareBinarySearchAgainstListBinarySearch(list, 7);
			list.Add(3);
			CompareBinarySearchAgainstListBinarySearch(list, 7);
			list.Add(4);
			CompareBinarySearchAgainstListBinarySearch(list, 7);
			list.Add(5);
			CompareBinarySearchAgainstListBinarySearch(list, 7);
		}

		void CompareBinarySearchAgainstListBinarySearch(List<int> list, int x)
		{
			var i1 = list.BinarySearch(x);
			var i2 = Algorithms.BinarySearch(j => list[(int)j], 0, list.Count - 1, x, (a, b) => a.CompareTo(b));

			if(i1 >= 0)
			{
				i1.Should().Be(i2);				
			}
			else
			{
				i2.Should().Be.LessThan(0);
			}
		}
	}
}