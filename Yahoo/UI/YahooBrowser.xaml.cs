// copyright discretelogics © 2011

using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using UserControl = System.Windows.Controls.UserControl;

namespace TeaTime.Yahoo
{
	public partial class YahooBrowser : UserControl
	{
		public System.Windows.Forms.WebBrowser WebBrowser
		{
			get { return this.browser; }
		}

		public YahooBrowser()
		{
			InitializeComponent();
		}

        protected override void OnInitialized(System.EventArgs e)
        {
            base.OnInitialized(e);
            WebBrowser.ScriptErrorsSuppressed = true;
            WebBrowser.DocumentCompleted += this.DocumentCompleted;
            WebBrowser.Navigate("http://finance.yahoo.com");
            
        }

	    void DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url.Host != "finance.yahoo.com") return; // do not listen on ad pages and alike

            Trace.WriteLine(e.Url.AbsoluteUri);

            pageHandler = null;
            if (WebBrowser.Document == null) return;

            if (e.Url.Query.EndsWith("+Components"))
            {
                pageHandler = new ComponentsPageHandler();
            }
            else if (e.Url.Query.StartsWith("?s="))
            {
                pageHandler = new SingleSymbolPageHandler();
            }
            else
            {
                // pageHandler = new FinanceHomePageHandler(); // yahoo has made the page now white, so no need no more
            }
            if (pageHandler != null)
            {
                pageHandler.OnDocumentComplete(WebBrowser.Document);
            }
        }

        private IYahooPageHandler pageHandler;
	}

    static class LocalExtensions
    {
        public static HtmlElement GetElementByClassName(this HtmlElement e, string className)
        {
            foreach (HtmlElement c in e.Children)
            {
                var a = c.GetAttribute("className");
                if (a == className) return c;

                var cc = c.GetElementByClassName(className);
                if (cc != null) return cc;
            }
            return null;
        }

        public static HtmlElement GetByTagName(this HtmlElement e, string tagName)
        {
            return e.GetElementsByTagName(tagName).OfType<HtmlElement>().First();
        }
    }
}