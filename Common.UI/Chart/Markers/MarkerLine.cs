using System;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using TeaTime.Chart.Core;

namespace TeaTime.Chart.Markers
{
    internal class MarkerLine : ISelectable, IDisposable
    {
        #region properties
        public double ActualPosition
        {
            get
            {
                return actualPosition;
            }
        }
        public bool HideOnDeselected
        {
            get
            {
                return hideOnDeselected;
            }
            set
            {
                hideOnDeselected = value;
                if (value)
                {
                    linePath.Visibility = Visibility.Hidden;
                }
            }
        }
        #endregion

        #region public methods
        public void Update(double position)
        {
            if (orientation == Orientation.Vertical)
            {
                if (linePath.ActualHeight != container.ActualHeight)
                {
                    linePath.Data = new LineGeometry(new Point(0, 0), new Point(0, container.ActualHeight));
                }

                if (actualPosition != position)
                {
                    actualPosition = position;
                    Canvas.SetLeft(linePath, position);
                }
            }
            else
            {
                if (linePath.ActualWidth != container.ActualWidth)
                {
                    linePath.Data = new LineGeometry(new Point(0, 0), new Point(container.ActualWidth, 0));
                }

                if (actualPosition != position)
                {
                    actualPosition = position;
                    Canvas.SetTop(linePath, position);
                }
            }
        }
        public void Update()
        {
            double position = actualPosition;
            actualPosition = double.MinValue;
            Update(position);
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            container.Children.Remove(linePath);
        }
        #endregion

        #region ISelectable Members
        public void Select(object sender)
        {
            if (sender != this)
            {
                if (isBackground)
                {
                    linePath.Stroke = Marker.ForegroundBrush;
                }
                if (hideOnDeselected)
                {
                    linePath.Visibility = Visibility.Visible;
                }
            }
        }
        public void Deselect(object sender)
        {
            if (sender != this)
            {
                if (hideOnDeselected)
                {
                    linePath.Visibility = Visibility.Hidden;
                }
                else
                {
                    if (isBackground)
                    {
                        linePath.Stroke = Marker.BackgroundBrush;
                    }
                }
            }
        }
        #endregion

        #region ctor
        public MarkerLine(Orientation orientation, Canvas container, bool isBackground, bool isBold)
        {
            this.hideOnDeselected = false;
            this.actualPosition = double.MinValue;

            this.orientation = orientation;
            this.container = container;
            this.isBackground = isBackground;

            this.linePath = new Path();
            RenderOptions.SetEdgeMode(this.linePath, EdgeMode.Aliased);
            this.linePath.StrokeThickness = isBold ? Marker.BoldThickness : Marker.NormalThickness;
            if (isBackground)
            {
                this.linePath.Stroke = Marker.BackgroundBrush;
                container.Children.Insert(0, linePath);
            }
            else
            {
                this.linePath.Stroke = Marker.ForegroundBrush;
                container.Children.Add(linePath);
            }
        }
        #endregion

        #region fields
        private bool isBackground;
        private bool hideOnDeselected;
        private double actualPosition;

        private readonly Orientation orientation;
        private readonly Canvas container;

        private readonly Path linePath;
        #endregion
    }
}
