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

/// <summary>
/// One entry of the allow-list: a class of grammars whose value differs from the published version's on purpose. The
/// predicate says, from the shape of the grammar and the two outcomes, whether a difference belongs to the class.
/// </summary>
sealed record AllowEntry(string Name, string Description, Func<RuleShape, Mode, Outcome, Newer, Grammar, bool> Matches);

/// <summary>
/// The classes of difference from the published version, and nothing else: an entry may not be added, widened or removed
/// to cover a difference. A difference that no entry covers is a finding; an entry that covers no cell of a run fails
/// the run.
/// </summary>
static class RuleAllowList
{
	// Every predicate is read with the switch the grammar was read with: the rule of the specification, the shape of the
	// grammar, and the two outcomes. The published side built the grammar in each of them.

	static bool Differs(Outcome old, Newer now)
	{
		return old.Kind == "value" && (now.Refused || now.Value != old.Text);
	}

	static bool ValueDiffers(Outcome old, Newer now)
	{
		return old.Kind == "value" && now.Built && now.Value is not null && now.Value != old.Text;
	}

	static bool Any(Grammar grammar, Func<Grammar, Rule, bool> test)
	{
		return grammar.Rules.Any(r => test(grammar, r));
	}

	static IEnumerable<Expr> Nodes(Rule rule)
	{
		var found = new List<Expr>();

		void Visit(Expr expr)
		{
			found.Add(expr);

			switch (expr)
			{
				case Named named:
					Visit(named.Inner);
					break;

				case Sequence sequence:
					foreach (var item in sequence.Items)
						Visit(item);

					break;

				case Choice choice:
					foreach (var alternative in choice.Alternatives)
						Visit(alternative);

					break;

				case Group group:
					Visit(group.Inner);
					break;

				case Wrapped wrapped:
					Visit(wrapped.Inner);
					break;
			}
		}

		foreach (var alternative in rule.Alternatives)
			Visit(alternative);

		return found;
	}

	/// <summary>Every entry, with the measured grammars of the class beside it in the fixed cells of the gate.</summary>
	public static readonly IReadOnlyList<AllowEntry> Entries =
	[
		new("own-or-taken-operand-beats-construction",
			"names fill a construction and the rule reads one operand on every route: the operand is the value, not the construction",
			static (_, _, old, now, grammar) => ValueDiffers(old, now) && Any(grammar, static (g, r) =>
				r.Decision!.Source == Source.Operand && g.WritesConstructionNames(r) && !(r.IsTextType && r.Said == Said.Own))),

		new("taken-span-hands-its-bounds-on",
			"a rule that takes the type of a span that makes its value hands that span's bounds on; two such spans on a route are an error",
			static (_, _, old, now, grammar) => Differs(old, now) && Any(grammar, static (g, r) =>
				r.Said == Said.Taken && r.Type == "SourceSpan" && g.TakenTextPassesOn
				&& (r.Decision!.Source is Source.Operand or Source.PerRoute || r.Decision.ErrorId == "E-VALUE"))),

		new("span-array-skips-an-extent-span",
			"an array of spans does not collect a span rule that is its own bounds: it is text, as a text rule is",
			static (_, _, old, now, grammar) => Differs(old, now) && Any(grammar, static (g, r) =>
				r.Said == Said.Sequence && r.Type == "SourceSpan"
				&& Nodes(r).OfType<Call>().Any(c => c.Atom is not null && g.HelperOf(c.Rule.Name) is { Type: "SourceSpan", Making: false }))),

		new("array-skips-a-named-non-operand",
			"an array does not collect what a name sits on when that is not an operand",
			static (_, _, old, now, grammar) => Differs(old, now) && Any(grammar, static (g, r) =>
				r.Said == Said.Sequence
				&& Nodes(r).OfType<Named>().Any(n => !(n.Inner is Call { Atom: not null } c && g.IsOperandOf(c, r))))),

		new("array-skips-a-named-operand",
			"an array does not collect an operand that a name sits on directly: it is the author's",
			static (_, _, old, now, grammar) => Differs(old, now) && Any(grammar, static (g, r) =>
				r.Said == Said.Sequence
				&& Nodes(r).OfType<Named>().Any(n => n.Inner is Call { Atom: not null } c && g.IsOperandOf(c, r)))),

		new("capture-named-like-the-compilers",
			"a capture an author names the way the generator names its own is the author's capture, and is no longer the value",
			static (_, _, old, now, grammar) => Differs(old, now) && Any(grammar, static (_, r) =>
				Nodes(r).OfType<Named>().Any(static n => n.Name == "items"))),

		new("optional-operand-gives-the-routes-text",
			"a rule that takes a text type reads an optional operand: where there is none, the value is the rule's text (the published version gave null)",
			static (_, _, old, now, grammar) => old.WarnedNull && Differs(old, now) && Any(grammar, static (g, r) =>
				r.Said == Said.Taken && r.IsTextType && r.Decision!.Source == Source.PerRoute
				&& r.Routes.Any(static x => x.Operands == 0) && r.Routes.Any(static x => x.Operands > 0))),

		new("optional-operand-beside-construction",
			"names fill a construction and the operand is optional: the construction is the value where the operand is missing (the published version gave the operand or null)",
			static (_, _, old, now, grammar) => old.WarnedNull && Differs(old, now) && Any(grammar, static (g, r) =>
				r.Said != Said.Sequence && r.Decision!.Source == Source.Constructed && g.WritesConstructionNames(r)
				&& r.Routes.Any(static x => x.Operands > 0))),

		new("optional-operand-of-a-value-type",
			"a rule of a type that is not a text type reads an operand on some routes and none on others: it is an error, not null",
			static (_, _, old, now, grammar) => old.WarnedNull && Differs(old, now) && Any(grammar, static (g, r) =>
				r.Said != Said.Sequence && !r.IsTextType && r.Decision!.ErrorId == "E-VALUE"
				&& r.Routes.Any(static x => x.Operands > 0) && r.Routes.Any(static x => x.Operands == 0))),

		new("text-extent-whose-name-a-guard-reads-over-bytes",
			"a text extent over bytes in a grammar where a guard reads a name: left open",
			static (_, mode, _, _, g) => mode == Mode.B && g.ReadsNameByGuard() && g.Start.Type == "string" && g.Start.Decision!.Source is Source.Extent or Source.PerRoute),
	];

	/// <summary>The entries a difference belongs to.</summary>
	public static IEnumerable<AllowEntry> Covering(RuleShape shape, Mode mode, Outcome old, Newer now, Grammar grammar)
	{
		return Entries.Where(e => e.Matches(shape, mode, old, now, grammar));
	}
}
