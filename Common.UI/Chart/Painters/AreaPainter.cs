using System;
using System.Windows;
using System.Windows.Media;
using TeaTime.API;

namespace TeaTime.Chart.Painters
{
    [PainterDescription("Area", "AreaPainter.png", 2000)]
    class AreaPainter : Painter<double>
    {
        static readonly Color positiveColor = Color.FromArgb(125, 0, 200, 0);
        static readonly Color negativeColor = Color.FromArgb(125, 200, 0, 0);
        StreamGeometry geo;
        StreamGeometryContext geoContext;
        double firstMiddle;
        double lastMiddle;
        double zeroPosition;
        bool hasBegun;

        protected sealed override void BeginDraw(double width, double height)
        {
            geo = new StreamGeometry();
            geoContext = geo.Open();

            this.firstMiddle = 0;
            this.lastMiddle = 0;

            if (Tv.YScaler.ValueRange.Start >= 0)
            {
                zeroPosition = height;
            }
            else if (Tv.YScaler.ValueRange.End < 0)
            {
                zeroPosition = 0;
            }
            else
            {
                zeroPosition = Tv.YScaler.ValueToPosition(0.0);
            }

            hasBegun = false;
        }
        protected sealed override void Draw(double left, double sliceWidth, double height, double value)
        {
            this.lastMiddle = left + sliceWidth / 2; // draw from middle of slice

            if (!hasBegun)
            {
                this.firstMiddle = this.lastMiddle;
                geoContext.BeginFigure(new Point(this.firstMiddle, zeroPosition), true, false);
            }
            geoContext.LineTo(new Point(this.lastMiddle, Tv.YScaler.ValueToPosition(value)), hasBegun, false);
            hasBegun = true;
        }
        protected sealed override TimeSeriesDrawing EndDraw(double width, double height)
        {
            if (hasBegun)
            {
                geoContext.LineTo(new Point(this.lastMiddle, zeroPosition), false, false);
                geoContext.LineTo(new Point(this.firstMiddle, zeroPosition), false, false);
            }
            
            this.geoContext.Close();

            #region fill
            double zeroOffset = Tv.YScaler.ValueToPosition(0) / height;
            var stops = new GradientStopCollection();
            stops.Add(new GradientStop(positiveColor, zeroOffset));
            stops.Add(new GradientStop(negativeColor, zeroOffset));
            var fill = new LinearGradientBrush(stops, new Point(0, 0), new Point(0, 1));
            #endregion

            return new TimeSeriesDrawing(geo, fill);
        }

        protected sealed override double GetMin(double value)
        {
            return value;
        }
        protected sealed override double GetMax(double value)
        {
            return value;
        }
        protected sealed override double GetClose(double value)
        {
            return value;
        }
    }
}
