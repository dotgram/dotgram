using System;
using System.Linq;

using BenchmarkDotNet.Attributes;

using DotGram.Handwritten;
using DotGram.Sql.Standard;

using Ast = DotGram.Sql.Ast;

namespace DotGram.Benchmarks;

/// <summary>
/// SQL:2023: the generated parser against the one written by hand beside it.
/// </summary>
/// <remarks>
/// <para>
/// <b>The partner is ours, and that is the whole of what this table compares.</b> There is no
/// ScriptDom for the standard -- ScriptDom reads T-SQL -- so a row here is
/// <c>SqlStandardParser</c>, generated from <c>SqlStandard.gram</c>, against
/// <c>HandSqlStandard</c>, written by a person against the same BNF. Read it as what the
/// generator costs over writing the parser out, and never as what the standard costs against
/// somebody else's library, which is a question this table does not ask.
/// </para>
/// <para>
/// <b>The same inputs the stand uses</b>, by name and by text, so that the two harnesses answer
/// one question and not two: <c>--speed</c> reads them round-robin on a machine that is not idle,
/// and this reads one case per process, warmed and iterated, with the intervals printed. Six of
/// them, over three orders of magnitude of input: a literal, an arithmetic expression, a query of
/// one column, a query of twenty, and a search condition of a hundred conjuncts and of a thousand.
/// </para>
/// <para>
/// <b>Both sides build, and the setup holds them to the same tree.</b> Each case is read by both
/// before anything is timed and the two trees are dumped and compared, so a side that quietly
/// reads less -- or builds less -- cannot be quick for a reason that says nothing about how it is
/// built. The row returns what it built rather than whether it read, so nothing can be elided.
/// </para>
/// <para>
/// <b>The A/A row is not spare.</b> BenchmarkDotNet is strong on an absolute number and weak on a
/// ratio; whatever "by hand again" reads away from 1.00 is the resolution of the other ratio in
/// its row, and a difference smaller than that is not a difference.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class SqlStandardBenchmarks
{
	/// <summary>A reading written by hand: the shape every one of them has.</summary>
	public delegate bool ByHand<T>(string input, out T value);

	/// <summary>One input, read both ways, with what it is called on the stand.</summary>
	public sealed class Case(
		string name,
		string text,
		Func<object?> generated,
		Func<object?> byHand,
		Func<string?> disagreement)
	{
		public string Name { get; } = name;
		public string Text { get; } = text;
		public Func<object?> Generated { get; } = generated;
		public Func<object?> ByHand { get; } = byHand;

		/// <summary>What the two disagree about, or null where they build the same tree.</summary>
		public Func<string?> Disagreement { get; } = disagreement;

		/// <summary>What BenchmarkDotNet prints for the case: the name and the size, in twenty</summary>
		/// <remarks>
		/// Twenty characters is what a parameter column keeps before it elides the middle, and an
		/// elided name is a row nobody can tell from its neighbour. "conditions1000 12889" is exactly
		/// twenty; the words that used to be in here ("characters") pushed it over and the report
		/// printed "arith(...)ters) [26]".
		/// </remarks>
		public override string ToString()
		{
			return Name + " " + Text.Length;
		}
	}

	/// <summary>Six inputs, taken from the stand by name and text rather than invented again.</summary>
	public static Case[] Cases { get; } =
	[
		Of<Ast.LiteralValue, Ast.LiteralValue>(
			"literal", "1",
			SqlStandardParser.TryParseLiteral, HandSqlStandard.TryParseLiteral),
		Of<Ast.Expression, Ast.Expression>(
			"arithmetic", "(a + b) * c - d / 5",
			SqlStandardParser.TryParseValueExpression, HandSqlStandard.TryParseValueExpression),
		Of<Ast.Statement.Select, Ast.Statement.Select>(
			"select1", "SELECT a FROM t",
			SqlStandardParser.TryParseQueryExpression, HandSqlStandard.TryParseQueryExpression),
		Of<Ast.Statement.Select, Ast.Statement.Select>(
			"select20", Select(20),
			SqlStandardParser.TryParseQueryExpression, HandSqlStandard.TryParseQueryExpression),
		Of<Ast.Expression, Ast.Expression>(
			"conditions100", Conditions(100),
			SqlStandardParser.TryParseSearchCondition, HandSqlStandard.TryParseSearchCondition),
		Of<Ast.Expression, Ast.Expression>(
			"conditions1000", Conditions(1000),
			SqlStandardParser.TryParseSearchCondition, HandSqlStandard.TryParseSearchCondition),
	];

	[ParamsSource(nameof(Cases))]
	public Case Input { get; set; } = Cases[0];

	/// <summary>What a parameter column keeps before it elides the middle of a name.</summary>
	/// <remarks>
	/// The limit is here rather than in a comment because a name over it is not an error and not
	/// a warning: the row simply prints as "arith(...)ters) [26]" and stops being tellable from
	/// its neighbour. That happened once already, and the person it happens to next would have no
	/// way to know why -- so the setup refuses instead, which is the only form of the rule that
	/// does not depend on somebody remembering it.
	/// </remarks>
	const int NameWidth = 20;

	/// <summary>The names fit, and both parsers build the same tree, before anything is timed.</summary>
	[GlobalSetup]
	public void TheNamesFitAndBothParsersAgree()
	{
		foreach (var one in Cases)
		{
			var printed = one.ToString();

			if (printed.Length > NameWidth)
				throw new InvalidOperationException(
					$"The case '{one.Name}' prints as '{printed}', {printed.Length} characters where " +
					$"BenchmarkDotNet keeps {NameWidth} and elides the middle of the rest. Two rows would " +
					"read alike in the report. Shorten the name.");
		}

		if (Input.Disagreement() is { } difference)
			throw new InvalidOperationException(
				$"The two parsers do not agree about '{Input.Name}':" + Environment.NewLine + difference);
	}

	[Benchmark(Baseline = true, Description = "by hand (HandSqlStandard)")]
	public object? Hand()
	{
		return Input.ByHand();
	}

	[Benchmark(Description = "generated (SqlStandardParser)")]
	public object? Generated()
	{
		return Input.Generated();
	}

	/// <summary>The baseline under a second name: the resolution of the ratio beside it.</summary>
	[Benchmark(Description = "by hand again (A/A)")]
	public object? HandAgain()
	{
		return Input.ByHand();
	}

	/// <summary>A query of that many columns, as the stand writes it.</summary>
	static string Select(int columns)
	{
		return "SELECT " + string.Join(", ", Enumerable.Range(0, columns).Select(static at => "a" + at)) +
			" FROM t WHERE a0 = 1";
	}

	/// <summary>That many conjuncts, as the stand writes them.</summary>
	static string Conditions(int many)
	{
		return string.Join(" AND ", Enumerable.Range(0, many).Select(static at => "a" + at + " = " + at));
	}

	/// <summary>One case: the two readings, and the check that they build one tree.</summary>
	static Case Of<TGenerated, THand>(
		string name,
		string text,
		Func<string, SqlStandardParser.Match<TGenerated>> generated,
		ByHand<THand> byHand)
	{
		return new Case(name, text, Read, ReadByHand, Disagreement);

		object? Read()
		{
			var match = generated(text);

			return match.IsSuccess ? match.Value : null;
		}

		object? ReadByHand()
		{
			return byHand(text, out var value) ? value : null;
		}

		string? Disagreement()
		{
			var match = generated(text);

			if (!match.IsSuccess)
				return "  the generated parser refuses it";

			if (!byHand(text, out var value))
				return "  the hand-written parser refuses it";

			var expected = Standard.Dump(match.Value);
			var actual   = Standard.Dump(value);

			return expected == actual ? null : $"  generated {expected}" + Environment.NewLine + $"  by hand   {actual}";
		}
	}
}
