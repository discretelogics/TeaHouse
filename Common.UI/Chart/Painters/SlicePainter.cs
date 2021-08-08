using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using TeaTime.API;

namespace TeaTime.Chart.Painters
{
    public abstract class SlicePainter<T> : Painter<T> where T : struct
    {
        GeometryGroup geometryGroup;
        DrawingGroup drawingGroup;

        protected sealed override void BeginDraw(double width, double height)
        {
            geometryGroup = new GeometryGroup();
            drawingGroup = new DrawingGroup();
        }
        protected sealed override void Draw(double left, double sliceWidth, double height, T value)
        {
            TimeSeriesDrawing sliceDrawing = DrawSlice(value, left, sliceWidth);
            if (sliceDrawing != null)
            {
                if (sliceDrawing.Geometry != null)
                {
                    geometryGroup.Children.Add(sliceDrawing.Geometry);
                }
                if (sliceDrawing.Fill != null)
                {
                    var sliceRect = new RectangleGeometry(new Rect(left, 0, sliceWidth, height));
                    drawingGroup.Children.Add(new GeometryDrawing(sliceDrawing.Fill, null, sliceRect));
                }
            }
        }
        protected sealed override TimeSeriesDrawing EndDraw(double width, double height)
        {
            Geometry geo = null;
            DrawingBrush fill = null;
            if (geometryGroup.Children.Any())
            {
                geo = geometryGroup;
            }
            if (drawingGroup.Children.Any())
            {
                fill = new DrawingBrush(drawingGroup);
                fill.Stretch = Stretch.None;
                fill.AlignmentX = AlignmentX.Left;
                fill.AlignmentY = AlignmentY.Top;
                fill.ViewboxUnits = BrushMappingMode.Absolute;
                fill.ViewportUnits = BrushMappingMode.Absolute;
                fill.Viewport = new Rect(0, 0, width, height);
            }
            if ((geo == null) && (fill == null))
            {
                return null;
            }
            return new TimeSeriesDrawing(geo, fill);
        }

        public abstract TimeSeriesDrawing DrawSlice(T value, double left, double width);
    }
}