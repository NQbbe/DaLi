

using System;
using System.Linq;
using System.Data;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DaLi.Tests.DAL;
using Ckode.DaLi.Entities;
using System.Collections.Generic;

namespace DaLi.Tests.Tests
{

	[TestClass]
	public class PerformanceTest
	{
		//[TestMethod]
		public void CompareUncachedPerformance()
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			sw.Stop();
			sw.Reset();
			AllType.GetTable();

			const int reps = 10000;
			sw.Start();
			for (int i = 0; i < reps; i++)
			{
				var dt = AllType.GetTable();
				foreach (DataRow dr in dt.Rows)
				{
					AllType inst = new AllType();
					inst.Fill(dr);
				}
			}
			sw.Stop();
			Console.WriteLine("DaLi uncached (" + reps + "): " + sw.ElapsedMilliseconds.ToString());
			sw.Reset();
			sw.Start();
			for (int i = 0; i < reps; i++)
			{
				var dt = AllType.GetTable();
				foreach (DataRow dr in dt.Rows)
				{
					PerformanceAllType inst = new PerformanceAllType();
					inst.Fill(dr);
				}
			}
			sw.Stop();
			Console.WriteLine("Raw implementation uncached (" + reps + "): " + sw.ElapsedMilliseconds.ToString());
		}

		//[TestMethod]
		public void CompareCachedPerformance()
		{
			var dt = AllType.GetTableCached();

			const int reps = 10000;
			CachedDaliPerformanceParallel(1); // JIT
			CachedRawPerformance(1); // JIT
			
			var sw = Stopwatch.StartNew();
			CachedDaliPerformanceParallel(reps);
			sw.Stop();
			Console.WriteLine("DaLi parallel cached (" + reps + "): " + sw.ElapsedMilliseconds.ToString());

			sw.Reset();
			sw.Start();
			CachedDaliPerformance(reps);
			sw.Stop();
			Console.WriteLine("DaLi cached (" + reps + "): " + sw.ElapsedMilliseconds.ToString());

			sw.Reset();
			sw.Start();
			CachedDaliPerformanceList(reps);
			sw.Stop();
			Console.WriteLine("DaLi cached list (" + reps + "): " + sw.ElapsedMilliseconds.ToString());

			sw.Reset();
			sw.Start();
			CachedRawPerformance(reps);
			sw.Stop();
			Console.WriteLine("Raw implementation cached (" + reps + "): " + sw.ElapsedMilliseconds.ToString());
		}

		private void CachedDaliPerformanceList(int repeats)
		{
			for (int i = 0; i < repeats; i++)
			{
				AllType.GetListCached();
			}
		}

		private void CachedDaliPerformance(int repeats)
		{
			var dt = AllType.GetTableCached();
			for (int i = 0; i < repeats; i++)
			{
				foreach (DataRow dr in dt.Rows)
				{
					AllType inst = new AllType();
					inst.Fill(dr);
				}
			}
		}

		private void CachedDaliPerformanceParallel(int repeats)
		{
			var dt = AllType.GetTableCached();
			for (int i = 0; i < repeats; i++)
			{
				var instances = dt.Rows.Cast<DataRow>()
								.AsParallel()
								.AsOrdered()
								.Select(dr => { AllType result = new AllType(); result.Fill(dr); return result; });
				instances.ToList();
			}
		}

		private void CachedRawPerformance(int repeats)
		{
			var dt = AllType.GetTableCached();
			for (int i = 0; i < repeats; i++)
			{
				foreach (DataRow dr in dt.Rows)
				{
					PerformanceAllType inst = new PerformanceAllType();
					inst.Fill(dr);
				}
			}
		}

		[TestMethod]
		public void TestForProfiling()
		{
			// Used for profiling where the bottlenecks are (using unittest-profiler in #Develop)
			for (int i = 0; i < 10000; i++)
			{
				AllType.GetListCached();
			}
		}

		[TestMethod]
		public void PopulateTable()
		{
			AllType.EmptyTable();
			int count = 5;
			for (int i = 0; i < count; i++)
			{
				DateTime date = DateTime.Now;
				date = date.AddMilliseconds(1000 - date.Millisecond); // removes milliseconds
				string text = "Foobar";
				AllType row = new AllType();
				row.Bool = true;
				row.Date = date;
				row.Decimal = 4.2m;
				row.Double = 4.2;
				row.Float = 4.2f;
				row.Int = 42;
				row.Text = text;
				row.Uint = 42;
				row.Varchar = text;
				row.NullableInt = 42;
				row.Save();
			}
		}
	}
}
