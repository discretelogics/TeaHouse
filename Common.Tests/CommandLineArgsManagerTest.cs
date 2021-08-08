// copyright discretelogics © 2011
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpTestsEx;

namespace TeaTime
{
	class SampleOptions
	{
		public string Assembly { get; set; }
		public string Analysis { get; set; }
		public string Action { get; set; }
		public string ResultDir { get; set; }
		public string Parameters { get; set; }
		public int UDPPort { get; set; }
	}

	[TestClass]
	public class CommandLineArgsManagerTest
	{
		[TestMethod]
		public void ReadCommandLineAssignsAction()
		{
			var args = new string[] {"/action:bunny"};
			var options = CommandLineArgsManager<SampleOptions>.FromArgs(args);
			Assert.AreEqual("bunny", options.Action);
		}

		[TestMethod]
		public void ReadCommandLineAssignsActionAndAnalysis()
		{
			var args = new string[] {"/action:bunny", "/analysis:x1234"};
			var options = CommandLineArgsManager<SampleOptions>.FromArgs(args);
			Assert.AreEqual("bunny", options.Action);
			Assert.AreEqual("x1234", options.Analysis);
		}

		[TestMethod]
		public void SetPropertiesFromLineIgnoresAdditionalValues()
		{
			var args = new string[] {"/action:bunny", "/analysis:x1234", "/additional:222"};
			var options = CommandLineArgsManager<SampleOptions>.FromArgs(args);
			Assert.AreEqual("bunny", options.Action);
			Assert.AreEqual("x1234", options.Analysis);
		}

		[TestMethod]
		public void CommandLineArgsRoundTrip()
		{
			var options = new SampleOptions();
			options.Analysis = "xana";
			options.Action = "xact";
			options.Assembly = "xass";
			options.Parameters = "xpar";
			options.ResultDir = "xres";

			string sargs = CommandLineArgsManager<SampleOptions>.ToArgs(options);

			var options2 = CommandLineArgsManager<SampleOptions>.FromArgs(sargs.Split());

			Assert.AreEqual(options.Analysis, options2.Analysis);
			Assert.AreEqual(options.Action, options2.Action);
			Assert.AreEqual(options.Assembly, options2.Assembly);
			Assert.AreEqual(options.ResultDir, options2.ResultDir);
			Assert.AreEqual(options.Parameters, options2.Parameters);
		}

		[TestMethod]
		public void IntegerValuetest()
		{
			var options = new SampleOptions();
			options.Analysis = "xana";
			options.UDPPort = 1172;

			string sargs = CommandLineArgsManager<SampleOptions>.ToArgs(options);

			var options2 = CommandLineArgsManager<SampleOptions>.FromArgs(sargs.Split());

			options2.Analysis.Should().Be("xana");
			options2.UDPPort.Should().Be(1172);
		}
	}
}