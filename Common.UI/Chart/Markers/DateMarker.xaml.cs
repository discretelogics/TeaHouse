using System;
using System.Windows;
using System.Windows.Controls;
using TeaTime.Chart.Core;

namespace TeaTime.Chart.Markers
{
    // TODO: review again
    public partial class DateMarker : Canvas
    {
        #region properties
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty<DateMarker, bool>.Register("IsSelected", false, IsSelectedChanged);
        public static readonly DependencyProperty TimeModeProperty = DependencyProperty<DateMarker, DateTimeMode>.Register("TimeMode", DateTimeMode.TimeAndDate);


        public bool IsSelected
        {
            get
            {
                return (bool)GetValue(IsSelectedProperty);
            }
            set
            {
                if (value != IsSelected)
                {
                    SetValue(IsSelectedProperty, value);
                }
            }
        }
        internal DateTimeMode TimeMode
        {
            get
            {
                return (DateTimeMode)GetValue(TimeModeProperty);
            }
            set
            {
                SetValue(TimeModeProperty, value);
            }
        }
        #endregion

        #region public methods
        public void Update(double left, double right, DateTime content)
		{
			if (actualLeft != left)
			{
				actualLeft = left;
                Canvas.SetLeft(this, left);
			}

			double width = right - left;
			if (actualWidth != width)
			{
				actualWidth = width;
				this.Width = width;
			}

			if (actualContent != content)
			{
				actualContent = content;
                UpdateContent();
			}
		}

        public bool IsInRange(double position)
        {
            return ((position >= actualLeft) && (position < actualLeft + actualWidth));
        }
		#endregion

		#region ctor
		public DateMarker()
		{
			InitializeComponent();
            MeasureSize();

			actualLeft = double.MinValue;
			actualWidth = double.MinValue;
			actualContent = DateTime.MinValue;
		}
		#endregion

        #region private methods
        private void MeasureSize()
        {
            if (!sizeMeasured)
            {
                DateTime date = DateTime.Now;
                text.Text = date.ToShortDateString();
                fullDateDesiredWidth = text.Measure().Width;

                text.Text = date.ToMonthAndYearDisplayString();
                monthAndYearDesiredWidth = text.Measure().Width;

                text.Text = date.ToYearDisplayString();
                yearDesiredWidth = text.Measure().Width;

                sizeMeasured = true;
            }
        }
        private void UpdateContent()
        {
            string displayText = null;
            string toolTip;
            double desiredWidth;
            if (TimeMode == DateTimeMode.TimeAndDate)
            {
                toolTip = actualContent.ToShortDateString();
                desiredWidth = fullDateDesiredWidth;
            }
            else if (TimeMode == DateTimeMode.DayAndMonth)
            {
                toolTip = actualContent.ToMonthAndYearDisplayString();
                desiredWidth = monthAndYearDesiredWidth;
            }
            else
            {
                toolTip = actualContent.ToYearDisplayString();
                desiredWidth = yearDesiredWidth;
            }

            if ((actualWidth >= desiredWidth) || IsSelected)
            {
                displayText = toolTip;
            }

            this.text.Text = displayText;
            this.ToolTip = toolTip;

            if (actualWidth < desiredWidth)
            {
                desiredWidth += text.Padding.Left;  // ignore padding
            }
            Canvas.SetLeft(this.text, (actualWidth - desiredWidth) / 2);
        }
        #endregion

        #region eventhandler
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateContent();
        }

        private static void IsSelectedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            DateMarker dm = (DateMarker)sender;
            Canvas.SetZIndex(dm, (bool)e.NewValue ? 1 : 0);
            dm.UpdateContent();
        }
        #endregion

        #region fields
        private double actualLeft;
		private double actualWidth;
		private DateTime actualContent;
		#endregion

        #region static fields
        private static bool sizeMeasured;
        private static double fullDateDesiredWidth;
        private static double monthAndYearDesiredWidth;
        private static double yearDesiredWidth;
        #endregion
    }
}