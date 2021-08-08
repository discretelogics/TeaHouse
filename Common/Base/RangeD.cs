using TeaTime.Data;

namespace TeaTime
{
    public partial struct RangeD
    {
        #region public properties

		/// <summary>
		/// Length computation for RangeD differs from Length computation of Range.
		/// </summary>
        public double Length
        {
            get
            {
				if (end < start) return 0;
                return end - start;
            }
        }
        #endregion

		public bool Equals(RangeD other)
		{
			return other.start.Equals(start) && other.end.Equals(end);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (obj.GetType() != typeof (RangeD)) return false;
			return Equals((RangeD) obj);
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
