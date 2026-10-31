using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// Repetitions that can read one run of text in two ways, so that a refused input is read
/// again for every way the run can be cut: exponentially many in its length.
/// </summary>
/// <remarks>
/// <para>
/// Over characters a choice and a repetition are given back to a later failure, and a rule
/// call is transparent to that. So a refused input is refused only after every reading of
/// what came before has been tried, and where a repetition can read some text <c>w</c> in two
/// ways, <c>w</c> repeated <c>n</c> times has two to the <c>n</c> readings. <c>(x+)*</c>,
/// <c>(x | x)*</c> and <c>('a' | 'a' &amp; 'a')+</c> are the shape; a URI template's run of
/// literal characters and an address's words with folding white space on both sides were met.
/// </para>
/// <para>
/// Decided on each repetition's own letter automaton: the Glushkov automaton of its body with
/// the edge back from the end of a turn to the start of the next, every edge counted as often
/// as the body offers it, since two routes to the same letter are two readings for the
/// backtracking even where the automaton has one edge. A letter is a character, a class or
/// <c>any</c>, which overlap where their characters do; a braced group, a back-reference, an
/// external recognizer, a <c>switch</c>, a call to a recursive or braced rule, or a call past
/// the inlining budget is opaque, and overlaps only itself. Other calls are inlined; captures,
/// bindings and marks are transparent; looks, guards and the trivia woven at a seam read
/// nothing. A repetition is exponentially ambiguous exactly where two different paths lead
/// from some state back to it over one word (Weber and Seidl): where the product of the
/// trimmed automaton with itself has a strongly connected component holding a pair of one
/// state and a pair of two, or holding an edge the automaton offers twice.
/// </para>
/// <para>
/// Exact for what it models, and that is not the grammar: a look or a <c>when</c> that removes
/// one of the two readings is taken as reading nothing, which tells too much; an opaque letter
/// that reads what another letter reads is not seen to overlap it, which tells too little. So
/// not told is not proved safe. A repetition in braces commits what it read and is not asked;
/// in a grammar cut in two only the rules marked <c>?</c> are, since every other rule over
/// kinds answers once, and the lexer reads its terminals without giving anything back.
/// </para>
/// </remarks>
public static class ExponentialRepetition
{
	/// <summary>How many letters a repetition's automaton is given before a call is taken as opaque.</summary>
	const int Positions = 256;

	/// <summary>How many letters at most, where what is written in the repetition itself is longer than that.</summary>
	const int Longest = 4 * Positions;

	/// <summary>How many pairs one repetition's product may ask about before it is left untold.</summary>
	const int Budget = 1 << 20;

	/// <summary>How long a witness is written before it is cut.</summary>
	const int Widest = 40;

	/// <summary>The widest bounded repetition written out turn by turn; a wider one is an opaque letter.</summary>
	const int Unrolled = 8;

	/// <summary>A repetition that can read <paramref name="Witness"/> in two ways.</summary>
	/// <param name="Rule">The rule the repetition is written in, as the author wrote it.</param>
	/// <param name="Loop">The repetition.</param>
	/// <param name="Witness">The shortest text it reads in two ways, in the grammar's notation.</param>
	public sealed record Told(RuleSymbol Rule, Node Loop, string Witness);

	/// <summary>What the analysis found, and how much of the grammar it could not see.</summary>
	/// <param name="Found">The repetitions told, the innermost of each nest, each once.</param>
	/// <param name="Loops">How many repetitions were asked.</param>
	/// <param name="Skipped">How many of them were past the step budget, and so not told.</param>
	/// <param name="Opaque">How many opaque letters their automata held.</param>
	public sealed record Result(IReadOnlyList<Told> Found, int Loops, int Skipped, int Opaque);

	public static Result Check(RecognitionGraph graph, LexicalSplit? lexical)
	{
		return Check(graph, lexical, out _);
	}

	/// <param name="graph">The grammar over characters.</param>
	/// <param name="lexical">The grammar cut in two, where it is: then its syntax over kinds is asked instead.</param>
	/// <param name="steps">The letters built and the pairs asked: the work done, for a test to hold to the grammar's size.</param>
	public static Result Check(RecognitionGraph graph, LexicalSplit? lexical, out int steps)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		var over  = lexical?.Syntax ?? graph;
		var kinds = lexical is not null;

		// Every rule some namespace weaves in as its trivia: a call to one is a seam, which
		// GRAM4051 is about, and reads nothing here.
		var seams = new HashSet<RuleSymbol>();

		foreach (var woven in over.Trivia.Values)
			if (woven is Node.Call(var called, _))
				seams.Add(called);

		var context = new Context(over, kinds ? lexical!.Inventory : null, seams);
		var found   = new List<Told>();
		var said    = new HashSet<(string, int, string)>();
		var loops   = 0;
		var skipped = 0;

		foreach (var rule in over.Rules)
		{
			if (rule.IsBuiltIn || !over.Bodies.TryGetValue(rule, out var body))
				continue;

			// Over kinds a rule not marked `?` answers once: its repetitions are as if braced.
			if (kinds && !rule.GivesBack)
				continue;

			var places = new List<Place>();
			var walked = new HashSet<Node>(NodeIdentity.Instance);

			Collect(body, -1, braced: false, places, walked);

			if (over.Folds.TryGetValue(rule, out var fold))
				Collect(fold.Loop, -1, braced: false, places, walked);

			foreach (var place in places)
			{
				loops++;

				var witness = context.Judge(place.Loop, rule, out var past);

				if (past)
					skipped++;

				place.Witness = witness;
			}

			// The innermost of a nest is told: a repetition around one that is told is
			// ambiguous because of it, and is said again once that one is mended.
			for (var i = 0; i < places.Count; i++)
				if (places[i].Witness is not null && places[i].Parent >= 0)
					for (var parent = places[i].Parent; parent >= 0; parent = places[parent].Parent)
						places[parent].Covered = true;

			var origin = over.Origin(rule);

			foreach (var place in places)
			{
				if (place.Witness is not { } witness || place.Covered)
					continue;

				// One repetition written once is one finding, however many clones see it.
				if (said.Add((origin.Name, origin.Declaration?.At.Position ?? -1, place.Loop.ToString())))
					found.Add(new Told(origin, place.Loop, witness));
			}
		}

		steps = context.Steps;

		return new Result(found, loops, skipped, context.Opaque);
	}

	/// <summary>A repetition found in a rule, and the repetition it is nested in.</summary>
	sealed class Place(Node loop, int parent)
	{
		public Node Loop { get; } = loop;

		public int Parent { get; } = parent;

		public string? Witness;

		public bool Covered;
	}

	/// <summary>Every unbounded repetition in a body, not entering calls, each with the one around it.</summary>
	/// <param name="braced">Whether the node stands directly in braces, with nothing after it there.</param>
	static void Collect(Node node, int parent, bool braced, List<Place> places, HashSet<Node> walked)
	{
		if (!walked.Add(node))
			return;

		switch (node)
		{
			// A count by a length is as unbounded as `*` where it has no upper bound.
			case Node.Repeat { Max: null } or Node.Counted { Max: null }:
			{
				var at = parent;

				// In braces with nothing after it, what it read is committed once it ends.
				if (!braced)
				{
					places.Add(new Place(node, parent));
					at = places.Count - 1;
				}

				foreach (var child in node.Children)
					Collect(child, at, braced: false, places, walked);

				return;
			}

			case Node.Atomic(var body):
				Collect(body, parent, braced: true, places, walked);

				return;

			case Node.Capture or Node.Marked or Node.Construct or Node.Bind:
				foreach (var child in node.Children)
					Collect(child, parent, braced, places, walked);

				return;

			// A call is a boundary: what is behind it is its own rule's. What a `when A is B`
			// asks is answered while the parser is generated.
			case Node.Call or Node.Condition:
				return;
		}

		foreach (var child in node.Children)
			Collect(child, parent, braced: false, places, walked);
	}

	/// <summary>What a letter reads: characters (or kinds), or one opaque thing.</summary>
	/// <param name="Reads">The characters, or null for an opaque letter.</param>
	/// <param name="Opaque">The opaque thing's number, -1 for characters.</param>
	/// <param name="Name">How the witness writes it, for an opaque letter.</param>
	readonly record struct Letter(FirstSets.First? Reads, int Opaque, string Name);

	/// <summary>The edges and letters of one fragment of a body, Glushkov's way.</summary>
	/// <param name="First">The letters it can begin with, each with how many ways.</param>
	/// <param name="Last">
	/// The letters it can end with, each with how many ways, and what a look after the letter
	/// refuses to let follow it on that way: <c>'a' &amp; eof</c> ends with an <c>'a'</c> nothing follows.
	/// </param>
	/// <param name="Empty">The ways it can read nothing, each with what a look on that way refuses to let follow.</param>
	readonly record struct Fragment(
		IReadOnlyList<(int Letter, int Ways)>                            First,
		IReadOnlyList<(int Letter, int Ways, FirstSets.First? Refused)> Last,
		IReadOnlyList<(int Ways, FirstSets.First? Refused)>             Empty)
	{
		public static readonly Fragment Nothing = new([], [], [(1, null)]);

		public static readonly Fragment Never = new([], [], []);

		/// <summary>Nothing read, and what follows is held to a look.</summary>
		public static Fragment Look(FirstSets.First? refused)
		{
			return new Fragment([], [], [(1, refused)]);
		}
	}

	/// <summary>What is shared by every repetition of a grammar: the graph, and the counts.</summary>
	sealed class Context(RecognitionGraph graph, TerminalInventory? inventory, HashSet<RuleSymbol> seams)
	{
		readonly Dictionary<object, int> _opaque = [];
		readonly Dictionary<Node, int>   _nodes  = new(NodeIdentity.Instance);

		public int Steps { get; private set; }

		public int Opaque { get; private set; }

		/// <summary>The shortest text a repetition reads in two ways, or null where it reads each one way.</summary>
		/// <param name="past">Whether it was past the budget, and so not told.</param>
		public string? Judge(Node loop, RuleSymbol rule, out bool past)
		{
			past = false;

			var automaton = new Automaton(this, rule);
			var whole     = loop is Node.Counted(var counted, _, _) ? new Node.Repeat(counted, 0, null) : loop;
			var fragment  = automaton.Of(whole);

			Opaque += automaton.Opaque;

			if (automaton.Letters.Count > Longest)
			{
				past = true;

				return null;
			}

			var product = new Product(automaton, fragment, this);

			if (!product.Explore())
			{
				past = true;

				return null;
			}

			return product.Witness();
		}

		public int Count(int steps)
		{
			Steps += steps;

			return Steps;
		}

		public RecognitionGraph Graph => graph;

		public HashSet<RuleSymbol> Seams => seams;

		public bool OverKinds => inventory is not null;

		/// <summary>The number of an opaque thing, the same for the same thing in every repetition.</summary>
		public int Numbered(object key)
		{
			if (key is Node node)
			{
				if (!_nodes.TryGetValue(node, out var known))
					_nodes[node] = known = _nodes.Count + _opaque.Count;

				return known;
			}

			if (!_opaque.TryGetValue(key, out var number))
				_opaque[key] = number = _nodes.Count + _opaque.Count;

			return number;
		}

		/// <summary>One letter of a witness: what both readings read there.</summary>
		public string Spelled(Letter one, Letter other)
		{
			if (one.Reads is null)
				return one.Name;

			var shared = other.Reads is null ? one.Reads : one.Reads.And(other.Reads);
			var ranges = shared.Ranges;

			if (inventory is not null)
				return ranges.Count == 1 && ranges[0].IsSingle && ranges[0].From - 1 < inventory.Kinds.Count && ranges[0].From > 0
					? Kind(inventory.Kinds[ranges[0].From - 1])
					: $"[{ranges.Sum(static range => range.To - range.From + 1)} kinds]";

			if (ranges.Count == 1 && ranges[0].IsSingle)
				return Quoted(ranges[0].From);

			if (ranges.Count == 1 && ranges[0].From == char.MinValue && ranges[0].To == char.MaxValue)
				return "any";

			const int most = 3;

			var spelled = string.Join(" | ", ranges.Take(most).Select(static range => range.IsSingle ? Quoted(range.From) : $"{Quoted(range.From)}..{Quoted(range.To)}"));

			return ranges.Count > most ? $"[{spelled} | …]" : $"[{spelled}]";
		}

		/// <summary>A character as a grammar writes it, with what does not print written by its number.</summary>
		static string Quoted(char value)
		{
			return value is < ' ' and not ('\n' or '\r' or '\t') || value is >= '\u007f' and < '\u00a0' || char.IsSurrogate(value) || value >= '\ufff0'
				? $"'\\u{(int)value:X4}'"
				: CharRange.Quote(value);
		}

		static string Kind(TerminalInventory.Kind kind)
		{
			return kind.Matched.Count > 0 ? kind.Matched[0].ToString() : kind.Number.ToString(System.Globalization.CultureInfo.InvariantCulture);
		}
	}

	/// <summary>The letters and counted edges of one repetition.</summary>
	sealed class Automaton(Context context, RuleSymbol rule)
	{
		readonly HashSet<RuleSymbol> _inlining = [rule];

		/// <summary>The letters; the first is the start, which reads nothing.</summary>
		public List<Letter> Letters { get; } = [new Letter(null, -1, "")];

		/// <summary>Every edge in the order it was found, an edge offered twice found twice.</summary>
		public List<(int From, int To, int Ways)> Edges { get; } = [];

		public int Opaque { get; private set; }

		public Context Context => context;

		public Fragment Of(Node node)
		{
			switch (node)
			{
				case Node.Literal literal:
				{
					var fragment = Fragment.Nothing;

					foreach (var item in literal.Text)
						fragment = Then(fragment, Leaf(new Letter(
							literal.IgnoreCase ? FirstSets.First.Chars(CaseFold.Ranges(item)) : FirstSets.First.Chars([new CharRange(item, item)]),
							-1, "")));

					return fragment;
				}

				case Node.Element { References.Count: 0 } element:
					return Leaf(new Letter(FirstSets.OfElement(element), -1, ""));

				// A class that asks the host: what it reads is the host's to know.
				case Node.Element element:
					return Opaqued(element.ToString(), element.ToString());

				case Node.Sequence(var parts):
					return Sequence(parts);

				case Node.Choice { Selection: not null } chosen:
					return Opaqued(chosen, "switch");

				case Node.Choice(var alternatives):
				{
					var first = new List<(int, int)>();
					var last  = new List<(int, int, FirstSets.First?)>();
					var empty = new List<(int, FirstSets.First?)>();

					foreach (var alternative in alternatives)
					{
						var one = Of(alternative);

						first.AddRange(one.First);
						last.AddRange(one.Last);

						foreach (var (ways, refused) in one.Empty)
							Merge(empty, ways, refused);
					}

					return new Fragment(first, last, empty);
				}

				case Node.Repeat repeat:
					return Repeated(repeat);

				case Node.Atomic atomic:
					return Opaqued(atomic, "{…}");

				case Node.Same same:
					return Opaqued(same.Binding, same.ToString());

				case Node.External external:
					return Opaqued("@" + external.Name, external.ToString());

				case Node.Counted counted:
					return Opaqued(counted, counted.ToString());

				case Node.Capture(_, var body):    return Of(body);
				case Node.Marked(var body, _):     return Of(body);
				case Node.Construct(var body, _):  return Of(body);
				case Node.Bind(_, var body, _):    return Of(body);

				case Node.Call call:
					return Called(call);

				// A look that is one class holds what may follow it; any other is taken as reading nothing.
				case Node.Lookahead(false, var body) when OneClass(body) is { } one:
					return Fragment.Look(one);

				case Node.Lookahead(true, var body) when Begins(body) is { } one:
					return Fragment.Look(Outside(one));

				// Guards, readings, glue and `none` read nothing.
				default:
					return Fragment.Nothing;
			}
		}

		/// <summary>
		/// A sequence, where a look in front of an operand that is one class takes its characters
		/// out of what the operand begins with: <c>?!'\'' &amp; any</c> reads no quote.
		/// </summary>
		Fragment Sequence(IReadOnlyList<Node> parts)
		{
			var fragment = Fragment.Nothing;

			FirstSets.First? refused = null;
			FirstSets.First? wanted  = null;

			foreach (var part in parts)
			{
				switch (part)
				{
					case Node.Lookahead(false, var body) when OneClass(body) is { } one:
						refused = refused is null ? one : refused.Or(one);
						continue;

					case Node.Lookahead(true, var body) when Begins(body) is { } one:
						wanted = wanted is null ? one : wanted.And(one);
						continue;

					case Node.Lookahead or Node.Glue or Node.Guard or Node.Condition or Node.Empty or Node.Reading or Node.Behind:
						continue;
				}

				var letters = Letters.Count;
				var edges   = Edges.Count;
				var next    = Of(part);

				if (refused is not null || wanted is not null)
				{
					Narrowed(next, letters, edges, refused, wanted);

					// Where the operand can read nothing, the look holds what follows it too.
					var held = Joined(refused, wanted is null ? null : Outside(wanted));

					next = next with { Empty = [.. next.Empty.Select(route => (route.Ways, Joined(route.Refused, held)))] };
				}

				refused = null;
				wanted  = null;

				fragment = Then(fragment, next);
			}

			if (refused is not null || wanted is not null)
				fragment = Then(fragment, Fragment.Look(Joined(refused, wanted is null ? null : Outside(wanted))));

			return fragment;
		}

		/// <summary>Every character a set does not hold.</summary>
		static FirstSets.First Outside(FirstSets.First set)
		{
			return FirstSets.First.Chars(FirstSets.Complement(set.Ranges));
		}

		/// <summary>What two looks at one place refuse together.</summary>
		static FirstSets.First? Joined(FirstSets.First? one, FirstSets.First? other)
		{
			return one is null ? other : other is null ? one : one.Or(other);
		}

		/// <summary>Whether a look refuses every character a letter reads, so that it cannot follow.</summary>
		bool Blocked(FirstSets.First? refused, int letter)
		{
			if (refused is null)
				return false;

			return Letters[letter].Reads is { } reads
				? refused.Covers(reads)
				: refused.Covers(FirstSets.First.Chars([new CharRange(char.MinValue, char.MaxValue)]));
		}

		/// <summary>One more way to read nothing, counted with the others held to the same look.</summary>
		static void Merge(List<(int Ways, FirstSets.First? Refused)> routes, int ways, FirstSets.First? refused)
		{
			for (var i = 0; i < routes.Count; i++)
				if (routes[i].Refused is null ? refused is null : refused is not null && FirstSets.Same(routes[i].Refused!, refused))
				{
					routes[i] = (Ways(routes[i].Ways + ways), refused);

					return;
				}

			routes.Add((ways, refused));
		}

		/// <summary>
		/// The letters an operand begins with, narrowed to what the looks in front of it let
		/// through; only those the operand itself never comes back to, where the look still holds.
		/// </summary>
		void Narrowed(Fragment next, int letters, int edges, FirstSets.First? refused, FirstSets.First? wanted)
		{
			var returned = new HashSet<int>();

			for (var i = edges; i < Edges.Count; i++)
				returned.Add(Edges[i].To);

			foreach (var (at, _) in next.First)
			{
				if (at < letters || returned.Contains(at) || Letters[at].Reads is not { } reads)
					continue;

				if (refused is not null)
					reads = reads.And(Outside(refused));

				if (wanted is not null)
					reads = reads.And(wanted);

				Letters[at] = Letters[at] with { Reads = reads };
			}
		}

		/// <summary>
		/// The characters a positive look's body must begin with, where they are known and it
		/// cannot read nothing: what may stand next is one of them.
		/// </summary>
		FirstSets.First? Begins(Node body)
		{
			if (FirstSets.Nullable(body, context.Graph))
				return null;

			var first = FirstSets.Of(body, context.Graph);

			return first.IsKnown ? FirstSets.First.Chars(first.Ranges) : null;
		}

		/// <summary>The characters a look asks about, where it is one class: a class, a character, or a choice of them, at most <c>any</c> after it.</summary>
		FirstSets.First? OneClass(Node body)
		{
			switch (body)
			{
				case Node.Element { References.Count: 0 } element:
					return FirstSets.OfElement(element);

				case Node.Literal { Text.Length: 1 } literal:
					return literal.IgnoreCase
						? FirstSets.First.Chars(CaseFold.Ranges(literal.Text[0]))
						: FirstSets.First.Chars([new CharRange(literal.Text[0], literal.Text[0])]);

				case Node.Sequence(var parts):
				{
					var reading = parts.Where(static part => part is not Node.Glue).ToArray();

					return reading.Length == 1 ? OneClass(reading[0]) :
						reading.Length == 2 && reading[1] is Node.Call({ IsBuiltIn: true, Name: "any" }, _) ? OneClass(reading[0]) :
						null;
				}

				case Node.Call({ IsBuiltIn: true, Name: "any" }, _):
					return FirstSets.First.Chars([new CharRange(char.MinValue, char.MaxValue)]);

				// A rule that is one class, called: what it is, unless it is one of those being read.
				case Node.Call(var called, { Count: 0 }) when !_inlining.Contains(called) && context.Graph.Bodies.TryGetValue(called, out var rule):
				{
					_inlining.Add(called);

					var one = OneClass(rule);

					_inlining.Remove(called);

					return one;
				}

				case Node.Choice { Selection: null } choice:
				{
					var all = FirstSets.First.None;

					foreach (var alternative in choice.Nodes)
					{
						if (OneClass(alternative) is not { } one)
							return null;

						all = all.Or(one);
					}

					return all;
				}

				default:
					return null;
			}
		}

		Fragment Called(Node.Call call)
		{
			var graph  = context.Graph;
			var called = call.Rule;

			if (context.Seams.Contains(called))
				return Fragment.Nothing;

			if (call.Arguments.Count > 0 || !graph.Bodies.TryGetValue(called, out var body) ||
				graph.Calls.Recurses(called) || _inlining.Contains(called) ||
				context.OverKinds && !called.IsBuiltIn && !called.GivesBack)
				return Opaqued(called, called.Name);

			if (body is Node.Atomic)
				return Opaqued(called, called.Name);

			if (Letters.Count >= Positions)
				return Opaqued(called, called.Name);

			// Inlined, and taken back if it does not fit.
			var letters = Letters.Count;
			var edges   = Edges.Count;
			var opaque  = Opaque;

			_inlining.Add(called);

			var fragment = Of(body);

			_inlining.Remove(called);

			if (Letters.Count <= Positions)
				return fragment;

			Letters.RemoveRange(letters, Letters.Count - letters);
			Edges.RemoveRange(edges, Edges.Count - edges);
			Opaque = opaque;

			return Opaqued(called, called.Name);
		}

		Fragment Repeated(Node.Repeat repeat)
		{
			var (body, min, max) = (repeat.Body, repeat.Min, repeat.Max);

			if (max == 0)
				return Fragment.Nothing;

			// Too wide to write out, and not a run that may go on: one opaque letter, which is the
			// side that tells too little. `any{4096}` repeated cuts one way.
			if (max is > Unrolled)
				return Opaqued(repeat, repeat.ToString());

			// Written out turn by turn, where it is narrow: `X{2,3}` is `X & X & X?`.
			if (max is { } most)
			{
				var fragment = Fragment.Nothing;

				for (var turn = 0; turn < min; turn++)
					fragment = Then(fragment, Of(body));

				// The optional turns nest, each one only after the one before: `(X & X?)?`.
				var optional = Fragment.Nothing;

				for (var turn = min; turn < most; turn++)
				{
					var one = Of(body);

					optional = Then(one, optional);

					var empty = new List<(int, FirstSets.First?)>(optional.Empty);

					Merge(empty, 1, null);

					optional = optional with { Empty = empty };
				}

				return Then(fragment, optional);
			}

			// An unbounded repetition: the end of a turn leads back to its beginning. A turn that
			// reads nothing ends it, so an empty turn is not another way round.
			var turns = Of(body);

			Link(turns.Last, turns.First);

			if (min > 0)
				return turns;

			var none = new List<(int, FirstSets.First?)>(turns.Empty);

			Merge(none, 1, null);

			return turns with { Empty = none };
		}

		Fragment Then(Fragment before, Fragment after)
		{
			Link(before.Last, after.First);

			var first = new List<(int, int)>(before.First);
			var last  = new List<(int, int, FirstSets.First?)>(after.Last);
			var empty = new List<(int, FirstSets.First?)>();

			foreach (var (ways, refused) in before.Empty)
				foreach (var (letter, more) in after.First)
					if (!Blocked(refused, letter))
						first.Add((letter, Ways(ways * more)));

			foreach (var (ways, refused) in after.Empty)
				foreach (var (letter, more, held) in before.Last)
					last.Add((letter, Ways(ways * more), Joined(held, refused)));

			foreach (var (ways, refused) in before.Empty)
				foreach (var (more, held) in after.Empty)
					Merge(empty, Ways(ways * more), Joined(refused, held));

			return new Fragment(first, last, empty);
		}

		void Link(IReadOnlyList<(int Letter, int Ways, FirstSets.First? Refused)> from, IReadOnlyList<(int Letter, int Ways)> to)
		{
			foreach (var (one, ways, refused) in from)
				foreach (var (other, more) in to)
					if (!Blocked(refused, other))
						Edges.Add((one, other, Ways(ways * more)));
		}

		Fragment Leaf(Letter letter)
		{
			context.Count(1);

			// A letter that reads nothing at all can never be read: trimmed here, before the product.
			if (letter.Reads is { Nothing: true })
				return Fragment.Never;

			Letters.Add(letter);

			var at = Letters.Count - 1;

			return new Fragment([(at, 1)], [(at, 1, null)], []);
		}

		Fragment Opaqued(object key, string name)
		{
			Opaque++;

			return Leaf(new Letter(null, context.Numbered(key), name));
		}

		/// <summary>Two ways is all the question needs: one, or more than one.</summary>
		static int Ways(int ways)
		{
			return ways > 2 ? 2 : ways;
		}
	}

	/// <summary>The product of a repetition's trimmed automaton with itself, and what it shows.</summary>
	sealed class Product
	{
		readonly Automaton                 _automaton;
		readonly Context                   _context;
		readonly int                       _count;
		readonly bool[]                    _final;
		readonly List<(int To, int Ways)>[] _next;

		readonly Dictionary<long, int> _index  = [];
		readonly List<(int One, int Other)> _pairs = [];
		readonly List<List<(int To, bool Twice)>> _edges = [];

		sbyte[]? _overlap;

		int[]? _component;
		bool[]? _exponential;

		public Product(Automaton automaton, Fragment loop, Context context)
		{
			_automaton = automaton;
			_context   = context;
			_count     = automaton.Letters.Count;
			_final     = new bool[_count];
			_next      = new List<(int, int)>[_count];

			for (var i = 0; i < _count; i++)
				_next[i] = [];

			var ways = new Dictionary<long, int>();

			foreach (var (from, to, count) in automaton.Edges)
				Add(ways, from, to, count);

			foreach (var (letter, count) in loop.First)
				Add(ways, 0, letter, count);

			foreach (var (letter, _, _) in loop.Last)
				_final[letter] = true;

			if (loop.Empty.Count > 0)
				_final[0] = true;

			var useful = Useful(ways);

			foreach (var pair in ways)
			{
				var from = (int)(pair.Key / _count);
				var to   = (int)(pair.Key % _count);

				if (useful[from] && useful[to])
					_next[from].Add((to, pair.Value));
			}
		}

		void Add(Dictionary<long, int> ways, int from, int to, int count)
		{
			var key = (long)from * _count + to;

			ways[key] = Math.Min(2, (ways.TryGetValue(key, out var already) ? already : 0) + count);
		}

		/// <summary>The letters reached from the start that reach an end: the trimmed automaton.</summary>
		bool[] Useful(Dictionary<long, int> ways)
		{
			var forward  = new List<int>[_count];
			var backward = new List<int>[_count];

			for (var i = 0; i < _count; i++)
			{
				forward[i]  = [];
				backward[i] = [];
			}

			foreach (var key in ways.Keys)
			{
				var from = (int)(key / _count);
				var to   = (int)(key % _count);

				// A letter a look narrowed to nothing is never read, and nothing is reached past it.
				if (_automaton.Letters[from].Reads is { Nothing: true } || _automaton.Letters[to].Reads is { Nothing: true })
					continue;

				forward[from].Add(to);
				backward[to].Add(from);
			}

			var reached = Reach(forward, [0]);
			var ending  = Reach(backward, Enumerable.Range(0, _count).Where(i => _final[i]));
			var useful  = new bool[_count];

			for (var i = 0; i < _count; i++)
				useful[i] = reached[i] && ending[i];

			return useful;
		}

		bool[] Reach(List<int>[] edges, IEnumerable<int> from)
		{
			var seen    = new bool[_count];
			var pending = new Stack<int>();

			foreach (var start in from)
				if (!seen[start])
				{
					seen[start] = true;
					pending.Push(start);
				}

			while (pending.Count > 0)
				foreach (var next in edges[pending.Pop()])
					if (!seen[next])
					{
						seen[next] = true;
						pending.Push(next);
					}

			return seen;
		}

		bool Overlap(int one, int other)
		{
			_overlap ??= new sbyte[_count * _count];

			var at = one * _count + other;

			if (_overlap[at] != 0)
				return _overlap[at] > 0;

			var a = _automaton.Letters[one];
			var b = _automaton.Letters[other];

			var overlaps = a.Reads is null || b.Reads is null
				? a.Opaque == b.Opaque
				: a.Reads.Overlaps(b.Reads);

			_overlap[at] = overlaps ? (sbyte)1 : (sbyte)-1;

			return overlaps;
		}

		int Pair(int one, int other, Queue<int> pending)
		{
			var key = (long)one * _count + other;

			if (_index.TryGetValue(key, out var at))
				return at;

			at = _pairs.Count;

			_index[key] = at;
			_pairs.Add((one, other));
			_edges.Add([]);
			pending.Enqueue(at);

			return at;
		}

		/// <summary>Every pair reached from the start; false where that was past the budget.</summary>
		public bool Explore()
		{
			var pending = new Queue<int>();
			var asked   = 0;

			Pair(0, 0, pending);

			while (pending.Count > 0)
			{
				var at           = pending.Dequeue();
				var (one, other) = _pairs[at];

				foreach (var (next, ways) in _next[one])
					foreach (var (more, moreWays) in _next[other])
					{
						if (++asked > Budget)
						{
							_context.Count(asked);

							return false;
						}

						if (!Overlap(next, more))
							continue;

						// The same edge taken by both is one path, unless the automaton offers it twice.
						var twice = one == other && next == more && ways > 1;

						_edges[at].Add((Pair(next, more, pending), twice));
					}
			}

			_context.Count(asked);

			Components();

			return true;
		}

		/// <summary>Tarjan's components, without recursion, and which of them prove two paths round.</summary>
		void Components()
		{
			var count     = _pairs.Count;
			var index     = new int[count];
			var low       = new int[count];
			var onStack   = new bool[count];
			var stack     = new Stack<int>();
			var component = new int[count];
			var next      = 1;
			var found     = 0;

			for (var start = 0; start < count; start++)
			{
				if (index[start] != 0)
					continue;

				var work = new Stack<(int Node, int At)>();

				index[start] = low[start] = next++;
				stack.Push(start);
				onStack[start] = true;
				work.Push((start, 0));

				while (work.Count > 0)
				{
					var (node, at) = work.Pop();

					if (at < _edges[node].Count)
					{
						work.Push((node, at + 1));

						var to = _edges[node][at].To;

						if (index[to] == 0)
						{
							index[to] = low[to] = next++;
							stack.Push(to);
							onStack[to] = true;
							work.Push((to, 0));
						}
						else if (onStack[to])
							low[node] = Math.Min(low[node], index[to]);

						continue;
					}

					if (low[node] == index[node])
					{
						int member;

						do
						{
							member = stack.Pop();
							onStack[member] = false;
							component[member] = found;
						}
						while (member != node);

						found++;
					}

					if (work.Count > 0)
					{
						var parent = work.Peek().Node;

						low[parent] = Math.Min(low[parent], low[node]);
					}
				}
			}

			var same      = new bool[found];
			var different = new bool[found];
			var twice     = new bool[found];

			for (var at = 0; at < count; at++)
			{
				var (one, other) = _pairs[at];

				if (one == other)
					same[component[at]] = true;
				else
					different[component[at]] = true;

				foreach (var (to, both) in _edges[at])
					if (both && component[to] == component[at])
						twice[component[at]] = true;
			}

			_component   = component;
			_exponential = new bool[found];

			for (var i = 0; i < found; i++)
				_exponential[i] = same[i] && different[i] || twice[i];
		}

		/// <summary>
		/// The shortest text read from the start to the end of a turn on two paths, one of them
		/// round the cycle that makes it exponential; null where there is no such cycle.
		/// </summary>
		public string? Witness()
		{
			if (_component is null || _exponential is null || !_exponential.Any(static one => one))
				return null;

			// A path through the pairs, with whether it has yet gone round the exponential part.
			var count   = _pairs.Count;
			var parent  = new int[2 * count];
			var seen    = new bool[2 * count];
			var pending = new Queue<int>();

			seen[0] = true;
			parent[0] = -1;
			pending.Enqueue(0);

			while (pending.Count > 0)
			{
				var state       = pending.Dequeue();
				var at          = state % count;
				var apart       = state >= count;
				var (one, other) = _pairs[at];

				if (apart && _final[one] && _final[other])
					return Spelled(state, parent);

				foreach (var (to, both) in _edges[at])
				{
					var inside = _exponential[_component[to]] && _component[to] == _component[at];
					var parted = apart || inside && (both || _pairs[to].One != _pairs[to].Other);
					var next   = to + (parted ? count : 0);

					if (seen[next])
						continue;

					seen[next] = true;
					parent[next] = state;
					pending.Enqueue(next);
				}
			}

			return "…";
		}

		string Spelled(int state, int[] parent)
		{
			var count   = _pairs.Count;
			var letters = new List<string>();

			for (var at = state; parent[at] >= 0; at = parent[at])
			{
				var (one, other) = _pairs[at % count];

				letters.Add(_context.Spelled(_automaton.Letters[one], _automaton.Letters[other]));
			}

			letters.Reverse();

			var written = string.Join(" ", letters);

			if (written.Length <= Widest)
				return written;

			var cut = written.LastIndexOf(' ', Widest);

			return (cut > 0 ? written.Substring(0, cut) : written.Substring(0, Widest)) + " …";
		}
	}
}
