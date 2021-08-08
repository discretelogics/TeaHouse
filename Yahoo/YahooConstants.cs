using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaTime.Yahoo
{
    class YahooConstants
    {
        // package
        public const string guidYahooPkgString = "7dd6df24-1a74-4731-a23b-ab51f5109171";

        // editors

        // command sets
        public const string guidYahooViewCmdSetString = "9354c932-0481-4a7f-9a17-7dd2ad88d1bf";
        public const string guidYahooToolsCmdSetString = "f503b6c4-5f14-49b3-8029-f03dfa55f209";

        // commands
        public const int idShowYahooBrowserCmd = 0x102;
        public const int idUpdateYahooData = 0x103;

        public static readonly Guid guidYahooViewCmdSet = new Guid(guidYahooViewCmdSetString);
        public static readonly Guid guidYahooToolsCmdSet = new Guid(guidYahooToolsCmdSetString);

        public const string TeaFileExtension = ".tea";
    }
}
