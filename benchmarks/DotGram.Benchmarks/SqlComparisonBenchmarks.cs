using System;
using System.Linq;

using BenchmarkDotNet.Attributes;

using DotGram.Parsers;

namespace DotGram.Benchmarks;

/// <summary>
/// The generated SQL recognizer against two hand-written ones reading the same language.
/// </summary>
/// <remarks>
/// <para>
/// <b>Equal footing is the whole point, and it took two attempts to get.</b> The generated
/// parser tokenizes and then reads kinds. <see cref="HandSql"/> is scannerless, so a ratio
/// against it measures the lexical split and says nothing about how either parser is
/// shaped — it is kept here as the third row, because that is what the ratio in
/// <c>docs/next.md</c> used to be taken against and a reader should be able to see the
/// difference rather than be told about it. <see cref="HandSqlTokens"/> is the comparison:
/// it lexes into kinds first, so what stands between it and the generated parser is the
/// reader.
/// </para>
/// <para>
/// The other three things held equal, none of them free:
/// </para>
/// <list type="bullet">
/// <item>
/// <b>The same language.</b> <see cref="SqlAgainst.Agree"/> runs in
/// <c>[GlobalSetup]</c> and throws where the three disagree about any of forty-two shapes
/// — the test suite's corpus, comments, delimited identifiers, exponent and leading-point
/// numerals, and nine inputs that must be refused. A hand-written parser that quietly
/// reads less is faster for a reason that says nothing about the generator, and refusals
/// are half of reading the same language.
/// </item>
/// <item>
/// <b>The same answer.</b> All three recognize and none builds: the generated side is
/// asked through <c>TryParseSearchCondition</c>, whose value is the extent, so nothing is
/// materialized on any of them.
/// </item>
/// <item>
/// <b>The same input.</b> A string in, a bool out, each doing its own lexing inside. The
/// generated parser's tokenizer is not reachable from here, so the reader's own share
/// cannot be measured directly; <c>--hand</c> prints the hand-written lexer beside the
/// totals, and what that licenses is a subtraction under the assumption that two lexers
/// doing the same work cost about the same. The totals need no assumption, and they are
/// the number to quote.
/// </item>
/// </list>
/// </remarks>
[MemoryDiagnoser]
public class SqlComparisonBenchmarks
{
	public static string[] Inputs => SqlAgainst.Inputs;

	[ParamsSource(nameof(Inputs))]
	public string Input { get; set; } = "";

	[GlobalSetup]
	public void CheckTheyReadTheSameLanguage() => SqlAgainst.Agree();

	[Benchmark(Baseline = true, Description = "generated")]
	public bool Generated() => SqlStandard92.TryParseSearchCondition(Input).IsSuccess;

	[Benchmark(Description = "by hand, over tokens")]
	public bool Hand() => HandSqlTokens.Parse(Input);

	[Benchmark(Description = "by hand, scannerless")]
	public bool Scannerless() => HandSql.Parse(Input);

	[Benchmark(Description = "the hand-written lexer alone")]
	public int Lexer() => HandSqlTokens.LexOnly(Input);

	/// <summary>What the first day's ratio was divided by; it reads a fraction of the language.</summary>
	[Benchmark(Description = "day one, recovered")]
	public bool DayOne() => HandSqlOriginal.Parse(Input);
}
