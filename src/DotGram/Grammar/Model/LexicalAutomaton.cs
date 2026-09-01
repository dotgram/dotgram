using System;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// One machine over all the patterns at once, and the sets of them its states accept.
/// </summary>
/// <remarks>
/// <para>
/// This is what tells a lexer from a pile of recognizers: the patterns are not tried in
/// turn, they are read together, and a state that accepts says <em>which</em> of them
/// accepted there. Those sets are the kinds — <c>10</c> is <c>{Digits,
/// UnsignedNumericLiteral}</c>, <c>1.5</c> is <c>{UnsignedNumericLiteral}</c> — and the
/// construction answers exactly the question the inventory was answering from witnesses:
/// which sets can actually occur.
/// </para>
/// <para>
/// Thompson and then a subset construction, over an alphabet of atoms rather than of
/// characters. The atoms are the coarsest partition every pattern's element sets are a
/// union of, so <c>\p{L}</c> costs one symbol per interval it already had rather than one
/// per letter, and the machine is the same size whether the grammar is written for ASCII or
/// for Unicode.
/// </para>
/// <para>
/// <b>What it refuses.</b> A pattern is a regular language or it is not a pattern. A
/// lookahead, an external recognizer or a rule that reaches itself is none of the three
/// shapes a Thompson construction has, and rather than approximate them this says so and
/// the grammar keeps the character machine. `BlockComment`'s <c>(?!"*/" &amp; any)*</c> is
/// the shape that would come up, and it is trivia — which is skipped rather than tokenized,
/// so it is never a pattern.
/// </para>
/// </remarks>
public sealed class LexicalAutomaton
{
	LexicalAutomaton(
		IReadOnlyList<CharRange> atoms,
		IReadOnlyList<int[]> next,
		IReadOnlyList<int> accepts,
		IReadOnlyList<IReadOnlyList<int>> sets)
	{
		Atoms   = atoms;
		Next    = next;
		Accepts = accepts;
		Sets    = sets;
	}

	/// <summary>The alphabet: disjoint character ranges, in order.</summary>
	public IReadOnlyList<CharRange> Atoms { get; }

	/// <summary>One row a state, one column an atom; -1 where there is no way on.</summary>
	public IReadOnlyList<int[]> Next { get; }

	/// <summary>What each state accepts, as an index into <see cref="Sets"/>, or -1.</summary>
	public IReadOnlyList<int> Accepts { get; }

	/// <summary>The distinct sets of patterns some string makes accept — the kinds.</summary>
	public IReadOnlyList<IReadOnlyList<int>> Sets { get; }

	/// <summary>
	/// What leaves a state: one entry a target, holding the characters that lead there.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The atoms are how the machine was built and not how it is written. A state inside an
	/// identifier has one way on — itself, for every letter, digit and underscore — and the
	/// alphabet says that in three hundred atoms because a keyword's letters cut it. Grouped
	/// by target it is one test again, over the set it was written as.
	/// </para>
	/// <para>
	/// Measured before it was chosen. <c>SqlStandard92</c> is 528 states over 897 atoms, so a
	/// dense table is 473,616 cells and out of the question; merging atoms that neighbour each
	/// other leaves 186,342 tests, because the atoms alternate; grouping by target leaves
	/// <b>1,034</b>, forty-three at the widest state.
	/// </para>
	/// </remarks>
	public IReadOnlyList<(IReadOnlyList<CharRange> On, int To)> From(int state)
	{
		var grouped = new Dictionary<int, List<CharRange>>();
		var row     = Next[state];

		for (var atom = 0; atom < row.Length; atom++)
		{
			if (row[atom] < 0)
				continue;

			if (!grouped.TryGetValue(row[atom], out var ranges))
				grouped[row[atom]] = ranges = [];

			ranges.Add(Atoms[atom]);
		}

		return
		[
			.. grouped
				.OrderBy(one => one.Key)
				.Select(one => ((IReadOnlyList<CharRange>)FirstSets.First.Normalized(one.Value), one.Key)),
		];
	}

	/// <summary>
	/// The machine, or null where one of the patterns is not a regular language.
	/// </summary>
	/// <param name="graph">For the bodies calls reach.</param>
	/// <param name="patterns">One node a pattern, in the order the caller numbers them.</param>
	/// <param name="blocked">What refused, appended to.</param>
	public static LexicalAutomaton? Of(
		RecognitionGraph graph, IReadOnlyList<Node> patterns, List<string> blocked)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		if (patterns is null)
			throw new ArgumentNullException(nameof(patterns));

		return new Builder(graph, blocked).Run(patterns);
	}

	sealed class Builder(RecognitionGraph graph, List<string> blocked)
	{
		// ── The machine under construction ───────────────────────────────────────────
		//
		// States are numbers. An edge is either an atom edge or an empty one, and both are
		// kept as lists indexed by the state they leave, which is all a subset construction
		// reads and all a Thompson construction writes.
		readonly List<List<(int Atom, int To)>> _on    = [];
		readonly List<List<int>>                _empty = [];
		readonly Dictionary<int, int>           _accepts = [];

		readonly List<FirstSets.First> _sets = [];
		IReadOnlyList<CharRange>       _atoms = [];

		int State()
		{
			_on.Add([]);
			_empty.Add([]);

			return _on.Count - 1;
		}

		public LexicalAutomaton? Run(IReadOnlyList<Node> patterns)
		{
			// Two passes over the patterns: the first to learn the alphabet, the second to
			// build over it. A Thompson edge is labelled with an atom, and an atom cannot be
			// known until every set that has to be partitioned has been seen.
			foreach (var pattern in patterns)
				if (!Gather(pattern, []))
					return null;

			_atoms = Partition(_sets);

			var start = State();

			for (var i = 0; i < patterns.Count; i++)
			{
				if (Build(patterns[i], []) is not var (from, to))
					return null;

				_empty[start].Add(from);
				_accepts[to] = i;
			}

			return Subsets(start);
		}

		// ── The alphabet ─────────────────────────────────────────────────────────────

		/// <summary>Every set a pattern tests, so the partition can be taken over them.</summary>
		bool Gather(Node node, HashSet<RuleSymbol> inside)
		{
			switch (node)
			{
				case Node.Empty or Node.Guard:
					return true;

				case Node.Literal(var text) literal:
					foreach (var c in text)
						_sets.Add(Folded(c, literal.IgnoreCase));

					return true;

				case Node.Element element:
					_sets.Add(FirstSets.OfElement(element));

					return true;

				case Node.Call(var called, _):
					if (!inside.Add(called))
						return Refuse($"{called.Name} reaches itself, so it is not a lexical pattern");

					if (!graph.Bodies.TryGetValue(called, out var body))
						return Refuse($"{called.Name} has no body to read");

					var reached = Gather(body, inside);

					inside.Remove(called);

					return reached;

				case Node.Lookahead:
					return Refuse($"a lookahead inside a pattern: {node}");

				case Node.Behind:
					return Refuse($"a look-behind inside a pattern: {node}");

				case Node.External(var name):
					return Refuse($"an external recognizer inside a pattern: @{name}");

				case Node.Sequence(var parts):
					foreach (var part in parts)
						if (!Gather(part, inside))
							return false;

					return true;

				case Node.Choice(var alternatives):
					foreach (var alternative in alternatives)
						if (!Gather(alternative, inside))
							return false;

					return true;

				case Node.Repeat(var turns, _, _):       return Gather(turns, inside);
				case Node.Atomic(var kept):              return Gather(kept, inside);
				case Node.Marked(var kept, _):           return Gather(kept, inside);
				case Node.Capture(_, var held):          return Gather(held, inside);
				case Node.Construct(var made, _):        return Gather(made, inside);

				default:
					return Refuse($"a shape a pattern cannot have: {node.GetType().Name}");
			}
		}

		/// <summary>What an ignore-case character accepts.</summary>
		/// <remarks>
		/// Both cases and nothing more. Ordinal folding also ties U+017F to <c>S</c>, and
		/// leaving that out narrows a pattern by one character that no grammar here writes;
		/// what it could cost is an overlap gone unnoticed, and an overlap between two
		/// patterns that differ only in the long s is not one either of them meant.
		/// </remarks>
		static FirstSets.First Folded(char c, bool ignoreCase) =>
			ignoreCase
				? FirstSets.First.Chars(
					[new CharRange(Char.ToUpperInvariant(c), Char.ToUpperInvariant(c)),
					 new CharRange(Char.ToLowerInvariant(c), Char.ToLowerInvariant(c))])
				: FirstSets.First.Chars([new CharRange(c, c)]);

		/// <summary>
		/// The coarsest ranges every set is a union of.
		/// </summary>
		/// <remarks>
		/// Cut at every boundary any set has, and what is between two neighbouring cuts is
		/// inside all the same sets — so it can be one symbol however many characters it
		/// holds. This is what keeps a Unicode category from costing tens of thousands of
		/// transitions: <c>\p{L}</c> arrives as a few hundred intervals and leaves as a few
		/// hundred symbols.
		/// </remarks>
		static IReadOnlyList<CharRange> Partition(IReadOnlyList<FirstSets.First> sets)
		{
			var cuts = new SortedSet<int>();

			foreach (var set in sets)
				foreach (var range in set.Ranges)
				{
					cuts.Add(range.From);
					cuts.Add(range.To + 1);
				}

			var atoms  = new List<CharRange>();
			var edges  = cuts.ToList();

			for (var i = 0; i + 1 <= edges.Count - 1; i++)
				if (edges[i] <= char.MaxValue)
					atoms.Add(new CharRange((char)edges[i], (char)Math.Min(edges[i + 1] - 1, char.MaxValue)));

			if (edges.Count > 0 && edges[^1] <= char.MaxValue)
				atoms.Add(new CharRange((char)edges[^1], char.MaxValue));

			return atoms;
		}

		/// <summary>The atoms a set holds.</summary>
		List<int> Atoms(FirstSets.First set)
		{
			var held = new List<int>();

			for (var i = 0; i < _atoms.Count; i++)
				if (set.Overlaps(FirstSets.First.Chars([_atoms[i]])))
					held.Add(i);

			return held;
		}

		// ── Thompson ─────────────────────────────────────────────────────────────────

		(int From, int To)? Build(Node node, HashSet<RuleSymbol> inside)
		{
			switch (node)
			{
				case Node.Empty or Node.Guard:
				{
					var one = State();

					return (one, one);
				}

				case Node.Literal(var text) literal:
				{
					var from = State();
					var at   = from;

					foreach (var c in text)
						at = Step(at, Folded(c, literal.IgnoreCase));

					return (from, at);
				}

				case Node.Element element:
				{
					var from = State();

					return (from, Step(from, FirstSets.OfElement(element)));
				}

				case Node.Sequence(var parts):
				{
					var from = State();
					var at   = from;

					foreach (var part in parts)
					{
						if (Build(part, inside) is not var (one, end))
							return null;

						_empty[at].Add(one);
						at = end;
					}

					return (from, at);
				}

				case Node.Choice(var alternatives):
				{
					var from = State();
					var to   = State();

					foreach (var alternative in alternatives)
					{
						if (Build(alternative, inside) is not var (one, end))
							return null;

						_empty[from].Add(one);
						_empty[end].Add(to);
					}

					return (from, to);
				}

				case Node.Repeat(var body, var min, var max):
					return Repeated(body, min, max, inside);

				case Node.Call(var called, _):
				{
					if (!inside.Add(called) || !graph.Bodies.TryGetValue(called, out var body))
						return null;

					var built = Build(body, inside);

					inside.Remove(called);

					return built;
				}

				case Node.Atomic(var kept):       return Build(kept, inside);
				case Node.Marked(var kept, _):    return Build(kept, inside);
				case Node.Capture(_, var held):   return Build(held, inside);
				case Node.Construct(var made, _): return Build(made, inside);

				default:
					return null;
			}
		}

		int Step(int from, FirstSets.First set)
		{
			var to = State();

			foreach (var atom in Atoms(set))
				_on[from].Add((atom, to));

			return to;
		}

		/// <summary>
		/// A repetition, written out to its floor and then looped or unrolled to its ceiling.
		/// </summary>
		/// <remarks>
		/// A bounded repetition is copies, which is the only way an automaton can count. The
		/// cap is what stops <c>X{1,4000}</c> from being four thousand copies of <c>X</c>;
		/// nothing in a lexical rule is written that way, and a rule that is says so rather
		/// than being quietly built into something enormous.
		/// </remarks>
		(int From, int To)? Repeated(Node body, int min, int? max, HashSet<RuleSymbol> inside)
		{
			if ((max ?? min) > Copies || min > Copies)
			{
				Refuse($"a repetition of more than {Copies} turns inside a pattern: {min}..{max}");

				return null;
			}

			var from = State();
			var at   = from;

			for (var turn = 0; turn < min; turn++)
			{
				if (Build(body, inside) is not var (one, end))
					return null;

				_empty[at].Add(one);
				at = end;
			}

			if (max is null)
			{
				// The loop: into the body, back to where it started, and past it.
				if (Build(body, inside) is not var (loop, back))
					return null;

				var to = State();

				_empty[at].Add(loop);
				_empty[at].Add(to);
				_empty[back].Add(loop);
				_empty[back].Add(to);

				return (from, to);
			}

			var last = State();

			_empty[at].Add(last);

			for (var turn = min; turn < max; turn++)
			{
				if (Build(body, inside) is not var (one, end))
					return null;

				_empty[at].Add(one);
				at = end;

				_empty[at].Add(last);
			}

			return (from, last);
		}

		const int Copies = 64;

		// ── The subset construction ──────────────────────────────────────────────────

		/// <summary>
		/// The deterministic machine, and the accepting sets that are the kinds.
		/// </summary>
		/// <remarks>
		/// Both, because they are one construction: the sets are what the accepting states
		/// carry, and the states are what a lexer runs. Working either out on its own would be
		/// working the other out twice.
		/// </remarks>
		LexicalAutomaton Subsets(int start)
		{
			var numbered = new Dictionary<string, int>();
			var pending  = new Queue<(HashSet<int> States, int Number)>();
			var sets     = new List<IReadOnlyList<int>>();
			var named    = new Dictionary<string, int>();
			var accepts  = new List<int>();
			var rows     = new List<int[]>();

			var first = Closed([start]);

			numbered[Key(first)] = 0;
			pending.Enqueue((first, 0));
			Room(rows, accepts, 0);

			while (pending.Count > 0)
			{
				var (here, number) = pending.Dequeue();

				var accepting = here.Where(_accepts.ContainsKey).Select(one => _accepts[one]).ToList();

				if (accepting.Count > 0)
				{
					accepting.Sort();

					var key = string.Join(",", accepting);

					if (!named.TryGetValue(key, out var which))
					{
						named[key] = which = sets.Count;
						sets.Add(accepting);
					}

					accepts[number] = which;
				}

				for (var atom = 0; atom < _atoms.Count; atom++)
				{
					var next = new HashSet<int>();

					foreach (var one in here)
						foreach (var (on, to) in _on[one])
							if (on == atom)
								next.Add(to);

					if (next.Count == 0)
						continue;

					var closed = Closed(next);
					var key    = Key(closed);

					if (!numbered.TryGetValue(key, out var to2))
					{
						numbered[key] = to2 = numbered.Count;

						Room(rows, accepts, to2);
						pending.Enqueue((closed, to2));
					}

					rows[number][atom] = to2;
				}
			}

			return Ordered(_atoms, rows, accepts, sets);
		}

		/// <summary>
		/// The sets renumbered by the patterns they hold, and the states pointed at the new
		/// numbers.
		/// </summary>
		/// <remarks>
		/// The subset construction meets its states in whatever order the alphabet takes it,
		/// and that is not the order the patterns are in — while the caller has arranged the
		/// patterns so that a named set is one run of them, which only survives if the kinds
		/// follow the patterns. So they are sorted by the lowest pattern each set holds, which
		/// puts a word's kind in word order and the wider sets after.
		///
		/// Here and not in the caller. Doing it there left two numberings — the caller's and
		/// the one the emitted scanner printed — and forty-six inputs became nineteen
		/// disagreements the moment the scanner was generated rather than written by hand.
		/// </remarks>
		static LexicalAutomaton Ordered(
			IReadOnlyList<CharRange> atoms,
			IReadOnlyList<int[]> rows,
			List<int> accepts,
			List<IReadOnlyList<int>> sets)
		{
			var order = Enumerable.Range(0, sets.Count)
				.OrderBy(one => sets[one][0])
				.ThenBy(one => sets[one].Count)
				.ThenBy(one => string.Join(",", sets[one]))
				.ToList();

			var moved = new int[sets.Count];

			for (var to = 0; to < order.Count; to++)
				moved[order[to]] = to;

			for (var state = 0; state < accepts.Count; state++)
				if (accepts[state] >= 0)
					accepts[state] = moved[accepts[state]];

			return new LexicalAutomaton(atoms, rows, accepts, [.. order.Select(one => sets[one])]);
		}

		void Room(List<int[]> rows, List<int> accepts, int number)
		{
			while (rows.Count <= number)
			{
				var row = new int[_atoms.Count];

				for (var i = 0; i < row.Length; i++)
					row[i] = -1;

				rows.Add(row);
				accepts.Add(-1);
			}
		}

		HashSet<int> Closed(IEnumerable<int> states)
		{
			var reached = new HashSet<int>();
			var pending = new Stack<int>(states);

			while (pending.Count > 0)
			{
				var one = pending.Pop();

				if (!reached.Add(one))
					continue;

				foreach (var to in _empty[one])
					pending.Push(to);
			}

			return reached;
		}

		static string Key(HashSet<int> states) =>
			string.Join(",", states.OrderBy(one => one));

		bool Refuse(string reason)
		{
			if (!blocked.Contains(reason))
				blocked.Add(reason);

			return false;
		}
	}
}
