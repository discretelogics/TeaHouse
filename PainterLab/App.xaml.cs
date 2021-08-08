using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace TeaTime.PainterLab
{
    public partial class App : Application
    {
        public static StartupArgs StartupArgs { get; private set; }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            StartupArgs = CommandLineArgsManager<StartupArgs>.FromArgs(e.Args);
        }
    }
}