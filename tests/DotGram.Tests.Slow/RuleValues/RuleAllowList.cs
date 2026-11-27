using System;
using System.Collections.Generic;
using System.Linq;

namespace DotGram.Tests.RuleValues;

/// <summary>
/// The newer side of a difference: what a build of the rule is expected to give (the oracle) or does give (this build).
/// </summary>
/// <param name="Built">The grammar is built and gives a value (or fails to match, which is not a refusal).</param>
/// <param name="Value">The value as printed, when there is one.</param>
/// <param name="Refused">The grammar is refused by the generator.</param>
sealed record Newer(bool Built, string? Value, bool Refused)
{
	/// <summary>What the oracle expects, as a newer side.</summary>
	public static Newer Of(Expectation expectation)
	{
		return new Newer(expectation.IsValue, expectation.Value, expectation.IsError);
	}

	/// <summary>What this build gave, as a newer side.</summary>
	public static Newer Of(Outcome outcome)
	{
		return new Newer(outcome.Built, outcome.Kind == "value" ? outcome.Text : null, !outcome.Built);
	}
}

/// <summary>What an entry is asked about: a grammar, the mode, what the older side gave and what the newer side gives.</summary>
sealed record AllowCase(RuleShape Shape, Mode Mode, Outcome Old, Newer Now, Grammar Grammar)
{
	/// <summary>The older side built a value and the newer side gives another or refuses.</summary>
	public bool Differs
	{
		get
		{
			return AsIfDifferent || Old.Kind == "value" && (Now.Refused || Now.Value != Old.Text);
		}
	}

	/// <summary>Asks whether the grammar is of the class, whether or not the two sides differ: counts the cells where they agree.</summary>
	public bool AsIfDifferent { get; init; }

	/// <summary>The grammar holds what the feature names (see <see cref="Grammar.Features"/>).</summary>
	public bool Has(string feature)
	{
		return Grammar.Features.Contains(feature);
	}
}

/// <summary>
/// One entry of the allow-list: a class of grammars whose value differs from the published version's on purpose, said by
/// what the grammar holds. A silent class changes a value that built; a loud one makes a build fail.
/// </summary>
sealed record AllowEntry(string Name, bool Silent, string Description, Func<AllowCase, bool> Matches);

/// <summary>
/// The classes of difference from the published version, and nothing else: an entry may not be added, widened or removed
/// to cover a difference. A difference that no entry covers is a finding; an entry that covers no cell of a run fails the
/// run. Silent classes come first, which is the order the release notes need.
/// </summary>
static class RuleAllowList
{
	public static readonly IReadOnlyList<AllowEntry> Entries =
	[
		new("string-rule-joins-built-value", true,
			"a string rule without => that calls a rule building a string is its pieces joined, not the text it matched",
			static c => c.Differs && c.Has("C-built")),

		new("taken-string-rule-keeps-own-literals", true,
			"a rule that takes a string rule's type and calls a building rule is its pieces joined too: its own literals stay",
			static c => c.Differs && c.Has("C-taken-built")),

		new("array-collects-text-rule", true,
			"an array collects every call of the element type, a text rule's included",
			static c => c.Differs && c.Has("A-text-rule")),

		new("trivia-dropped-in-string-rule", true,
			"the trivia woven between the operands of a string rule is no part of its value, over characters and over kinds",
			static c => c.Differs && c.Has("C-seam")),

		new("char-piece-gives-character", true,
			"a rule of type char inside a string rule gives its character, not the text it matched",
			static c => c.Differs && c.Has("C-char")),

		new("untyped-rule-read-through", true,
			"an untyped rule or template is read as if its body were written in place: the values of the calls in it count",
			static c => c.Differs && (c.Has("C-read-through") || c.Has("A-read-through"))),

		new("string-not-built-from-names", false,
			"names are not a constructor of a string: the call that fills one is a piece like any other",
			static c => c.Differs && c.Has("C-names-of-constructor")),

		new("non-text-piece-is-error", false,
			"a piece of a type that is not text (int, a span, a host or generated type) in a string rule is a build error",
			static c => c.Differs && c.Has("C-non-text")),

		new("array-call-splices-elements", true,
			"inside an array rule a call of an array rule of the same element type gives its elements in place; 0.3.0 gave the first element or none",
			static c => c.Differs && c.Has("A-splice")),

		new("named-literal-not-collected", true,
			"a name on a literal makes no element of an array: only calls are collected",
			static c => c.Differs && c.Has("A-named-literal")),

		new("operand-beats-construction", true,
			"names fill a construction and the rule reads one operand of its type on every route: the operand is the value",
			static c => c.Differs && c.Has("O-operand-beats-construction")),

		new("compiler-named-capture", true,
			"a capture an author names the way the generator names its own is the author's, and is no longer the value",
			static c => c.Differs && c.Has("compiler-name")),

		new("optional-text-route", true,
			"a string rule with an optional call: where it is missing the value is the other pieces joined (0.3.0 gave null)",
			static c => c.Old.WarnedNull && c.Differs && c.Has("C-optional-call")),
	];

	/// <summary>The entries a difference belongs to, silent ones first.</summary>
	public static IEnumerable<AllowEntry> Covering(AllowCase c)
	{
		return Entries.Where(e => e.Matches(c)).OrderByDescending(static e => e.Silent);
	}
}

/// <summary>
/// Differences from the published version that no class of the allow-list explains and the rule does not decide: each is a
/// question for the rule's owner, with the cells that raise it. They are listed, never allowed.
/// </summary>
static class RuleQuestions
{
	static bool Kind(AllowCase c, Kind kind)
	{
		return c.Grammar.Tested.Kind == kind;
	}

	public static readonly IReadOnlyList<AllowEntry> Entries =
	[
		new("taken-span-with-names-hands-on-operand", true,
			"a rule that takes a span's type and writes names: 0.3.0 gives the operand's bounds, a span rule gives its own",
			static c => c.Differs && c.Grammar.Start.Kind == DotGram.Tests.RuleValues.Kind.S && (c.Shape.Type.How == TypeSaid.Taken || c.Shape.SeenFrom != SeenFrom.Itself)),

		new("optional-operand-by-route", true,
			"a rule of another type reads an operand on some routes and none on others: 0.3.0 gives the operand where it is, or null with a warning",
			static c => c.Differs && Kind(c, DotGram.Tests.RuleValues.Kind.O) && c.Old.WarnedNull),

		new("array-element-only-in-zero-repetition", true,
			"an array rule whose only call of the element type is inside X{0}: 0.3.0 gives an empty array, the rule says there is no call to collect",
			static c => c.Differs && Kind(c, DotGram.Tests.RuleValues.Kind.A) && c.Shape.Structure == "zero"),

		new("name-on-untyped-rule-in-array", true,
			"a name on an untyped rule in an array rule: 0.3.0 collects its text, the rule says an untyped rule gives nothing to collect",
			static c => c.Differs && c.Has("A-named-untyped")),

		new("name-inside-zero-repetition-fills-empty", true,
			"a name inside X{0} that a constructor takes: 0.3.0 fills it with an empty text, the rule says X{0} gives nothing",
			static c => c.Differs && Kind(c, DotGram.Tests.RuleValues.Kind.O) && c.Shape.Structure == "zero"),
	];

	/// <summary>The questions a difference raises.</summary>
	public static IEnumerable<AllowEntry> Raised(AllowCase c)
	{
		return Entries.Where(e => e.Matches(c));
	}
}
