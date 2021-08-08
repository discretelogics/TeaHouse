// copyright discretelogics 2012.

using TeaTime.UI;
using TeaTime.VSX;

namespace TeaTime.Commands
{
    public class ExportCsvCmd : CommandBase<TeaHousePackage>
    {
        public override void Execute(object parameter = null)
        {
            // show TeaHouse Tree
            TeaHouseCommands.ViewTeaHouseExplorer.Execute();

            // show Export ToolWindow
            var toolWindow = this.package.GetToolWindow<CSVExportPane>();
            toolWindow.Evaluate(); // evaluate configuration (parameters, files)
            toolWindow.Show();
        }
    }
}
