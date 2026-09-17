using DotGram;

using BenchmarkDotNet.Attributes;

namespace DotGram.Benchmarks;

/// <summary>A tiny valued parser called repeatedly, with storage selected at generation time.</summary>
[MemoryDiagnoser]
public class ValueStorageBenchmarks
{
	readonly string _input = new string('a', 2);
	[Benchmark(Baseline = true)] public int Flat() => SmallValues.ParseStart(_input);
	[Benchmark] public int Adaptive() => SmallValues.Adaptive.ParseStart(_input);
	[Benchmark] public int Paged() => SmallValues.Paged.ParseStart(_input);
}

[Gram("""
	Start : @int = items: Item+ & when @(items.Length > 0) => @(items.Length)
	Item : @int = 'a' => @(1)
	parse Start
	""", Carrier = GramCarrier.Tape, ValueStorage = GramValueStorage.Flat)]
[GramOptions(Suffix = "Adaptive", ValueStorage = GramValueStorage.Adaptive)]
[GramOptions(Suffix = "Paged", ValueStorage = GramValueStorage.Paged)]
public static partial class SmallValues { }
