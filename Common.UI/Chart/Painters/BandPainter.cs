using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using TeaTime.API;
using TeaTime.Data;

namespace TeaTime.Chart.Painters
{
    [PainterDescription("Band", "BandPainter.png", 1010)]
    internal class BandPainter : Painter<Band>
    {
        StreamGeometry geo;
        StreamGeometryContext geoContext;
        bool hasBegun;
        Point startPoint;
        readonly List<Point> upperPoints = new List<Point>();

        protected sealed override void BeginDraw(double width, double height)
        {
            this.geo = new StreamGeometry();
            this.geoContext = this.geo.Open();
            this.hasBegun = false;
            this.upperPoints.Clear();
        }
        protected sealed override void Draw(double left, double sliceWidth, double height, Band value)
        {
            double middle = left + sliceWidth / 2;
            var lowerPoint = new Point(middle, Tv.YScaler.ValueToPosition(value.Lower));

            if (!hasBegun)
            {
                this.geoContext.BeginFigure(lowerPoint, true, false);
                this.startPoint = lowerPoint;
                hasBegun = true;
            }
            else
            {
                this.geoContext.LineTo(lowerPoint, true, false);
            }
            this.upperPoints.Add(new Point(middle, Tv.YScaler.ValueToPosition(value.Upper)));
        }
        protected sealed override TimeSeriesDrawing EndDraw(double width, double height)
        {
            if (hasBegun)
            {
                for (int i = upperPoints.Count - 1; i >= 0; i--)
                {
                    this.geoContext.LineTo(upperPoints[i], i < upperPoints.Count - 1, false);
                }
                this.geoContext.LineTo(this.startPoint, false, false);
            }

            this.geoContext.Close();

            var fill = new SolidColorBrush(Color.FromArgb(125, Tv.DrawingAttributes.Color.R, Tv.DrawingAttributes.Color.G, Tv.DrawingAttributes.Color.B));
            return new TimeSeriesDrawing(this.geo, fill);
        }

        protected sealed override double GetMin(Band value)
        {
            return value.Lower;
        }
        protected sealed override double GetMax(Band value)
        {
            return value.Upper;
        }
        protected sealed override double GetClose(Band value)
        {
            return value.Upper;
        }
    }
}
