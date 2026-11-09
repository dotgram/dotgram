using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using System.Threading;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// Whether the lexical automata a compile builds may be taken from, and kept for, the other
/// compiles of the process (<see cref="LexicalAutomaton"/>).
/// </summary>
enum AutomatonReuse
{
	/// <summary>Every automaton is built afresh, and none is kept.</summary>
	Off,

	/// <summary>
	/// An automaton kept for the same patterns is taken; a large one built is kept.
	/// </summary>
	On,

	/// <summary>
	/// As <see cref="On"/>, and an automaton taken is built afresh as well and held to the kept
	/// one: where the two differ, something a build reads is missing from the key it was kept
	/// under, and the compile fails saying so.
	/// </summary>
	Verify,
}

sealed partial class LexicalAutomaton
{
	/// <summary>
	/// The automata built in this process, kept for the next compile that asks for one of them.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A grammar's lexical patterns are compiled again far more often than they change: by the
	/// other target framework of the same project, by every fixture that compiles the same grammar
	/// with other options (a trace build, a tape reading, an older framework), by a build after an
	/// edit to C# whose parser the compile cache let go, and in an editor by every edit that leaves
	/// the terminals as they were. The automaton is the largest single thing such a compile builds
	/// and a function of the patterns alone, so it is kept by them. Like the generator's compile
	/// cache, nothing is on disk: it lives as long as the process that loaded this assembly.
	/// </para>
	/// <para>
	/// Bounded by what the machines hold, because one is large: T-SQL's is nine thousand states
	/// over more than a thousand atoms, a row of four bytes for each, and the ways out the emitter
	/// reads off them (<see cref="From"/>) half as much again. Ninety-six megabytes holds the three
	/// SQL grammars of the repository, the largest it has, with room beside them; and only a
	/// machine whose construction walked enough to be worth the room is kept (<see cref="Worth"/>),
	/// so the hundreds of small machines a grammar builds to ask whether one terminal is regular do
	/// not push it out.
	/// </para>
	/// </remarks>
	static readonly Shelf Kept = new(budget: 96L << 20, capacity: 32);

	/// <summary>
	/// How many states a construction walks (<see cref="Walked"/>) before its machine is worth
	/// keeping: a hundredth of what T-SQL's walks, and a few milliseconds of building.
	/// </summary>
	internal const long Worth = 131_072;

	/// <summary>
	/// <see cref="Of"/>: the machine built, or the one an earlier build in this process made of the
	/// same patterns.
	/// </summary>
	/// <remarks>
	/// Only a machine is kept: a refusal is not, since nothing was built and its reasons name
	/// what refused; and a build that ran out of room (<see cref="Exceeded"/>) throws as it did. A
	/// refusal and a machine never come of one key, because a build that refuses appends a reason
	/// and one that builds appends none.
	/// </remarks>
	static LexicalAutomaton? Reused(
		RecognitionGraph   graph,
		IReadOnlyList<Node> patterns,
		List<string>       blocked,
		Func<int, bool>?   exact)
	{
		var reuse = graph.ReuseAutomata;

		if (reuse == AutomatonReuse.Off || Key.Of(graph, patterns, exact) is not { } key)
			return new Builder(graph, blocked, exact).Run(patterns);

		if (Kept.TryGet(key, out var kept))
		{
			if (reuse == AutomatonReuse.Verify)
				Verified(key, kept, graph, patterns, exact);

			return kept;
		}

		var built = new Builder(graph, blocked, exact).Run(patterns);

		if (built is not null && built.Walked >= Worth)
			Kept.Add(key, built);

		return built;
	}

	/// <summary>
	/// A kept machine held to one built afresh for the same patterns: where the two differ,
	/// something a build reads is missing from <see cref="Key"/>, and that is a defect to fail on
	/// rather than a machine to use.
	/// </summary>
	static void Verified(
		string              key,
		LexicalAutomaton    kept,
		RecognitionGraph    graph,
		IReadOnlyList<Node> patterns,
		Func<int, bool>?    exact)
	{
		LexicalAutomaton? fresh;

		try
		{
			fresh = new Builder(graph, [], exact).Run(patterns);
		}
		catch (Exceeded)
		{
			fresh = null;
		}

		if (fresh is not null && Same(kept, fresh))
			return;

		Kept.Remove(key);

		throw new InvalidOperationException(
			"The lexical automaton kept from an earlier compile differs from one built afresh " +
			"for the same patterns: something the build reads is missing from the key it was " +
			"kept under (LexicalAutomaton.Key).");
	}

	/// <summary>Whether two machines are the same in everything they say.</summary>
	internal static bool Same(LexicalAutomaton left, LexicalAutomaton right)
	{
		if (left.Nondeterministic != right.Nondeterministic || left.Walked != right.Walked ||
			left.Atoms.Count != right.Atoms.Count || left.Next.Count != right.Next.Count ||
			left.Sets.Count != right.Sets.Count)
			return false;

		for (var i = 0; i < left.Atoms.Count; i++)
			if (left.Atoms[i] != right.Atoms[i])
				return false;

		for (var state = 0; state < left.Next.Count; state++)
		{
			if (left.Accepts[state] != right.Accepts[state])
				return false;

			if (!left.Next[state].AsSpan().SequenceEqual(right.Next[state]))
				return false;
		}

		for (var i = 0; i < left.Sets.Count; i++)
		{
			if (left.Sets[i].Count != right.Sets[i].Count)
				return false;

			for (var j = 0; j < left.Sets[i].Count; j++)
				if (left.Sets[i][j] != right.Sets[i][j])
					return false;
		}

		return true;
	}

	/// <summary>
	/// The key a machine of these patterns is kept under, or null where none would be (for the
	/// tests).
	/// </summary>
	internal static string? KeyOf(
		RecognitionGraph graph, IReadOnlyList<Node> patterns, Func<int, bool>? exact = null)
	{
		return Key.Of(graph, patterns, exact);
	}

	/// <summary>
	/// Everything a build reads, written out as text: what a kept machine is found by, compared in
	/// full.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A build reads the patterns, the bodies of the rules they call, and which patterns must be
	/// read exactly — nothing else of the graph, and nothing of the grammar's text. So the key is
	/// each pattern as a tree, with a rule written out the first time it is called and by its
	/// number after that: two keys are equal exactly where the patterns are the same shapes calling
	/// rules that call one another the same way, which is all the build tells rules apart by — the
	/// rules it is inside of, and the bodies it reads.
	/// </para>
	/// <para>
	/// Every node is written as what it is, even where the build reads two kinds alike: a key that
	/// tells more apart than the build does only keeps a machine twice. What the build never reads
	/// is left out — where in the text a node was written, a capture's name, what a construction
	/// builds, a call's arguments — so that the same patterns written elsewhere, or in a grammar
	/// that differs only in its syntax, are the same key. A node the build cannot read at all
	/// (a look-behind, an external recognizer) gives no key: such a build refuses, and a refusal
	/// is not kept.
	/// </para>
	/// </remarks>
	static class Key
	{
		/// <summary>
		/// The key, or null where the patterns hold something no machine is built of.
		/// </summary>
		/// <remarks>
		/// <paramref name="exact"/> is asked of every pattern, as a build that gets past the
		/// alphabet asks it: it is a question, and asking it once more changes nothing.
		/// </remarks>
		public static string? Of(
			RecognitionGraph graph, IReadOnlyList<Node> patterns, Func<int, bool>? exact)
		{
			var writer = new Writer(graph);

			writer.Number(patterns.Count);

			for (var i = 0; i < patterns.Count; i++)
			{
				if (!writer.Node(patterns[i]))
					return null;

				writer.Tag(exact?.Invoke(i) ?? false ? '!' : '.');
			}

			return writer.ToString();
		}

		sealed class Writer(RecognitionGraph graph)
		{
			readonly StringBuilder                _text  = new();
			readonly Dictionary<RuleSymbol, int>  _rules = [];

			public void Tag(char tag)
			{
				_text.Append(tag);
			}

			public void Number(long number)
			{
				_text.Append(number.ToString(CultureInfo.InvariantCulture)).Append(';');
			}

			void Text(string text)
			{
				Number(text.Length);
				_text.Append(text);
			}

			/// <summary>Writes a node; false where it is one no machine is built of.</summary>
			public bool Node(Node node)
			{
				// Follows calls, so a chain of them is as deep as the grammar makes it (DeepStack).
				if (DeepStack.Low)
					return DeepStack.Run(() => Node(node));

				switch (node)
				{
					case Model.Node.Empty:
						Tag('e');

						return true;

					case Model.Node.Guard:
						Tag('g');

						return true;

					case Model.Node.Glue:
						Tag('u');

						return true;

					case Model.Node.Literal literal:
						Tag(literal.IgnoreCase ? 'I' : 'L');
						Text(literal.Text);

						return true;

					case Model.Node.Element element:
						Tag(element.IsNegated ? 'N' : 'X');
						Number(element.Ranges.Count);

						foreach (var range in element.Ranges)
						{
							Number(range.From);
							Number(range.To);
						}

						Number(element.Categories.Count);

						foreach (var category in element.Categories)
							Text(category);

						Number(element.References.Count);

						foreach (var reference in element.References)
							Text(reference.Name);

						return true;

					case Model.Node.Sequence(var parts):
						Tag('S');

						return Nodes(parts);

					case Model.Node.Choice { Selection: null } choice:
						Tag('C');

						return Nodes(choice.Nodes);

					case Model.Node.Repeat(var turns, var min, var max):
						Tag('R');
						Number(min);
						Number(max ?? -1);

						return Node(turns);

					case Model.Node.Lookahead lookahead:
						Tag(lookahead.IsPositive ? 'P' : 'K');
						Tag(lookahead.BeforeSegmentEnd ? '1' : '0');

						return Node(lookahead.Body);

					case Model.Node.Atomic(var kept):
						Tag('A');

						return Node(kept);

					case Model.Node.Marked(var kept, _):
						Tag('M');

						return Node(kept);

					case Model.Node.Capture(_, var held):
						Tag('T');

						return Node(held);

					case Model.Node.Construct(var made, _):
						Tag('O');

						return Node(made);

					case Model.Node.Bind(_, var bound, _):
						Tag('B');

						return Node(bound);

					case Model.Node.Call(var called, _):
						if (_rules.TryGetValue(called, out var number))
						{
							Tag('F');
							Number(number);

							return true;
						}

						_rules[called] = _rules.Count;

						Tag('D');
						Number(_rules.Count - 1);

						if (!graph.Bodies.TryGetValue(called, out var body))
						{
							Tag('0');

							return true;
						}

						Tag('=');

						return Node(body);

					default:
						return false;
				}
			}

			bool Nodes(IReadOnlyList<Node> nodes)
			{
				Number(nodes.Count);

				foreach (var node in nodes)
					if (!Node(node))
						return false;

				return true;
			}

			public override string ToString()
			{
				return _text.ToString();
			}
		}
	}

	/// <summary>
	/// The machines kept, by their key: the least recently used out first, held to a budget of
	/// bytes and a number of machines.
	/// </summary>
	/// <remarks>
	/// Reading takes no lock; keeping one takes one, briefly, to count what is held and choose what
	/// leaves. Two compiles that miss at once both build, and the second to finish finds the first
	/// one's machine kept and keeps nothing.
	/// </remarks>
	sealed class Shelf(long budget, int capacity)
	{
		readonly ConcurrentDictionary<string, Entry> _entries = new(StringComparer.Ordinal);
		readonly object                              _adding  = new();

		long _clock;
		long _held;

		public bool TryGet(string key, out LexicalAutomaton machine)
		{
			if (_entries.TryGetValue(key, out var entry))
			{
				entry.Used = Interlocked.Increment(ref _clock);
				machine    = entry.Machine;

				return true;
			}

			machine = null!;

			return false;
		}

		public void Add(string key, LexicalAutomaton machine)
		{
			var weight = Weight(key, machine);

			if (weight > budget)
				return;

			lock (_adding)
			{
				if (_entries.ContainsKey(key))
					return;

				_entries[key] = new Entry(machine, weight)
				{
					Used = Interlocked.Increment(ref _clock),
				};

				_held += weight;

				if (_held <= budget && _entries.Count <= capacity)
					return;

				var oldest = _entries
					.Where(one => one.Key != key)
					.OrderBy(static one => one.Value.Used)
					.ToList();

				foreach (var pair in oldest)
				{
					if (_held <= budget && _entries.Count <= capacity)
						break;

					if (_entries.TryRemove(pair.Key, out var gone))
						_held -= gone.Weight;
				}
			}
		}

		public void Remove(string key)
		{
			lock (_adding)
				if (_entries.TryRemove(key, out var gone))
					_held -= gone.Weight;
		}

		/// <summary>
		/// What a machine and its key hold, in bytes, as an estimate: four bytes a cell of the
		/// table, and half as much again for the ways out read off it; the key's characters.
		/// </summary>
		static long Weight(string key, LexicalAutomaton machine)
		{
			var cells = (long)machine.Next.Count * machine.Atoms.Count;

			return cells * 6 + key.Length * 2L + (machine.Atoms.Count + machine.Accepts.Count) * 4L;
		}

		sealed class Entry(LexicalAutomaton machine, long weight)
		{
			public readonly LexicalAutomaton Machine = machine;
			public readonly long             Weight  = weight;

			long _used;

			public long Used
			{
				get { return Interlocked.Read(ref _used); }
				set { Interlocked.Exchange(ref _used, value); }
			}
		}
	}
}
