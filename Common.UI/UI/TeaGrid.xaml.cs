// copyright discretelogics 2012.

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.VisualStudio.Shell;
using TeaTime.Data;

namespace TeaTime.UI
{
    public partial class TeaGrid : UserControl
    {
        #region life

        public TeaGrid()
        {
            InitializeComponent();
            InitializeStyles();

            visibleRange = RangeL.Empty;
            scrollbar.ValueChanged += (s, e) =>
                {
                    if (!suppressValueChanged)
                        UpdateVisibleRange();
                };
            textblockContainer.SizeChanged += (s, e) => Configure();
        }
        private void InitializeStyles()
        {
            if (!areStylesInitialized)
            {
                selectedHighlightBrush = ((Brush)FindResource(VsBrushes.HighlightKey)).Clone();
                selectedBrush = selectedHighlightBrush.Clone();
                selectedBrush.Opacity = 0.5;
                deselectedBrush = Brushes.Transparent;

                areStylesInitialized = true;
            }
        }

        public void Initialize(ITeaFileEditor teaFileEditor)
        {
            Guard.ArgumentNotNull(teaFileEditor, "teaFileEditor");

            this.teaFileEditor = teaFileEditor;
        }

        #endregion

        #region state

        public static readonly DependencyProperty IsStoppedProperty = DependencyProperty<TeaGrid, bool>.Register("IsStopped", false);
        public bool IsStopped
        {
            get { return (bool)GetValue(IsStoppedProperty); }
            set { SetValue(IsStoppedProperty, value); }
        }

        private static bool areStylesInitialized;
        private static Brush selectedBrush;
        private static Brush selectedHighlightBrush;
        private static Brush deselectedBrush;

        ITeaFileEditor teaFileEditor;
        ITeaFile teaFile;
        public ITeaFile TeaFile
        {
            get { return this.teaFile; }
            set
            {
                var safeTeaFile = new SafeTeaFileAccessor(value);
                safeTeaFile.DataAccessFailed += (sender, exception) => IsStopped = true;

                this.visibleRange = RangeL.Empty;
                this.teaFile = safeTeaFile;
                this.itemSource = this.teaFile.ItemLines;
                this.itemsCount = this.teaFile.Count;
                this.selectedIndex = this.itemsCount - 1;
                this.indexColumnWidth = this.itemsCount.ToString().Length + 2;
                this.header.Text = "".Blanked(this.indexColumnWidth) + "  " + this.itemSource.GetHeader();
                Configure();
            }
        }

        private IItemLines itemSource;
        private long itemsCount;

        private RangeL visibleRange;
        private long selectedIndex;
        public long SelectedIndex
        {
            get { return selectedIndex; }
            set
            {
                if (selectedIndex != value)
                {
                    selectedIndex = value;
                    if (!ScrollToSelectedIndex())
                    {
                        UpdateVisibleRange();
                    }
                }
            }
        }

        bool suppressValueChanged;
        private int indexColumnWidth;
        private TextBlock selectedTextBlock;

        private UIElementCollection TextBlocks { get { return panel.Children; } }

        #endregion

        #region int

        private void Configure()
        {
            try
            {
                if (!itemSource.IsSet() || (ActualHeight == 0))
                {
                    TextBlocks.Clear();
                    scrollbar.Maximum = 0;
                    return;
                }

                var n = (int)Math.Floor(textblockContainer.ActualHeight / (header.ActualHeight + header.Margin.Top + header.Margin.Bottom));
                while (TextBlocks.Count > n)
                {
                    var textBlock = TextBlocks[TextBlocks.Count - 1];
                    textBlock.MouseEnter -= textBlock_MouseEnter;
                    textBlock.MouseLeftButtonDown -= textBlock_MouseLeftButtonDown;
                    TextBlocks.Remove(textBlock);
                }
                while (n > TextBlocks.Count)
                {
                    var textBlock = new TextBlock();
                    textBlock.Background = deselectedBrush;
                    textBlock.MouseEnter += textBlock_MouseEnter;
                    textBlock.MouseLeftButtonDown += textBlock_MouseLeftButtonDown;
                    TextBlocks.Add(textBlock);
                }

                scrollbar.Maximum = itemsCount - TextBlocks.Count;
            }
            finally
            {
                suppressValueChanged = false;
            }

            if (!ScrollToSelectedIndex())
            {
                UpdateVisibleRange();
            }
        }
        
        private void UpdateVisibleRange()
        {
            if (TextBlocks.Count > 0)
            {
                var start = (long) scrollbar.Value;
                visibleRange = new RangeL(start, Math.Min(itemsCount - 1, start + TextBlocks.Count - 1));
            }
            else
            {
                visibleRange = RangeL.Empty;
            }
            UpdateText();

            if (!visibleRange.IsEmpty && !visibleRange.Contains(selectedIndex))
            {
                selectedIndex = visibleRange.EnsureContained(selectedIndex);
                SelectedIndexUpdated();
            }
            SelectTextBlock();
        }

        private void UpdateText()
        {
            if (visibleRange.IsEmpty || IsStopped) return;

            for (var i = visibleRange.Start; i <= visibleRange.End; i++)
            {
                var tb = (TextBlock)TextBlocks[(int)(i - visibleRange.Start)];
                tb.Text = (i + 1).ToString().Blanked(indexColumnWidth) + "  " + itemSource.GetLineText(i);
            }
        }

        private bool ScrollToSelectedIndex()
        {
            long value = Math.Max(0, Math.Min((long) scrollbar.Maximum, selectedIndex - TextBlocks.Count + 1));
            if (value != scrollbar.Value)
            {
                scrollbar.Value = value;
                return true;
            }
            return false;
        }

        private void SelectTextBlock()
        {
            if (visibleRange.Contains(selectedIndex))
            {
                var textBlock = (TextBlock)TextBlocks[(int)(selectedIndex - visibleRange.Start)];
                if (textBlock != selectedTextBlock)
                {
                    // deselect old
                    if (selectedTextBlock != null)
                    {
                        selectedTextBlock.Background = deselectedBrush;
                    }

                    // select new
                    selectedTextBlock = textBlock;
                    selectedTextBlock.Background = selectedBrush;
                }
            }
        }
        private void SelectAndHighlight(TextBlock textBlock)
        {
            var index = (long)scrollbar.Value + TextBlocks.IndexOf(textBlock);
            if (visibleRange.Contains(index))
            {
                selectedIndex = index;
                SelectedIndexUpdated();

                if (textBlock != selectedTextBlock)
                {
                    if (selectedTextBlock != null)
                    {
                        selectedTextBlock.Background = deselectedBrush;
                    }
                    selectedTextBlock = textBlock;
                }
            }
            if (selectedTextBlock != null)
            {
                selectedTextBlock.Background = selectedHighlightBrush;
            }
        }
        private void UnhighlightSelected()
        {
            if (selectedTextBlock != null)
            {
                selectedTextBlock.Background = selectedBrush;
            }
        }

        private void SelectedIndexUpdated()
        {
            if (teaFileEditor != null)
            {
                teaFileEditor.SetTeaFileIndex(this, selectedIndex);
            }
        }

        #endregion

        #region eventhandler
        private void TeaGrid_OnGotFocus(object sender, RoutedEventArgs e)
        {
            scrollbar.Focus();
            Keyboard.Focus(scrollbar);
        }

        private void TeaGrid_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (IsStopped) return;

            scrollbar.Value = Math.Min(scrollbar.Maximum, Math.Max(scrollbar.Minimum, scrollbar.Value - e.Delta / 10.0));
            e.Handled = true;
        }

        private void Scrollbar_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (IsStopped) return;

            if (e.Key == Key.End)
            {
                SelectedIndex = Math.Max(0, itemsCount - 1);
                e.Handled = true;
            }
            else if (e.Key == Key.Home)
            {
                SelectedIndex = 0;
                e.Handled = true;
            }
        }

        private void textBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectAndHighlight((TextBlock)sender);
        }
        private void textBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                SelectAndHighlight((TextBlock) sender);
            }
        }

        private void TextblockContainer_OnMouseLeave(object sender, MouseEventArgs e)
        {
            UnhighlightSelected();
        }
        private void TextblockContainer_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            UnhighlightSelected();
        }
        #endregion
    }
}