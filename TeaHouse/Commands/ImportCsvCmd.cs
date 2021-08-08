// copyright discretelogics © 2011
using System.Runtime.InteropServices;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using TeaTime.Data;
using TeaTime.UI;
using TeaTime.VSX;

namespace TeaTime.Commands
{
    public class ImportCsvCmd : CommandBase<TeaHousePackage>
	{
		public override void Execute(object parameter)
		{
			ToolWindowPane window = package.FindToolWindow(typeof (CSVImportPane), 0, true);
			if ((window == null) || (window.Frame == null))
			{
				throw new COMException("Failed to create CSVImport window.");
			}
			IVsWindowFrame windowFrame = (IVsWindowFrame) window.Frame;
			ErrorHandler.ThrowOnFailure(windowFrame.Show());
		}
	}
}