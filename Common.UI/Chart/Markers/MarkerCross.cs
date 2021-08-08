using System.Windows;
using System.Windows.Controls;
using TeaTime.Chart.Core;

namespace TeaTime.Chart.Markers
{
    internal class MarkerCross
    {
        #region properties
        public Point ActualPosition
        {
            get
            {
                return new Point(verticalLine.ActualPosition, horizontalLine.ActualPosition);
            }
        }
        #endregion

        #region public methods
        public void Update(double xPosition, double yPosition)
        {
            verticalLine.Update(xPosition);
            horizontalLine.Update(yPosition);
        }

        #region ISelectable Members
        public void Select(ISelectable sender)
        {
            if (sender != this)
            {
                horizontalLine.Select(sender);
                verticalLine.Select(sender);
            }
        }
        public void Deselect(ISelectable sender)
        {
            if (sender != this)
            {
                horizontalLine.Deselect(sender);
                verticalLine.Deselect(sender);
            }
        }
        #endregion
        #endregion

        #region ctor / Dispose
        public MarkerCross(Canvas container, bool isBackground, bool isBold)
        {
            horizontalLine = new MarkerLine(Orientation.Horizontal, container, isBackground, isBold);
            verticalLine = new MarkerLine(Orientation.Vertical, container, isBackground, isBold);
        }

        public void Dispose()
        {
            horizontalLine.Dispose();
            verticalLine.Dispose();
        }
        #endregion

        #region fields
        private MarkerLine horizontalLine;
        private MarkerLine verticalLine;
        #endregion
    }
}
