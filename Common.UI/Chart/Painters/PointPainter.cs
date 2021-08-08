using System;
using System.Windows;
using System.Windows.Media;
using TeaTime.API;

namespace TeaTime.Chart.Painters
{
    [PainterDescription("Point", "PointPainter.png", 1009)]
    class PointPainter : SlicePainter<double>
    {
        protected override double GetMin(double value) { return value; }
        protected override double GetMax(double value) { return value; }
        protected override double GetClose(double value) { return value; }

        public override TimeSeriesDrawing DrawSlice(double value, double left, double width)
        {
            double middle = left + width / 2;
            var geo = new EllipseGeometry(new Point(middle, Tv.YScaler.ValueToPosition(value)), radius, radius);
            return new TimeSeriesDrawing(geo, null);
        }

        private const double radius = 0.5;
    }
}
