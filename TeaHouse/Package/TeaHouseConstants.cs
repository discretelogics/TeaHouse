// Guids.cs
// MUST match guids.h
using System;

namespace TeaTime
{
    static class TeaHouseConstants
    {
        // package
        public const string guidTeaHousePackageString = "8645f529-f69e-45cd-8adc-68e5a024a4b0";

        // editors
        public const string guidTeaFileEditorFactoryString = "7e574ed2-97f6-4594-b7f3-a403f8056443";

        // command sets
        public const string guidTeaHouseViewCmdSetString = "b7e4fe5b-ed3c-4696-a930-89863c761bae";
        public const string guidTeaHouseToolsCmdSetString = "03E1813E-B55B-41AC-99A6-BEBB836EC75B";
        public const string guidTeaHouseHelpCmdSetString = "217D584C-BAB2-4980-B870-0F8D8D5FB912";

        // commands
        public const int idTeaHouseTreeCmd = 0x0102;
        public const int idImportCsvFileCmd = 0x0103;
        public const int idExportCsvFileCmd = 0x0104;
        public const int idShowAboutDialogCmd = 0x0102;

        public static readonly Guid guidTeaHousePackage = new Guid(guidTeaHousePackageString);
        public static readonly Guid guidTeaFileEditorFactory = new Guid(guidTeaFileEditorFactoryString);
        public static readonly Guid guidTeaHouseViewCmdSet = new Guid(guidTeaHouseViewCmdSetString);
        public static readonly Guid guidTeaHouseToolsCmdSet = new Guid(guidTeaHouseToolsCmdSetString);
        public static readonly Guid guidTeaHouseHelpCmdSet = new Guid(guidTeaHouseHelpCmdSetString);
    }
}