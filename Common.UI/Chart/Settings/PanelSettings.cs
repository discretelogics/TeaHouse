using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Ink;
using System.Windows.Media;
using TeaTime.Chart.Core;

namespace TeaTime.Chart.Settings
{
    public class PanelSettings : NotifyPropertyChanged
    {
        private int index;
        public int Index 
        {
            get { return index; }
            set
            {
                var oldIndex = index;
                if (SetProperty(ref index, value))
                {
                    if (Name == this.GetDefaultName(oldIndex))
                    {
                        Name = this.GetDefaultName(index);
                    }
                }
            }
        }
        private string name;
        public string Name
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private readonly ObservableCollection<PainterMapping> painters;
        [JsonProperty(ItemIsReference = true)]
        public ObservableCollection<PainterMapping> Painters { get { return painters; } }

        private PainterMapping selectedPainter;
        [JsonProperty(IsReference = true)]
        public PainterMapping SelectedPainter
        {
            get { return selectedPainter; }
            set { SetProperty(ref selectedPainter, value); }
        }

        private YScaleMode yScaleMode;
        public YScaleMode YScaleMode
        {
            get { return yScaleMode; }
            set { SetProperty(ref yScaleMode, value); }
        }

        internal const double MinRelativeHeight = 0.1;
        private double relativeHeight;
        public double RelativeHeight
        {
            get { return relativeHeight; }
            set { SetProperty(ref relativeHeight, value); }
        }

        private static readonly DrawingAttributes DefaultDrawingAttributes = new DrawingAttributes { Color = Colors.Black, Height = 1 };
        private DrawingAttributes drawingAttributes;
        [JsonConverter(typeof(JsonConverters.XamlConverter))]
        public DrawingAttributes DrawingAttributes
        {
            get { return drawingAttributes ?? DefaultDrawingAttributes; }
            set { SetProperty(ref drawingAttributes, value); }
        }

        private string GetDefaultName(int index)
        {
            return "Panel" + (index + 1);
        }

        public PanelSettings(int index)
        {
            this.index = index;
            this.name = GetDefaultName(index);
            painters = new ObservableCollection<PainterMapping>();
            relativeHeight = 1;
            yScaleMode = YScaleMode.Linear;
        }
    }
}
