using System;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// The keywords a lexer looks up in a table once it has read a whole word, instead of carrying
/// a path for each of them in its automaton.
/// </summary>
/// <remarks>
/// <para>
/// Over characters a literal made of word characters may not be followed by a character that
/// continues a word (§4.6). A lexer that reads the word first has that for free: the word is
/// the longest run of characters the boundary names, a keyword is a word that is in the table,
/// and a keyword inside a longer word is never found. What stays in the automaton are the
/// patterns that are not such words — names, numbers, strings, symbols, and the literals whose
/// spelling the table cannot hold — so the automaton is the one the grammar would have without
/// its keywords, a fraction of the size.
/// </para>
/// <para>
/// A token is still what the full automaton would have said. The kind of a text is the set of
/// patterns that match it exactly; the automaton without the keywords says that set without
/// the keyword in it, and <see cref="Kinds"/> says which kind it is with the keyword added.
/// The longest match is kept: a keyword wins only where nothing the automaton read is longer
/// than the word, and wins alone where nothing it read is as long.
/// </para>
/// <para>
/// A keyword is tabled only where the table answers as the literal does: it is written under
/// the grammar's one boundary, no syntax reads it with none, all of its characters (and, for a
/// literal that ignores case, every case it reads) continue a word, and an ignored-case literal
/// is ASCII, whose case partners are the one symmetric relation there is. The rest stay in the
/// automaton and are asked about the word after them there, as before.
/// </para>
/// </remarks>
public sealed class KeywordTable
{
	KeywordTable(
		Node.Element boundary,
		bool ignoreCase,
		IReadOnlyList<string> keys,
		LexicalAutomaton machine,
		IReadOnlyList<int> numbers,
		IReadOnlyList<int> contexts,
		IReadOnlyList<int> kinds,
		IReadOnlyList<int> direct)
	{
		Boundary   = boundary;
		IgnoreCase = ignoreCase;
		Keys       = keys;
		Machine    = machine;
		Numbers    = numbers;
		Contexts   = contexts;
		Kinds      = kinds;
		Direct     = direct;
	}

	/// <summary>What continues a word: the grammar's boundary, which every tabled keyword was written under.</summary>
	public Node.Element Boundary { get; }

	/// <summary>Whether the keys are matched with ASCII case ignored. They are kept in upper case then.</summary>
	public bool IgnoreCase { get; }

	/// <summary>The keywords, as they are compared, in ordinal order; a key's index is its number in the tables.</summary>
	public IReadOnlyList<string> Keys { get; }

	/// <summary>The automaton of the patterns that are not tabled. Their numbers are the inventory's.</summary>
	public LexicalAutomaton Machine { get; }

	/// <summary>By index into <see cref="LexicalAutomaton.Sets"/> of <see cref="Machine"/>, the kind that set is.</summary>
	public IReadOnlyList<int> Numbers { get; }

	/// <summary>
	/// The kinds the automaton can give a text that is also a keyword, 0 first for none: what the
	/// rest of the patterns say of it.
	/// </summary>
	public IReadOnlyList<int> Contexts { get; }

	/// <summary>
	/// By key and context (<c>key * Contexts.Count + context</c>), the kind a keyword is with what
	/// the automaton said of it; 0 where no text makes that pair.
	/// </summary>
	public IReadOnlyList<int> Kinds { get; }

	/// <summary>
	/// By key, the kind it is when the automaton need not be asked: the same whatever cases its
	/// letters are in, and nothing the automaton reads goes on past the word. 0 where it must be asked.
	/// </summary>
	public IReadOnlyList<int> Direct { get; }

	/// <summary>The length of the shortest key.</summary>
	public int Shortest
	{
		get { return Keys.Min(static key => key.Length); }
	}

	/// <summary>The length of the longest key.</summary>
	public int Longest
	{
		get { return Keys.Max(static key => key.Length); }
	}

	/// <summary>The key a spelling is looked up by: its ASCII letters in upper case where case is ignored.</summary>
	public static string Canonical(string text, bool ignoreCase)
	{
		return ignoreCase ? text.ToUpperInvariant() : text;
	}

	/// <summary>
	/// The table of a grammar's keywords and the automaton of the rest, or null where there is
	/// nothing to table or the automaton without them cannot be built. Kinds the automaton alone can
	/// give are appended to <paramref name="kinds"/> where they are not there yet.
	/// </summary>
	/// <param name="graph">For what the rules a boundary names hold.</param>
	/// <param name="patterns">The inventory's patterns.</param>
	/// <param name="shapes">What each pattern recognizes, as the automaton reads it.</param>
	/// <param name="kinds">The kinds the full automaton makes, the first <paramref name="machineKinds"/> of them.</param>
	/// <param name="machineKinds">How many of <paramref name="kinds"/> the full automaton made.</param>
	/// <param name="boundaryOf">The boundary a keyword was written under: null where none, or several that differ.</param>
	/// <param name="blocked">What refused, appended to.</param>
	internal static KeywordTable? Of(
		RecognitionGraph graph,
		IReadOnlyList<TerminalInventory.Pattern> patterns,
		IReadOnlyList<Node?> shapes,
		List<TerminalInventory.Kind> kinds,
		int machineKinds,
		Func<TerminalInventory.Pattern.Word, Node.Element?> boundaryOf,
		List<string> blocked)
	{
		var candidates = new List<(TerminalInventory.Pattern.Word Word, Node.Element Boundary)>();

		foreach (var word in patterns.OfType<TerminalInventory.Pattern.Word>())
			if (boundaryOf(word) is { } boundary)
				candidates.Add((word, boundary));

		if (candidates.Count == 0)
			return null;

		// The grammar's boundary is the one most keywords were written under; a keyword written
		// under another, or under none, stays in the automaton.
		var counted = new List<(Node.Element Boundary, int Count)>();

		foreach (var (_, boundary) in candidates)
		{
			var at = counted.FindIndex(one => Same(one.Boundary, boundary));

			if (at < 0)
				counted.Add((boundary, 1));
			else
				counted[at] = (counted[at].Boundary, counted[at].Count + 1);
		}

		var chosen = counted.OrderByDescending(static one => one.Count).First().Boundary;
		var continuing = FirstSets.Of(chosen, graph);

		if (!continuing.IsKnown)
			return null;

		var fits = candidates
			.Where(one => Same(one.Boundary, chosen) && Continues(one.Word, continuing))
			.Select(static one => one.Word)
			.ToList();

		// One table for one way of comparing: the larger of the two kinds, the other staying where
		// it is.
		var ignoring = fits.Count(static one => one.IgnoreCase) >= fits.Count(static one => !one.IgnoreCase);
		var tabled   = fits.Where(one => one.IgnoreCase == ignoring).ToList();

		if (tabled.Count == 0)
			return null;

		var left     = new HashSet<int>(tabled.Select(static one => one.Index));
		var machine  = LexicalAutomaton.Of(graph, [.. shapes.Select(static one => one!)], blocked, left);

		if (machine is null)
			return null;

		var keyOf = tabled.ToDictionary(static one => one.Index, one => Canonical(one.Text, ignoring));
		var keys  = keyOf.Values.Distinct().OrderBy(static one => one, StringComparer.Ordinal).ToList();

		// What the automaton says of a text is the patterns it matches but the tabled ones. Every
		// such set is a kind; one that no text makes alone is appended, after the others.
		int KindOf(IEnumerable<int> set)
		{
			var wanted = set.OrderBy(static one => one).ToList();

			if (wanted.Count == 0)
				return 0;

			foreach (var kind in kinds)
				if (kind.Matched.Count == wanted.Count && kind.Matched.Select(static one => one.Index).OrderBy(static one => one).SequenceEqual(wanted))
					return kind.Number;

			kinds.Add(new TerminalInventory.Kind(kinds.Count + 1, [.. wanted.Select(one => patterns[one])]));

			return kinds.Count;
		}

		var numbers = machine.Sets.Select(set => KindOf(set)).ToList();

		var contexts = new List<int> { 0 };
		var made     = new List<(int Key, int Context, int Kind)>();

		for (var at = 0; at < machineKinds; at++)
		{
			var kind   = kinds[at];
			var inside = kind.Matched.Where(one => left.Contains(one.Index)).ToList();

			if (inside.Count == 0)
				continue;

			var spelled = inside.Select(one => keyOf[one.Index]).Distinct().ToList();

			if (spelled.Count != 1)
				return null;

			var context = KindOf(kind.Matched.Where(one => !left.Contains(one.Index)).Select(static one => one.Index));

			if (!contexts.Contains(context))
				contexts.Add(context);

			made.Add((keys.BinarySearch(spelled[0], StringComparer.Ordinal), context, kind.Number));
		}

		// A context is a byte of the table of kinds, with one bit for the whole-word kinds and one value for none.
		if (contexts.Count > 126)
			return null;

		var table = new int[keys.Count * contexts.Count];

		foreach (var (key, context, number) in made)
			table[key * contexts.Count + contexts.IndexOf(context)] = number;

		// Whether the automaton need be asked about a keyword at all. It is read over every case the
		// keyword's letters can be in at once, as a set of states: if the states it can be in at the
		// word's end agree on what they accept, and none goes on past a character that does not
		// continue a word, then what the rest of the patterns say of the word is the same whatever its
		// cases, and the keyword's kind is a constant of the keyword.
		var direct = new int[keys.Count];

		for (var key = 0; key < keys.Count; key++)
			if (Alone(machine, keys[key], ignoring, continuing, numbers) is { } context &&
				contexts.IndexOf(context) is >= 0 and var at)
				direct[key] = table[key * contexts.Count + at];

		return new KeywordTable(chosen, ignoring, keys, machine, numbers, contexts, table, direct);
	}

	/// <summary>
	/// What the automaton makes of a keyword's text, the kind of the set it accepts there or 0 for none,
	/// when that is the same for every case of its letters and nothing it reads goes on from there past a
	/// character that does not continue a word; null where it is not.
	/// </summary>
	static int? Alone(LexicalAutomaton machine, string key, bool ignoreCase, FirstSets.First continuing, IReadOnlyList<int> numbers)
	{
		var states = new HashSet<int> { 0 };

		foreach (var c in key)
		{
			var next = new HashSet<int>();

			foreach (var state in states)
				foreach (var one in ignoreCase ? CaseFold.Of(c) : [c])
					if (Step(machine, state, one) is >= 0 and var to)
						next.Add(to);

			states = next;
		}

		// A character that does not continue a word may follow, or the text may end.
		foreach (var state in states)
			for (var atom = 0; atom < machine.Atoms.Count; atom++)
				if (machine.Next[state][atom] >= 0 && !continuing.Covers(FirstSets.First.Chars([machine.Atoms[atom]])))
					return null;

		var accepted = states.Select(state => machine.Accepts[state]).Distinct().ToList();

		return accepted.Count switch
		{
			0 => 0,
			1 => accepted[0] < 0 ? 0 : numbers[accepted[0]],
			_ => null,
		};
	}

	/// <summary>The state a character leads to from a state, or -1.</summary>
	static int Step(LexicalAutomaton machine, int state, char c)
	{
		var low  = 0;
		var high = machine.Atoms.Count - 1;

		while (low <= high)
		{
			var middle = (low + high) >> 1;
			var atom   = machine.Atoms[middle];

			if (c < atom.From)
				high = middle - 1;
			else if (c > atom.To)
				low = middle + 1;
			else
				return machine.Next[state][middle];
		}

		return -1;
	}

	/// <summary>Whether a keyword is one the table answers for as the literal does.</summary>
	static bool Continues(TerminalInventory.Pattern.Word word, FirstSets.First continuing)
	{
		if (word.Text.Length == 0)
			return false;

		foreach (var c in word.Text)
		{
			if (word.IgnoreCase && c > 0x7F)
				return false;

			var spellings = word.IgnoreCase ? CaseFold.Of(c) : [c];

			foreach (var one in spellings)
				if (!continuing.Covers(FirstSets.First.Chars([new CharRange(one, one)])))
					return false;
		}

		return true;
	}

	static bool Same(Node.Element left, Node.Element right)
	{
		return left.IsNegated == right.IsNegated &&
			left.Ranges.SequenceEqual(right.Ranges) &&
			left.Categories.SequenceEqual(right.Categories) &&
			left.References.SequenceEqual(right.References);
	}
}
