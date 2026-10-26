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
		IReadOnlyList<IReadOnlyList<int>> sets,
		int nondeterministic)
	{
		Atoms            = atoms;
		Next             = next;
		Accepts          = accepts;
		Sets             = sets;
		Nondeterministic = nondeterministic;
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
	/// How many states the nondeterministic machine the construction began with had, and how many
	/// more it met owing a delimiter (see <c>Delimited</c>): what building it cost, which the
	/// deterministic machine it became does not show.
	/// </summary>
	public int Nondeterministic { get; }

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
	/// <param name="exact">
	/// Which patterns are a token as the automaton reads them, with nothing to read them again:
	/// those may not hold an until the machine can only read wider than the rule (see
	/// <c>Delimited</c>). Null where every pattern is a beginning or a question some other reading
	/// answers, and the wider reading is the safe direction.
	/// </param>
	public static LexicalAutomaton? Of(
		RecognitionGraph graph, IReadOnlyList<Node> patterns, List<string> blocked, Func<int, bool>? exact = null)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		if (patterns is null)
			throw new ArgumentNullException(nameof(patterns));

		return new Builder(graph, blocked, exact).Run(patterns);
	}

	sealed class Builder(RecognitionGraph graph, List<string> blocked, Func<int, bool>? exact)
	{
		/// <summary>Whether the pattern being built must be read exactly.</summary>
		bool _exact;

		/// <summary>The patterns that must be read exactly, by number.</summary>
		readonly HashSet<int> _exacting = [];

		/// <summary>
		/// The delimiter of an until an exact pattern accepted a token still owing, or null: that
		/// token's reading is decided past it, so the pattern is not exact after all.
		/// </summary>
		string? _unsettled;

		// ── The machine under construction ───────────────────────────────────────────
		//
		// States are numbers. An edge is either an atom edge or an empty one, and both are
		// kept as lists indexed by the state they leave, which is all a subset construction
		// reads and all a Thompson construction writes.
		readonly List<List<(int Atom, int To)>> _on    = [];
		readonly List<List<int>>                _empty = [];
		readonly Dictionary<int, int>           _accepts = [];

		// The ways out of a run that hand on the prefixes of its delimiter still matched there
		// (Delimited): an empty edge that leaves owing them, read by the subset construction.
		readonly Dictionary<int, List<(int To, string Delimiter, ulong Matched)>> _handed = [];

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
				_exact = exact?.Invoke(i) ?? false;

				if (_exact)
					_exacting.Add(i);

				if (Build(patterns[i], []) is not var (from, to))
					return null;

				_empty[start].Add(from);
				_accepts[to] = i;
			}

			var machine = Subsets(start);

			if (_unsettled is { } delimiter)
			{
				Refuse($"an until whose delimiter can begin inside its run and end past the token: {delimiter}");

				return null;
			}

			return machine;
		}

		// ── The alphabet ─────────────────────────────────────────────────────────────

		/// <summary>Every set a pattern tests, so the partition can be taken over them.</summary>
		bool Gather(Node node, HashSet<RuleSymbol> inside)
		{
			switch (node)
			{
				// A pattern weaves no trivia, so `~` inside one asks about a gap that could
				// not have been there: accepted, and worth nothing.
				case Node.Empty or Node.Guard or Node.Glue:
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
						return Refuse(
							$"{called.Name} reaches itself, so it is not a lexical pattern. Begin the terminal " +
							"with what the lexer can read and end it with that rule or `@M` (§7.1), or, for a " +
							"pair that nests, end it with `Std.Nested(open, close)`");

					if (!graph.Bodies.TryGetValue(called, out var body))
						return Refuse($"{called.Name} has no body to read");

					var reached = Gather(body, inside);

					inside.Remove(called);

					return reached;

				case Node.Lookahead:
					return Refuse($"a lookahead inside a pattern: {node}");

				case Node.Repeat(var turns, _, _) when Until(node) is var (delimiter, _):
					// The delimiter's characters and the escape's, each as a set of its own,
					// so that the partition cuts at every one of them — the until machine
					// tells them apart by atom.
					foreach (var c in delimiter)
						_sets.Add(Folded(c, false));

					var item = turns is Node.Sequence([_, var consumed]) ? consumed : turns;

					// An escape pair or one character: the pair's own lookahead is the
					// machine's business, not a shape to refuse.
					if (Escaping(item) is var (escape, escaped, plain))
					{
						_sets.Add(Folded(escape, false));

						return Gather(escaped, inside) && Gather(plain, inside);
					}

					return Gather(item, inside);

				case Node.Behind:
					return Refuse($"a look-behind inside a pattern: {node}");

				case Node.Reading:
					return Refuse($"a test of the reading inside a pattern: {node}");

				case Node.External(var name):
					return Refuse($"an external recognizer inside a pattern: @{name}");

				case Node.Sequence(var parts):
					foreach (var part in parts)
						if (!Gather(part, inside))
							return false;

					return true;

				case Node.Choice { Selection: not null }:
					return false;

				case Node.Choice(var alternatives):
					foreach (var alternative in alternatives)
						if (!Gather(alternative, inside))
							return false;

					return true;

				case Node.Repeat(var turns, _, _):
					return Gather(turns, inside);
				case Node.Atomic(var kept):              return Gather(kept, inside);
				case Node.Marked(var kept, _):           return Gather(kept, inside);
				case Node.Capture(_, var held):          return Gather(held, inside);
				case Node.Construct(var made, _):        return Gather(made, inside);

				// A binding records what its body read, and the body is a regular language
				// where it is one; recording is the business of the machine that reads the
				// reference, which is never this one — a reference is refused below with the
				// rest of what no automaton reads.
				case Node.Bind(_, var bound, _):         return Gather(bound, inside);

				default:
					return Refuse($"a shape a pattern cannot have: {node.GetType().Name}");
			}
		}

		/// <summary>What a character of a literal accepts: <see cref="CaseFold"/> where it ignores case.</summary>
		static FirstSets.First Folded(char c, bool ignoreCase)
		{
			return ignoreCase
				? FirstSets.First.Chars(CaseFold.Ranges(c))
				: FirstSets.First.Chars([new CharRange(c, c)]);
		}

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
		/// <remarks>
		/// One walk over both, both in order: the atoms are disjoint and ascending, and so are a
		/// known set's ranges. It asked <c>Overlaps</c> of a set made of each atom in turn, which
		/// was a list, an array and a set for every atom of the alphabet on every edge built.
		/// </remarks>
		List<int> Atoms(FirstSets.First set)
		{
			var held = new List<int>();

			if (set.Nothing)
				return held;

			if (set.Anything)
			{
				for (var i = 0; i < _atoms.Count; i++)
					held.Add(i);

				return held;
			}

			var ranges = set.Ranges;
			var at     = 0;

			for (var i = 0; i < _atoms.Count; i++)
			{
				while (at < ranges.Count && ranges[at].To < _atoms[i].From)
					at++;

				if (at == ranges.Count)
					break;

				if (ranges[at].From <= _atoms[i].To)
					held.Add(i);
			}

			return held;
		}

		// ── Thompson ─────────────────────────────────────────────────────────────────

		(int From, int To)? Build(Node node, HashSet<RuleSymbol> inside)
		{
			switch (node)
			{
				case Node.Empty or Node.Guard or Node.Glue:
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

					for (var i = 0; i < parts.Count; i++)
					{
						var part = parts[i];

						// An until and the delimiter that closes it are read as one machine: what the
						// lookahead sees past the run is the delimiter, and nothing else is.
						var closed =
							Until(part) is var (delimiter, _) &&
							i + 1 < parts.Count &&
							parts[i + 1] is Node.Literal(var next) { IgnoreCase: false } &&
							next == delimiter;

						var read = closed && Until(part) is var (stop, item)
							? Delimited(stop, item, inside, closed: true)
							: Build(part, inside) is { } alone ? (alone, false) : null;

						if (read is not ((var one, var end), var absorbed))
							return null;

						_empty[at].Add(one);
						at = end;

						if (absorbed)
							i++;
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

				case Node.Repeat when Until(node) is var (delimiter, item):
					return Delimited(delimiter, item, inside, closed: false) is (var delimited, _) ? delimited : null;

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
				case Node.Bind(_, var bound, _):  return Build(bound, inside);

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
		/// The delimiter and the item of <c>(?!L &amp; X)*</c>, or null where a node is not it.
		/// </summary>
		/// <remarks>
		/// "Everything up to a delimiter", which is how every language writes the inside of a
		/// comment and how <c>.gram</c> writes it too: <c>BlockComment = "/*" &amp; (?!"*/"
		/// &amp; any)* &amp; "*/"</c>. The language is regular — strings in which the
		/// delimiter does not occur — but it is not one a Thompson construction reaches by
		/// following the shape, because the shape is a lookahead and a lookahead is not one of
		/// its three cases. So the idiom is recognized and built, and anything else wearing a
		/// lookahead is still refused.
		/// </remarks>
		static (string Delimiter, Node Item)? Until(Node node)
		{
			return node is Node.Repeat(
				Node.Sequence([Node.Lookahead(false, Node.Literal(var delimiter) { IgnoreCase: false }), var item]),
				0,
				null) &&
			delimiter.Length > 0
				? (delimiter, item)
				: null;
		}

		/// <summary>
		/// The machine for "an item at a time, and the delimiter never begins here", and whether
		/// it read the delimiter that closes the run too.
		/// </summary>
		/// <remarks>
		/// <para>
		/// One state a length of matched delimiter prefix — Knuth, Morris and Pratt's automaton.
		/// The lookahead is asked at every item boundary and sees past what the run consumed, so
		/// the run stops where the delimiter first <em>begins</em>, which an automaton reading the
		/// run alone cannot know: what follows decides it. Three cases are exact by their shape, and
		/// the rest are read with what the run hands on (the last paragraph); a pattern that must be
		/// exact and still owes where a token ends is refused, so the grammar keeps the character
		/// machine rather than a token that accepts more than the rule. A beginning, which its own
		/// rule reads again from the token's start, keeps the wider reading: there it only lets the
		/// rule be asked.
		/// </para>
		/// <para>
		/// Where the delimiter follows (<paramref name="closed"/>) and every one of its characters
		/// is an item, the run and the delimiter are read as one machine whose completed delimiter
		/// is its end: the strings in which the delimiter occurs exactly once, as their suffix.
		/// That is the rule's reading — the run stops at the first beginning, and the delimiter is
		/// what is there — including a delimiter that overlaps itself, where the first beginning
		/// may lie inside a later occurrence: <c>(?!"aa" &amp; any)* &amp; "aa"</c> reads <c>aa</c>
		/// of <c>aaa</c>.
		/// </para>
		/// <para>
		/// Where the delimiter follows but some of its characters are not items, the run is read
		/// as the strings the delimiter does not occur in and the delimiter after it — the two
		/// agree only where the delimiter cannot overlap itself, so that an occurrence cannot begin
		/// inside the run and end inside the delimiter. And a delimiter of one character is exact
		/// wherever the run stands, since an occurrence of it lies wholly in the run or not in it.
		/// </para>
		/// <para>
		/// Anywhere else an occurrence may begin inside the run and end past it, which the strings
		/// the delimiter does not occur in miss: <c>(?!"ab" &amp; ['a'|'b'])* &amp; 'b'</c> refuses
		/// <c>ab</c>, whose run stops before the <c>a</c>, and the run alone reads the <c>a</c>. So
		/// a state a length of matched prefix leaves the run owing that prefix and its borders, and
		/// the subset construction reads what follows with the debt: a character that finishes an
		/// owed occurrence is the delimiter begun inside the run, and no way on. What is still owed
		/// where the token ends is decided past the token: a beginning lets it go, the wider reading,
		/// and a pattern that must be exact is refused (Subsets). A delimiter of sixty-four characters
		/// or more, whose prefixes a debt cannot hold, is refused in a pattern that must be exact as
		/// before.
		/// </para>
		/// </remarks>
		((int From, int To) Built, bool Absorbed)? Delimited(string delimiter, Node item, HashSet<RuleSymbol> inside, bool closed)
		{
			if (Escaping(item) is var (escape, escaped, plain))
			{
				if (delimiter.Length != 1)
				{
					Refuse($"an escape inside an until whose delimiter is more than one character: {delimiter}");

					return null;
				}

				return Delimited(delimiter[0], escape, escaped, plain, inside) is { } pair ? (pair, false) : null;
			}

			var over = Consumed(item, inside);

			if (over is null)
			{
				Refuse($"an until whose item is not one character: {item}");

				return null;
			}

			var length = delimiter.Length;
			var atoms  = Atoms(over);

			// Whether each of the delimiter's characters is an item, so the one machine can read
			// the delimiter with the run: an atom is either exactly one of the delimiter's
			// characters or holds none of them, because each of those characters was added to the
			// alphabet as a set of its own and the partition cuts at every boundary.
			var absorbs = closed && delimiter.All(c => atoms.Any(atom => _atoms[atom].From == c && _atoms[atom].To == c));

			// Only where what the run hands on cannot be carried (below); otherwise the subset
			// construction says whether an exact pattern ever ends a token still owing it.
			if (_exact && !absorbs && length >= 64 && (!closed || Overlaps(delimiter)))
			{
				Refuse(
					closed
						? $"an until whose delimiter overlaps itself and is not all items: {delimiter}"
						: $"an until of more than one character that its delimiter does not follow: {delimiter}");

				return null;
			}

			var states = new int[length];

			for (var i = 0; i < length; i++)
				states[i] = State();

			var to = State();

			// Whether an occurrence begun inside the run can end it in what follows: never where the
			// one machine reads the delimiter with the run, or where the delimiter is one character,
			// or one that cannot overlap itself follows the run.
			var owing = !absorbs && length is > 1 and < 64 && (!closed || Overlaps(delimiter));

			if (!absorbs)
				for (var state = 0; state < length; state++)
					if (state == 0 || !owing)
						_empty[states[state]].Add(to);

			// The prefix function, and then the failure links read off it. Written the
			// standard way rather than in one pass, because the one-pass version this began
			// as gave `*/` a link from one matched character back to one matched character —
			// and a link to itself is a loop the builder never leaves.
			var prefix = new int[length];

			for (int i = 1, matched = 0; i < length; i++)
			{
				while (matched > 0 && delimiter[i] != delimiter[matched])
					matched = prefix[matched - 1];

				if (delimiter[i] == delimiter[matched])
					matched++;

				prefix[i] = matched;
			}

			// The prefixes still matched where the run ends in each state: the state's own length,
			// and the borders of it — one bit a length.
			if (owing)
				for (var state = 1; state < length; state++)
				{
					var matches = 0UL;

					for (var held = state; held > 0; held = prefix[held - 1])
						matches |= 1UL << held;

					_handed[states[state]] = [(to, delimiter, matches)];
				}

			// How much of the delimiter is still matched once the next character is not the
			// one that would continue it. Strictly less than what came in, which is what makes
			// the walk below finish.
			int Fail(int state)
			{
				return state == 0 ? 0 : prefix[state - 1];
			}

			int Delta(int state, char? c)
			{
				while (true)
				{
					if (c is { } one && state < length && delimiter[state] == one)
						return state + 1;

					if (state == 0)
						return 0;

					state = Fail(state);
				}
			}

			for (var state = 0; state < length; state++)
				foreach (var atom in atoms)
				{
					var here = _atoms[atom];

					var next = Delta(
						state,
						here.From == here.To && delimiter.IndexOf(here.From) >= 0 ? here.From : null);

					// Completing the delimiter ends the run: read with it, that is the end of
					// the pair; read alone, it is the one thing the run may not do, so the way
					// there is not written and the machine simply stops.
					if (next < length)
						_on[states[state]].Add((atom, states[next]));
					else if (absorbs)
						_on[states[state]].Add((atom, to));
				}

			return ((states[0], to), absorbs);
		}

		/// <summary>
		/// The prefixes of <paramref name="delimiter"/> still matched once <paramref name="atom"/>
		/// is read after <paramref name="matched"/>, or null where it finishes one.
		/// </summary>
		static ulong? Continued(string delimiter, ulong matched, CharRange atom)
		{
			// An atom is one of the delimiter's characters exactly or holds none of them.
			if (atom.From != atom.To)
				return 0;

			var still = 0UL;

			for (var held = 1; held < delimiter.Length; held++)
			{
				if ((matched & (1UL << held)) == 0 || delimiter[held] != atom.From)
					continue;

				if (held + 1 == delimiter.Length)
					return null;

				still |= 1UL << (held + 1);
			}

			return still;
		}

		/// <summary>Whether a proper prefix of <paramref name="delimiter"/> is also its suffix.</summary>
		static bool Overlaps(string delimiter)
		{
			for (var k = 1; k < delimiter.Length; k++)
				if (string.CompareOrdinal(delimiter, 0, delimiter, delimiter.Length - k, k) == 0)
					return true;

			return false;
		}

		/// <summary>What one turn of an "until" consumes, or null where it consumes no one item.</summary>
		FirstSets.First? Consumed(Node node, HashSet<RuleSymbol> inside)
		{
			switch (node)
			{
				case Node.Element element:
					return FirstSets.OfElement(element);

				case Node.Literal(var text) one when text.Length == 1:
					return Folded(text[0], one.IgnoreCase);

				case Node.Call(var called, _) when graph.Bodies.TryGetValue(called, out var body):
				{
					// Guarded against a rule reaching itself, and the guard taken back off:
					// left on, `any` stayed "inside" after the first until of a pattern, and
					// the second — the tail of a doubled-quote string — read as no one item.
					if (!inside.Add(called))
						return null;

					var over = Consumed(body, inside);

					inside.Remove(called);

					return over;
				}

				default:
					return null;
			}
		}

		/// <summary>
		/// The parts of an item that is an escape pair or one character —
		/// <c>(escape &amp; X | Y)</c>, or <c>(escape &amp; X | ?!escape &amp; Y)</c> — or
		/// null where the item is not that shape.
		/// </summary>
		/// <remarks>
		/// The second spelling is the one to write. Over characters a repetition hands back
		/// what it read when what follows fails, and <c>(escape &amp; any | any)</c> on
		/// <c>"a\"</c> then reads the escape alone and the quote after it as the end of the
		/// string — a reading the machine here, which takes the pair whole, does not have.
		/// With <c>?!escape</c> on the plain alternative there is nothing to hand back to,
		/// and the two readings are one. Here both spellings are the same machine: an escape
		/// character is taken as a pair from the boundary, whatever the plain alternative
		/// says about it.
		/// </remarks>
		static (char Escape, Node Escaped, Node Plain)? Escaping(Node item)
		{
			if (item is not Node.Choice([
				Node.Sequence([Node.Literal(var escape) { IgnoreCase: false }, var escaped]),
				var plain,
			]) || escape.Length != 1)
				return null;

			// Only a guard that refuses exactly the escape character: `?!'a'i` refuses `A` too, which
			// neither alternative then reads, and dropping it would read `A` as plain.
			if (plain is Node.Sequence([Node.Lookahead(false, Node.Literal(var same) guard), var inner]) &&
				same == escape &&
				(!guard.IgnoreCase || CaseFold.Ranges(same[0]) is [var only] && only == new CharRange(same[0], same[0])))
				plain = inner;

			return (escape[0], escaped, plain);
		}

		/// <summary>
		/// The machine for "an escape pair or one character at a time, and the delimiter
		/// never begins here" — the inside of a string with escapes.
		/// </summary>
		/// <remarks>
		/// <para>
		/// One delimiter character, or nothing: the lookahead is asked at item boundaries
		/// and an escape pair moves the boundary two characters at once, so a delimiter of
		/// several characters may begin inside a pair and never be seen, and may begin at a
		/// boundary and run through one — neither of which a machine that forgets where the
		/// boundaries were can tell. With one character the boundary is every position the
		/// pair does not cover, and the machine is exact: at a boundary the delimiter stops,
		/// the escape takes the next character whatever it is, and anything else is one
		/// character.
		/// </para>
		/// <para>
		/// The lookahead comes first, as it does in the grammar: an escape character that is
		/// also the delimiter ends the string rather than escaping anything.
		/// </para>
		/// </remarks>
		(int From, int To)? Delimited(char delimiter, char escape, Node escaped, Node plain, HashSet<RuleSymbol> inside)
		{
			var afterEscape = Consumed(escaped, inside);
			var one         = Consumed(plain, inside);

			if (afterEscape is null || one is null)
			{
				Refuse("an escape inside an until whose item is not one character");

				return null;
			}

			var boundary = State();
			var pair     = State();
			var to       = State();

			_empty[boundary].Add(to);

			foreach (var atom in Atoms(one))
			{
				var here = _atoms[atom];

				if (here.From == here.To && here.From == delimiter)
					continue;

				_on[boundary].Add((atom, here.From == here.To && here.From == escape ? pair : boundary));
			}

			foreach (var atom in Atoms(Folded(escape, false)))
			{
				var here = _atoms[atom];

				if (!(here.From == here.To && here.From == delimiter))
					_on[boundary].Add((atom, pair));
			}

			foreach (var atom in Atoms(afterEscape))
				_on[pair].Add((atom, boundary));

			return (boundary, to);
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
			var numbered = new Dictionary<int[], int>(Members.Same);
			var pending  = new Queue<(HashSet<int> States, int Number)>();
			var sets     = new List<IReadOnlyList<int>>();
			var named    = new Dictionary<string, int>();
			var accepts  = new List<int>();
			var rows     = new List<int[]>();

			var first = Closed([start], []);

			// Kept across the states rather than made for each: most of what a set reaches on an
			// atom closes to a state already numbered, and then everything built to find that out
			// was thrown away at once — a set per atom, a set per closure, a stack per closure.
			var moves = new Dictionary<int, HashSet<int>>();
			var free  = new Stack<HashSet<int>>();
			var spare = (HashSet<int>?)null;

			numbered[Key(first)] = 0;
			pending.Enqueue((first, 0));
			Room(rows, accepts, 0);

			while (pending.Count > 0)
			{
				var (here, number) = pending.Dequeue();

				var accepting = here
					.Select(one => Owing(one).State)
					.Where(_accepts.ContainsKey)
					.Select(one => _accepts[one])
					.Distinct()
					.ToList();

				// A token an exact pattern ends owing nothing on another way is decided here; one it
				// ends only owing is decided by what comes after it, which no automaton reads.
				if (_unsettled is null && _owing.Count > 0)
					foreach (var one in here)
						if (Owing(one) is (var state, var owed and not 0) &&
							_accepts.TryGetValue(state, out var pattern) &&
							_exacting.Contains(pattern) &&
							!here.Any(other => other < _on.Count && _accepts.TryGetValue(other, out var plain) && plain == pattern))
							_unsettled = _debts[owed][0].Delimiter;

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

				// The edges of this set read once and bucketed by the atom they leave on,
				// rather than once per atom with almost all of them thrown away. The loop
				// this replaces was `atoms × states × edges` where the work is `states ×
				// edges`, and an alphabet of a few hundred ranges made that the whole cost
				// of cutting a grammar in two.
				foreach (var reached in moves.Values)
				{
					reached.Clear();
					free.Push(reached);
				}

				moves.Clear();

				foreach (var one in here)
				{
					var (state, owed) = Owing(one);

					foreach (var (on, to) in _on[state])
					{
						var reading = to;

						if (owed != 0)
						{
							var still = After(owed, on);

							if (still < 0)
								continue;

							reading = Element(to, still);
						}

						if (!moves.TryGetValue(on, out var reached))
							moves[on] = reached = free.Count > 0 ? free.Pop() : [];

						reached.Add(reading);
					}
				}

				// In atom order still, because a row is indexed by atom and the states are
				// numbered in the order they are first reached — which is what `Ordered`
				// below and every snapshot beside it were written against.
				for (var atom = 0; atom < _atoms.Count; atom++)
				{
					if (!moves.TryGetValue(atom, out var next))
						continue;

					var closed = Closed(next, spare ??= []);
					var key    = Key(closed);

					if (!numbered.TryGetValue(key, out var to2))
					{
						numbered[key] = to2 = numbered.Count;

						Room(rows, accepts, to2);
						pending.Enqueue((closed, to2));

						// Kept by the queue now, so the next closure needs a set of its own.
						spare = null;
					}

					rows[number][atom] = to2;
				}
			}

			return Ordered(_atoms, rows, accepts, sets, _on.Count + _owing.Count);
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
			List<IReadOnlyList<int>> sets,
			int nondeterministic)
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

			return new LexicalAutomaton(atoms, rows, accepts, [.. order.Select(one => sets[one])], nondeterministic);
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

		/// <summary>What <see cref="Closed"/> walks with, one stack for every closure.</summary>
		readonly Stack<int> _closing = new();

		/// <summary>The states reached from these without reading, written into <paramref name="reached"/>.</summary>
		HashSet<int> Closed(IEnumerable<int> states, HashSet<int> reached)
		{
			var pending = _closing;

			reached.Clear();
			pending.Clear();

			foreach (var one in states)
				pending.Push(one);

			while (pending.Count > 0)
			{
				var one = pending.Pop();

				if (!reached.Add(one))
					continue;

				var (state, owed) = Owing(one);

				foreach (var to in _empty[state])
					pending.Push(Element(to, owed));

				if (_handed.TryGetValue(state, out var handed))
					foreach (var (to, delimiter, matched) in handed)
						pending.Push(Element(to, Owe(owed, delimiter, matched)));
			}

			// A state reached owing nothing reads everything it reads owing something, and more:
			// a debt only takes ways away. Kept, the debts multiplied every subset by what each run
			// hands on, since an empty run lets every later run's start be reached both ways.
			if (_owing.Count > 0)
				reached.RemoveWhere(one => one >= _on.Count && reached.Contains(_owing[one - _on.Count].State));

			return reached;
		}

		// ── What a run hands on ─────────────────────────────────────────────────────
		//
		// The subset construction reads elements: a state of the machine, and what it owes — for
		// each delimiter a run handed on, the prefixes of it matched since. An element that owes
		// nothing is numbered as its state, so a machine no run hands anything to is built as it
		// always was; one that owes is numbered after every state, the first time it is met.
		// Building the machine of what follows once for every debt instead copied the copies of
		// every later until in it: ten untils of a twelve-character delimiter in a row came to a
		// million and a half states for a machine of thirteen.

		readonly List<(int State, int Owed)>             _owing     = [];
		readonly Dictionary<(int State, int Owed), int>  _elements  = [];
		readonly List<(string Delimiter, ulong Matched)[]> _debts   = [[]];
		readonly Dictionary<string, int>                 _debtNames = [];
		readonly Dictionary<(int Owed, int Atom), int>   _after     = [];
		readonly Dictionary<(int Owed, string Delimiter, ulong Matched), int> _owed = [];

		/// <summary>The state an element is of, and the debt it carries (0 for none).</summary>
		(int State, int Owed) Owing(int element)
		{
			return element < _on.Count ? (element, 0) : _owing[element - _on.Count];
		}

		/// <summary>The element of a state carrying a debt.</summary>
		int Element(int state, int owed)
		{
			if (owed == 0)
				return state;

			if (!_elements.TryGetValue((state, owed), out var element))
			{
				_elements[(state, owed)] = element = _on.Count + _owing.Count;
				_owing.Add((state, owed));
			}

			return element;
		}

		/// <summary>
		/// A debt with <paramref name="matched"/> prefixes of <paramref name="delimiter"/> added to
		/// it. Two runs of one delimiter owe one set of prefixes: each prefix grows or is dropped on
		/// its own, and finishing any of them ends the reading.
		/// </summary>
		int Owe(int owed, string delimiter, ulong matched)
		{
			if (_owed.TryGetValue((owed, delimiter, matched), out var known))
				return known;

			var debt = new List<(string Delimiter, ulong Matched)>(_debts[owed]);
			var at   = debt.FindIndex(one => string.CompareOrdinal(one.Delimiter, delimiter) >= 0);

			if (at < 0)
				debt.Add((delimiter, matched));
			else if (debt[at].Delimiter == delimiter)
				debt[at] = (delimiter, debt[at].Matched | matched);
			else
				debt.Insert(at, (delimiter, matched));

			return _owed[(owed, delimiter, matched)] = Debt(debt);
		}

		/// <summary>What a debt is once <paramref name="atom"/> is read, or -1 where it finishes an occurrence.</summary>
		int After(int owed, int atom)
		{
			if (_after.TryGetValue((owed, atom), out var after))
				return after;

			var debt = new List<(string Delimiter, ulong Matched)>();

			foreach (var (delimiter, matched) in _debts[owed])
			{
				if (Continued(delimiter, matched, _atoms[atom]) is not { } still)
					return _after[(owed, atom)] = -1;

				if (still != 0)
					debt.Add((delimiter, still));
			}

			return _after[(owed, atom)] = Debt(debt);
		}

		/// <summary>The number of a debt, numbered the first time it is met; 0 is owing nothing.</summary>
		int Debt(List<(string Delimiter, ulong Matched)> debt)
		{
			if (debt.Count == 0)
				return 0;

			// The length first, so that no delimiter, whatever it holds, reads as two.
			var name = string.Join(
				"\n",
				debt.Select(static one => FormattableString.Invariant($"{one.Matched} {one.Delimiter.Length} {one.Delimiter}")));

			if (!_debtNames.TryGetValue(name, out var number))
			{
				_debtNames[name] = number = _debts.Count;
				_debts.Add([.. debt]);
			}

			return number;
		}

		/// <summary>A subset as its members in ascending order, which is what identifies it.</summary>
		/// <remarks>
		/// It was a comma-joined string of the sorted members, and then a bit per state of the
		/// whole nondeterministic machine. The string cost an allocation and a walk of its text;
		/// the bits cost a word per sixty-four states on every transition — hundreds of words
		/// for T-SQL, hashed and compared each time, where a subset holds a few dozen members.
		/// The members themselves are as long as the subset and no longer.
		/// </remarks>
		static int[] Key(HashSet<int> states)
		{
			var key = new int[states.Count];

			states.CopyTo(key);
			Array.Sort(key);

			return key;
		}

		/// <summary>Two subsets are the same when they hold the same members.</summary>
		sealed class Members : IEqualityComparer<int[]>
		{
			public static readonly Members Same = new();

			public bool Equals(int[]? left, int[]? right)
			{
				if (ReferenceEquals(left, right))
					return true;

				if (left is null || right is null || left.Length != right.Length)
					return false;

				for (var i = 0; i < left.Length; i++)
					if (left[i] != right[i])
						return false;

				return true;
			}

			public int GetHashCode(int[] key)
			{
				var hash = 17;

				foreach (var one in key)
					hash = hash * 31 + one;

				return hash;
			}
		}

		bool Refuse(string reason)
		{
			if (!blocked.Contains(reason))
				blocked.Add(reason);

			return false;
		}
	}
}
