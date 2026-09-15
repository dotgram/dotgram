using System;

using BenchmarkDotNet.Attributes;

using DotGram.Sql.Standard;

namespace DotGram.Benchmarks;

/// <summary>Repeated large parses, including inputs that exceed the retained pool budget.</summary>
[MemoryDiagnoser]
public class ParserResourceBenchmarks
{
	string input = "";

	[Params(1, 1000, 10000, 50000)]
	public int Terms { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		input = string.Join(" AND ", Enumerable.Range(0, Terms).Select(i => "a" + i + " = 1"));
		if (!Sql92Parser.TryParseSearchCondition(input).IsSuccess)
			throw new InvalidOperationException("The resource benchmark input must parse.");
	}

	[Benchmark]
	public bool Parse() => Sql92Parser.TryParseSearchCondition(input).IsSuccess;
}

/// <summary>A scalar reader whose setup should cost no allocations on successful short input.</summary>
[MemoryDiagnoser]
public class TinyParserBenchmarks
{
	[Params("a", "  ( ( a ) )  ", "?", "((a)")]
	public string Input { get; set; } = "";

	[GlobalSetup]
	public void Setup()
	{
		var match = TinyScalar.TryParseDepth(Input);
		var expected = Input is "a" or "  ( ( a ) )  ";
		if (match.IsSuccess != expected || (expected && match.Value != (Input == "a" ? 1 : 3)))
			throw new InvalidOperationException("Unexpected scalar benchmark result.");
	}

	[Benchmark]
	public bool Parse() => TinyScalar.TryParseDepth(Input).IsSuccess;
}

[Gram("""
trivia = ' '*
Depth : @int = 'a' => @(1) | '(' & n: Depth & ')' => @(n + 1)
parse Depth
""")]
partial class TinyScalar;
