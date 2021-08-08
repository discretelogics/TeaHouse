using System;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using TeaTime.Chart.Core;

namespace TeaTime.Chart.Markers
{
    /// <summary>
    /// Marker that renders a line and text in a canvas-container.
    /// The text is always horizontal.
    /// </summary>
    public class Marker : IDisposable, ISelectable
    {
        #region properties
        public double ActualPosition
        {
            get
            {
                return actualPosition;
            }
        }

        public Brush Background
        {
            get
            {
                return textBlock.Background;
            }
            set
            {
                textBlock.Background = value;
            }
        }
        #endregion

        #region public methods
        public void Update(double position, string text)
        {
            if (textBlock.Text != text)
            {
                textBlock.Text = text;
            }

            if (actualPosition != position)
            {
                actualPosition = position;

                if ((alignment == MarkerAlignment.Left) || (alignment == MarkerAlignment.Right))
                {
                    double containerHeight = container.ActualHeight;

                    if (position < textHeightCenter)
                    {
                        actualPosition = textHeightCenter;
                    }
                    else if ((containerHeight - position) < textHeightCenter)
                    {
                        actualPosition = containerHeight - textHeightCenter;
                    }

                    Canvas.SetTop(textBlock, actualPosition);
                    Canvas.SetTop(linePath, position);
                }
                else
                {
                    double containerWidth = container.ActualWidth;

                    double desiredWidth = textBlock.Measure().Width;
                    double center = desiredWidth / 2;

                    if (position < center)
                    {
                        Canvas.SetLeft(textBlock, 0.0);
                    }
                    else if ((containerWidth - position) < center)
                    {
                        //Canvas.SetRight(textBlock, 0.0); // bug in WPF => R2?
                        // workaround:
                        Canvas.SetLeft(textBlock, containerWidth - desiredWidth);
                    }
                    else
                    {
                        Canvas.SetLeft(textBlock, position - center);
                    }

                    Canvas.SetLeft(linePath, position);
                }
            }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            if (isBackground)
            {
                textBlock.MouseEnter -= mouseEnter;
                textBlock.MouseLeave -= mouseLeave;
            }

            container.Children.Remove(linePath);
            container.Children.Remove(textBlock);
        }
        #endregion

        #region ISelectable Members
        public void Select(object sender)
        {
            if (sender != this)
            {
                Select();
            }
        }
        public void Deselect(object sender)
        {
            if (sender != this)
            {
                Deselect();
            }
        }
        #endregion

        #region ctor
        public Marker(Canvas container,
                        ISelectable parent,
                        MarkerAlignment alignment,
                        bool isBackground,
                        bool isBold)
        {
            this.container = container;
            this.parent = parent;
            this.alignment = alignment;
            this.isBackground = isBackground;
            this.isBold = isBold;

            actualPosition = double.MinValue;

            length = isBold ? boldWidth : normalWidth;

            #region line
            linePath = new Path();
            RenderOptions.SetEdgeMode(linePath, EdgeMode.Aliased);
            linePath.StrokeThickness = isBold ? BoldThickness : NormalThickness;
            linePath.Stroke = isBackground ? BackgroundBrush : ForegroundBrush;
            if ((alignment == MarkerAlignment.Left) || (alignment == MarkerAlignment.Right))
            {
                linePath.Data = new LineGeometry(new Point(0.0, 0.0), new Point(length, 0.0));
            }
            else
            {
                linePath.Data = new LineGeometry(new Point(0.0, 0.0), new Point(0.0, length));
            }
            if (isBackground)
            {
                container.Children.Insert(0, linePath);
            }
            else
            {
                container.Children.Add(linePath);
            }
            #endregion

            #region textblock
            textBlock = new TextBlock();
            textBlock.Padding = new Thickness(1.0);
            textBlock.FontWeight = isBold ? FontWeights.Bold : FontWeights.Normal;

            textBlock.Text = "1";
            textHeightCenter = textBlock.Measure().Height / 2;
            textBlock.Text = "";

            if (isBackground)
            {
                textBlock.Foreground = BackgroundBrush;
                container.Children.Insert(0, textBlock);

                textBlock.MouseEnter += mouseEnter;
                textBlock.MouseLeave += mouseLeave;
            }
            else
            {
                textBlock.Foreground = ForegroundBrush;
                container.Children.Add(textBlock);
            }
            #endregion

            switch (alignment)
            {
                case MarkerAlignment.Left:
                    Canvas.SetLeft(linePath, 0.0);
                    Canvas.SetLeft(textBlock, length);
                    textBlock.TextAlignment = TextAlignment.Left;
                    break;
                case MarkerAlignment.Top:
                    Canvas.SetTop(linePath, 0.0);
                    Canvas.SetTop(textBlock, length);
                    textBlock.TextAlignment = TextAlignment.Left;
                    break;
                case MarkerAlignment.Right:
                    Canvas.SetRight(linePath, 0.0);
                    Canvas.SetRight(textBlock, length);
                    textBlock.TextAlignment = TextAlignment.Right;
                    break;
                case MarkerAlignment.Bottom:
                    Canvas.SetBottom(linePath, 0.0);
                    Canvas.SetBottom(textBlock, length);
                    textBlock.TextAlignment = TextAlignment.Left;
                    break;
            }

            // precalc margin if the alignment allows it
            if ((alignment == MarkerAlignment.Left) || (alignment == MarkerAlignment.Right))
            {
                textBlock.Margin = new Thickness(0, -textHeightCenter, 0, 0);
            }
        }
        #endregion

        #region private methods
        private void Select()
        {
            if (isBackground)
            {
                linePath.Stroke =
                textBlock.Foreground = ForegroundBrush;
            }
        }
        private void Deselect()
        {
            if (isBackground)
            {
                linePath.Stroke =
                textBlock.Foreground = BackgroundBrush;
            }
        }
        #endregion

        #region eventhandler
        private void mouseEnter(object sender, MouseEventArgs e)
        {
            Select();

            if (parent != null)
            {
                parent.Select(this);
            }
        }
        private void mouseLeave(object sender, MouseEventArgs e)
        {
            Deselect();

            if (parent != null)
            {
                parent.Deselect(this);
            }
        }
        #endregion

        #region fields
        private Path linePath;
        private TextBlock textBlock;

        private Canvas container;
        private ISelectable parent;
        private MarkerAlignment alignment;
        private bool isBackground;
        private bool isBold;

        private double actualPosition;
        private double textHeightCenter;
        private double length;

        public static readonly Brush BackgroundBrush = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
        public static readonly Brush ForegroundBrush = new SolidColorBrush(Colors.Black);
        public static double NormalThickness = 1.0;
        public static double BoldThickness = 2.0;
        private static double normalWidth = 5.0;
        private static double boldWidth = 10.0;
        #endregion
    }
}
