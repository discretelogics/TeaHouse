using System;
using System.Linq;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace TeaTime.Data
{
    public struct OHLC
    {
        public double Open;
        public double High;
        public double Low;
        public double Close;

        public override string ToString()
        {
            return Open.ToString("#.##") + "  " + High.ToString("#.##") + " " + Low.ToString("#.##") + " " + Close.ToString("#.##");
        }
    }

    public struct OHLCV
    {
        public double Open;
        public double High;
        public double Low;
        public double Close;
        public Int64 Volume;

        public override string ToString()
        {
            return Open.ToString("#.##") + "  "
                + High.ToString("#.##") + " "
                + Low.ToString("#.##") + " "
                + Close.ToString("#.##") + " "
                + Volume.ToString("#.##");
        }
    }

    public struct OHLCVI
    {
        public double Open;
        public double High;
        public double Low;
        public double Close;
        public Int64 Volume;
        public Int64 OpenInterest;

        public override string ToString()
        {
            return Open.ToString("#.##") + "  "
                + High.ToString("#.##") + " "
                + Low.ToString("#.##") + " "
                + Close.ToString("#.##") + " "
                + Volume.ToString("#.##") + " "
                + OpenInterest.ToString("#.##");
        }
    }

    public struct Tick
    {                
        public double Price;	//	8
        public Int32 Id;		//	4
        public Int32 Volume;	//	4

        public override string ToString()
        {
            return Id + " " + Price + " " + Volume;
        }
    }

    public struct Band
    {
        public double Lower;
        public double Upper;

        public override string ToString()
        {
            return this.Lower.ToString("#.##") + " - " + this.Upper.ToString("#.##");
        }
    }
}