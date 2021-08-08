// copyright discretelogics 2013.

namespace TeaTime
{
    public interface ITextReporter
    {
        void WriteLine(string text);
    }

    class NullTextReporter : ITextReporter
    {
        public void WriteLine(string text) { }
    }
}
