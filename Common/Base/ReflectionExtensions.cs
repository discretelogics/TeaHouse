#if false // note used. Obfuscator fears this class

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TeaTime
{
	public static class ReflectionExtensions
	{
		public static FileInfo GetFileInfo(this Assembly assembly)
		{
			var dir = assembly.CodeBase;
			dir = dir.Replace("file:///", "");
			return new FileInfo(dir);
		}

		//[CodeQuality(Quality.Raw)]
		//public static IEnumerable GetAllPropertyValues(this object instance)
		//{
		//    return instance.GetType().GetProperties().Select(p => p.GetValue(instance, null));
		//}

		public static object GetPropertyValue(this object instance, string name)
		{
			Guard.ArgumentNotNull(instance, "instance");
			Guard.ArgumentNotNull(name, "name");

			return instance.GetType()
				.GetProperties()
				.FirstOrExcept(p => p.Name == name, "Property {0} not found on type {1} ".Formatted(name, instance.GetType().FullName))
				.GetValue(instance, null);
		}

		public static IEnumerable<PropertyInfo> GetProperties<TBase>(this object instance)
		{
			return instance.GetType().GetProperties().Where(p => p.PropertyType.IsAssignAbleFrom<TBase>());
		}

		public static IEnumerable<PropertyInfo> GetProperties<TBase>(this object instance, BindingFlags bindingFlags)
		{
			return instance.GetType().GetProperties(bindingFlags).Where(p => p.PropertyType.IsAssignAbleFrom<TBase>());
		}

		public static IEnumerable<TBase> GetPropertyValues<TBase>(this object instance)
		{
			return instance.GetProperties<TBase>().Select(p => (TBase)p.GetValue(instance, null));
		}

		public static T GetValue<T>(this PropertyInfo pi, object instance)
		{
			return (T)pi.GetValue(instance, null);
		}

		public static void SetValue(this PropertyInfo pi, object instance, object propertyValue)
		{
			pi.SetValue(instance, propertyValue, null);
		}

		public static bool IsAssignAbleFrom<T>(this Type t)
		{
			return typeof(T).IsAssignableFrom(t);
		}

		public static object Construct(this Type t, params  object[] constructorArgs)
		{
			return Activator.CreateInstance(t, constructorArgs);
		}
		
		/// <summary>
		/// Composes a name for a type using widespread template syntax.
		/// Example: For the Type int, this method returns "int".
		/// Example: For the Type Event&lt;double&gt; it returns Event&lt;double&gt; .
		/// </summary>
		/// <remarks>
		/// A similar function exists in TeaFile&lt;T&gt; with name GetNameFromType(). PainterManager and TeaFile API must work here together.
		/// Using general accepted template formatting instead of .Net reflection formatting is favored to remain platform agnostic.
		/// </remarks>
		/// <param name="t"></param>
		/// <returns></returns>
		public static string GetNiceName(this Type t)
		{
			if (!t.IsGenericType) return t.Name;
			return t.Name.GetFirstPart('`') + "<" + t.GetGenericArguments().Select(GetNiceName).Joined(",") + ">";
		}

		public static string GetNameWithoutGeneric(this Type t)
		{
			if (!t.IsGenericType) return t.Name;
			return t.Name.GetFirstPart('`');
		}

		public static bool IsSpecificGenericType(this Type t, Type genericType)
		{
			return t.IsGenericType && t.GetGenericTypeDefinition() == genericType;
		}
	}
}

#endif