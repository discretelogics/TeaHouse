using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Globalization;
using System.Windows.Threading;
using TeaTime.Base;

namespace TeaTime
{
	public static class UIExtensions
	{
		#region UIElements
		public static Size ActualSize(this FrameworkElement element)
		{
			return new Size(element.ActualWidth, element.ActualHeight);
		}
		public static Size Measure(this UIElement element)
		{
			element.Measure(InfiniteSize);
			return element.DesiredSize;
		}
		public static Size InfiniteSize = new Size(double.PositiveInfinity, double.PositiveInfinity);

		#endregion

		#region TreeView
		/// <summary>
		/// Walks the tree items to find the node corresponding with
		/// the given item, then sets it to be selected.
		/// </summary>
		/// <param name="treeView">The tree view to set the selected
		/// item on</param>
		/// <param name="item">The item to be selected</param>
		/// <returns><c>true</c> if the item was found and set to be
		/// selected</returns>
		public static bool SetSelectedItem(this TreeView treeView, object item)
		{
			return SetSelectedItemInternal(treeView, item);
		}
		private static bool SetSelectedItemInternal(ItemsControl parent, object item)
		{
			if (parent == null || item == null)
			{
				return false;
			}

			var childNode = parent.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

			if (childNode != null)
			{
				childNode.Focus();
				childNode.IsSelected = true;

				var parentNode = parent as TreeViewItem;
				if ((parentNode != null) && !parentNode.IsExpanded)
				{
					parentNode.IsExpanded = true;
				}

				return true;
			}

			foreach (object childItem in parent.Items)
			{
				var childControl = parent.ItemContainerGenerator.ContainerFromItem(childItem) as ItemsControl;

				if (SetSelectedItemInternal(childControl, item))
				{
					return true;
				}
			}

			return false;
		}

		public static TreeViewItem FindTreeViewItem(this TreeView treeView, object item)
		{
			return FindTreeViewItemInternal(treeView, item);
		}
		private static TreeViewItem FindTreeViewItemInternal(ItemsControl parent, object item)
		{
			if (parent == null || item == null)
			{
				return null;
			}

			var childNode = parent.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

			if (childNode != null)
			{
				return childNode;
			}

			foreach (object childItem in parent.Items)
			{
				var childControl = parent.ItemContainerGenerator.ContainerFromItem(childItem) as ItemsControl;

				childNode = FindTreeViewItemInternal(childControl, item);
				if (childNode != null)
				{
					return childNode;
				}
			}

			return null;
		}

		public static T FindChild<T>(this FrameworkElement element, string name) where T : FrameworkElement
		{
			for (int i = 0; i <
				VisualTreeHelper.GetChildrenCount(element); i++)
			{
				var child = (FrameworkElement)VisualTreeHelper.GetChild(element, i);

				if (child != null)
				{
					T childT = child as T;
					if ((childT != null) && (childT.Name == name))
					{
						return childT;
					}

					T descendent = child.FindChild<T>(name);
					if (descendent != null)
					{
						return descendent;
					}
				}
			}

			return null;
		}
		#endregion

        #region ListBox
        public static Point GetSelectedItemLocation(this ListBox list)
        {
            var container = list.ItemContainerGenerator.ContainerFromIndex(list.SelectedIndex);
            if (container == null) return new Point(0, 0);
            var itemElement = VisualTreeHelper.GetChild(container, 0) as FrameworkElement;
            if (itemElement == null) return new Point(0, 0);
            var location = itemElement.TranslatePoint(new Point(0, 0), list);
            location.Offset(1,1);
            return location;
        }
        #endregion

        #region Datetime
        static UIExtensions()
        {
            var format = CultureInfo.CurrentUICulture.DateTimeFormat;
            DateFormat = SettingsManager.Instance.Read("Chart", "DateFormat", () => new Setting<string>("dd" + format.DateSeparator + "MM")).Value;
            MonthAndYearFormat = SettingsManager.Instance.Read("Chart", "MonthAndYearFormat", () => new Setting<string>("MMM yy")).Value;
            MonthFormat = SettingsManager.Instance.Read("Chart", "MonthFormat", () => new Setting<string>("MMM")).Value;
            YearFormat = SettingsManager.Instance.Read("Chart", "YearFormat", () => new Setting<string>("yyyy")).Value;
        }

	    static readonly string DateFormat;
		public static string ToDateDisplayString(this DateTime value)
		{
            return value.ToString(DateFormat);
		}
        static readonly string MonthAndYearFormat;
		public static string ToMonthAndYearDisplayString(this DateTime value)
		{
            return value.ToString(MonthAndYearFormat);
		}
        static readonly string MonthFormat;
		public static string ToMonthDisplayString(this DateTime value)
		{
            return value.ToString(MonthFormat);
		}
        static readonly string YearFormat;
		public static string ToYearDisplayString(this DateTime value)
		{
            return value.ToString(YearFormat);
		}

		public static DateTime MonthDate(this DateTime value)
		{
			return value.Date.AddDays(-(value.Day - 1));
		}
		public static DateTime YearDate(this DateTime value)
		{
			return value.Date.AddMonths(-(value.Month - 1)).MonthDate();
		}
		#endregion

		#region Path
		public static void ApplyDrawingAttributes(this Path path, DrawingAttributes da)
		{
			Guard.ArgumentNotNull(path, "path");
            Guard.ArgumentNotNull(da, "da");

			path.Stroke = new SolidColorBrush(da.Color);
			path.StrokeThickness = da.Height;
		}
		#endregion
        
		#region UI Hierarchy
		public static IEnumerable<T> GetLogicalChildren<T>(this DependencyObject dpo, bool recursive)
		{
			if (dpo == null) yield break;

			var children = LogicalTreeHelper.GetChildren(dpo);
			foreach (var child in children)
			{
				if (child is T) yield return (T) child;

				if (recursive)
				{
					var cdpo = child as DependencyObject;
					foreach (T childchild in cdpo.GetLogicalChildren<T>(recursive))
					{
						yield return childchild;
					}
				}
			}
		}

        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the
        /// queried item.</param>
        /// <returns>The first parent item that matches the submitted
        /// type parameter. If not matching item can be found, a null
        /// reference is being returned.</returns>
        public static T TryFindParent<T>(this DependencyObject child)
            where T : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = GetParentObject(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            T parent = parentObject as T;
            if (parent != null)
            {
                return parent;
            }
            else
            {
                //use recursion to proceed with next level
                return TryFindParent<T>(parentObject);
            }
        }

        /// <summary>
        /// This method is an alternative to WPF's
        /// <see cref="VisualTreeHelper.GetParent"/> method, which also
        /// supports content elements. Keep in mind that for content element,
        /// this method falls back to the logical tree of the element!
        /// </summary>
        /// <param name="child">The item to be processed.</param>
        /// <returns>The submitted item's parent, if available. Otherwise
        /// null.</returns>
        public static DependencyObject GetParentObject(this DependencyObject child)
        {
            if (child == null) return null;

            //handle content elements separately
            ContentElement contentElement = child as ContentElement;
            if (contentElement != null)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                FrameworkContentElement fce = contentElement as FrameworkContentElement;
                return fce != null ? fce.Parent : null;
            }

            //also try searching for parent in framework elements (such as DockPanel, etc)
            FrameworkElement frameworkElement = child as FrameworkElement;
            if (frameworkElement != null)
            {
                DependencyObject parent = frameworkElement.Parent;
                if (parent != null) return parent;
            }

            //if it's not a ContentElement/FrameworkElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
		}
		#endregion

        #region TextBox
        public static double MeasureDesiredWidth(this TextBox tb, string text)
        {
            if (String.IsNullOrEmpty(text))
                return 0;

            Typeface typeface = new Typeface(tb.FontFamily, tb.FontStyle, tb.FontWeight, tb.FontStretch);
            GlyphTypeface glyphTypeface;

            if (!typeface.TryGetGlyphTypeface(out glyphTypeface))
            {
                return new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, tb.FontSize, Brushes.Black).Width;
            }

            double totalWidth = 0;

            for (int n = 0; n < text.Length; n++)
            {
                ushort glyphIndex = glyphTypeface.CharacterToGlyphMap[text[n]];
                double width = glyphTypeface.AdvanceWidths[glyphIndex] * tb.FontSize;
                totalWidth += width;
            }

            return totalWidth;
        }
        #endregion
    }
}