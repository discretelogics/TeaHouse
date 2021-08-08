using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TeaTime
{    
	public static class CommandLineArgsManager<T> where T : new()
	{
		public static string ToArgs(T options)
		{
			return ToArgsArray(options).Joined(" ");
		}

		/// <summary>
		/// AppDomain.ExecuteAssembly requires an array of strings.
		/// </summary>
		/// <param name="options"></param>
		/// <returns></returns>
		public static string[] ToArgsArray(T options)
		{
			List<string> args = new List<string>();
			foreach (var pi in typeof(T).GetProperties())
			{
				object value = pi.GetValue(options, null);
				if (value.IsSet() && !HasDefaultValue(pi.PropertyType, value))
				{
					args.Add("{0}=\"{1}\"".Formatted(pi.Name, value));
				}
			}
			return args.ToArray();
		}

		/// <summary>
		/// Returns a new instance of options of type <typeparamref name="T"/> whose values are populated from <paramref name="args"/>.
		/// </summary>
		/// <param name="args"></param>
		/// <returns></returns>
		public static T FromArgs(string[] args)
		{
			try
			{
				var options = new T();
				args.ForEach(arg => ReadField(options, arg));
				return options;
			}
			catch (Exception ex)
			{
				throw new Exception("Error parsing arguments: " + string.Join(" ", args), ex);
			}
		}

		private static void ReadField(T options, string argument)
		{
			string name;
			string value;
			ParseArgument(argument, out name, out value);
			const BindingFlags bf = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public;
			var property = typeof(T).GetProperty(name, bf);
			if (property != null)
			{
				object typedValue = null;
				if (property.PropertyType.IsEnum)
				{
					typedValue = Enum.Parse(property.PropertyType, value);
				}
				else
				{
					typedValue = Convert.ChangeType(value, property.PropertyType);					
				}
				property.SetValue(options, typedValue, bf, null, null, null);
			}
		}

		private static void ParseArgument(string argument, out string name, out string value)
		{
			var re = new Regex("/?(.*?)[:=]\"?(.*?)\"?$");
			var m = re.Match(argument);
			if (m.Groups.Count == 3)
			{
				name = m.Groups[1].ToString();
				value = m.Groups[2].ToString();
				return;
			}
			throw new Exception("Error reading argument <" + argument + ">");
		}

        /// <summary>
        /// Returns false positive if type is unknown. Add types below as needed.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="o"></param>
        /// <returns></returns>
        public static bool HasDefaultValue(Type t, object o)
        {
            if (t.IsClass || t == typeof(string)) return o == null;
            if (t.IsEnum) return (int)o == 0;
            if (t == typeof(int)) return HasDefaultValue<int>(o);
            if (t == typeof(double)) return HasDefaultValue<double>(o);
            if (t == typeof(bool)) return HasDefaultValue<bool>(o);
            return false;
        }

        public static bool HasDefaultValue<V>(object value)
        {
            return default(V).Equals(value);
        }
	}
}