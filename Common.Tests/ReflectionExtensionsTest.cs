#if false

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace TeaTime
{
	[TestClass]
	public class ReflectionExtensionsTest
	{
		[TestMethod]
		public void AssemblyGetFileInfoTest()
		{
			typeof (ReflectionExtensionsTest).Assembly.GetFileInfo().Exists.Should().Be.True();
		}
	}
}

#endif