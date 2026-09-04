using System;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;

using DotGram.Parsers;

namespace DotGram.Benchmarks;

/// <summary>
/// The three SQL parsers on the long condition, counted rather than timed: instructions
/// retired, branches mispredicted, cache misses, per parse.
/// </summary>
/// <remarks>
/// <para>
/// Timing says the eager parser is half again the hand-written one and not where. A
/// sampling profile charges half of everything to native code it cannot attribute, and a
/// tracing profile charges by the call, which the hand-written parser makes fewer and
/// smaller of. The counters divide the question instead: a parser that retires more
/// instructions is doing more work, and one that retires the same and takes longer is
/// stalling — on a branch it guessed wrong, or on memory it did not have.
/// </para>
/// <para>
/// Needs ETW, which means an elevated console; without it the counters come back empty
/// and the timings stand alone.
/// </para>
/// </remarks>
[HardwareCounters(HardwareCounter.InstructionRetired, HardwareCounter.BranchMispredictions, HardwareCounter.CacheMisses)]
public class SqlCounters
{
	const string Long =
		"a0 = 1 AND a1 = 1 AND a2 = 1 AND a3 = 1 AND a4 = 1 AND a5 = 1 AND a6 = 1 AND a7 = 1 AND " +
		"a8 = 1 AND a9 = 1 AND a10 = 1 AND a11 = 1 AND a12 = 1 AND a13 = 1 AND a14 = 1 AND a15 = 1 AND " +
		"a16 = 1 AND a17 = 1 AND a18 = 1 AND a19 = 1 AND a20 = 1 AND a21 = 1 AND a22 = 1 AND a23 = 1 AND " +
		"a24 = 1 AND a25 = 1 AND a26 = 1 AND a27 = 1 AND a28 = 1 AND a29 = 1 AND a30 = 1 AND a31 = 1 AND " +
		"a32 = 1 AND a33 = 1 AND a34 = 1 AND a35 = 1 AND a36 = 1 AND a37 = 1 AND a38 = 1 AND a39 = 1 AND " +
		"a40 = 1 AND a41 = 1 AND a42 = 1 AND a43 = 1 AND a44 = 1 AND a45 = 1 AND a46 = 1 AND a47 = 1 AND " +
		"a48 = 1 AND a49 = 1 AND a50 = 1 AND a51 = 1 AND a52 = 1 AND a53 = 1 AND a54 = 1 AND a55 = 1 AND " +
		"a56 = 1 AND a57 = 1 AND a58 = 1 AND a59 = 1 AND a60 = 1 AND a61 = 1 AND a62 = 1 AND a63 = 1";

	[Benchmark(Baseline = true)]
	public bool Hand() => HandSqlTokens.Parse(Long);

	[Benchmark]
	public bool Eager() => EagerSql.TryParseSearchCondition(Long).IsSuccess;

	[Benchmark]
	public bool Tape() => SqlStandard92.TryParseSearchCondition(Long).IsSuccess;

	/// <summary>The lexer alone, to take it off all three.</summary>
	[Benchmark]
	public int Lexer() => HandSqlTokens.LexOnly(Long);
}
