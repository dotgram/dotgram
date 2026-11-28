using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

/// <summary>Where the value of a rule comes from (§4.1).</summary>
enum ValueSource
{
	/// <summary>The rule says it with <c>=&gt;</c>; one that says it on some alternatives only is told by <c>CheckConstruction</c>.</summary>
	Explicit,

	/// <summary>A string some of whose pieces are not their own text: the pieces the route read, joined.</summary>
	Joined,

	/// <summary>A string whose every piece is its own text, the text it matched; or a span, the bounds of what it matched.</summary>
	Extent,

	/// <summary>An array: the values of the calls of its element type, in order.</summary>
	Collected,

	/// <summary>Another type: on every route of every alternative, the one call of that type.</summary>
	Operand,

	/// <summary>The same, where an alternative without such a call is built from the names the rule writes (§7.3).</summary>
	Constructed,

	/// <summary>What the host recognizer the whole rule is hands back (§7.1).</summary>
	External,

	/// <summary>Nothing says the value, and the rule is told why.</summary>
	Refused,
}

/// <summary>
/// The value of one rule, decided once from what the rule writes and the types of what it calls, and acted on by the passes
/// that realise it.
/// </summary>
/// <param name="Source">Where the value comes from.</param>
/// <param name="Type">The type the rule has, declared or taken from the rule it names.</param>
/// <param name="Refusal">What the rule is told, for <see cref="ValueSource.Refused"/>.</param>
/// <param name="Fill">The construction the names fill, for <see cref="ValueSource.Constructed"/>.</param>
/// <param name="Ways">
/// For <see cref="ValueSource.Operand"/> and <see cref="ValueSource.Constructed"/>, per alternative as the rule offers them
/// (a left-recursive rule's seeds, then its steps): whether it hands on the one call of its type, and for a step, the value so far.
/// </param>
sealed record RuleValue(ValueSource Source, string Type, string? Refusal = null, Construction? Fill = null, IReadOnlyList<bool>? Ways = null);

/// <summary>
/// One value function for every rule (§4.1): a string is its pieces joined, a span the bounds of what it matched, an array the
/// values of the calls of its element type, any other type the one call of its type on each route or the construction its
/// names fill.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="DecideValues"/> only decides: it rewrites nothing and reports nothing, so that what a rule's value is cannot depend
/// on the order rules are rewritten in. What it decides is then realised in the grammar the machine already reads: a string that
/// is its own text is left the extent it is (the slice, as fast as ever), every other value becomes a construction on each
/// alternative, and the refusals are told.
/// </para>
/// <para>
/// One walk (<see cref="Walk"/>) goes through what a rule reads for every question asked of it here, for the decision and the
/// rewrite alike, so that the two cannot disagree about where a piece is.
/// </para>
/// </remarks>
sealed partial class GrammarNormalizer
{
	/// <summary>
	/// Whether a published rule with no type and no names is a string, its pieces joined (§4.1), or, as before, the text
	/// it matched. One with a piece that has no text to join, or that binds a text, is the text it matched either way.
	/// </summary>
	static readonly bool PublishedPlumbingJoins = false;

	/// <summary>The decision for every rule that has a type.</summary>
	readonly Dictionary<RuleSymbol, RuleValue> _ruleValues = [];

	/// <summary>What the walk does at a node: stop there, with what stands in its place and whether it is one of what is counted.</summary>
	readonly record struct WalkStep(Node Node, bool Counts);

	/// <summary>
	/// The one walk over what a rule reads: through sequences, choices, repetitions, groups, marks, bindings and names; not into
	/// a lookahead, a guard, a construction or what never runs (<c>X{0}</c>).
	/// </summary>
	/// <remarks>
	/// <paramref name="visit"/> is asked of each node first: null goes into it, a step stops there. Gives the fewest and the most
	/// counted nodes one route through the node reads, two standing for more than one, and the node with each step's replacement
	/// in place (the same node where nothing changed). <paramref name="written"/> goes into what never runs too, which reads
	/// nothing and is visited for what it writes: a name, or a call.
	/// </remarks>
	(int Least, int Most) Walk(Node node, Func<Node, WalkStep?> visit, out Node result, bool written = false)
	{
		if (DeepStack.Low)
		{
			Node deep = node;
			var  ways = DeepStack.Run(() => Walk(node, visit, out deep, written));

			result = deep;

			return ways;
		}

		result = node;

		if (visit(node) is { } step)
		{
			result = step.Node;

			return step.Counts ? (1, 1) : (0, 0);
		}

		switch (node)
		{
			case Node.Sequence(var parts):
			{
				var least = 0;
				var most  = 0;
				var built = new List<Node>(parts.Count);

				foreach (var part in parts)
				{
					var (one, many) = Walk(part, visit, out var inner, written);

					least += one;
					most  += many;

					built.Add(inner);
				}

				result = Same(built, parts) ? node : new Node.Sequence(built);

				return (Math.Min(least, 2), Math.Min(most, 2));
			}

			case Node.Choice(var alternatives):
			{
				var least = 2;
				var most  = 0;
				var built = new List<Node>(alternatives.Count);

				foreach (var alternative in alternatives)
				{
					var (one, many) = Walk(alternative, visit, out var inner, written);

					least = Math.Min(least, one);
					most  = Math.Max(most, many);

					built.Add(inner);
				}

				result = Same(built, alternatives) ? node : ((Node.Choice)node).Rebuild(built);

				return (alternatives.Count == 0 ? 0 : least, most);
			}

			// What never runs is written all the same: it is counted as if it ran once, so that a name in it fills a
			// construction as it is written.
			case Node.Repeat(var body, _, 0) when written:
				return Walk(body, visit, out _, written);

			case Node.Counted(var body, _, CountBound.Items(0)) when written:
				return Walk(body, visit, out _, written);

			case Node.Repeat(_, _, 0):
			case Node.Counted(_, _, CountBound.Items(0)):
				return (0, 0);

			case Node.Repeat(var body, var min, var max):
			{
				var ways = Passes(Walk(body, visit, out var inner, written), min > 0, max);

				result = ReferenceEquals(inner, body) ? node : Repeated(node, new Node.Repeat(inner, min, max));

				return ways;
			}

			case Node.Counted(var body, var min, var max):
			{
				var ways = Passes(Walk(body, visit, out var inner, written), min is CountBound.Items(> 0), max is CountBound.Items(var most) ? most : null);

				result = ReferenceEquals(inner, body) ? node : Placed(node, new Node.Counted(inner, min, max));

				return ways;
			}

			case Node.Atomic(var body):
			{
				var ways = Walk(body, visit, out var inner, written);

				result = ReferenceEquals(inner, body) ? node : new Node.Atomic(inner);

				return ways;
			}

			case Node.Marked(var body, var text):
			{
				var ways = Walk(body, visit, out var inner, written);

				result = ReferenceEquals(inner, body) ? node : new Node.Marked(inner, text);

				return ways;
			}

			case Node.Bind bind:
			{
				var ways = Walk(bind.Body, visit, out var inner, written);

				result = ReferenceEquals(inner, bind.Body) ? node : bind with { Body = inner };

				return ways;
			}

			case Node.Capture(var name, var body):
			{
				var ways = Walk(body, visit, out var inner, written);

				result = ReferenceEquals(inner, body) ? node : new Node.Capture(name, inner);

				return ways;
			}

			default:
				return (0, 0);
		}

		static bool Same(List<Node> built, IReadOnlyList<Node> parts)
		{
			for (var i = 0; i < parts.Count; i++)
				if (!ReferenceEquals(built[i], parts[i]))
					return false;

			return true;
		}

		// What a repetition makes of what its body reads: once when it runs at most once, and otherwise more than one however
		// few turns it takes.
		static (int Least, int Most) Passes((int Least, int Most) body, bool atLeastOnce, int? atMost)
		{
			return (atLeastOnce ? body.Least : 0, body.Most == 0 ? 0 : atMost == 1 ? body.Most : 2);
		}
	}

	/// <summary>The calls a node reads, as the walk finds them, in order; <paramref name="written"/>, those inside what never runs too.</summary>
	List<Node.Call> CallsIn(Node node, bool written = false)
	{
		var calls = new List<Node.Call>();

		Walk(node, part =>
		{
			if (part is Node.Call call)
			{
				calls.Add(call);

				return new WalkStep(part, false);
			}

			return part is Node.Lookahead or Node.Guard or Node.Condition or Node.Construct ? new WalkStep(part, false) : null;
		}, out _, written);

		return calls;
	}

	/// <summary>The type a rule has, declared, taken from another or generated for it; null for a rule that has none.</summary>
	string? ValueTypeOf(RuleSymbol rule)
	{
		return _types.TryGetValue(rule, out var type) ? type
			: _extents.TryGetValue(rule, out type) ? type
			: GeneratedNameOf(rule);
	}

	/// <summary>A rule with no type and no names: plumbing, which a string or an array reads as if it were written in place.</summary>
	bool ReadThrough(RuleSymbol rule)
	{
		return rule.Declaration is not null && ValueTypeOf(rule) is null;
	}

	/// <summary>
	/// Whether a type is certainly the string: by its keyword, or rooted in <c>global::</c>. <c>String</c> alone may be a type
	/// of the host's, which is built as any other type is.
	/// </summary>
	bool IsText(string type)
	{
		return _language.Classify(type, out var form) == KnownType.Text && form is TypeSpellingForm.Syntax or TypeSpellingForm.Global;
	}

	bool IsSpan(string type)
	{
		return _language.Classify(type, out _) == KnownType.SourceSpan;
	}

	/// <summary>What a call is to a string rule that reads it: nothing, its text, a value to join, or a piece that has none.</summary>
	enum Piece
	{
		/// <summary>Trivia: it gives nothing.</summary>
		Nothing,

		/// <summary>Its own text: a rule that builds nothing, or a string rule that is its own text.</summary>
		Text,

		/// <summary>A string that need not be its text: one built with <c>=&gt;</c>, by a host, or joined.</summary>
		Built,

		/// <summary>A character.</summary>
		Char,

		/// <summary>An untyped rule, read through.</summary>
		Through,

		/// <summary>A value that is no text: a number, a span, a host or generated type.</summary>
		Other,
	}

	/// <summary>What a call is to a string rule that reads it, given which string rules are not their own text.</summary>
	Piece PieceOf(RuleSymbol owner, Node.Call call, HashSet<RuleSymbol> joined)
	{
		if (_trivia.TryGetValue(owner, out var seam) && IsSeam(call, seam))
			return Piece.Nothing;

		var called = call.Rule;

		if (ValueTypeOf(called) is not { } type)
			return called.Declaration is null ? Piece.Text : Piece.Through;

		if (IsText(type))
			return called.Declaration is null || joined.Contains(called) ||
				_ruleValues.TryGetValue(called, out var value) && value.Source is ValueSource.Explicit or ValueSource.External or ValueSource.Joined
				? Piece.Built
				: Piece.Text;

		return _language.Classify(type, out _) == KnownType.Char ? Piece.Char : Piece.Other;
	}

	/// <summary>
	/// The string rules, and the untyped rules a string reads through, that are not their own text: a least fixpoint over their
	/// calls. A rule is its text unless it reads a built string, a character, a rule that is not its text, or a seam.
	/// </summary>
	HashSet<RuleSymbol> NotOwnText(IReadOnlyList<RuleSymbol> candidates)
	{
		var joined = new HashSet<RuleSymbol>();

		for (var changed = true; changed;)
		{
			changed = false;

			foreach (var rule in candidates)
			{
				if (joined.Contains(rule))
					continue;

				foreach (var call in CallsIn(_bodies[rule]))
				{
					var piece = PieceOf(rule, call, joined);

					if (piece is Piece.Nothing or Piece.Built or Piece.Char || piece == Piece.Through && joined.Contains(call.Rule))
					{
						joined.Add(rule);
						changed = true;

						break;
					}
				}
			}
		}

		return joined;
	}

	/// <summary>The first piece of a string rule, read through its untyped calls, that has no text to join; null where there is none.</summary>
	Node.Call? NonText(RuleSymbol rule, HashSet<RuleSymbol> joined, HashSet<RuleSymbol> seen)
	{
		// Follows the untyped rules a string reads through, a chain as long as the grammar makes it.
		if (DeepStack.Low)
			return DeepStack.Run(() => NonText(rule, joined, seen));

		if (!seen.Add(rule))
			return null;

		foreach (var call in CallsIn(_bodies[rule]))
			switch (PieceOf(rule, call, joined))
			{
				case Piece.Other:
					return call;

				case Piece.Through when NonText(call.Rule, joined, seen) is { } inner:
					return inner;
			}

		return null;
	}

	/// <summary>
	/// The value of every rule that has a type (§4.1), once every type is known and before any is realised.
	/// </summary>
	void DecideValues()
	{
		var strings = new List<RuleSymbol>();

		foreach (var rule in _rules)
		{
			if (rule.Declaration is null || !_types.TryGetValue(rule, out var type) || !_bodies.ContainsKey(rule))
				continue;

			// A '=>' anywhere is the author's: on every alternative, behind a switch, or on some only, which CheckConstruction
			// tells.
			if (Constructs(_bodies[rule]).Any())
				_ruleValues[rule] = new RuleValue(ValueSource.Explicit, type);
			else if (_bodies[rule] is Node.Call(var called, _) && _bodies.TryGetValue(called, out var external) &&
				external is Node.External { HasValue: true })
				_ruleValues[rule] = new RuleValue(ValueSource.External, type);
			else if (IsText(type))
				strings.Add(rule);
		}

		// What a string reads through is asked the same question, so that a rule written in place of its call says the same.
		var candidates = strings.Concat(_rules.Where(ReadThrough).Where(_bodies.ContainsKey)).ToList();
		var joined     = NotOwnText(candidates);

		// A published rule with no type and no names is a string: its value is its pieces joined, where they are all text.
		if (PublishedPlumbingJoins)
			foreach (var publication in _publications)
				if (ReadThrough(publication.Rule) && joined.Contains(publication.Rule) && !strings.Contains(publication.Rule) &&
					NonText(publication.Rule, joined, []) is null && !Bound(_bodies[publication.Rule]))
				{
					_types[publication.Rule] = _language.Spelling(KnownType.Text);
					strings.Add(publication.Rule);
				}

		foreach (var rule in strings)
			_ruleValues[rule] = NonText(rule, joined, []) is { } piece
				? new RuleValue(ValueSource.Refused, _types[rule], NonTextMessage(rule, piece))
				: new RuleValue(joined.Contains(rule) ? ValueSource.Joined : ValueSource.Extent, _types[rule]);

		foreach (var rule in _rules)
			if (rule.Declaration is not null && !_ruleValues.ContainsKey(rule) && _types.TryGetValue(rule, out var type) &&
				_bodies.ContainsKey(rule))
				_ruleValues[rule] = Decide(rule, type);
	}

	/// <summary>The value of a rule of a type other than a string (§4.1): a span, an array or any other.</summary>
	RuleValue Decide(RuleSymbol rule, string type)
	{
		var alternatives = ValueAlternatives(_bodies[rule]);
		var recursive    = alternatives.Any(alternative => Tail(alternative, rule) is not null);

		if (IsSpan(type))
			return new RuleValue(ValueSource.Extent, type);

		// A left-recursive array is its seed and its steps, and the value so far is the elements so far: a call of the array
		// type, which gives its elements in place, as any other does.
		if (_language.ElementOf(type) is { } element)
		{
			// A call of the array's type whose element can hold that array too: one element, or its elements in place.
			if (CallsIn(_bodies[rule]).FirstOrDefault(call => Fits(call, element) && Spreads(call.Rule, type)) is { } both)
				return new RuleValue(ValueSource.Refused, type,
					$"'{rule.Name}' declares its result as '{type}', and '{both.Rule.Name}' is '{type}' too, which a '{element}' can " +
					"hold: it is one element, or its elements in place, and nothing says which (§4.1). Build the array with '=>', " +
					$"as in 'x: {both.Rule.Name} & … => @(new {element}[] {{ x, … }})'.");

			// What is written decides, so that one inside what never runs, `X{0}`, or inside a lookahead gives an array of none.
			return NodeWalk.Descendants(_bodies[rule]).OfType<Node.Call>().Any(call => call.Rule != rule && Collects(call, element, type, []))
				? new RuleValue(ValueSource.Collected, type)
				: new RuleValue(ValueSource.Refused, type, SequenceMessage(rule, element));
		}

		// Rule O is the rule's: the call of the type that every route of it reads, or else the construction its names fill,
		// on every alternative. A left-recursive rule is its seeds, decided so, and its steps: a step reads the value so far
		// first, its one call of the type, unless the author named it or the step reads another.
		var seeds  = alternatives.Where(alternative => Tail(alternative, rule) is null).ToList();
		var handed = seeds.All(seed => Operands(seed, type) == (1, 1));
		var ways     = new List<bool>();
		var unsaid   = new List<string>();
		var names    = new HashSet<string>(StringComparer.Ordinal);
		var building = new List<(Node Alternative, string Which)>();

		for (var i = 0; i < alternatives.Count; i++)
		{
			var alternative = alternatives[i];
			var tail        = Tail(alternative, rule);
			var which       = alternatives.Count == 1 ? "its only alternative" : $"alternative {i + 1} of its {alternatives.Count}";

			var (least, most) = tail is var (step, accumulator)
				? accumulator.Length == 0 ? Shift(Operands(step, type)) : (2, 2)
				: Operands(alternative, type);

			if (tail is null ? handed : (least, most) == (1, 1))
			{
				ways.Add(true);

				continue;
			}

			ways.Add(false);

			var named = NamesIn(alternative);

			names.UnionWith(named);

			if (named.Count > 0)
			{
				building.Add((alternative, which));

				continue;
			}

			unsaid.Add(
				tail is not null ? $"step {i + 1} of it adds a call of '{type}' to the value so far, or names it, and names nothing to build from"
				: most == 0 ? $"{which} reads no call of '{type}'"
				: least == 0 && most == 1 ? $"{which} may read none"
				: (least, most) == (1, 1) ? $"{which} reads one, and another alternative does not"
				: $"{which} may read more than one");
		}

		if (ways.All(static one => one))
			return new RuleValue(ValueSource.Operand, type, Ways: ways);

		var fill = names.Count > 0 ? Fill(type, [.. names]) : null;

		// The names fill the construction on every route of every alternative it builds, or the rule says none: a route that
		// writes none of the names the construction takes, `(name: X)?` or a name the type has no use for, has nothing to
		// build from.
		if (fill is not null)
		{
			var taken = new HashSet<string>(fill switch
			{
				Construction.Constructor(var order)    => order,
				Construction.Initializer(var bindings) => bindings.Select(static one => one.Capture),
				_                                      => [],
			}, StringComparer.Ordinal);

			foreach (var (alternative, which) in building)
				if (Walk(alternative, part => part switch
					{
						Node.Capture(var name, _) when taken.Contains(name) => new WalkStep(part, true),
						Node.Lookahead or Node.Construct => new WalkStep(part, false),
						_ => null,
					}, out _, written: true).Least == 0)
					unsaid.Add($"a route through {which} names none of them");
		}

		if (fill is not null && unsaid.Count == 0)
			return new RuleValue(ValueSource.Constructed, type, Fill: fill, Ways: ways);

		return new RuleValue(ValueSource.Refused, type, ValueMessage(rule, type, recursive, unsaid, names.Count > 0, names));

		// The value so far is the step's one call.
		static (int Least, int Most) Shift((int Least, int Most) step)
		{
			return (Math.Min(step.Least + 1, 2), Math.Min(step.Most + 1, 2));
		}
	}

	/// <summary>The fewest and the most unnamed calls of a type one route through a node reads (§4.1 rule O).</summary>
	(int Least, int Most) Operands(Node node, string type)
	{
		return Walk(node, part => OperandStep(part, type, null), out _);
	}

	/// <summary>
	/// What the walk does at a node of a rule of another type: a call of the type is the operand (made a capture of
	/// <paramref name="capture"/> where one is given), and a name written on a call makes it the author's.
	/// </summary>
	WalkStep? OperandStep(Node node, string type, string? capture)
	{
		return node switch
		{
			Node.Call when IsOperand(node, type) => new WalkStep(capture is null ? node : new Node.Capture(capture, node), true),
			Node.Capture(_, var inner) when NamesACall(inner) => new WalkStep(node, false),
			Node.Lookahead or Node.Guard or Node.Condition or Node.Construct => new WalkStep(node, false),
			_ => null,
		};
	}

	/// <summary>Whether a node is a call of a rule of the type, which an array rule is not.</summary>
	bool IsOperand(Node node, string type)
	{
		return node is Node.Call(var called, _) && ValueTypeOf(called) is { } held && _language.ElementOf(held) is null &&
			(_namedTypes.ContainsKey(type) ? held == type : _language.Fits(held, type));
	}

	/// <summary>A name written directly on a call: <c>x: R</c>, <c>x: R?</c>, <c>x: R+</c>.</summary>
	static bool NamesACall(Node inner)
	{
		return inner switch
		{
			Node.Call                       => true,
			Node.Repeat(var body, _, _)     => NamesACall(body),
			Node.Counted(var body, _, _)    => NamesACall(body),
			Node.Atomic(var body)           => NamesACall(body),
			Node.Sequence { Nodes.Count: 1 } one => NamesACall(one.Nodes[0]),
			_                               => false,
		};
	}

	/// <summary>The names an alternative writes, outside lookaheads, in order: one inside what never runs is written all the same.</summary>
	List<string> NamesIn(Node alternative)
	{
		var names = new List<string>();

		Walk(alternative, part =>
		{
			if (part is Node.Capture(var name, _) && !names.Contains(name))
				names.Add(name);

			return part is Node.Lookahead or Node.Construct ? new WalkStep(part, false) : null;
		}, out _, written: true);

		return names;
	}

	/// <summary>
	/// Whether a call gives elements of an array of <paramref name="element"/>: its value is one, it is an array of the same
	/// type (whose elements it gives in place), or it reads through to one.
	/// </summary>
	bool Collects(Node.Call call, string element, string array, HashSet<RuleSymbol> seen)
	{
		// Follows the untyped rules an array reads through, a chain as long as the grammar makes it.
		if (DeepStack.Low)
			return DeepStack.Run(() => Collects(call, element, array, seen));

		return Fits(call, element) || Spreads(call.Rule, array) ||
			ReadThrough(call.Rule) && seen.Add(call.Rule) && CallsIn(_bodies[call.Rule]).Any(inner => Collects(inner, element, array, seen));
	}

	/// <summary>Whether a rule is an array of the type, whose elements a call of it gives in place (§4.1 rule A).</summary>
	bool Spreads(RuleSymbol called, string array)
	{
		return _types.TryGetValue(called, out var type) && type == array;
	}

	/// <summary>
	/// §7.3: what the names fill of a type, the construction <see cref="ValueSource.Constructed"/> is built by; null where they
	/// fill nothing, or two constructors equally.
	/// </summary>
	/// <remarks>
	/// The longest constructor every parameter of which is named; else the settable properties the names set, every required
	/// one named. Names match parameters and properties without regard to case. Where the resolver cannot see the host, a
	/// constructor of all the names is taken, which the build then asks the host about; not for a type of C#'s own.
	/// </remarks>
	Construction? Fill(string type, IReadOnlyList<string> names)
	{
		if (names.Count == 0)
			return null;

		var chosen    = (IReadOnlyList<string>?)null;
		var chosenFor = (IReadOnlyList<MethodParameter>?)null;
		var tied      = false;

		if (TryConstructors(type, out var constructors))
			foreach (var constructor in constructors)
			{
				if (Covered(constructor, names) is not { } order)
					continue;

				if (chosen is null || order.Count > chosen.Count)
					(chosen, chosenFor, tied) = (order, constructor, false);
				else if (order.Count == chosen.Count)
					tied = true;
			}

		if (tied)
			return null;

		if (chosen is not null)
			return new Construction.Constructor(chosen)
			{
				Spans = [.. chosen.Where((_, at) => _language.TakesCharacterView(chosenFor![at].Type))],
			};

		if (Settable(type, names, out var spans) is { } written)
			return new Construction.Initializer(written) { Spans = spans };

		return _language.HostUnseen && !BuiltIn(type) ? new Construction.Constructor([.. names]) : null;
	}

	/// <summary>
	/// What is decided before the left-recursive rules are rewritten: a string that is its own text and a span are left the
	/// extent they matched, and a rule of another type is given its operand or its construction on each alternative, a step its
	/// value so far, so that the rewrite folds constructions as it does for any rule that writes them.
	/// </summary>
	void RealiseBeforeRecursion()
	{
		foreach (var rule in _rules.ToList())
		{
			if (!_ruleValues.TryGetValue(rule, out var value))
				continue;

			// A name inside a lookahead is refused in every rule (GRAM4006): what a lookahead records is dropped.
			var body = _bodies[rule];

			switch (value.Source)
			{
				// Names change nothing in a string or a span: where nothing reads them they are dropped, and where a guard or a
				// switch does they stay, and the extent is built from the text the rule matched.
				case ValueSource.Extent when IsText(value.Type):
					_types.Remove(rule);
					_extents[rule] = value.Type;

					// A name a guard reads inside a lookahead is a capture of the rule's too.
					if (!NodeWalk.Descendants(body).Any(static one => one is Node.Capture))
						break;

					if (ReadsValues(body))
					{
						_extents.Remove(rule);
						_types[rule]  = value.Type;
						_bodies[rule] = Extended(UnnamedUnread(body), "parserText.ToString()");
					}
					else
					{
						_bodies[rule] = Unnamed(body);
					}

					break;

				// A span is the bounds of what it matched: a name nothing reads is dropped, and a name a guard reads stays.
				case ValueSource.Extent:
					_bodies[rule] = Extended(ReadsValues(body) ? UnnamedUnread(body) : Unnamed(body), "parserSpan");
					break;

				// Names change nothing in a string or an array either: the ones nothing reads are dropped.
				case ValueSource.Joined:
				case ValueSource.Collected:
					_bodies[rule] = ReadsValues(body) ? UnnamedUnread(body) : Unnamed(body);
					break;

				case ValueSource.Operand:
				case ValueSource.Constructed:
					_bodies[rule] = Operated(rule, value);
					break;

				case ValueSource.Refused:
					_unsaid.Add(rule);
					Report(UnbuiltConstruction, value.Refusal!, rule.Declaration!.At);
					break;
			}
		}
	}

	/// <summary>
	/// A rule of another type with each alternative given its value: the one call of the type, made a capture and handed back;
	/// for a step, the value so far; else the construction the names fill.
	/// </summary>
	Node Operated(RuleSymbol rule, RuleValue value)
	{
		var alternatives = ValueAlternatives(_bodies[rule]);
		var reserved     = ReservedNames(rule);
		var rewritten    = new List<Node>(alternatives.Count);

		// A name is read by a guard, a condition, a mark or a switch, or by the construction it fills; one nothing reads is
		// dropped, so that it is held to no type (GRAM4007 asks only of a name that is read).
		var guarded = NamesReadIn(_bodies[rule], _bodies[rule]);
		var filled  = new HashSet<string>(guarded, StringComparer.Ordinal);

		filled.UnionWith(value.Fill switch
		{
			Construction.Constructor(var order)    => order,
			Construction.Initializer(var bindings) => bindings.Select(static one => one.Capture),
			_                                      => [],
		});

		for (var i = 0; i < alternatives.Count; i++)
		{
			var alternative = alternatives[i];

			if (!value.Ways![i])
			{
				rewritten.Add(new Node.Construct(Tail(alternative, rule) is null ? Unread(alternative, filled) : alternative, value.Fill!));

				continue;
			}

			var name = Unreserved("item" + i, reserved);

			// The step's leading call of the rule itself, which the rewrite turns into the value so far under this name.
			if (Tail(alternative, rule) is not null && alternative is Node.Sequence(var parts))
			{
				rewritten.Add(new Node.Construct(
					new Node.Sequence([new Node.Capture(name, parts[0]), .. parts.Skip(1)]),
					new Construction.Expression(name)));

				continue;
			}

			// One capture per type the operand has on some route: a member holds one type.
			var held = new Dictionary<string, string>(StringComparer.Ordinal);

			Walk(alternative, part => OperandStep(part, value.Type, part is Node.Call(var called, _) && Held(called) is { } kind
				? held.TryGetValue(kind, out var known) ? known : held[kind] = held.Count == 0 ? name : Unreserved(name + "_" + held.Count, reserved)
				: name), out var taken);

			// A name the author wrote on a call is not the operand, whether or not it is read: dropped only once the operand
			// is found.
			rewritten.Add(new Node.Construct(Unread(taken, [.. guarded, .. held.Values]), new Construction.Operand([.. held.Values.DefaultIfEmpty(name)])));
		}

		return Rebuilt(_bodies[rule], rewritten);

		// What a member holds the operand by: a declared type, shared by every rule that declares it, or the rule itself.
		string? Held(RuleSymbol called)
		{
			return _types.TryGetValue(called, out var type) ? type : ValueTypeOf(called) is null ? null : "rule " + called.Name;
		}
	}

	/// <summary>
	/// However the grammar wrote it, the span the emitter names is the one nested in the host class: said once every rule has
	/// been decided, so that what a rule takes the type of is compared with it as written.
	/// </summary>
	void SpellSpans()
	{
		foreach (var rule in _rules)
			if (_types.TryGetValue(rule, out var type) && IsSpan(type))
				_types[rule] = _language.Spelling(KnownType.SourceSpan);
	}

	/// <summary>The node with every name taken off but the ones given.</summary>
	Node Unread(Node node, HashSet<string> kept)
	{
		_namesRead = kept;

		try
		{
			return Unnamed(node);
		}
		finally
		{
			_namesRead = null;
		}
	}

	/// <summary>The node with the names nothing in it reads taken off: a guard's, a condition's, a mark's or a switch's text reads one.</summary>
	/// <param name="scope">What the names are read in: the whole rule, where only a part of it is unnamed.</param>
	Node UnnamedUnread(Node node, Node? scope = null)
	{
		_namesRead = NamesReadIn(scope ?? node, node);

		try
		{
			return Unnamed(node);
		}
		finally
		{
			_namesRead = null;
		}
	}

	/// <summary>The names of captures in <paramref name="node"/> that a guard, a condition, a mark or a switch in <paramref name="scope"/> reads.</summary>
	HashSet<string> NamesReadIn(Node scope, Node node)
	{
		var read  = new HashSet<string>(StringComparer.Ordinal);
		var texts = NodeWalk.Descendants(scope).SelectMany(static one => one switch
		{
			Node.Guard(var text, _)                       => [text],
			Node.Marked(_, var text)                      => [text],
			Node.Choice { Selection: { } selection }      => [selection.Selector.Text],
			Node.Condition(var test, _)                   => Runs(test),
			_                                             => (IEnumerable<string>)[],
		}).ToList();

		foreach (var capture in NodeWalk.Descendants(node).OfType<Node.Capture>())
			if (texts.Any(text => _language.NamesUsed(text) is { } used
				? used.Contains(capture.Name)
				: System.Text.RegularExpressions.Regex.IsMatch(text, @"\b" + System.Text.RegularExpressions.Regex.Escape(capture.Name) + @"\b")))
				read.Add(capture.Name);

		return read;
	}

	/// <summary>The C# a condition runs.</summary>
	static IEnumerable<string> Runs(Test test)
	{
		return test switch
		{
			Test.Runs(var text, _) => [text],
			Test.All(var items)    => items.SelectMany(Runs),
			Test.Any(var items)    => items.SelectMany(Runs),
			_                      => [],
		};
	}

	/// <summary>A body with its alternatives replaced, a switch kept a switch.</summary>
	static Node Rebuilt(Node body, List<Node> alternatives)
	{
		return body is Node.Choice { Selection: not null } selected
			? selected.Rebuild(alternatives)
			: alternatives.Count == 1 ? alternatives[0] : new Node.Choice(alternatives);
	}

	/// <summary>
	/// Every string that is not its own text, given a <c>=&gt;</c> on each alternative that joins what it read
	/// (<see cref="Construction.Join"/>).
	/// </summary>
	/// <remarks>
	/// <para>
	/// A piece that is its own text is captured as text, or written as the literal it is; a string or a character is captured
	/// as its value; trivia, lookaheads and guards are left out of every capture. What can read pieces of both kinds in more
	/// than one way — a choice, a repetition, a group read more than once — is given a rule of its own, which joins its own
	/// pieces; a repetition of it gives each turn's value. So is an untyped rule a string reads through, as if it were written
	/// in place, under a name of its own.
	/// </para>
	/// <para>
	/// After the left-recursive rules are rewritten: a string that calls itself leftmost is its seed and its steps
	/// (<c>Seed (Step)*</c>), and is joined like any other repetition, in one string for the whole match.
	/// </para>
	/// </remarks>
	void RealiseJoins()
	{
		for (var i = 0; i < _rules.Count; i++)
		{
			var rule = _rules[i];

			if (!_ruleValues.TryGetValue(rule, out var value) || value.Source != ValueSource.Joined)
				continue;

			// One set of names for the whole rule: a capture of one alternative is a member of the rule, and two of one name
			// in two alternatives would be one member holding two kinds of thing.
			var reserved = ReservedNames(rule);

			_bodies[rule] = Rebuilt(_bodies[rule], [.. ValueAlternatives(_bodies[rule]).Select(one => Joined(rule, one, reserved))]);
		}
	}

	/// <summary>The string rules made to read through an untyped rule, by the rule they read.</summary>
	readonly Dictionary<RuleSymbol, RuleSymbol> _readThrough = [];

	/// <summary>The rules made to join a part of another that can be read in more than one way.</summary>
	readonly HashSet<RuleSymbol> _joinParts = [];

	/// <summary>One alternative of a string that is not its own text, given the construction that joins it.</summary>
	Node Joined(RuleSymbol rule, Node alternative, HashSet<string> reserved)
	{
		var parts    = new List<JoinPart>();
		var empty    = new HashSet<RuleSymbol>();

		var moved       = false;
		var conditional = 0;
		var read        = NamesReadIn(_bodies[rule], alternative);
		var unreachable = (string?)null;

		Walk(alternative, Visit, out var rewritten);

		// A name a guard reads where it stands cannot go with a part to a rule of its own: told, and the alternative is left
		// its text, so that nothing after this reads a rule half rewritten.
		if (unreachable is { } name)
		{
			if (_unsaid.Add(rule))
				Report(
					UnbuiltConstruction,
					$"'{rule.Name}' is a string joined from its pieces, and the name '{name}' a guard reads stands in a repetition " +
					"whose turns are read in more than one way, which is joined apart from the guard. Build the value with '=>'.",
					rule.Declaration!.At);

			return new Node.Construct(alternative, new Construction.Expression("parserText.ToString()"));
		}

		// A binding is its rule's, so a part that is given a rule of its own cannot hold one: told, and the alternative is
		// left its text, so that nothing after this reads a rule half rewritten.
		if (moved && Bound(alternative))
		{
			if (_unsaid.Add(rule))
				Report(
					UnbuiltConstruction,
					$"'{rule.Name}' is a string joined from its pieces, and a part of it read more than once or in more than one " +
					"way holds a back-reference or the text it binds: the part cannot be joined apart from the binding. Build " +
					"the value with '=>'.",
					rule.Declaration!.At);

			return new Node.Construct(alternative, new Construction.Expression("parserText.ToString()"));
		}

		return new Node.Construct(rewritten, new Construction.Join(parts));

		WalkStep? Visit(Node node)
		{
			switch (node)
			{
				// A group is gone into: what it holds is joined in place, and a brace keeps its commit around the captures.
				case Node.Sequence or Node.Capture or Node.Marked or Node.Atomic:
					return null;

				// A binding is a call like any other, and the back-reference a piece that matches text: it gives its text.
				case Node.Bind:
					return null;

				// What reads nothing, a call of `eof` as a lookahead, gives nothing.
				case Node.Call(var called, _) when _bodies.TryGetValue(called, out var read) && ReadsNothing(read):
				case Node.Call call when PieceOf(rule, call, empty) == Piece.Nothing:
				case Node.Lookahead or Node.Guard or Node.Condition or Node.Behind or Node.Glue or Node.Empty or Node.Reading:
				case Node.Repeat(_, _, 0):
				case Node.Counted(_, _, CountBound.Items(0)):
					return new WalkStep(node, false);

				// Fixed text where every route reads it; inside a choice or an option it is captured like any piece, so that
				// it is joined only on the route that read it.
				case Node.Literal { IgnoreCase: false } literal when !Joins(node) && conditional == 0:
					parts.Add(JoinPart.Fixed(literal.Text));

					return new WalkStep(node, false);

				case var _ when !Joins(node):
					return Held(node);

				// A choice and an option are read one way on each route: what each branch holds is captured where it stands,
				// and what the route did not read is empty. Nothing moves to a rule of its own, so a name a guard reads stays
				// where the guard can read it.
				case Node.Choice choice:
				{
					conditional++;

					var alternatives = new List<Node>(choice.Nodes.Count);

					foreach (var one in choice.Nodes)
					{
						Walk(one, Visit, out var inner);

						alternatives.Add(inner);
					}

					conditional--;

					return new WalkStep(choice.Rebuild(alternatives), false);
				}

				case Node.Repeat(var body, var min, 1):
				{
					conditional++;

					Walk(body, Visit, out var inner);

					conditional--;

					return new WalkStep(ReferenceEquals(inner, body) ? node : Repeated(node, new Node.Repeat(inner, min, 1)), false);
				}

				// A call of a string or a char is its value; one of an untyped rule is that rule read in place.
				case Node.Call(var called, var arguments):
					return Held(ReadThrough(called) ? new Node.Call(ThroughRule(rule, called), arguments) : node);

				// A repetition whose turn holds a name a guard reads is joined turn by turn where it stands, so that the name
				// stays where the guard reads it: each piece of the turn is captured once per turn.
				case Node.Repeat(Node.Sequence(var pieces), var min, var max) when Kept(pieces) && pieces.All(InTurn):
				{
					var turn    = new List<JoinPart>();
					var rebuilt = new List<Node>(pieces.Count);

					foreach (var piece in pieces)
						switch (piece)
						{
							case Node.Literal { IgnoreCase: false } literal:
								turn.Add(JoinPart.Fixed(literal.Text));
								rebuilt.Add(piece);
								break;

							// A name on a call that hands its text on keeps that text, and the turn joins the string the call
							// is read as, under a capture of its own.
							case Node.Capture(var named, Node.Call call) when HandsText(call):
							{
								var name = Unreserved("item" + reserved.Count, reserved);

								turn.Add(JoinPart.Held(name));
								rebuilt.Add(new Node.Capture(named, new Node.Capture(name, Valued(call))));
								_listed.Add((rule, name));
								break;
							}

							case Node.Capture(var named, Node.Call call):
								turn.Add(JoinPart.Held(named));
								rebuilt.Add(new Node.Capture(named, Valued(call)));
								break;

							case Node.Call call when !Silent(call):
							{
								var name = Unreserved("item" + reserved.Count, reserved);

								turn.Add(JoinPart.Held(name));
								rebuilt.Add(new Node.Capture(name, Valued(call)));
								break;
							}

							default:
								rebuilt.Add(piece);
								break;
						}

					parts.Add(JoinPart.Turns(turn));

					return new WalkStep(Repeated(node, new Node.Repeat(new Node.Sequence(rebuilt), min, max)), false);
				}

				// A repetition whose turns hold a name a guard reads and are not read one way each is joined where it stands,
				// its pieces gathered in the order they were read.
				case Node.Repeat(var repeated, _, _) when repeated is not Node.Call && HoldsRead(repeated):
					return Gathered(node);

				case Node.Counted(var counted, _, _) when counted is not Node.Call && HoldsRead(counted):
					return Gathered(node);

				// A repetition of one call gives the value of each turn; of more, each turn is joined by a rule of its own.
				case Node.Repeat(var body, var min, var max):
					return Held(Repeated(node, new Node.Repeat(Turn(body), min, max)), inside: true);

				case Node.Counted(var body, var min, var max):
					return Held(Placed(node, new Node.Counted(Turn(body), min, max)), inside: true);

				default:
					moved = true;

					return Held(Part(rule, node));
			}

			// The node as a capture of the alternative's own, which is the next part.
			WalkStep Held(Node held, bool inside = false)
			{
				var name = Unreserved("item" + reserved.Count, reserved);

				parts.Add(JoinPart.Held(name));

				if (!inside)
					return new WalkStep(new Node.Capture(name, held), false);

				// A capture binds tighter than a quantifier: inside the repetition, it holds each turn.
				return new WalkStep(held switch
				{
					Node.Repeat repeat   => Repeated(held, repeat with { Body = new Node.Capture(name, repeat.Body) }),
					Node.Counted counted => Placed(held, counted with { Body = new Node.Capture(name, counted.Body) }),
					_                    => new Node.Capture(name, held),
				}, false);
			}

			// Whether a node holds a name a guard of the rule reads.
			bool HoldsRead(Node body)
			{
				return NodeWalk.Descendants(body).OfType<Node.Capture>().Any(capture => read.Contains(capture.Name));
			}

			// A repetition joined where it stands: every piece it reads is made a string captured in its place, and the pieces
			// are gathered into one sequence in the order they were read, which is one part of the join. Nothing moves to a
			// rule of its own, and a name the author wrote holds what it held, where the guard reads it: one on what hands its
			// text on is the text of the string it is made a name of, one on a built string is that string.
			WalkStep Gathered(Node repeated)
			{
				var group = new List<string>();
				var made  = new List<string>();
				var first = (string?)null;

				Walk(repeated, Gather, out var rewritten);

				if (group.Count > 0)
				{
					parts.Add(JoinPart.Held(first ?? group[0]));

					if (first is not null)
						(_gatheredJoins.TryGetValue(rule, out var groups) ? groups : _gatheredJoins[rule] = []).Add((first, group, made));
				}

				return new WalkStep(rewritten, false);

				WalkStep? Gather(Node node)
				{
					switch (node)
					{
						case Node.Call(var called, _) when _bodies.TryGetValue(called, out var reads) && ReadsNothing(reads):
						case Node.Call call when Silent(call):
						case Node.Lookahead or Node.Guard or Node.Condition or Node.Behind or Node.Glue or Node.Empty or Node.Reading:
						case Node.Repeat(_, _, 0):
						case Node.Counted(_, _, CountBound.Items(0)):
							return new WalkStep(node, false);

						case Node.Sequence or Node.Choice or Node.Repeat or Node.Counted or Node.Marked or Node.Atomic or Node.Bind:
							return null;

						// A terminal is the text it matched.
						case var _ when node is not (Node.Call or Node.Same or Node.Capture) && !node.Children.Any():
							return new WalkStep(Taken(new Node.Call(MatchedRule(rule, node), [])), false);

						case Node.Call call when PieceOf(rule, call, empty) is Piece.Text or Piece.Built or Piece.Char or Piece.Through:
							return new WalkStep(Taken(Stringed(call)), false);

						// A name on a call that hands its text on holds that text: it is made a name of the string the call is
						// read as. A name on a built string is that string, one of the gathered pieces as it stands.
						case Node.Capture(var named, Node.Call call) when HandsText(call):
							return new WalkStep(new Node.Capture(named, Taken(Stringed(call))), false);

						case Node.Capture(var named, Node.Call call) when PieceOf(rule, call, empty) == Piece.Built:
							group.Add(named);

							return new WalkStep(node, false);

						// A name on a group holds its text; what the group reads is gathered inside it.
						case Node.Capture(_, var held) when held is not Node.Call:
							return null;

						default:
							unreachable ??= read.First(HoldsReadName);

							return new WalkStep(node, false);
					}
				}

				// A piece of the join, as a capture of its own gathered with the others.
				Node Taken(Node.Call call)
				{
					var name = Unreserved("item" + reserved.Count, reserved);

					first ??= name;
					group.Add(name);
					made.Add(name);
					_listed.Add((rule, name));

					return new Node.Capture(name, call);
				}

				bool HoldsReadName(string name)
				{
					return NodeWalk.Descendants(repeated).OfType<Node.Capture>().Any(capture => capture.Name == name);
				}
			}

			// Whether a name on a call holds its text rather than a value of it.
			bool HandsText(Node.Call call)
			{
				return PieceOf(rule, call, empty) switch
				{
					Piece.Text    => true,
					Piece.Through => !BuildsValue(call.Rule),
					_             => false,
				};
			}

			// A call as a string: what it is where it is one, the string a character makes, the text or the string read
			// through otherwise.
			Node.Call Stringed(Node.Call call)
			{
				return PieceOf(rule, call, empty) switch
				{
					Piece.Built => call,
					Piece.Char  => new Node.Call(CharRule(call.Rule), call.Arguments),
					_           => (Node.Call)Valued(call),
				};
			}

			// Whether a turn holds a name a guard of the rule reads, one turn at a time.
			bool Kept(IReadOnlyList<Node> pieces)
			{
				return pieces.Any(piece => piece is Node.Capture(var named, _) && read.Contains(named)) &&
					pieces.Any(piece => piece is Node.Capture or Node.Call && !(piece is Node.Call call && Silent(call)));
			}

			// What a turn joined where it stands may hold: fixed text, a call, a name on a call, and what gives nothing.
			bool InTurn(Node piece)
			{
				return piece is Node.Literal { IgnoreCase: false } or Node.Call or Node.Capture(_, Node.Call) or Node.Lookahead or Node.Guard or
					Node.Condition or Node.Behind or Node.Glue or Node.Empty or Node.Reading;
			}

			// A call that gives nothing: trivia, or what reads nothing.
			bool Silent(Node.Call call)
			{
				return PieceOf(rule, call, empty) == Piece.Nothing || _bodies.TryGetValue(call.Rule, out var called) && ReadsNothing(called);
			}

			// A call as a value of its own: a string that is its own text through the rule that hands its text on, an
			// untyped rule read in place.
			Node Valued(Node.Call call)
			{
				return ReadThrough(call.Rule) ? new Node.Call(ThroughRule(rule, call.Rule), call.Arguments)
					: _extents.ContainsKey(call.Rule) || ValueTypeOf(call.Rule) is null ? new Node.Call(TextRule(call.Rule, rule), call.Arguments)
					: call;
			}

			// What one turn reads: the call it is, or a rule of its own that joins it.
			Node Turn(Node body)
			{
				if (body is Node.Call(var called, var arguments))
					return ReadThrough(called) ? new Node.Call(ThroughRule(rule, called), arguments) : body;

				moved = true;

				unreachable ??= NodeWalk.Descendants(body).OfType<Node.Capture>().FirstOrDefault(capture => read.Contains(capture.Name))?.Name;

				return Part(rule, body);
			}
		}

		// Whether a node holds a piece that is not its own text, read through what it calls.
		bool Joins(Node node)
		{
			return CallsIn(node).Any(call => PieceOf(rule, call, empty) switch
			{
				Piece.Nothing or Piece.Built or Piece.Char => true,
				Piece.Through => _ruleValues.TryGetValue(call.Rule, out var through) ? through.Source == ValueSource.Joined : JoinedThrough(call.Rule),
				_ => false,
			});
		}
	}

	/// <summary>Whether an untyped rule a string reads through holds a piece that is not its text, as the decision found.</summary>
	bool JoinedThrough(RuleSymbol rule)
	{
		_joinedThrough ??= NotOwnText([.. _rules.Where(ReadThrough).Where(_bodies.ContainsKey)]);

		return _joinedThrough.Contains(rule);
	}

	HashSet<RuleSymbol>? _joinedThrough;

	/// <summary>The string rule that reads an untyped rule in place, made once per rule it reads.</summary>
	RuleSymbol ThroughRule(RuleSymbol owner, RuleSymbol read)
	{
		if (_readThrough.TryGetValue(read, out var made))
			return made;

		made = Synthetic(read, read.Name + "_Text", _bodies[read], _language.Spelling(KnownType.Text), ValueSource.Joined);

		_readThrough[read] = made;

		return made;
	}

	/// <summary>A rule of its own for a part of a string that is joined from more than one reading.</summary>
	RuleSymbol PartRule(RuleSymbol owner, Node node)
	{
		var made = Synthetic(owner, owner.Name + "_Part" + (_joinParts.Count + 1), node, _language.Spelling(KnownType.Text), ValueSource.Joined);

		_joinParts.Add(made);

		return made;
	}

	/// <summary>
	/// The pieces of a repetition a string joins where it stands, by rule: the member that gathers them, named by the first
	/// of them, and the captures it gathers in the order they were read.
	/// </summary>
	readonly Dictionary<RuleSymbol, List<(string Name, List<string> Captures, List<string> Made)>> _gatheredJoins = [];

	/// <summary>The rules made to hand on the text a terminal matched as a string, by the rule and the terminal as written.</summary>
	readonly Dictionary<(RuleSymbol, string), RuleSymbol> _matchedTexts = [];

	/// <summary>The rules made to hand on a character as a string, by the rule that builds it.</summary>
	readonly Dictionary<RuleSymbol, RuleSymbol> _charTexts = [];

	/// <summary>A string rule that reads a terminal and is the text it matched, for a piece gathered with others.</summary>
	RuleSymbol MatchedRule(RuleSymbol owner, Node terminal)
	{
		var key = (owner, terminal.ToString());

		if (!_matchedTexts.TryGetValue(key, out var made))
			_matchedTexts[key] = made = Synthetic(
				owner, owner.Name + "_Matched", new Node.Construct(terminal, new Construction.Expression("parserText.ToString()")),
				_language.Spelling(KnownType.Text), ValueSource.Explicit);

		return made;
	}

	/// <summary>A string rule that reads a character's rule and is that character, for a piece gathered with strings.</summary>
	RuleSymbol CharRule(RuleSymbol character)
	{
		if (!_charTexts.TryGetValue(character, out var made))
			_charTexts[character] = made = Synthetic(
				character, character.Name + "_String",
				new Node.Construct(new Node.Capture("character", new Node.Call(character, [])), new Construction.Expression("character.ToString()")),
				_language.Spelling(KnownType.Text), ValueSource.Explicit);

		return made;
	}

	Node Part(RuleSymbol owner, Node node)
	{
		return new Node.Call(PartRule(owner, node), []);
	}

	/// <summary>Whether a node binds a text or reads one again: <c>$q:</c>, <c>$q</c>, <c>#q</c>.</summary>
	static bool Bound(Node node)
	{
		return NodeWalk.Descendants(node).Any(static one =>
			one is Node.Bind or Node.Same || one is Node.Counted(_, var min, var max) && (min is CountBound.Length || max is CountBound.Length));
	}

	/// <summary>
	/// A rule the compiler makes for a part of another, of the type and with the value given; added where the rules are being
	/// realised, so that it is realised in turn.
	/// </summary>
	RuleSymbol Synthetic(RuleSymbol from, string stem, Node body, string type, ValueSource source)
	{
		var name = stem;

		while (_rules.Exists(rule => rule.Namespace == from.Namespace && rule.Name == name))
			name += "_";

		// The declaration it was made from, under that rule's name, so that it is named what that rule is named wherever a
		// name is told: a refusal, a recovery, a trace.
		var declaration = from.Declaration! with
		{
			Params = [],
			Type   = new TypeRef(true, type, false, from.Declaration!.At),
			OnFail = null,
		};
		var made = new RuleSymbol(name, from.Namespace, declaration);

		_rules.Add(made);
		_bodies[made]   = body;
		_types[made]    = type;
		_nullable[made] = FirstSets.Nullable(body, _ruleIsNullable);
		_ruleValues[made]   = new RuleValue(source, type);

		if (_trivia.TryGetValue(from, out var trivia))
			_trivia[made] = trivia;

		return made;
	}

	/// <summary>A string rule that reads a piece of another type: the piece has no text to join.</summary>
	string NonTextMessage(RuleSymbol rule, Node.Call piece)
	{
		var type = ValueTypeOf(piece.Rule);

		return $"'{rule.Name}' is a string, which is the join of its pieces, and '{piece.Rule.Name}' is " +
			(type is null ? "a rule whose value is no text" : $"'{type}'") +
			", which has no text to join (§4.1). Build the value with '=>', as in '<' & n: Num & '>' => @(\"<\" + n + \">\").";
	}

	/// <summary>An array none of whose calls gives an element.</summary>
	string SequenceMessage(RuleSymbol rule, string element)
	{
		return $"'{rule.Name}' declares its result as a sequence of '{element}', and no call it reads gives one: every part " +
			$"either has no value or a value that is not a '{element}' (§4.1). Call a rule of that type, or build the array with '=>'.";
	}

	/// <summary>A rule of another type whose value nothing says, with the alternatives that leave it unsaid.</summary>
	string ValueMessage(RuleSymbol rule, string type, bool recursive, List<string> unsaid, bool named, HashSet<string> names)
	{
		var list = string.Join(", ", names);

		// The names build the value on some alternatives and the rest name none of them.
		if (named && unsaid.Count > 0 && Fill(type, [.. names]) is { } fill)
			return $"'{rule.Name}' is built by '{Shown(fill)}' from its names, and {string.Join("; ", unsaid)}. Write '=> …' on " +
				"every alternative, or name the parts there too.";

		var why = unsaid.Count > 0 ? string.Join("; ", unsaid) + "." : "";

		if (named)
			why += (why.Length > 0 ? " " : "") + $"No constructor of '{type}' has every parameter covered by its names ({list}), " +
				"or two are covered equally, and they set none of its settable properties.";

		return recursive
			? $"'{rule.Name}' is left-recursive and declares '{type}', and which value a step hands on is not said: {why} " +
				"Write '=> …' on each alternative."
			: $"'{rule.Name}' declares '{type}' and does not say how to build it: {why} Only a string or a span is what the rule " +
				$"matched (§4.1). Write '=> …' on each alternative, or leave one unnamed call of '{type}' on every route.";

		string Shown(Construction construction)
		{
			return construction switch
			{
				Construction.Constructor(var order) => $"{type}({string.Join(", ", order)})",
				Construction.Initializer(var bindings) => $"{type} {{ {string.Join(", ", bindings.Select(static one => one.Property))} }}",
				_ => type,
			};
		}
	}
}
