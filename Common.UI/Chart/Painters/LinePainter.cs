using System;
using System.Windows;
using System.Windows.Media;
using TeaTime.API;

namespace TeaTime.Chart.Painters
{
    [PainterDescription("Line", "LinePainter.png", 1008)]
    internal class LinePainter : Painter<double>
    {
        StreamGeometry geo;
        StreamGeometryContext geoContext;
        bool hasBegun;

        protected sealed override void BeginDraw(double width, double height)
        {
            geo = new StreamGeometry();
            geoContext = geo.Open();
            hasBegun = false;
        }
        protected sealed override void Draw(double left, double sliceWidth, double height, double value)
        {
            double middle = left + sliceWidth / 2; // draw from middle of slice

            if (!hasBegun)
            {
                geoContext.BeginFigure(new Point(middle, Tv.YScaler.ValueToPosition(value)), false, false);
                hasBegun = true;
            }
            else
            {
                geoContext.LineTo(new Point(middle, Tv.YScaler.ValueToPosition(value)), true, false);
            }
        }
        protected sealed override TimeSeriesDrawing EndDraw(double width, double height)
        {
            geoContext.Close();
            return new TimeSeriesDrawing(geo, null);
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
