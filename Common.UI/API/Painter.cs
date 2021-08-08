using System;
using System.Linq;
using System.Windows.Media;
using TeaTime.Chart.Painters;

namespace TeaTime.API
{
    public abstract class Painter<T> : IPainter where T : struct
    {
        #region properties
        
        public virtual ImageSource Image
        {
            get
            {
                var desc = GetDescriptionAttribute();
                if (desc != null) return ResourcesUtility.GetImage(desc.ImageResourceName);
                return null;
            }
        }
        public virtual string Text
        {
            get
            {
                var desc = GetDescriptionAttribute();
                if (desc != null) return desc.Text;
                return "Painter";
            }
        }
        public virtual int Order
        {
            get
            {
                var desc = GetDescriptionAttribute();
                if (desc != null) return desc.Order;                
                return int.MaxValue;
            }
        }
        public Type ItemType
        {
            get
            {
                return typeof(T);
            }
        }

        protected ITsVisualization Tv { get { return tv; } }
        #endregion

        #region public methods
        public TimeSeriesDrawing Draw(RangeL tapeIndexRange, double panelWidth)
        {
            double leftX = double.MaxValue;
            double rightX = tapeView.PositionAt(tapeIndexRange.Start) + tapeView.WidthAt(tapeIndexRange.Start);
            double height = tv.YScaler.ActualHeight;

            var tapeIndex = tapeIndexRange.Start;
            var endTapeIndex = tapeIndexRange.End;
            long tsi = tapeView.GetTimeSeriesStart(tv.TimeSeries, tapeIndex);
            DateTime leftTime = tapeView.TimeAt(tapeIndexRange.End);
            DateTime time = tv.TimeSeries.TimeAt(tsi); //  the time of the current TimeSeries Item

            BeginDraw(panelWidth, height);

            //  we draw from right to left
            while (time >= leftTime)
            {
                double newX = tapeView.PositionAt(tapeIndex);
                if ((leftX - newX) >= 1) // don't draw in sub-pixel-regions
                {
                    leftX = newX;

                    double sliceWidth = tapeView.WidthAt(tapeIndex);
                    double left = panelWidth - (rightX - leftX);

                    Draw(left, sliceWidth, height, this.items[tsi]);
                }

                tsi--;
                if (tsi < 0) break;

                time = tv.TimeSeries.TimeAt(tsi);

                // find TapeIndex => performant version of tapeIndex = tapeView.IndexAt(time, new Range(tapeIndex, endTapeIndex))
                tapeIndex++;
                while ((tapeIndex <= endTapeIndex) &&
                        (tapeView.TimeAt(tapeIndex) > time))
                {
                    tapeIndex++;
                }
            }

            return EndDraw(panelWidth, height);
        }

        public double GetMin(long tsi)
        {
            return GetMin(this.items[tsi]);
        }
        public double GetMax(long tsi)
        {
            return GetMax(this.items[tsi]);
        }
        public double GetClose(long tsi)
        {
            return GetClose(this.items[tsi]);
        }
        
        // TODO: support MinWidth
        public void InitializeInternal(ITapeView tapeView, ITsVisualization tv)
        {
            this.tapeView = tapeView;
            this.tv = tv;
            this.items = tv.TimeSeries.GetItemsConverted<T>(tv.FieldMappings);
        }
        #endregion

        #region protected methods

        protected abstract void BeginDraw(double width, double height);
        protected abstract void Draw(double left, double sliceWidth, double height, T value);
        protected abstract TimeSeriesDrawing EndDraw(double width, double height);

        protected double GetYPosition(double value)
        {
            return this.tv.YScaler.ValueToPosition(value);
        }

        protected abstract double GetMin(T value);
        protected abstract double GetMax(T value);
        protected abstract double GetClose(T value);
        #endregion

        #region private methods
        PainterDescriptionAttribute GetDescriptionAttribute()
        {
            return this.GetType().GetAttribute<PainterDescriptionAttribute>();
        }
        #endregion

        #region fields
        ITapeView tapeView;
        ITsVisualization tv;
        IItemCollection<T> items;
        #endregion
    }
}
