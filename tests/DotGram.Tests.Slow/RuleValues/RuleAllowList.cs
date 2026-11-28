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

/// <summary>What a class of difference does to a program that was built with the published version.</summary>
enum Impact
{
	/// <summary>A value that built changes, with nothing said: first in the release notes.</summary>
	Silent,

	/// <summary>A build that succeeded fails, with a message.</summary>
	Loud,

	/// <summary>The published version built only with a warning of a possible null, so a program had to live with that already.</summary>
	WarnOnly,
}

/// <summary>
/// One entry of the allow-list: a class of grammars whose value differs from the published version's on purpose, said by
/// what the grammar holds.
/// </summary>
sealed record AllowEntry(string Name, Impact Impact, string Description, Func<AllowCase, bool> Matches);

/// <summary>
/// The classes of difference from the published version, and nothing else: an entry may not be added, widened or removed
/// to cover a difference. A difference that no entry covers is a finding; an entry that covers no cell of a run fails the
/// run. They stand in the order of the release notes: silent classes, then loud ones, then those the published version
/// built only with a warning.
/// </summary>
static class RuleAllowList
{
	static bool Kind(AllowCase c, Kind kind)
	{
		return c.Grammar.Tested.Kind == kind;
	}

	public static readonly IReadOnlyList<AllowEntry> Entries =
	[
		new("string-rule-joins-built-value", Impact.Silent,
			"a string rule without => (an untyped rule with no names included) that calls a rule building a string is its pieces joined, not the text it matched",
			static c => c.Differs && c.Has("C-built")),

		new("taken-string-rule-keeps-own-literals", Impact.Silent,
			"a rule that takes a string rule's type and calls a building rule is its pieces joined too: its own literals stay",
			static c => c.Differs && c.Has("C-taken-built")),

		new("array-collects-text-rule", Impact.Silent,
			"an array collects every call of the element type, a text rule's included",
			static c => c.Differs && c.Has("A-text-rule")),

		new("array-call-splices-elements", Impact.Silent,
			"inside an array rule a call of an array rule of the same element type gives its elements in place; 0.3.0 gave the first element or none",
			static c => c.Differs && c.Has("A-splice")),

		new("taken-span-names-own-bounds", Impact.Silent,
			"a span rule is the bounds of what it matched, names or no names: a rule that takes a span's type and writes names no longer hands on its operand's bounds",
			static c => c.Differs && c.Grammar.Start.Kind == DotGram.Tests.RuleValues.Kind.S && (c.Shape.Type.How == TypeSaid.Taken || c.Shape.SeenFrom != SeenFrom.Itself)),

		new("trivia-dropped-in-string-rule", Impact.Silent,
			"the trivia woven between the operands of a string rule is no part of its value, over characters and over kinds",
			static c => c.Differs && c.Has("C-seam")),

		new("char-piece-gives-character", Impact.Silent,
			"a rule of type char inside a string rule gives its character, not the text it matched",
			static c => c.Differs && c.Has("C-char")),

		new("untyped-rule-read-through", Impact.Silent,
			"an untyped rule or template is read as if its body were written in place: the values of the calls in it count",
			static c => c.Differs && (c.Has("C-read-through") || c.Has("A-read-through"))),

		new("named-text-not-collected", Impact.Silent,
			"a name on a literal, or on text an untyped rule reads, makes no element of an array: only calls of the element type are collected",
			static c => c.Differs && c.Has("A-named-text")),

		new("operand-beats-construction", Impact.Silent,
			"names fill a construction and the rule has a call of its type on every route: the call is the value",
			static c => c.Differs && c.Has("O-operand-beats-construction")),

		new("compiler-named-capture", Impact.Silent,
			"a capture an author names the way the generator names its own is the author's, and is no longer the value",
			static c => c.Differs && c.Has("compiler-name")),

		new("string-not-built-from-names", Impact.Loud,
			"names are not a constructor of a string: the call that fills one is a piece like any other",
			static c => c.Differs && c.Has("C-names-of-constructor")),

		new("non-text-piece-is-error", Impact.Loud,
			"a piece of a type that is not text (int, a span, a host or generated type) in a string rule is a build error",
			static c => c.Differs && c.Has("C-non-text")),

		new("object-rule-reads-two-calls", Impact.Loud,
			"a rule of type object takes a call of any typed rule, so two calls on a route are two operands and no value",
			static c => c.Differs && c.Has("O-object-two-calls")),

		new("zero-repetition-operand-refused", Impact.Loud,
			"a call written only in X{0} reads nothing, so it is no call of the rule's type on any route (0.3.0 gave an empty list)",
			static c => c.Differs && c.Has("O-zero-operand")),

		new("partial-construction-refused", Impact.Loud,
			"names choose the construction of the whole rule: an alternative that captures none of them is refused",
			static c => c.Differs && c.Has("O-partial")),

		new("mixed-array-over-bytes-refused", Impact.Loud,
			"over bytes an array rule that collects both a text call (its bytes) and a building one (a string) has no one element type: refused (0.3.0 collected only the building call)",
			static c => c.Differs && c.Mode == Mode.B && c.Has("A-mixed-bytes")),

		new("tied-constructor-refused", Impact.Loud,
			"a name that two constructors take equally chooses none",
			static c => c.Differs && c.Has("O-tie")),

		new("optional-text-route", Impact.WarnOnly,
			"a string rule with an optional call, or a route that reads none: where it is missing the value is the other pieces joined (0.3.0 gave null with a warning)",
			static c => c.Old.WarnedNull && c.Differs && (c.Has("C-optional-call") || c.Old.Text == "<null>" && Kind(c, DotGram.Tests.RuleValues.Kind.C))),

		new("optional-operand-falls-to-construction", Impact.WarnOnly,
			"a rule of another type has a call of its type on some routes only: the construction from its names is the value, or the build is refused (0.3.0 gave the call, or null with a warning)",
			static c => c.Old.WarnedNull && c.Differs && Kind(c, DotGram.Tests.RuleValues.Kind.O)),
	];

	/// <summary>The entries a difference belongs to, in the order of the release notes.</summary>
	public static IEnumerable<AllowEntry> Covering(AllowCase c)
	{
		return Entries.Where(e => e.Matches(c));
	}
}
