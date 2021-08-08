
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;
using TeaTime.Tree;
using TeaTime.UI;
using TeaTime.VSX;

namespace TeaTime.Commands
{
    public class ViewTeaHouseExplorer : CommandBase<TeaHousePackage>
    {
        public override void Execute(object parameter = null)
        {
            var toolWindow = package.GetToolWindow<TeaHouseTreePane>();
            toolWindow.Show();
        }
    }
}
