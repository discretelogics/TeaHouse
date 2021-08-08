using System;

namespace TeaTime
{
    public delegate T Accessor<T>(long index);

    public static class Algorithms
    {
        /// <summary>
        /// For references on binary search see wikipedia about binary search, the programming pearls findings and
        /// Chuck Jazdzewski,   http://www.removingalldoubt.com/PermaLink.aspx/f7e6feff-8257-4efe-ad64-acd1c7a4a1e3
        ///                     and http://www.removingalldoubt.com/CategoryView.aspx/Programming
        /// </summary>
        /// <typeparam name="K">The Type of the member of the item T that is the Key</typeparam>
        /// <param name="accessor">A function that returns the items of the searchdomain by index</param>
        /// <param name="lower">The lower index of the search area</param>
        /// <param name="upper">The upper index of the search area</param>
        /// <param name="value">The value of the key to be searched</param>
        /// <param name="compare">A Comparison delegate</param>
        /// <param name="roundMode">Determines if the upper or lower adjacent element shall be returned if no exact match could be found.</param>
        /// <returns>If the list contains an item with key==value, its index is returned.
        /// Otherwise the Complement of the last inspected element is returned. (The generic .net List.BinarySearch method returns the complement of lower, which might not be that useful)
        /// </returns>
        public static long BinarySearch<K>(Accessor<K> accessor, long lower, long upper, K value, Comparison<K> compare, RoundMode roundMode)
        {
            long bsindex = BinarySearch(accessor, lower, upper, value, compare);
            if (bsindex >= 0)
            {
                return bsindex;   //  the item was found
            }
            else
            {
                long index = ~bsindex;
                if (index >= lower && index <= upper)   //  if the index is valid
                {
                    K valueAtIndex = accessor(index);
                    switch (roundMode)
                    {
                        case RoundMode.Up:
                            {
                                //  compare the last inspected element
                                if (compare(valueAtIndex, value) < 0)
                                {
                                    index++;
                                }
                                if (index.IsWithin(lower, upper))
                                {
                                    return index;
                                }
                                else
                                {
                                    return bsindex;
                                }
                            }
                        case RoundMode.Down:
                            {
                                //  compare the last inspected element
                                if (compare(valueAtIndex, value) > 0)
                                {
                                    index--;
                                }
                                if (index.IsWithin(lower, upper))
                                {
                                    return index;
                                }
                                else
                                {
                                    return bsindex;
                                }
                            }
                        default:
                            {
                                throw new ArgumentException("RoundMode " + roundMode.ToString() + " is not supported.");
                            }
                    }
                }
                //	the value was not found and rounding is not possible either
            	return bsindex;
            }
        }

        /// <summary>
        /// Keyed Binary Search.
        /// see Chuck Jazdzewski,   http://www.removingalldoubt.com/PermaLink.aspx/f7e6feff-8257-4efe-ad64-acd1c7a4a1e3
        ///                     and http://www.removingalldoubt.com/CategoryView.aspx/Programming
        /// </summary>
        /// <typeparam name="T">The Type of the items in the collection</typeparam>
        /// <typeparam name="K">The Type of the member of the item T that is the Key</typeparam>
        /// <param name="accessor">A function that returns the items of the searchdomain by index</param>
        /// <param name="lower">The lower index of the search area</param>
        /// <param name="upper">The upper index of the search area</param>
        /// <param name="value">The value of the key to be searched</param>
        /// <param name="compare">A Comparison delegate</param>        
        /// <returns>If the list contains an item with key==value, its index is returned.
        /// Otherwise the Complement of the last inspected element is returned. (The generic .net List.BinarySearch method returns the complement of lower, which might not be that useful)
        /// </returns>
        public static long BinarySearch<K>(Accessor<K> accessor, long lower, long upper, K value, Comparison<K> compare)
        {
            long mid = lower;
            while (lower <= upper)
            {
                mid = lower + (upper - lower) / 2;      // overflow save version
                long c = compare(accessor(mid), value);
                if (c == 0) return mid;
                if (c < 0)
                {
                    lower = mid + 1;
                }
                else
                {
                    upper = mid - 1;
                }
            }
            return ~mid;    //  returns the complement of the last checked element
        }

        public static long BinarySearch<K>(Accessor<K> accessor, long lower, long upper, K value, RoundMode mode) where K : IComparable
        {
            return BinarySearch(accessor, lower, upper, value, (m,n) => m.CompareTo(n), mode);
        }

        public static long BinarySearch<K>(Accessor<K> accessor, long lower, long upper, K value) where K : IComparable
		{
			return BinarySearch(accessor, lower, upper, value, (v1, v2) => v1.CompareTo(v2));
		}
    }
}
