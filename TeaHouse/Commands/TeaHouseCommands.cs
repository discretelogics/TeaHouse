namespace TeaTime.Commands
{
    public static class TeaHouseCommands
    {
        public static readonly ViewTeaHouseExplorer ViewTeaHouseExplorer = new ViewTeaHouseExplorer();
        public static readonly ViewTeaHouseOptions ViewTeaHouseOptions = new ViewTeaHouseOptions();
        public static readonly ImportCsvCmd ImportCsvCmd = new ImportCsvCmd();
        public static readonly ExportCsvCmd ExportCsvCmd = new ExportCsvCmd();
        public static readonly ShowAboutDialogCmd ShowAboutDialogCmd = new ShowAboutDialogCmd();
    }
}
