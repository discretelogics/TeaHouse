using System;
using System.Linq;
using TeaTime.Base;
using TeaTime.Chart.Painters;

namespace TeaTime.Chart.Settings
{
    static class SettingsFactory
    {
        public static ItemTypeSettings CreateDefaultItemTypeSettings(Type tsItemType)
        {
            var proposal = new ItemTypeSettings();
            proposal.IsProposal = true;

            var cs = new ChartSettings();
            cs.Name = proposal.GetDefaultChartSettingsName();

            try
            {
                int panelIndex = 0;
                var tsNumericFields = tsItemType.GetNumericItemFields();
                var closeField = tsNumericFields.FirstOrDefault(f => f.Is("close"));
                var valueField = tsNumericFields.FirstOrDefault(f => f.Is("value"));
                var priceField = tsNumericFields.FirstOrDefault(f => f.Is("price"));
                var volumeField = tsNumericFields.FirstOrDefault(f => f.Is("volume"));
                var interestField = tsNumericFields.FirstOrDefault(f => f.Is("interest"));

                #region main panel
                var mainPanel = new PanelSettings(panelIndex++);

                var pas = PainterManager.Instance.FindPaintersByItemType(tsItemType).ToArray();
                foreach (var pa in pas)
                {
                    mainPanel.Painters.Add(CreateDefaultPainterMapping(pa.PainterType, pa.DefaultInstance.ItemType, tsItemType));
                }

                if (closeField != null || priceField != null)
                {
                    mainPanel.Name = "Price";
                }

                if (closeField != null)
                    mainPanel.Painters.Add(CreateDoublePainterMapping(typeof(LinePainter), closeField.Name, "Close"));
                if (priceField != null)
                    mainPanel.Painters.Add(CreateDoublePainterMapping(typeof(LinePainter), priceField.Name, "Price"));
                if (valueField != null)
                    mainPanel.Painters.Add(CreateDoublePainterMapping(typeof(LinePainter), valueField.Name, "Value"));

                mainPanel.SelectedPainter = mainPanel.Painters.FirstOrDefault();
                cs.PanelSettings.Add(mainPanel);
                #endregion

                #region sub panels
                if (volumeField != null)
                {
                    var volumePanel = new PanelSettings(panelIndex++) { Name = "Volume", RelativeHeight = 0.5 };
                    var pm = CreateDoublePainterMapping(typeof(HistogramPainter), volumeField.Name);
                    volumePanel.Painters.Add(pm);
                    cs.PanelSettings.Add(volumePanel);
                }
                if (interestField != null)
                {
                    var interestPanel = new PanelSettings(panelIndex++) { Name = "Interest", RelativeHeight = 0.5 };
                    var pm = CreateDoublePainterMapping(typeof(LinePainter), interestField.Name);
                    interestPanel.Painters.Add(pm);
                    cs.PanelSettings.Add(interestPanel);
                }
                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to create default settings: " + ex.Message);
            }

            cs.SelectedPanelSettings = cs.PanelSettings.FirstOrDefault();   // for designer only
            proposal.ChartSettings.Add(cs);
            proposal.SelectedChartSettings = cs;
            return proposal;
        }

        private static PainterMapping CreateDoublePainterMapping(Type painterType, string tsFieldName, string name = null)
        {
            var pm = CreateDefaultPainterMapping(painterType, typeof (double));
            pm.FieldMappings[0].FileItemFieldName = tsFieldName;
            pm.Name = name;
            return pm;
        }

        public static PainterMapping CreateDefaultPainterMapping(Type painterType, Type painterItemType, Type tsItemType = null)
        {
            var pm = new PainterMapping { PainterType = new StorableType(painterType) };

            var requiredFields = painterItemType.GetAllInstanceFields();
            foreach (var fm in pm.FieldMappings.ToArray())
            {
                if (!requiredFields.Any(f => f.Name == fm.AccessorItemFieldName))
                {
                    pm.FieldMappings.Remove(fm);
                }
            }
            foreach (var f in requiredFields)
            {
                if (pm.FieldMappings.All(fmm => fmm.AccessorItemFieldName != f.Name))
                {
                    var fm = new API.FieldMapping(f.Name);
                    pm.FieldMappings.Add(fm);
                }
            }

            if (tsItemType != null)
            {
                var availableFields = tsItemType.GetAllInstanceFields();
                foreach (var fieldMapping in pm.FieldMappings)
                {
                    var f = availableFields.FirstOrDefault(fi => fi.Is(fieldMapping.AccessorItemFieldName));
                    if (f != null)
                    {
                        fieldMapping.FileItemFieldName = f.Name;
                    }
                }
            }
            return pm;
        }
    }
}
