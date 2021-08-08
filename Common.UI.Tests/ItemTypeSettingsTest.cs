using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TeaTime.Chart.Settings;
using SharpTestsEx;
using System.Windows.Media;
using TeaTime.Chart.Painters;
using TeaTime.Data;
using System.IO;
using System.Linq;
using TeaTime.Base;

namespace TeaTime
{
    [TestClass]
    public class ItemTypeSettingsTest
    {
        static readonly string directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "UnitTests");
        [TestInitialize]
        public void Initialize()
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }

            SettingsManager.Instance.Configure((collectionName, name, value) =>
                        {
                            if (!Directory.Exists(directory))
                            {
                                Directory.CreateDirectory(directory);
                            }
                            File.WriteAllText(Path.Combine(directory, name), value);
                        },
                        (collectionName, name) =>
                        {
                            string file = Path.Combine(directory, name);
                            if (File.Exists(file))
                            {
                                return File.ReadAllText(file);
                            }
                            return null;
                        });
        }

        [TestMethod]
        public void DefaultUnknown()
        {
            var examinee = SettingsFactory.CreateDefaultItemTypeSettings(typeof(UnknownEvent));
            examinee.Should().Not.Be.Null();
            examinee.IsProposal.Should().Be.True();
            examinee.ChartSettings.Count.Should().Be(1);
            examinee.ChartSettings[0].Name.Should().Not.Be.NullOrEmpty();
            examinee.SelectedChartSettings.Should().Be.SameInstanceAs(examinee.ChartSettings[0]);
        }

        [TestMethod]
        public void DefaultByPainter()
        {
            // TODO: Implement test
        }

        [TestMethod]
        public void DefaultCustomOHLCVI()
        {
            var examinee = SettingsFactory.CreateDefaultItemTypeSettings(typeof(CustomOHLCVI));
            examinee.Should().Not.Be.Null();
            examinee.IsProposal.Should().Be.True();
            var cs = examinee.ChartSettings[0];
            cs.PanelSettings.Count.Should().Be(3);

            var ps = cs.PanelSettings[0];
            ps.Name.Should().Be("Price");
            ps.Index.Should().Be(0);
            ps.RelativeHeight.Should().Be(1);
            ps.Painters.Count.Should().Be(3);
            ps.Painters[0].PainterType.Should().Not.Be.Null();
            ps.Painters[0].FieldMappings.Count.Should().Be(4);
            ps.Painters[0].FieldMappings[0].AccessorItemFieldName.Should().Be("Open");
            ps.Painters[0].FieldMappings[0].FileItemFieldName.Should().Be("_Open");
            ps.Painters[0].FieldMappings[1].AccessorItemFieldName.Should().Be("High");
            ps.Painters[0].FieldMappings[1].FileItemFieldName.Should().Be("_High");
            ps.Painters[0].FieldMappings[2].AccessorItemFieldName.Should().Be("Low");
            ps.Painters[0].FieldMappings[2].FileItemFieldName.Should().Be("Low");
            ps.Painters[0].FieldMappings[3].AccessorItemFieldName.Should().Be("Close");
            ps.Painters[0].FieldMappings[3].FileItemFieldName.Should().Be("close");
            ps.Painters[1].PainterType.Should().Not.Be.Null();
            ps.Painters[1].FieldMappings.Count.Should().Be(4);
            ps.Painters[2].PainterType.Should().Not.Be.Null();
            ps.Painters[2].FieldMappings.Count.Should().Be(1);
            ps.SelectedPainter.Should().Be.SameInstanceAs(ps.Painters[0]);

            ps = cs.PanelSettings[1];
            ps.Name.Should().Be("Volume");
            ps.Index.Should().Be(1);
            ps.RelativeHeight.Should().Be(0.5M);
            ps.Painters.Count.Should().Be(1);
            ps.Painters[0].FieldMappings.Count.Should().Be(1);
            ps.SelectedPainter.Should().Be.Null();

            ps = cs.PanelSettings[2];
            ps.Name.Should().Be("Interest");
            ps.Index.Should().Be(2);
            ps.RelativeHeight.Should().Be(0.5M);
            ps.Painters.Count.Should().Be(1);
            ps.Painters[0].FieldMappings.Count.Should().Be(1);
            ps.SelectedPainter.Should().Be.Null();
        }

        [TestMethod]
        public void DefaultPrimitiveType()
        {
            var examinee = SettingsFactory.CreateDefaultItemTypeSettings(typeof(long));
            examinee.Should().Not.Be.Null();
            examinee.IsProposal.Should().Be.True();
            var cs = examinee.ChartSettings[0];
            cs.PanelSettings.Count.Should().Be(1);

            var ps = cs.PanelSettings[0];
            ps.Name.Should().Be("Panel1");
            ps.Index.Should().Be(0);
            ps.RelativeHeight.Should().Be(1);                        
            //ps.Painters.Count.Should().Be(3);
            ps.Painters.Select(pm => pm.PainterType.FullName.Split('.').Last()).Should().Have.SameValuesAs("LinePainter", "PointPainter", "HistogramPainter", "AreaPainter");
            ps.Painters[0].FieldMappings[0].AccessorItemFieldName.Should().Be("m_value");
            ps.Painters[0].FieldMappings[0].FileItemFieldName.Should().Be("m_value");
            ps.SelectedPainter.Should().Be.SameInstanceAs(ps.Painters[0]);
        }

        [TestMethod]
        public void DefaultTypeContainingValue()
        {
            var examinee = SettingsFactory.CreateDefaultItemTypeSettings(typeof(CustomValue));
            examinee.Should().Not.Be.Null();
            examinee.IsProposal.Should().Be.True();
            var cs = examinee.ChartSettings[0];
            cs.PanelSettings.Count.Should().Be(1);

            var ps = cs.PanelSettings[0];
            ps.Name.Should().Be("Panel1");
            ps.Index.Should().Be(0);
            ps.RelativeHeight.Should().Be(1);
            ps.Painters.Count.Should().Be(1);
            ps.Painters[0].FieldMappings[0].AccessorItemFieldName.Should().Be("m_value");
            ps.Painters[0].FieldMappings[0].FileItemFieldName.Should().Be("_value");
            ps.SelectedPainter.Should().Be.SameInstanceAs(ps.Painters[0]);
        }

        [TestMethod]
        public void StoreAndRead()
        {
            var examinee = CreateEmptyUnitTestSettings();
            var read = StoreAndRead(examinee); // empty
            AssertEquality(examinee, read);
            
            var chartSettings1 = new ChartSettings { Name = "chartSettings1" };
            var chartSettings2 = new ChartSettings { Name = "chartSettings2" };
            var chartSettings3 = new ChartSettings { Name = "chartSettings3" };
            examinee.ChartSettings.Add(chartSettings1);
            examinee.ChartSettings.Add(chartSettings2);
            examinee.ChartSettings.Add(chartSettings3);
            read = StoreAndRead(examinee); // empty chart settings without default
            AssertEquality(examinee, read);

            examinee.SelectedChartSettings = chartSettings2;
            read = StoreAndRead(examinee); // empty chart settings with default
            AssertEquality(examinee, read);

            var panelSettings1 = new PanelSettings(1) { Name = "panelSettings1" };
            var panelSettings2 = new PanelSettings(2) { Name = "panelSettings2", Index = 2, YScaleMode = Chart.Core.YScaleMode.Logarithmic, RelativeHeight = 2.5, DrawingAttributes = new System.Windows.Ink.DrawingAttributes { Color = Colors.Orange, Height = 2 } };
            var panelSettings3 = new PanelSettings(5) { Name = null, YScaleMode = Chart.Core.YScaleMode.Absolute };
            chartSettings1.PanelSettings.Add(panelSettings1);
            chartSettings1.PanelSettings.Add(panelSettings2);
            chartSettings2.PanelSettings.Add(panelSettings3);
            read = StoreAndRead(examinee); // with panelsettings without painters
            AssertEquality(examinee, read);

            var painterMapping1 = new PainterMapping { PainterType = new StorableType(typeof(OhlcBarPainter)) };
            var painterMapping2 = new PainterMapping { PainterType = new StorableType(typeof(LinePainter)) };
            panelSettings1.Painters.Add(painterMapping1);
            panelSettings1.Painters.Add(painterMapping2);
            read = StoreAndRead(examinee); // with empty painters
            AssertEquality(examinee, read);

            painterMapping1.FieldMappings.Add(new API.FieldMapping("painterField1", "tsField1"));
            painterMapping1.FieldMappings.Add(new API.FieldMapping("painterField2", "tsField2"));
            read = StoreAndRead(examinee); // fully configured painters
            AssertEquality(examinee, read);
        }

        [TestMethod]
        public void CreateDefaultPainterMapping()
        {
            var examinee = SettingsFactory.CreateDefaultPainterMapping(typeof (OhlcBarPainter), typeof (OHLC));
            examinee.FieldMappings.Count.Should().Be(4);
            examinee.FieldMappings[0].AccessorItemFieldName.Should().Be("Open");
            examinee.FieldMappings[1].AccessorItemFieldName.Should().Be("High");
            examinee.FieldMappings[2].AccessorItemFieldName.Should().Be("Low");
            examinee.FieldMappings[3].AccessorItemFieldName.Should().Be("Close");
            examinee.FieldMappings[0].FileItemFieldName.Should().Be.NullOrEmpty();
            examinee.FieldMappings[1].FileItemFieldName.Should().Be.NullOrEmpty();
            examinee.FieldMappings[2].FileItemFieldName.Should().Be.NullOrEmpty();
            examinee.FieldMappings[3].FileItemFieldName.Should().Be.NullOrEmpty();

            examinee = SettingsFactory.CreateDefaultPainterMapping(typeof(OhlcBarPainter), typeof(OHLC), typeof(CustomOHLCVI));
            examinee.FieldMappings.Count.Should().Be(4);
            examinee.FieldMappings[0].AccessorItemFieldName.Should().Be("Open");
            examinee.FieldMappings[1].AccessorItemFieldName.Should().Be("High");
            examinee.FieldMappings[2].AccessorItemFieldName.Should().Be("Low");
            examinee.FieldMappings[3].AccessorItemFieldName.Should().Be("Close");
            examinee.FieldMappings[0].FileItemFieldName.Should().Be("_Open");
            examinee.FieldMappings[1].FileItemFieldName.Should().Be("_High");
            examinee.FieldMappings[2].FileItemFieldName.Should().Be("Low");
            examinee.FieldMappings[3].FileItemFieldName.Should().Be("close");


            examinee = SettingsFactory.CreateDefaultPainterMapping(typeof(LinePainter), typeof(double), typeof(double));
            examinee.FieldMappings.Count.Should().Be(1);
            examinee.FieldMappings[0].AccessorItemFieldName.Should().Be("m_value");
            examinee.FieldMappings[0].FileItemFieldName.Should().Be("m_value");
        }

        [TestMethod]
        public void Clean()
        {
            var examinee = CreateEmptyUnitTestSettings();
            var chartSettings1 = new ChartSettings { Name = "chartSettings1" };
            var chartSettings2 = new ChartSettings { Name = "chartSettings2" };
            var chartSettings3 = new ChartSettings { Name = "chartSettings3" };
            examinee.ChartSettings.Add(chartSettings1);
            examinee.ChartSettings.Add(chartSettings2);
            
            examinee.SelectedChartSettings = chartSettings3;
            examinee.Clean();   // invalid settings selection
            examinee.SelectedChartSettings.Should().Be.SameInstanceAs(examinee.ChartSettings[0]);

            chartSettings1.Name = null;
            chartSettings2.Name = " ";
            examinee.Clean();   // empty chartSettings names
            examinee.ChartSettings[0].Name.Should().Be.EqualTo("MyChartSetting1");
            examinee.ChartSettings[1].Name.Should().Be.EqualTo("MyChartSetting2");

            var panelSettings1 = new PanelSettings(1) { Name = "panelSettings1" };
            var panelSettings2 = new PanelSettings(1) { Name = "panelSettings2" };
            chartSettings2.PanelSettings.Add(panelSettings1);
            chartSettings2.PanelSettings.Add(panelSettings2);
            examinee.Clean();    // duplicate indices
            examinee.ChartSettings[1].PanelSettings[0].Index.Should().Be(0);
            examinee.ChartSettings[1].PanelSettings[1].Index.Should().Be(1);

            panelSettings2.RelativeHeight = 0;
            examinee.Clean();    // unvisible panel
            examinee.ChartSettings[1].PanelSettings[1].RelativeHeight.Should().Be.EqualTo(0.1);

            var painterMapping1 = new PainterMapping { PainterType = new StorableType(typeof(OhlcBarPainter)) };
            var painterMapping2 = new PainterMapping { PainterType = new StorableType(typeof(LinePainter)) };
            var painterMapping3 = new PainterMapping { PainterType = new StorableType(typeof(OhlcCandlePainter)) };
            panelSettings2.Painters.Add(painterMapping1);
            panelSettings2.Painters.Add(painterMapping2);
            panelSettings2.SelectedPainter = painterMapping3;
            examinee.Clean();    // invalid painter selection
            examinee.ChartSettings[1].PanelSettings[1].SelectedPainter.Should().Be.Null();
        }

        [TestMethod]
        public void Validate()
        {
            var examinee = CreateEmptyUnitTestSettings();
            var chartSettings1 = new ChartSettings { Name = "chartSettings1" };
            examinee.ChartSettings.Add(chartSettings1);
            var panelSettings1 = new PanelSettings(1) { Name = "panelSettings1"};
            chartSettings1.PanelSettings.Add(panelSettings1);
            var painterMapping1 = SettingsFactory.CreateDefaultPainterMapping(typeof(OhlcBarPainter), typeof(OHLC));
            panelSettings1.Painters.Add(painterMapping1);

            Executing.This(examinee.Store).Should().Throw<ItemTypeSettings.ValidationException>();

            painterMapping1.FieldMappings.ForEach(fm => fm.FileItemFieldName = "FieldName");
            Executing.This(examinee.Store).Should().NotThrow();
        }

        private ItemTypeSettings CreateEmptyUnitTestSettings()
        {
            var itemType = typeof(UnitTestEvent);
            var settings = ItemTypeSettings.Read(itemType);
            settings.ChartSettings.Clear();
            settings.SelectedChartSettings = null;
            return settings;
        }

        private ItemTypeSettings StoreAndRead(ItemTypeSettings settings)
        {
            settings.Store();
            return ItemTypeSettings.Read(settings.TsItemType);
        }

        private void AssertEquality(ItemTypeSettings expected, ItemTypeSettings actual)
        {
            actual.ChartSettings.Count.Should().Be.EqualTo(expected.ChartSettings.Count);
            for (int i = 0; i < expected.ChartSettings.Count; i++)
            {
                var expectedChartSettings = expected.ChartSettings[i];
                var actualChartSettings = actual.ChartSettings[i];
                actualChartSettings.Name.Should().Be.EqualTo(expectedChartSettings.Name);
                if (expected.SelectedChartSettings == expectedChartSettings)
                {
                    actual.SelectedChartSettings.Should().Be.SameInstanceAs(actualChartSettings);
                }

                actualChartSettings.PanelSettings.Count.Should().Be(expectedChartSettings.PanelSettings.Count);
                for (int j = 0; j < expectedChartSettings.PanelSettings.Count; j++)
                {
                    var expectedPanelSettings = expectedChartSettings.PanelSettings[j];
                    var actualPanelSettings = actualChartSettings.PanelSettings[j];
                    actualPanelSettings.DrawingAttributes.Should().Be.EqualTo(expectedPanelSettings.DrawingAttributes);
                    actualPanelSettings.Index.Should().Be(expectedPanelSettings.Index);
                    actualPanelSettings.Name.Should().Be(expectedPanelSettings.Name);
                    actualPanelSettings.RelativeHeight.Should().Be(expectedPanelSettings.RelativeHeight);
                    actualPanelSettings.YScaleMode.Should().Be(expectedPanelSettings.YScaleMode);

                    actualPanelSettings.Painters.Count.Should().Be.EqualTo(expectedPanelSettings.Painters.Count);
                    for (int k = 0; k < expectedPanelSettings.Painters.Count; k++)
                    {
                        var expectedPainter = expectedPanelSettings.Painters[k];
                        var actualPainter = actualPanelSettings.Painters[k];
                        actualPainter.PainterType.Should().Be.EqualTo(expectedPainter.PainterType);
                        actualPainter.FieldMappings.Count.Should().Be(expectedPainter.FieldMappings.Count);
                        for (int l = 0; l < actualPainter.FieldMappings.Count; l++)
                        {
                            var expectedMapping = expectedPainter.FieldMappings[l];
                            var actualMapping = actualPainter.FieldMappings[l];
                            actualMapping.AccessorItemFieldName.Should().Be.EqualTo(expectedMapping.AccessorItemFieldName);
                            actualMapping.FileItemFieldName.Should().Be.EqualTo(expectedMapping.FileItemFieldName);
                        }

                        if (expectedPanelSettings.SelectedPainter == expectedPainter)
                        {
                            actualPanelSettings.SelectedPainter.Should().Be.SameInstanceAs(actualPainter);
                        }
                    }
                }
            }
        }

        public struct UnknownEvent
        {
            public double Field1;
            public double Field2;
        }
        public struct CustomOHLCVI
        {
            public int _Open;
            public int _High;
            public int Low;
            public int close;
            public long _volume;
            private short INTEREST;
        }
        public struct CustomValue
        {
            public decimal Banana;
            public float _value;
        }
        public struct UnitTestEvent
        {
            public double Field1;
            public double Field2;
        }
    }
}