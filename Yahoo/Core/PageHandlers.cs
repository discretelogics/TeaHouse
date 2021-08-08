// copyright discretelogics 2012.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows.Forms;
using NLog;
using TeaTime.VSX;

namespace TeaTime.Yahoo
{
    interface IYahooPageHandler
    {
        void OnDocumentComplete(HtmlDocument doc);
    }

    class SingleSymbolPageHandler : IYahooPageHandler
    {
        static Logger logger = LogManager.GetCurrentClassLogger();

        public void OnDocumentComplete(HtmlDocument doc)
        {
            HtmlElement div = doc.GetElementById("yfi_investing_content");
            if (div == null)
            {
                div = doc.Body; // if yahoo changes its webpage, we search from the body
            }
            if (div == null) return; // simply exit
            HtmlElement title = div.GetElementByClassName("title");
            if (!this.buttonAdded && (title != null))
            {
                HtmlElement button = doc.CreateElement("Button");
                if (button == null) return;

                button.InnerText = "Add to TeaHouse";
                button.Id = "teadown";
                title.AppendChild(button);
                button.Click += this.AddToTeaHouse;
                button.Style = "background-color:orange;border:solid 1px maroon;margin-left:6px";

                // name
                HtmlElement h2 = title.GetByTagName("H2");
                this.name = h2.InnerText;

                // symbol
                string s = doc.Url.Query;
                s = HttpUtility.UrlDecode(s);
                var re = new Regex(@"s=(.*)");
                Match mc = re.Match(s);
                this.symbol = mc.Groups[1].ToString();

                this.buttonAdded = true;
            }
        }

        void AddToTeaHouse(object sender, HtmlElementEventArgs e)
        {
            try
            {                
                Downloader.ScheduleSymbolUpdate(this.symbol, this.name);
            }
            catch (Exception ex)
            {
                YahooPackage.Instance.WriteError("Error while downloading: " + ex.Message);
                logger.Error(ex);
            }
        }

        #region fields

        bool buttonAdded;
        string name;
        string symbol;

        #endregion
    }

    class ComponentsPageHandler : IYahooPageHandler
    {
        static Logger logger = LogManager.GetCurrentClassLogger();

        bool buttonAdded;
        List<SymbolAndName> symbols;

        public void OnDocumentComplete(HtmlDocument doc)
        {
            HtmlElement tableparent = doc.GetElementById("yfncsumtab");
            if (tableparent == null)
            {
                tableparent = doc.Body; // if yahoo changes its webpage, we search from the body
            }
            HtmlElement table = tableparent.GetElementByClassName("yfnc_mod_table_title1");
            if (!this.buttonAdded)
            {
                dynamic t = table.DomElement;
                var cell = t.cells[0];
                HtmlElement button = doc.CreateElement("Button");
                if (button == null) return;

                table = table.NextSibling;
                HtmlElementCollection atags = table.GetElementsByTagName("a");
                this.symbols = new List<SymbolAndName>();
                foreach (HtmlElement atag in atags)
                {
                    var sn = new SymbolAndName();

                    string href = atag.GetAttribute("href");
                    // "http://finance.yahoo.com/q?s=AACC"
                    var u = new Uri(href);
                    var arr = u.Query.Split('=');
                    if (arr.Length == 2)
                    {
                        sn.Symbol = arr[1];
                    }

                    var p = atag.Parent;
                    while (!p.TagName.Equals("td", StringComparison.InvariantCultureIgnoreCase))
                        p = p.Parent;
                    var nameCell = p.NextSibling;
                    sn.Name = nameCell.InnerText;

                    this.symbols.Add(sn);
                }

                button.InnerText = "Add {0} Components from this page to TeaHouse".Formatted(this.symbols.Count);
                button.Id = "teadown";
                button.Style = "background-color:orange;border:solid 1px maroon;margin-left:6px";
                cell.insertAdjacentElement("beforeEnd", button.DomElement);
                
                button.Click += this.AddToTeaHouse;

                this.buttonAdded = true;
            }
        }

        void AddToTeaHouse(object sender, HtmlElementEventArgs e)
        {
            try
            {
                this.symbols.ForEach(sn => Downloader.ScheduleSymbolUpdate(sn.Symbol, sn.Name));
            }
            catch (Exception ex)
            {
                YahooPackage.Instance.WriteError("Error while downloading: " + ex.Message);
                logger.Error(ex);
            }
        }
    }

    class SymbolAndName
    {
        public string Name { get; set; }
        public string Symbol { get; set; }
    }
}
