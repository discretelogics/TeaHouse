using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace TeaTime
{
    [TestClass]
    public class IOTest
    {
        const string root =         @"C:\MyRoot";
        const string absoluteFile = @"C:\MyRoot\Test1\TestFile1.fil";
        const string relativeFile =            @"Test1\TestFile1.fil";

		public TestContext TestContext { get; set; }

        #region ConvertToAbsolutePath
        [TestMethod]
        public void ConvertToAbsolutePath()
        {
            string examinee = IOUtils.ConvertToAbsolutePath(root, relativeFile);
            AssertEqualPaths(absoluteFile, examinee);
        }

        [TestMethod]
        public void ConvertToAbsolutePathWithRootNull()
        {
            string examinee = IOUtils.ConvertToAbsolutePath(null, relativeFile);
            AssertEqualPaths(relativeFile, examinee);
        }

        [TestMethod]
        public void ConvertToAbsolutePathWithRelativePathNull()
        {
            string examinee = IOUtils.ConvertToAbsolutePath(root, null);
            Assert.IsNull(examinee);
        }

        [TestMethod]
        public void ConvertToAbsolutePathWithRelativePathAbsolute()
        {
            string examinee = IOUtils.ConvertToAbsolutePath(root, absoluteFile);
            AssertEqualPaths(absoluteFile, examinee);
        }

        [TestMethod]
        public void ConvertToAbsolutePathWithRelativePathAbsoluteOnOtherRoot()
        {
            string examinee = IOUtils.ConvertToAbsolutePath(@"C:\OtherRoot", absoluteFile);
            AssertEqualPaths(absoluteFile, examinee);
        }
        #endregion

        #region ConvertToRelativePath
        [TestMethod]
        public void ConvertToRelativePath()
        {
            string examinee = IOUtils.ConvertToRelativePath(root, absoluteFile);
            AssertEqualPaths(relativeFile, examinee);
        }

        [TestMethod]
        public void ConvertToRelativePathWithRootNull()
        {
            string examinee = IOUtils.ConvertToRelativePath(null, absoluteFile);
            AssertEqualPaths(absoluteFile, examinee);
        }

        [TestMethod]
        public void ConvertToRelativePathWithAbsolutPathNull()
        {
            string examinee = IOUtils.ConvertToRelativePath(root, null);
            Assert.IsNull(examinee);
        }

        [TestMethod]
        public void ConvertToRelativePathWithAbsolutPathOtherRoot()
        {
            string examinee = IOUtils.ConvertToRelativePath(@"C:\OtherRoot", absoluteFile);
            AssertEqualPaths(absoluteFile, examinee);
        }

        [TestMethod]
        public void ConvertToRelativePathWithAbsolutPathRelative()
        {
            string examinee = IOUtils.ConvertToRelativePath(root, relativeFile);
            AssertEqualPaths(relativeFile, examinee);
        }
        #endregion

        void AssertEqualPaths(string expected, string actual)
        {
            expected = expected.Replace("/", "\\");
            actual = actual.Replace("/", "\\");
            Assert.AreEqual(expected, actual);
        }

    	[TestMethod]
    	public void IsFileTest()
    	{
    		var dir = Directory.CreateDirectory("dir1").FullName;
    		IOUtils.IsFile("dir1").Should().Be.False();
			var fullFilePath = Path.GetFullPath("file1.txt");
			File.WriteAllText(dir.PathCombine(fullFilePath), "dummy text");
			IOUtils.IsFile("file1.txt").Should().Be.True();
    	}

		[TestMethod]
		public void IsFolderTest()
		{
			var dir = Directory.CreateDirectory("dir1").FullName;
			IOUtils.IsFolder("dir1").Should().Be.True();
			var fullFilePath = Path.GetFullPath("file1.txt");
			File.WriteAllText(dir.PathCombine(fullFilePath), "dummy text");
			IOUtils.IsFolder("file1.txt").Should().Be.False();
		}

    	[TestMethod]
		public void GetDirectoryNameTest()
    	{
    		DirectoryInfo dir = Directory.CreateDirectory("Bunny");
    		Console.WriteLine(dir.FullName);
    		IOUtils.GetDirectoryName(dir.FullName).Should().Be("Bunny");
    	}

    	[TestMethod]
    	public void IsFileInUseTest()
    	{
    		using (var f = File.OpenWrite("bunny.txt"))
    		{
    			IOUtils.IsFileInUse("bunny.txt").Should().Be.True();
    		}
			IOUtils.IsFileInUse("bunny.txt").Should().Be.False();
    	}

		[TestMethod]
		public void IsFileInUseReturnsFalseIfNotExistsTest()
		{
			IOUtils.IsFileInUse("notExists.txt").Should().Be.False();
		}

    	[TestMethod]
		public void AreEqualPathsTest()
    	{
    		var path1 = "y:/aaa/bbb/f.txt";
    		var path2 = Path.GetFullPath(path1);
			Console.WriteLine(path1);
			Console.WriteLine(path2);
    		IOUtils.AreEqualPaths(path1, path2).Should().Be.True();
    	}

		[TestMethod]
		public void AreEqualPathsTest2()
		{
			var path1 = "y:/aaa/bbb/f.txt";
			var path2 = Path.GetFullPath(path1) + "notequal";
			Console.WriteLine(path1);
			Console.WriteLine(path2);
			IOUtils.AreEqualPaths(path1, path2).Should().Be.False();
		}

    	[TestMethod]
		public void GetUniqueDirectoryTest()
    	{
			var rootDir = TestContext.TestRunResultsDirectory;
			var subDir = IOUtils.GetUniqueDirectory(TestContext.TestRunResultsDirectory, "Sub");
			subDir.Should().Be.EqualTo(rootDir + @"\Sub");
    		Directory.CreateDirectory(subDir);
			subDir = IOUtils.GetUniqueDirectory(TestContext.TestRunResultsDirectory, "Sub");
			subDir.Should().Be.EqualTo(rootDir + @"\Sub (1)");
			Directory.CreateDirectory(subDir);
			subDir = IOUtils.GetUniqueDirectory(TestContext.TestRunResultsDirectory, "Sub");
			subDir.Should().Be.EqualTo(rootDir + @"\Sub (2)");
    	}

		[TestMethod]
		public void GetUniqueFileTest()
		{
			var rootDir = TestContext.TestRunResultsDirectory;
			var filename = IOUtils.GetUniqueFile(TestContext.TestRunResultsDirectory, "Bunny", ".txt");
			filename.Should().Be.EqualTo(rootDir + @"\Bunny.txt");
			File.WriteAllText(filename, "dummy");
			filename = IOUtils.GetUniqueFile(TestContext.TestRunResultsDirectory, "Bunny", ".txt");
			filename.Should().Be.EqualTo(rootDir + @"\Bunny (1).txt");
			File.WriteAllText(filename, "dummy");
			filename = IOUtils.GetUniqueFile(TestContext.TestRunResultsDirectory, "Bunny", ".txt");
			filename.Should().Be.EqualTo(rootDir + @"\Bunny (2).txt");
			File.WriteAllText(filename, "dummy");
		}

    	[TestMethod]
		public void PathCompactPathExTest()
    	{
			var di = new DirectoryInfo(TestContext.TestRunResultsDirectory);
			while(di.FullName.Length < 150)
			{
				di = di.CreateSubdirectory("a sub directory");
			}
    		di.FullName.Length.Should().Be.GreaterThanOrEqualTo(150);
			var compact = IOUtils.GetCompactPath(di.FullName, 50);

    		Console.WriteLine(compact);

			compact.Substring(0, 10).Should().Be.EqualTo(di.FullName.Substring(0, 10));
    		compact.Should().Contain("...");
    		compact.Length.Should().Be.LessThan(50);

            compact = IOUtils.GetCompactPath(di.FullName, 5);
            compact.Length.Should().Be.LessThan(50);
            compact.Length.Should().Be.GreaterThanOrEqualTo(1);

            compact = IOUtils.GetCompactPath(null, 50);
            compact.Should().Be.Null();

            compact = IOUtils.GetCompactPath(String.Empty, 50);
            compact.Should().Be(String.Empty);
    	}

    	[TestMethod]
		public void PathCombineTest()
    	{
    		var fullpath = TestContext.TestRunResultsDirectory.PathCombine("sub1", "sub2", "sub3");
    		Console.WriteLine(fullpath);
    		Directory.CreateDirectory(fullpath);
    		var di = new DirectoryInfo(fullpath);
			di.Name.Should().Be("sub3");
			di.Parent.Name.Should().Be("sub2");
			di.Parent.Parent.Name.Should().Be("sub1");
    	}

    	[TestMethod]
		public void CombineToFileTest()
    	{
    		var dir = new DirectoryInfo(TestContext.TestRunResultsDirectory);
    		var filepath = dir.CombineToFile("file.txt").FullName;
			Console.WriteLine(filepath);

			File.WriteAllText(filepath, "dummy text");
    		File.Exists(filepath).Should().Be.True();
    	}
    }
}
