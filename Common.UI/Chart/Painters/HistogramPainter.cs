using System;
using System.Windows;
using System.Windows.Media;
using TeaTime.API;

namespace TeaTime.Chart.Painters
{
    [PainterDescription("Histogram", "HistogramPainter.png", 1011)]
    class HistogramPainter : SlicePainter<double>
    {
        protected override double GetMin(double value) { return value; }
        protected override double GetMax(double value) { return value; }
        protected override double GetClose(double value) { return value; }

        public override TimeSeriesDrawing DrawSlice(double value, double left, double width)
        {
            double right = left + width;
            double top = GetYPosition(value) - 0.5; // correct horizontal line rounding

            StreamGeometry sliceGeometry = new StreamGeometry();
            sliceGeometry.FillRule = FillRule.Nonzero;
            using (StreamGeometryContext sc = sliceGeometry.Open())
            {
                sc.BeginFigure(new Point(left, Tv.YScaler.ActualHeight), true, false);
                sc.LineTo(new Point(left, top), true, false);
                sc.LineTo(new Point(right, top), true, false);
                sc.LineTo(new Point(right, Tv.YScaler.ActualHeight), true, false);
            }

            return new TimeSeriesDrawing(sliceGeometry, null);
        }
    }
}
