// copyright discretelogics 2012.

using TeaTime.Special;
using TeaTime.VSX;

namespace TeaTime.Commands
{
    public class ShowAboutDialogCmd : CommandBase<TeaHousePackage>
    {
        public override void Execute(object parameter = null)
        {
            AboutDialog.ShowModal();
        }
    }
}
