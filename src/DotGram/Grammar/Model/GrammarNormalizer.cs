using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

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
/// <para>
/// Lowering walks the located tree and matches on the shape it carries. Both are to
/// hand at once, which is the point of keeping them apart: the deep patterns below
/// read as shapes, while a diagnostic still has a node to point at.
/// </para>
/// </remarks>
public sealed class GrammarNormalizer
{
	public const string NullableRepetition  = "GRAM4001";
	public const string LeftRecursion       = "GRAM4002";
	public const string TriviaNotNullable   = "GRAM4003";
	public const string UnsupportedElement  = "GRAM4005";
	public const string UnbuiltCapture      = "GRAM4006";
	public const string CaptureTypeMismatch = "GRAM4007";

	readonly GrammarModel                                      _model;
	readonly Dictionary<RuleSymbol, Node>                      _bodies      = [];
	readonly Dictionary<RuleSymbol, bool>                      _nullable    = [];
	readonly Dictionary<RuleSymbol, IReadOnlyList<ResultMember>> _results   = [];
	readonly List<GramDiagnostic>                              _diagnostics = [];
	readonly List<RuleSymbol>                                  _rules       = [];

	GrammarNormalizer(GrammarModel model) => _model = model;

	public static RecognitionGraph Normalize(GrammarModel model)
	{
		if (model is null)
			throw new ArgumentNullException(nameof(model));

		var normalizer = new GrammarNormalizer(model);

		normalizer.Collect(model.Root);
		normalizer.LowerAll();
		normalizer.ComputeNullability();
		normalizer.ComputeResults();
		normalizer.Check();

		return new RecognitionGraph(
			normalizer._rules,
			normalizer._bodies,
			normalizer._nullable,
			normalizer._results,
			model.Publications,
			normalizer._diagnostics);
	}

	void Report(string id, string message, Location at) =>
		_diagnostics.Add(new GramDiagnostic(id, message, at.Position, at.Length, GramSeverity.Error));

	// ── Lowering ─────────────────────────────────────────────────────────────────

	/// <summary>
	/// Every declared rule, collected before any of them is lowered.
	/// </summary>
	/// <remarks>
	/// Two passes rather than one, so that lowering may ask for another rule's body and
	/// get it whatever the declaration order was — which a reference inside an element
	/// set needs, since it has to be merged into the set that names it.
	/// </remarks>
	void Collect(GrammarScope scope)
	{
		foreach (var rule in scope.Rules.Values)
			if (rule.Declaration is not null)
				_rules.Add(rule);

		foreach (var nested in scope.Nested)
			Collect(nested);
	}

	void LowerAll()
	{
		// Indexed, because lowering registers built-ins and appends them.
		for (var i = 0; i < _rules.Count; i++)
			BodyOf(_rules[i]);
	}

	/// <summary>A rule's lowered body, lowering it now if that has not happened yet.</summary>
	Node BodyOf(RuleSymbol rule)
	{
		if (_bodies.TryGetValue(rule, out var body))
			return body;

		if (rule.Declaration is null)
			return Node.Empty.Instance;

		// Placed before lowering: a rule whose body reaches itself would otherwise
		// recurse for ever here rather than being reported by the left-recursion check.
		_bodies[rule] = Node.Empty.Instance;

		return _bodies[rule] = Lower(rule.Declaration.Body, rule.Scope);
	}

	Node Lower(Expr expression, GrammarScope scope) => expression switch
	{
		Expr.Literal(_, var value)              => new Node.Literal(value),
		Expr.ElementSet(var negated, var items) => LowerElementSet(negated, items, expression),
		Expr.Group(var body)                    => Lower(body, scope),
		Expr.Capture(var name, var operand)     => new Node.Capture(name, Lower(operand, scope)),
		Expr.Lookahead(var positive, var operand) => new Node.Lookahead(positive, Lower(operand, scope)),
		Expr.Guard(var value)                   => new Node.Guard(Text(value)),
		Expr.CSharp(var text)                   => new Node.Guard($"@({text})"),

		Expr.Construct(var pattern, var value)  => new Node.Construct(Lower(pattern, scope), Text(value)),

		Expr.Quantified(var operand, var kind, var min, _, var max, _) =>
			new Node.Repeat(Lower(operand, scope), Bounds(kind, min).Min, Bounds(kind, max).Max),

		Expr.Sequence(var operands)             => LowerSequence(operands, scope),
		Expr.Choice(var alternatives)           => LowerChoice(alternatives, scope),

		Expr.Call(var target, var arguments) => CallTo(
			RuleOf(expression, target.Name),
			[.. arguments.Select(argument => Lower(argument, scope))]),

		Expr.Reference(_, var name, _) => _model.Bindings.TryGetValue(expression, out var symbol) && symbol is RuleSymbol rule
			? CallTo(rule, [])
			: new Node.Element(false, [], [], [symbol ?? Unresolved(name)]),

		_ => Node.Empty.Instance,
	};

	/// <summary>
	/// A call — and, the first time a built-in is called, the body it is a call to.
	/// </summary>
	/// <remarks>
	/// §3.1 says <c>any</c>, <c>none</c>, <c>eol</c>, <c>eof</c> and <c>Trivia</c> are
	/// ordinary standard-library rules rather than keywords. This is where that stops
	/// being a claim: they are lowered into the same nodes a grammar could have written
	/// itself, so every stage downstream — nullability, the checks, emission — treats
	/// them as what they are and needs to know nothing about them.
	/// <para>
	/// Registered on demand, so a grammar that never says <c>eol</c> carries no
	/// <c>eol</c>.
	/// </para>
	/// </remarks>
	Node CallTo(RuleSymbol rule, IReadOnlyList<Node> arguments)
	{
		if (rule.IsBuiltIn && !_bodies.ContainsKey(rule))
		{
			_rules.Add(rule);
			_bodies[rule] = BuiltInBody(rule.Name);
		}

		return new Node.Call(rule, arguments);
	}

	static Node BuiltInBody(string name) => name switch
	{
		"any" => AnyItem,
		"eol" => new Node.Choice([new Node.Literal("\r\n"), new Node.Literal("\n"), new Node.Literal("\r")]),
		"eof" => new Node.Lookahead(IsPositive: false, AnyItem),

		// `none`, and `Trivia` until a grammar shadows it with one of its own (§4.5).
		_     => Node.Empty.Instance,
	};

	/// <summary>The complement of nothing: one item, whatever it is.</summary>
	static Node.Element AnyItem => new(IsNegated: true, [], [], []);

	static (int Min, int? Max) Bounds(QuantifierKind kind, int? count) => kind switch
	{
		QuantifierKind.Optional   => (0, 1),
		QuantifierKind.ZeroOrMore => (0, null),
		QuantifierKind.OneOrMore  => (1, null),
		_                         => (count ?? 0, count),
	};

	RuleSymbol RuleOf(Expr expression, string name) =>
		_model.Bindings.TryGetValue(expression, out var symbol) && symbol is RuleSymbol rule
			? rule
			: Unresolved(name);

	/// <summary>Binding already reported it; a placeholder keeps lowering going.</summary>
	static RuleSymbol Unresolved(string name) =>
		new(name, new GrammarScope("<unresolved>", null), Declaration: null);

	/// <summary>
	/// Values in `=&gt;` and `where` are C# by the time they get here — and are rendered
	/// as C#, not described.
	/// </summary>
	/// <remarks>
	/// What comes out is pasted into the generated file and compiled by C#, so a name
	/// that resolves to nothing is C#'s error to report, on the grammar's line (§7.6).
	/// The `@` is not part of it: it marks the crossing into C# and does not survive it.
	/// </remarks>
	static string Text(Expr value) => value switch
	{
		Expr.CSharp(var text)                   => $"({text})",
		Expr.Reference(_, var name, var types)  => name + TypeArguments(types),
		Expr.Call(var target, var arguments)    =>
			Text(target) + "(" + string.Join(", ", arguments.Select(Text)) + ")",

		Expr.Literal(true,  var text)           => CharRange.Quote(text[0]),
		Expr.Literal(false, var text)           => "\"" + text.Replace("\\", @"\\").Replace("\"", "\\\"") + "\"",

		_                                       => value.ToString() ?? "",
	};

	static string TypeArguments(IReadOnlyList<TypeRef> types) =>
		types.Count == 0 ? "" : "<" + string.Join(", ", types.Select(TypeName)) + ">";

	/// <summary>A type as it is written in C#, which is as the grammar wrote it.</summary>
	internal static string TypeName(TypeRef type) => type.Name + (type.IsSequence ? "[]" : "");

	/// <summary>
	/// A set of one-item tests, with references to elementary rules merged into it.
	/// </summary>
	/// <remarks>
	/// §3.1 allows only ranges, characters and references to other elementary rules
	/// inside brackets, and merging is what makes that restriction mean something: a set
	/// stays one test against one item however it was written, and nothing downstream
	/// has to know a reference was ever there.
	/// </remarks>
	Node.Element LowerElementSet(bool negated, IReadOnlyList<Elem> items, Expr set)
	{
		var ranges     = new List<CharRange>();
		var categories = new List<string>();
		var references = new List<Symbol>();

		foreach (var item in items)
		{
			switch (item)
			{
				case Elem.Chars(var from, var to) when from.Length > 0:
					ranges.Add(new CharRange(from[0], (to ?? from)[0]));
					break;

				case Elem.Category(var name):
					categories.Add(name);
					break;

				case Elem.Ref(var reference):
					Merge(reference, ranges, categories, references);
					break;
			}
		}

		return new Node.Element(negated, Coalesce(ranges), categories, references);

		void Merge(Expr.Reference reference, List<CharRange> into, List<string> alsoInto, List<Symbol> unresolved)
		{
			if (!_model.Bindings.TryGetValue(reference, out var symbol) || symbol is not RuleSymbol rule)
			{
				// A C# predicate — `[Letter | @IsDigit]` — needs the C# seam at run time,
				// which does not exist yet. Named as unbuilt rather than silently dropped.
				Report(
					UnsupportedElement,
					symbol is CSharpSymbol
						? $"'@{reference.Name}' cannot be used inside an element set yet: C# predicates are not implemented."
						: $"'{reference.Name}' is not a rule.",
					set.At);

				unresolved.Add(symbol ?? Unresolved(reference.Name));

				return;
			}

			// Not a set but a call: what it refers to must be one item drawn from a set,
			// or the brackets would be testing something that is not an item.
			if (BodyOf(rule) is not Node.Element(false, var theirRanges, var theirCategories, var theirReferences))
			{
				Report(
					UnsupportedElement,
					$"'{rule.Name}' is not an elementary rule, so it cannot appear inside an element set. " +
					"Only ranges, characters and rules that are themselves a single element set are allowed.",
					set.At);

				return;
			}

			into.AddRange(theirRanges);
			alsoInto.AddRange(theirCategories);
			unresolved.AddRange(theirReferences);
		}
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

	Node LowerSequence(IReadOnlyList<Expr> operands, GrammarScope scope)
	{
		var nodes  = new List<Node>();
		var trivia = TriviaFor(scope);

		foreach (var operand in operands)
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
			? CallTo(trivia, [])
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
		Node.Empty          => true,
		Node.Literal(var t) => t.Length == 0,
		Node.Repeat(var body, _, var max) => max == 0 || MatchesNothing(body, seen),
		Node.Sequence(var nodes)   => nodes.All(child => MatchesNothing(child, seen)),
		Node.Choice(var nodes)     => nodes.All(child => MatchesNothing(child, seen)),
		Node.Capture(_, var body)  => MatchesNothing(body, seen),
		Node.Construct(var body, _) => MatchesNothing(body, seen),
		Node.Call(var rule, _)     => MatchesNothing(rule, seen),
		_                          => false,
	};

	/// <summary>`'a' &amp; 'b'` is `"ab"`: a sequence of literals already means their
	/// concatenation.</summary>
	static List<Node> MergeLiterals(List<Node> nodes)
	{
		var merged = new List<Node>();

		foreach (var node in nodes)
		{
			if (node is Node.Literal(var text) &&
				merged.Count > 0 &&
				merged[merged.Count - 1] is Node.Literal(var previous))
			{
				merged[merged.Count - 1] = new Node.Literal(previous + text);
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
			if (node is Node.Sequence(var nested))
				flat.AddRange(nested);
			else
				flat.Add(node);
		}

		return flat.Count == 1 ? flat[0] : new Node.Sequence(flat);
	}

	Node LowerChoice(IReadOnlyList<Expr> alternatives, GrammarScope scope)
	{
		var nodes = alternatives.Select(a => Lower(a, scope)).ToList();

		var merged = MergeAdjacentElements(nodes);

		// A choice of one is that one: merging alternatives into a set routinely leaves
		// a single node behind, and keeping a wrapper around it would show up in every
		// dump and in every generated switch.
		return merged.Count == 1 ? merged[0] : new Node.Choice(merged);
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
		Node.Literal(var text) => text.Length == 1,
		Node.Element(var negated, _, _, _) => !negated,
		_ => false,
	};

	static Node.Element Combine(List<Node> run)
	{
		var ranges     = new List<CharRange>();
		var categories = new List<string>();
		var references = new List<Symbol>();

		foreach (var node in run)
		{
			switch (node)
			{
				case Node.Literal(var text):
					ranges.Add(new CharRange(text[0], text[0]));
					break;

				case Node.Element(_, var elementRanges, var elementCategories, var elementReferences):
					ranges.AddRange(elementRanges);
					categories.AddRange(elementCategories);
					references.AddRange(elementReferences);
					break;
			}
		}

		return new Node.Element(false, Coalesce(ranges), categories, references);
	}

	/// <summary>
	/// An alternative that a preceding literal shadows as a prefix can never be
	/// reached. Diagnosed rather than repaired — see docs/syntax.md §11.
	/// </summary>
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
		Node.Empty                     => true,
		Node.Literal(var text)         => text.Length == 0,
		Node.Element                   => false,
		Node.Guard                     => true,
		Node.Lookahead                 => true,
		Node.Capture(_, var body)      => IsNullable(body),
		Node.Construct(var body, _)    => IsNullable(body),
		Node.Repeat(var body, var min, _) => min == 0 || IsNullable(body),
		Node.Sequence(var nodes)       => nodes.All(IsNullable),
		Node.Choice(var nodes)         => nodes.Any(IsNullable),
		Node.Call(var rule, _)         => _nullable.TryGetValue(rule, out var nullable) && nullable,
		_                              => false,
	};

	// ── Results ──────────────────────────────────────────────────────────────────

	/// <summary>
	/// What each rule's value is made of. A rule that captures nothing has no members and
	/// keeps the value it always had — the text it matched.
	/// </summary>
	void ComputeResults()
	{
		foreach (var rule in _rules)
		{
			var body    = _bodies[rule];
			var layout  = CaptureLayout.Of(body, BuildsValue);
			var members = new List<ResultMember>();
			var slots   = new Dictionary<string, List<CaptureSlot>>(StringComparer.Ordinal);

			foreach (var slot in layout.Slots)
			{
				if (slots.TryGetValue(slot.Name, out var sharing))
				{
					// The same name in two alternatives is one member — but only if the two
					// agree on what it holds, since a member has one type.
					if (sharing[0].Rule != slot.Rule || sharing[0].IsSequence != slot.IsSequence)
						Report(
							CaptureTypeMismatch,
							$"'{slot.Name}' is captured twice in '{rule.Name}' with different types: " +
							$"{Held(sharing[0])} and {Held(slot)}.",
							rule.Declaration!.At);

					sharing.Add(slot);

					continue;
				}

				slots[slot.Name] = [slot];

				members.Add(new ResultMember(
					slot.Name,
					slot.Rule,
					slot.IsSequence,

					// A sequence is never absent: no iterations is an empty one, the same way
					// a run of no text is "".
					IsOptional: !slot.IsSequence && !Writes(body, slot.Name),
					[]));
			}

			for (var i = 0; i < members.Count; i++)
				members[i] = members[i] with
				{
					Slots = slots[members[i].Name].Select(slot => slot.Index).ToList(),
				};

			_results[rule] = members;
		}
	}

	static string Held(CaptureSlot slot) =>
		slot.Rule is null
			? "text"
			: slot.IsSequence
				? $"a sequence of '{slot.Rule.Name}'"
				: $"the value of '{slot.Rule.Name}'";

	/// <summary>Whether a rule has a value of its own — which is to say, any capture.</summary>
	bool BuildsValue(RuleSymbol rule) => _bodies.TryGetValue(rule, out var body) && HasCapture(body);

	static bool HasCapture(Node node) => node switch
	{
		Node.Capture                       => true,
		Node.Sequence(var nodes)           => nodes.Any(HasCapture),
		Node.Choice(var nodes)             => nodes.Any(HasCapture),
		Node.Repeat(var body, _, _)        => HasCapture(body),
		Node.Construct(var built, _)       => HasCapture(built),

		// Not across a call — that is another rule's result — and not into a lookahead,
		// which consumes nothing and is compiled with its captures stripped.
		_                                  => false,
	};

	/// <summary>
	/// Whether every way through this node writes <paramref name="name"/>. What decides
	/// whether the member can be null, and so whether the generated property is nullable.
	/// </summary>
	static bool Writes(Node node, string name) => node switch
	{
		Node.Capture(var captured, var body) => captured == name || Writes(body, name),
		Node.Sequence(var nodes)             => nodes.Any(child => Writes(child, name)),
		Node.Choice(var nodes)               => nodes.All(child => Writes(child, name)),
		Node.Construct(var built, _)         => Writes(built, name),

		// A run that may be empty still writes: the text of no iterations is "". Only a
		// genuine option — `X?`, which either happened or did not — leaves it unwritten.
		Node.Repeat(var body, var min, var max) => (min > 0 || max != 1) && Writes(body, name),

		_                                    => false,
	};

	void Check()
	{
		foreach (var rule in _rules)
		{
			CheckRepetitions(_bodies[rule], rule);
			CheckCaptures(_bodies[rule], rule, repeated: null);
			CheckLeftRecursion(rule);
		}

		CheckTrivia();
	}

	/// <summary>
	/// What a capture under a repetition is allowed to be, and what it is not yet.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A repeated capture of a rule that builds is a sequence of values (§7.3), and where
	/// it sits under the repetition does not matter: every iteration appends where its
	/// value is built, and an abandoned attempt truncates back to the length it pushed.
	/// </para>
	/// <para>
	/// A repeated capture of <b>text</b> is not that. §10 binds a capture tighter than a
	/// quantifier, so <c>scheme: ['a'..'z']+</c> is a capture repeated, and §7.3 gives it
	/// the text joined — which is the extent from the first iteration to the last, and is
	/// exactly that only when the capture is the whole of what repeats. Written around
	/// something else, the text between the iterations would be swept in with them.
	/// </para>
	/// <para>
	/// Inside a lookahead a capture belongs to a machine of its own that answers yes or no
	/// and hands nothing back.
	/// </para>
	/// </remarks>
	/// <param name="repeated">
	/// What the innermost enclosing repetition repeats, or null when there is none. A
	/// repetition bounded at one iteration does not count: what is under it is written at
	/// most once, which is an option rather than a run.
	/// </param>
	void CheckCaptures(Node node, RuleSymbol rule, Node? repeated, bool inLookahead = false)
	{
		if (node is Node.Capture(var name, var captured))
		{
			var collects = captured is Node.Call(var called, _) && BuildsValue(called);

			if (inLookahead)
				Report(
					UnbuiltCapture,
					$"'{name}' is captured inside a lookahead in '{rule.Name}', which is not built: " +
					"a lookahead consumes nothing and answers only whether it matched.",
					rule.Declaration!.At);

			else if (repeated is not null && !collects && !ReferenceEquals(repeated, node))
				Report(
					UnbuiltCapture,
					$"'{name}' captures text inside a repetition in '{rule.Name}' without being the whole of " +
					"what repeats, which is not built yet: the text of the iterations cannot be told from " +
					"the text between them. Move the quantifier inside the capture.",
					rule.Declaration!.At);
		}

		var inside   = node is Node.Repeat(var body, _, not 1) ? body : repeated;
		var lookings = inLookahead || node is Node.Lookahead;

		foreach (var child in Children(node))
			CheckCaptures(child, rule, inside, lookings);
	}

	void CheckRepetitions(Node node, RuleSymbol rule)
	{
		if (node is Node.Repeat(var body, _, var max) && max != 1 && IsNullable(body))
			Report(
				NullableRepetition,
				$"The body of a repetition in '{rule.Name}' can match without consuming input, so the repetition would not terminate.",
				rule.Declaration!.At);

		foreach (var child in Children(node))
			CheckRepetitions(child, rule);
	}

	/// <summary>
	/// A rule that can reach itself without consuming anything first. Nullability is
	/// what makes this more than a syntactic check: `A = B &amp; A` is left-recursive
	/// exactly when `B` is nullable.
	/// </summary>
	void CheckLeftRecursion(RuleSymbol start)
	{
		if (Reaches(_bodies[start], start, []))
			Report(
				LeftRecursion,
				$"'{start.Name}' is left-recursive, which is not built yet (docs/syntax.md §4.3); " +
				"write the loop with a quantifier instead.",
				start.Declaration!.At);
	}

	bool Reaches(Node node, RuleSymbol target, HashSet<RuleSymbol> seen)
	{
		switch (node)
		{
			case Node.Call(var rule, _) when ReferenceEquals(rule, target):
				return true;

			case Node.Call(var rule, _):
				return seen.Add(rule) &&
					_bodies.TryGetValue(rule, out var body) &&
					Reaches(body, target, seen);

			case Node.Sequence(var nodes):

				foreach (var child in nodes)
				{
					if (Reaches(child, target, seen))
						return true;

					if (!IsNullable(child))
						return false;
				}

				return false;

			case Node.Choice(var nodes):        return nodes.Any(child => Reaches(child, target, seen));
			case Node.Capture(_, var captured): return Reaches(captured, target, seen);
			case Node.Construct(var built, _):  return Reaches(built, target, seen);
			case Node.Repeat(var repeated, _, _): return Reaches(repeated, target, seen);
			case Node.Lookahead(_, var ahead):  return Reaches(ahead, target, seen);

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
				trivia.Declaration.At);
		}
	}

	static IEnumerable<Node> Children(Node node) => node switch
	{
		Node.Sequence(var nodes)        => nodes,
		Node.Choice(var nodes)          => nodes,
		Node.Repeat(var body, _, _)     => [body],
		Node.Capture(_, var body)       => [body],
		Node.Construct(var body, _)     => [body],
		Node.Lookahead(_, var body)     => [body],
		Node.Call(_, var arguments)     => arguments,
		_                               => [],
	};
}
