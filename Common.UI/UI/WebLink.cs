using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace TeaTime.UI
{
    class WebLink : Hyperlink
    {
        public WebLink()
        {
            base.RequestNavigate += this.WebRequestNavigate;
        }

        void WebRequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.AbsoluteUri);
            e.Handled = true;
        }
    }
}
