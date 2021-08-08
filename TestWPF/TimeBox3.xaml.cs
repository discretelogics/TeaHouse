using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DiscreteLogics.Common;
using DiscreteLogics.Common.GUI;

namespace TestWPF
{
	/// <summary>
	///     Interaction logic for TimeBox3.xaml
	/// </summary>
	public partial class TimeBox3 : UserControl
	{
		private DateTimeModel model;

		public TimeBox3()
		{
			InitializeComponent();

			this.GetLogicalChildren<TextBox>(true)
				.ForEach(tb => { tb.GotFocus += SelectAll; });

			this.tbDay.PreviewKeyDown += (s, e) =>
			                             	{
			                             		if (e.Key == Key.Up) this.model.Add(TimeSpan.FromDays(1));
			                             		else if (e.Key == Key.Down) this.model.Add(TimeSpan.FromDays(-1));
			                             		SelectAllUponUpDown(s, e.Key);
			                             	};
			this.tbMonth.PreviewKeyDown += (s, e) =>
			                               	{
			                               		if (e.Key == Key.Up) this.model.AddMonths(1);
			                               		else if (e.Key == Key.Down) this.model.AddMonths(-1);
			                               		SelectAllUponUpDown(s, e.Key);
			                               	};
			this.tbYear.PreviewKeyDown += (s, e) =>
			                              	{
			                              		if (e.Key == Key.Up) this.model.AddYears(1);
			                              		else if (e.Key == Key.Down) this.model.AddYears(-1);
			                              		SelectAllUponUpDown(s, e.Key);
			                              	};
			this.tbHour.PreviewKeyDown += (s, e) =>
			                              	{
			                              		if (e.Key == Key.Up) this.model.Add(TimeSpan.FromHours(1));
			                              		else if (e.Key == Key.Down) this.model.Add(TimeSpan.FromHours(-1));
			                              		SelectAllUponUpDown(s, e.Key);
			                              	};
			this.tbMinute.PreviewKeyDown += (s, e) =>
			                                	{
			                                		if (e.Key == Key.Up) this.model.Add(TimeSpan.FromMinutes(1));
			                                		else if (e.Key == Key.Down) this.model.Add(TimeSpan.FromMinutes(-1));
			                                		SelectAllUponUpDown(s, e.Key);
			                                	};
			this.tbSecond.PreviewKeyDown += (s, e) =>
			                                	{
			                                		if (e.Key == Key.Up) this.model.Add(TimeSpan.FromSeconds(1));
			                                		else if (e.Key == Key.Down) this.model.Add(TimeSpan.FromSeconds(-1));
			                                		SelectAllUponUpDown(s, e.Key);
			                                	};

			// model - dp connection
			DataContext = model = new DateTimeModel();
			model.PropertyChanged += ModelChanged;
		}

		private void SelectAllUponUpDown(object sender, Key key)
		{
			if (key == Key.Down || key == Key.Up)
			{
				TextBox tb = (TextBox) sender;
				tb.SelectAll();
			}
		}

		private void ModelChanged(object sender, PropertyChangedEventArgs e)
		{
			this.DateTime = model.Value;
		}

		public void SelectAll(object o, RoutedEventArgs e)
		{
			var tb = (TextBox) o;
			tb.SelectAll();
		}

		private void ControlLoaded(object sender, RoutedEventArgs e)
		{
		}

		#region dependency properties

		public static readonly DependencyProperty DateTimeProperty;

		static TimeBox3()
		{
			var md = new FrameworkPropertyMetadata();
			md.PropertyChangedCallback += DateTimePropertyChanged;
			DateTimeProperty = DependencyProperty.Register("DateTime", typeof (DateTime), typeof (TimeBox3), md);
		}

		public DateTime DateTime
		{
			get { return (DateTime) GetValue(DateTimeProperty); }
			set { SetValue(DateTimeProperty, value); }
		}

		private static void DateTimePropertyChanged(DependencyObject dp, DependencyPropertyChangedEventArgs ea)
		{
			TimeBox3 tb = (TimeBox3) dp;
			tb.model.Set((DateTime) ea.NewValue);
		}

		#endregion
	}

	public class DateTimeModel : INotifyPropertyChanged
	{
		private DateTime t;

		public int Year
		{
			get { return this.t.Year; }
			set
			{
				SafeUpdate(value, t.Month, t.Day, t.Hour, t.Minute, t.Second);
				Changed("Year");
			}
		}

		public int Month
		{
			get { return this.t.Month; }
			set
			{
				SafeUpdate(t.Year, value, t.Day, t.Hour, t.Minute, t.Second);
				Changed("Month");
			}
		}

		public int Day
		{
			get { return this.t.Day; }
			set
			{
				SafeUpdate(t.Year, t.Month, value, t.Hour, t.Minute, t.Second);
				Changed("Day");
			}
		}

		public int Hour
		{
			get { return this.t.Hour; }
			set
			{
				SafeUpdate(t.Year, t.Month, t.Day, value, t.Minute, t.Second);
				Changed("Hour");
			}
		}

		public int Minute
		{
			get { return this.t.Minute; }
			set
			{
				SafeUpdate(t.Year, t.Month, t.Day, t.Hour, value, t.Second);
				Changed("Minute");
			}
		}

		public int Second
		{
			get { return this.t.Second; }
			set
			{
				SafeUpdate(t.Year, t.Month, t.Day, t.Hour, t.Minute, value);
				Changed("Second");
			}
		}

		public DateTime Value
		{
			get { return this.t; }
		}

		#region INotifyPropertyChanged Members

		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		private void SafeUpdate(int year, int month, int day, int hour, int minutes, int seconds)
		{
			try
			{
				this.t = new DateTime(year, month, day, hour, minutes, seconds);
			}
			catch
			{
			}
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

		public void Set(DateTime t)
		{
			this.t = t;
			Changed("Year", "Month", "Day", "Hour", "Minute", "Second");
		}

		private void Set(int year, int month, int day, int hour, int minutes, int seconds)
		{
			Set(new DateTime(year, month, day, hour, minutes, seconds));
		}

		public void Add(TimeSpan ts)
		{
			this.Set(t.Add(ts));
		}

		public void AddMonths(int n)
		{
			var m = this.t.Month + n;
			if (m == 13) Set(t.Year + 1, 1, t.Day, t.Hour, t.Minute, t.Second);
			else if (m == 0) Set(t.Year - 1, 12, t.Day, t.Hour, t.Minute, t.Second);
			else Set(t.Year, m, t.Day, t.Hour, t.Minute, t.Second);
		}

		public void AddYears(int n)
		{
			Set(t.Year + n, t.Month, t.Day, t.Hour, t.Minute, t.Second);
		}
	}
}