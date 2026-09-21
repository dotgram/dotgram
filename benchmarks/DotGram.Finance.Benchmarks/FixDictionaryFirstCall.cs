using System;
using System.IO;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

using DotGram.Finance.Fix;

namespace DotGram.Finance.Benchmarks;

/// <summary>
/// What a schema costs before the first message: their dictionary against ours, read once.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Cold, because warm would be the wrong question.</strong> A consumer reads a dictionary
/// once, when the process starts; BenchmarkDotNet's ordinary iteration measures the warm cost of
/// doing it again, with the file in the operating system's cache, the XML reader's types already
/// loaded and every method already compiled. That is a real number about a thing nobody does. So
/// this runs cold: one invocation a sample, a fresh process a launch, ten launches, and the median
/// and the range of those reported rather than a mean.
/// </para>
/// <para>
/// <strong>Both sides read the same file</strong> — the FIX 4.4 dictionary that ships with
/// QuickFIXn.FIX44 1.14.1 — and it is read once in the setup so that neither side pays for the
/// first disk read. What is left is the parsing and the building, which is what the row is about.
/// </para>
/// <para>
/// It is the asymmetry again: they have no other way to get a schema, and we have this one AND the
/// one compiled into the package, which costs nothing before the first message.
/// </para>
/// </remarks>
[SimpleJob(RunStrategy.ColdStart, launchCount: 10, warmupCount: 0, iterationCount: 1, invocationCount: 1)]
[MemoryDiagnoser]
public class FixDictionaryFirstCall
{
	static string Dictionary() => Path.Combine(AppContext.BaseDirectory, "FIX44.xml");

	[GlobalSetup]
	public void Setup()
	{
		// The file into the operating system's cache, so that whichever side runs first does not
		// pay the disk for both.
		_ = File.ReadAllBytes(Dictionary());
	}

	[Benchmark(Description = "ours: LoadDictionary, cold")]
	public void Ours()
	{
		using var file = File.OpenRead(Dictionary());

		FixParser.LoadDictionary(file);
	}

	[Benchmark(Description = "QuickFIX/n: new DataDictionary, cold")]
	public object Theirs() => new QuickFix.DataDictionary.DataDictionary(Dictionary());
}
