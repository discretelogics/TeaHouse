using System;
using System.Collections.Generic;
using TeaTime.API;

namespace TeaTime.Chart.Core
{
    /// <summary>
    /// maps an absolute position to a value with a specified range
    /// and vice versa
    /// </summary>
    [Serializable]
    internal class YScaler : IYScaler
    {
        #region properties
        public RangeD ValueRange
        {
            get
            {
                return new RangeD(min.Value, max.Value);
            }
        }

        string name;
        public string Name { get { return this.name; }  }

        public double ActualHeight
        {
            get
            {
                return actualHeight;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="YScaleMode"/> of this YScaler
        /// </summary>
        internal YScaleMode ScaleMode
        {
            get
            {
                return scaleMode;
            }
            set
            {
                if (scaleMode != value)
                {
                    scaleMode = value;
                    Invalidate();

                    this.chartControl.YScaleModeChanged(tvs);
                    this.chartControl.UpdateFrame();
                }
            }
        }
        internal bool IsLogarithmic
        {
            get
            {
                return (scaleMode == YScaleMode.Logarithmic);
            }
        }

        internal IEnumerable<TsVisualization> Tvs
        {
            get
            {
                return tvs;
            }
        }
        #endregion

        #region ctor
        public YScaler(ChartControl chartControl, YScalePanel yScalePanel, string name, YScaleMode yScaleMode)
        {
            Guard.ArgumentNotNull(chartControl, "chartControl");
            Guard.ArgumentNotNull(yScalePanel, "yScalePanel");

            this.name = name;
            this.tvs = new List<TsVisualization>();
            this.chartControl = chartControl;
            this.tapeView = chartControl.TapeView;
            this.yScalePanel = yScalePanel;
            this.scaleMode = yScaleMode;
            this.actualHeight = 0.0;
            this.itemsHeight = 0.0;
            this.tapeRange = RangeL.Empty;
            this.min = new ExtremeValue { TapeIndex = int.MinValue, Value = double.MaxValue };
            this.max = new ExtremeValue { TapeIndex = int.MinValue, Value = double.MinValue };            
        }
        #endregion

        #region public methods
        #region value to position
        public double ValueToPosition(double y)
        {
            if (max.Value <= min.Value)
            {
                return 0.0;
            }

            if (itemsHeight == 0.0)
            {
                return itemsPadding;
            }

            double relation;
            if (IsLogarithmic)
            {
                relation = (Math.Log10(y) - Math.Log10(min.Value)) / (Math.Log10(max.Value) - Math.Log10(min.Value));
            }
            else
            {
                relation = (y - min.Value) / (max.Value - min.Value);
            }
            return actualHeight - itemsPadding - (relation * itemsHeight);
        }
        public double ValueHeightToPositionHeight(double height)
        {
            if (IsLogarithmic)
            {
                throw new InvalidOperationException("Operation isn't valid for logarithmic yScale-modes.");
            }

            if (max.Value <= min.Value)
            {
                return 0.0;
            }

            double relation = height / (max.Value - min.Value);
            return relation * itemsHeight;
        }
        #endregion

        #region position to value
        public double PositionToValue(double y)
        {
            if (itemsHeight <= 0.0)
            {
                return (y <= itemsPadding) ? max.Value : min.Value;
            }

            double relation = (actualHeight - itemsPadding - y) / itemsHeight;

            if (relation <= 0.0)
            {
                return min.Value;
            }

            if (IsLogarithmic)
            {
                return Math.Pow(10, (relation * (Math.Log10(max.Value) - Math.Log10(min.Value))) + Math.Log10(min.Value));
            }
            else
            {
                return (relation * (max.Value - min.Value) + min.Value);
            }
        }
        public double PositionHeightToValueHeight(double height)
        {
            if (IsLogarithmic)
            {
                throw new InvalidOperationException("Operation isn't valid for logarithmic yScale-modes.");
            }

            double relation = (itemsHeight > 0.0) ? (height / itemsHeight) : 1.0;
            return relation * (max.Value - min.Value);
        }
        #endregion
        #endregion

        #region internal methods
        internal void Add(TsVisualization tv)
        {
            Guard.ArgumentNotNull(tv, "tv");

            tvs.Add(tv);
            tv.PainterChanged += tv_PainterChanged;
            
            Invalidate();

            yScalePanel.Add(tv);
        }

        internal void Remove(TsVisualization tv)
        {
            Guard.ArgumentNotNull(tv, "tv");

            tv.PainterChanged -= tv_PainterChanged;
            tvs.Remove(tv);

            Invalidate();

            yScalePanel.Remove(tv);
        }

        /// <summary>
        /// prepares the scaler with for the passed values-range and a control-height
        /// </summary>
        /// <param name="tapeRange">the range of the currently displayed values</param>
        /// <param name="screenHeight">the height of the target-control. The start-point for each scale-operation is 0.</param>
        internal void Prepare(double screenHeight, RangeL tapeRange)
        {
            if (isPrepared && (this.tapeRange == tapeRange) && (this.actualHeight == screenHeight))
            {
                return;
            }

            bool overlaps = tapeRange.Overlaps(this.tapeRange);
            var newRange = tapeRange.Outersection(this.tapeRange);   //  if !overlaps then newRange = tapeRange !

            switch (scaleMode)
            {
                case YScaleMode.Linear:
                case YScaleMode.Logarithmic:
                    {
                        #region compute searchRange:  check if we can restrict to search only the newRange for new extreme values and set
                        RangeL minSearchRange;
                        if (!newRange.IsEmpty && overlaps && tapeRange.Contains(min.TapeIndex))
                        {
                            //  in this case, the new taperange overlaps with the old one for which this scaler has been prepared.
                            //  if additionally the old extremevalue is inside the new(!) tapeRange, we can restrict ourselves to
                            //  search a new exteme value in the non-overlapping range which has been set into the newRange variable above
                            minSearchRange = newRange;
                        }
                        else
                        {
                            //  in every other case we search the whole new tapeRange
                            minSearchRange = tapeRange;
                            min.Value = double.MaxValue;
                        }

                        RangeL maxSearchRange;
                        if (!newRange.IsEmpty && overlaps && tapeRange.Contains(max.TapeIndex))
                        {
                            //  in this case, the new taperange overlaps with the old one for which this scaler has been prepared.
                            //  if additionally the old extremevalue is inside the new(!) tapeRange, we can restrict ourselves to
                            //  search a new exteme value in the non-overlapping range which has been set into the newRange variable above
                            maxSearchRange = newRange;
                        }
                        else
                        {
                            //  in every other case we search the whole new tapeRange
                            maxSearchRange = tapeRange;
                            max.Value = double.MinValue;
                        }
                        #endregion
                        foreach (TsVisualization tv in tvs)
                        {
                            var painter = tv.Painter;
                            var ts = tv.TimeSeries;

                            #region find min
                            {
                                #region find new extreme value (the new minimum)
                                long tsistart = tapeView.GetTimeSeriesStart(ts, minSearchRange.End);
                                long tsiend = tapeView.GetTimeSeriesEnd(ts, minSearchRange.Start);

                                DateTime tnewMin = DateTime.MinValue; //  t will be larger than MinValue if a lower value has been found

                                for (long tsi = tsistart; tsi <= tsiend; tsi++)
                                {
                                    double value = painter.GetMin(tsi);
                                    if (value < min.Value)
                                    {
                                        min.Value = value;
                                        tnewMin = ts.TimeAt(tsi);
                                    }
                                }

                                //  if we found a new minimum, we set the tapeindex (we do this here and not in the loop since it is somewhat more expensive)
                                if (tnewMin > DateTime.MinValue)
                                {
                                    min.TapeIndex = tapeView.IndexAt(tnewMin, minSearchRange);
                                }
                                #endregion
                            }
                            #endregion
                            #region find max
                            {
                                #region find new extreme value (the new maximum)
                                long tsistart = tapeView.GetTimeSeriesStart(ts, maxSearchRange.End);
                                long tsiend = tapeView.GetTimeSeriesEnd(ts, maxSearchRange.Start);

                                DateTime tnewMax = DateTime.MaxValue; //  t will be less than MaxValue if a larger value has been found

                                for (long tsi = tsistart; tsi <= tsiend; tsi++)
                                {
                                    double value = painter.GetMax(tsi);
                                    if (value > max.Value)
                                    {
                                        max.Value = value;
                                        tnewMax = ts.TimeAt(tsi);
                                    }
                                }

                                //  if we found a new minimum, we set the tapeindex (we do this here and not in the loop since it is somewhat more expensive)
                                if (tnewMax < DateTime.MaxValue)
                                {
                                    max.TapeIndex = tapeView.IndexAt(tnewMax, maxSearchRange);
                                }
                                #endregion
                            }
                            #endregion
                        }
                        break;
                    }
                case YScaleMode.Absolute:
                    {
                        // nothing to do here, because extreme-values are always set!
                        break;
                    }
                default:
                    {
                        throw new NotSupportedException("The requested ScaleMode is currently not supported.");
                    }
            }

            #region set preparation parameters
            //  the Scaler is now prepared for these parameters:
            this.actualHeight = screenHeight;
            this.itemsHeight = (screenHeight > itemsPaddingTotal) ? (screenHeight - itemsPaddingTotal) : 0.0;
            this.tapeRange = tapeRange;
            this.isPrepared = true;
            #endregion

            yScalePanel.UpdateValueMarkers(ValueRange);
        }

        #region Absolute Scaling
        internal void MoveRange(double deltaPixel)
        {
            ScaleMode = YScaleMode.Absolute;

            double valueDelta = PositionHeightToValueHeight(Math.Abs(deltaPixel));
            if (deltaPixel < 0) // if position-deltaPixel is negative the value-deltaPixel will be positive
            {
                min.Value += valueDelta;
                max.Value += valueDelta;
            }
            else
            {
                if ((min.Value - valueDelta) < 0)
                {
                    valueDelta = min.Value;
                }
                min.Value -= valueDelta;
                max.Value -= valueDelta;
            }

            AbsoluteScaleChanged();
        }
        internal void ZoomRangeRelative(double deltaPixel)
        {
            ScaleMode = YScaleMode.Absolute;

            double rangeDelta = (Math.Abs(deltaPixel) / actualHeight) * (max.Value - min.Value);
            double minDelta = rangeDelta / 2;
            double maxDelta = rangeDelta / 2;
            if (deltaPixel < 0) // if position-delta is negative the value-delta will be positive
            {
                if (min.Value < minDelta)
                {
                    maxDelta += minDelta - min.Value;
                    minDelta = min.Value;
                }
                min.Value -= minDelta;
                max.Value += maxDelta;
            }
            else
            {
                double actualRange = max.Value - min.Value;
                if (actualRange < rangeDelta)
                {
                    minDelta =
                    maxDelta = (actualRange - 1) / 2;
                }
                min.Value += minDelta;
                max.Value -= maxDelta;
            }

            AbsoluteScaleChanged();
        }

        internal void ZoomRangeAbsolute(double startPosition, double endPosition)
        {
            if (Math.Abs(endPosition - startPosition) > 10)
            {
                ScaleMode = YScaleMode.Absolute;

                if (endPosition > startPosition)
                {
                    // zoom in
                    double minValue = PositionToValue(endPosition);
                    double maxValue = PositionToValue(startPosition);
                    min.Value = minValue;
                    max.Value = maxValue;
                }
                else
                {
                    // zoom out
                    ZoomRangeRelative(endPosition - startPosition);
                }
            }
            // else ignore

            AbsoluteScaleChanged();
        }
        #endregion
        #endregion

        #region private methods
        private void Invalidate()
        {
            isPrepared = false;
        }
        private void AbsoluteScaleChanged()
        {
            Invalidate();
            this.chartControl.UpdateFrame();
        }
        private void tv_PainterChanged(object sender, EventArgs<IPainter> e)
        {
            Invalidate();
        }
        #endregion

        #region fields
        private readonly List<TsVisualization> tvs;
        private readonly ITapeView tapeView;
        private readonly ChartControl chartControl;
        private readonly YScalePanel yScalePanel;
        private YScaleMode scaleMode;

        private bool isPrepared;
        private double actualHeight;
        private double itemsHeight;
        private RangeL tapeRange;
        private readonly ExtremeValue min;
        private readonly ExtremeValue max;

        private const double itemsPadding = 8.0;
        private const double itemsPaddingTotal = itemsPadding * 2;
        #endregion

        #region embedded types
        private class ExtremeValue
        {
            public long TapeIndex;
            public double Value;
        }
        #endregion
    }
}
