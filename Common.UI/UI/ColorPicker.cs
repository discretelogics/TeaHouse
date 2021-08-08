using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TeaTime.UI
{
    [TemplatePart(Name = PART_AvailableColors, Type = typeof(ListBox))]
    [TemplatePart(Name = PART_StandardColors, Type = typeof(ListBox))]
    [TemplatePart(Name = PART_RecentColors, Type = typeof(ListBox))]
    public class ColorPicker : Control
    {
        private const string PART_AvailableColors = "PART_AvailableColors";
        private const string PART_StandardColors = "PART_StandardColors";
        private const string PART_RecentColors = "PART_RecentColors";

        #region Members

        private ListBox _availableColors;
        private ListBox _standardColors;
        private ListBox _recentColors;

        #endregion

        #region Properties

        public static readonly DependencyProperty AvailableColorsProperty = DependencyProperty<ColorPicker, ObservableCollection<Color>>.Register("AvailableColors", CreateAvailableColors());
        public ObservableCollection<Color> AvailableColors
        {
            get
            {
                return (ObservableCollection<Color>)GetValue(AvailableColorsProperty);
            }
            set
            {
                SetValue(AvailableColorsProperty, value);
            }
        }

        public static readonly DependencyProperty AvailableColorsHeaderProperty = DependencyProperty<ColorPicker, string>.Register("AvailableColorsHeader", "Available Colors");
        public string AvailableColorsHeader
        {
            get
            {
                return (string)GetValue(AvailableColorsHeaderProperty);
            }
            set
            {
                SetValue(AvailableColorsHeaderProperty, value);
            }
        }

        public static readonly DependencyProperty IsOpenProperty = DependencyProperty<ColorPicker, bool>.Register("IsOpen", false);
        public bool IsOpen
        {
            get
            {
                return (bool)GetValue(IsOpenProperty);
            }
            set
            {
                SetValue(IsOpenProperty, value);
            }
        }

        public static readonly DependencyProperty RecentColorsProperty = DependencyProperty<ColorPicker, ObservableMruCollection<Color>>.Register("RecentColors", null);
        public ObservableMruCollection<Color> RecentColors
        {
            get
            {
                return (ObservableMruCollection<Color>)GetValue(RecentColorsProperty);
            }
            set
            {
                SetValue(RecentColorsProperty, value);
            }
        }

        public static readonly DependencyProperty RecentColorsHeaderProperty = DependencyProperty<ColorPicker, string>.Register("RecentColorsHeader", "Recent Colors");
        public string RecentColorsHeader
        {
            get
            {
                return (string)GetValue(RecentColorsHeaderProperty);
            }
            set
            {
                SetValue(RecentColorsHeaderProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty<ColorPicker, Color>.Register("SelectedColor", Colors.Black, OnSelectedColorPropertyChanged);
        public Color SelectedColor
        {
            get
            {
                return (Color)GetValue(SelectedColorProperty);
            }
            set
            {
                SetValue(SelectedColorProperty, value);
            }
        }

        private static void OnSelectedColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ColorPicker colorPicker = (ColorPicker)d;
            if (colorPicker != null)
                colorPicker.OnSelectedColorChanged((Color)e.OldValue, (Color)e.NewValue);
        }

        private void OnSelectedColorChanged(Color oldValue, Color newValue)
        {
            RoutedPropertyChangedEventArgs<Color> args = new RoutedPropertyChangedEventArgs<Color>(oldValue, newValue);
            args.RoutedEvent = ColorPicker.SelectedColorChangedEvent;
            RaiseEvent(args);
        }

        public static readonly DependencyProperty ShowAvailableColorsProperty = DependencyProperty<ColorPicker, bool>.Register("ShowAvailableColors", true);
        public bool ShowAvailableColors
        {
            get
            {
                return (bool)GetValue(ShowAvailableColorsProperty);
            }
            set
            {
                SetValue(ShowAvailableColorsProperty, value);
            }
        }

        public static readonly DependencyProperty ShowRecentColorsProperty = DependencyProperty<ColorPicker, bool>.Register("ShowRecentColors", true);
        public bool ShowRecentColors
        {
            get
            {
                return (bool)GetValue(ShowRecentColorsProperty);
            }
            set
            {
                SetValue(ShowRecentColorsProperty, value);
            }
        }

        public static readonly DependencyProperty ShowStandardColorsProperty = DependencyProperty<ColorPicker, bool>.Register("ShowStandardColors", true);
        public bool ShowStandardColors
        {
            get
            {
                return (bool)GetValue(ShowStandardColorsProperty);
            }
            set
            {
                SetValue(ShowStandardColorsProperty, value);
            }
        }

        public static readonly DependencyProperty StandardColorsProperty = DependencyProperty<ColorPicker, ObservableCollection<Color>>.Register("StandardColors", CreateStandardColors());
        public ObservableCollection<Color> StandardColors
        {
            get
            {
                return (ObservableCollection<Color>)GetValue(StandardColorsProperty);
            }
            set
            {
                SetValue(StandardColorsProperty, value);
            }
        }

        public static readonly DependencyProperty StandardColorsHeaderProperty = DependencyProperty<ColorPicker, string>.Register("StandardColorsHeader", "Standard Colors");
        public string StandardColorsHeader
        {
            get
            {
                return (string)GetValue(StandardColorsHeaderProperty);
            }
            set
            {
                SetValue(StandardColorsHeaderProperty, value);
            }
        }

        #endregion

        #region Constructors

        static ColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
        }

        public ColorPicker()
        {
            RecentColors = SettingsManager.Instance.Read<ObservableMruCollection<Color>>("Chart", "RecentColors", () => new ObservableMruCollection<Color>());
            RecentColors.MaxCount = 20;

            Keyboard.AddKeyDownHandler(this, OnKeyDown);
            Mouse.AddPreviewMouseDownOutsideCapturedElementHandler(this, OnMouseDownOutsideCapturedElement);
        }

        #endregion

        #region Base Class Overrides

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_availableColors != null)
                _availableColors.SelectionChanged -= Color_SelectionChanged;

            _availableColors = GetTemplateChild(PART_AvailableColors) as ListBox;
            if (_availableColors != null)
                _availableColors.SelectionChanged += Color_SelectionChanged;

            if (_standardColors != null)
                _standardColors.SelectionChanged -= Color_SelectionChanged;

            _standardColors = GetTemplateChild(PART_StandardColors) as ListBox;
            if (_standardColors != null)
                _standardColors.SelectionChanged += Color_SelectionChanged;

            if (_recentColors != null)
                _recentColors.SelectionChanged -= Color_SelectionChanged;

            _recentColors = GetTemplateChild(PART_RecentColors) as ListBox;
            if (_recentColors != null)
                _recentColors.SelectionChanged += Color_SelectionChanged;
        }

        #endregion

        #region Event Handlers

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                case Key.Tab:
                    {
                        CloseColorPicker();
                        break;
                    }
            }
        }

        private void OnMouseDownOutsideCapturedElement(object sender, MouseButtonEventArgs e)
        {
            CloseColorPicker();
        }

        private void Color_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = (ListBox)sender;

            if (e.AddedItems.Count > 0)
            {
                var color = (Color)e.AddedItems[0];
                SelectedColor = color;
                RecentColors.Add(color);
                CloseColorPicker();
                lb.SelectedIndex = -1;
            }
        }

        #endregion

        #region Events

        public static readonly RoutedEvent SelectedColorChangedEvent = EventManager.RegisterRoutedEvent("SelectedColorChanged", RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<Color>), typeof(ColorPicker));
        public event RoutedPropertyChangedEventHandler<Color> SelectedColorChanged
        {
            add
            {
                AddHandler(SelectedColorChangedEvent, value);
            }
            remove
            {
                RemoveHandler(SelectedColorChangedEvent, value);
            }
        }

        #endregion

        #region Methods

        private void CloseColorPicker()
        {
            if (IsOpen)
                IsOpen = false;
            ReleaseMouseCapture();
        }

        private static ObservableCollection<Color> CreateStandardColors()
        {
            var standardColors = new ObservableCollection<Color>();
            standardColors.Add(Colors.Black);
            standardColors.Add(Colors.Gray);
            standardColors.Add(Colors.DarkBlue);
            standardColors.Add(Colors.Blue);
            standardColors.Add(Colors.LightBlue);
            standardColors.Add(Colors.DarkRed);
            standardColors.Add(Colors.Red);
            standardColors.Add(Colors.Orange);
            standardColors.Add(Colors.Green);
            standardColors.Add(Colors.LightGreen);
            return standardColors;
        }
        private static ObservableCollection<Color> CreateAvailableColors()
        {
            var baseColors = new[] { 
                Color.FromRgb(80, 80, 80),
                Color.FromRgb(178, 178, 178),
                Color.FromRgb(225, 225, 225),
                Color.FromRgb(61, 76, 95),
                Color.FromRgb(56, 101, 180),
                Color.FromRgb(54, 140, 218), 
                Color.FromRgb(236, 20, 20),
                Color.FromRgb(236, 108, 20),
                Color.FromRgb(229, 173, 0),
                Color.FromRgb(99, 155, 63)
            };

            var availableColors = new ObservableCollection<Color>();

            for (int i = 0; i < 4; i++)
            {
                foreach (var baseColor in baseColors)
                {
                    double factor = 1 - 0.2 * i;
                    availableColors.Add(Color.FromRgb((byte)(baseColor.R * factor), (byte)(baseColor.G * factor), (byte)(baseColor.B * factor)));
                }
            }

            return availableColors;
        }

        public void StoreRecentColors()
        {
            SettingsManager.Instance.Store("Chart", "RecentColors", RecentColors);
        }

        #endregion
    }
}
