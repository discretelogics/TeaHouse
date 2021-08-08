using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TeaTime.CommonUI
{
    public class Constants
    {
        public const string TeaFileExtension = ".tea";
        public const string TeaFileSearchPattern = "*" + TeaFileExtension;
        public const string TeaFileFilter = "Tea Files (*.tea)|*.tea";

        public static DependencyProperty Update; // this is the license flag ! if not null, then license is valid
    }
}
