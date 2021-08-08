using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using TeaTime.API;
using TeaTime.Chart.Core;
using TeaTime.Chart.Painters;

namespace TeaTime.Chart.Settings
{
    partial class ItemTypeSettingsControl : Grid
    {
        #region commands
        public static RoutedCommand AddPanelSettingsCommand = new RoutedCommand();
        public static RoutedCommand RemovePanelSettingsCommand = new RoutedCommand();
        public static RoutedCommand MovePanelSettingsUpCommand = new RoutedCommand();
        public static RoutedCommand MovePanelSettingsDownCommand = new RoutedCommand();
        public static RoutedCommand AddPainterMappingCommand = new RoutedCommand();
        public static RoutedCommand RemovePainterMappingCommand = new RoutedCommand();
        public static RoutedCommand MovePainterMappingUpCommand = new RoutedCommand();
        public static RoutedCommand MovePainterMappingDownCommand = new RoutedCommand();

        private void AddPanelSettingsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ItemTypeSettings != null &&
                           ItemTypeSettings.SelectedChartSettings != null &&
                           ItemTypeSettings.SelectedChartSettings.PanelSettings.Count < 10;
        }
        private void UpdatePanelSettingsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ItemTypeSettings != null &&
                           ItemTypeSettings.SelectedChartSettings != null &&
                           ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings != null;
        }
        private void MovePanelSettingsUpCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ItemTypeSettings != null &&
                            ItemTypeSettings.SelectedChartSettings != null &&
                            ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings != null && 
                            ItemTypeSettings.SelectedChartSettings.PanelSettings.IndexOf(ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings) > 0;
        }
        private void MovePanelSettingsDownCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ItemTypeSettings != null && 
                            ItemTypeSettings.SelectedChartSettings != null &&
                            ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings != null &&
                            ItemTypeSettings.SelectedChartSettings.PanelSettings.IndexOf(ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings) < ItemTypeSettings.SelectedChartSettings.PanelSettings.Count - 1;
        }
        private void RemovePainterMappingCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ItemTypeSettings != null &&
                           ItemTypeSettings.SelectedChartSettings != null &&
                           ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings != null &&
                           ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.SelectedPainter != null;
        }
        private void MovePainterMappingUpCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ItemTypeSettings != null &&
                            ItemTypeSettings.SelectedChartSettings != null &&
                            ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings != null &&
                            ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.SelectedPainter != null &&
                            ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.Painters.IndexOf(ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.SelectedPainter) > 0;
        }
        private void MovePainterMappingDownCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ItemTypeSettings != null &&
                            ItemTypeSettings.SelectedChartSettings != null &&
                            ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings != null &&
                            ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.SelectedPainter != null &&
                            ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.Painters.IndexOf(ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.SelectedPainter) < ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.Painters.Count - 1;
        }
 
        private void AddPanelSettingsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var ps = new PanelSettings(ItemTypeSettings.SelectedChartSettings.PanelSettings.Count);
            ItemTypeSettings.SelectedChartSettings.PanelSettings.Add(ps);
            ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings = ps;
            panelName.Focus();
            Keyboard.Focus(panelName);
            if (!String.IsNullOrEmpty(ps.Name))
            {
                panelName.SelectionStart = 0;
                panelName.SelectionLength = ps.Name.Length;
            }
        }
        private void RemovePanelSettingsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var index = ItemTypeSettings.SelectedChartSettings.PanelSettings.IndexOf(ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings);
            ItemTypeSettings.SelectedChartSettings.PanelSettings.Remove(ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings);
            if (ItemTypeSettings.SelectedChartSettings.PanelSettings.Any())
            {
                ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings = ItemTypeSettings.SelectedChartSettings.PanelSettings[Math.Min(ItemTypeSettings.SelectedChartSettings.PanelSettings.Count - 1, index)];
            }
            else
            {
                ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings = null;
            }
        }
        private void MovePanelSettingsUpExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var selected = ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings;
            var index = ItemTypeSettings.SelectedChartSettings.PanelSettings.IndexOf(selected);
            ItemTypeSettings.SelectedChartSettings.PanelSettings.Remove(selected);
            ItemTypeSettings.SelectedChartSettings.PanelSettings.Insert(index - 1, selected);
            UpdatePanelSettingsIndices();
            ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings = selected;
        }
        private void MovePanelSettingsDownExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var selected = ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings;
            var index = ItemTypeSettings.SelectedChartSettings.PanelSettings.IndexOf(selected);
            ItemTypeSettings.SelectedChartSettings.PanelSettings.Remove(selected);
            ItemTypeSettings.SelectedChartSettings.PanelSettings.Insert(index + 1, selected);
            UpdatePanelSettingsIndices();
            ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings = selected;
        }
        private void UpdatePanelSettingsIndices()
        {
            for (int i = 0; i < ItemTypeSettings.SelectedChartSettings.PanelSettings.Count; i++)
            {
                ItemTypeSettings.SelectedChartSettings.PanelSettings[i].Index = i;
            }
        }
        private void AddPainterMappingExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var pd = registeredPainters.First();
            var pm = SettingsFactory.CreateDefaultPainterMapping(pd.Painter.GetType(), pd.Painter.ItemType, ItemTypeSettings.TsItemType);
            ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.Painters.Add(pm);
            ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.SelectedPainter = pm;
            painter.Focus();
            Keyboard.Focus(painter);
        }
        private void RemovePainterMappingExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var index = ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.Painters.IndexOf(ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.SelectedPainter);
            ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.Painters.Remove(ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.SelectedPainter);
            if (ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.Painters.Any())
            {
                ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.SelectedPainter = ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.Painters[Math.Min(ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.Painters.Count - 1, index)];
            }
            else
            {
                ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.SelectedPainter = null;
            }
        }
        private void MovePainterMappingUpExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var selected = ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.SelectedPainter;
            var index = ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.Painters.IndexOf(selected);
            ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.Painters.Remove(selected);
            ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.Painters.Insert(index - 1, selected);
            ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.SelectedPainter = selected;
        }
        private void MovePainterMappingDownExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var selected = ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.SelectedPainter;
            var index = ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.Painters.IndexOf(selected);
            ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.Painters.Remove(selected);
            ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.Painters.Insert(index + 1, selected);
            ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.SelectedPainter = selected;
        }
  
        #endregion

        #region fields and properties

        bool suppressPainterSelectionChanged;
        ItemTypeSettings ItemTypeSettings { get { return DataContext as ItemTypeSettings; } }

        readonly ObservableCollection<PainterDefinition> registeredPainters = new ObservableCollection<PainterDefinition>();
        public ObservableCollection<PainterDefinition> RegisteredPainters { get { return registeredPainters; } }
        
        readonly ObservableCollection<string> availableFileItemFields = new ObservableCollection<string>();
        public ObservableCollection<string> AvailableFileItemFields { get { return availableFileItemFields; } }

        readonly ObservableCollection<YScaleMode> yScaleModes = new ObservableCollection<YScaleMode> { YScaleMode.Linear, YScaleMode.Logarithmic };
        public ObservableCollection<YScaleMode> YScaleModes { get { return yScaleModes; } }

        #endregion

        #region ctor
        public ItemTypeSettingsControl()
        {
            InitializeComponent();
        }
        #endregion
        
        #region show/close
        public void Show(Type tsItemType)
        {
            if (Visibility != Visibility.Visible)
            {
                registeredPainters.Clear();
                registeredPainters.Add(PainterManager.Instance.RegisteredPainters.Select(pa => new PainterDefinition(pa.DefaultInstance)).OrderBy(pd => pd.Painter.Order));
                
                availableFileItemFields.Clear();
                availableFileItemFields.Add(tsItemType.GetAllInstanceFields().Select(fi => fi.Name));

                title.Text = String.Join(" - ", availableFileItemFields);

                this.DataContext = ItemTypeSettings.Read(tsItemType);

                Visibility = Visibility.Visible;

                this.Focus();
            }
        }
        public void Close()
        {
            this.DataContext = null;
            errorMessage.Visibility = Visibility.Collapsed;
            Visibility = Visibility.Collapsed;
        }
        #endregion

        #region eventhandler
        public delegate void ItemTypeSettingsChangedHandler(object sender, EventArgs<ItemTypeSettings> args);
        public static event ItemTypeSettingsChangedHandler ItemTypeSettingsChanged;

        void PanelSettings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemTypeSettings == null ||
                ItemTypeSettings.SelectedChartSettings == null || 
                ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings == null)
            {
                color.SelectedColor = Colors.Black;
            }
            else
            {
                color.SelectedColor = ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.DrawingAttributes.Color;
            }
        }
        void PainterMappings_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            suppressPainterSelectionChanged = true;
            try
            {
                if (ItemTypeSettings == null ||
                    ItemTypeSettings.SelectedChartSettings == null ||
                    ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings == null ||
                    ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.SelectedPainter == null)
                {
                    painter.SelectedItem = null;
                }
                else
                {
                    painter.SelectedItem = registeredPainters.FirstOrDefault(p => ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.SelectedPainter.PainterType.Equals(p.Painter.GetType()));
                }
            }
            finally
            {
                suppressPainterSelectionChanged = false;
            }
        }
        void Painter_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!suppressPainterSelectionChanged)
            {
                var pd = (PainterDefinition)painter.SelectedItem;
                if (pd != null &&
                    ItemTypeSettings != null &&
                    ItemTypeSettings.SelectedChartSettings != null &&
                    ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings != null &&
                    ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.SelectedPainter != null)
                {
                    var pm = SettingsFactory.CreateDefaultPainterMapping(pd.Painter.GetType(), pd.Painter.ItemType, ItemTypeSettings.TsItemType);
                    ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.SelectedPainter.PainterType = pm.PainterType;
                    ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.SelectedPainter.FieldMappings.Remove(fm => pm.FieldMappings.All(pfm => pfm.AccessorItemFieldName != fm.AccessorItemFieldName));
                    pm.FieldMappings.ForEach(pfm =>
                        {
                            if (ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.SelectedPainter.FieldMappings.All(fm => fm.AccessorItemFieldName != pfm.AccessorItemFieldName))
                                ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.SelectedPainter.FieldMappings.Add(pfm);
                        });
                }
            }
        }
        private void Color_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            if (ItemTypeSettings != null &&
                ItemTypeSettings.SelectedChartSettings != null &&
                ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings != null)
            {
                ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings.DrawingAttributes.Color = e.NewValue;
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ItemTypeSettings.Store();
                if (ItemTypeSettingsChanged != null)
                {
                    ItemTypeSettingsChanged(this, new EventArgs<ItemTypeSettings>(ItemTypeSettings));
                }
                color.StoreRecentColors();
                Close();
            }
            catch (ItemTypeSettings.ValidationException ex)
            {
                if (ex.ChartSettings != null)
                {
                    ItemTypeSettings.SelectedChartSettings = ex.ChartSettings;
                    if (ex.PanelSettings != null)
                    {
                        ItemTypeSettings.SelectedChartSettings.SelectedPanelSettings = ex.PanelSettings;
                    }
                }
                if (ex.PainterMapping != null)
                {
                    painterMappings.SelectedItem = ex.PainterMapping;
                }

                errorMessage.Text = ex.Message;
                errorMessage.Visibility = Visibility.Visible;
            }
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        #region embedded type
        public class PainterDefinition : NotifyPropertyChanged
        {
            private readonly IPainter painter;
            public IPainter Painter { get { return painter; } }

            public PainterDefinition(IPainter painter)
            {
                this.painter = painter;
            }

            public override string ToString()
            {
                return painter.GetType().Name;
            }
        }
        #endregion
    }
}