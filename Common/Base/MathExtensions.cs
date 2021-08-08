using System;

namespace TeaTime
{
	public static class MathExtensions
	{
		#region int

		public static int LowerBound(this int value, int lowerBound)
		{
			if (value < lowerBound) return lowerBound;
			return value;
		}

		public static int UpperBound(this int value, int upperBound)
		{
			if (value > upperBound) return upperBound;
			return value;
		}

		public static bool IsWithin(this int value, int lowerBound, int upperBound)
		{
			return value >= lowerBound && value <= upperBound;
		}

		#endregion

		#region double

		public static double LowerBound(this double value, double lowerBound)
		{
			if (value < lowerBound) return lowerBound;
			return value;
		}

		public static double UpperBound(this double value, double upperBound)
		{
			if (value > upperBound) return upperBound;
			return value;
		}

		public static bool IsWithin(this double value, double lowerBound, double upperBound)
		{
			return value >= lowerBound && value <= upperBound;
		}

		#endregion

		#region float

		public static float LowerBound(this float value, float lowerBound)
		{
			if (value < lowerBound) return lowerBound;
			return value;
		}

		public static float UpperBound(this float value, float upperBound)
		{
			if (value > upperBound) return upperBound;
			return value;
		}

		public static bool IsWithin(this float value, float lowerBound, float upperBound)
		{
			return value >= lowerBound && value <= upperBound;
		}

		#endregion

		#region long

		public static long LowerBound(this long value, long lowerBound)
		{
			if (value < lowerBound) return lowerBound;
			return value;
		}

		public static long UpperBound(this long value, long upperBound)
		{
			if (value > upperBound) return upperBound;
			return value;
		}

		public static bool IsWithin(this long value, long lowerBound, long upperBound)
		{
			return value >= lowerBound && value <= upperBound;
		}

		#endregion

		#region DateTime

		public static DateTime UpperBound(this DateTime value, DateTime Maximum)
		{
			return value > Maximum ? Maximum : value;
		}

		public static DateTime LowerBound(this DateTime value, DateTime Minimum)
		{
			return value < Minimum ? Minimum : value;
		}

		public static DateTime Rounded(this DateTime t, TimeSpan timeSpan)
		{
			double units = (double) t.Ticks / (double) timeSpan.Ticks;
			long runits = (long) Math.Round(units);
			return new DateTime(runits * timeSpan.Ticks);
		}

		#endregion
	}
}