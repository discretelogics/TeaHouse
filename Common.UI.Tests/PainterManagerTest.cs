using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;
using TeaTime.Chart.Painters;

namespace TeaTime
{
    [TestClass]
    public class PainterManagerTest
    {
        [TestMethod]
        public void ExportTest()
        {
            var c = PainterManager.GetExportContainer();
            //IEnumerable<IConverter> con = c.GetExportedValues<IConverter>();
            //con.Should().Not.Be.Null();
            //con.Count().Should().Be.EqualTo(1);
            //con.First().Should().Be.OfType<OHLCVConverter>();
        }
    }
}
