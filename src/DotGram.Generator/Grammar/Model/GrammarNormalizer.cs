using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Syntax;

namespace DotGram.Grammar.Model;

/// <summary>
/// Lowers the bound syntax tree into a recognition graph and folds what can be folded.
/// </summary>
/// <remarks>
/// <para>
/// Folding is allowed only where it provably cannot change meaning. Adjacent literals
/// merge because concatenation is what a sequence of them already means; adjacent
/// single-item alternatives merge into one set because a set matches exactly one item,
/// so their order among themselves cannot matter.
/// </para>
/// <para>
/// What is not done is reordering. Roc's macro moved every single-character
/// alternative ahead of the rest, which turns <c>"ab" | 'a'</c> into <c>'a' | "ab"</c>
/// and makes the second unreachable. Here only an adjacent run is merged, so nothing
/// crosses anything else.
/// </para>
/// </remarks>
public sealed class GrammarNormalizer
{
	public const string NullableRepetition = "GRAM4001";
	public const string LeftRecursion      = "GRAM4002";
	public const string TriviaNotNullable  = "GRAM4003";
	public const string ShadowedAlternative = "GRAM4004";

	readonly GrammarModel                     _model;
	readonly Dictionary<RuleSymbol, Node>     _bodies   = [];
	readonly Dictionary<RuleSymbol, bool>     _nullable = [];
	readonly List<GramDiagnostic>             _diagnostics = [];
	readonly List<RuleSymbol>                 _rules = [];

	GrammarNormalizer(GrammarModel model) => _model = model;

	public static RecognitionGraph Normalize(GrammarModel model)
	{
		if (model is null)
			throw new ArgumentNullException(nameof(model));

		var normalizer = new GrammarNormalizer(model);

		normalizer.LowerAll(model.Root);
		normalizer.ComputeNullability();
		normalizer.Check();

		return new RecognitionGraph(
			normalizer._rules, normalizer._bodies, normalizer._nullable, normalizer._diagnostics);
	}

	void Report(string id, string message, int position, int length) =>
		_diagnostics.Add(new GramDiagnostic(id, message, position, length, GramSeverity.Error));

	// ── Lowering ─────────────────────────────────────────────────────────────────

	void LowerAll(GrammarScope scope)
	{
		foreach (var rule in scope.Rules.Values)
		{
			if (rule.Declaration is null)
				continue;

			_rules.Add(rule);
			_bodies[rule] = Lower(rule.Declaration.Body, scope);
		}

		foreach (var nested in scope.Nested)
			LowerAll(nested);
	}

	Node Lower(SyntaxNode node, GrammarScope scope) => node switch
	{
		LiteralExpression literal      => new LiteralNode(literal.Value),
		ElementSetExpression set       => LowerElementSet(set),
		GroupExpression group          => Lower(group.Body, scope),
		CaptureExpression capture      => new CaptureNode(capture.Name, Lower(capture.Operand, scope)),
		LookaheadExpression lookahead  => new LookaheadNode(lookahead.IsPositive, Lower(lookahead.Operand, scope)),
		GuardExpression guard          => new GuardNode(guard.Value.ToString().Trim()),
		QuantifiedExpression quantified => LowerQuantifier(quantified, scope),
		SequenceExpression sequence    => LowerSequence(sequence, scope),
		ChoiceExpression choice        => LowerChoice(choice, scope),

		AlternativeExpression alternative => new ConstructNode(
			Lower(alternative.Pattern, scope),
			alternative.Construction?.ToString().Trim() ?? ""),

		CallExpression call => new RuleCallNode(
			Bound(call.Target) as RuleSymbol ?? Unresolved(call.Target.Name),
			[.. call.Arguments.Select(argument => Lower(argument, scope))]),

		ReferenceExpression reference => Bound(reference) is RuleSymbol rule
			? new RuleCallNode(rule, [])
			: new ElementNode(false, [], [], [Bound(reference) ?? Unresolved(reference.Name)]),

		CSharpExpression expression => new GuardNode($"@({expression.Text})"),

		_ => EmptyNode.Instance,
	};

	Symbol? Bound(SyntaxNode node) => _model.Bindings.TryGetValue(node, out var symbol) ? symbol : null;

	/// <summary>Binding already reported it; a placeholder keeps lowering going.</summary>
	static RuleSymbol Unresolved(string name) =>
		new(name, new GrammarScope("<unresolved>", null), Declaration: null);

	static ElementNode LowerElementSet(ElementSetExpression set)
	{
		var ranges     = new List<CharRange>();
		var categories = new List<string>();
		var references = new List<Symbol>();

		foreach (var item in set.Items)
		{
			switch (item)
			{
				case CharacterRangeItem range when range.From.Length > 0:
					ranges.Add(new CharRange(range.From[0], (range.To ?? range.From)[0]));
					break;

				case UnicodeCategoryItem category:
					categories.Add(category.Category);
					break;

				case ReferenceItem reference:
					references.Add(new RuleSymbol(reference.Reference.Name, new GrammarScope("", null), null));
					break;
			}
		}

		return new ElementNode(set.IsNegated, Coalesce(ranges), categories, references);
	}

	/// <summary>
	/// Sorts and merges ranges: `'a' | 'b'` becomes `'a'..'b'`, a range swallows what it
	/// contains, duplicates fall away. Order among them is not observable, since a set
	/// matches exactly one item — which is what makes this fold legal.
	/// </summary>
	static IReadOnlyList<CharRange> Coalesce(List<CharRange> ranges)
	{
		if (ranges.Count < 2)
			return ranges;

		ranges.Sort((x, y) => x.From != y.From ? x.From.CompareTo(y.From) : x.To.CompareTo(y.To));

		var merged = new List<CharRange> { ranges[0] };

		for (var i = 1; i < ranges.Count; i++)
		{
			var last = merged[merged.Count - 1];
			var next = ranges[i];

			if (next.From <= last.To || next.From == last.To + 1)
				merged[merged.Count - 1] = new CharRange(last.From, (char)Math.Max(last.To, next.To));
			else
				merged.Add(next);
		}

		return merged;
	}

	Node LowerQuantifier(QuantifiedExpression quantified, GrammarScope scope)
	{
		var body = Lower(quantified.Operand, scope);

		var (min, max) = quantified.Kind switch
		{
			QuantifierKind.Optional   => (0, (int?)1),
			QuantifierKind.ZeroOrMore => (0, null),
			QuantifierKind.OneOrMore  => (1, null),
			_                         => (quantified.Min ?? 0, quantified.Max),
		};

		return new RepeatNode(body, min, max);
	}

	Node LowerSequence(SequenceExpression sequence, GrammarScope scope)
	{
		var nodes  = new List<Node>();
		var trivia = TriviaFor(scope);

		foreach (var operand in sequence.Operands)
		{
			if (nodes.Count > 0 && trivia is not null)
				nodes.Add(trivia);

			nodes.Add(Lower(operand, scope));
		}

		return Flatten(MergeLiterals(nodes));
	}

	/// <summary>
	/// The `Trivia` this scope sees, or null when it matches nothing — in which case the
	/// insertions are not emitted at all rather than emitted and skipped (§4.5).
	/// </summary>
	Node? TriviaFor(GrammarScope scope) =>
		_model.Trivia.TryGetValue(scope, out var trivia) && !MatchesNothing(trivia, [])
			? new RuleCallNode(trivia, [])
			: null;

	/// <summary>
	/// Whether a rule can only ever match the empty sequence. Stronger than nullable,
	/// which merely allows it — and it is the stronger property that lets an insertion
	/// be dropped rather than kept and skipped at run time.
	/// </summary>
	bool MatchesNothing(RuleSymbol rule, HashSet<RuleSymbol> seen) =>
		rule.IsBuiltIn
			? rule.Name is "none" or "Trivia" or "eof"
			: seen.Add(rule) && _bodies.TryGetValue(rule, out var body) && MatchesNothing(body, seen);

	bool MatchesNothing(Node node, HashSet<RuleSymbol> seen) => node switch
	{
		EmptyNode        => true,
		LiteralNode l    => l.Text.Length == 0,
		RepeatNode r     => r.Max == 0 || MatchesNothing(r.Body, seen),
		SequenceNode s   => s.Nodes.All(child => MatchesNothing(child, seen)),
		ChoiceNode c     => c.Nodes.All(child => MatchesNothing(child, seen)),
		CaptureNode c    => MatchesNothing(c.Body, seen),
		ConstructNode c  => MatchesNothing(c.Body, seen),
		RuleCallNode r   => MatchesNothing(r.Rule, seen),
		_                => false,
	};

	/// <summary>`'a' & 'b'` is `"ab"`: a sequence of literals already means their
	/// concatenation.</summary>
	static List<Node> MergeLiterals(List<Node> nodes)
	{
		var merged = new List<Node>();

		foreach (var node in nodes)
		{
			if (node is LiteralNode literal &&
				merged.Count > 0 &&
				merged[merged.Count - 1] is LiteralNode previous)
			{
				merged[merged.Count - 1] = new LiteralNode(previous.Text + literal.Text);
				continue;
			}

			merged.Add(node);
		}

		return merged;
	}

	static Node Flatten(List<Node> nodes)
	{
		var flat = new List<Node>();

		foreach (var node in nodes)
		{
			if (node is SequenceNode nested)
				flat.AddRange(nested.Nodes);
			else
				flat.Add(node);
		}

		return flat.Count == 1 ? flat[0] : new SequenceNode(flat);
	}

	Node LowerChoice(ChoiceExpression choice, GrammarScope scope)
	{
		var nodes = choice.Alternatives.Select(alternative => Lower(alternative, scope)).ToList();

		ReportShadowedAlternatives(nodes, choice);

		var merged = MergeAdjacentElements(nodes);

		// A choice of one is that one: merging alternatives into a set routinely leaves
		// a single node behind, and keeping a wrapper around it would show up in every
		// dump and in every generated switch.
		return merged.Count == 1 ? merged[0] : new ChoiceNode(merged);
	}

	/// <summary>
	/// Merges a run of adjacent single-item alternatives into one set. Only a run:
	/// merging across a multi-item alternative would move something past it, and that
	/// is the mistake Roc's macro made.
	/// </summary>
	static IReadOnlyList<Node> MergeAdjacentElements(List<Node> nodes)
	{
		var merged = new List<Node>();
		var run    = new List<Node>();

		void FlushRun()
		{
			if (run.Count == 0)
				return;

			merged.Add(run.Count == 1 ? run[0] : Combine(run));
			run.Clear();
		}

		foreach (var node in nodes)
		{
			if (IsSingleItem(node))
				run.Add(node);
			else
			{
				FlushRun();
				merged.Add(node);
			}
		}

		FlushRun();

		return merged;
	}

	static bool IsSingleItem(Node node) => node switch
	{
		LiteralNode literal => literal.Text.Length == 1,
		ElementNode element => !element.IsNegated,
		_                   => false,
	};

	static ElementNode Combine(List<Node> run)
	{
		var ranges     = new List<CharRange>();
		var categories = new List<string>();
		var references = new List<Symbol>();

		foreach (var node in run)
		{
			switch (node)
			{
				case LiteralNode literal:
					ranges.Add(new CharRange(literal.Text[0], literal.Text[0]));
					break;

				case ElementNode element:
					ranges.AddRange(element.Ranges);
					categories.AddRange(element.Categories);
					references.AddRange(element.References);
					break;
			}
		}

		return new ElementNode(false, Coalesce(ranges), categories, references);
	}

	/// <summary>
	/// An alternative that a preceding literal shadows as a prefix can never be
	/// reached. Diagnosed rather than repaired — see docs/syntax.md §10.
	/// </summary>
	void ReportShadowedAlternatives(List<Node> nodes, ChoiceExpression choice)
	{
		for (var later = 1; later < nodes.Count; later++)
		{
			if (nodes[later] is not LiteralNode shadowed)
				continue;

			for (var earlier = 0; earlier < later; earlier++)
			{
				if (nodes[earlier] is not LiteralNode first ||
					first.Text.Length > shadowed.Text.Length ||
					!shadowed.Text.StartsWith(first.Text, StringComparison.Ordinal))
				{
					continue;
				}

				var alternative = choice.Alternatives[later];

				Report(
					ShadowedAlternative,
					$"Alternative \"{shadowed.Text}\" is unreachable — \"{first.Text}\" shadows it as a prefix.",
					alternative.Position,
					alternative.Length);

				break;
			}
		}
	}

	// ── Nullability and the checks that need it ──────────────────────────────────

	/// <summary>
	/// Whether a rule can match without consuming anything. Reached by fixpoint, since
	/// rules call one another.
	/// </summary>
	void ComputeNullability()
	{
		SeedBuiltIns(_model.Root);

		foreach (var rule in _rules)
			_nullable[rule] = false;

		for (var changed = true; changed; )
		{
			changed = false;

			foreach (var rule in _rules)
			{
				var nullable = IsNullable(_bodies[rule]);

				if (nullable != _nullable[rule])
				{
					_nullable[rule] = nullable;
					changed         = true;
				}
			}
		}
	}

	/// <summary>
	/// Built-in rules have no body to compute from, so their nullability is stated:
	/// `none`, `eof` and the default `Trivia` consume nothing, `any` and `eol` consume.
	/// </summary>
	void SeedBuiltIns(GrammarScope scope)
	{
		for (var outer = scope; outer is not null; outer = outer.Parent)
			foreach (var rule in outer.Rules.Values)
				if (rule.IsBuiltIn)
					_nullable[rule] = rule.Name is "none" or "eof" or "Trivia";
	}

	bool IsNullable(Node node) => node switch
	{
		EmptyNode        => true,
		LiteralNode l    => l.Text.Length == 0,
		ElementNode      => false,
		GuardNode        => true,
		LookaheadNode    => true,
		CaptureNode c    => IsNullable(c.Body),
		ConstructNode c  => IsNullable(c.Body),
		RepeatNode r     => r.Min == 0 || IsNullable(r.Body),
		SequenceNode s   => s.Nodes.All(IsNullable),
		ChoiceNode c     => c.Nodes.Any(IsNullable),
		RuleCallNode r   => _nullable.TryGetValue(r.Rule, out var nullable) && nullable,
		_                => false,
	};

	void Check()
	{
		foreach (var rule in _rules)
		{
			CheckRepetitions(_bodies[rule], rule);
			CheckLeftRecursion(rule);
		}

		CheckTrivia();
	}

	void CheckRepetitions(Node node, RuleSymbol rule)
	{
		if (node is RepeatNode repeat && repeat.Max != 1 && IsNullable(repeat.Body))
		{
			Report(
				NullableRepetition,
				$"The body of a repetition in '{rule.Name}' can match without consuming input, so the repetition would not terminate.",
				rule.Declaration!.Position,
				rule.Declaration.Length);
		}

		foreach (var child in Children(node))
			CheckRepetitions(child, rule);
	}

	/// <summary>
	/// A rule that can reach itself without consuming anything first. Nullability is
	/// what makes this more than a syntactic check: `A = B & A` is left-recursive
	/// exactly when `B` is nullable.
	/// </summary>
	void CheckLeftRecursion(RuleSymbol start)
	{
		var seen = new HashSet<RuleSymbol>();

		if (!Reaches(_bodies[start], start, seen))
			return;

		Report(
			LeftRecursion,
			$"'{start.Name}' is left-recursive; write the loop with a quantifier instead.",
			start.Declaration!.Position,
			start.Declaration.Length);
	}

	bool Reaches(Node node, RuleSymbol target, HashSet<RuleSymbol> seen)
	{
		switch (node)
		{
			case RuleCallNode call when ReferenceEquals(call.Rule, target):
				return true;

			case RuleCallNode call:
				return seen.Add(call.Rule) &&
					_bodies.TryGetValue(call.Rule, out var body) &&
					Reaches(body, target, seen);

			case SequenceNode sequence:
				foreach (var child in sequence.Nodes)
				{
					if (Reaches(child, target, seen))
						return true;

					if (!IsNullable(child))
						return false;
				}

				return false;

			case ChoiceNode choice:
				return choice.Nodes.Any(child => Reaches(child, target, seen));

			case CaptureNode capture:   return Reaches(capture.Body, target, seen);
			case ConstructNode build:   return Reaches(build.Body, target, seen);
			case RepeatNode repeat:     return Reaches(repeat.Body, target, seen);
			case LookaheadNode look:    return Reaches(look.Body, target, seen);

			default: return false;
		}
	}

	/// <summary>
	/// `Trivia` has to accept empty input. That single condition is what lets it be
	/// inserted everywhere without doubling (§4.5), so it is worth a message of its own.
	/// </summary>
	void CheckTrivia()
	{
		foreach (var trivia in _model.Trivia.Values.Distinct())
		{
			if (trivia.Declaration is null || IsNullable(_bodies[trivia]))
				continue;

			Report(
				TriviaNotNullable,
				"'Trivia' must accept empty input: it is inserted between every pair of operands, and a required match would demand whitespace everywhere.",
				trivia.Declaration.Position,
				trivia.Declaration.Length);
		}
	}

	static IEnumerable<Node> Children(Node node) => node switch
	{
		SequenceNode s  => s.Nodes,
		ChoiceNode c    => c.Nodes,
		RepeatNode r    => [r.Body],
		CaptureNode c   => [c.Body],
		ConstructNode c => [c.Body],
		LookaheadNode l => [l.Body],
		RuleCallNode r  => r.Arguments,
		_               => [],
	};
}
