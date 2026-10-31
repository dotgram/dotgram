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
/// not told is not proved safe. A repetition where nothing after it, up to the braces or the
/// look around it, can fail keeps its first reading and is not asked, unless it must take more
/// turns than one; in a grammar cut in two only the rules marked <c>?</c> are asked, since every
/// other rule over kinds answers once, and the lexer reads its terminals without giving
/// anything back. Building an automaton is held to a budget as the product is, and a
/// repetition past either is counted and not told.
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

	/// <summary>
	/// How much work building one repetition's automaton may take before it is left untold: every
	/// letter, every edge and every entry of a fragment made or copied counts, so that a nest of
	/// written-out counts or a run of looks that multiply the ways to read nothing stops early
	/// rather than after the memory is spent.
	/// </summary>
	const int Building = 1 << 16;

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

	/// <summary>
	/// A warning for every repetition told, at the rule it is written in: the graph's nodes carry
	/// no place of their own, so the loop is spelled in the message for the author to find.
	/// </summary>
	public static IReadOnlyList<GramDiagnostic> Report(RecognitionGraph graph, LexicalSplit? lexical)
	{
		var reported = new List<GramDiagnostic>();

		foreach (var told in Check(graph, lexical).Found)
			reported.Add(new GramDiagnostic(
				GramCompiler.ExponentialRepetitions,
				$"The repetition '{Spelled(told.Loop)}' in '{told.Rule.Name}' can read {told.Witness} in two ways, so a " +
				"refused input is read again for every way of cutting it: exponentially many times (docs/syntax.md §3.3).",
				told.Rule.Declaration?.At.Position ?? 0,
				told.Rule.Declaration?.At.Length ?? 0,
				GramSeverity.Warning));

		return reported;
	}

	/// <summary>A repetition as the grammar writes it, cut where it is long.</summary>
	static string Spelled(Node loop)
	{
		const int most = 60;

		var written = loop.ToString();

		return written.Length <= most ? written : written.Substring(0, most) + " …";
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

		var context   = new Context(over, kinds ? lexical!.Inventory : null, seams);
		var unfailing = new Unfailing(over, context);
		var lifts     = new Lifts(over);
		var found   = new List<Told>();
		var said    = new HashSet<(object, string, int)>();
		var loops   = 0;
		var skipped = 0;

		foreach (var rule in over.Rules)
		{
			if (rule.IsBuiltIn || !over.Bodies.TryGetValue(rule, out var body))
				continue;

			// Over kinds a rule not marked `?` answers once: its repetitions are as if braced.
			if (kinds && !rule.GivesBack)
				continue;

			// A rule of a grammar spliced on, an included one or the standard library, is that
			// grammar's to be told about, in its own build: here nobody could mend it.
			if (Spliced(over.Origin(rule)))
				continue;

			var places = new List<Place>();
			var walked = new Dictionary<Node, bool>(NodeIdentity.Instance);

			Collect(body, -1, free: false, unfailing, places, walked);

			if (over.Folds.TryGetValue(rule, out var fold))
				Collect(fold.Loop, -1, free: false, unfailing, places, walked);

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

				// One repetition written once is one finding, however many clones see it: a clone of a
				// rule is walked in the same order, so the place a repetition has among its rule's is
				// the occurrence, and two written alike in one rule are two. A rule lifted out of a
				// marked repetition is a part of its origin of its own, known by where it was lifted
				// among the parts its parent gave, which a clone gives in the same order.
				// A clone made by a rebinding carries the declaration it was made from.
				if (said.Add(((object?)origin.Declaration ?? origin, lifts.Path(rule), places.IndexOf(place))))
					found.Add(new Told(origin, place.Loop, witness));
			}
		}

		steps = context.Steps;

		return new Result(found, loops, skipped, context.Opaque);
	}

	/// <summary>Where each rule lifted out of a marked repetition stands among the parts its parent gave, and its parent among its own.</summary>
	sealed class Lifts
	{
		readonly RecognitionGraph                          _graph;
		readonly Dictionary<RuleSymbol, int>               _index = [];
		readonly Dictionary<RuleSymbol, string>            _paths = [];

		public Lifts(RecognitionGraph graph)
		{
			_graph = graph;

			var given = new Dictionary<RuleSymbol, int>();

			foreach (var rule in graph.Rules)
				if (graph.Lifted.TryGetValue(rule, out var lift))
				{
					given.TryGetValue(lift.Parent, out var count);
					given[lift.Parent] = count + 1;
					_index[rule]       = count;
				}
		}

		/// <summary>The indices from the rule the author wrote down to this one, empty for that rule.</summary>
		public string Path(RuleSymbol rule)
		{
			if (_paths.TryGetValue(rule, out var known))
				return known;

			var steps = new List<int>();

			for (var at = rule; _graph.Lifted.TryGetValue(at, out var lift) && steps.Count <= _graph.Rules.Count; at = lift.Parent)
				steps.Add(_index[at]);

			steps.Reverse();

			return _paths[rule] = string.Join("/", steps);
		}
	}

	/// <summary>Whether a rule came in with a grammar spliced onto the author's, rather than being written by the author.</summary>
	static bool Spliced(RuleSymbol rule)
	{
		for (var ns = rule.Namespace; ns is not null; ns = ns.Parent)
			if (ns.IsRoot)
				return true;

		return false;
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
	/// <param name="free">
	/// Whether nothing after the node, up to what commits it, can fail: then no failure ever comes
	/// back to ask it for another reading. Braces and looks commit their first reading, so what
	/// stands last in them is free; <c>{ (('a' | 'a' &amp; 'a')+)* }</c> reads its run once.
	/// </param>
	/// <param name="walked">Every node asked, and whether it was free there: one asked free is asked again where it is not.</param>
	static void Collect(Node node, int parent, bool free, Unfailing facts, List<Place> places, Dictionary<Node, bool> walked)
	{
		if (walked.TryGetValue(node, out var was) && (!was || free))
			return;

		walked[node] = free;

		switch (node)
		{
			// A count by a length is as unbounded as `*` where it has no upper bound.
			case Node.Repeat { Max: null } or Node.Counted { Max: null }:
			{
				var at        = parent;
				var committed = free && Fewest(node) is <= 1;

				// Free, what it reads first is what it keeps; but a repetition that must take more
				// than one turn, or as many as the input says, can fail after any reading of the turns
				// before, and is asked: `{ ('a' | 'a' & 'a'){#q,} }` never succeeds and so never
				// commits. A repetition marked `recover` commits each turn it finishes (§8.2).
				if (!committed && !facts.Graph.Recoveries.ContainsKey(node))
				{
					places.Add(new Place(node, parent));
					at = places.Count - 1;
				}

				foreach (var child in node.Children)
					Collect(child, at, committed, facts, places, walked);

				return;
			}

			// A turn of a bounded repetition is free where the repetition is and one turn is enough.
			case Node.Repeat or Node.Counted:
				foreach (var child in node.Children)
					Collect(child, parent, free && Fewest(node) is <= 1, facts, places, walked);

				return;

			case Node.Atomic(var body):
				Collect(body, parent, free: true, facts, places, walked);

				return;

			// A look holds or refuses on the first reading of what it looks at.
			case Node.Lookahead(_, var body):
				Collect(body, parent, free: true, facts, places, walked);

				return;

			case Node.Sequence(var parts):
			{
				// A part is free where the sequence is and nothing after it in the sequence can fail.
				var rest = free;

				var frees = new bool[parts.Count];

				for (var i = parts.Count - 1; i >= 0; i--)
				{
					frees[i] = rest;
					rest     = rest && facts.Holds(parts[i]);
				}

				for (var i = 0; i < parts.Count; i++)
					Collect(parts[i], parent, frees[i], facts, places, walked);

				return;
			}

			case Node.Choice or Node.Capture or Node.Marked or Node.Construct or Node.Bind:
				foreach (var child in node.Children)
					Collect(child, parent, free, facts, places, walked);

				return;

			// A call is a boundary: what is behind it is its own rule's. What a `when A is B`
			// asks is answered while the parser is generated.
			case Node.Call or Node.Condition:
				return;
		}

		foreach (var child in node.Children)
			Collect(child, parent, free: false, facts, places, walked);
	}

	/// <summary>The fewest turns a repetition takes, or null where the input says.</summary>
	static int? Fewest(Node repetition)
	{
		return repetition switch
		{
			Node.Repeat repeat                                 => repeat.Min,
			Node.Counted { Min: CountBound.Items(var fewest) } => fewest,
			_                                                  => null,
		};
	}

	/// <summary>How deep a walk of the analysis follows calls before it answers as if it knew nothing more.</summary>
	const int Deepest = 64;

	/// <summary>
	/// Whether a node succeeds wherever it is asked, reading nothing if it must: what follows it
	/// cannot fail there.
	/// </summary>
	/// <remarks>
	/// What every rule is, worked out once for the grammar and without following a call: the
	/// rules that cannot fail are the least set closed under their bodies, found from a work list
	/// that asks a rule again only when a rule it calls was found to be one. So a chain or a
	/// diamond of calls of any depth costs its size, once, and every body asked is counted.
	/// </remarks>
	sealed class Unfailing(RecognitionGraph graph, Context context)
	{
		HashSet<RuleSymbol>? _holding;

		public RecognitionGraph Graph => graph;

		public bool Holds(Node node)
		{
			_holding ??= Settled();

			return Local(node, _holding);
		}

		HashSet<RuleSymbol> Settled()
		{
			var holding = new HashSet<RuleSymbol>();
			var callers = new Dictionary<RuleSymbol, List<RuleSymbol>>();
			var pending = new Queue<RuleSymbol>();

			foreach (var rule in graph.Rules)
			{
				if (!graph.Bodies.TryGetValue(rule, out var body))
					continue;

				pending.Enqueue(rule);

				var nodes = new Stack<Node>();

				nodes.Push(body);

				while (nodes.Count > 0)
				{
					var node = nodes.Pop();

					context.Count(1);

					if (node is Node.Call(var called, _))
					{
						if (!callers.TryGetValue(called, out var calling))
							callers[called] = calling = [];

						calling.Add(rule);
					}

					foreach (var child in node.Children)
						nodes.Push(child);
				}
			}

			while (pending.Count > 0)
			{
				var rule = pending.Dequeue();

				if (holding.Contains(rule) || !Local(graph.Bodies[rule], holding))
					continue;

				holding.Add(rule);

				if (callers.TryGetValue(rule, out var calling))
					foreach (var caller in calling)
						pending.Enqueue(caller);
			}

			return holding;
		}

		/// <summary>Whether a body cannot fail, given the rules known so far that cannot; a call is not followed.</summary>
		bool Local(Node node, HashSet<RuleSymbol> holding)
		{
			context.Count(1);

			switch (node)
			{
				case Node.Empty:
				case Node.Literal { Text.Length: 0 }:
				case Node.Repeat { Min: 0 }:
				case Node.Counted { Min: CountBound.Items(0) }:
					return true;

				case Node.Sequence(var parts):
					return parts.All(part => Local(part, holding));

				case Node.Choice { Selection: null } choice:
					return choice.Nodes.Any(alternative => Local(alternative, holding));

				case Node.Atomic(var body):
					return Local(body, holding);

				case Node.Capture or Node.Marked or Node.Construct or Node.Bind:
					return node.Children.All(child => Local(child, holding));

				case Node.Call(var called, _):
					return holding.Contains(called);

				default:
					return false;
			}
		}
	}

	/// <summary>What a letter reads: characters (or kinds), or one opaque thing.</summary>
	/// <param name="Reads">The characters, or null for an opaque letter.</param>
	/// <param name="Opaque">The opaque thing's number, -1 for characters.</param>
	/// <param name="Name">How the witness writes it, for an opaque letter.</param>
	/// <param name="NotAfter">What the character before it may not be, where a look behind it says (§4.6).</param>
	/// <param name="Begins">What an opaque letter that reads something begins with, where that is known.</param>
	readonly record struct Letter(FirstSets.First? Reads, int Opaque, string Name, FirstSets.First? NotAfter = null, FirstSets.First? Begins = null);

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

		ExecutionPlan? _plan;

		/// <summary>What each rule is as one class, or null where it is not one (<see cref="Automaton.OneClass"/>).</summary>
		public Dictionary<RuleSymbol, FirstSets.First?> Classes { get; } = [];

		public int Steps { get; private set; }

		public int Opaque { get; private set; }

		/// <summary>The shortest text a repetition reads in two ways, or null where it reads each one way.</summary>
		/// <param name="past">Whether it was past the budget, and so not told.</param>
		public string? Judge(Node loop, RuleSymbol rule, out bool past)
		{
			past = false;

			var automaton = new Automaton(this, rule);
			var whole     = loop is Node.Counted(var counted, _, _) ? new Node.Repeat(counted, 0, null) : loop;

			Fragment fragment;

			try
			{
				fragment = automaton.Of(whole);
			}
			catch (Exhausted)
			{
				past = true;

				return null;
			}
			finally
			{
				Count(automaton.Work);

				Opaque += automaton.Opaque;
			}

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

		/// <summary>
		/// Whether a repetition's body is one test of one character, as the emitter writes it: a
		/// choice of such alternatives is then one letter, since whichever matched, nothing is
		/// left to come back to (<see cref="ExecutionPlan.OneCharacter"/>).
		/// </summary>
		/// <remarks>
		/// A rule counts as written in place as the emitter writes it where it is most careful,
		/// so that no rendering keeps a way back this takes for gone.
		/// </remarks>
		/// <param name="step">Told of every node the walk visits: the repetition's budget.</param>
		public bool OneCharacter(Node body, Func<bool> step)
		{
			return ExecutionPlan.OneCharacter(body, graph, InPlace, step);
		}

		bool InPlace(RuleSymbol rule)
		{
			_plan ??= ExecutionPlan.Of(graph);

			return _plan.CompiledInPlace.Contains(rule) && rule.OnFail is null && !graph.Says.ContainsKey(rule);
		}

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

	/// <summary>The automaton of a repetition was past <see cref="Building"/>, or had more letters than <see cref="Longest"/>.</summary>
	sealed class Exhausted : Exception;

	/// <summary>A call being written out had more letters than <see cref="Positions"/>, and is one letter instead.</summary>
	sealed class Overlong : Exception;

	/// <summary>The letters and counted edges of one repetition.</summary>
	sealed class Automaton(Context context, RuleSymbol rule)
	{
		readonly HashSet<RuleSymbol> _inlining = [rule];

		/// <summary>The letters; the first is the start, which reads nothing.</summary>
		public List<Letter> Letters { get; } = [new Letter(null, -1, "")];

		/// <summary>Every edge in the order it was found, an edge offered twice found twice.</summary>
		public List<(int From, int To, int Ways)> Edges { get; } = [];

		public int Opaque { get; private set; }

		/// <summary>The work done building the automaton (<see cref="Building"/>).</summary>
		public int Work { get; private set; }

		/// <summary>How many calls are being written out, each to be taken back where it does not fit.</summary>
		int _attempts;

		/// <summary>How many answers of <see cref="OneClass"/> were cut short by the path or the depth, so far.</summary>
		int _cuts;

		public Context Context => context;

		void Charge(int work)
		{
			Work += work;

			if (Work > Building)
				throw new Exhausted();
		}

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
					return Opaqued(element.ToString(), element.ToString(), element);

				case Node.Sequence(var parts):
					return Sequence(parts);

				case Node.Choice { Selection: not null } chosen:
					return Opaqued(chosen, "switch", chosen);

				case Node.Choice(var alternatives):
				{
					var first = new List<(int, int)>();
					var last  = new List<(int, int, FirstSets.First?)>();
					var empty = new List<(int, FirstSets.First?)>();

					foreach (var alternative in alternatives)
					{
						var one = Of(alternative);

						Charge(one.First.Count + one.Last.Count + one.Empty.Count);

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
					return Braced(Opaqued(atomic, "{…}", atomic), atomic.Body);

				case Node.Same same:
					return Opaqued(same.Binding, same.ToString(), same);

				case Node.External external:
					return Opaqued("@" + external.Name, external.ToString(), external);

				case Node.Counted counted:
					return Opaqued(counted, counted.ToString(), counted);

				case Node.Capture(_, var body):    return Of(body);
				case Node.Marked(var body, _):     return Of(body);
				case Node.Construct(var body, _):  return Of(body);
				case Node.Bind(_, var body, _):    return Of(body);

				case Node.Call call:
					return Called(call);

				// A look that is one class holds what may follow it; any other is taken as reading nothing.
				case Node.Lookahead(false, var body) when OneClass(body) is { } one:
					return Fragment.Look(one);

				// `?=eof`: what the look inside it refuses, the look outside refuses too.
				case Node.Lookahead(true, var body) when Unwrapped(body) is Node.Lookahead(false, var inner) && OneClass(inner) is { } one:
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

			FirstSets.First? refused  = null;
			FirstSets.First? wanted   = null;
			FirstSets.First? notAfter = null;

			for (var at = 0; at < parts.Count; at++)
			{
				var part = parts[at];

				switch (part)
				{
					// The trivia woven between two operands is a seam, GRAM4051's, and reads nothing
					// here. One the author joined with `~` is no seam, and is read as any call is:
					// `(trivia ~ 'a')*` over `trivia = 'a'*` is `('a'* & 'a')*`.
					case Node.Call(var called, _) when context.Seams.Contains(called) &&
						!(at > 0 && parts[at - 1] is Node.Glue) && !(at + 1 < parts.Count && parts[at + 1] is Node.Glue):
						continue;

					case Node.Behind(var test):
						notAfter = Joined(notAfter, FirstSets.OfElement(test));
						continue;

					case Node.Lookahead(false, var body) when OneClass(body) is { } one:
						refused = refused is null ? one : refused.Or(one);
						continue;

					case Node.Lookahead(true, var body) when Unwrapped(body) is Node.Lookahead(false, var inner) && OneClass(inner) is { } one:
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

				if (notAfter is not null)
					Behind(next, letters, edges, notAfter);

				notAfter = null;

				if (refused is not null || wanted is not null)
				{
					Narrowed(next, letters, edges, refused, wanted);

					// Where the operand can read nothing, the look holds what follows it too.
					var held = Joined(refused, wanted is null ? null : Outside(wanted));

					Charge(next.Empty.Count);

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

		/// <summary>What a call to a rule without arguments stands for, through every such call: <c>eof</c> is a look.</summary>
		Node Unwrapped(Node node)
		{
			var seen = new HashSet<RuleSymbol>();

			while (node is Node.Call(var called, { Count: 0 }) && seen.Add(called) && context.Graph.Bodies.TryGetValue(called, out var body))
				node = body;

			return node;
		}

		/// <summary>
		/// A braced group that ends with a run of one class read to its end: what follows it cannot
		/// begin with that class, since the run would have taken it. <c>{ [^ '\n']* }</c> is
		/// followed by a line end or by nothing.
		/// </summary>
		Fragment Braced(Fragment opaque, Node body)
		{
			var last = body;

			while (true)
			{
				switch (last)
				{
					case Node.Sequence(var parts) when parts.Count > 0:
						last = parts[parts.Count - 1];
						continue;

					case Node.Capture(_, var held):
						last = held;
						continue;

					case Node.Marked(var held, _):
						last = held;
						continue;

					case Node.Construct(var held, _):
						last = held;
						continue;

					case Node.Bind(_, var held, _):
						last = held;
						continue;
				}

				break;
			}

			if (last is not Node.Repeat { Max: null } run || OneClass(run.Body) is not { } taken)
				return opaque;

			Charge(opaque.Last.Count);

			return opaque with { Last = [.. opaque.Last.Select(end => (end.Letter, end.Ways, Joined(end.Refused, taken)))] };
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

			return (Letters[letter].Reads ?? Letters[letter].Begins) is { } reads
				? refused.Covers(reads)
				: refused.Covers(FirstSets.First.Chars([new CharRange(char.MinValue, char.MaxValue)]));
		}

		/// <summary>One more way to read nothing, counted with the others held to the same look.</summary>
		void Merge(List<(int Ways, FirstSets.First? Refused)> routes, int ways, FirstSets.First? refused)
		{
			Charge(routes.Count + 1);

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
			Charge(Edges.Count - edges + next.First.Count);

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

		/// <summary>The letters an operand begins with, held to what may not stand before them.</summary>
		void Behind(Fragment next, int letters, int edges, FirstSets.First notAfter)
		{
			Charge(Edges.Count - edges + next.First.Count);

			var returned = new HashSet<int>();

			for (var i = edges; i < Edges.Count; i++)
				returned.Add(Edges[i].To);

			foreach (var (at, _) in next.First)
				if (at >= letters && !returned.Contains(at))
					Letters[at] = Letters[at] with { NotAfter = notAfter };
		}

		/// <summary>The characters a look asks about, where it is one class: a class, a character, or a choice of them, at most <c>any</c> after it.</summary>
		FirstSets.First? OneClass(Node body)
		{
			Charge(1);

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
				// Kept once known, for every repetition of the grammar: a rule reached by many paths
				// is asked once. An answer cut short by the path or the depth is not kept.
				case Node.Call(var called, { Count: 0 }) when context.Graph.Bodies.TryGetValue(called, out var rule):
				{
					if (context.Classes.TryGetValue(called, out var known))
						return known;

					if (_inlining.Contains(called) || _inlining.Count > Deepest)
					{
						_cuts++;

						return null;
					}

					var cuts = _cuts;

					_inlining.Add(called);

					var one = OneClass(rule);

					_inlining.Remove(called);

					if (one is not null || _cuts == cuts)
						context.Classes[called] = one;

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

			// Past the calls a walk follows, one letter: a chain of thousands of calls that read
			// nothing would otherwise be followed one frame each.
			if (call.Arguments.Count > 0 || !graph.Bodies.TryGetValue(called, out var body) ||
				graph.Calls.Recurses(called) || _inlining.Contains(called) || _inlining.Count > Deepest ||
				context.OverKinds && !called.IsBuiltIn && !called.GivesBack)
				return Opaqued(called, called.Name, call);

			if (body is Node.Atomic(var braced))
				return Braced(Opaqued(called, called.Name, call), braced);

			if (Letters.Count >= Positions)
				return Opaqued(called, called.Name, call);

			// Inlined, and taken back if it does not fit.
			var letters = Letters.Count;
			var edges   = Edges.Count;
			var opaque  = Opaque;

			_inlining.Add(called);
			_attempts++;

			try
			{
				return Of(body);
			}
			catch (Overlong)
			{
				// Taken back below: the innermost call being written out is the one that does not
				// fit, and every call begins below the letters a call may reach.
			}
			finally
			{
				_inlining.Remove(called);
				_attempts--;
			}

			Letters.RemoveRange(letters, Letters.Count - letters);
			Edges.RemoveRange(edges, Edges.Count - edges);
			Opaque = opaque;

			return Opaqued(called, called.Name, call);
		}

		Fragment Repeated(Node.Repeat repeat)
		{
			var (body, min, max) = (repeat.Body, repeat.Min, repeat.Max);

			if (max == 0)
				return Fragment.Nothing;

			// Too wide to write out, and not a run that may go on: one opaque letter, which is the
			// side that tells too little. `any{4096}` repeated cuts one way.
			if (max is > Unrolled)
				return Opaqued(repeat, repeat.ToString(), repeat);

			// Written out turn by turn, where it is narrow: `X{2,3}` is `X & X & X?`.
			if (max is { } most)
			{
				var fragment = Fragment.Nothing;

				for (var turn = 0; turn < min; turn++)
					fragment = Then(fragment, Turn(body));

				// The optional turns nest, each one only after the one before: `(X & X?)?`.
				var optional = Fragment.Nothing;

				for (var turn = min; turn < most; turn++)
				{
					var one = Turn(body);

					optional = Then(one, optional);

					Charge(optional.Empty.Count);

					var empty = new List<(int, FirstSets.First?)>(optional.Empty);

					Merge(empty, 1, null);

					optional = optional with { Empty = empty };
				}

				return Then(fragment, optional);
			}

			// An unbounded repetition: the end of a turn leads back to its beginning. A turn that
			// reads nothing ends it, so an empty turn is not another way round.
			var turns = Turn(body);

			Link(turns.Last, turns.First);

			if (min > 0)
				return turns;

			var none = new List<(int, FirstSets.First?)>(turns.Empty);

			Merge(none, 1, null);

			return turns with { Empty = none };
		}

		/// <summary>
		/// One turn of a repetition: a body the emitter writes as one test of a character is one
		/// letter, however many alternatives it has.
		/// </summary>
		Fragment Turn(Node body)
		{
			return context.OneCharacter(body, Step) && Union(body) is { } reads
				? Leaf(new Letter(reads, -1, ""))
				: Of(body);
		}

		/// <summary>One node of a walk visited, charged to the budget.</summary>
		bool Step()
		{
			Charge(1);

			return true;
		}

		/// <summary>
		/// The characters a body that is one test reads, or null where some of them are the host's
		/// to know: walked with a stack of its own, each rule's body once, every node charged.
		/// </summary>
		FirstSets.First? Union(Node body)
		{
			var all     = FirstSets.First.None;
			var pending = new Stack<Node>();
			var entered = new HashSet<RuleSymbol>();

			pending.Push(body);

			while (pending.Count > 0)
			{
				Charge(1);

				switch (pending.Pop())
				{
					case Node.Element { References.Count: 0 } element:
						all = all.Or(FirstSets.OfElement(element));
						break;

					case Node.Literal { Text.Length: 1 } literal:
						all = all.Or(FirstSets.First.Chars([new CharRange(literal.Text[0], literal.Text[0])]));
						break;

					case Node.Call(var called, _) when context.Graph.Bodies.TryGetValue(called, out var inlined):
						if (entered.Add(called))
							pending.Push(inlined);

						break;

					case Node.Sequence([var only]):
						pending.Push(only);
						break;

					case Node.Choice(var alternatives):
						foreach (var alternative in alternatives)
							pending.Push(alternative);

						break;

					default:
						return null;
				}
			}

			return all;
		}

		Fragment Then(Fragment before, Fragment after)
		{
			Charge(
				before.First.Count + after.Last.Count +
				before.Empty.Count * (after.First.Count + after.Empty.Count) + after.Empty.Count * before.Last.Count);

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
			Charge(from.Count * to.Count);

			foreach (var (one, ways, refused) in from)
				foreach (var (other, more) in to)
					if (!Blocked(refused, other) && !(Letters[other].NotAfter is { } notAfter && Letters[one].Reads is { } reads && notAfter.Covers(reads)))
						Edges.Add((one, other, Ways(ways * more)));
		}

		Fragment Leaf(Letter letter)
		{
			Charge(1);

			// Past the letters a repetition may have, it is not told; past those a call written out
			// may add, the call is one letter instead (Called).
			if (Letters.Count >= Longest)
				throw new Exhausted();

			if (_attempts > 0 && Letters.Count >= Positions)
				throw new Overlong();

			// A letter that reads nothing at all can never be read: trimmed here, before the product.
			if (letter.Reads is { Nothing: true })
				return Fragment.Never;

			Letters.Add(letter);

			var at = Letters.Count - 1;

			return new Fragment([(at, 1)], [(at, 1, null)], []);
		}

		/// <summary>
		/// One letter for something read whole: it overlaps only itself, and what it begins with,
		/// where that is known, holds what a look or a braced run before it lets stand there.
		/// </summary>
		Fragment Opaqued(object key, string name, Node node)
		{
			Opaque++;

			return Leaf(new Letter(null, context.Numbered(key), name, Begins: Begins(node)));
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
					// Two readings part on the way into the exponential part as well as inside it:
					// `((X | Y) & 'z')*` reads `'x' 'z'` two ways on its first turn.
					var inside = _exponential[_component[to]];
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
