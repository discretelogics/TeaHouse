using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;

namespace TeaTime.API
{
    [InheritedExport]
    public interface IPainter
    {
        ImageSource Image { get; }
        string Text { get; }
        int Order { get; }
        Type ItemType { get; }

        TimeSeriesDrawing Draw(RangeL tapeIndexRange, double panelWidth);
        double GetMin(long tsi);
        double GetMax(long tsi);
        double GetClose(long tsi);

        // TODO: hide methods
        void InitializeInternal(ITapeView tapeView, ITsVisualization tv);
    }
}
