using TeaTime.VSX;

namespace TeaTime.Yahoo.Commands
{
	class ViewYahooBrowser : CommandBase<YahooPackage>
    {
        public override void Execute(object parameter)
        {
            this.package.ShowYahooToolWindow();
        }
    }
}
