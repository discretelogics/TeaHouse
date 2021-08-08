using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using mshtml;

namespace TeaTime.UI
{
    public partial class FallbackWebBrowser : UserControl
    {
        #region properties
        public static readonly DependencyProperty SourceProperty = DependencyProperty<FallbackWebBrowser, Uri>.Register("Source", null, SourceChanged);
        public static readonly DependencyProperty FallbackSourceProperty = DependencyProperty<FallbackWebBrowser, Uri>.Register("FallbackSource", null, FallbackSourceChanged);

        public Uri Source
        {
            get { return (Uri)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }
        public Uri FallbackSource
        {
            get { return (Uri)GetValue(FallbackSourceProperty); }
            set { SetValue(FallbackSourceProperty, value); }
        }
        #endregion

        #region ctor
        public FallbackWebBrowser()
        {
            InitializeComponent();
        }
        #endregion

        #region events
        private static void SourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var owb = (FallbackWebBrowser)sender;
            if (owb.IsLoaded && !owb.isOffline)
            {
                owb.webBrowser.Source = (Uri)e.NewValue;
            }
        }
        private static void FallbackSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var owb = (FallbackWebBrowser)sender;
            if (owb.IsLoaded && owb.isOffline)
            {
                owb.webBrowser.Source = (Uri)e.NewValue;
            }
        }


        private void webBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            if (webBrowser.Source != null)
            {
                string readyState = ((HTMLDocument)(webBrowser.Document)).readyState;
                string url = ((HTMLDocument)(webBrowser.Document)).url;
                string title = ((HTMLDocument)(webBrowser.Document)).title;
                if (String.Compare(readyState, "complete", true) != 0 ||
                    String.IsNullOrEmpty(url) ||
                    url.StartsWith("res:") ||
                    String.Compare(title, "The resource cannot be found.", true) == 0)
                {
                    isOffline = true;
                    if (FallbackSource != null)
                    {
                        var fallbackContent = Application.GetResourceStream(FallbackSource);
                        webBrowser.NavigateToStream(fallbackContent.Stream);
                    }
                }
                else
                {
                    isOffline = false;
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            webBrowser.Source = Source;
        } 
        #endregion

        #region fields
        private bool isOffline; 
        #endregion
    }
}
