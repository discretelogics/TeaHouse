// copyright discretelogics 2012. released under the gpl v3. see license.txt for details.

using System.Windows;
using System.Windows.Media;
using TeaTime.API;
using TeaTime.Data;

namespace TeaTime.Chart.Painters
{
    [PainterDescription("Bars", "OhlcBarPainter.png", 1006)]
    class OhlcBarPainter : SlicePainter<OHLC>
    {
        protected override double GetMin(OHLC value) { return value.Low; }
        protected override double GetMax(OHLC value) { return value.High; }
        protected override double GetClose(OHLC value) { return value.Close; }

        public override TimeSeriesDrawing DrawSlice(OHLC value, double left, double width)
        {
            double yOpen = GetYPosition(value.Open) - 0.5; // correct horizontal line rounding
            double yClose = GetYPosition(value.Close) - 0.5; // correct horizontal line rounding
            double yLow = GetYPosition(value.Low);
            double yHigh = GetYPosition(value.High);

            double middle = left + width / 2;
            double right = left + width;

            var geo = new StreamGeometry();
            using (var context = geo.Open())
            {
                context.BeginFigure(new Point(left, yOpen), true, false);
                context.LineTo(new Point(middle, yOpen), true, false);
                context.BeginFigure(new Point(middle, yHigh), true, false);
                context.LineTo(new Point(middle, yLow), true, false);
                context.BeginFigure(new Point(middle, yClose), true, false);
                context.LineTo(new Point(right, yClose), true, false);
            }

            return new TimeSeriesDrawing(geo, null);
        }
    }
}
