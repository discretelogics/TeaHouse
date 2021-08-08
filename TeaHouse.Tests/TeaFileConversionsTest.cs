using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;
using TeaTime.Data;
using TeaTime.Diagnostics;

namespace TeaTime
{
    [TestClass]
    public class TeaFileConversionsTest
    {
        public TestContext TestContext { get; set; }

        [TestInitialize]
        public void Clean()
        {
            Utils.SwitchToEnglish();
            var di = new DirectoryInfo(this.TestContext.TestRunResultsDirectory);
            if (di.Exists) di.Clear();
            Environment.CurrentDirectory = di.FullName;
        }

        // tbd: add file as resource
        //[TestMethod]
        //public void ReadTestFile()
        //{
        //    Utils.CopyResourceToFile("TeaTime.AA.day.tea", "AA.day.tea");
        //    using (TeaFile<OHLCV> tf = TeaFile<OHLCV>.OpenRead("AA.day.tea"))
        //    {
        //        tf.Items.Count.Should().Be(4076);
        //    }
        //}

        [TestMethod]
        public void Convert_OHLCV_to_B()
        {
            const string filename = "TeaFileConversionsTest.Convert_OHLCV_to_B_by_Function";
            File.Delete(filename);
            using (var tf = TeaFile<OHLCV>.Create(filename))
            {
                var bar = new OHLCV();
                bar.Open = 11;
                bar.High = 12;
                bar.Low = 9;
                bar.Close = 12.1;
                bar.Volume = 711;
                tf.Write(bar);
            }

            // knowing the type, open teafile typed
            using (TeaFile<OHLCV> tf = TeaFile<OHLCV>.OpenRead(filename, ItemDescriptionElements.None))
            {
                tf.Count.Should().Be(1);
                // then use item converter
                B b = tf.GetItemsConverted<B>(null)[0];
                b.Open.Should().Be(11);
                b.Close.Should().Be(12.1);
            }

            // or use TeaFactory, which does not ask for the original type
            var factory = new TeaFactory();
            using (ITeaFile tf = factory.OpenReadTyped(filename))
            {
                // then use item converter
                B b = tf.GetItemsConverted<B>(null)[0];
                b.Open.Should().Be(11);
                b.Close.Should().Be(12.1);
            }
        }

        [TestMethod]
        [Ignore] // fails, because nested types are not yet implemented by TeaFactory.CreateConverterDelegate<TA, TB>()
        public void Convert_OHLCV_to_B_by_Reflection()
        {
            const string filename = "TeaFileConversionsTest.Convert_OHLCV_to_B_by_Reflection";
            File.Delete(filename);
            using (TeaFile<Event<OHLCV>> tf = TeaFile<Event<OHLCV>>.Create(filename))
            {
                var bar = new OHLCV();
                bar.Open = 11;
                bar.High = 12;
                bar.Low = 9;
                bar.Close = 12.1;
                bar.Volume = 711;
                tf.Write(new Event<OHLCV>(new DateTime(2000, 1, 2, 3, 4, 5), bar));
            }

            using (TeaFile<Event<OHLCV>> tf = TeaFile<Event<OHLCV>>.OpenRead(filename))
            {
                tf.Count.Should().Be(1);
                Event<B> b = tf.GetItemsConverted<Event<B>>(null)[0];
                b.Value.Open.Should().Be(11);
                b.Value.Close.Should().Be(12.1);
            }
        }

        [TestMethod]
        [Ignore]  // fails, because nested types are not yet implemented by TeaFactory.CreateConverterDelegate<TA, TB>()
        public void Convert_OHLCV_to_B_by_Reflection_SourceTypeByDescription()
        {
            var original = "Convert_OHLCV_to_B_by_Reflection_SourceTypeByDescription_OHLCV.tea";
            var targetFile = "Convert_OHLCV_to_B_by_Reflection_SourceTypeByDescription_OHLCV.converted.tea";
            File.Delete(original);
            File.Delete(targetFile);

            using (var tf = TeaFile<Event<OHLCV>>.Create(original))
            {
                var bar = new OHLCV();
                bar.Open = 11;
                bar.High = 12;
                bar.Low = 9;
                bar.Close = 12.1;
                bar.Volume = 711;
                tf.Write(new Event<OHLCV>(new DateTime(2000, 1, 2, 3, 4, 5), bar));
            }

            var s = TeaFileSnapshot.Get(original);

            //var c = new TeaFileConverter<Event<B>>(s.Description.ItemDescription);
            //c.Explain();
            //c.Convert(original, targetFile);

            using (var tf = TeaFile<Event<B>>.OpenRead(targetFile))
            {
                tf.Count.Should().Be(1);
                Event<B> b = tf.Items[0];
                b.Value.Open.Should().Be(11);
                b.Value.Close.Should().Be(12.1);
                b.Time.Should().Be.EqualTo(new DateTime(2000, 1, 2, 3, 4, 5));
            }
        }

        [TestMethod]
        public void UnAssignedFields()
        {
            var original = "UnAssignedFields_OHLCV.tea";
            File.Delete(original);
            
            using (var tf = TeaFile<Event<OHLCV>>.Create(original))
            {
                var bar = new OHLCV();
                bar.Open = 11;
                bar.High = 12;
                bar.Low = 9;
                bar.Close = 12.1;
                bar.Volume = 711;
                tf.Write(new Event<OHLCV>(new DateTime(2000, 1, 2, 3, 4, 5), bar));
            }

            var s = TeaFileSnapshot.Get(original);            
        }

        [TestMethod]
        [Ignore]    // fails, because nested types are not yet implemented by TeaFactory.CreateConverterDelegate<TA, TB>()
        public void Convert_OHLCV_To_B_With_Description()
        {
            NameValueCollection nv = new NameValueCollection();
            nv.Add("name1", "desc1");
            nv.Add("name2", "desc2");
            using (var tf = TeaFile<Event<OHLCV>>.Create("A.tea", "some content desc", nv))
            {
                var bar = new OHLCV();
                bar.Open = 11;
                bar.High = 12;
                bar.Low = 9;
                bar.Close = 12.1;
                bar.Volume = 711;
                tf.Write(new Event<OHLCV>(new DateTime(2000, 1, 2, 3, 4, 5), bar));
            }           

            using (var tf = TeaFile<Event<OHLCV>>.OpenRead("A.tea", ItemDescriptionElements.None))
            {
                tf.Count.Should().Be(1);
                Event<B> b = tf.GetItemsConverted<Event<B>>(null)[0];
                b.Value.Open.Should().Be(11);
                b.Value.Close.Should().Be(12.1);
                tf.Description.Should().Not.Be.Null();
                tf.Description.NameValues.Should().Not.Be.Null();
                tf.Description.NameValues.Select(nvv => nvv.Name).Should().Have.SameValuesAs("name1", "name2");
                tf.Description.NameValues.Select(nvv => nvv.GetValue<string>()).Should().Have.SameValuesAs("desc1", "desc2");
                tf.Description.ContentDescription.Should().Be.EqualTo("some content desc");
            }
        }

        #region Nested type: B

        struct B
        {
            public double Open;
            public double Close;
        }

        #endregion
    }
}