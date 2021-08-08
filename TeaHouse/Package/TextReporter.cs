using TeaTime.VSX;

namespace TeaTime
{
    public class TextReporter : ITextReporter
    {
        readonly TeaHousePackage package;

        public TextReporter(TeaHousePackage package)
        {
            this.package = package;
        }

        public void WriteLine(string text)
        {            
            this.package.WriteMessage(true, text); 
        }
    }
}