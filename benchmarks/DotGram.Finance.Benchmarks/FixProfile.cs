using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Text;

namespace DotGram.Finance.Benchmarks;

static class FixProfile
{
	public static void Run(string path, string input, string workload, int iterations, string parser = "Fix44")
	{
		if (iterations <= 0) throw new ArgumentOutOfRangeException(nameof(iterations));
		if (input != "Bytes" && input != "Characters" && input != "String") throw new ArgumentException("Unknown input.");
		if (parser != "Fix44" && parser != "FixDispatch") throw new ArgumentException("Unknown parser.");
		var type = new AssemblyLoadContext("Profile target").LoadFromAssemblyPath(Path.GetFullPath(path)).GetType("DotGram.Finance.Fix." + parser)!;
		var wire = Fix44Benchmarks.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=100|40=2|44=12.50|");
		if (workload == "Groups")
		{
			var body = new StringBuilder("55=ABC|262=REQ|268=1000|");
			for (var i = 0; i < 1000; i++) body.Append("269=0|270=12.50|271=100|");
			wire = Fix44Benchmarks.Wire("W", body.ToString());
		}
		else if (workload == "Raw") wire = Fix44Benchmarks.Wire("A", "98=0|108=30|95=65536|96=" + new string('X', 65536) + "|");
		else if (workload == "Recovery") wire = "55=ABC\u0001bad\u000138=2\u00010=X\u000155=END\u0001tail";
		else if (workload == "Tag1" || workload == "Tag100" || workload == "Tag198") wire = workload.Substring(3) + "=X\u0001";
		else if (workload != "Order") throw new ArgumentException("Unknown workload.");
		var operation = FixGrammarComparisonBenchmarks.Operation(FixGrammarComparisonBenchmarks.Bind(type, input), wire, input);
		var fieldsPerOperation = operation();
		Measure(operation, workload is "Groups" or "Raw" ? 100 : 50000);
		Console.WriteLine($"Assembly SHA256: {Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))}");
		Console.WriteLine($"Parser={parser}");
		Console.WriteLine($"Workload={workload}, Input={input}, Iterations={iterations}, Fields/operation={fieldsPerOperation}");
		Action? start = null, stop = null, save = null;
		if (Environment.GetEnvironmentVariable("DOTGRAM_DOTTRACE_API") is { } apiPath)
		{
			var api = Assembly.LoadFrom(apiPath).GetType("JetBrains.Profiler.Api.MeasureProfiler")!;
			start = api.GetMethod("StartCollectingData", Type.EmptyTypes)!.CreateDelegate<Action>();
			stop = api.GetMethod("StopCollectingData", Type.EmptyTypes)!.CreateDelegate<Action>();
			save = api.GetMethod("SaveData", Type.EmptyTypes)!.CreateDelegate<Action>();
		}
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
		start?.Invoke();
		var allocated = GC.GetAllocatedBytesForCurrentThread();
		var gen0 = GC.CollectionCount(0);
		var gen1 = GC.CollectionCount(1);
		var gen2 = GC.CollectionCount(2);
		var began = Stopwatch.GetTimestamp();
		var fields = Measure(operation, iterations);
		var elapsed = Stopwatch.GetElapsedTime(began);
		allocated = GC.GetAllocatedBytesForCurrentThread() - allocated;
		gen0 = GC.CollectionCount(0) - gen0;
		gen1 = GC.CollectionCount(1) - gen1;
		gen2 = GC.CollectionCount(2) - gen2;
		stop?.Invoke();
		save?.Invoke();
		if (fields != (long)fieldsPerOperation * iterations) throw new InvalidOperationException("Field count changed.");
		Console.WriteLine(FormattableString.Invariant($"Seconds={elapsed.TotalSeconds:F6}; ns/op={elapsed.TotalNanoseconds / iterations:F2}; B/op={(double)allocated / iterations:F2}; GC={gen0}/{gen1}/{gen2}; Fields={fields}"));
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	static long Measure(Func<int> operation, int iterations)
	{
		long count = 0;
		for (var i = 0; i < iterations; i++) count += operation();
		return count;
	}
}
