using TeaTime;
using TeaTime.VSX;

namespace TeaTime.Commands
{
	public class ViewTeaHouseOptions : CommandBase<TeaHousePackage>
    {
        public override void Execute(object parameter)
        {
            package.ShowOptionPage(typeof(TeaHouseOptionPage));
        }
    }
}
