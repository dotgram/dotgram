using System;
using System.Globalization;
using System.Linq;

using BenchmarkDotNet.Attributes;

using DotGram;

namespace DotGram.Benchmarks;

/// <summary>
/// What eager construction costs per record, not per parse — the number the incremental
/// materializer (docs/next.md, "Incremental materializer") exists to keep linear.
/// </summary>
/// <remarks>
/// <c>Feed</c>'s own body is exactly <c>Item*</c> — nothing follows the repeat, which is
/// what keeps <c>Feed</c> itself <c>Committed &amp;&amp; Deterministic</c> the moment each
/// <c>Item</c> returns, and so eager all the way through (confirmed the same way the
/// regression tests do, not assumed: <c>GeneratorDriverTests</c>'
/// <c>Eager_construction_survives_a_repetition_giving_back_its_last_turn</c> checks the
/// generated source of this exact shape for <c>Materialize_DotGram_Eager</c>). Doubling
/// <see cref="Records"/> should double the time, not quadruple it — a linking pass that
/// relinked the whole arena from <c>0</c> on every return, the thing
/// <c>Parser.LinkedUpTo</c> exists to avoid, would show up here as a bend in that line.
/// </remarks>
[MemoryDiagnoser]
public partial class EagerConstruction
{
	[Gram("""
		Feed : @int[] = items: Item* => @(items)
		Item : @int = value: ['0'..'9']+ & ',' => @(Number(value))
		parse Feed
		""")]
	public static partial class EagerFeed
	{
		static int Number(string digits) => int.Parse(digits, CultureInfo.InvariantCulture);
	}

	/// <summary>A tenfold step, so a quadratic bend would be a hundredfold jump in time.</summary>
	[Params(10_000, 100_000)]
	public int Records { get; set; }

	string _input = "";

	[GlobalSetup]
	public void Setup() =>
		_input = string.Concat(Enumerable.Repeat("1,", Records));

	[Benchmark]
	public int Parse() => EagerFeed.ParseFeed(_input).Length;
}
