using System;

namespace TeaTime
{    
    public partial struct Range
    {
        public static readonly Range Infinite = new Range(int.MinValue, int.MaxValue);

        #region properties
        public int Length
        {
            get
            {
                if (end < start) return 0;
                int l = this.end - this.start + 1;
                if (l >= 0) return l;
                else return 0;
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
        public Range Outersection(Range r)
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
                return new Range(r.end + 1, this.end);
            }
            if (r.end >= this.end)
            {
                return new Range(this.start, r.start - 1);
            }
            return this;
        }
        #endregion

        public bool Equals(Range other)
        {
            return other.start == start && other.end == end;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof(Range)) return false;
            return Equals((Range)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (start * 397) ^ end;
            }
        }
    }
}
