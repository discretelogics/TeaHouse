using System;

namespace TeaTime
{
    public partial struct RangeT
    {
        #region Constants
        public static readonly RangeT Unlimited = new RangeT(DateTime.MinValue, DateTime.MaxValue);
        #endregion

        #region public methods

		//public RangeT Intersect(RangeT limitRange)
		//{
		//    RangeT r = this;
		//    if (limitRange.start > r.start) r.start = limitRange.start;
		//    if (limitRange.end > r.end) r.end = limitRange.end;
		//    return r;
		//}

		//public RangeT Union(RangeT unifiedRange)
		//{
		//    RangeT r = this;
		//    if (unifiedRange.start < r.start) r.start = unifiedRange.start;
		//    if (unifiedRange.end > r.end) r.end = unifiedRange.end;
		//    return r;
		//}

    	#endregion

		public bool Equals(RangeT other)
		{
			return other.start.Equals(start) && other.end.Equals(end);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (obj.GetType() != typeof (RangeT)) return false;
			return Equals((RangeT) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (start.GetHashCode()*397) ^ end.GetHashCode();
			}
		}
    }
}
