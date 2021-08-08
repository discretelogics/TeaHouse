using System;
using System.Collections.Generic;
using System.Windows.Ink;
using TeaTime.API;
using TeaTime.Chart.Painters;
using TeaTime.Chart.Settings;
using Path = System.Windows.Shapes.Path;

namespace TeaTime.Chart.Core
{
    public class TsVisualization : ITsVisualization
    {
        #region properties
        public ITeaFile TimeSeries
        {
            get
            {
                return timeSeries;
            }
        }

        public IYScaler YScaler
        {
            get
            {
                return yScaler;
            }
        }

        internal Path Path
        {
            get
            {
                return path;
            }
        }

        internal YScaler YScalerInternal
        {
            get
            {
                return yScaler;
            }
        }

        public IPainter Painter
        {
            get
            {
                return painter;
            }
        }
        internal PainterActivator PainterActivator
        {
            get
            {
                return painterActivator;
            }
        }
        internal PainterMapping PainterMapping
        {
            get
            {
                return painterMapping;
            }
        }
        public IEnumerable<IFieldMapping> FieldMappings
        {
            get
            {
                return painterMapping == null ? null : painterMapping.FieldMappings;
            }
        }

        internal void UpdatePainter(PainterActivator painterActivator, PainterMapping painterMapping)
        {
            this.painterActivator = painterActivator;
            this.painterMapping = painterMapping;
            CreateAndConfigurePainter();
            if (PainterChanged != null)
            {
                PainterChanged(this, new EventArgs<IPainter>(painter));
            }
        }

        private void CreateAndConfigurePainter()
        {
            this.painter = this.painterActivator.CreateInstance(this.tapeView, this);
        }

        public DrawingAttributes DrawingAttributes
        {
            get
            {
                return drawingAttributes;
            }
            set
            {
                drawingAttributes = value;
                path.ApplyDrawingAttributes(drawingAttributes);
                if (DrawingAttributesChanged != null)
                {
                    DrawingAttributesChanged(this, new EventArgs<DrawingAttributes>(drawingAttributes));
                }
            }
        }
        #endregion

        #region ctor
        internal TsVisualization(ITapeView tapeView, ITeaFile ts, YScaler yScaler, PainterActivator painterActivator, PainterMapping painterMapping, 
            Path path, DrawingAttributes drawingAttributes)
        {
            Guard.ArgumentNotNull(tapeView, "tapeView");
            Guard.ArgumentNotNull(ts, "ts");
            Guard.ArgumentNotNull(yScaler, "yScaler");
            Guard.ArgumentNotNull(painterActivator, "painterActivator");
            Guard.ArgumentNotNull(path, "path");

            this.tapeView = tapeView;
            this.timeSeries = ts;
            this.yScaler = yScaler;
            this.painterActivator = painterActivator;
            this.painterMapping = painterMapping;
            this.CreateAndConfigurePainter();

            this.path = path;
            this.DrawingAttributes = drawingAttributes;
        }
        #endregion

        #region events
        public event EventHandler<EventArgs<IPainter>> PainterChanged;
        public event EventHandler<EventArgs<DrawingAttributes>> DrawingAttributesChanged;
        #endregion

        #region fields
        private readonly ITapeView tapeView;
        private readonly ITeaFile timeSeries;
        private readonly YScaler yScaler;

        private readonly Path path;
        private IPainter painter;
        private PainterActivator painterActivator;
        private PainterMapping painterMapping;
        private DrawingAttributes drawingAttributes;

        #endregion
    }
}
