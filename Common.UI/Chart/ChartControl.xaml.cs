// copyright discretelogics © 2011
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using NLog;
using TeaTime.Chart.Core;
using TeaTime.Chart.Markers;
using TeaTime.Chart.Painters;
using TeaTime.Data;
using TeaTime.UI;
using TeaTime.Chart.Settings;
using TeaTime.API;

namespace TeaTime.Chart
{
    /// <summary>
    /// The complete chart. time series are drawn on chart panels.
    /// </summary>
    public partial class ChartControl : ConstantFrameControl, ISelectable, IDisposable
    {
        static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region commands

        public static RoutedCommand TsSettingsCommand = new RoutedCommand();
        private void TsSettingsCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = HasValidState && 
                ((SelectedTss.Select(ts => ts.ItemType).Distinct().Count() == 1) ||
                (tapeView.Tss.Select(ts => ts.ItemType).Distinct().Count() == 1));
        }
        private void TsSettingsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var ts = SelectedTss.Any() ? SelectedTss.First() : tapeView.Tss.First();
            itemTypeSettingsControl.Show(ts.ItemType);
        }

        #endregion

        #region properties

        public static readonly DependencyProperty IsSingleTsProperty = DependencyProperty<ChartControl, bool>.Register("IsSingleTs", false);
        public static readonly DependencyProperty IsFixedLayoutProperty = DependencyProperty<ChartControl, bool>.Register("IsFixedLayout", false);
        public static readonly DependencyProperty IsStoppedProperty = DependencyProperty<ChartControl, bool>.Register("IsStopped", false);

        /// <summary>
        ///     Determines if the chart can display mutliple TimeSeriess or not.
        /// </summary>
        public bool IsSingleTs
        {
            get { return (bool)GetValue(IsSingleTsProperty); }
            set { SetValue(IsSingleTsProperty, value); }
        }

        /// <summary>
        ///     Determines wether the chart can be scrolled or not.
        /// </summary>
        /// <remarks>
        ///     If true, the chart can't be scrolled and will always disply the complete Tape.
        /// </remarks>
        public bool IsFixedLayout
        {
            get { return (bool) GetValue(IsFixedLayoutProperty); }
            set { SetValue(IsFixedLayoutProperty, value); }
        }

        public bool IsStopped
        {
            get { return (bool)GetValue(IsStoppedProperty); }
            set { SetValue(IsStoppedProperty, value); }
        }

        public new bool IsInitialized
        {
            get { return isInitialized; }
        }

        public ITapeView TapeView
        {
            get { return tapeView; }
        }

        internal RangeL TapeRange
        {
            get { return tapeRange; }
        }

        internal int TimeMarkerCount
        {
            get { return timeMarkers.Count; }
        }

        public IEnumerable<TsVisualization> Tvs
        {
            get
            {
                var tvs = new List<TsVisualization>();
                ForEachPanel(p => tvs.AddRange(p.Tvs));
                return tvs;
            }
        }

        private IEnumerable<ITeaFile> SelectedTss
        {
            get
            {
                IEnumerable<ITeaFile> selected = new List<ITeaFile>();
                ForEachPanel(p => selected = selected.Union(p.SelectedTss));
                return selected;
            }
        }

        public IEnumerable<TsVisualization> SelectedTvs
        {
            get
            {
                var selected = new List<TsVisualization>();
                ForEachPanel(p => selected.AddRange(p.SelectedTvs));
                return selected;
            }
        }

        private bool HasValidState
        {
            get { return isInitialized && !isDisposed && !IsStopped; }
        }
        
        #endregion

        #region ctor, Initialize, Dispose

        public ChartControl()
        {
            InitializeComponent();
            InitializeStyles();

            childPanels = new List<ChartPanelControls>();
            tapeRange = RangeL.Empty;
            timeMode = DateTimeMode.TimeAndDate;
            movingTimes = new SortedList<DateTime, double>();

            timeMarkers = new List<Marker>();
            dateMarkers = new List<DateMarker>();
            cursorMarkers = new Dictionary<ICursor, Marker>();
            selectionPainterButtons = new List<ToggleButton>();
            chartSettingsToolbarItems = new List<object>();
            panelSettingsPainterButtons = new Dictionary<PanelSettings, List<ToggleButton>>();

            ItemTypeSettingsControl.ItemTypeSettingsChanged += this.ItemTypeSettingsControl_ItemTypeSettingsChanged;
        }

        private void InitializeStyles()
        {
            if (!areStylesInitialized)
            {
                toolbarSeparatorStyle = (Style)FindResource("ToolBarSeparator");
                toolbarTextBlockStyle = (Style)FindResource("ToolBarTextBlock");
                toolbarButtonStyle = (Style)FindResource("ToolBarButton");
                toolbarImageStyle = (Style)FindResource("ToolBarImage");
                panelSplitterStyle = (Style)FindResource("PanelSplitter");

                areStylesInitialized = true;
            }
        }

        public void Initialize(ITeaFileEditor selectionService)
        {
            if (!isInitialized)
            {
                this.editor = selectionService;

                if (IsSingleTs)
                {
                    tapeView = new SimpleTapeView();
                }
                else
                {
                    tapeView = new TapeView();
                }
                tapeView.LengthChanged += (s, e) => UpdateScrollBar(e.Value);
                tapeView.ScaleFactorChanged += (s, e) =>
                                                {
                                                    if (!isScaleFactorChangedIgnored)
                                                    {
                                                        UpdateScrollBar(tapeView.Length);
                                                    }
                                                };

                ForEachPanel(p => p.Initialize(this));

                isInitialized = true;
            }
            else
            {
                throw new InvalidOperationException("This Chart was already initialized.");
            }
        }

        public override void Dispose()
        {
            if (!isDisposed)
            {
                Clear();

                base.Dispose();

                isDisposed = true;
            }
        }

        #endregion

        #region public methods

        public void SetTapeIndex(long tapeIndex)
        {
            this.GuardInitialized();

            double targetPosition = tapeView.PositionAt(tapeIndex);
            tapeScrollBar.Value = Math.Max(tapeScrollBar.Minimum, targetPosition);

            if (selectedItemTapeIndex != tapeIndex)   // in case the scrollbar value was not changed
            {
                double width = tapeView.WidthAt(tapeIndex);
                double left = content.ActualWidth - (tapeView.PositionAt(tapeRange.Start) + width) + targetPosition;
                this.ItemSelected(tapeIndex, left, width);
            }
        }

        public void Add(params ITeaFile[] tss)
        {
            Guard.ArgumentNotNull(tss, "tss");
            this.GuardInitialized();

            var safeTss = tss.Select(ts =>
                {
                    var safeTs = new SafeTeaFileAccessor(ts);
                    safeTs.DataAccessFailed += (sender, exception) => this.IsStopped = true;
                    return (ITeaFile)safeTs;
                }).ToArray();

            tapeView.Add(safeTss);
            if (IsSingleTs)
            {
                var settings = ItemTypeSettings.Read(safeTss.First().ItemType);
                if (settings.IsProposal)
                {
                    itemTypeSettingsControl.Show(safeTss.First().ItemType);
                }
                else
                {
                    UpdateItemTypeSettings(settings);
                }
            }
            else
            {
                // TODO: Implement
                throw new NotImplementedException("Multiple TimeSeries support is not yet implemented");
            }
        }

        public void Clear()
        {
            if (isInitialized && IsSingleTs)
            {
                UpdateItemTypeSettings(null);
            }
        
            childPanels.ToList().ForEach(RemovePanel);

            #region destroy TimeAxis

            var markers = new List<Marker>(timeMarkers);
            foreach (Marker marker in markers)
            {
                RemoveTimeMarker(marker);
            }
            var dms = new List<DateMarker>(dateMarkers);
            foreach (DateMarker dm in dms)
            {
                RemoveDateMarker(dm);
            }

            #endregion

            cursorMarkers.Values.ForEach(cm => cm.Dispose());
            cursorMarkers.Clear();

            if (tapeView != null)
            {
                tapeView.Remove(tapeView.Tss.ToArray());
            }

            tapeRange = RangeL.Empty;
            selectedItemTapeIndex = null;
            selectedItemTs = null;
        }

        #endregion

        #region ISelectable Members

        public void Select(object sender)
        {
            Marker marker = (Marker) sender;
            double position = marker.ActualPosition;
            int index = timeMarkers.IndexOf(marker);

            ForEachPanel(panel => panel.SelectTimeMarker(index, marker));

            foreach (DateMarker dm in dateMarkers)
            {
                if (dm.IsInRange(position))
                {
                    dm.IsSelected = true;
                }
            }
        }

        public void Deselect(object sender)
        {
            Marker marker = (Marker) sender;
            int index = timeMarkers.IndexOf(marker);

            ForEachPanel(panel => panel.DeselectTimeMarker(index, marker));

            foreach (DateMarker dm in dateMarkers)
            {
                dm.IsSelected = false;
            }
        }

        #endregion

        #region internal methods

        /// <summary>
        ///     Recalcs the Chart's own Margin in case a YScalePanel was removed or added on a ChartPanel.
        /// </summary>
        internal void RecalcMargin()
        {
            double left = 0;
            double right = 0;

            ForEachPanel(p =>
                            {
                                left = Math.Max(left, Math.Abs(p.Margin.Left));
                                right = Math.Max(right, Math.Abs(p.Margin.Right));
                            }
                );

            content.Margin = new Thickness(left, 0, right, 0);
        }

        internal void DragTimeSeries(Geometry pathGeometry, TsVisualization tv, ChartPanel sender)
        {
            if (!IsSingleTs)
            {
                this.GuardInitialized();

                draggingPathStart = Mouse.GetPosition(this);
                draggingYOffset = panelContainer.RowDefinitions[Grid.GetRow(sender)].Offset;

                // set 0-offset before setting Path.Data to prevent it from being drawn on old position
                Canvas.SetLeft(draggingPath, 0);
                Canvas.SetTop(draggingPath, draggingYOffset);

                draggingPath.Data = pathGeometry;
                draggingTv = tv;
                draggingSource = sender;
                isDragging = true;
            }
        }

        internal void Untape(IEnumerable<ITsVisualization> tvs)
        {
            if (!IsSingleTs)
            {
                tapeView.Remove(tvs.Select(tv => tv.TimeSeries).Where(ts => this.Tvs.All(tv => tv.TimeSeries != ts)).ToArray());
            }
        }

        #region TimeSeries-Selection

        internal void ClearTsSelection()
        {
            suppressSelectionChanged = true;
            ForEachPanel(p => p.ClearSelection());

            suppressSelectionChanged = false;
            TsSelectionChanged();
        }

        internal void TsSelectionChanged()
        {
            if (!IsSingleTs && !suppressSelectionChanged)
            {
                var selectedTss = SelectedTss;
                var selectedTvs = SelectedTvs;
                bool isSelected = selectedTvs.Any();

                #region toolbar

                #region painter

                IEnumerable<PainterActivator> availablePainters = null;
                if (isSelected)
                {
                    selectedTss.ForEach(ts =>
                                            {
                                                var tsPainters = PainterManager.Instance.FindPaintersByItemType(ts.ItemType);
                                                if (availablePainters == null)
                                                {
                                                    availablePainters = tsPainters;
                                                }
                                                else
                                                {
                                                    availablePainters = availablePainters.Intersect(tsPainters);
                                                }
                                            }
                        );
                }
                else
                {
                    availablePainters = new List<PainterActivator>();
                }
                var existingPainters = new List<PainterActivator>();
                var obsoleteButtons = new List<ToggleButton>();
                foreach (ToggleButton painterButton in selectionPainterButtons)
                {
                    var p = (PainterActivator) painterButton.Tag;
                    if (availablePainters.Contains(p))
                    {
                        existingPainters.Add(p);
                    }
                    else
                    {
                        obsoleteButtons.Add(painterButton);
                    }
                }

                foreach (ToggleButton b in obsoleteButtons)
                {
                    b.Click -= painter_selection_Click;

                    selectionPainterButtons.Remove(b);
                    toolBar.Items.Remove(b);
                }

                foreach (PainterActivator p in availablePainters.Except(existingPainters))
                {
                    var painterImage = new Image();
                    painterImage.Source = p.DefaultInstance.Image ?? ResourcesUtility.GetImage("Painter.png");
                    painterImage.Style = toolbarImageStyle;

                    var painterButton = new ToggleButton();
                    painterButton.ToolTip = "Draw with " + p.DefaultInstance.Text;
                    painterButton.Style = toolbarButtonStyle;
                    painterButton.Content = painterImage;

                    painterButton.Tag = p;
                    painterButton.Click += painter_selection_Click;

                    selectionPainterButtons.Add(painterButton);
                    toolBar.Items.Insert(1, painterButton);
                }

                toolBar.InvalidateMeasure();

                UpdateSelectedPainterButton(selectedTvs);

                #endregion

                // TODO: YScaleMode
                
                #endregion
            }
        }

        internal void YScaleModeChanged(IEnumerable<TsVisualization> tvs)
        {
            if (!IsSingleTs)
            {
                Guard.ArgumentNotNull(tvs, "tvs");

                var selectedTvs = SelectedTvs;
                if (tvs.Intersect(selectedTvs).Any())
                {
                    // TODO: YScaleMode
                }
            }
        }

        #endregion

        #region Move/Zoom

        internal void MoveRange(double deltaPixel)
        {
            if (isInitialized)
            {
                double scrollValue = tapeScrollBar.Value + deltaPixel;
                if (scrollValue < tapeScrollBar.Minimum)
                {
                    scrollValue = tapeScrollBar.Minimum;
                }
                if (scrollValue > tapeScrollBar.Maximum)
                {
                    scrollValue = tapeScrollBar.Maximum;
                }

                tapeScrollBar.Value = scrollValue;
            }
        }

        internal void ZoomRangePosition(double startPosition, double endPosition)
        {
            if (isInitialized && (Math.Abs(endPosition - startPosition) > minXZoomDistance))
            {
                var actualStartIndex = tapeRange.Start;
                var actualEndIndex = tapeRange.End;
                double offset = content.ActualWidth - (tapeView.PositionAt(actualStartIndex) + tapeView.WidthAt(actualStartIndex));

                long endIndex = -1;
                long startIndex = -1;

                for (var i = actualStartIndex; i <= actualEndIndex; i++)
                {
                    double left = offset + tapeView.PositionAt(i);
                    double right = left + tapeView.WidthAt(i);

                    if ((startPosition >= left) && (startPosition < right))
                    {
                        endIndex = i;
                    }

                    if ((endPosition >= left) && (endPosition < right))
                    {
                        startIndex = i;
                    }

                    if ((endIndex >= 0) && (startIndex >= 0))
                    {
                        break;
                    }
                }

                if ((endIndex < 0) || (startIndex < 0) || (Math.Abs(startIndex - endIndex) <= 0))
                {
                    return;
                }

                long leftIndex;
                long rightIndex;
                if (endIndex < startIndex)
                {
                    // zoom in
                    leftIndex = endIndex;
                    rightIndex = startIndex;
                }
                else
                {
                    // zoom out
                    long delta = endIndex - startIndex; // will be added on both sides
                    leftIndex = tapeRange.End - delta;
                    rightIndex = tapeRange.Start + delta;
                    if (leftIndex < 0)
                    {
                        leftIndex = 0;
                    }
                    if (rightIndex >= tapeView.Length)
                    {
                        rightIndex = tapeView.Length - 1;
                    }
                }

                if ((rightIndex != tapeRange.Start) ||
                    (leftIndex != tapeRange.End))
                {
                    tapeRange.Start = rightIndex;
                    tapeRange.End = leftIndex;
                    if (tapeRange.Length > int.MaxValue)
                    {
                        tapeRange.End = tapeRange.Start + int.MaxValue - 1;
                    }
                    tapeView.ScaleRangeToWidth(tapeRange, content.ActualWidth);
                }
            }
        }

        #endregion

        #region CursorMarkers

        internal void UpdateCursorMarker(ChartPanel sender, ICursor cursor, double position)
        {
            if (tapeRange.IsEmpty)
            {
                cursorMarkers[cursor].Update(position, null);
            }
            else
            {
                double contentWidth = content.ActualWidth;
                double right = tapeView.PositionAt(tapeRange.Start) + tapeView.WidthAt(tapeRange.Start);
                double tapePosition = right - (contentWidth - position);
                if (tapePosition >= 0)
                {
                    var ti = tapeView.IndexAt(tapePosition);
                    string timeDisplayString;
                    if (timeMode == DateTimeMode.TimeAndDate)
                    {
                        timeDisplayString = tapeView.TimeAt(ti).ToLongTimeString();
                    }
                    else // show complete date for DayAndMonth & MonthAndYear
                    {
                        timeDisplayString = tapeView.TimeAt(ti).ToDateDisplayString();
                    }
                    cursorMarkers[cursor].Update(position, timeDisplayString);
                    double left = contentWidth - (right - tapeView.PositionAt(ti));
                    this.ItemSelected(ti, left, this.tapeView.WidthAt(ti));
                }

                foreach (DateMarker dm in dateMarkers)
                {
                    dm.IsSelected = dm.IsInRange(position);
                }
            }
        }

        internal void AddCursorMarker(ICursor cursor)
        {
            this.GuardInitialized();

            var marker = new Marker(timeMarkerContainer, null, MarkerAlignment.Top, false, false);
            marker.Background = timeMarkerContainer.Background;
            cursorMarkers.Add(cursor, marker);

            ForEachPanel(p => p.TimeSliceMarker.Select(this));
        }

        internal void RemoveCursorMarker(ICursor cursor)
        {
            this.GuardInitialized();

            cursorMarkers[cursor].Dispose();
            cursorMarkers.Remove(cursor);

            ForEachPanel(p => p.TimeSliceMarker.Deselect(this));

            foreach (DateMarker dm in dateMarkers)
            {
                dm.IsSelected = false;
            }
        }

        #endregion

        #endregion

        #region protected methods

        protected override void UpdateFrameElements()
        {
            tapeRange = tapeView.ComputeTapeRange(tapeRange);

            // time axis updated immediately...
            UpdateTimeAxis();

            // ...but panels are dispatched. Nevertheless it is ensure that all panels are updated at once.
            this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                                        new Action<RangeL>(tr =>
                                                            {
                                                                if (HasValidState)
                                                                {
                                                                    ForEachPanel(panel => panel.Update(tr));
                                                                }
                                                            }), tapeRange);
        }

        #endregion

        #region private methods

        private void GuardInitialized()
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException("This Chart is not initialized.");
            }            
        }

        private void RollbackDragging()
        {
            draggingPath.Data = null;

            Canvas.SetLeft(draggingPath, 0);
            Canvas.SetTop(draggingPath, 0);

            draggingTv = null;
            draggingSource = null;
            isDragging = false;
        }

        private void ScrollBy(double delta)
        {
            // we could think about moving the selected item, if scrollBar cannot be moved
            tapeScrollBar.Value = Math.Min(tapeScrollBar.Maximum, Math.Max(tapeScrollBar.Minimum, tapeScrollBar.Value + delta));
        }
        
        #region tape

        private void UpdateScrollBar(long tapeLength)
        {
            var sw = Stopwatch.StartNew();

            isValueChangedIgnored = true;

            if (tapeLength > 0)
            {
                double contentWidth = content.ActualWidth;
                if (IsFixedLayout)
                {
                    tapeScrollBar.Maximum =
                        tapeScrollBar.SmallChange =
                        tapeScrollBar.LargeChange =
                        tapeScrollBar.Minimum =
                        tapeScrollBar.ViewportSize = 0;

                    isScaleFactorChangedIgnored = true;
                    tapeRange.Start = 0;
                    tapeRange.End = tapeLength - 1;
                    tapeView.ScaleRangeToWidth(tapeRange, contentWidth);
                    isScaleFactorChangedIgnored = false;
                }
                else
                {
                    var startIndexAtMinimum = tapeView.IndexAt(contentWidth);
                    double screenWidth = tapeView.PositionAt(startIndexAtMinimum);
                    double sliceWidth = tapeView.WidthAt(0); // TODO: snap to custom slice-widths

                    tapeScrollBar.Maximum = tapeView.PositionAt(0);
                    tapeScrollBar.SmallChange = sliceWidth;
                    tapeScrollBar.LargeChange = sliceWidth*10;
                    tapeScrollBar.Minimum = screenWidth - tapeView.WidthAt(tapeLength - 1);
                    tapeScrollBar.ViewportSize = screenWidth;

                    if (tapeRange.IsEmpty)
                    {
                        tapeRange.Start = selectedItemTapeIndex ?? 0;
                    }
                    if (tapeRange.Start > startIndexAtMinimum)
                    {
                        tapeRange.Start = Math.Max(0, startIndexAtMinimum);
                    }
                    this.CalcTapeRangeEnd(contentWidth);
                    tapeScrollBar.Value = tapeView.PositionAt(tapeRange.Start);
                }
            }
            else
            {
                tapeScrollBar.Maximum =
                    tapeScrollBar.SmallChange =
                    tapeScrollBar.LargeChange =
                    tapeScrollBar.Minimum =
                    tapeScrollBar.ViewportSize = 0;

                tapeRange = RangeL.Empty;
            }

            isValueChangedIgnored = false;

            UpdateFrame();

            Trace.WriteLine(sw.Elapsed.TotalMilliseconds);
            Logger.Log(LogLevel.Debug, sw.Elapsed.TotalMilliseconds.ToString());
        }

        #endregion

        #region ChartPanel handling

        private void ForEachPanel(Action<ChartPanel> action)
        {
            childPanels.Select(cpc => cpc.Panel).ForEach(action);
        }

        private ChartPanel FindPanel(Func<ChartPanel, bool> predicate)
        {
            foreach (ChartPanelControls child in childPanels)
            {
                if (predicate.Invoke(child.Panel))
                {
                    return child.Panel;
                }
            }
            return null;
        }

        private ChartPanel AddPanel(PanelSettings panelSettings)
        {
            return InsertPanel(childPanels.Count(), panelSettings);
        }
        private ChartPanel InsertPanel(int index, PanelSettings panelSettings)
        {
            #region Create Splitter

            GridSplitter splitter = new GridSplitter();
            splitter.VerticalAlignment = VerticalAlignment.Center;
            splitter.Style = panelSplitterStyle;
            splitter.DragCompleted += this.childSplitter_DragCompleted;

            #endregion

            #region ensure splitter height on previous first

            if (index == 0 && childPanels.Any())
            {
                panelContainer.RowDefinitions[0].Height = new GridLength(splitter.Height);
            }

            #endregion

            #region add required rows

            int splitterRowIndex = index * 2;

            // add the required rows:
            var splitterRow = new RowDefinition();
            splitterRow.Height = new GridLength(index > 0 ? splitter.Height : 0);
            panelContainer.RowDefinitions.Insert(splitterRowIndex, splitterRow);

            var panelRow = new RowDefinition();
            panelContainer.RowDefinitions.Insert(splitterRowIndex + 1, panelRow);

            foreach (UIElement child in panelContainer.Children)
            {
                int oldIndex = Grid.GetRow(child);
                if (oldIndex >= splitterRowIndex)
                {
                    Grid.SetRow(child, oldIndex + 2);
                }
            }

            #endregion

            #region Add Splitter

            Grid.SetRow(splitter, splitterRowIndex);
            panelContainer.Children.Add(splitter);

            #endregion

            #region Create & Add panel

            var panel = new ChartPanel();
            panel.Tag = panelSettings;
            panel.Initialize(this);

            Grid.SetRow(panel, splitterRowIndex + 1);

            panelContainer.Children.Add(panel);
            childPanels.Insert(index, new ChartPanelControls(panel, splitter));

            #endregion

            #region adapt heights

            this.UpdatePanelRowHeights();

            #endregion

            return panel;
        }

        private void RemovePanel(ChartPanelControls pc)
        {
            int lastRowIndex = Grid.GetRow(pc.Panel);

            #region Remove panel

            panelContainer.Children.Remove(pc.Panel);
            pc.Panel.Dispose();

            #endregion

            #region Remove splitter

            pc.Splitter.DragCompleted -= this.childSplitter_DragCompleted;
            panelContainer.Children.Remove(pc.Splitter);

            #endregion

            #region Remove Rows

            panelContainer.RowDefinitions.RemoveAt(lastRowIndex); // deletes the panelRow
            panelContainer.RowDefinitions.RemoveAt(lastRowIndex - 1); // deletes the splitterRow

            foreach (UIElement child in panelContainer.Children)
            {
                int oldIndex = Grid.GetRow(child);
                if (oldIndex > lastRowIndex)
                {
                    Grid.SetRow(child, oldIndex - 2);
                }
            }

            #endregion

            childPanels.Remove(pc);

            #region adapt heights

            if (lastRowIndex == 1 && panelContainer.RowDefinitions.Any())
            {
                panelContainer.RowDefinitions[0].Height = new GridLength(0);    // hide topmost splitter
            }
            UpdatePanelRowHeights();

            #endregion
        }

        private void UpdatePanelRowHeights()
        {
            for (int i = 0; i < childPanels.Count; i++)
            {
                double relativeHeight = 1;
                var ps = childPanels[i].Panel.Tag as PanelSettings;
                if (ps != null)
                    relativeHeight = ps.RelativeHeight;
                panelContainer.RowDefinitions[i * 2 + 1].Height = new GridLength(relativeHeight, GridUnitType.Star);
            }
        }

        #endregion

        #region Selection updates

        private IEnumerable<TsVisualization> ExecuteForSelection(Action<ChartPanel, IEnumerable<TsVisualization>> action)
        {
            var selectedTvs = SelectedTvs;

            ForEachPanel(p =>
                            {
                                var pTvs = p.Tvs.Intersect(selectedTvs);
                                if (pTvs.Any())
                                {
                                    action.Invoke(p, pTvs);
                                }
                            });

            return selectedTvs;
        }
        
        private void UpdateSelectedPainterButton(IEnumerable<TsVisualization> selectedTvs)
        {
            PainterActivator selectedPainterActivator = selectedTvs.SelectAllEqualOrDefault(tv => tv.PainterActivator);

            if (selectedPainterActivator == null)
            {
                selectionPainterButtons.ForEach(pb => { pb.IsChecked = false; });
            }
            else
            {
                selectionPainterButtons.ForEach(pb => { pb.IsChecked = (selectedPainterActivator == (PainterActivator) (pb.Tag)); });
            }
        }

        #endregion

        #region TapeRange

        private void CalcTapeRangeEnd(double contentWidth)
        {
            double right = tapeView.PositionAt(tapeRange.Start) + tapeView.WidthAt(tapeRange.Start);
            double left = Math.Max(right - contentWidth, 0);
            var leftIndex = tapeView.IndexAt(left);
            if (tapeView.PositionAt(leftIndex) < left) // do not draw partial slices
            {
                --leftIndex;
            }
            tapeRange.End = leftIndex;
        }

        #endregion

        #region TimeAxis

        private void UpdateTimeAxis()
        {
            var positionTimes = new Dictionary<DateTime, double>();
            var rangeDates = new Dictionary<DateTime, RangeD>();

            if (!tapeRange.IsEmpty) // zoom will alsow guarantee that the tape range s no longer than int.Max
            {
                var startIndex = tapeRange.Start;
                var endIndex = tapeRange.End;
                var rangeLength = (int)tapeRange.Length;
                if (endIndex < tapeView.Length - 1)
                {
                    ++endIndex;
                    ++rangeLength;
                }
                double actualWidth = content.ActualWidth;
                double offset = actualWidth - (tapeView.PositionAt(startIndex) + tapeView.WidthAt(startIndex));
                double maxTimePosition = actualWidth - minTimeDistance;
                double previousTimeMiddle = double.MaxValue;
                DateTime previousDateTime = DateTime.MaxValue;
                bool previousDateTimeSet = false;
                var currentDateRange = new RangeD(0.0, actualWidth);

                var lefts = new List<double>(rangeLength);
                var widths = new List<double>(rangeLength);
                var middles = new List<double>(rangeLength);
                var dateTimes = new List<DateTime>(rangeLength);

                #region precalc positions

                for (var i = startIndex; i <= endIndex; i++)
                {
                    double left = offset + tapeView.PositionAt(i);
                    lefts.Add(left);

                    double width = tapeView.WidthAt(i);
                    widths.Add(width);

                    middles.Add(left + (width/2));

                    dateTimes.Add(tapeView.TimeAt(i));
                }

                #endregion

                // Possible performance gain: don't calc last two times twice

                #region Calc TimeMode

                if (dateTimes.Count >= 2)
                {
                    if (dateTimes[0].TimeOfDay == dateTimes[1].TimeOfDay)
                    {
                        if (dateTimes[0].Year - dateTimes[dateTimes.Count - 1].Year > 2)
                        {
                            timeMode = DateTimeMode.MonthAndYear;
                        }
                        else
                        {
                            timeMode = DateTimeMode.DayAndMonth;
                        }
                    }
                    else
                    {
                        timeMode = DateTimeMode.TimeAndDate;
                    }
                }

                #endregion

                #region cached times

                for (int i = movingTimes.Count - 1; i >= 0; --i)
                {
                    DateTime dt = movingTimes.ElementAt(i).Key;
                    if (!dateTimes.Contains(dt))
                    {
                        movingTimes.RemoveAt(i);
                    }
                    else
                    {
                        int j = dateTimes.IndexOf(dt);
                        double middle = middles[j];
                        if ((middle < minTimeDistance) ||
                            (middle > maxTimePosition) ||
                            (Math.Abs(previousTimeMiddle - middle) < minTimeDistance))
                        {
                            movingTimes.RemoveAt(i);
                        }
                        else
                        {
                            movingTimes[dt] = middle;
                            positionTimes.Add(dt, middle);
                            previousTimeMiddle = middle;
                        }
                    }
                }
                previousTimeMiddle = double.MaxValue;

                #endregion

                for (int i = 0; i < rangeLength; ++i)
                {
                    double left = lefts[i];
                    double width = widths[i];
                    double middle = middles[i];
                    DateTime dateTime = dateTimes[i];

                    bool timeCandidate = (i == 0);

                    if ((timeMode == DateTimeMode.TimeAndDate) && (dateTime.Date < previousDateTime.Date) ||
                        (timeMode == DateTimeMode.DayAndMonth) && (dateTime.Month < previousDateTime.Month) ||
                        (timeMode == DateTimeMode.MonthAndYear) && (dateTime.Year < previousDateTime.Year))
                    {
                        if (previousDateTimeSet) // we need to wait with adding the date, because we need to find its beginning
                        {
                            if (timeMode == DateTimeMode.TimeAndDate)
                            {
                                rangeDates.Add(previousDateTime.Date, currentDateRange);
                            }
                            else if (timeMode == DateTimeMode.DayAndMonth)
                            {
                                rangeDates.Add(previousDateTime.MonthDate(), currentDateRange);
                            }
                            else
                            {
                                rangeDates.Add(previousDateTime.YearDate(), currentDateRange);
                            }
                        }

                        timeCandidate = true;
                        currentDateRange = new RangeD(left, left + width);
                    }
                    else
                    {
                        currentDateRange.Start = left;
                    }

                    if (positionTimes.ContainsKey(dateTime))
                    {
                        previousTimeMiddle = middle;
                    }
                    else
                    {
                        if (timeCandidate &&
                            ((previousTimeMiddle - middle) >= minTimeDistance) &&
                            (middle >= minTimeDistance))
                        {
                            if (i != 0)
                            {
                                movingTimes.Add(dateTime, middle);
                            }
                            // search the SortedList for the next entry
                            int next = movingTimes.IndexOfKey(dateTime) - 1;
                            if ((next >= 0) &&
                                ((middle - movingTimes.ElementAt(next).Value) < minTimeDistance))
                            {
                                movingTimes.Remove(dateTime);
                            }
                            else
                            {
                                positionTimes.Add(dateTime, middle);
                                previousTimeMiddle = middle;
                            }
                        }
                    }

                    previousDateTime = dateTime;
                    previousDateTimeSet = true;
                }

                if (previousDateTimeSet)
                {
                    if (timeMode == DateTimeMode.TimeAndDate)
                    {
                        rangeDates.Add(previousDateTime.Date, currentDateRange);
                    }
                    else if (timeMode == DateTimeMode.DayAndMonth)
                    {
                        rangeDates.Add(previousDateTime.MonthDate(), currentDateRange);
                    }
                    else
                    {
                        rangeDates.Add(previousDateTime.YearDate(), currentDateRange);
                    }
                }

                #region select item
                var tapeIndex = tapeRange.EnsureContained(selectedItemTapeIndex ?? 0);
                this.ItemSelected(tapeIndex, lefts[(int)(tapeIndex - startIndex)], this.tapeView.WidthAt(tapeIndex));
                #endregion
            }

            #region Remove unused TimeMarkers

            int tmCount = positionTimes.Count;
            if (timeMarkers.Count > tmCount)
            {
                List<Marker> unused = timeMarkers.GetRange(tmCount, timeMarkers.Count - tmCount);
                foreach (Marker marker in unused)
                {
                    RemoveTimeMarker(marker);
                }
            }

            #endregion

            #region Create and Update TimeMarkers

            int index = 0;
            for (int i = tmCount - 1; i >= 0; i--)
            {
                if (index >= timeMarkers.Count)
                {
                    Marker timeMarker = new Marker(timeMarkerContainer, this, MarkerAlignment.Top, true, false);
                    timeMarkers.Add(timeMarker);

                    ForEachPanel(panel => panel.AddTimeMarker());
                }

                KeyValuePair<DateTime, double> positionDateTime = positionTimes.ElementAt(i);
                DateTime date = positionDateTime.Key;
                double position = positionDateTime.Value;
                string timeDisplayString;
                if (timeMode == DateTimeMode.TimeAndDate)
                {
                    timeDisplayString = date.ToLongTimeString();
                }
                else if (timeMode == DateTimeMode.DayAndMonth)
                {
                    timeDisplayString = date.ToDateDisplayString();
                }
                else
                {
                    timeDisplayString = date.ToMonthDisplayString();
                }

                timeMarkers[index].Update(position, timeDisplayString);
                ForEachPanel(panel => panel.UpdateTimeMarker(index, position));

                ++index;
            }

            #endregion

            int dmCount = rangeDates.Count;

            #region Remove unused DateMarkers

            while (dateMarkers.Count > dmCount)
            {
                RemoveDateMarker(dateMarkers.Last());
            }

            #endregion

            #region Create DateMarkers

            double timeMarkerHeight = timeMarkerContainer.Height;
            while (dateMarkers.Count < dmCount)
            {
                DateMarker dateMarker = new DateMarker();

                timeMarkerContainer.Children.Add(dateMarker);
                Canvas.SetTop(dateMarker, timeMarkerHeight - dateMarker.Height);

                dateMarkers.Add(dateMarker);
            }

            #endregion

            #region Update DateMarkers
            if (dmCount > 0)
            {
                for (int i = 0; i < dmCount; ++i)
                {
                    DateMarker dm = dateMarkers[i];

                    KeyValuePair<DateTime, RangeD> rangeDate = rangeDates.ElementAt(i);
                    dm.TimeMode = timeMode;
                    dm.Update(rangeDate.Value.Start, rangeDate.Value.End, rangeDate.Key);
                }
            }
            #endregion
        }

        private void RemoveTimeMarker(Marker timeMarker)
        {
            timeMarker.Dispose();
            int index = timeMarkers.IndexOf(timeMarker);
            timeMarkers.RemoveAt(index);

            ForEachPanel(p => p.RemoveTimeMarker(index));
        }

        private void RemoveDateMarker(DateMarker dateMarker)
        {
            timeMarkerContainer.Children.Remove(dateMarker);
            dateMarkers.Remove(dateMarker);
        }

        #endregion

        #region item selection
        private void ItemSelected(long tapeIndex, double sliceLeft, double sliceWidth)
        {
            ITeaFile ts = this.SelectedTss.LastOrDefault() ?? tapeView.Tss.LastOrDefault();

            if (selectedItemTapeIndex != tapeIndex || selectedItemTs != ts)
            {
                selectedItemTapeIndex = tapeIndex;
                bool displayName = !IsSingleTs;

                if ((selectedItemTs != ts) || (selectedItemPanel.Children.Count == 0))
                {
                    selectedItemTs = ts;
                    selectedItemPanel.Children.Clear();
                    if (ts != null)
                    {
                        if (displayName)
                        {
                            selectedItemPanel.Children.Add(new NameValueView {NameText = "Name", ValueText = ts.Name});
                        }
                        selectedItemPanel.Children.Add(new NameValueView {NameText = "Index"});
                        ts.Description.ItemDescription.Fields.ForEach(f => selectedItemPanel.Children.Add(new NameValueView {NameText = f.Name}));
                    }
                }

                if (ts != null)
                {
                    var tsi = tapeView.GetTimeSeriesEnd(ts, tapeIndex); // tbd: inefficient
                    ((NameValueView)selectedItemPanel.Children[displayName ? 1 : 0]).ValueText = (tsi + 1).ToString();
                    int i = displayName ? 2 : 1;
                    foreach (var f in ts.Description.ItemDescription.Fields)
                    {
                        var value = ts.GetFieldValueText(f, tsi);
                        ((NameValueView)selectedItemPanel.Children[i]).ValueText = value;
                        i++;
                    }

                    if (this.editor != null)
                    {
                        this.editor.SetTeaFileIndex(this, tsi);
                    }
                }
            }

            ForEachPanel(p => p.TimeSliceMarker.Update(sliceLeft, sliceWidth));
        }
        #endregion

        #region ItemTypeSettings

        private void UpdateItemTypeSettings(ItemTypeSettings settings)
        {
            itemTypeSettings = settings;
            chartSettings.DataContext = itemTypeSettings;
        }
        private void UpdateAndStoreChartSettings(bool updateHeights)
        {
            if (!ignoreChartSettingChanges &&
                itemTypeSettings != null &&
                itemTypeSettings.SelectedChartSettings != null)
            {
                #region panel heights

                if (updateHeights)
                {
                    double sumPanelHeight = childPanels.Sum(cpc => cpc.Panel.ActualHeight);
                    if (sumPanelHeight > 0)
                    {
                        childPanels.Select(cpc => cpc.Panel).ForEach(cp =>
                            {
                                var ps = cp.Tag as PanelSettings;
                                if (ps != null)
                                {
                                    ps.RelativeHeight = Math.Max(PanelSettings.MinRelativeHeight, cp.ActualHeight / sumPanelHeight);
                                }
                            });
                    }
                }

                #endregion

                #region painter

                itemTypeSettings.SelectedChartSettings.PanelSettings.ForEach(ps =>
                    {
                        List<ToggleButton> btns;
                        if (panelSettingsPainterButtons.TryGetValue(ps, out btns))
                        {
                            var selectedButton = btns.FirstOrDefault(b => b.IsChecked == true);
                            ps.SelectedPainter = selectedButton == null ? null : ((Tuple<PanelSettings, PainterActivator, PainterMapping>)selectedButton.Tag).Item3;
                        }
                    });

                #endregion

                itemTypeSettings.Store();
            }
        }

        #endregion

        #endregion

        #region eventhandler

        private void panelContainer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (HasValidState && e.WidthChanged && (e.NewSize.Width > 0)) // if the height was changed, the panels will update themselves
            {
                UpdateScrollBar(tapeView.Length);
            }
        }

        private void chartControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsKeyboardFocusWithin)
            {
                this.Focus();
                Keyboard.Focus(this);
            }
        }

        private void chartControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging && HasValidState)
            {
                ChartPanel dropPanel = FindPanel(p =>
                                                    {
                                                        double yPosition = e.GetPosition(p).Y;
                                                        return (yPosition >= 0) && (yPosition <= p.ActualHeight);
                                                    }
                    );

                if ((dropPanel != null) && (dropPanel != draggingSource))
                {
                    draggingSource.Remove(draggingTv, false);
                    dropPanel.Add(draggingTv.TimeSeries, draggingTv.PainterActivator, draggingTv.PainterMapping, draggingTv.DrawingAttributes, draggingTv.YScalerInternal.ScaleMode);

                    UpdateFrame();
                }

                RollbackDragging();
            }
        }

        private void chartControl_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (HasValidState)
            {
                if (KeyboardUtility.IsCtrlPressed)
                {
                    tapeView.Zoom((double)e.Delta / 1000.0);
                }
                else
                {
                    ScrollBy(e.Delta);
                    e.Handled = true;
                }
            }
        }

        private void chartControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && HasValidState)
            {
                Point currentPosition = e.GetPosition(this);
                Canvas.SetLeft(draggingPath, currentPosition.X - draggingPathStart.X);
                Canvas.SetTop(draggingPath, currentPosition.Y - draggingPathStart.Y + draggingYOffset);
            }
        }

        private void chartControl_MouseLeave(object sender, MouseEventArgs e)
        {
            if (isDragging && HasValidState)
            {
                RollbackDragging();
            }
        }

        private void chartControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (HasValidState)
            {
                if (isDragging)
                {
                    if (e.Key == Key.Escape)
                    {
                        RollbackDragging();
                        e.Handled = true;
                    }
                }
                else
                {
                    if (!IsSingleTs && (e.Key == Key.Delete))
                    {
                        var selected = SelectedTvs.ToArray();
                        ForEachPanel(p =>
                            {
                                var toDelete = p.Tvs.Intersect(selected);
                                toDelete.ForEach(tv => p.Remove(tv, !IsSingleTs));
                            });
                        Untape(selected);
                        e.Handled = true;
                    }
                }
            }
        }

        private void chartControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (HasValidState)
            {
                if ((e.Key == Key.Left) || (e.Key == Key.Down))
                {
                    ScrollBy(-tapeScrollBar.LargeChange);
                    e.Handled = true;
                }
                else if (e.Key == Key.PageDown)
                {
                    ScrollBy(tapeView.PositionAt(tapeRange.End) - tapeView.PositionAt(tapeRange.Start));
                    e.Handled = true;
                }
                else if ((e.Key == Key.Right) || (e.Key == Key.Up))
                {
                    ScrollBy(tapeScrollBar.LargeChange);
                    e.Handled = true;
                }
                else if (e.Key == Key.PageUp)
                {
                    ScrollBy(tapeView.PositionAt(tapeRange.Start) - tapeView.PositionAt(tapeRange.End));
                    e.Handled = true;
                }
                else if (e.Key == Key.End)
                {
                    SetTapeIndex(0);
                    e.Handled = true;
                }
                else if (e.Key == Key.Home)
                {
                    SetTapeIndex(Math.Max(0, tapeView.Length - 1));
                    e.Handled = true;
                }
            }
        }

        private void painter_selection_Click(object sender, RoutedEventArgs e)
        {
            if (HasValidState)
            {
                var painterButton = (ToggleButton) sender;
                var painterActivator = (PainterActivator) (painterButton.Tag);

                var selectedTvs = ExecuteForSelection((p, tvs) => p.SetPainter(tvs, painterActivator, null));
                UpdateFrame();

                UpdateSelectedPainterButton(selectedTvs);
            }
        }

        private void rootSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (HasValidState)
            {
                if (rootSplitterRow.ActualHeight > rootSplitter.Height)
                {
                    AddPanel(null); // TODO: set correct height
                }
            }
            rootSplitterRow.Height = new GridLength(rootSplitter.Height);
        }

        private void childSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (HasValidState)
            {
                // search for panel to remove
                childPanels.Where(cpc =>
                                  cpc.Panel.ActualHeight <= 0).ToArray().ForEach(cpc =>
                                      {
                                          RemovePanel(cpc);
                                          if (IsSingleTs)
                                          {
                                              var panelSettings = (PanelSettings)cpc.Panel.Tag;
                                              panelSettingsPainterButtons[panelSettings].ForEach(b => b.IsChecked = false);
                                          }
                                      });
                this.UpdateAndStoreChartSettings(true);
            }
        }

        private void tapeScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (!isValueChangedIgnored && HasValidState)
            {
                var ti = tapeView.IndexAt(e.NewValue);
                if (ti >= 0)
                {
                    tapeRange.Start = ti;
                    this.CalcTapeRangeEnd(this.content.ActualWidth);

                    UpdateFrame();
                }
            }
        }
        
        private void ItemTypeSettingsControl_ItemTypeSettingsChanged(object sender, EventArgs<ItemTypeSettings> args)
        {
            if (tapeView.Tss.Any(ts => ts.ItemType == args.Value.TsItemType))
                UpdateItemTypeSettings(args.Value);
        }

        private void chartSettings_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                this.ignoreChartSettingChanges = true;
                
                #region clear
                chartSettingsToolbarItems.ForEach(o => toolBar.Items.Remove(o));
                chartSettingsToolbarItems.Clear();
                panelSettingsPainterButtons.SelectMany(kv => kv.Value).ForEach(b => b.Click -= painter_chartSettings_Click);
                panelSettingsPainterButtons.Clear();
                childPanels.ToList().ForEach(RemovePanel);
                #endregion

                if (itemTypeSettings != null && itemTypeSettings.SelectedChartSettings != null)
                {
                    var defaultPainters = new List<ToggleButton>();

                    #region add controls
                    int index = 1;
                    foreach (var panelSettings in itemTypeSettings.SelectedChartSettings.PanelSettings.OrderBy(ps => ps.Index))
                    {
                        if (!String.IsNullOrEmpty(panelSettings.Name))
                        {
                            var textBlock = new TextBlock { Style = toolbarTextBlockStyle };
                            textBlock.Text = panelSettings.Name;
                            toolBar.Items.Insert(index++, textBlock);
                            chartSettingsToolbarItems.Add(textBlock);
                        }

                        foreach (var t in panelSettings.Painters
                            .Select(pm =>
                            {
                                var pa = PainterManager.Instance.FindPainterByType(pm.PainterType);
                                if (pa == null)
                                {
                                    Console.WriteLine("Painter {0} is not registered.", pm.PainterType);
                                }
                                return new Tuple<PanelSettings, PainterActivator, PainterMapping>(panelSettings, pa, pm);
                            })
                            .Where(tt => tt.Item2 != null))
                        {
                            var painterImage = new Image();
                            painterImage.Source = t.Item3.Image;
                            painterImage.Style = toolbarImageStyle;

                            var painterButton = new ToggleButton();
                            painterButton.ToolTip = t.Item3.ToolTip;
                            painterButton.Style = toolbarButtonStyle;
                            painterButton.Content = painterImage;

                            painterButton.Tag = t;
                            painterButton.Click += painter_chartSettings_Click;

                            toolBar.Items.Insert(index++, painterButton);
                            chartSettingsToolbarItems.Add(painterButton);
                            if (!panelSettingsPainterButtons.ContainsKey(panelSettings))
                            {
                                panelSettingsPainterButtons.Add(panelSettings, new List<ToggleButton>());
                            }
                            panelSettingsPainterButtons[panelSettings].Add(painterButton);

                            if (panelSettings.SelectedPainter == t.Item3)
                            {
                                defaultPainters.Add(painterButton);
                            }
                        }

                        var separator = new Separator { Style = toolbarSeparatorStyle };
                        toolBar.Items.Insert(index++, separator);
                        chartSettingsToolbarItems.Add(separator);
                    }
                    #endregion

                    #region select default values
                    defaultPainters.ForEach(b =>
                        {
                            b.IsChecked = true;
                            painter_chartSettings_Click(b, null);
                        });
                    #endregion
                }

                toolBar.InvalidateMeasure();
            }
            finally
            {
                ignoreChartSettingChanges = false;
            }
        }
        private void painter_chartSettings_Click(object sender, RoutedEventArgs e)
        {
            if (HasValidState)
            {
                var painterButton = (ToggleButton)sender;
                var tag = (Tuple<PanelSettings, PainterActivator, PainterMapping>)painterButton.Tag;
                var panelSettings = tag.Item1;
                var painterActivator = tag.Item2;
                var painterMapping = tag.Item3;

                if (painterButton.IsChecked == true)
                {
                    panelSettingsPainterButtons[panelSettings].Except(new[] { painterButton }).ForEach(b => b.IsChecked = false);
                    // Add panel or update Ts
                    var panelControls = childPanels.FirstOrDefault(cpc => cpc.Panel.Tag == panelSettings);
                    if (panelControls == null)
                    {
                        int index = this.childPanels.Select(cpc => cpc.Panel).TakeWhile(p => ((PanelSettings)p.Tag).Index <= panelSettings.Index).Count();
                        var panel = InsertPanel(index, panelSettings);
                        if (IsSingleTs)
                        {
                            panel.Add(tapeView.Tss.Single(), painterActivator, painterMapping, panelSettings.DrawingAttributes, panelSettings.YScaleMode);
                        }
                        else
                        {
                            // TODO: Implement
                            throw new NotImplementedException("Painter on ChartSettings not yet implemented.");
                        }
                    }
                    else
                    {
                        panelControls.Panel.SetPainter(panelControls.Panel.Tvs, painterActivator, painterMapping);
                    }
                    UpdateFrame();
                    this.UpdateAndStoreChartSettings(true);
                }
                else
                {
                    var panel = childPanels.FirstOrDefault(cpc => cpc.Panel.Tag == panelSettings);
                    if (panel != null)
                    {
                        RemovePanel(panel);
                        this.UpdateAndStoreChartSettings(false);
                    }
                }
            }
        }

        #endregion

        #region fields

        private readonly List<ChartPanelControls> childPanels;
        private readonly Dictionary<ICursor, Marker> cursorMarkers;
        private readonly List<DateMarker> dateMarkers;
        private bool isDisposed;
        private bool isInitialized;

        private bool isScaleFactorChangedIgnored;
        private bool isValueChangedIgnored;
        private readonly SortedList<DateTime, double> movingTimes;
        private readonly List<ToggleButton> selectionPainterButtons;
        private ITeaFileEditor editor;

        private bool suppressSelectionChanged;
        private RangeL tapeRange;
        private TapeViewBase tapeView;
        private readonly List<Marker> timeMarkers;
        private DateTimeMode timeMode;

        private ItemTypeSettings itemTypeSettings;
        private readonly Dictionary<PanelSettings, List<ToggleButton>> panelSettingsPainterButtons;
        private readonly List<object> chartSettingsToolbarItems;
        bool ignoreChartSettingChanges;

        #region item selection

        private long? selectedItemTapeIndex;
        private ITeaFile selectedItemTs;

        #endregion

        #region drag/drop ts

        private Point draggingPathStart;
        private ChartPanel draggingSource;
        private TsVisualization draggingTv;
        private double draggingYOffset;
        private bool isDragging;

        #endregion

        #region styles

        private static bool areStylesInitialized;
        private static Style toolbarSeparatorStyle;
        private static Style toolbarTextBlockStyle;
        private static Style toolbarButtonStyle;
        private static Style toolbarImageStyle;
        private static Style panelSplitterStyle;

        #endregion

        #region constants

        private static double minTimeDistance = 100;
        private static double minXZoomDistance = 10;

        #endregion

        #endregion

        #region embedded types

        private class ChartPanelControls
        {
            #region properties

            private ChartPanel panel;

            private GridSplitter splitter;

            public ChartPanel Panel
            {
                get { return panel; }
            }

            public GridSplitter Splitter
            {
                get { return splitter; }
            }

            #endregion

            #region ctor

            public ChartPanelControls(ChartPanel panel, GridSplitter splitter)
            {
                this.panel = panel;
                this.splitter = splitter;
            }

            #endregion
        }

        #endregion
    }
}