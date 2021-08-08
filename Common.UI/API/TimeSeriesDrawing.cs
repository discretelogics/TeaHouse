using System.Windows.Media;

namespace TeaTime.API
{
    /// <summary>
    /// Defines how to draw a specific TimeSeries.
    /// </summary>
    /// <remarks>
    /// An fast alternative to GeometryDrawing for unbound access only.
    /// </remarks>
    public sealed class TimeSeriesDrawing
    {
        public Geometry Geometry
        {
            get
            {
                return geometry;
            }
        }
        public Brush Fill
        {
            get
            {
                return fill;
            }
        }

        public TimeSeriesDrawing(Geometry geometry, Brush fill)
        {
            this.geometry = geometry;
            this.fill = fill;
        }

        Geometry geometry;
        Brush fill;
    }

}
