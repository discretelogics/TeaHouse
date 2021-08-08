using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace TeaTime.Tree
{
    [TestClass]
    public class FolderTest
    {
        // tests use a sample tree that has root "m:" with child nodes below named "name1", "name2", ...

        [TestMethod]
        public void DemandLoadingTest()
        {            
            Folder.ItemFactory = caption =>
                {
                    Console.WriteLine("get children for node " + caption);
                    if (caption.Length > 20) return Enumerable.Empty<INode>();
                    return Enumerable.Range(1, 5).Select(i => new Folder(caption + "/" + "folder" + i));
                };
            Folder m = new Folder("m:");
            m.Items.Count.Should().Be.EqualTo(1); // not expanded
            m.IsExpanded = true;
            m.Items.Count.Should().Be.EqualTo(5); // expanded
        }

        [TestMethod]
        public void FindNodeTest1()
        {            
            Folder.ItemFactory = caption =>
            {
                Console.WriteLine("get children for node " + caption);
                if (caption.Length > 20) return Enumerable.Empty<INode>();
                return Enumerable.Range(1, 5).Select(i => new Folder(caption + "/" + "folder" + i));
            };
            Folder m = new Folder("m:");
            m.IsExpanded = true;
            m.Items.OfType<Folder>().ForEach(k => k.IsExpanded = true);

            Folder parent;
            var n = Folder.FindNode(new DirectoryInfo("m:/folder3/folder1"), m, out parent);
            parent.Should().Not.Be.Null();
            parent.Name.Should().Be("folder3");
            n.Name.Should().Be("folder1");
        }

        [TestMethod]
        public void FindNodeTest2()
        {            
            Folder.ItemFactory = caption =>
            {
                Console.WriteLine("get children for node " + caption);
                if (caption.Length > 20) return Enumerable.Empty<INode>();
                return Enumerable.Range(1, 5).Select(i => new Folder(caption + "/" + "folder" + i))
                                             .Union(
                       Enumerable.Range(1, 3).Select(i => new Folder(caption + "/" + "file" + i))                                             
                                             );
            };
            Folder m = new Folder("m:");
            m.IsExpanded = true;
            m.Items.OfType<Folder>().ForEach(k => k.IsExpanded = true);

            Folder parent;
            var n = Folder.FindNode(new DirectoryInfo("m:/folder4/file2"), m, out parent);
            parent.Should().Not.Be.Null();
            parent.Name.Should().Be("folder4");
            n.Name.Should().Be("file2");
        }
    }
}
