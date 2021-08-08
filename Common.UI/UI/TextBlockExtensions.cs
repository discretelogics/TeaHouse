using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TeaTime.UI
{
    /// <summary>
    /// Attached property provider which adds the read-only attached property
    /// <c>TextBlockExtensions.IsTextTrimmed</c> to the framework's <see cref="TextBlock"/> control.
    /// </summary>
    /// <remarks>
    /// http://tranxcoder.wordpress.com/2008/10/12/customizing-lookful-wpf-controls-take-2/"
    /// </remarks>
    public class TextBlockExtensions
    {
        static TextBlockExtensions()
        {
            // Register for the SizeChanged event on all TextBlocks, even if the event was handled.
            EventManager.RegisterClassHandler(
                typeof(TextBlock),
                FrameworkElement.SizeChangedEvent,
                new SizeChangedEventHandler(OnTextBlockSizeChanged),
                true);
        }

        /// <summary>
        /// Key returned upon registering the read-only attached property <c>IsTextTrimmed</c>.
        /// </summary>
        public static readonly DependencyPropertyKey IsTextTrimmedKey = DependencyProperty.RegisterAttachedReadOnly(
            "IsTextTrimmed",
            typeof(bool),
            typeof(TextBlockExtensions),
            new PropertyMetadata(false));    // defaults to false

        /// <summary>
        /// Identifier associated with the read-only attached property <c>IsTextTrimmed</c>.
        /// </summary>
        public static readonly DependencyProperty IsTextTrimmedProperty = IsTextTrimmedKey.DependencyProperty;

        /// <summary>
        /// Returns the current effective value of the IsTextTrimmed attached property.
        /// </summary>
        /// <remarks>Invoked automatically by the framework when databound.</remarks>
        /// <param name="target"><see cref="TextBlock"/> to evaluate</param>
        /// <returns>Effective value of the IsTextTrimmed attached property</returns>
        [AttachedPropertyBrowsableForType(typeof(TextBlock))]
        public static Boolean GetIsTextTrimmed(TextBlock target)
        {
            return (Boolean)target.GetValue(IsTextTrimmedProperty);
        }

        /// <summary>
        /// Event handler for TextBlock's SizeChanged routed event. Triggers evaluation of the
        /// IsTextTrimmed attached property.
        /// </summary>
        /// <param name="sender">Object where the event handler is attached</param>
        /// <param name="e">Event data</param>
        public static void OnTextBlockSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var textBlock = sender as TextBlock;
            if (null == textBlock)
            {
                return;
            }

            if (TextTrimming.None == textBlock.TextTrimming)
            {
                SetIsTextTrimmed(textBlock, false);
            }
            else
            {
                SetIsTextTrimmed(textBlock, CalculateIsTextTrimmed(textBlock));
            }
        }

        /// <summary>
        /// Sets the instance value of read-only dependency property <see cref="IsTextTrimmed"/>.
        /// </summary>
        /// <param name="target">Associated <see cref="TextBlock"/> instance</param>
        /// <param name="value">New value for IsTextTrimmed</param>
        private static void SetIsTextTrimmed(TextBlock target, Boolean value)
        {
            target.SetValue(IsTextTrimmedKey, value);
        }

        /// <summary>
        /// Determines whether or not the text in <paramref name="textBlock"/> is currently being
        /// trimmed due to width or height constraints.
        /// </summary>
        /// <remarks>Does not work properly when TextWrapping is set to WrapWithOverflow.</remarks>
        /// <param name="textBlock"><see cref="TextBlock"/> to evaluate</param>
        /// <returns><c>true</c> if the text is currently being trimmed; otherwise <c>false</c></returns>
        private static bool CalculateIsTextTrimmed(TextBlock textBlock)
        {
            if (!textBlock.IsArrangeValid)
            {
                return GetIsTextTrimmed(textBlock);
            }

            Typeface typeface = new Typeface(
                textBlock.FontFamily,
                textBlock.FontStyle,
                textBlock.FontWeight,
                textBlock.FontStretch);

            // FormattedText is used to measure the whole width of the text held up by TextBlock container
            FormattedText formattedText = new FormattedText(
                textBlock.Text,
                System.Threading.Thread.CurrentThread.CurrentCulture,
                textBlock.FlowDirection,
                typeface,
                textBlock.FontSize,
                textBlock.Foreground);

            return formattedText.Width > textBlock.ActualWidth;
        }
    }
}
