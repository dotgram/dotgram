using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using BenchmarkDotNet.Attributes;

using DotGram.Parsers;

namespace DotGram.Benchmarks;

/// <summary>
/// <c>src/DotGram.Parsers/ExpressionLanguage.cs</c>: the largest grammar in this
/// repository, over the shapes a lambda is actually written in.
/// </summary>
/// <remarks>
/// <para>
/// An instrument rather than a comparison, and that is the difference between this and
/// <see cref="UrlBenchmarks"/>. A URL has a regular expression beside it that answers the
/// same question, so the number worth printing there is a ratio. This language has no such
/// partner in the box: the engines that read C# expressions at run time are outside
/// dependencies, and taking one on to have something to divide by is a decision about what
/// this repository depends on rather than a decision about measurement. So what is printed
/// here is what the parse costs, and its use is that a change to the grammar or to the
/// engine under it moves a number somebody is looking at.
/// </para>
/// <para>
/// Which is a use it has already earned. Three rules here were reading their operand twice
/// — <c>Target</c>, <c>Primary</c>, <c>NamedType</c> — and it took a profile to find, on a
/// parser nothing was timing between releases; the same shape had twice before made a
/// parse take thirty seconds and most of a second, and both of those are recorded in
/// comments in the grammar rather than in any measurement that would have caught them
/// coming back. A rule that reads its operand twice costs nothing on a short input and
/// doubles per level of nesting, which is why the inputs below are graded by depth: the
/// shallow ones say what a parse costs and the deep ones say whether it is still linear.
/// </para>
/// <para>
/// Each input is here for a part of the grammar, not for length. Between them they walk
/// the operator ladder, both right-associative levels, the parenthesized nest, a block
/// with statements in it, a name resolved against the loaded assemblies while the text is
/// being read, and <c>Assignment</c>'s eleven alternatives — which is the rule the
/// operand-read-twice bug was found in, and so the one most worth watching.
/// </para>
/// <para>
/// The last three inputs are the three before them with their final operand cut off. Each
/// differs from its partner by one character, so the difference between two numbers is
/// what refusing costs over accepting and nothing else — and refusal is where a
/// backtracking engine does its worst work, which is exactly the work nobody measures
/// because nobody benchmarks the input that does not parse. Both sides go through
/// <c>TryParse</c>, so what is timed is the parse and not the cost of throwing.
/// </para>
/// <para>
/// Allocation is reported because it is not incidental here. Unlike <see cref="Urls"/>,
/// this parser hands back no parser to a pool — a shipped API taking one is a design
/// question and not this file's to answer — so what the column shows is a parse that
/// starts from nothing every time, which is what a caller of the package gets today.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class ExpressionBenchmarks
{
	/// <summary>The same expression at two, four and six levels of parenthesis.</summary>
	/// <remarks>
	/// Three depths and not one, because the question these are here to answer is about a
	/// shape and not about a cost: a rule that reads its operand twice is free on a short
	/// input and doubles per level, so one deep input says nothing that a slow machine does
	/// not also say. Three say which curve it is. Both right-associative levels of the
	/// ladder — <c>Conditional</c> and <c>Coalesce</c> — are written with an optional tail
	/// for exactly this reason, and written the other way this input took thirty seconds.
	/// </remarks>
	static readonly string[] Nests =
	[
		"(int x) => ((x + 1) * 2) + x",
		"(int x) => ((((x + 1) * 2) - 3) / 4) + x",
		"(int x) => ((((((x + 1) * 2) - 3) / 4) + 5) * 6) + x",
	];

	/// <summary>The same three with their final operand cut off.</summary>
	/// <remarks>
	/// Cut from the accepted ones rather than written out, so the pairing holds by
	/// construction: each differs from its partner by one character, and so the difference
	/// between two numbers is the cost of refusing and nothing else. Graded for the same
	/// reason the accepted ones are — refusal is where a backtracking engine does its worst
	/// work, and whether that work grows with depth the way the accepting work does is a
	/// different question. The first run of these answered it with an exponential: two
	/// alternatives of <c>Primary</c> both read a bare integer, the second reading consumed
	/// the same digits as the first, and a refusal walked every combination of the live
	/// ways back that left — 2^(literals) rereadings of everything after them. The pair is
	/// one alternative now, deciding int-or-long in its factory, and these three exist to
	/// say if that shape ever comes back.
	/// </remarks>
	static readonly string[] Refusals = [.. Nests.Select(nest => nest[..^1])];

	/// <summary>What each of these is for is the comment beside it.</summary>
	public static IEnumerable<string> Inputs =>
	[
		// The floor: every rule from `Lambda` down to `Name` walked once, and nothing
		// else. What a parse costs before it parses anything.
		"(int x) => x",

		// The operator ladder, folded twice.
		"(int x) => x * x - 1",

		// Two parameters, and a parenthesis that has to be read as a group rather than
		// as a call.
		"(int x, int y) => (x + y) * 3 - x / 5",

		// A member read, which is `Postfix` and not `Target`.
		"(string s) => s.Length",

		// `NamedType`, which asks what a name means while the text is being read: this
		// one costs a reflection lookup the first time and a cache hit afterwards, and
		// the benchmark measures the second because the setup below has already paid the
		// first.
		"(int x) => Math.Max(x, 1)",

		// A block, and `Assignment` reached twice through `Target` — eleven alternatives
		// each beginning with the same operand, which is the rule that was reading it
		// eleven times over.
		"(int x) => { x += 1; x *= 2; return x; }",

		.. Nests,
		.. Refusals,
	];

	[ParamsSource(nameof(Inputs))]
	public string Input { get; set; } = "";

	/// <summary>
	/// That the input reads the way this file says it does, before anything is timed.
	/// </summary>
	/// <remarks>
	/// A benchmark of an input that stopped parsing is a benchmark of the failure path
	/// wearing the name of the success one, and it would not otherwise show: the numbers
	/// would simply get better. It doubles as the warm-up the name resolution needs, which
	/// is why it is a whole parse and not a cheaper check.
	/// </remarks>
	[GlobalSetup]
	public void CheckItReadsAsClaimed()
	{
		var match  = ExpressionLanguage.TryParse(Input);
		var should = Array.IndexOf(Refusals, Input) < 0;

		if (match.IsSuccess != should)
			throw new InvalidOperationException(
				$"'{Input}' was expected {(should ? "to read" : "not to read")} and did " +
				$"{(match.IsSuccess ? "read" : "not")}: {match.Error}");
	}

	[Benchmark]
	public LambdaExpression? Parse()
	{
		var match = ExpressionLanguage.TryParse(Input);

		return match.IsSuccess ? match.Value : null;
	}
}
