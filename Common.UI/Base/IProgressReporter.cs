namespace TeaTime
{
    public interface IProgressReporter
    {
        void ReportProgress(string message, uint completed, uint total);
        //void LengthyperationStarted();
        //void LengthyperationCompleted();
    }
}