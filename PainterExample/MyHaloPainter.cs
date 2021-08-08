#if false // requires adapation to new painter 


// copyright discretelogics © 2011
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using TeaTime;
using TeaTime.API;
using TeaTime.Chart.Painters;
using TeaTime.Data;
using TeaTime.UI;

namespace ACME
{
    /// <summary>
    ///     Performant painter drawing a circle and a Text on every 20th slice.
    /// </summary>
    /// <remarks>
    ///     Explains additional Painter features, like Order, Image or Fill.
    /// </remarks>
    public class MyHaloPainter : SlicePainter
    {
        // Defines where the Painter is displayed in the Chart menu.
        // The lower the Order the more to the left it is displayed.
        // Built-in painters have an order of 1000-2000.
        // By default int.MaxValue and therefore added to the end.
        // Additionally DefaultPainters with lower Order are preffered against higher Order, 
        // but Custom Painters are always preffered against builtin Painters.
        private static string[] supportedEventTypes = new string[] { typeof(Event<OHLC>).GetLanguageName(), typeof(Event<OHLCV>).GetLanguageName()};

        #region Initialize

        // Prepare value getters for this specific TimeSeries

        private Func<int, double> closeGetter;
        private Func<int, double> highGetter;
        private BuiltinBarType itemType;

        private Func<int, double> lowGetter;

        private CultureInfo textCulture;
        private Typeface textTypeFace;

        protected override void Initialize()
        {
            string itemTypeName = tv.TimeSeries.Description.ItemDescription.ItemTypeName;
            if (itemTypeName == typeof(Event<OHLC>).GetLanguageName())
            {
                itemType = BuiltinBarType.OHLC;
                lowGetter = (int tsi) =>
                                {
                                    unsafe
                                    {
                                        return ((OHLC*) tv.TimeSeries.EventValueAt(tsi))->Low;
                                    }
                                };
                highGetter = (int tsi) =>
                                {
                                    unsafe
                                    {
                                        return ((OHLC*) tv.TimeSeries.EventValueAt(tsi))->High;
                                    }
                                };
                closeGetter = (int tsi) =>
                                {
                                    unsafe
                                    {
                                        return ((OHLC*) tv.TimeSeries.EventValueAt(tsi))->Close;
                                    }
                                };
            }
            else if (itemTypeName == typeof(Event<OHLCV>).GetLanguageName())
            {
                itemType = BuiltinBarType.OHLCV;
                lowGetter = (int tsi) =>
                                {
                                    unsafe
                                    {
                                        return ((OHLCV*) tv.TimeSeries.EventValueAt(tsi))->Low;
                                    }
                                };
                highGetter = (int tsi) =>
                                {
                                    unsafe
                                    {
                                        return ((OHLCV*) tv.TimeSeries.EventValueAt(tsi))->High;
                                    }
                                };
                closeGetter = (int tsi) =>
                                {
                                    unsafe
                                    {
                                        return ((OHLCV*) tv.TimeSeries.EventValueAt(tsi))->Close;
                                    }
                                };
            }

            textCulture = CultureInfo.CurrentCulture;
            textTypeFace = new Typeface("Segoe UI");
        }

        #endregion

        public override int Order
        {
            get { return 50; }
        }

        // Display text for this painter, used in the Chart menu tooltip.
        // By default "Painter".
        public override string Text
        {
            get { return "Halo-Painter"; }
        }

        // The image displayed in the Chart menu.
        public override ImageSource Image
        {
            get
            {
                var uri = new Uri("/ACME.PainterExamples;component/resources/halopainter.png", UriKind.Relative);
                return ImageCache.Instance.Get(uri);
            }
        }

        // This painter can draw multiple item types
        public override IEnumerable<string> SupportedEventTypes
        {
            get { return supportedEventTypes; }
        }

        // The minimum value at a specific index. Used for scaling.
        public override double GetMin(int tsi)
        {
            return lowGetter(tsi);
        }

        // The maximum value at a specific index. Used for scaling.
        public override double GetMax(int tsi)
        {
            return highGetter(tsi);
        }

        // The value displayed by the CloseMarker.
        public override double GetClose(int tsi)
        {
            return closeGetter(tsi);
        }

        // draws one specific slice
        protected override TimeSeriesDrawing DrawSlice(int tsi, double left, double width)
        {
            #region get values

            double low;
            double high;
            double close;

            unsafe
            {
                if (itemType == BuiltinBarType.OHLC)
                {
                    OHLC* ohlc = (OHLC*) tv.TimeSeries.EventValueAt(tsi);
                    low = ohlc->Low;
                    high = ohlc->High;
                    close = ohlc->Close;
                }
                else if (itemType == BuiltinBarType.OHLCV)
                {
                    OHLCV* ohlcv = (OHLCV*) tv.TimeSeries.EventValueAt(tsi);
                    low = ohlcv->Low;
                    high = ohlcv->High;
                    close = ohlcv->Close;
                }
                else
                {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "The ItemType {0} is not supported by this painter.", itemType));
                }
            }

            #endregion

            double yLow = GetYPosition(low);
            double yHigh = GetYPosition(high);
            double height = yLow - yHigh;

            // create geometry
            var haloGeo = new EllipseGeometry(new Rect(left, yHigh, width, height));
            if (tsi%20 == 0)
            {
                FormattedText text = new FormattedText(close.ToString(CultureInfo.CurrentCulture), textCulture,
                                                       FlowDirection.LeftToRight,
                                                       textTypeFace,
                                                       8, Brushes.DarkGray);
                var textGeo = text.BuildGeometry(new Point(left + width/2, yHigh + height/2));

                var combined = new GeometryGroup();
                combined.Children.Add(haloGeo);
                combined.Children.Add(textGeo);

                // create fill for this slice
                var fill = new LinearGradientBrush(tv.DrawingAttributes.Color, Colors.Transparent, new Point(0, yHigh/tv.YScaler.ActualHeight), new Point(0, yLow/tv.YScaler.ActualHeight));
                return new TimeSeriesDrawing(combined, fill);
            }
            else
            {
                return new TimeSeriesDrawing(haloGeo, null);
            }
        }
    }
}

#endif