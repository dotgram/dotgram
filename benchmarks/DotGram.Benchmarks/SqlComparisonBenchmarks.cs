using System;
using System.Linq;

using BenchmarkDotNet.Attributes;

using DotGram.Sql;
using DotGram.Sql.Standard;

namespace DotGram.Benchmarks;

/// <summary>
/// One SQL-92 grammar read two ways: the tape and the immediate carrier.
/// </summary>
/// <remarks>
/// <para>
/// The same rules, the same tree, the same input — the only difference is how a reader
/// carries what it read, which is what the carriers are. A ratio here is about that choice
/// and about nothing else, and it needs no assumption about two parsers doing the same
/// work at the same cost, because there is only one parser in it.
/// </para>
/// <para>
/// <b>The same answer, checked before anything is timed.</b> Two compilations of one
/// grammar ought to agree on every input, and <c>[GlobalSetup]</c> holds them to it: a
/// carrier that quietly reads less would otherwise look quick for a reason that says
/// nothing about carrying.
/// </para>
/// <para>
/// The comparison against a parser written by hand is not here. It is
/// <c>HandSqlStandard</c> against <c>SqlStandardParser</c>, over the whole of SQL:2023
/// rather than the four publications SQL-92 offers, and it is run by
/// <c>--standard "^production"</c> (<see cref="Standard"/>) with the tests holding the two
/// to the same language the rest of the time.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class SqlComparisonBenchmarks
{
	/// <summary>Search conditions of a few shapes and one refusal, which the carriers are read on.</summary>
	public static string[] Inputs { get; } =
	[
		"a = 1",
		"(a + b) * c > d",
		"((((a + 1) * 2) - 3) / 4) + b > 0",
		"x = 1 AND y IS NOT NULL",
		string.Join(" AND ", Enumerable.Range(0, 64).Select(i => "a" + i + " = 1")),
		string.Join(" + ", Enumerable.Range(0, 64).Select(i => "a" + i)) + " > 0",
		"(a + b) * c >",
	];

	[ParamsSource(nameof(Inputs))]
	public string Input { get; set; } = "";

	[GlobalSetup]
	public void CheckTheCarriersAgree()
	{
		foreach (var input in Inputs)
		{
			var tape      = Sql92Parser.TryParseSearchCondition(input).IsSuccess;
			var immediate = ImmediateSql.TryParseSearchCondition(input).IsSuccess;

			if (tape != immediate)
				throw new InvalidOperationException(
					$"The carriers disagree about \"{input}\": " +
					$"tape {tape}, immediate {immediate}.");
		}
	}

	[Benchmark(Baseline = true, Description = "tape")]
	public bool Tape()
	{
		return Sql92Parser.TryParseSearchCondition(Input).IsSuccess;
	}

	[Benchmark(Description = "immediate")]
	public bool Immediate()
	{
		return ImmediateSql.TryParseSearchCondition(Input).IsSuccess;
	}
}
