using System.Windows.Controls;
using System.Windows.Ink;

namespace TeaTime.Chart.Markers
{
    public partial class TimeSeriesMarker : Grid
    {
        #region properties
        public string TimeSeriesName
        {
            get
            {
                return text.Text;
            }
            set
            {
                text.Text = value;
            }
        }
        #endregion

        #region ctor
        public TimeSeriesMarker()
        {
            InitializeComponent();
        }
        #endregion

        #region inernal methods
        internal void ApplyDrawingAttributes(DrawingAttributes da)
        {
            path.ApplyDrawingAttributes(da);
        }
        #endregion
    }
}
