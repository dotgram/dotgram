using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

/// <summary>
/// A rule spelled from single characters where trivia may stand between them.
/// </summary>
/// <remarks>
/// <para>
/// §4.5 weaves <c>trivia</c> between the operands of every sequence, so a rule meant as a
/// lexeme and declared where <c>trivia</c> is not empty reads <c>1 . 5</c> as a number and
/// <c>i f</c> as a name. Whether a rule is a lexeme cannot be told from its shape, since
/// <c>Word &amp; ('.' &amp; Word)*</c> is the same shape as a dotted number and is
/// deliberate. What can be told is the one shape that is almost never meant: every operand
/// tests one input item, at least one of them a class, and a seam is woven between two of
/// them.
/// </para>
/// <para>
/// Judged on the rule as declared, once it is lowered and before anything is specialized or
/// cloned: <c>List(['a'..'z'], ',')</c> is a list of letters that is meant to be spaced, and
/// the rule it makes is not what an author wrote. A rule that names <c>trivia</c> has said
/// where the space goes, and the rules <c>trivia</c> is made of have their seams taken back
/// out (<see cref="UnweaveTrivia"/>), so neither is judged.
/// </para>
/// <para>
/// A run such as <c>Std.Digits</c> or <c>Word</c> tests many items and is not one operand
/// of this kind, so <c>Std.Digits &amp; ('.' &amp; Std.Digits)?</c> is not caught: it looks
/// like a dotted name, which is §4.5's own example of what the compiler cannot tell apart.
/// </para>
/// </remarks>
public sealed partial class GrammarNormalizer
{
	/// <summary>A rule built from single characters, declared where trivia is not empty.</summary>
	public const string SpacedLexeme = "GRAM4043";

	void CheckSpacedLexemes()
	{
		HashSet<RuleSymbol>? made = null;

		foreach (var rule in _rules.ToList())
		{
			if (rule.Declaration is not { Params.Count: 0, Replaces: null } declaration ||
				_origins.ContainsKey(rule) ||
				!_trivia.TryGetValue(rule, out var seam) ||
				!_bodies.TryGetValue(rule, out var body))
				continue;

			made ??= TriviaRules();

			if (made.Contains(rule) || NamesTrivia(declaration.Body))
				continue;

			var found = new Spacing();

			if (Operand(body, seam, found) != ItemKind.Reads || !found.HasClass || !found.HasSeam)
				continue;

			var at = declaration.At;

			Warn(
				SpacedLexeme,
				$"'{rule.Name}' is spelled from single characters but is declared where trivia is not " +
				"empty, so trivia may stand between them: it reads '1 . 5' as one number and 'i f' as " +
				"one name. Write '~' between its parts or declare it where 'trivia = none'; if the " +
				"space is meant, write 'trivia' there (docs/syntax.md §4.5).",
				at);
		}
	}

	/// <summary>Every rule the namespaces' trivia is made of, whose seams are taken out again.</summary>
	HashSet<RuleSymbol> TriviaRules()
	{
		var made = new HashSet<RuleSymbol>();

		foreach (var trivia in _model.Trivia.Values.Distinct())
			if (Nullable(trivia, []))
				made.UnionWith(Reaches(trivia));

		return made;
	}

	/// <summary>Whether a rule's own text mentions <c>trivia</c>.</summary>
	static bool NamesTrivia(Expr expression)
	{
		var pending = new Stack<Expr>([expression]);

		while (pending.Count > 0)
		{
			var one = pending.Pop();

			if (one is Expr.Reference { IsCSharp: false, Name: "trivia" })
				return true;

			foreach (var child in Dump.Children(one))
				pending.Push(child);
		}

		return false;
	}

	enum ItemKind
	{
		/// <summary>Not one input item: a run, a word, something computed.</summary>
		Other,

		/// <summary>Reads nothing: a look, a guard, a glue.</summary>
		Nothing,

		/// <summary>Reads input items, one at a time.</summary>
		Reads,
	}

	sealed class Spacing
	{
		public bool HasClass;
		public bool HasSeam;
	}

	/// <summary>What a node is, as an operand: the items it tests are all single ones.</summary>
	ItemKind Operand(Node node, Node seam, Spacing found)
	{
		switch (node)
		{
			case Node.Element:
				found.HasClass = true;
				return ItemKind.Reads;

			case Node.Literal { Text.Length: 1 }:
			case Node.Same:
				return ItemKind.Reads;

			case Node.Guard or Node.Condition or Node.Reading or Node.Behind or Node.Lookahead or Node.Glue or Node.Empty:
				return ItemKind.Nothing;

			case Node.Call(var called, { Count: 0 }):
				return Called(called, found);

			case Node.Atomic(var kept):
				return Operand(kept, seam, found);

			case Node.Marked(var kept, _):
				return Operand(kept, seam, found);

			case Node.Capture(_, var kept):
				return Operand(kept, seam, found);

			case Node.Construct(var kept, _):
				return Operand(kept, seam, found);

			case Node.Bind(_, var kept, _):
				return Operand(kept, seam, found);

			case Node.Choice(var alternatives):
			{
				var result = ItemKind.Nothing;

				foreach (var alternative in alternatives)
				{
					var one = Operand(alternative, seam, found);

					if (one == ItemKind.Other)
						return ItemKind.Other;

					if (one == ItemKind.Reads)
						result = ItemKind.Reads;
				}

				return result;
			}

			case Node.Repeat(Node.Sequence(var turn) body, _, var max):
			{
				var result = Operand(body, seam, found);

				// The seam in front of each turn: it stands between one turn and the next.
				if (result == ItemKind.Reads && turn.Count > 0 && IsSeam(turn[0], seam) && max is null or > 1)
					found.HasSeam = true;

				return result;
			}

			case Node.Repeat(var body, _, _):
				return Operand(body, seam, found);

			case Node.Sequence(var parts):
			{
				var reads = false;
				var apart = false;

				foreach (var part in parts)
				{
					if (IsSeam(part, seam))
					{
						apart = reads;
						continue;
					}

					switch (Operand(part, seam, found))
					{
						case ItemKind.Other:
							return ItemKind.Other;

						case ItemKind.Reads:
							if (apart)
								found.HasSeam = true;

							apart = false;
							reads = true;
							break;
					}
				}

				return reads ? ItemKind.Reads : ItemKind.Nothing;
			}

			default:
				return ItemKind.Other;
		}
	}

	/// <summary>A call to a rule that is itself one input item and builds nothing.</summary>
	ItemKind Called(RuleSymbol rule, Spacing found)
	{
		if (rule.IsBuiltIn)
		{
			switch (rule.Name)
			{
				case "any":
					found.HasClass = true;
					return ItemKind.Reads;

				case "eol":
					return ItemKind.Reads;

				case "none" or "eof" or "wordboundary":
					return ItemKind.Nothing;

				default:
					return ItemKind.Other;
			}
		}

		if (rule.Declaration is not { Params.Count: 0, Type: null })
			return ItemKind.Other;

		var inner = new Spacing();

		if (!OneItem(BodyOf(rule), inner, [rule]))
			return ItemKind.Other;

		found.HasClass |= inner.HasClass;

		return ItemKind.Reads;
	}

	/// <summary>Whether a node is exactly one input item, drawn from a set or a few spellings.</summary>
	bool OneItem(Node node, Spacing found, HashSet<RuleSymbol> seen)
	{
		switch (node)
		{
			case Node.Element:
				found.HasClass = true;
				return true;

			case Node.Literal { Text.Length: 1 }:
				return true;

			case Node.Atomic(var kept):
				return OneItem(kept, found, seen);

			case Node.Choice(var alternatives):
				return alternatives.All(one => OneItem(one, found, seen));

			case Node.Call(var called, { Count: 0 }) when called.IsBuiltIn:
				if (called.Name == "any")
				{
					found.HasClass = true;
					return true;
				}

				return false;

			case Node.Call(var called, { Count: 0 }) when called.Declaration is { Params.Count: 0, Type: null }:
				return seen.Add(called) && OneItem(BodyOf(called), found, seen);

			default:
				return false;
		}
	}
}
