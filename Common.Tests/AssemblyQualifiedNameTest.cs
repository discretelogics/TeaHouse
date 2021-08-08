using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace DiscreteLogics.Common.Tests
{
	[TestClass]
	public class AssemblyQualifiedNameTest
	{
		[TestMethod]
		public void NameTest()
		{
			AssemblyQualifiedName aqn = typeof (int).AssemblyQualifiedName;
			aqn.Name.Should().Be("Int32");
		}

		[TestMethod]
		public void FullNameTest()
		{
			AssemblyQualifiedName aqn = typeof(int).AssemblyQualifiedName;
			aqn.FullName.Should().Be("System.Int32");
		}

		[TestMethod]
		public void AssignmentTest()
		{
			AssemblyQualifiedName aqn = typeof(int).AssemblyQualifiedName;
			AssemblyQualifiedName aqn2 = aqn;
			aqn2.Name.Should().Be("Int32");
		}

		[TestMethod]
		public void AQNIsSerializableByDataContractSerializerTest()
		{
			AssemblyQualifiedName aqn = typeof(int).AssemblyQualifiedName;
			DataContractSerializer serializer = new DataContractSerializer(typeof(AssemblyQualifiedName));
			var ms = new MemoryStream();
			serializer.WriteObject(ms, aqn);
		}
	}
}
