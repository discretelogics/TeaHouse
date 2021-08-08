using SharpTestsEx;
using TeaTime;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TeaTime
{
    
    
    /// <summary>
    ///This is a test class for MathExtensionsTest and is intended
    ///to contain all MathExtensionsTest Unit Tests
    ///</summary>
	[TestClass()]
	public class MathExtensionsTest
	{
		public TestContext TestContext { get; set;}

    	[TestMethod]
    	public void IntTest()
    	{
			5.LowerBound(3).Should().Be(5);
			5.LowerBound(5).Should().Be(5);
			5.LowerBound(7).Should().Be(7);

			5.UpperBound(3).Should().Be(3);
			5.UpperBound(5).Should().Be(5);
			5.UpperBound(7).Should().Be(5);

			5.IsWithin(3, 7).Should().Be.True();
			5.IsWithin(5, 7).Should().Be.True();
			5.IsWithin(3, 5).Should().Be.True();
			5.IsWithin(100, 200).Should().Be.False();
    	}

		[TestMethod]
		public void DoubleTest()
		{
			5d.LowerBound(3).Should().Be(5);
			5d.LowerBound(5).Should().Be(5);
			5d.LowerBound(7).Should().Be(7);

			5d.UpperBound(3).Should().Be(3);
			5d.UpperBound(5).Should().Be(5);
			5d.UpperBound(7).Should().Be(5);

			5d.IsWithin(3, 7).Should().Be.True();
			5d.IsWithin(5, 7).Should().Be.True();
			5d.IsWithin(3, 5).Should().Be.True();
			5d.IsWithin(100, 200).Should().Be.False();
		}

		[TestMethod]
		public void FloatTest()
		{
			5f.LowerBound(3).Should().Be(5);
			5f.LowerBound(5).Should().Be(5);
			5f.LowerBound(7).Should().Be(7);
			 
			5f.UpperBound(3).Should().Be(3);
			5f.UpperBound(5).Should().Be(5);
			5f.UpperBound(7).Should().Be(5);
			 
			5f.IsWithin(3, 7).Should().Be.True();
			5f.IsWithin(5, 7).Should().Be.True();
			5f.IsWithin(3, 5).Should().Be.True();
			5f.IsWithin(100, 200).Should().Be.False();
		}


		[TestMethod]
		public void LongTest()
		{
			5L.LowerBound(3).Should().Be(5);
			5L.LowerBound(5).Should().Be(5);
			5L.LowerBound(7).Should().Be(7);
			 
			5L.UpperBound(3).Should().Be(3);
			5L.UpperBound(5).Should().Be(5);
			5L.UpperBound(7).Should().Be(5);
			 
			5L.IsWithin(3, 7).Should().Be.True();
			5L.IsWithin(5, 7).Should().Be.True();
			5L.IsWithin(3, 5).Should().Be.True();
			5L.IsWithin(100, 200).Should().Be.False();
		}

		[TestMethod]
		public void DateTimeTest()
		{
			var t = GetDate(5);
			t.LowerBound(GetDate(3)).Day.Should().Be(5);
			t.LowerBound(GetDate(5)).Day.Should().Be(5);
			t.LowerBound(GetDate(7)).Day.Should().Be(7);

			t.UpperBound(GetDate(3)).Day.Should().Be(3);
			t.UpperBound(GetDate(5)).Day.Should().Be(5);
			t.UpperBound(GetDate(7)).Day.Should().Be(5);
		}

    	[TestMethod]
    	public void DateTimeRouningTest()
    	{
    		var t = new DateTime(2007, 3, 2, 21, 59, 33);
			t.Rounded(TimeSpan.FromHours(1)).Should().Be.EqualTo(new DateTime(2007, 3, 2, 22, 00, 00));
			t.Rounded(TimeSpan.FromDays(1)).Should().Be.EqualTo(new DateTime(2007, 3, 3, 00, 00, 00));
			t.Rounded(TimeSpan.FromMinutes(1)).Should().Be.EqualTo(new DateTime(2007, 3, 2, 22, 00, 00));
    	}

		DateTime GetDate(int day)
		{
			return new DateTime(2000, 1, day);
		}
	}
}
