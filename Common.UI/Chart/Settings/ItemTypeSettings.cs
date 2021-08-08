using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace TeaTime.Chart.Settings
{
    public class ItemTypeSettings : NotifyPropertyChanged
    {
        #region properties

        private readonly ObservableCollection<ChartSettings> chartSettings = new ObservableCollection<ChartSettings>();
        [JsonProperty(ItemIsReference = true)]
        public ObservableCollection<ChartSettings> ChartSettings { get { return chartSettings; } }

        private ChartSettings selectedChartSettings;
        [JsonProperty(IsReference = true)]
        public ChartSettings SelectedChartSettings
        {
            get { return selectedChartSettings; }
            set { SetProperty(ref selectedChartSettings, value); }
        }

        private bool isProposal;
        [JsonIgnore]
        public bool IsProposal
        {
            get { return isProposal; }
            set { SetProperty(ref isProposal, value); }
        }

        [JsonIgnore]
        public Type TsItemType { get; private set; }

        #endregion
        
        internal string GetDefaultChartSettingsName()
        {
            string defaultName;
            int i = 1;
            do
            {
                defaultName = "MyChartSetting" + i++;
            } while (ChartSettings.Any(css => defaultName.Equals(css.Name, StringComparison.InvariantCultureIgnoreCase)));
            return defaultName;
        }
        internal void Clean()
        {
            if (SelectedChartSettings == null || !ChartSettings.Contains(SelectedChartSettings))
            {
                SelectedChartSettings = ChartSettings.FirstOrDefault();
            }
            foreach (var cs in ChartSettings)
            {
                if (String.IsNullOrWhiteSpace(cs.Name))
                {
                    cs.Name = GetDefaultChartSettingsName();
                }
                if (cs.SelectedPanelSettings == null || !cs.PanelSettings.Contains(cs.SelectedPanelSettings))
                {
                    cs.SelectedPanelSettings = cs.PanelSettings.FirstOrDefault();
                }

                foreach (var ps in cs.PanelSettings)
                {
                    if (ps.SelectedPainter != null && !ps.Painters.Contains(ps.SelectedPainter))
                    {
                        ps.SelectedPainter = null;
                    }
                    ps.RelativeHeight = Math.Max(PanelSettings.MinRelativeHeight, ps.RelativeHeight);
                    if (cs.PanelSettings.Except(new[] { ps }).Any(pss => pss.Index == ps.Index))
                    {
                        int i = 0;
                        while (cs.PanelSettings.Any(pss => pss.Index == i))
                        {
                            i++;
                        }
                        ps.Index = i;
                    }
                }
            }
        }
        private void Validate()
        {
            foreach (var cs in ChartSettings)
            {
                foreach (var ps in cs.PanelSettings)
                {
                    foreach (var pm in ps.Painters)
                    {
                        if (pm.FieldMappings.Any(fm => String.IsNullOrWhiteSpace(fm.AccessorItemFieldName)))
                        {
                            throw new ValidationException("Not all field mappings have an accessor item assigned.", cs, ps, pm);
                        }
                        var fms = pm.FieldMappings.Where(fm => String.IsNullOrWhiteSpace(fm.FileItemFieldName));
                        if (fms.Any())
                        {
                            throw new ValidationException("Not all field mappings have a field of the TeaFile assigned.", cs, ps, pm);
                        }
                    }
                }
            }
        }

        public static ItemTypeSettings Read(Type tsItemType)
        {
            var settings = SettingsManager.Instance.Read("Chart", tsItemType.Name, () => SettingsFactory.CreateDefaultItemTypeSettings(tsItemType));
            settings.TsItemType = tsItemType;
            settings.Clean();
            return settings;
        }
        public void Store()
        {
            Validate();
            SettingsManager.Instance.Store("Chart", TsItemType.Name, this);
        }

        public class ValidationException : Exception
        {
            public ChartSettings ChartSettings { get; private set; }
            public PanelSettings PanelSettings { get; private set; }
            public PainterMapping PainterMapping { get; private set; }

            public ValidationException(string message, ChartSettings chartSettings, PanelSettings panelSettings, PainterMapping painterMapping)
                : base(message)
            {
                this.ChartSettings = chartSettings;
                this.PanelSettings = panelSettings;
                this.PainterMapping = painterMapping;
            }
        }
    }
}
