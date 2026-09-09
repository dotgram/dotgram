using System;

using DotGram;

namespace DotGram.Examples.Languages;

// A matcher whose comparisons are case-sensitive, unless a region says otherwise:
//
//     name = "Bob" and ci(city = "berlin")
//     ci(cs(kind = "Draft"))
//
// `ci(…)` does not change what is read. The characters inside it are read by the same
// rule, along the same route, failing in the same places — what changes is which
// comparison the construction builds when it gets there. That is what a mark is for
// (docs/syntax.md §7.8), and it is a different thing from `context` (§7.7, and
// ScopedExpressionExample): a context is what a parse *accumulates* and still has at the
// end; a mark holds while one thing is being read and is gone after it. What C# means by
// `checked(…)`, what a language means by "inside a loop".
//
//     state : @Case
//
//     Test = "ci" & '(' & inner: Test with state @(Case.Insensitive) & ')' => @(inner)
//
// `with state @(…)` marks an extent, and `parserState` is the marks standing over a
// construction, **outermost first** — so the last of them is the nearest, which is why
// `ci(cs(…))` is sensitive and `cs(ci(…))` is not. `Of` below reads the span from the
// end and stops at the first mark that settles the question, which is the whole of the
// idiom.
//
// Like every supplied name, a hook that does not mention `parserState` is not handed it,
// and a grammar that places no mark hands an empty span to one that does — so nothing
// here costs anything until somebody writes `ci(`.
//
// One type for all the marks in a composition (`GRAM3016`, `GRAM3020`). They are a stack
// of values several authors may push onto and one span everybody reads, and a span holds
// one element type.

/// <summary>Whether comparisons in a region mind the case of what they compare.</summary>
public enum Case { Sensitive, Insensitive }

/// <summary>One comparison, and how it is to be made.</summary>
public sealed record Match(string Field, string Value, StringComparison How)
{
	/// <summary>
	/// The nearest mark decides, so the span is read from the end: the first that
	/// answers the question ends the search, and no mark at all means the default.
	/// </summary>
	public static Match Of(string field, string value, ReadOnlySpan<Case> marks)
	{
		for (var i = marks.Length - 1; i >= 0; i--)
		{
			if (marks[i] == Case.Insensitive)
				return new Match(field, Unquoted(value), StringComparison.OrdinalIgnoreCase);

			if (marks[i] == Case.Sensitive)
				break;
		}

		return new Match(field, Unquoted(value), StringComparison.Ordinal);
	}

	static string Unquoted(string quoted) =>
		quoted.Substring(1, quoted.Length - 2).Replace("\"\"", "\"");
}

[Gram("""
	@using DotGram.Examples.Languages;

	using Std;

	state : @Case

	trivia = Blank?

	Filter : @Match[] = Test & ("and" & Test)* & eof

	Test : @Match
		= "ci" & '(' & inner: Test with state @(Case.Insensitive) & ')' => @(inner)
		| "cs" & '(' & inner: Test with state @(Case.Sensitive)   & ')' => @(inner)
		| field: Identifier & '=' & value: Quoted('"')                  => @(Match.Of(field, value, parserState))

	parse Filter
	""")]
public static partial class Filters
{
	/// <summary>Whether a row of fields satisfies every comparison of a filter.</summary>
	public static bool Matches(string filter, Func<string, string?> field)
	{
		foreach (var one in ParseFilter(filter))
		{
			if (field(one.Field) is not { } value || !value.Equals(one.Value, one.How))
				return false;
		}

		return true;
	}
}
