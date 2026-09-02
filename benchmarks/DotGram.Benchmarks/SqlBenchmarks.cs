using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Attributes;

using DotGram.Parsers;

namespace DotGram.Benchmarks;

/// <summary>
/// <c>src/DotGram.Parsers/SqlStandard92.cs</c>: the only parser here that reads token
/// kinds rather than characters.
/// </summary>
/// <remarks>
/// <para>
/// It is here because it was not, and that was a hole. Every number the lexical split has
/// been justified by — the split itself, the terminal inventory, the generated lexer, its
/// division into methods, and the transition table that replaced its <c>switch</c> — came
/// from a throwaway program in <c>.work</c> that no longer exists. A parser whose numbers
/// live nowhere is a parser whose next change is measured against a memory.
/// </para>
/// <para>
/// An instrument and not a comparison, the same as <see cref="ExpressionBenchmarks"/>:
/// there is no SQL expression parser in the box to divide by, and taking one on to have a
/// denominator is a decision about dependencies rather than about measurement. What is
/// printed is what a parse costs, and its use is that a change to either machine moves a
/// number somebody is looking at.
/// </para>
/// <para>
/// <b>Two machines, and that is what makes this different from the others.</b> A parse here
/// reads the input twice: once by the lexer, into kinds and extents, and once by the
/// syntactic machine over those. The two answer to different things — the lexer to the
/// alphabet and the shape of the keyword trie, the parser to how much backtracking a
/// grammar asks for — so an input that got slower says which of them without a profiler
/// only if the inputs are chosen to separate them. The short ones below are mostly lexing;
/// the deep nest is mostly parsing.
/// </para>
/// <para>
/// The refusals are half the point. Refusal is where a backtracking engine does its worst
/// work and where the split has always paid best, and the two kinds of refusal here are not
/// the same thing at all: an input that stops being this language at the first character
/// never reaches the parser, and one that is a valid prefix reaches all of it.
/// </para>
/// <para>
/// Allocation is reported for the same reason as next door, and there is one thing extra to
/// see in it: a split parse rents its three token arrays from a thread-static and hands them
/// back on every exit, so what the column should show is a parse that allocates what the
/// values cost and nothing for the tokens. It showed three arrays sized to the input before
/// that pooling existed, which on a short condition was the whole of what the split cost
/// over the character parser.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class SqlBenchmarks
{
	/// <summary>The same condition at two, four and six levels of parenthesis.</summary>
	/// <remarks>
	/// Graded for the reason the expression benchmark's are: a rule that reads its operand
	/// twice is free on a short input and doubles per level, so one deep input says nothing
	/// a slow machine does not also say. Three say which curve it is — and this grammar's
	/// value expression is a precedence ladder of eight rules, which is where such a shape
	/// would hide.
	/// </remarks>
	static readonly string[] Nests =
	[
		"(a + b) * c > d",
		"((((a + 1) * 2) - 3) / 4) + b > 0",
		"((((((a + 1) * 2) - 3) / 4) + 5) * 6) + b > 0",
	];

	/// <summary>The same three with their last character cut off.</summary>
	/// <remarks>
	/// Cut rather than written out, so each differs from its partner by one character and
	/// the difference between two numbers is the cost of refusing and nothing else.
	/// </remarks>
	static readonly string[] Refusals = [.. Nests.Select(nest => nest[..^1])];

	/// <summary>What each of these is for is the comment beside it.</summary>
	public static IEnumerable<string> Inputs =>
	[
		// The floor: `SearchCondition` down to `Identifier` once, and nothing else. Mostly
		// the lexer — four tokens and a parse that barely branches.
		"a = 1",

		// Three keywords in a row, which is the keyword trie doing the only work it ever
		// does: `BETWEEN`, `AND`, and two numbers that are prefixes of nothing.
		"salary BETWEEN 1000 AND 2000",

		// A list, and `IS NOT NULL` — where the parser has to tell a predicate from the
		// value expression that begins the same way.
		"x IN (1, 2, 3) AND y IS NOT NULL",

		// A quoted string and a dotted name, so the lexer leaves its keyword trie for the
		// two states that are not one: a string body, and the identifier continuation.
		"warehouse.zip_code = 'X' AND vendor_key IS NOT NULL AND quota > 0",

		// The two functions whose arguments are keywords rather than commas, which is the
		// shape SQL-92 writes that nothing else does: `SUBSTRING(s FROM 1 FOR 3)`.
		"CAST(x AS INTEGER) = 5 OR SUBSTRING(s FROM 1 FOR 3) = 'abc'",

		// `CASE` with two `WHEN`s. It is here because a woven word boundary once refused
		// the second one, and a grammar that stops accepting an input is not something a
		// benchmark should be the last to notice.
		"CASE WHEN a > 1 THEN 'big' WHEN a > 0 THEN 'small' ELSE 'none' END = label",

		.. Nests,
		.. Refusals,

		// A refusal at the first character, which is a different measurement from the three
		// above: `!` begins no terminal, so the lexer stops before the parser is entered at
		// all. What this times is the cost of finding that out — and it is the number that
		// moved most when the scanner's transition table learned to answer for the
		// characters below its own alphabet instead of falling past itself to a chain.
		"! a = 1",
	];

	[ParamsSource(nameof(Inputs))]
	public string Input { get; set; } = "";

	/// <summary>
	/// That the input reads the way this file says it does, before anything is timed.
	/// </summary>
	/// <remarks>
	/// A benchmark of an input that stopped parsing is a benchmark of the refusal path
	/// wearing the name of the accepting one, and it would not otherwise show: the numbers
	/// would simply get better.
	/// </remarks>
	[GlobalSetup]
	public void CheckItReadsAsClaimed()
	{
		var match  = SqlStandard92.TryParseSearchCondition(Input);
		var should = Array.IndexOf(Refusals, Input) < 0 && Input != "! a = 1";

		if (match.IsSuccess != should)
			throw new InvalidOperationException(
				$"'{Input}' was expected {(should ? "to read" : "not to read")} and did " +
				$"{(match.IsSuccess ? "read" : "not")}: {match.Error}");
	}

	[Benchmark]
	public bool Parse() => SqlStandard92.TryParseSearchCondition(Input).IsSuccess;
}
