using System;
using System.Collections.Generic;
using System.Linq;

namespace TeaTime.Chart.Core
{
    [Serializable]
    internal class TapeView : TapeViewBase
    {
        #region properties

        public override IEnumerable<ITeaFile> Tss { get { return tapedTss; } }

        #endregion

        #region ctor
        public TapeView()
        {
            this.tapedTss = new List<ITeaFile>();
            this.remainingTimeEnumerators = new List<IEnumerator<DateTime>>();
            this.timeSlices = new List<DateTime>();
        }
        #endregion

        #region public methods
        public override void Add(params ITeaFile[] tss)
        {
            Guard.ArgumentNotNull(tss, "tss");

            if (tss.Any())
            {
                if (tss.Any(ts => tapedTss.Contains(ts)))
                {
                    throw new InvalidOperationException("At least one TimeSeries was already added to this Tape.");
                }

                tapedTss.AddRange(tss);

                Invalidate();
            }
        }
        public override void Remove(params ITeaFile[] tss)
        {
            Guard.ArgumentNotNull(tss, "tss");

            if (tss.Any(ts => !tapedTss.Contains(ts)))
            {
                throw new InvalidOperationException("At least one TimeSeries was not added to this Tape.");
            }

            if (tss.Any())
            {
                tapedTss.RemoveAll(tss.Contains);

                Invalidate();
            }
        }

        public override RangeL ComputeTapeRange(RangeL range)
        {
            if (tapedTss.Count == 1)
            {
                return range;
            }
            
            var targetIndexEnd = range.End;
            if (this.timeSlices.Count <= targetIndexEnd)
            {
                long compressedStart = 0;
                long compressedEnd = 0;
                long compressedTotal = 0;
                long targetIndexStart = range.Start;
                long targetIndexWithBuffer = targetIndexEnd + bufferSize;
                long targetLength = range.Length;

                var endedTimeEnumerators = new List<IEnumerator<DateTime>>();
                while (this.remainingTimeEnumerators.Any())
                {
                    DateTime t = this.remainingTimeEnumerators.Max(e => e.Current);
                    this.timeSlices.Add(t);
                    int computedCount = this.timeSlices.Count;

                    bool skippedMax = false;
                    foreach (IEnumerator<DateTime> e in this.remainingTimeEnumerators)
                    {
                        while (e.Current >= t)
                        {
                            if (skippedMax)
                            {
                                ++compressedTotal;
                                if (computedCount < targetIndexStart)
                                {
                                    ++compressedStart;
                                }
                                if (computedCount < targetIndexEnd)
                                {
                                    ++compressedEnd;
                                }
                            }
                            else
                            {
                                skippedMax = true;
                            }

                            if (!e.MoveNext())
                            {
                                endedTimeEnumerators.Add(e);
                                break;
                            }
                        }
                    }
                    foreach (IEnumerator<DateTime> e in endedTimeEnumerators)
                    {
                        this.remainingTimeEnumerators.Remove(e);
                    }
                    endedTimeEnumerators.Clear();

                    if (computedCount > targetIndexWithBuffer)
                    {
                        this.UpdateLength(this.Length - compressedTotal);
                        var computedRangeFromStart = new RangeL(targetIndexStart - compressedStart, targetIndexEnd - compressedEnd);
                        if (computedRangeFromStart.Length < targetLength)
                        {
                            // take start as fixpoint
                            computedRangeFromStart.End = Math.Min(targetIndexWithBuffer, computedRangeFromStart.Start + targetLength - 1);
                        }
                        return computedRangeFromStart;
                    }
                }

                // reached left end
                this.UpdateLength(this.timeSlices.Count);
                var computedRangeFromEnd = new RangeL(targetIndexStart - compressedStart, targetIndexEnd - compressedEnd);
                if (computedRangeFromEnd.Length < targetLength)
                {
                    // take end as fixpoint
                    computedRangeFromEnd.Start = Math.Max(0, computedRangeFromEnd.End - targetLength + 1);
                }
                return computedRangeFromEnd;
            }

            return range;
        }

        public override DateTime TimeAt(long index)
        {
            // Trace.WriteLine("TimeAt a {0}".Formatted(index)); // evil
            if (tapedTss.Count == 1)
            {
                return tapedTss[0].TimeAt(MaxIndex - index);
            }
            // TODO: safe solution
            return timeSlices[(int)index];
        }
        #endregion

        #region private methods
        private void Invalidate()
        {
            #region clear calculated timeSlices
            timeSlices.Clear();
            #endregion

            #region prepare enumerators
            remainingTimeEnumerators.Clear();
            if (tapedTss.Count > 1) // we don't any enumerator if there's only 1 TimeSeries taped
            {
                remainingTimeEnumerators.AddRange(tapedTss.Where(ts => ts.Count > 0).Select(GetTimeEnumeratorFromRight));
                remainingTimeEnumerators.RemoveAll(te => !te.MoveNext());
            }
            #endregion

            UpdateLength(tapedTss.Sum(ts => ts.Count));
        }

        private static IEnumerator<DateTime> GetTimeEnumeratorFromRight(ITeaFile ts)
        {
            for (var i = ts.Count - 1; i >= 0; --i)
            {
                yield return ts.TimeAt(i);
            }
        }
        #endregion

        #region fields
        private readonly List<ITeaFile> tapedTss;
        private readonly List<IEnumerator<DateTime>> remainingTimeEnumerators;
        private readonly List<DateTime> timeSlices;
        
        private const int bufferSize = 1000;
        #endregion
    }
}
