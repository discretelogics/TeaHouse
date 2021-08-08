using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace TeaTime.Chart.Settings
{
    public class ChartSettings : NotifyPropertyChanged
    {
        private string name;
        public string Name 
        {
            get { return name; }
            set { SetProperty(ref name, value); }
        }

        private readonly ObservableCollection<PanelSettings> panelSettings;
        [JsonProperty(ItemIsReference = true)]
        public ObservableCollection<PanelSettings> PanelSettings { get { return panelSettings; } }


        private PanelSettings selectedPanelSettings;
        [JsonProperty(IsReference=true)]
        public PanelSettings SelectedPanelSettings
        {
            get { return selectedPanelSettings; }
            set { SetProperty(ref selectedPanelSettings, value); }
        }
        
        public ChartSettings()
        {
            panelSettings = new ObservableCollection<PanelSettings>();
        }

        public override string ToString()
        {
            return name;
        }
    }
}