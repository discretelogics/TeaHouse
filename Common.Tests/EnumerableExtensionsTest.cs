// copyright discretelogics © 2011
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace TeaTime
{
	[TestClass]
	public class EnumerableExtensionsTest
	{
		[TestMethod]
		public void SelectAllEqualOrDefaultTest()
		{
			var list = new List<int>();
			list.Add(3);
			list.Add(4);
			//list.SelectAllEqualOrDefault()

			// todo
		}

		[TestMethod]
		public void ForEachReverseTest()
		{
			var list = Enumerable.Range(10, 7).ToList();
			string s = "";
			list.ForEachReverse(x => s += "{0},".Formatted(x));
			s.Should().Be("16,15,14,13,12,11,10,");
		}

		[TestMethod]
		public void ForEachIndexTest()
		{
			var list = Enumerable.Range(5, 3).Select(n => (double) n).ToList();
			string s = "";
			list.ForEachIndex((i, x) => s += "{0}-{1},".Formatted(i, x));
			s.Should().Be("0-5,1-6,2-7,");
		}

		[TestMethod]
		public void CollectAndRemoveTest()
		{
			var list = Enumerable.Range(10, 8).ToList();
			list.Should().Have.SameSequenceAs(10,11,12,13,14,15,16,17);
			var evenValues = list.CollectAndRemove(x => x % 2 == 0);
			evenValues.Should().Have.SameSequenceAs(10, 12, 14, 16);
			list.Should().Have.SameSequenceAs(11, 13, 15, 17);

			var smallValues = list.CollectAndRemove(x => x < 14);
			smallValues.Should().Have.SameSequenceAs(11, 13);
			list.Should().Have.SameValuesAs(15, 17);

			var noValues = list.CollectAndRemove(x => false);
			noValues.Should().Be.Empty();
			list.Should().Have.SameValuesAs(15, 17);

			var lastValues = list.CollectAndRemove(x => true);
			lastValues.Should().Have.SameSequenceAs(15, 17);
			list.Should().Be.Empty();
		}

		[TestMethod]
		public void ForEachTest()
		{
			var list = Enumerable.Range(10, 4).ToList();
			string s = "";
			list.ForEach(x => s += "{0},".Formatted(x));
			s.Should().Be("10,11,12,13,");
		}

		[TestMethod]
		public void ObservableCollectionRemoveTest()
		{
			var os = new ObservableCollection<int>();
			Enumerable.Range(10, 8).ForEach(os.Add);
			os.Should().Have.SameSequenceAs(10, 11, 12, 13, 14, 15, 16, 17);
			os.Remove(x => false);
			os.Should().Have.SameSequenceAs(10, 11, 12, 13, 14, 15, 16, 17);
			os.Remove(x => x > 12);
			os.Should().Have.SameSequenceAs(10, 11, 12);
			os.Remove(x => true);
			os.Should().Be.Empty();
		}

		[TestMethod]
		public void SelectChildrenFirstTest()
		{
			DirectoryInfo root = new DirectoryInfo("root");
			var d1 = root.CreateSubdirectory("dir1");
			d1.CreateSubdirectory("d1sub1");
			d1.CreateSubdirectory("d1sub2");
			
			var d2 = root.CreateSubdirectory("dir2");
			d2.CreateSubdirectory("d2sub1");

			var s = root.SelectChildrenFirst(d => d.GetDirectories()).Select(d => d.Name).Joined(",");
			s.Should().Be("d1sub1,d1sub2,dir1,d2sub1,dir2,root");
		}

		[TestMethod]
		public void AsDepthFirstEnumerableTest()
		{
			DirectoryInfo root = new DirectoryInfo("root");
			var d1 = root.CreateSubdirectory("dir1");
			d1.CreateSubdirectory("d1sub1");
			d1.CreateSubdirectory("d1sub2");

			var d2 = root.CreateSubdirectory("dir2");
			d2.CreateSubdirectory("d2sub1");

			var s = root.AsDepthFirstEnumerable(d => d.GetDirectories()).Select(d => d.Name).Joined(",");
			s.Should().Be("root,dir1,d1sub1,d1sub2,dir2,d2sub1");
		}
	}
}