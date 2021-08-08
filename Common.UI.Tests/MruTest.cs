using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using SharpTestsEx;

namespace TeaTime
{
    [TestClass]
    public class MruTest
    {
        [TestMethod]
        public void StoreAndReadStrings()
        {
            var colors = new ObservableMruCollection<string>();
            colors.Add("s1");
            colors.Add("s2");
            colors.Add("s3");
            colors.Add("s4");
            SettingsManager.Instance.Store("UnitTest", "RecentStrings", colors);

            var read = SettingsManager.Instance.Read<ObservableMruCollection<string>>("UnitTest", "RecentStrings", () => null);
            read.Should().Not.Be.Null();
            read.Count.Should().Be(4);
            read[0].Should().Be("s4");
            read[1].Should().Be("s3");
            read[2].Should().Be("s2");
            read[3].Should().Be("s1");
        }

        [TestMethod]
        public void StoreAndReadColors()
        {
            var colors = new ObservableMruCollection<Color>();
            colors.Add(Colors.Black);
            colors.Add(Colors.Blue);
            colors.Add(Colors.Pink);
            colors.Add(Colors.Green);
            SettingsManager.Instance.Store("UnitTest", "RecentColors", colors);

            var read = SettingsManager.Instance.Read<ObservableMruCollection<Color>>("UnitTest", "RecentColors", () => null);
            read.Should().Not.Be.Null();
            read.Count.Should().Be(4);
            read[0].Should().Be(Colors.Green);
            read[1].Should().Be(Colors.Pink);
            read[2].Should().Be(Colors.Blue);
            read[3].Should().Be(Colors.Black);
        }
    }
}
