using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.VisualStudio.Shell;
using TeaTime.Chart.Core;

namespace TeaTime.Chart.Markers
{
    internal class TimeSliceMarker : ISelectable, IDisposable
    {
        #region public methods
        public void Update(double left, double width)
        {
            if ((actualWidth != width) || (path.ActualHeight != container.ActualHeight))
            {
                actualWidth = width;
                path.Data = new RectangleGeometry(new Rect(0, 0, width, container.ActualHeight));
                path.Data.Freeze();
            }

            if (actualLeft != left)
            {
                actualLeft = left;
                Canvas.SetLeft(path, left);
            }
        }
        public void Update()
        {
            Update(actualLeft, actualWidth);
        }

        public void Select(object sender)
        {
            path.Opacity = 1;
        }
        public void Deselect(object sender)
        {
            path.Opacity = 0.5;
        }
        #endregion

        #region ctor / Dispose
        public TimeSliceMarker(Canvas container)
        {
            InitializeStyles(container);

            this.container = container;

            actualLeft = double.NaN;
            actualWidth = double.NaN;

            path = new Path();
            RenderOptions.SetEdgeMode(path, EdgeMode.Aliased);
            path.Fill = fill;
            Deselect(this);
            container.Children.Insert(0, path);
        }
        public static void InitializeStyles(Canvas container)
        {
            if (fill == null)
            {
                fill = ((Brush)container.FindResource(VsBrushes.HighlightKey)).Clone();
            }
        }

        public void Dispose()
        {
            container.Children.Remove(path);
        }
        #endregion

        #region fields
        private double actualLeft;
        private double actualWidth;

        private readonly Canvas container;

        private Path path;

        private static Brush fill = null;
        #endregion
    }
}
