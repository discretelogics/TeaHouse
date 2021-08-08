using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using System.Windows.Forms;

namespace TeaTime.Yahoo
{
    [Guid("798e36d4-4305-4ed4-ac18-21d9edf5968a")]
    public class YahooBrowserPane : ToolWindowPane
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public YahooBrowserPane() : base(null)
    	{
    		this.Caption = "Yahoo Finance";
    		this.BitmapResourceID = 303; // tbd - ask wuni
    		this.BitmapIndex = 0;

    		this.Content = new YahooBrowser();
    	}
    }
}
