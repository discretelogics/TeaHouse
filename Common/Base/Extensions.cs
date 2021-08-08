using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TeaTime
{
    public static class Extensions
    {
        public static void Times<T>(this int n, Func<T> f)
        {
            for (int i = 0; i < n; i++)
            {
                f();
            }
        }
        
        public static void Times(this int n, Action<int> a)
        {
            for (int i = 0; i < n; i++)
            {
                a(i);
            }
        }

        public static void OnEvery(this uint n, uint blocksize, Action f)
        {
            if (n % blocksize == 0)
                f();
        }

        public static IEnumerable<string> ReadLines(this StreamReader reader, int maxLinesToRead)
        {
            for (int i = 0; i < maxLinesToRead; i++)
            {
                var s = reader.ReadLine();
                if (!s.IsSet()) break;
                yield return s;
            }
        }

        public static bool IsSet(this object o)
        {
            return o != null;
        }

        public static bool IsOneOf(this string s, params string[] values)
        {
            return values.Contains(s);
        }

        public static string Joined(this IEnumerable<string> values, string s)
        {
            return string.Join(s, values);
        }

        public static string JoinedToLines(this IEnumerable<string> values)
        {
            return values.Joined("\r\n");
        }

        public static string TrimEnd(this string value, string textToCut, bool ignoreCase)
        {
            if (value.EndsWith(textToCut, ignoreCase, null))
            {
                return value.Substring(0, value.Length - textToCut.Length);
            }
            return value;
        }

        public static string ToUIString(this string s)
        {
            return s.ToString(CultureInfo.CurrentCulture);
        }

        public static string ToSafeString(this object o, string nullString)
        {
            if (o == null) return nullString;
            return o.ToString();
        }

        public static bool In(this object value, params object[] values)
        {
            return values.Any(v => v == value);
        }

        public static string GetReflectedPropertiesString(this object value)
        {
            return value.GetType().GetProperties()
                .Select(pi => pi.Name + "=" + pi.GetValue(value, null).ToSafeString("null"))
                .JoinedToLines();
        }

        public static SortedSet<T> ToSet<T>(this IEnumerable<T> values)
        {
            return new SortedSet<T>(values);
        }

        public static T GetAttribute<T>(this Type type)
        {
            return (T)type.GetCustomAttributes(true).FirstOrDefault(a => a is T);
        }

        public static bool IsEventOfPrimitive(this Type type)
        {
            var fields = type.GetAllInstanceFields();
            if (fields.Count() != 2)
            {
                return false;
            }
            var timeField = fields.FirstOrDefault(f => f.FieldType == typeof(long));
            return (timeField != null) && fields.Any(f => f != timeField && f.FieldType.IsPrimitive);
        }

        public static long IndexAt(this ITeaFile teafilet, DateTime time, RoundMode mode)
        {
            return Algorithms.BinarySearch(i => teafilet.TimeAt(i), 0, teafilet.Count - 1, time, mode);
        }

        public static IEnumerable<FieldInfo> GetNumericItemFields(this Type itemType)
        {
            return itemType.GetAllInstanceFields().Where(f => f.FieldType.IsNumeric()).ToArray();
        }

        public static bool Is(this FieldInfo field, string name)
        {
            return field.Name.TrimStart('_').Equals(name.TrimStart('_'), StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool EqualsIgnoreCase(this string value, string other)
        {
            return string.Compare(value, other, StringComparison.InvariantCultureIgnoreCase) == 0;
        }
    }
}
