using System;

namespace TeaTime
{    
    public partial struct RangeL
    {
        public static readonly RangeL Infinite = new RangeL(long.MinValue, long.MaxValue);

        #region properties
        public long Length
        {
            get
            {
                if (end < start) return 0;
                long l = this.end - this.start + 1;
                if (l >= 0) return l;
                return 0;
            }
        }
        #endregion

        #region public methods
        /// <summary>
        /// Returns the part of r that does not intersect with this if this is not contained in r. 
        /// If this is fully contained in r, the method returns r instead of the 2 not intersecting ranges left and right.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public RangeL Outersection(RangeL r)
        {
            if (r.IsEmpty)
            {
                return this;
            }
            if (!this.Overlaps(r))
            {
                return this;
            }
            if (r.start <= this.start)
            {
                return new RangeL(r.end + 1, this.end);
            }
            if (r.end >= this.end)
            {
                return new RangeL(this.start, r.start - 1);
            }
            return this;
        }
        #endregion

        public bool Equals(RangeL other)
        {
            return other.start == start && other.end == end;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(RangeL)) return false;
            return Equals((RangeL)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (start.GetHashCode() * 397) ^ end.GetHashCode();
            }
        }
    }
}
