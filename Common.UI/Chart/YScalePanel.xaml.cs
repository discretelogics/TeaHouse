using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Ink;
using TeaTime.Chart.Core;
using TeaTime.Chart.Markers;

namespace TeaTime.Chart
{
    public partial class YScalePanel : UserControl, IDisposable, ISelectable
    {
        #region properties
        public static readonly DependencyProperty FormatStringProperty = DependencyProperty<YScalePanel, string>.Register("FormatString", "0.00");
        public static readonly DependencyProperty MarkerAlignmentProperty = DependencyProperty<YScalePanel, MarkerAlignment>.Register("MarkerAlignment", MarkerAlignment.Left);

        public string FormatString
        {
            get
            {
                return (string)GetValue(FormatStringProperty);
            }
            set
            {
                SetValue(FormatStringProperty, value);
            }
        }
        internal MarkerAlignment MarkerAlignment
        {
            get
            {
                return (MarkerAlignment)GetValue(MarkerAlignmentProperty);
            }
            set
            {
                SetValue(MarkerAlignmentProperty, value);
            }
        }

        private bool IsSingleTs
        {
            get { return chartPanel.IsSingleTs; }
        }
        private bool IsFixedLayout
        {
            get { return chartPanel.IsFixedLayout; }
        }
        #endregion

        #region ctor, Initialize, Dispose
        public YScalePanel()
        {
            InitializeComponent();

            valueMarkers = new List<Marker>();
            cursorMarkers = new Dictionary<ICursor, Marker>();
            closeMarkers = new Dictionary<TsVisualization, Marker>();
            timeSeriesMarkers = new Dictionary<TsVisualization, TimeSeriesMarker>();
            timeSeriesMarkerWidths = new Dictionary<TsVisualization, double>();

            scaleMode = ScaleMode.None;
            scaleOrigin = double.MinValue;
        }

        internal void Initialize(ChartPanel chartPanel, YScaler yscaler, HorizontalAlignment containerAlignment)
        {
            if (!isInitialized)
            {
                Guard.ArgumentNotNull(chartPanel, "chartPanel");
                Guard.ArgumentNotNull(yscaler, "yscaler");

                this.chartPanel = chartPanel;
                this.yScaler = yscaler;

                container.HorizontalAlignment = containerAlignment;
                if (containerAlignment == HorizontalAlignment.Left)
                {
                    this.MarkerAlignment = MarkerAlignment.Right;
                }
                else
                {
                    this.MarkerAlignment = MarkerAlignment.Left;
                }

                this.Width = container.Width = IsSingleTs ? 70 : 80;

                isInitialized = true;
            }
            else
            {
                throw new InvalidOperationException("This YScalePanel was already initialized.");
            }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                valueMarkers.ForEach(ym => ym.Dispose());
                valueMarkers.Clear();

                cursorMarkers.Values.ForEach(cm => cm.Dispose());
                cursorMarkers.Clear();

                closeMarkers.Values.ForEach(cm => cm.Dispose());
                closeMarkers.Clear();

                timeSeriesMarkers.Clear();

                isDisposed = true;
            }
            else
            {
                throw new InvalidOperationException("This YScalePanel was already disposed.");
            }
        }
        #endregion

        #region ISelectable members
        public void Select(object sender)
        {
            Marker marker = (Marker)sender;
            chartPanel.SelectValueMarker(marker.ActualPosition, marker);
        }
        public void Deselect(object sender)
        {
            Marker marker = (Marker)sender;
            chartPanel.DeselectValueMarker(marker.ActualPosition, marker);
        }
        #endregion

        #region internal methods
        internal void UpdateCloseMarker(TsVisualization tv, double value)
        {
            closeMarkers[tv].Update(yScaler.ValueToPosition(value), value.ToString(FormatString));
        }

        #region yScaler
        internal void UpdateValueMarkers(RangeD valueRange)
        {
            UpdateLayout();

            List<double> values = GetMarkerValues(valueRange);

            #region Remove unused markers
            if (valueMarkers.Count > values.Count)
            {
                List<Marker> unused = valueMarkers.GetRange(values.Count, valueMarkers.Count - values.Count);
                foreach (Marker marker in unused)
                {
                    marker.Dispose();
                    valueMarkers.Remove(marker);
                }
            }
            #endregion

            var positions = new List<double>();

            #region Create and Update ValueMarkers
            string formatString = FormatString;
            for (int i = 0; i < values.Count; i++)
            {
                if (i >= valueMarkers.Count)
                {
                    valueMarkers.Add(new Marker(container, this, MarkerAlignment, true, false));
                }

                double value = values[i];
                double position = yScaler.ValueToPosition(value);
                positions.Add(position);

                valueMarkers[i].Update(position, value.ToString(formatString));
            }
            #endregion

            chartPanel.ValuePositionsUpdated(this, positions);
        }

        internal void Add(TsVisualization tv)
        {
            UpdateLayout();

            #region TimeSeriesMarker
            if (!IsSingleTs)
            {
                var timeSeriesMarker = new TimeSeriesMarker();
                timeSeriesMarker.TimeSeriesName = tv.TimeSeries.Name;
                timeSeriesMarker.ApplyDrawingAttributes(tv.DrawingAttributes);
                timeSeriesMarker.Measure();
                double width = timeSeriesMarker.DesiredSize.Width;

                double angle = (MarkerAlignment == MarkerAlignment.Left) ? 90 : -90;
                timeSeriesMarker.RenderTransform = new RotateTransform(angle, 0, 0);
                timeSeriesMarker.Margin = (MarkerAlignment == MarkerAlignment.Left) ? new Thickness(container.Width, 0, 0, 0) : new Thickness(0, 0, container.Width, -timeSeriesMarker.Height);

                container.Children.Insert(0, timeSeriesMarker);

                double offset = timeSeriesMarkerWidths.Values.Sum();
                if (MarkerAlignment == MarkerAlignment.Left)
                {
                    Canvas.SetTop(timeSeriesMarker, offset);
                }
                else
                {
                    Canvas.SetBottom(timeSeriesMarker, offset);
                }
                timeSeriesMarkers.Add(tv, timeSeriesMarker);
                timeSeriesMarkerWidths.Add(tv, width);
            }
            #endregion

            #region CloseMarker
            var closeMarker = new Marker(container, null, MarkerAlignment, false, false);
            ApplyDrawingAttributesToCloseMarker(closeMarker, tv.DrawingAttributes);
            closeMarkers.Add(tv, closeMarker);
            UpdateCloseMarker(tv, 0);
            #endregion

            tv.DrawingAttributesChanged += tv_DrawingAttributesChanged;
        }
        internal void Remove(TsVisualization tv)
        {
            UpdateLayout();

            tv.DrawingAttributesChanged -= tv_DrawingAttributesChanged;

            #region Ts-Marker
            if (timeSeriesMarkers.ContainsKey(tv))
            {
                container.Children.Remove(timeSeriesMarkers[tv]);

                timeSeriesMarkers.Remove(tv);
                timeSeriesMarkerWidths.Remove(tv);

                double offset = 0;
                foreach (var tvtsm in timeSeriesMarkers)
                {
                    if (MarkerAlignment == MarkerAlignment.Left)
                    {
                        Canvas.SetTop(tvtsm.Value, offset);
                    }
                    else
                    {
                        Canvas.SetBottom(tvtsm.Value, offset);
                    }

                    offset += timeSeriesMarkerWidths[tvtsm.Key];
                }
            }
            #endregion

            #region Closing-Marker
            closeMarkers[tv].Dispose();
            closeMarkers.Remove(tv);
            #endregion
        }
        #endregion

        #region Cursor
        internal void UpdateCursorMarker(ICursor cursor, double position)
        {
            string value = yScaler.ValueRange.IsEmpty ? null : yScaler.PositionToValue(position).ToString(FormatString);
            cursorMarkers[cursor].Update(position, value);
        }
        internal void AddCursorMarker(ICursor cursor)
        {
            var marker = new Marker(container, null, MarkerAlignment, false, false);
            marker.Background = container.Background;
            cursorMarkers.Add(cursor, marker);
        }
        internal void RemoveCursorMarker(ICursor cursor)
        {
            cursorMarkers[cursor].Dispose();
            cursorMarkers.Remove(cursor);
        }
        #endregion
        #endregion

        #region private methods
        private void ApplyDrawingAttributesToCloseMarker(Marker marker, DrawingAttributes da)
        {
            Color color = (da == null) ? Colors.Black : da.Color;
            color.A = 100;
            marker.Background = new SolidColorBrush(color);
        }

        private void SetScaleMode(ScaleMode scaleMode)
        {
            if (scaleMode != this.scaleMode &&
                !IsFixedLayout && 
                !IsSingleTs)
            {
                this.scaleMode = scaleMode;
                switch (scaleMode)
                {
                    case ScaleMode.Move:
                        container.Cursor = Cursors.ScrollNS;
                        break;
                    case ScaleMode.Zoom:
                        container.Cursor = Cursors.SizeNS;
                        break;
                    default:
                        container.Cursor = Cursors.Arrow;
                        break;
                }
            }
        }
        
        private List<double> GetMarkerValues(RangeD valueRange)
        {
            var values = new List<double>();

            if ((container.ActualHeight > 0.0) && !valueRange.IsEmpty)
            {
                double start = valueRange.Start;
                double end = valueRange.End;

                if (yScaler.IsLogarithmic)
                {
                    double logStart = Math.Log10(start);
                    int count = Convert.ToInt32(Math.Log10(end) - logStart) + 1;
                    for (int i = 0; i <= count; ++i)
                    {
                        double value = Math.Pow(10, Math.Floor(logStart + i));
                        if (valueRange.Contains(value))
                        {
                            values.Add(value);
                        }
                    }
                }
                else
                {
                    double length = valueRange.Length;
                    double roundLength = length / 10.0;

                    double multiplier = 0.0000001;
                    while (roundLength > multiplier)
                    {
                        multiplier *= 10.0;
                    }

                    double spaceRegular = container.ActualHeight - valueMarkerDistance;
                    int countRegular = Math.Min(5, (int)(spaceRegular / valueMarkerDistance));

                    double delta = 1.0;
                    int difference = int.MaxValue;
                    foreach (double d in preferredValues)
                    {
                        double dm = d * multiplier;
                        int c = (int)(length / dm);
                        int diff = countRegular - c;
                        if ((c <= countRegular) && (diff < difference))
                        {
                            difference = diff;
                            delta = dm;
                        }
                    }

                    if (difference <= countRegular)
                    {
                        double value = 0.0;
                        if (valueRange.Contains(0.0))
                        {
                            while (value >= start)
                            {
                                values.Add(value);
                                value -= delta;
                            }

                            value = delta;
                        }
                        else
                        {
                            double modulo = start % delta;
                            value = start - modulo;
                            if (modulo > 0.0)
                            {
                                value += delta;
                            }
                        }

                        while (value <= end)
                        {
                            values.Add(value);
                            value += delta;
                        }
                    }
                }
            }
            return values;
        }
        #endregion

        #region eventhandler
        #region tv
        private void tv_DrawingAttributesChanged(object sender, EventArgs<DrawingAttributes> e)
        {
            var tv = (TsVisualization)sender;
            if (timeSeriesMarkers.ContainsKey(tv))
            {
                timeSeriesMarkers[tv].ApplyDrawingAttributes(e.Value);
            }
            ApplyDrawingAttributesToCloseMarker(closeMarkers[tv], e.Value);
        }
        #endregion

        private void container_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (isInitialized)
            {
                if (scaleMode == ScaleMode.Move)
                {
                    if (e.Key.IsCtrl())
                    {
                        SetScaleMode(ScaleMode.Zoom);
                    }
                }
            }
        }
        private void container_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (isInitialized)
            {
                if (scaleMode == ScaleMode.Zoom)
                {
                    if (e.Key.IsCtrl())
                    {
                        SetScaleMode(ScaleMode.Move);
                    }
                }
            }
        }
        private void container_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (isInitialized)
            {
                container.Focus();

                scaleOrigin = e.GetPosition(container).Y;
                if (KeyboardUtility.IsCtrlPressed)
                {
                    SetScaleMode(ScaleMode.Zoom);
                }
                else
                {
                    SetScaleMode(ScaleMode.Move);
                }
            }
        }
        private void container_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isInitialized)
            {
                SetScaleMode(ScaleMode.None);
            }
        }
        private void container_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isInitialized)
            {
                SetScaleMode(ScaleMode.None);
            }
        }
        private void container_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isInitialized)
            {
                if (scaleMode != ScaleMode.None)
                {
                    double y = e.GetPosition(container).Y;
                    double delta = y - scaleOrigin;
                    if (Math.Abs(delta) >= SystemParameters.MinimumVerticalDragDistance)
                    {
                        if (scaleMode == ScaleMode.Move)
                        {
                            yScaler.MoveRange(delta * MoveMultiplier);
                        }
                        else if (scaleMode == ScaleMode.Zoom)
                        {
                            yScaler.ZoomRangeRelative(delta * ZoomMultiplier);
                        }
                        scaleOrigin = y;
                    }
                }

                e.Handled = true;
            }
        }
        #endregion

        #region fields
        private bool isInitialized;
        private bool isDisposed;

        private ChartPanel chartPanel;
        private YScaler yScaler;

        private readonly List<Marker> valueMarkers;
        private readonly Dictionary<ICursor, Marker> cursorMarkers;
        private readonly Dictionary<TsVisualization, Marker> closeMarkers;
        private readonly Dictionary<TsVisualization, TimeSeriesMarker> timeSeriesMarkers;
        private readonly Dictionary<TsVisualization, double> timeSeriesMarkerWidths;

        private ScaleMode scaleMode;
        private double scaleOrigin;

        private const double valueMarkerDistance = 50.0;
        private static readonly double[] preferredValues = new double[] { 2.0, 1.0, 0.5, 0.25, 0.2 };

        public const double MoveMultiplier = 2.0;
        public const double ZoomMultiplier = 1.5;
        #endregion

        #region embedded types
        private enum ScaleMode
        {
            None,
            Move,
            Zoom
        }
        #endregion
    }
}
