using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TeaTime.Elements
{
	public struct Trade
	{
		public double Profit { get { throw new NotImplementedException(); } }
	}

	public struct Signal
	{
		string BuySell;
	}

	public class PerformanceMetrics
	{
		public PerformanceMetrics(object streamOfTradesWithActualProfitLosses)
		{
			
		}

		public static double GetEquityPercentFast(object streamOfTradesWithActualProfitLosses)
		{
			throw new NotImplementedException();
		}
	}

	public class ParametricEvaluation
	{
		// how - brute force, simulated annealing

		// write it to a file: parameters, outcome
	}
}
