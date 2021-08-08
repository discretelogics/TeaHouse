namespace TeaTime.Chart.Markers
{
    internal interface ICursor
    {
        CursorModes Mode { get; set; }
        bool Permanent { get; set; }
    }
}
