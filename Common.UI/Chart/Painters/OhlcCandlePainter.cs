using System;
using System.Windows;
using System.Windows.Media;
using TeaTime.API;
using TeaTime.Data;

namespace TeaTime.Chart.Painters
{
    [PainterDescription("Candles", "OhlcCandlePainter.png", 1007)]
    class OhlcCandlePainter : SlicePainter<OHLC>
    {
        protected override double GetMin(OHLC value) { return value.Low; }
        protected override double GetMax(OHLC value) { return value.High; }
        protected override double GetClose(OHLC value) { return value.Close; }

        public override TimeSeriesDrawing DrawSlice(OHLC value, double left, double width)
        {
            double yOpen = GetYPosition(value.Open);
            double yClose = GetYPosition(value.Close);
            double yLow = GetYPosition(value.Low);
            double yHigh = GetYPosition(value.High);

            bool upBar = value.Close > value.Open;

            double padding = 2.0;
            if (width < 5.0)
            {
                padding = 0.0;
            }

            double middle = left + width / 2;
            double right = left + width - padding;
            left += padding;

            double top;
            double bottom;
            if (upBar)
            {
                top = yClose;
                bottom = yOpen;
            }
            else
            {
                top = yOpen;
                bottom = yClose;                
            }
            top -= 0.5; // correct horizontal line rounding
            bottom -= 0.5; // correct horizontal line rounding

            StreamGeometry geo = new StreamGeometry();
            using (StreamGeometryContext sc = geo.Open())
            {
                sc.BeginFigure(new Point(middle, yHigh), false, false);
                sc.LineTo(new Point(middle, top), true, false);
                sc.BeginFigure(new Point(left, top), true, true);
                sc.LineTo(new Point(right, top), true, false);
                sc.LineTo(new Point(right, bottom), true, false);
                sc.LineTo(new Point(left, bottom), true, false);
                sc.BeginFigure(new Point(middle, bottom), true, false);
                sc.LineTo(new Point(middle, yLow), true, false);
            }

            Brush fill = null;
            if (!upBar)
            {
                fill = new SolidColorBrush(Tv.DrawingAttributes.Color);
            }
            return new TimeSeriesDrawing(geo, fill);
        }
    }
}
