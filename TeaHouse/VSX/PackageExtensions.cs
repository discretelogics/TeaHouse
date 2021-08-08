using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace TeaTime.VSX
{
	static class PackageExtensions
	{
		static Logger logger = LogManager.GetCurrentClassLogger();

		static Guid teaTimeOutputPane = new Guid("{6D599CC6-2A96-42EA-9D6D-FC6D67CE3D8A}");

		#region messages

		public static void WriteError(this Package package, string message)
		{
			WriteError(package, message, null);
		}

		public static void WriteError(this Package package, string message, Exception ex)
		{
            WriteMessage(package, true, message + Environment.NewLine + ex);
		}

		static bool inOutputPaneCall;

        public static void WriteMessage(this Package package, bool activate, string format, params object[] args)
		{
			if(inOutputPaneCall)
			{
				logger.Warn("OutputWindow reentrance: message is written to log instead: " + format, args);
				return;
			}
			inOutputPaneCall = true;
			string message = format.Formatted(args);
			try
			{
                // get the pane - actually we might just use the general pane, should we?
				var op = package.GetOutputPane(teaTimeOutputPane, "TeaTime");
				
                // write the string
                op.OutputStringThreadSafe(message + Environment.NewLine);
			    
                // activate the teatime pane - this does not yet display the whole Output Window(!)
                op.Activate();

                if(activate)
                {
                    // we ensure the output window is shown by this:
			        IServiceProvider sp = package;
                    DTE dte = sp.GetService(typeof(SDTE)) as DTE;
			        if (dte != null) dte.ExecuteCommand("View.Output", string.Empty);
                }
			}
			catch (Exception)
			{
				LogManager.GetCurrentClassLogger().Error("Failed to write message into output pane: " + message);
			}
			finally
			{
				inOutputPaneCall = false;
			}
        }

        #endregion

	    public static T GetToolWindow<T>(this Package package) where T: ToolWindowPane
	    {
            T toolWindow = (T)package.FindToolWindow(typeof(T), 0, true);
            if ((toolWindow == null) || (toolWindow.Frame == null))
            {
                throw new COMException("Failed to create CSVExport window.");
            }
	        return toolWindow;
	    }

	    public static void Show<T>(this T toolWindow) where T : ToolWindowPane
	    {
            IVsWindowFrame windowFrame = (IVsWindowFrame)toolWindow.Frame;
            ErrorHandler.ThrowOnFailure(windowFrame.Show());
	    }
    }    
}
