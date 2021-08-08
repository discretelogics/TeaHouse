namespace TeaTime.API
{
    public interface IYScaler
    {
        double ActualHeight { get; }
        RangeD ValueRange { get; }
        string Name { get; }

        double PositionHeightToValueHeight(double height);
        double PositionToValue(double y);
        double ValueHeightToPositionHeight(double height);
        double ValueToPosition(double y);
    }
}
