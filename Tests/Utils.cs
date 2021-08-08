// copyright discretelogics © 2011
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using SharpTestsEx;
using TeaTime.Data;

namespace TeaTime
{
	public class Utils
	{
		public static void CopyResourceToFile(string resourceName, string localName)
		{
			if (File.Exists(localName)) return;

			var teaRes = typeof(SampleValuesFactory).Assembly.GetManifestResourceStream(resourceName);
			if (teaRes == null) throw new Exception("cannot find resource: '" + resourceName + "'");
			IOUtils.WriteNewFile(localName, teaRes.CopyTo);
			File.Exists(localName).Should().Be.True();
		}

		public static void SwitchToEnglish()
		{
			Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");			
		}
	}
}