using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Text;

using BenchmarkDotNet.Attributes;

using DotGram.Finance.Fix44;
using DotGram.Finance.Fix;

namespace DotGram.Finance.Benchmarks;

[MemoryDiagnoser]
public class FixInitializationBenchmarks
{
	[Params("Create", "Empty", "One", "Order", "Orders16")]
	public string Phase { get; set; } = "Order";
	static Type? oldType;
	Func<int> previous = null!;
	Func<int> simplified = null!;

	internal static Type TypeOf(bool old)
	{
		return !old ? typeof(FixParser) : oldType ??=
		Environment.GetEnvironmentVariable("DOTGRAM_FIX_BASELINE") is { } path
			? FixGrammarComparisonBenchmarks.PreviousType(path) : typeof(Fix44Parser);
	}

	[GlobalSetup]
	public void Setup()
	{
		previous = Operation(TypeOf(true), Phase);
		simplified = Operation(TypeOf(false), Phase);
		if (previous() != simplified()) throw new InvalidOperationException("Different field counts.");
	}

	internal static Func<int> Operation(Type type, string phase)
	{
		var order = Fix44Benchmarks.Wire("D", "11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=100|40=2|44=12.50|");
		var wire = phase == "Empty" ? "" : phase == "One" ? "11=ORDER\u0001" : phase == "Orders16" ? string.Concat(Enumerable.Repeat(order, 16)) : order;
		var bytes = Encoding.Latin1.GetBytes(wire);
		var parse = FixGrammarComparisonBenchmarks.Bind(type, "Bytes");
		return () =>
		{
			using var input = new MemoryStream(bytes, false);
			var fields = parse(input);
			if (phase == "Create") { GC.KeepAlive(fields); return 0; }
			var count = 0;
			foreach (var field in fields) count++;
			return count;
		};
	}

	internal static void Probe(bool old)
	{
		var type = TypeOf(old);
		var parser = type.Assembly.GetType(type.Namespace + (type.Name == "Fix44Parser" || type.Name == "Fix44" && type.Namespace == "DotGram.Examples.Finance" ? ".Fix44Grammar" : ".FixGrammar"))!;
		var start = Stopwatch.GetTimestamp();
		RuntimeHelpers.RunClassConstructor(parser.TypeHandle);
		Console.WriteLine($"Class initialization: {Stopwatch.GetElapsedTime(start).TotalMilliseconds:F3} ms");
		foreach (var method in parser.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
			.Where(m => m.Name.Contains("ReadFields_Bytes", StringComparison.Ordinal) &&
				(m.Name.Contains("Materialize", StringComparison.Ordinal) || m.Name == "Recognize_DotGram_Buffered_ReadFields_Bytes")))
			Console.WriteLine($"IL {method.Name}: {method.GetMethodBody()?.GetILAsByteArray()?.Length} bytes");
		var operation = Operation(type, "Order");
		for (var i = 0; i < 20000; i++) operation();
	}

	[Benchmark(Baseline = true)]
	public int Previous()
	{
		return previous();
	}

	[Benchmark]
	public int Simplified()
	{
		return simplified();
	}
}
