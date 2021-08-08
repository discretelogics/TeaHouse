// copyright discretelogics © 2011

using System;
using System.IO;
using System.Linq;
using TeaTime.VSX;

namespace TeaTime.Yahoo.Commands
{
	class UpdateYahooData : CommandBase<YahooPackage>
	{
		public override void Execute(object parameter)
		{
            if (!Directory.Exists(this.package.Options.DownloadDirectory))
            {
                this.package.WriteMessage("Directory {0} does not exist. No updates are executed.");
                return;
            }
		    Downloader.UpdateTeaFolder(this.package.Options.DownloadDirectory);
		}
	}
}