using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace TestWPF
{
	public class TimeSliderModel : INotifyPropertyChanged
	{
		public TimeSliderModel()
		{
			this.selectedStart = this.start = new DateTime(2000, 1, 1);
			this.selectedEnd = this.end = new DateTime(2000, 3, 1);
			this.resolution = TimeSpan.FromDays(1);
			Compute();
			Audit();
		}

		#region core properties

		private DateTime start;
		private DateTime end;
		private DateTime selectedStart;
		private DateTime selectedEnd;
		private TimeSpan resolution;

		public DateTime Start
		{
			get { return start; }
			set { start = value.UpperBound(end); }
		}

		public DateTime End
		{
			get { return end; }
			set { end = value.LowerBound(start); }
		}

		public DateTime SelectedStart
		{
			get { return selectedStart; }
			set
			{
				var newValue = value.UpperBound(selectedEnd);
				if (newValue == selectedStart) return;
				selectedStart = newValue;
				Changed("SelectedStart", "SelectedStartRelative");
			}
		}

		public DateTime SelectedEnd
		{
			get { return selectedEnd; }
			set
			{
				var newValue = value.LowerBound(selectedStart);
				if (newValue == selectedEnd) return;
				selectedEnd = newValue;
				Changed("SelectedEnd", "SelectedEndRelative");
			}
		}

		public TimeSpan Resolution
		{
			get { return resolution;}
			set
			{
				if (this.resolution == value) return;
				this.resolution = value;
				Compute();
			}
		}

		#endregion

		#region derived properties

		private DateTime displayStart;
		private DateTime displayEnd;
		private long steps;

		public DateTime DisplayStart
		{
			get { return displayStart; }
		}

		public DateTime DisplayEnd
		{
			get { return displayEnd; }
		}

		public long Steps
		{
			get
			{
				return steps;
			}
		}

		public TimeSpan Range
		{
			get
			{
				return this.end - this.start;
			}
		}

		public TimeSpan DisplayRange
		{
			get
			{
				return this.displayEnd - this.displayStart;
			}
		}

		public double SelectedStartRelative 
		{
			get
			{
				return GetTimeLinePosition(this.selectedStart);
			}
		}

		public double SelectedEndRelative
		{
			get
			{
				return GetTimeLinePosition(this.selectedEnd);
			}
		}

		#endregion

		#region operations

		private void Compute()
		{
			var rangeTicks = Range.Ticks;
			long borderTicks = (long)(rangeTicks / 8.0); // the core range holds 80% of the timeline, the display range adds 10% on the left and on the right, making it 100%
			this.displayStart = Rounded(new DateTime(this.start.Ticks - borderTicks));
			this.displayEnd = Rounded(new DateTime(this.end.Ticks + borderTicks));
			this.steps = rangeTicks / this.resolution.Ticks;
		}

		DateTime Rounded(DateTime t)
		{
			return t.Rounded(this.resolution);
		}

		#endregion

		#region start and end

		public double SetSelectedStart(double timeLineLocation)
		{
			this.selectedStart = GetDatetimeFromLocation(timeLineLocation);
			if(this.selectedStart > this.selectedEnd)
			{
				this.selectedStart = this.selectedEnd;
				timeLineLocation = GetTimeLinePosition(this.selectedStart);
			}
			Changed("SelectedStart");
			return timeLineLocation;
		}

		public double SetSelectedEnd(double timeLineLocation)
		{
			this.selectedEnd = GetDatetimeFromLocation(timeLineLocation);
			if (this.selectedEnd < this.selectedStart)
			{
				this.selectedEnd = this.selectedStart;
				timeLineLocation = GetTimeLinePosition(this.selectedEnd);
			}
			Changed("SelectedEnd");
			return timeLineLocation;
		}

		public void ShiftSelectedStart(long numberOfSteps)
		{
			var t = this.selectedStart;
			t += TimeSpan.FromTicks((long)(numberOfSteps * this.resolution.Ticks));
			this.SelectedStart = t.LowerBound(this.displayStart);
			Audit();
		}

		public void ShiftSelectedEnd(long numberOfSteps)
		{
			var t = this.selectedEnd;
			t += TimeSpan.FromTicks((long)(numberOfSteps * this.resolution.Ticks));
			this.SelectedEnd = t.UpperBound(this.displayEnd);
			Audit();
		}

		public double GetSelectedStartRelative()
		{
			return this.GetTimeLinePosition(this.selectedStart);
		}

		public double GetSelectedEndRelative()
		{
			return this.GetTimeLinePosition(this.selectedEnd);
		}

		#endregion

		#region utils

		private DateTime GetDatetimeFromLocation(double timeLineLocation)
		{
			var t = this.displayStart + new TimeSpan((long)(timeLineLocation * DisplayRange.Ticks));
			return Rounded(t);
		}

		public void Changed(params string[] names)
		{
			Array.ForEach(names, Changed);
		}

		public void Changed(string name)
		{
			if (PropertyChanged == null) return;
			PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		double GetTimeLinePosition(DateTime t)
		{
			var ticksFromDisplayStart = t.Ticks - this.DisplayStart.Ticks;
			var position = (double)ticksFromDisplayStart / (double)this.DisplayRange.Ticks;
			Debug.Assert(position >= 0);
			Debug.Assert(position <= 1);
			return position;
		}

		private void Audit()
		{
			Debug.Assert(this.selectedStart >= this.displayStart);
			Debug.Assert(this.selectedEnd <= this.displayEnd);
		}

		#endregion

		public event PropertyChangedEventHandler PropertyChanged;
	}

	internal static class Extensions
	{
		public static DateTime UpperBound(this DateTime value, DateTime Maximum)
		{
			return value > Maximum ? Maximum : value;
		}

		public static DateTime LowerBound(this DateTime value, DateTime Minimum)
		{
			return value < Minimum ? Minimum : value;
		}

		public static double UpperBound(this double value, double Maximum)
		{
			return value > Maximum ? Maximum : value;
		}

		public static double LowerBound(this double value, double Minimum)
		{
			return value < Minimum ? Minimum : value;
		}

		public static DateTime Rounded(this DateTime t, TimeSpan timeSpan)
		{
			double units = (double)t.Ticks / (double)timeSpan.Ticks;
			long runits = (long) Math.Round(units);
			return new DateTime(runits * timeSpan.Ticks);
		}
	}
}