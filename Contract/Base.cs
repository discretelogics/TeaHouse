using System;
using System.ComponentModel.Composition;

namespace TeaTime.Elements
{
	[InheritedExport]
	public class Analysis
	{
		public virtual void Setup() { }
	}

	public interface IStreamAnalysis
	{
		void StreamSetup();
	}

	public abstract class StreamAnalysis : Analysis, IStreamAnalysis
	{
		public abstract void StreamSetup();
	}

	public interface IStreamAnalysis<TInput, TOutput> : IStreamAnalysis
	{
		InputStream<TInput> Input { get; set; }
		OutputStream<TOutput> Output { get; set; }
	}

	public interface IAnalysisWithOutput<T> : IStreamAnalysis
	{
		OutputStream<T> Output { get; }
	}

	public interface IAnalysisWithInput<T> : IStreamAnalysis
	{
		InputStream<T> Output { get; }
	}

	public class InputStream<T>
	{
		public string ShortName { get; set; }
		public string Description { get; set; }
		public IObservable<T> Events { get; set; }
	}

	public class InputStreamDescriptor
	{
		
	}

	public abstract class OutputStream<T> : IObservable<T>
	{
		public string Description { get; set; }
		public abstract void Write(T value);
		public IDisposable Subscribe(IObserver<T> observer)
		{
			throw new NotImplementedException();
		}
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class InputAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Property)]
	public class OutputAttribute : Attribute
	{
		public string Name { get; set; }
	}

	//[AttributeUsage(AttributeTargets.Method)]
	//public class AnalysisAttribute : Attribute
	//{
	//}

	public class Trades
	{
		public void AddTrade()
		{
			
		}

		public double GetPerformanceMetric()
		{
			return 0;
		}
	}

	public class Simulation
	{
		public static void Run(StreamAnalysis sa, SimulationArgs simulationArgs)
		{
			
		}
	}

	public class SimulationArgs
	{
		
	}
}
