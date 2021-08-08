using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Newtonsoft.Json;
using TeaTime.API;
using TeaTime.Chart.Painters;
using TeaTime.Base;

namespace TeaTime.Chart.Settings
{
    public class PainterMapping : NotifyPropertyChanged
    {
        private StorableType painterType;
        public StorableType PainterType
        {
            get { return painterType; }
            set
            {
                if (SetProperty(ref painterType, value))
                {
                    this.Changed("Image");
                    this.Changed("ToolTip");
                }
            }
        }

        private readonly ObservableCollection<FieldMapping> fieldMappings;
        public ObservableCollection<FieldMapping> FieldMappings { get { return fieldMappings; } }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (SetProperty(ref name, value))
                {
                    this.Changed("ToolTip");
                }
            }
        }

        [JsonIgnore]
        public ImageSource Image
        {
            get
            {
                if (painterType != null)
                {
                    var painter = PainterManager.Instance.FindPainterByType(painterType);
                    if (painter != null)
                    {
                        return painter.DefaultInstance.Image ?? ResourcesUtility.GetImage("Painter.png");
                    }
                }
                return ResourcesUtility.GetImage("Painter.png");
            }
        }

        [JsonIgnore]
        public string ToolTip
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(name))
                    return name;

                if (painterType != null)
                {
                    var painter = PainterManager.Instance.FindPainterByType(painterType);
                    if (painter != null)
                    {
                        return "Draw with " + painter.DefaultInstance.Text;
                    }
                }
                return "Painter unknown";
            }
        }

        public PainterMapping()
        {
            this.fieldMappings = new ObservableCollection<FieldMapping>();
        }
    }
}
