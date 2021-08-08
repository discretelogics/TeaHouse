using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TeaTime.API;

namespace TeaTime.UI
{
	public partial class TimeBox : UserControl
	{
		#region state

		readonly DateTimeModel model;

		#endregion

		public TimeBox()
		{
			InitializeComponent();

			// model - dp connection
			this.model = new DateTimeModel();
			model.ValueChanged = t => this.DateTime = t;

			// up down handing
			SetupTextBox(tbYear, () => this.model.AddYears(1), () => this.model.AddYears(-1));
			SetupTextBox(tbMonth, () => this.model.AddMonths(1), () => this.model.AddMonths(-1));
			SetupTextBox(tbDay, () => this.model.Add(TimeSpan.FromDays(1)), () => this.model.Add(TimeSpan.FromDays(-1)));
			SetupTextBox(tbHour, () => this.model.Add(TimeSpan.FromHours(1)), () => this.model.Add(TimeSpan.FromHours(-1)));
			SetupTextBox(tbMinute, () => this.model.Add(TimeSpan.FromMinutes(1)), () => this.model.Add(TimeSpan.FromMinutes(-1)));
			SetupTextBox(tbSecond, () => this.model.Add(TimeSpan.FromSeconds(1)), () => this.model.Add(TimeSpan.FromSeconds(-1)));
		}

		void SetupTextBox(TextBox tb, Action upAction, Action downAction)
		{
			// we must set the model to each element, we cannot set the DataContext, because this is set by the user of this control !
			tb.DataContext = this.model;

			tb.PreviewKeyDown += (s, e) =>
			                     	{
			                     		if (e.Key == Key.Up) upAction();
			                     		else if (e.Key == Key.Down) downAction();
			                     	};

			// select all upon entry
            tb.GotFocus += (s, e) => tb.SelectAll();
            tb.PreviewMouseLeftButtonDown += (s, e) => // prevents immediate deselect on mouseclick
                                    {
                                        var textBox = (TextBox)s;
                                        if (!textBox.IsFocused)
                                        {
                                            e.Handled = true;
                                            textBox.Focus();
                                        }
                                    };

			// select all after up/down action
			tb.KeyUp += (s, e) => { if (e.Key == Key.Up || e.Key == Key.Down) tb.SelectAll(); };
		}

		#region dependency properties

		static TimeBox()
		{
			var md = new FrameworkPropertyMetadata();
			md.BindsTwoWayByDefault = true;
			md.PropertyChangedCallback += (dp, ea) => ((TimeBox) dp).model.Set((DateTime) ea.NewValue);
			DateTimeProperty = DependencyProperty.Register("DateTime", typeof (DateTime), typeof (TimeBox), md);
		}

		public static readonly DependencyProperty DateTimeProperty;
		
		public DateTime DateTime
		{
			get { return (DateTime) GetValue(DateTimeProperty); }
			set
			{
				SetValue(DateTimeProperty, value);

				// if the update fails in the data binding world, the DateTime value will remain unchanged.
				// putting it back to the model ensures that the model is in synch
				this.model.Set(this.DateTime);
			}
		}

		#endregion
	}

	public class DateTimeModel : NotifyPropertyChanged
	{
		#region state

		DateTime value;

		#endregion

		#region properties

		public int Year
		{
			get { return this.value.Year; }
			set
			{
				Set(value, this.value.Month, this.value.Day, this.value.Hour, this.value.Minute, this.value.Second);
				Changed("Year");
			}
		}

		public int Month
		{
			get { return this.value.Month; }
			set
			{
				Set(this.value.Year, value, this.value.Day, this.value.Hour, this.value.Minute, this.value.Second);
				Changed("Month");
			}
		}

		public int Day
		{
			get { return this.value.Day; }
			set
			{
				Set(this.value.Year, this.value.Month, value, this.value.Hour, this.value.Minute, this.value.Second);
				Changed("Day");
			}
		}

		public int Hour
		{
			get { return this.value.Hour; }
			set
			{
				Set(this.value.Year, this.value.Month, this.value.Day, value, this.value.Minute, this.value.Second);
				Changed("Hour");
			}
		}

		public int Minute
		{
			get { return this.value.Minute; }
			set
			{
				Set(this.value.Year, this.value.Month, this.value.Day, this.value.Hour, value, this.value.Second);
				Changed("Minute");
			}
		}

		public int Second
		{
			get { return this.value.Second; }
			set
			{
				Set(this.value.Year, this.value.Month, this.value.Day, this.value.Hour, this.value.Minute, value);
				Changed("Second");
			}
		}

		#endregion

		#region logical update

		internal Action<DateTime> ValueChanged;

		public void Set(DateTime t)
		{
			if (this.value == t) return;

			this.value = t;
			Changed("");
			ValueChanged(this.value);
		}

		void Set(int year, int month, int day, int hour, int minute, int second)
		{
			try
			{
				day = day.UpperBound(DateTime.DaysInMonth(value.Year, month));
				Set(new DateTime(year, month, day, hour, minute, second));
			}
			catch
			{
			}
		}

		#endregion

		#region operations up / down

		public void Add(TimeSpan ts)
		{
			this.Set(value.Add(ts));
		}

		public void AddMonths(int n)
		{
			var m = this.value.Month + n;
			if (m == 13) Set(value.Year + 1, 1, value.Day, value.Hour, value.Minute, value.Second);
			else if (m == 0) Set(value.Year - 1, 12, value.Day, value.Hour, value.Minute, value.Second);
			else Set(value.Year, m, value.Day, value.Hour, value.Minute, value.Second);
		}

		public void AddYears(int n)
		{
			Set(value.Year + n, value.Month, value.Day, value.Hour, value.Minute, value.Second);
		}

		#endregion
	}
}