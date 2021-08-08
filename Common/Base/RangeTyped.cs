using System;
using System.Diagnostics;

namespace TeaTime
{
    [Serializable]
    public partial struct Range
    {
        #region fields
        private int start;
        private int end;
        #endregion

        #region ctor
        public Range(int start, int end)
        {
            this.start = start;
            this.end = end;
        }
        #endregion

        #region Empty constant
        public static readonly Range Empty = new Range(int.MaxValue, int.MinValue);
        #endregion

        #region public properties
        public int Start
        {
            get
            {
                return start;
            }
            set
            {
                start = value;
            }
        }
        public int End
        {
            set
            {
                end = value;
            }
            get
            {
                return end;
            }
        }        
        public bool IsEmpty
        {
            get
            {
                return this.end < this.start;
            }
        }        
        #endregion

        #region public methods
        public bool Contains(int value)
        {
            return value >= start && value <= end;
        }
		public int EnsureContained(int value)
        {
            if (value > end)
            {
                return end;
            }
            if (value < start)
            {
                return start;
            }
            return value;
        }
        /// <summary>
        /// Returns true if r is fully contained or equivalent in this.
        /// </summary>
        /// <param name="r">The Range to be checked</param>
        /// <returns>true if r is fully contained or equivalent in this.</returns>
        public bool Contains(Range r)
        {
            return (r.start >= this.start) && (r.end <= this.end);
        }
        public bool Overlaps(Range r)
        {
            if(r.start > this.end || r.end < this.start)
            {
                return false;
            }
            if (this.IsEmpty || r.IsEmpty)
            {
                return false;
            }
            return true;
        }
        
        public override string ToString()
        {
            if (IsEmpty) return "Empty (" + this.start + "=>" + this.end + ")";
            return this.start + "=>" + this.end;
        }
        #endregion

        #region operators
        public static bool operator==(Range lhs, Range rhs)
        {
            return lhs.start == rhs.start
                && lhs.end == rhs.end;
        }
        public static bool operator !=(Range lhs, Range rhs)
        {
            return !(lhs == rhs);
        }       
        #endregion
    }
    [Serializable]
    public partial struct RangeD
    {
        #region fields
        private double start;
        private double end;
        #endregion

        #region ctor
        public RangeD(double start, double end)
        {
            this.start = start;
            this.end = end;
        }
        #endregion

        #region Empty constant
        public static readonly RangeD Empty = new RangeD(double.MaxValue, double.MinValue);
        #endregion

        #region public properties
        public double Start
        {
            get
            {
                return start;
            }
            set
            {
                start = value;
            }
        }
        public double End
        {
            set
            {
                end = value;
            }
            get
            {
                return end;
            }
        }        
        public bool IsEmpty
        {
            get
            {
                return this.end < this.start;
            }
        }        
        #endregion

        #region public methods
        public bool Contains(double value)
        {
            return value >= start && value <= end;
        }
		public double EnsureContained(double value)
        {
            if (value > end)
            {
                return end;
            }
            if (value < start)
            {
                return start;
            }
            return value;
        }
        /// <summary>
        /// Returns true if r is fully contained or equivalent in this.
        /// </summary>
        /// <param name="r">The RangeD to be checked</param>
        /// <returns>true if r is fully contained or equivalent in this.</returns>
        public bool Contains(RangeD r)
        {
            return (r.start >= this.start) && (r.end <= this.end);
        }
        public bool Overlaps(RangeD r)
        {
            if(r.start > this.end || r.end < this.start)
            {
                return false;
            }
            if (this.IsEmpty || r.IsEmpty)
            {
                return false;
            }
            return true;
        }
        
        public override string ToString()
        {
            if (IsEmpty) return "Empty (" + this.start + "=>" + this.end + ")";
            return this.start + "=>" + this.end;
        }
        #endregion

        #region operators
        public static bool operator==(RangeD lhs, RangeD rhs)
        {
            return lhs.start == rhs.start
                && lhs.end == rhs.end;
        }
        public static bool operator !=(RangeD lhs, RangeD rhs)
        {
            return !(lhs == rhs);
        }       
        #endregion
    }
    [Serializable]
    public partial struct RangeL
    {
        #region fields
        private long start;
        private long end;
        #endregion

        #region ctor
        public RangeL(long start, long end)
        {
            this.start = start;
            this.end = end;
        }
        #endregion

        #region Empty constant
        public static readonly RangeL Empty = new RangeL(long.MaxValue, long.MinValue);
        #endregion

        #region public properties
        public long Start
        {
            get
            {
                return start;
            }
            set
            {
                start = value;
            }
        }
        public long End
        {
            set
            {
                end = value;
            }
            get
            {
                return end;
            }
        }        
        public bool IsEmpty
        {
            get
            {
                return this.end < this.start;
            }
        }        
        #endregion

        #region public methods
        public bool Contains(long value)
        {
            return value >= start && value <= end;
        }
		public long EnsureContained(long value)
        {
            if (value > end)
            {
                return end;
            }
            if (value < start)
            {
                return start;
            }
            return value;
        }
        /// <summary>
        /// Returns true if r is fully contained or equivalent in this.
        /// </summary>
        /// <param name="r">The RangeL to be checked</param>
        /// <returns>true if r is fully contained or equivalent in this.</returns>
        public bool Contains(RangeL r)
        {
            return (r.start >= this.start) && (r.end <= this.end);
        }
        public bool Overlaps(RangeL r)
        {
            if(r.start > this.end || r.end < this.start)
            {
                return false;
            }
            if (this.IsEmpty || r.IsEmpty)
            {
                return false;
            }
            return true;
        }
        
        public override string ToString()
        {
            if (IsEmpty) return "Empty (" + this.start + "=>" + this.end + ")";
            return this.start + "=>" + this.end;
        }
        #endregion

        #region operators
        public static bool operator==(RangeL lhs, RangeL rhs)
        {
            return lhs.start == rhs.start
                && lhs.end == rhs.end;
        }
        public static bool operator !=(RangeL lhs, RangeL rhs)
        {
            return !(lhs == rhs);
        }       
        #endregion
    }
    [Serializable]
    public partial struct RangeT
    {
        #region fields
        private DateTime start;
        private DateTime end;
        #endregion

        #region ctor
        public RangeT(DateTime start, DateTime end)
        {
            this.start = start;
            this.end = end;
        }
        #endregion

        #region Empty constant
        public static readonly RangeT Empty = new RangeT(DateTime.MaxValue, DateTime.MinValue);
        #endregion

        #region public properties
        public DateTime Start
        {
            get
            {
                return start;
            }
            set
            {
                start = value;
            }
        }
        public DateTime End
        {
            set
            {
                end = value;
            }
            get
            {
                return end;
            }
        }        
        public bool IsEmpty
        {
            get
            {
                return this.end < this.start;
            }
        }        
        #endregion

        #region public methods
        public bool Contains(DateTime value)
        {
            return value >= start && value <= end;
        }
		public DateTime EnsureContained(DateTime value)
        {
            if (value > end)
            {
                return end;
            }
            if (value < start)
            {
                return start;
            }
            return value;
        }
        /// <summary>
        /// Returns true if r is fully contained or equivalent in this.
        /// </summary>
        /// <param name="r">The RangeT to be checked</param>
        /// <returns>true if r is fully contained or equivalent in this.</returns>
        public bool Contains(RangeT r)
        {
            return (r.start >= this.start) && (r.end <= this.end);
        }
        public bool Overlaps(RangeT r)
        {
            if(r.start > this.end || r.end < this.start)
            {
                return false;
            }
            if (this.IsEmpty || r.IsEmpty)
            {
                return false;
            }
            return true;
        }
        
        public override string ToString()
        {
            if (IsEmpty) return "Empty (" + this.start + "=>" + this.end + ")";
            return this.start + "=>" + this.end;
        }
        #endregion

        #region operators
        public static bool operator==(RangeT lhs, RangeT rhs)
        {
            return lhs.start == rhs.start
                && lhs.end == rhs.end;
        }
        public static bool operator !=(RangeT lhs, RangeT rhs)
        {
            return !(lhs == rhs);
        }       
        #endregion
    }
}