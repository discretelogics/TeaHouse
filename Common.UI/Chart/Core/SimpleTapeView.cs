using System;
using System.Collections.Generic;
using System.Linq;

namespace TeaTime.Chart.Core
{
    [Serializable]
    internal class SimpleTapeView : TapeViewBase
    {
        #region properties

        public override IEnumerable<ITeaFile> Tss
        {
            get
            {
                if (tapedTs == null)
                    return new ITeaFile[] {};
                return new [] { tapedTs };
            }
        }

        #endregion

        #region public methods

        public override void Add(params ITeaFile[] tss)
        {
            Guard.ArgumentNotNull(tss, "tss");
            if (tss.Any())
            {
                if (tapedTs != null)
                {
                    throw new NotSupportedException("You can't add more than one TimeSeries to a SimpleTape.");
                }

                tapedTs = tss.Single();

                UpdateLength(tapedTs.Count);
            }
        }
        public override void Remove(params ITeaFile[] tss)
        {
            Guard.ArgumentNotNull(tss, "tss");

            if (tss.Any())
            {
                if (tapedTs != tss.Single())
                {
                    throw new InvalidOperationException("You can't remove a TimeSeries that was not added before.");
                }

                tapedTs = null;

                UpdateLength(0);
            }
        }

        public override RangeL ComputeTapeRange(RangeL range)
        {
            return range;
        }

        public override DateTime TimeAt(long index)
        {
            return tapedTs.TimeAt(MaxIndex - index);
        }

        #endregion

        #region fields
        private ITeaFile tapedTs;
        #endregion
    }
}
