using System;
using TeaTime.Data;

namespace TeaTime
{
    public class SampleValuesFactory
    {
        static Random random = new Random();
        static DateTime t = DateTime.Parse("1.1.2000");

        public static void Reset()
        {
            t = DateTime.Parse("1.1.2000");
        }

        public static DateTime GetNextTime()
        {
            t = t.AddSeconds(1);
            return t;
        }

        public static Event<OHLCV> GetOHLCV(int n)
        {            
            var ohlcv = new Event<OHLCV>();
            ohlcv.Time = new Time(2000, 1, 1).AddDays(n);            
            ohlcv.Value.Open = random.NextDouble();
            ohlcv.Value.High = random.NextDouble();
            ohlcv.Value.Low = random.NextDouble();
            ohlcv.Value.Close = random.NextDouble();
            return ohlcv;
        }
    }
}
