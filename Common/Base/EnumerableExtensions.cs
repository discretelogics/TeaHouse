// copyright discretelogics © 2011
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

// ReSharper disable RedundantTypeArgumentsOfMethod
// for clarity such redundant "<T>" arguments are helpful.

namespace TeaTime
{
    public static class EnumerableExtensions
    {
        /// <summary>
        ///     Selects an element of a sequence if all are the equal or a default value.
        /// </summary>
        /// <typeparam name = "TSource">The type of the elements contained in the sequence.</typeparam>
        /// <typeparam name = "TResult">The type to select.</typeparam>
        /// <param name = "enumerable">The sequence containing the elements.</param>
        /// <param name = "selector">The selector for each element.</param>
        /// <returns>The selected element or its default value.</returns>
        public static TResult SelectAllEqualOrDefault<TSource, TResult>(this IEnumerable<TSource> enumerable, Func<TSource, TResult> selector)
        {
            TResult selected = default(TResult);

            if ((enumerable != null) && enumerable.Any())
            {
                bool set = false;
                foreach (var s in enumerable.Select(selector))
                {
                    if (!set)
                    {
                        selected = s;
                        set = true;
                    }
                    else
                    {
                        if (!selected.Equals(s))
                        {
                            return default(TResult);
                        }
                    }
                }

                return selected;
            }

            return selected;
        }

        public static bool IsEmpty<TSource>(this IEnumerable<TSource> enumerable)
        {
            return (enumerable == null) || !enumerable.Any();
        }

        public static void ForEachReverse<TSource>(this IList<TSource> list, Action<TSource> action)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                action.Invoke(list[i]);
            }
        }

        public static void ForEachIndex<TSource>(this IEnumerable<TSource> source, Action<int, TSource> action)
        {
            Guard.ArgumentNotNull(source, "source");
            if (action == null) return;

            int i = 0;
            var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                action(i++, enumerator.Current);
            }
        }

        public static List<T> CollectAndRemove<T>(this IList<T> source, Predicate<T> selector)
        {
            var collected = new List<T>();
            int i = 0;
            while (i < source.Count)
            {
                if (selector(source[i]))
                {
                    collected.Add(source[i]);
                    source.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
            return collected;
        }

        [DebuggerHidden]
        public static void ForEachCollectExceptions<T>(this IEnumerable<T> collection, Action<T> a)
        {
            List<Exception> es = new List<Exception>();			
            foreach (var item in collection)
            {
                try
                {
                    a(item);
                }
                catch (Exception ex)
                {
                    es.Add(ex);
                }
            }
            if(es.Any())
            {
                var ae = new AggregateException("Operation upon collection failed at least once: ", es);
                throw ae;
            }
        }        

        public static bool SafeAny<T>(this IEnumerable<T> value)
        {
            if (value == null) return false;
            return value.Any();
        }

        public static bool SafeAny<T>(this IEnumerable<T> e, Func<T, bool> f)
        {
            if (e == null) return false;
            return e.Any(f);
        }

        public static void Add<T>(this ObservableCollection<T> collection, IEnumerable<T> values)
        {
            values.ForEach(collection.Add);
        }

        /// <summary>
        ///     Removes all elements matching the condition.
        /// </summary>
        /// <typeparam name = "T"></typeparam>
        /// <param name = "collection"></param>
        /// <param name = "condition"></param>
        public static void Remove<T>(this ObservableCollection<T> collection, Func<T, bool> condition)
        {
            var removals = collection.Where<T>(condition).ToList();
            foreach (var removal in removals)
            {
                collection.Remove(removal);
            }
        }

        #region smooth update

        public static void SmoothUpdate<T, TKey>(this ICollection<T> target,
                                                 IEnumerable<T> source,
                                                 Func<T, TKey> key,
                                                 Action<T, T> assignFrom)
            where TKey : IComparable<TKey>
        {
            // remove all elements in source that have no key in the source
            var removed = target.Where(si => !source.Any(ti => key(ti).Equals(key(si)))).ToArray();
            removed.ForEach(a => target.Remove(a));

            // add or adapt
            foreach (var newVal in source)
            {
                var oVal = target.FirstOrDefault(ssi => key(ssi).Equals(key(newVal))); // ReSharper disable CompareNonConstrainedGenericWithNull
                if (oVal == null) // ReSharper restore CompareNonConstrainedGenericWithNull
                {
                    // add new
                    target.Add(newVal); // we take ownership of the new one (!)
                }
                else
                {
                    // smoothly adapt
                    assignFrom(oVal, newVal);
                }
            }
        }

        public static void SmoothUpdate<TTarget, TSource, TKey>(this ICollection<TTarget> target,
                                                                IEnumerable<TSource> source,
                                                                Func<TTarget, TKey> targetKey,
                                                                Func<TSource, TKey> sourceKey,
                                                                Func<TSource, TTarget> create)
            where TKey : IComparable<TKey>
        {
            var removed = target.Where(t => !source.Any(s => sourceKey(s).Equals(targetKey(t)))).ToArray();
            removed.ForEach(a => target.Remove(a));

            // add new values
            foreach (var s in source)
            {
                if (!target.Any(t => targetKey(t).Equals(sourceKey(s))))
                {
                    target.Add(create(s));
                }
            }

            Debug.Assert(source.Count() == target.Count());
        }

        /// <summary>
        ///     Simplified string version.
        /// </summary>
        /// <param name = "target"></param>
        /// <param name = "source"></param>
        public static void SmoothUpdate(this ICollection<object> target, IEnumerable<object> source)
        {
            var removed = target.Where(t => !source.Any(s => s.ToString() == t.ToString())).ToArray();
            removed.ForEach(a => target.Remove(a));

            // add new values
            foreach (var s in source)
            {
                if (!target.Any(t => t.ToString() == s.ToString()))
                {
                    target.Add(s);
                }
            }
        }

        #endregion

        /// <summary>
        /// Iterates over parent-child related structures guaranteeing that a child is enumerated before its parent.
        /// </summary>
        /// <typeparam name="T">The node type.</typeparam>
        /// <param name="node">The root node.</param>
        /// <param name="childSelector">Returns a node's children.</param>
        /// <returns>An enumerator that returns children before their parent.</returns>
        public static IEnumerable<T> SelectChildrenFirst<T>(this T node, Func<T, IEnumerable<T>> childSelector)
        {
            foreach (T child in childSelector(node))
            {
                foreach (var cc in SelectChildrenFirst<T>(child, childSelector))
                {
                    yield return cc;
                }
            }
            yield return node;
        }

        public static IEnumerable<T> AsDepthFirstEnumerable<T>(this T head, Func<T, IEnumerable<T>> childrenFunc)
        {
            yield return head;
            foreach (var node in childrenFunc(head))
            {
                foreach (var child in AsDepthFirstEnumerable(node, childrenFunc))
                {
                    yield return child;
                }
            }
        }

        public static T FirstOrExcept<T>(this IEnumerable<T> collection, Func<T, bool> predicate, string exceptionMessage) where T : class
        {
            var e = collection.FirstOrDefault(predicate);
            if (e == null) throw new Exception(exceptionMessage);
            return e;
        }

        public static T FirstOrExcept<T>(this IEnumerable collection, string exceptionMessage) where T : class
        {
            var e = collection.OfType<T>().FirstOrDefault();
            if (e == null) throw new Exception(exceptionMessage);
            return e;
        }
    }
}