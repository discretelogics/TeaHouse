using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace TeaTime
{
	[TestClass]
	public class RangeTests
	{
		[TestMethod]
		public void RangeTest()
		{
			TeaTime.Range r = new Range(5, 10);
			r.Contains(3).Should().Be.False();
			r.Contains(5).Should().Be.True();
			r.Contains(7).Should().Be.True();

			r.Start.Should().Be(5);
			r.End.Should().Be(10);
			r.IsEmpty.Should().Be.False();

			var r2 = new Range(3, 7);
			r2.Overlaps(r);
			r.Contains(new Range(3, 7)).Should().Be.False();
			r.Contains(new Range(5, 8)).Should().Be.True();
			r.Contains(new Range(3, 70)).Should().Be.False();

            r.EnsureContained(6).Should().Be(6);
            r.EnsureContained(5).Should().Be(5);
            r.EnsureContained(4).Should().Be(5);
            r.EnsureContained(10).Should().Be(10);
            r.EnsureContained(11).Should().Be(10);

			(r == r2).Should().Be.False();
			(r == new Range(5, 10)).Should().Be.True();
			(r == new Range(5, 10)).Should().Be.True();
			(r == new Range(6, 10)).Should().Be.False();
			(r == new Range(5, 11)).Should().Be.False();
		}

		[TestMethod]
		public void RangeDTest()
		{
			var r = new RangeD(5, 10);
			r.Contains(3).Should().Be.False();
			r.Contains(5).Should().Be.True();
			r.Contains(7).Should().Be.True();

			r.Start.Should().Be(5);
			r.End.Should().Be(10);
			r.IsEmpty.Should().Be.False();

			var r2 = new RangeD(3, 7);
			r2.Overlaps(r);
			r.Contains(new RangeD(3, 7)).Should().Be.False();
			r.Contains(new RangeD(5, 8)).Should().Be.True();
			r.Contains(new RangeD(3, 70)).Should().Be.False();

            r.EnsureContained(6).Should().Be(6);
            r.EnsureContained(5).Should().Be(5);
            r.EnsureContained(4).Should().Be(5);
            r.EnsureContained(10).Should().Be(10);
            r.EnsureContained(11).Should().Be(10);

			(r == r2).Should().Be.False();
			(r == new RangeD(5, 10)).Should().Be.True();
			(r == new RangeD(5, 10)).Should().Be.True();
			(r == new RangeD(6, 10)).Should().Be.False();
			(r == new RangeD(5, 11)).Should().Be.False();
		}

		[TestMethod]
		public void RangeTTest()
		{
			var r = new RangeT(5.ToDate(), 10.ToDate());
			r.Contains(3.ToDate()).Should().Be.False();
			r.Contains(5.ToDate()).Should().Be.True();
			r.Contains(7.ToDate()).Should().Be.True();

			r.Start.Should().Be.EqualTo(5.ToDate());
			r.End.Should().Be.EqualTo(10.ToDate());
			r.IsEmpty.Should().Be.False();

			var r2 = new RangeT(3.ToDate(), 7.ToDate());
			r2.Overlaps(r);
			r.Contains(new RangeT(3.ToDate(), 7.ToDate())).Should().Be.False();
			r.Contains(new RangeT(5.ToDate(), 8.ToDate())).Should().Be.True();
			r.Contains(new RangeT(3.ToDate(), 17.ToDate())).Should().Be.False();

            r.EnsureContained(6.ToDate()).Should().Be(6.ToDate());
            r.EnsureContained(5.ToDate()).Should().Be(5.ToDate());
            r.EnsureContained(4.ToDate()).Should().Be(5.ToDate());
            r.EnsureContained(10.ToDate()).Should().Be(10.ToDate());
            r.EnsureContained(11.ToDate()).Should().Be(10.ToDate());

			(r == r2).Should().Be.False();
			(r == new RangeT(5.ToDate(), 10.ToDate())).Should().Be.True();
			(r == new RangeT(5.ToDate(), 10.ToDate())).Should().Be.True();
			(r == new RangeT(6.ToDate(), 10.ToDate())).Should().Be.False();
			(r == new RangeT(5.ToDate(), 11.ToDate())).Should().Be.False();
		}

		[TestMethod]
		public void RangeOuterSectionTest()
		{
			var r = new Range(5, 10);
			r.Outersection(new Range(7, 12)).Should().Be.EqualTo(new Range(5, 6));

			Range.Empty.Outersection(new Range(7, 12)).Should().Be.EqualTo(Range.Empty);
			r.Outersection(Range.Empty).Should().Be.EqualTo(r);
		}

		[TestMethod]
		public void RangeEqualsTest()
		{
			var r = new Range(5, 10);
			r.Equals(new Range(5, 10)).Should().Be.True();
			r.Equals(new Range(5, 11)).Should().Be.False();
			r.Equals(new Range(6, 10)).Should().Be.False();
		}

		[TestMethod]
		public void RangeLengthTest()
		{
			Range.Empty.Length.Should().Be(0);
			var r = new Range(5, 5);
			r.Length.Should().Be(1);
			r = new Range(5, 6);
			r.Length.Should().Be(2);
		}

		[TestMethod]
		public void RangeDEqualsTest()
		{
			var r = new RangeD(5, 10);
			r.Equals(new RangeD(5, 10)).Should().Be.True();
			r.Equals(new RangeD(5, 11)).Should().Be.False();
			r.Equals(new RangeD(6, 10)).Should().Be.False();
		}

		[TestMethod]
		public void RangeDLengthTest()
		{
			RangeD.Empty.Length.Should().Be(0);
			var r = new RangeD(5.5, 6.5);
			r.Length.Should().Be(1);
			r = new RangeD(5d, 5d);
			r.Length.Should().Be(0);
		}

		[TestMethod]
		public void RanegTEqualsTest()
		{
			var r = new RangeT(3.ToDate(), 4.ToDate());
			var r2 = new RangeT(3.ToDate(), 4.ToDate());
			var r3 = new RangeT(2.ToDate(), 4.ToDate());
			var r4 = new RangeT(3.ToDate(), 5.ToDate());
			r.Equals(r2).Should().Be.True();
			r.Equals(r3).Should().Be.False();
			r.Equals(r4).Should().Be.False();
			Range.Empty.Equals(Range.Empty).Should().Be.True();
		}
	}

	static class E
	{
		public static DateTime ToDate(this int value)
		{
			return new DateTime(2000, 1, value);
		}
	}
}
