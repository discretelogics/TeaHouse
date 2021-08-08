using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using NLog;
using TeaTime.API;
using TeaTime.Chart.Core;
using TeaTime.Chart.Markers;
using TeaTime.Chart.Painters;
using TeaTime.Data;
using Microsoft.VisualStudio.Shell;
using TeaTime.Chart.Settings;

namespace TeaTime.Chart
{
    public partial class ChartPanel : UserControl, IDisposable
    {
        static readonly Logger logger = LogManager.GetCurrentClassLogger();

        #region properties
        internal IEnumerable<TsVisualization> Tvs
        {
            get
            {
                return tvs;
            }
        }
        internal IEnumerable<ITeaFile> SelectedTss
        {
            get
            {
                return selectedTvs.Select(tv => tv.TimeSeries);
            }
        }
        internal IEnumerable<TsVisualization> SelectedTvs
        {
            get
            {
                return selectedTvs;
            }
        }

        internal bool IsSingleTs
        {
            get { return chart.IsSingleTs; }
        }
        internal bool IsFixedLayout
        {
            get { return chart.IsFixedLayout; }
        }
        private bool HasValidState
        {
            get
            {
                return isInitialized && !isDisposed && !chart.IsStopped;
            }
        }
        #endregion

        #region ctor, Initialize, Dispose
        public ChartPanel()
        {
            InitializeComponent();

            tvs = new List<TsVisualization>();
            selectedTvs = new List<TsVisualization>();

            timeMarkers = new List<MarkerLine>();
            valueMarkers = new List<MarkerLine>();

            yScalePanels = new Dictionary<YScaler, YScalePanel>();

            cursor = new ChartPanelCursor(this, container);

            moveState = MoveState.Stopped;
        }

        public void Initialize(ChartControl chart)
        {
            if (!isInitialized)
            {
                Guard.ArgumentNotNull(chart, "chart");

                this.chart = chart;

                timeSliceMarker = new TimeSliceMarker(container);
                for (int i = 0; i < chart.TimeMarkerCount; i++)
                {
                    AddTimeMarker();
                }

                isInitialized = true;
            }
            else
            {
                throw new InvalidOperationException("This ChartPanel was already initialized.");
            }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                if (isInitialized)
                {
                    ClearSelection();

                    #region remove Tvs
                    chart.Untape(tvs);
                    #endregion

                    #region remove TimeMarkers
                    for (int i = timeMarkers.Count - 1; i >= 0; i--)
                    {
                        RemoveTimeMarker(i);
                    }
                    #endregion

                    #region remove ValueMarkers
                    RemoveAllValueMarkers();
                    #endregion

                    #region remove TimeSliceMarkers
                    timeSliceMarker.Dispose();
                    timeSliceMarker = null;
                    #endregion

                    #region remove cursor
                    cursor.Dispose();
                    #endregion
                }

                isDisposed = true;
            }            
        }
        #endregion

        #region internal methods
        internal IEnumerable<TsVisualization> Add(IEnumerable<ITeaFile> tss)
        {
            Guard.ArgumentNotNull(tss, "tss");
            GuardState();

            var addedTvs = new List<TsVisualization>();
            foreach (var ts in tss)
            {
                var tv = Add(ts, null, null, null, YScaleMode.Linear);
                addedTvs.Add(tv);
            }

            return addedTvs;
        }
        internal TsVisualization Add(ITeaFile ts, PainterActivator painterActivator, PainterMapping painterMapping, DrawingAttributes drawingAttributes, YScaleMode yScaleMode)
        {
            GuardState();
            Guard.ArgumentNotNull(ts, "ts");

            #region Create Path
            Path path = new Path();
            RenderOptions.SetEdgeMode(path, EdgeMode.Aliased);
            container.Children.Add(path);
            #endregion

            #region Find or create YScaler

            TsVisualization associatedTv = tvs.FirstOrDefault(tvv => ts.YScaleName == tvv.YScaler.Name);
                
            YScaler yScaler;
            if (associatedTv == null)
            {
                yScaler = AddYScaler(ts.YScaleName, yScaleMode);
            }
            else
            {
                yScaler = associatedTv.YScalerInternal;
            }
            #endregion

            #region PainterActivator
            if (painterActivator == null)
            {
                painterActivator = PainterManager.Instance.GetPainterByItemType(ts.ItemType);
            }
            #endregion

            #region Create DrawingAttributes
            if (drawingAttributes == null)
            {
                drawingAttributes = new DrawingAttributes { Color = Colors.Black, Width = 1, Height = 1 };
            }
            #endregion
            
            #region create new TsVisualization
            TsVisualization tv = new TsVisualization(chart.TapeView, ts, yScaler, painterActivator, painterMapping, path, drawingAttributes);
            #endregion

            #region Add to YScaler
            yScaler.Add(tv);
            #endregion

            tvs.Add(tv);

            // select last added ts only
            chart.ClearTsSelection();
            Select(tv);

            return tv;
        }

        internal void Remove(TsVisualization tv, bool closeTs)
        {
            container.Children.Remove(tv.Path);
            Deselect(tv);

            #region YScaler
            var yScaler = tv.YScalerInternal;
            yScaler.Remove(tv);
            if (!yScaler.Tvs.Any())
            {
                RemoveYScaler(yScaler);
            }
            #endregion

            tvs.Remove(tv);

            if (closeTs)
            {
                tv.TimeSeries.Dispose();
            }
        }

        internal void SetDrawingAttributes(IEnumerable<TsVisualization> tvs, DrawingAttributes da)
        {
            Guard.ArgumentNotNull(tvs, "tvs");
            GuardState();

            tvs.ForEach(tv =>
                {
                    tv.DrawingAttributes = da;

                    if (selectedTvs.Contains(tv))
                    {
                        tv.Path.StrokeThickness = tv.Path.StrokeThickness * 2;
                    }
                }
            );
        }
        internal void SetPainter(IEnumerable<TsVisualization> tvs, PainterActivator painterActivator, PainterMapping painterMapping)
        {
            Guard.ArgumentNotNull(tvs, "tvs");
            GuardState();

            tvs.ForEach(tv => tv.UpdatePainter(painterActivator, painterMapping));
        }
        internal void SetYScaleMode(IEnumerable<TsVisualization> tvs, YScaleMode mode)
        {
            Guard.ArgumentNotNull(tvs, "tvs");
            GuardState();

            tvs.ForEach(tv =>
                {
                    tv.YScalerInternal.ScaleMode = mode;
                }
            );
        }

        internal void Update(RangeL targetTapeRange)
        {
            GuardState();

            var sw = Stopwatch.StartNew();
            Update(tvs, targetTapeRange, container.ActualSize());
            var ms = sw.Elapsed.TotalMilliseconds;
            logger.Log(LogLevel.Debug, "Update took {0} ms".Formatted(ms));
        }

        internal void ClearSelection()
        {
            selectedTvs.ToArray().ForEach(this.Deselect);
        }

        #region TimeMarkers
        internal void SelectTimeMarker(int index, ISelectable sender)
        {
            timeMarkers[index].Select(sender);
        }
        internal void DeselectTimeMarker(int index, ISelectable sender)
        {
            timeMarkers[index].Deselect(sender);
        }

        internal void UpdateTimeMarker(int index, double xPosition)
        {
            timeMarkers[index].Update(xPosition);
        }
        internal void AddTimeMarker()
        {
            MarkerLine timeMarker = new MarkerLine(Orientation.Vertical, container, true, false);
            timeMarkers.Add(timeMarker);
        }
        internal void RemoveTimeMarker(int index)
        {
            timeMarkers[index].Dispose();
            timeMarkers.RemoveAt(index);
        }
        #endregion

        #region ValueMarkers
        internal void SelectValueMarker(double position, ISelectable sender)
        {
            var marker = valueMarkers.FirstOrDefault(vm => vm.ActualPosition == position);
            if (marker != null)
            {
                marker.Select(sender);
            }
        }
        internal void DeselectValueMarker(double position, ISelectable sender)
        {
            var marker = valueMarkers.FirstOrDefault(vm => vm.ActualPosition == position);
            if (marker != null)
            {
                marker.Deselect(sender);
            }
        }

        internal void ValuePositionsUpdated(YScalePanel sender, List<double> positions)
        {
            if (sender == yScalePanels.Values.FirstOrDefault())
            {
                while (valueMarkers.Count > positions.Count)
                {
                    RemoveValueMarker(valueMarkers.Count - 1);
                }

                while (valueMarkers.Count < positions.Count)
                {
                    MarkerLine line = new MarkerLine(Orientation.Horizontal, container, true, false);
                    valueMarkers.Add(line);
                }

                for (int i = 0; i < positions.Count; i++)
                {
                    valueMarkers[i].Update(positions[i]);
                }
            }
        }
        #endregion
        #endregion

        #region private methods
        private void GuardState()
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException("This ChartPanel is not initialized.");
            }            
        }

        private void Update(IEnumerable<TsVisualization> tvs, RangeL targetTapeRange, Size targetSize)
        {
            if (targetTapeRange.IsEmpty) return;

            // check if the viewport is still actual. If not skip the calculation of the path.
            if ((chart.TapeRange == targetTapeRange) &&
                (container.ActualSize() == targetSize))
            {
                var tapeView = chart.TapeView;
                foreach (TsVisualization tv in tvs)
                {
                    TimeSeriesDrawing drawing = null;
                    
                    long tsiLeft = tapeView.GetTimeSeriesStart(tv.TimeSeries, targetTapeRange.End);
                    long tsiRight = tapeView.GetTimeSeriesEnd(tv.TimeSeries, targetTapeRange.Start);

                    if ((tv.TimeSeries.TimeAt(tsiLeft) <= tapeView.TimeAt(targetTapeRange.Start)) &&
                        (tv.TimeSeries.TimeAt(tsiRight) >= tapeView.TimeAt(targetTapeRange.End)))
                    {
                        tv.YScalerInternal.Prepare(targetSize.Height, targetTapeRange);
                        UpdateCloseMarker(tv, targetTapeRange.Start);

                        drawing = tv.Painter.Draw(targetTapeRange, targetSize.Width);
                    }
                    //else
                    //{
                        // TODO: review handling, do we need to hide CloseMarker, ValueMarkers in YScalePanel and switch ValueMarkers to other tvs if first?
                    //}

                    // set the real path-data
                    if (drawing == null)
                    {
                        tv.Path.Data = null;
                        tv.Path.Fill = null;
                    }
                    else
                    {
                        if (drawing.Geometry != null)
                        {
                            drawing.Geometry.Freeze();
                        }
                        tv.Path.Data = drawing.Geometry;
                        tv.Path.Fill = drawing.Fill;
                    }
                }

                // Update Cursors
                if (cursor.IsVisible)
                {
                    Point cursorPosition = cursor.ActualPosition;
                    UpdateCursorMarkers(cursor, cursorPosition.X, cursorPosition.Y);
                }
            }
        }

        #region Selection
        private void Select(TsVisualization tv)
        {
            if (!selectedTvs.Contains(tv))
            {
                selectedTvs.Add(tv);
                
                if (!IsSingleTs)
                {
                    tv.Path.StrokeThickness = tv.Path.StrokeThickness * 2;
                }

                chart.TsSelectionChanged();
            }
        }
        private void Deselect(TsVisualization tv)
        {
            if (!IsSingleTs && selectedTvs.Contains(tv)) // do not deselect on IsSingleTs
            {
                selectedTvs.Remove(tv);
                tv.Path.StrokeThickness = tv.Path.StrokeThickness / 2;

                chart.TsSelectionChanged();
            }
        }
        #endregion

        #region YScalePanel
        private void UpdateCloseMarker(TsVisualization tv, long startTapeIndex)
        {
            long tsi = chart.TapeView.GetTimeSeriesStart(tv.TimeSeries, startTapeIndex);
            double close = tv.Painter.GetClose(tsi);

            yScalePanels[tv.YScalerInternal].UpdateCloseMarker(tv, close);
        }

        private YScaler AddYScaler(string name, YScaleMode yScaleMode)
        {
            double actualLeft = Math.Abs(this.Margin.Left);
            double actualRight = Math.Abs(this.Margin.Right);

            HorizontalAlignment containerAlignment;
            if (actualLeft < actualRight)
            {
                containerAlignment = HorizontalAlignment.Left;
            }
            else
            {
                containerAlignment = HorizontalAlignment.Right;
            }

            // create YScalePanel
            var yScalePanel = new YScalePanel();
            // create YScaler
            var yScaler = new YScaler(chart, yScalePanel, name, yScaleMode);
            // link yScalePanel with yScaler
            yScalePanel.Initialize(this, yScaler, containerAlignment);
            
            // set width of the column
            int columnIndex = (containerAlignment == HorizontalAlignment.Left) ? 0 : 2;
            double columnWidth = panelGrid.ColumnDefinitions[columnIndex].Width.Value + yScalePanel.Width;
            panelGrid.ColumnDefinitions[columnIndex].Width = new GridLength(columnWidth);

            // set margin of the container
            if (containerAlignment == HorizontalAlignment.Left)
            {
                leftDock.Children.Add(yScalePanel);
                DockPanel.SetDock(yScalePanel, Dock.Right);
            }
            else
            {
                rightDock.Children.Add(yScalePanel);
                DockPanel.SetDock(yScalePanel, Dock.Left);
            }

            // set margin of panel and chart
            double left;
            double right;
            if (containerAlignment == HorizontalAlignment.Left)
            {
                left = columnWidth;
                right = actualRight;
            }
            else
            {
                left = actualLeft;
                right = columnWidth;
            }
            this.Margin = new Thickness(-left, 0, -right, 0);
            chart.RecalcMargin();

            yScalePanels.Add(yScaler, yScalePanel);

            // create Cursor-Marker
            if (cursor.IsVisible)
            {
                yScalePanel.AddCursorMarker(cursor);
            }

            return yScaler;
        }

        private void RemoveYScaler(YScaler yscaler)
        {
            // remove panel
            YScalePanel yScalePanel = yScalePanels[yscaler];
            int columnIndex = leftDock.Children.Contains(yScalePanel) ? 0 : 2;

            double columnWidth = panelGrid.ColumnDefinitions[columnIndex].Width.Value - yScalePanel.Width;
            panelGrid.ColumnDefinitions[columnIndex].Width = new GridLength(columnWidth);

            // set margin of panel and chart
            double left;
            double right;
            if (columnIndex == 0)
            {
                left = columnWidth;
                right = Math.Abs(this.Margin.Right);

                leftDock.Children.Remove(yScalePanel);
            }
            else
            {
                left = Math.Abs(this.Margin.Left);
                right = columnWidth;

                rightDock.Children.Remove(yScalePanel);
            }

            this.Margin = new Thickness(-left, 0, -right, 0);
            chart.RecalcMargin();

            yScalePanel.Dispose();
            yScalePanels.Remove(yscaler);

            if (!yScalePanels.Any())
            {
                RemoveAllValueMarkers();
            }
        }
        #endregion

        #region ValueMarkers
        private void RemoveValueMarker(int index)
        {
            MarkerLine line = valueMarkers[index];
            line.Dispose();
            valueMarkers.Remove(line);
        }
        private void RemoveAllValueMarkers()
        {
            for (int i = valueMarkers.Count - 1; i >= 0; i--)
            {
                RemoveValueMarker(i);
            }
        }
        #endregion

        #region ChartPanelCursor
        private void UpdateCursorMarkers(ICursor cursor, double xPosition, double yPosition)
        {
            yScalePanels.Values.ForEach(yp => yp.UpdateCursorMarker(cursor, yPosition));

            chart.UpdateCursorMarker(this, cursor, xPosition);
        }
        private void AddCursorMarkers(ICursor cursor)
        {
            yScalePanels.Values.ForEach(ym => ym.AddCursorMarker(cursor));
            chart.AddCursorMarker(cursor);
        }
        private void RemoveCursorMarkers(ICursor cursor)
        {
            yScalePanels.Values.ForEach(ym => ym.RemoveCursorMarker(cursor));
            chart.RemoveCursorMarker(cursor);
        }

        private void CursorRegionSelected(Point startPosition, Point endPosition)
        {
            if (KeyboardUtility.IsCtrlPressed)
            {
                tvs.ForEach(tv => tv.YScalerInternal.ZoomRangeAbsolute(startPosition.Y, endPosition.Y));
                chart.ZoomRangePosition(startPosition.X, endPosition.X);
            }
            else if (KeyboardUtility.IsShiftPressed)
            {
                var geo = new RectangleGeometry(new Rect(startPosition, endPosition));
                foreach (TsVisualization tv in tvs)
                {
                    if (geo.FillContainsWithDetail(tv.Path.Data) != IntersectionDetail.Empty)
                    {
                        Select(tv);
                    }
                }
            }
        }
        private void CursorPointSelected(Point position)
        {
            if (!KeyboardUtility.IsShiftPressed && !KeyboardUtility.IsCtrlPressed)
            {
                chart.ClearTsSelection();
            }

            var tvsInRange = GetTvsInRange(position);
            tvsInRange.ForEach(tv =>
                {
                    if ((KeyboardUtility.IsShiftPressed || KeyboardUtility.IsCtrlPressed) &&
                        selectedTvs.Contains(tv))
                    {
                        Deselect(tv);
                    }
                    else
                    {
                        Select(tv);
                    }
                });
        }
        #endregion

        #region move panel
        private void StartMoving()
        {
            if (!IsFixedLayout && !IsSingleTs && (moveState != MoveState.Moving))
            {
                movePosition = Mouse.GetPosition(container);

                container.Cursor = Cursors.ScrollAll;
                moveState = MoveState.Moving;
            }
        }
        private void StopMoving()
        {
            if (moveState != MoveState.Stopped)
            {
                container.Cursor = Cursors.Arrow;
                moveState = MoveState.Stopped;
            }
        }
        #endregion

        private IEnumerable<TsVisualization> GetTvsInRange(Point position)
        {
            var geo = new EllipseGeometry(position, 8, 8);
            return tvs.Where(tv => geo.FillContainsWithDetail(tv.Path.Data) != IntersectionDetail.Empty);
        }         
        #endregion

        #region delegates
        private delegate void UpdateDelegate(IEnumerable<TsVisualization> tvs, RangeL targetTapeRange, Size targetSize);
        #endregion

        #region eventhandler
        private void container_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (HasValidState)
            {
                if (e.WidthChanged)
                {
                    valueMarkers.ForEach(vm => vm.Update());
                }
                if (e.HeightChanged)    // if the width was changed, SetTapeRange will be called from tape 
                {
                    this.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                        new UpdateDelegate(Update), tvs, chart.TapeRange, container.ActualSize());

                    timeMarkers.ForEach(tm => tm.Update());
                    timeSliceMarker.Update();
                }
            }
        }

        private void container_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (HasValidState && (moveState == MoveState.Moving))
            {
                Point position = e.GetPosition(container);
                double deltaY = (position.Y - movePosition.Y) * YScalePanel.MoveMultiplier;
                double deltaX = (movePosition.X - position.X) * YScalePanel.MoveMultiplier;

                if (Math.Abs(deltaY) >= 1)
                {
                    tvs.ForEach(tv => tv.YScalerInternal.MoveRange(deltaY));
                    movePosition.Y = position.Y;
                }

                if (((deltaX < 0) && (Math.Abs(deltaX) >= chart.TapeView.WidthAt(chart.TapeRange.End))) ||
                    ((deltaX > 0) && (deltaX >= chart.TapeView.WidthAt(chart.TapeRange.Start))))
                {
                    chart.MoveRange(deltaX);
                    movePosition.X = position.X;
                }
            }
        }
        private void container_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (HasValidState)
            {
                container.Focus();
                if (KeyboardUtility.IsAltPressed)
                {
                    StartMoving();
                    e.Handled = true;
                }
                else
                {
                    var position = e.GetPosition(container);
                    var tvsInRange = GetTvsInRange(position);
                    if (tvsInRange.Any())
                    {
                        var tv = tvsInRange.First();
                        chart.DragTimeSeries(tv.Path.Data, tv, this);
                    }
                }
            }
        }
        private void container_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (HasValidState)
            {
                StopMoving();
            }
        }
        private void container_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (HasValidState && 
                e.Key.IsAlt() && (Mouse.LeftButton == MouseButtonState.Pressed))
            {
                StartMoving();
            }
        }
        private void container_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (HasValidState && e.Key.IsAlt())
            {
                StopMoving();
            }
        }
        private void container_MouseLeave(object sender, MouseEventArgs e)
        {
            if (HasValidState)
            {
                StopMoving();
            }
        }

        #region Extern Drop
        private void container_Drag(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;

            if (HasValidState && !IsSingleTs)
            {
                if (e.Data.GetDataPresent(typeof(List<string>)))
                {
                    var fullnames = (List<string>)e.Data.GetData(typeof(List<string>));
                    if ((fullnames != null) && fullnames.Any())
                    {
                        e.Effects = DragDropEffects.All;
                    }
                }
            }
            e.Handled = true;
        }
        private void container_Drop(object sender, DragEventArgs e)
        {
            if (HasValidState && !IsSingleTs)
            {
                var fullnames = (List<string>)e.Data.GetData(typeof(List<string>));
                // TODO: Implement in v2
throw new NotImplementedException("Drag and Drop not implemented!!!");
#if false
                var tss = new List<ITeaFileT>();
                try
                {
                    fullnames.ForEach(f => tss.Add(TimeSeriesTeaFile.OpenRead(f)));

                    var addedTvs = Add(tss);
                    chart.TapeView.Add(addedTvs.Select(tv => tv.TimeSeries));
                }
                catch (Exception ex)
                {
                    tss.ForEach(ts => ts.Dispose());

                    MessageBox.Show(String.Format(@"Failed to open TimeSeriesTeaFiles.{0}", ex), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
#endif
            }

            e.Handled = true;
        }
        #endregion
        #endregion

        #region fields
        private bool isInitialized;
        private bool isDisposed;

        private TeaTime.Chart.ChartControl chart;

        private List<TsVisualization> tvs;
        private List<TsVisualization> selectedTvs;
        private Dictionary<YScaler, YScalePanel> yScalePanels;
        private List<MarkerLine> timeMarkers;
        private List<MarkerLine> valueMarkers;
        private TimeSliceMarker timeSliceMarker;
        internal TimeSliceMarker TimeSliceMarker { get { return timeSliceMarker; } }
        
        private ChartPanelCursor cursor;
        
        #region Move Panel
        private MoveState moveState;
        private Point movePosition;
        #endregion
        #endregion

        #region embedded types
        private class ChartPanelCursor : ICursor, IDisposable
        {
            #region properties
            public CursorModes Mode
            {
                get
                {
                    return mode;
                }
                set
                {
                    if (mode != value)
                    {
                        if (parent.IsSingleTs && value == CursorModes.Region)
                        {
                            throw new NotSupportedException("Region selection is not supported in SingleTs mode.");
                        }

                        RemoveMarkers();
                        moveState = MoveState.Stopped;
                        mode = value;
                    }
                }
            }
            public bool Permanent
            {
                get
                {
                    return permanent;
                }
                set
                {
                    if (permanent != value)
                    {
                        permanent = value;
                        if (!permanent && (moveState == MoveState.Stopped))
                        {
                            RemoveMarkers();
                        }
                    }
                }
            }

            public bool IsVisible
            {
                get
                {
                    return markers.Any();
                }
            }
            public Point ActualPosition
            {
                get
                {
                    return markers.Last().ActualPosition;
                }
            }
            #endregion

            #region ctor / Dispose
            public ChartPanelCursor(ChartPanel parent, Canvas container)
            {
                Guard.ArgumentNotNull(parent, "parent");
                Guard.ArgumentNotNull(container, "container");

                InitializeStyles(container);

                this.parent = parent;
                this.container = container;
                this.markers = new List<MarkerCross>();
                this.mode = CursorModes.Point;
                this.moveState = MoveState.Stopped;

                container.PreviewKeyUp += container_PreviewKeyUp;
                container.PreviewMouseLeftButtonDown += container_PreviewMouseLeftButtonDown;
                container.PreviewMouseLeftButtonUp += container_PreviewMouseLeftButtonUp;
                container.PreviewMouseMove += container_PreviewMouseMove;
                container.MouseLeave += container_MouseLeave;
            }

            private static void InitializeStyles(Canvas container)
            {
                if (regionPathFill == null)
                {
                    regionPathFill = ((Brush)container.FindResource(VsBrushes.HighlightKey)).Clone();
                    regionPathFill.Opacity = 0.5;
                }
            }

            public void Dispose()
            {
                if (!isDisposed && !isDisposing)
                {
                    isDisposing = true;

                    try
                    {
                        container.PreviewKeyUp -= container_PreviewKeyUp;
                        container.PreviewMouseLeftButtonDown -= container_PreviewMouseLeftButtonDown;
                        container.PreviewMouseLeftButtonUp -= container_PreviewMouseLeftButtonUp;
                        container.PreviewMouseMove -= container_PreviewMouseMove;
                        container.MouseLeave -= container_MouseLeave;

                        moveState = MoveState.Stopped;

                        RemoveMarkers();
                        isDisposed = true;
                    }
                    finally
                    {
                        isDisposing = false;
                    }
                }
                else
                {
                    throw new InvalidOperationException("This ChartPanelCursor was already disposed or is currently disposing.");
                }
            }
            #endregion

            #region private methods
            private void SwitchRegionToPoint()
            {
                markers[1].Dispose();
                markers.RemoveAt(1);

                container.Children.Remove(regionPath);
                regionPath = null;

                Point position = Mouse.GetPosition(container);
                UpdateMarkers(0, position.X, position.Y);
                startPosition = position;
                Mode = CursorModes.Point;
            }
            private void SwitchPointToRegion()
            {
                markers.Add(new MarkerCross(container, false, false));
                regionPath = new Path();
                RenderOptions.SetEdgeMode(regionPath, EdgeMode.Aliased);
                regionPath.Fill = regionPathFill;
                container.Children.Add(regionPath);
                Mode = CursorModes.Region;
            }

            private void RemoveMarkers()
            {
                if (markers.Any())
                {
                    parent.RemoveCursorMarkers(this);

                    markers.ForEach(m => m.Dispose());
                    markers.Clear();
                }

                if (regionPath != null)
                {
                    container.Children.Remove(regionPath);
                    regionPath = null;
                }
            }

            private void UpdateMarkers(int index, double xPosition, double yPosition)
            {
                markers[index].Update(xPosition, yPosition);

                parent.UpdateCursorMarkers(this, xPosition, yPosition);
            }

            private void CursorReleased()
            {
                moveState = MoveState.Stopped;

                if (Mode == CursorModes.Region)
                {
                    SwitchRegionToPoint();
                }

                if (!permanent)
                {
                    RemoveMarkers();
                }
            }
            #endregion

            #region eventhandler
            private void container_PreviewKeyUp(object sender, KeyEventArgs e)
            {
                if (parent.isInitialized)
                {
                    if ((Mode == CursorModes.Region) && (e.Key.IsCtrl() || e.Key.IsShift()))
                    {
                        SwitchRegionToPoint();
                    }

                    if ((moveState == MoveState.Moving) && (e.Key == Key.Escape))
                    {
                        moveState = MoveState.Stopped;
                        RemoveMarkers();

                        if (permanent)
                        {
                            markers.Add(new MarkerCross(container, false, false));
                            parent.AddCursorMarkers(this);
                            Mode = CursorModes.Point;

                            UpdateMarkers(0, startPosition.X, startPosition.Y);
                        }
                    }

                    if (e.Key == Key.P)
                    {
                        Permanent = !permanent;
                    }
                }
            }

            private void container_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                if (parent.isInitialized)
                {
                    moveState = MoveState.Moving;
                    container.Focus();

                    Point position = e.GetPosition(container);
                    startPosition = position;

                    if (!markers.Any())
                    {
                        markers.Add(new MarkerCross(container, false, false));
                        parent.AddCursorMarkers(this);
                    }

                    Mode = CursorModes.Point;

                    UpdateMarkers(0, position.X, position.Y);
                }
            }
            private void container_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
            {
                if (parent.isInitialized)
                {
                    if (moveState == MoveState.Moving)
                    {
                        if (Mode == CursorModes.Region)
                        {
                            parent.CursorRegionSelected(markers[0].ActualPosition, markers[1].ActualPosition);
                        }
                        else
                        {
                            parent.CursorPointSelected(markers[0].ActualPosition);
                        }

                        CursorReleased();
                    }
                }
            }
            private void container_PreviewMouseMove(object sender, MouseEventArgs e)
            {
                if (parent.isInitialized)
                {
                    if (moveState == MoveState.Moving)
                    {
                        int updateIndex = 0;

                        Point position = e.GetPosition(container);
                        if ((Mode != CursorModes.Region) && 
                            !parent.IsSingleTs &&
                            (KeyboardUtility.IsShiftPressed || KeyboardUtility.IsCtrlPressed))
                        {
                            Vector diff = startPosition - position;

                            if (Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance &&
                                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
                            {
                                SwitchPointToRegion();
                            }
                        }

                        if (Mode == CursorModes.Region)
                        {
                            RectangleGeometry rect = new RectangleGeometry(new Rect(position, markers[0].ActualPosition));
                            regionPath.Data = rect;

                            updateIndex = 1;
                        }

                        UpdateMarkers(updateIndex, position.X, position.Y);
                    }
                }
            }

            private void container_MouseLeave(object sender, MouseEventArgs e)
            {
                if (parent.isInitialized)
                {
                    CursorReleased();
                }
            }
            #endregion

            #region fields
            private bool isDisposed;
            private bool isDisposing;

            private ChartPanel parent;
            private Canvas container;
            private List<MarkerCross> markers;
            private Path regionPath;

            private CursorModes mode;
            private MoveState moveState;
            private bool permanent;
            private Point startPosition;

            private static Brush regionPathFill = null;
            #endregion
        }

        private enum MoveState
        {
            Moving,
            Stopped
        }
        #endregion
    }
}