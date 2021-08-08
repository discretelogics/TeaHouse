using System;
using System.Collections.Generic;
using TeaTime.API;

namespace TeaTime.Chart.Core
{
    internal abstract class TapeViewBase : ITapeView
    {
        #region properties
        public long Length
        {
            get
            {
                return length;
            }
        }
        protected long MaxIndex
        {
            get
            {
                return maxIndex;
            }
        }

        public bool IsZoomed
        {
            get
            {
                return scaleFactor != 1;
            }
        }

        public abstract IEnumerable<ITeaFile> Tss { get; }
        #endregion

        #region ctor
        public TapeViewBase()
        {
            this.scaleFactor = 1;
        }
        #endregion

        #region events

        public event EventHandler<EventArgs<long>> LengthChanged;
        protected void UpdateLength(long newLength)
        {
            if (this.length != newLength)
            {
                this.length = newLength;
                this.maxIndex = newLength - 1;
                if (LengthChanged != null)
                {
                    LengthChanged(this, new EventArgs<long>(newLength));
                }
            }
        }

        public event EventHandler<EventArgs<double>> ScaleFactorChanged;
        private void UpdateScaleFactor(double newScaleFactor)
        {
            if ((this.scaleFactor != newScaleFactor) &&
                (newScaleFactor <= 100) &&
                (newScaleFactor >= 0.01))
            {
                this.scaleFactor = newScaleFactor;
                if (ScaleFactorChanged != null)
                {
                    ScaleFactorChanged(this, new EventArgs<double>(newScaleFactor));
                }
            }
        }
        #endregion

        #region public methods
        public abstract void Add(params ITeaFile[] tss);
        public abstract void Remove(params ITeaFile[] tss);

        public abstract RangeL ComputeTapeRange(RangeL range);

        public long GetTimeSeriesEnd(ITeaFile ts, long index)
        {
            Guard.ArgumentNotNull(ts, "ts");

            DateTime time = TimeAt(index);
            long tsi = ts.IndexAt(time, RoundMode.Up);
            if (tsi < 0)
            {
                tsi = ts.IndexAt(time, RoundMode.Down);
            }
            return tsi;
        }
        public long GetTimeSeriesStart(ITeaFile ts, long index)
        {
            Guard.ArgumentNotNull(ts, "ts");

            DateTime time = TimeAt(index);
            long tsi = ts.IndexAt(time, RoundMode.Down);
            if (tsi < 0)
            {
                tsi = ts.IndexAt(time, RoundMode.Up);
            }
            return tsi;
        }

        public abstract DateTime TimeAt(long index);
        public long IndexAt(DateTime t, RangeL searchRange)
        {
            return Algorithms.BinarySearch(TimeAt, searchRange.Start, searchRange.End, t, (t1, t2) => t2.CompareTo(t1));
        }

        #region width
        // TODO: implement width for each slice separately
        public double PositionAt(long index)
        {
            return (maxIndex - index) * defaultWidth * scaleFactor;
        }
        // TODO: implement width for each slice separately
        public double WidthAt(long index)
        {
            return defaultWidth * scaleFactor;
        }
        // TODO: implement width for each slice separately
        public long IndexAt(double position)
        {
            double sliceWidth = defaultWidth * scaleFactor;
            return maxIndex - (long)((position - (position % sliceWidth)) / sliceWidth);
        }

        public void ScaleRangeToWidth(RangeL range, double width)
        {
            double newScaleFactor = width / ((PositionAt(range.End) + WidthAt(range.End)) - PositionAt(range.Start));
            UpdateScaleFactor(newScaleFactor);
        }

        /// <summary>
        /// Zoom the widths with the delta specified in percent.
        /// </summary>
        /// <remarks>
        /// Fires ScaleFactorChanged event.
        /// </remarks>
        /// <param name="delta">The delta int percent to zoom in or out</param>
        public void Zoom(double delta)
        {
            double newScaleFactor = scaleFactor + delta;
            UpdateScaleFactor(newScaleFactor);
        }
        /// <summary>
        /// Sets ScaleFactor to 1.
        /// </summary>
        /// <remarks>
        /// Fires ScaleFactorChanged event.
        /// </remarks>
        public void ResetZoom()
        {
            UpdateScaleFactor(1);
        }
        #endregion
        #endregion

        #region fields
        private double scaleFactor;
        private long length;
        private long maxIndex;

        private const double defaultWidth = 10.0;
        #endregion
    }
}
