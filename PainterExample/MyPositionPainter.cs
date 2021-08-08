#if false // requires adapation to new painter 

// copyright discretelogics © 2011

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using TeaTime;
using TeaTime.API;
using TeaTime.Data;
using TeaTime.UI;
using System.Diagnostics.CodeAnalysis;

namespace ACME
{
	/// <summary>
	///     Performant Painter for the custom event <see cref = "Position" />.
	/// </summary>
	/// <remarks>
	///     Shows the felxibility of Painters, including your custom fills.
	/// </remarks>
	public unsafe class MyPositionPainter : Painter
	{
		// Support the Position events set this painter as the default painter for Position events.
		private static string[] eventTypes = new string[] {typeof (Position).Name};

		public override IEnumerable<string> SupportedEventTypes
		{
			get { return eventTypes; }
		}

		public override IEnumerable<string> DefaultEventTypes
		{
			get { return eventTypes; }
		}

		// The display Text of this Painter
		public override string Text
		{
			get { return "My Position-Painter"; }
		}

		// Define an image to be displayed in the menu
		public override ImageSource Image
		{
			get
			{
                var uri = new Uri("/ACME.PainterExamples;component/resources/positionpainter.png", UriKind.Relative);
				return ImageCache.Instance.Get(uri);
			}
		}

		// ALways display this Painter first in the menu
		public override int Order
		{
			get { return 0; }
		}

		// Draws the ProfitLoss as a line across the whole screen and adds markers for Buy/Sell.
		// Markers below 0 will be drawn in red, above 0 in green.
		public override TimeSeriesDrawing Draw(Range tapeIndexRange, double panelWidth)
		{
			// This geometry is the container for all other geometries we create
			var geometryGroup = new GeometryGroup();
			// this geometry will hold the geometry to draw the ProfitLoss
			StreamGeometry profitLossGeo = new StreamGeometry();
			using (StreamGeometryContext profitLossGeoContext = profitLossGeo.Open())
			{
				bool figureBegun = false;
				double rightX = tapeView.PositionAt(tapeIndexRange.Start) + tapeView.WidthAt(tapeIndexRange.Start);
				double actualX = double.MaxValue;

				int tapeIndex = tapeIndexRange.Start;
				int endTapeIndex = tapeIndexRange.End;
				int tsi = tapeView.GetTimeSeriesStart(tv.TimeSeries, tapeIndex);
				DateTime leftTime = tapeView.TimeAt(tapeIndexRange.End);
				DateTime time = tv.TimeSeries.TimeAt(tsi); //  the time of the current TimeSeries Item

				//  we draw from right to left
				while (time >= leftTime)
				{
					// absolute left on the complete tape
					double left = tapeView.PositionAt(tapeIndex);
					// width of this slice
					double width = tapeView.WidthAt(tapeIndex);
					// draw from middle of slice
					double newX = left + width/2;
					// don't draw in sub-pixel-regions
					if ((actualX - newX) >= 1)
					{
						actualX = newX;
						// get the item to draw
						Position* item = (Position*) tv.TimeSeries.EventValueAt(tsi);
						// get screen positions
						double xMiddlePosition = panelWidth - (rightX - actualX);
						double xLeftPosition = panelWidth - (rightX - left);
						double yPosition = GetYPosition(item->ProfitLoss);
						// draw ProfitLoss
						if (!figureBegun)
						{
							profitLossGeoContext.BeginFigure(new Point(xMiddlePosition, yPosition), false, false);
							figureBegun = true;
						}
						else
						{
							profitLossGeoContext.LineTo(new Point(xMiddlePosition, yPosition), true, false);
						}

						// draw the buy marker
						if (item->BuySell > 0)
						{
							double yTopPosition = yPosition - 6;
							StreamGeometry buyGeo = new StreamGeometry();
							using (StreamGeometryContext buyGeoContext = buyGeo.Open())
							{
								// isFilled=true to ensure marker is filled
								buyGeoContext.BeginFigure(new Point(xLeftPosition, yTopPosition), true, true);
								buyGeoContext.LineTo(new Point(xMiddlePosition, yPosition), true, false);
								buyGeoContext.LineTo(new Point(xLeftPosition + width, yTopPosition), true, false);
							}
							geometryGroup.Children.Add(buyGeo);
						}
						// draw the sell marker
						if (item->BuySell < 0)
						{
							double yBottomPosition = yPosition + 6;
							StreamGeometry sellGeo = new StreamGeometry();
							using (StreamGeometryContext sellGeoContext = sellGeo.Open())
							{
								// isFilled=true to ensure marker is filled
								sellGeoContext.BeginFigure(new Point(xLeftPosition, yBottomPosition), true, true);
								sellGeoContext.LineTo(new Point(xMiddlePosition, yPosition), true, false);
								sellGeoContext.LineTo(new Point(xLeftPosition + width, yBottomPosition), true, false);
							}
							geometryGroup.Children.Add(sellGeo);
						}
					}

					// decrease the timeSeriesIndex
					tsi--;
					if (tsi < 0) break;

					// get the TapeIndex for the designated tsi
					time = tv.TimeSeries.TimeAt(tsi);
					tapeIndex = tapeView.IndexAt(time, new Range(tapeIndex, endTapeIndex));
				}
			}
			geometryGroup.Children.Add(profitLossGeo);

			// create the fill splitted at 0
			double zero = GetYPosition(0)/tv.YScaler.ActualHeight;
			GradientStopCollection stops = new GradientStopCollection();
			stops.Add(new GradientStop(Colors.Green, zero));
			stops.Add(new GradientStop(Colors.Red, zero));
			LinearGradientBrush fill = new LinearGradientBrush(stops, new Point(0, 0), new Point(0, 1));

			return new TimeSeriesDrawing(geometryGroup, fill);
		}

		// The minimum value at a specific index. Used for scaling.
		public override double GetMin(int tsi)
		{
			// 10% additional padding
			double pl = ((Position*) tv.TimeSeries.EventValueAt(tsi))->ProfitLoss;
			if (pl > 0)
			{
				return pl*0.9;
			}
			else
			{
				return pl*1.1;
			}
		}

		// The maximum value at a specific index. Used for scaling.
		public override double GetMax(int tsi)
		{
			// 10% additional padding
			double pl = ((Position*) tv.TimeSeries.EventValueAt(tsi))->ProfitLoss;
			if (pl > 0)
			{
				return pl*1.1;
			}
			else
			{
				return pl*0.9;
			}
		}

		// The value displayed by the CloseMarker.
		public override double GetClose(int tsi)
		{
			return ((Position*) tv.TimeSeries.EventValueAt(tsi))->ProfitLoss;
		}
	}

	/// <summary>
	/// Our custom event holding a positions ProfitLoss and Buy/Sell markers.
	/// </summary>
	public struct Position : IEvent
	{
		public double BuySell;
		public double ProfitLoss;
		public DateTime Time;

#region IEvent Members

		public DateTime GetTime()
		{
			return Time;
		}

#endregion

#region equality - for structs that might get compared for equality, overriding equality members oimproves performance.

		public bool Equals(Position other)
		{
			return other.BuySell.Equals(BuySell) && other.ProfitLoss.Equals(ProfitLoss) && other.Time.Equals(Time);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (obj.GetType() != typeof (Position)) return false;
			return Equals((Position) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = BuySell.GetHashCode();
				result = (result*397) ^ ProfitLoss.GetHashCode();
				result = (result*397) ^ Time.GetHashCode();
				return result;
			}
		}

		public static bool operator ==(Position left, Position right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Position left, Position right)
		{
			return !left.Equals(right);
		}

#endregion
	}
}

#endif