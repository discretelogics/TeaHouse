using System;
using System.Collections.Generic;

namespace TeaTime.API
{
    public interface ITapeView
    {
        IEnumerable<ITeaFile> Tss { get; }
        long Length { get; }
        long GetTimeSeriesEnd(ITeaFile ts, long index);
        long GetTimeSeriesStart(ITeaFile ts, long index);
        double WidthAt(long index);
        double PositionAt(long index);

        DateTime TimeAt(long index);
        long IndexAt(DateTime t, RangeL searchRange);
    }
}
