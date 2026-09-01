using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// The lexical machine, written out: characters in, a token kind and its extent out.
/// </summary>
/// <remarks>
/// <para>
/// One method and one small array. The method is the deterministic machine
/// <see cref="LexicalAutomaton"/> built — a state per case, and per state one test for each
/// way on — and the array says which kind each state accepts. Longest match falls out of
/// running to a stop and remembering the last state that accepted, which is the rule a lexer
/// needs and the only one it needs.
/// </para>
/// <para>
/// <b>Not a dense table, which was measured.</b> <c>SqlStandard92</c> comes to 528 states
/// over an alphabet of 897 atoms: state by atom is 473,616 cells, and merging atoms that
/// neighbour each other leaves 186,342 tests because the atoms alternate — a keyword's
/// letters cut the alphabet everywhere a category is dense. Grouped by target it is 1,034
/// tests, forty-three at the widest state, which is smaller than the syntactic machine it
/// feeds.
/// </para>
/// <para>
/// <b>But a table in front of it, which was measured too.</b> Grouping by target says what a
/// state's ways out <em>are</em>; it does not say they have to be asked one at a time. So
/// each state gets a row of 128 characters — <see cref="Table"/> — and reading a transition
/// is one load of where the row starts, one compare against a constant, and one load of the
/// cell. No call, no switch, no chain, and nothing that depends on which state it is except
/// that first load.
/// </para>
/// <para>
/// <b>The row sits where the state's own alphabet is</b>, not at zero. That is what lets a
/// machine over Cyrillic or Greek have the table a machine over ASCII has, and what the
/// window is chosen for: the most of what the grammar named that 128 characters can hold,
/// counted in ranges and not in characters, because a Unicode category is a great many
/// characters in a great many scattered pieces and counting those would drag every window
/// into the middle of one. Then it is slid as far down as it can go without dropping any of
/// them, since the room is free and it is where the characters that <em>end</em> a token
/// live.
/// </para>
/// <para>
/// What no window can hold keeps the chain: a category is most of a plane. Nothing but the
/// chain is reached for such a character, so the table costs it nothing — and for
/// <c>SqlStandard92</c> the whole table is 53 distinct rows and 6,784 cells, thirteen
/// kilobytes, because a keyword trie has a great many states that admit exactly the letters
/// continuing a word.
/// </para>
/// <para>
/// <b>No arena.</b> Not as an optimization but as the correctness signal the design asked
/// for: a lexer that wrote one would mean the boundary had been drawn in the wrong place.
/// There is no way back here, nothing is written down, and the whole of the state is three
/// locals.
/// </para>
/// </remarks>
public static class LexerEmitter
{
	/// <summary>Writes the scanner for a machine.</summary>
	/// <param name="machine">What to write.</param>
	/// <param name="tag">What to hang on the emitted names, so two may live together.</param>
	public static string Emit(LexicalAutomaton machine, string tag = "")
	{
		if (machine is null)
			throw new ArgumentNullException(nameof(machine));

		Tag = tag;
		Bounds.Clear();
		Named.Clear();
		Low.Clear();
		Lows.Clear();
		Edges.Clear();
		Edged.Clear();
		Reached = true;
		Wide = false;

		// The scanner is written first and the sets it asked for after it, because which sets
		// there are is only known once every state has been written.
		var lows = new int[machine.Next.Count];

		for (var state = 0; state < lows.Length; state++)
			lows[state] = Window(machine.From(state));

		var body = new Writer();

		Scanner(body, machine, tag, lows);

		var text = new Writer();

		Accepting(text, machine, tag);
		text.Line();
		Table(text, machine, tag, lows);
		Sets(text, tag);
		text.Line();
		text.Add(body);

		return text.ToString();
	}

	/// <summary>
	/// The bound arrays the wide tests search, one static field each.
	/// </summary>
	/// <remarks>
	/// Written as fields and not inline, which is not a matter of taste. Inline they were
	/// <c>new char[] { … }</c> inside the scanning loop — an allocation per character per
	/// test — and the generated lexer came out seventeen times slower than the hand-written
	/// one it replaced, slow enough to make the whole split a loss. The first run of it said
	/// 8,997 nanoseconds where the hand tokenizer said 510.
	/// </remarks>
	static void Sets(Writer text, string tag)
	{
		if (Bounds.Count == 0)
			return;

		foreach (var (bits, at) in Low.Select((one, at) => (one, at)))
			text.Line($"static readonly ulong[] Scan{tag}_Low{at} = {{ {bits} }};");

		foreach (var (bounds, at) in Edges.Select((one, at) => (one, at)))
			text.Line(
				bounds.Length == 0
					? $"static readonly char[] Scan{tag}_Bounds{at} = new char[0];"
					: $"static readonly char[] Scan{tag}_Bounds{at} = {{ {bounds} }};");

		text.Line();

		if (Edges.Count > 0)
		{
			text.Line("/// <summary>Whether a character is inside a set given as alternating bounds.</summary>");
			text.Line("/// <remarks>");
			text.Line("/// The bounds alternate: a range's first character and one past its last. So a");
			text.Line("/// character is inside exactly where the number of bounds at or below it is odd,");
			text.Line("/// which a search answers in a handful of steps. This is what a set too small to");
			text.Line("/// be worth eight kilobytes of bitmap costs instead.");
			text.Line("/// </remarks>");
			text.Line($"static bool Scan{tag}_Between(char c, char[] bounds)");

			using (text.Braces())
			{
				text.Line("var low  = 0;");
				text.Line("var high = bounds.Length;");
				text.Line();

				using (text.Braces("while (low < high)", ""))
				{
					text.Line("var middle = (low + high) / 2;");
					text.Line();
					text.Line("if (bounds[middle] <= c)");

					using (text.Indent())
						text.Line("low = middle + 1;");

					text.Line("else");

					using (text.Indent())
						text.Line("high = middle;");
				}

				text.Line();
				text.Line("return (low & 1) != 0;");
			}

			text.Line();
		}

		foreach (var (bits, at) in Bounds.Select((one, at) => (one, at)))
		{
			text.Line($"static readonly byte[] Scan{tag}_High{at} =");

			using (text.Braces("", ";"))
			{
				var line = new StringBuilder("	");

				foreach (var cell in bits.Split(','))
				{
					line.Append(cell).Append(',');

					if (line.Length < 96)
						continue;

					text.Line(line.ToString());
					line.Clear().Append('	');
				}

				if (line.Length > 1)
					text.Line(line.ToString());
			}
		}

		text.Line();
	}

	static readonly List<string>             Bounds = [];
	static readonly Dictionary<string, int>  Named  = [];
	static readonly List<string>             Low    = [];
	static readonly Dictionary<string, int>  Lows   = [];
	static readonly List<string>             Edges  = [];
	static readonly Dictionary<string, int>  Edged  = [];
	/// <summary>Whether any state is numbered past what a <c>short</c> holds.</summary>
	static bool Wide;

	/// <summary>Whether a character below ASCII can reach the case being written.</summary>
	static bool Reached = true;

	/// <summary>
	/// How wide one state's row may be.
	/// </summary>
	/// <remarks>
	/// A budget and not a place. The row begins at the state's own first character, so what
	/// this bounds is how far apart the characters a state admits may be before it stops
	/// being worth a table — a machine over Greek or Cyrillic gets one exactly as a machine
	/// over ASCII does, shifted to where its alphabet actually is. What no budget can hold is
	/// a Unicode category, which is most of a plane; that keeps the chain, and a character
	/// reaching it was never going to be answered by a row.
	/// </remarks>
	const int Reach = 128;

	/// <summary>
	/// Where every state goes for every character below <see cref="Reach"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// One array for the whole machine and one descriptor per state, packed into an
	/// <c>int</c>: the low byte is how far the state's row reaches and the rest is where it
	/// begins. So reading a transition is a load, a mask, a compare and a load — against a
	/// chain of range tests behind a call behind a switch, which is what a state used to be.
	/// </para>
	/// <para>
	/// A row stops at the state's own last character rather than at 128 where that is
	/// shorter, which costs nothing: a character past it is one the state refuses, and the
	/// chain it falls to says so. And two states that go to the same places share one row —
	/// a keyword trie has a great many states that admit exactly the letters continuing a
	/// word.
	/// </para>
	/// </remarks>
	static void Table(Writer text, LexicalAutomaton machine, string tag, IReadOnlyList<int> lows)
	{
		var cells  = new List<int>();
		var shared = new Dictionary<string, int>();
		var states = new List<long>();

		Wide = machine.Next.Count > short.MaxValue;

		for (var state = 0; state < machine.Next.Count; state++)
		{
			var ways = machine.From(state);
			var low  = lows[state];

			if (low < 0)
			{
				// Nowhere to put a row: the state admits nothing, or admits it only above
				// where a row could reach. Pointed at a run of cells that refuse everything,
				// so the loop needs no case for it.
				states.Add(Empty(cells, shared));

				continue;
			}

			var row = new int[Reach];

			for (var at = 0; at < Reach; at++)
				row[at] = -1;

			// Every way out leads somewhere different and no character takes two of them, so
			// a cell is written once however the ranges are ordered.
			foreach (var (on, to) in ways)
				foreach (var range in on)
					for (var c = Math.Max(range.From, low); c <= range.To && c - low < Reach; c++)
						row[c - low] = to;

			var key = string.Join(" ", row);

			if (!shared.TryGetValue(key, out var at2))
			{
				shared[key] = at2 = cells.Count;
				cells.AddRange(row);
			}

			// Unsigned, because a row index is a place in an array and not a number that could
			// be negative — signed, the shift below it would smear its top bit across the low.
			states.Add((uint)at2 | ((long)low << 32));
		}

		text.Line("/// <summary>Where each state goes, for the characters its row holds.</summary>");
		text.Line($"static readonly {(Wide ? "int" : "short")}[] Scan{tag}_Cells =");

		using (text.Braces("", ";"))
			Numbers(text, cells);

		text.Line();
		text.Line("/// <summary>Each state's row: where it begins in the cells, how wide, and from");
		text.Line("/// which character.</summary>");
		text.Line($"static readonly long[] Scan{tag}_States =");

		using (text.Braces("", ";"))
			Numbers(text, states);

		text.Line();
	}

	/// <summary>A row that refuses everything, shared by every state that needs one.</summary>
	static long Empty(List<int> cells, Dictionary<string, int> shared)
	{
		var row = new int[Reach];

		for (var at = 0; at < Reach; at++)
			row[at] = -1;

		var key = string.Join(" ", row);

		if (!shared.TryGetValue(key, out var at2))
		{
			shared[key] = at2 = cells.Count;
			cells.AddRange(row);
		}

		return at2;
	}

	/// <summary>
	/// Where to put one state's row.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Anywhere, not at the state's first character. A state that admits <c>'='</c> and a
	/// Cyrillic letter has an alphabet a thousand apart, and a row anchored at the <c>'='</c>
	/// answers for one character and sends every letter to the chain.
	/// </para>
	/// <para>
	/// <b>Counted in ways out and not in ranges, which took a second look.</b> A range is an
	/// artefact of how a set is written; a way out is a decision the machine makes. Counting
	/// ranges put 440 of <c>SqlStandard92</c>'s 528 rows at <c>U+0B25</c> — a window that
	/// holds more <em>pieces</em> of <c>\p{L}</c> than the whole of ASCII holds, and one
	/// single way out. Every ASCII character in those states missed the table and took the
	/// chain, which is to say the table was working for eighty-eight states out of five
	/// hundred.
	/// </para>
	/// <para>
	/// A way out counts where any part of it falls inside, because the row is filled by
	/// clipping and half a way out is half a row that answers. Characters break the tie, and
	/// then the window is slid as far down as it can go without dropping any way it was
	/// chosen for — the room below is free, since those cells refuse exactly as the chain
	/// would, and it is where the characters that end a token live.
	/// </para>
	/// </remarks>
	static int Window(IReadOnlyList<(IReadOnlyList<CharRange> On, int To)> ways)
	{
		if (ways.Count == 0)
			return -1;

		// Two candidates and no more: ASCII, or the best place above it. A row is 128 wide
		// and the characters that *end* a token — the space, the comma, the operator — are
		// all below 128 in every language there is, so a window that begins part way up
		// ASCII is never the right answer: it buys Latin-1 letters and sells the space.
		var best = (Low: 0, Ways: 0L, Chars: 0);

		Weigh(ref best, ways, 0);

		foreach (var start in ways
			.SelectMany(way => way.On)
			.Select(range => (int)range.From)
			.Where(from => from >= Reach)
			.Distinct())
		{
			Weigh(ref best, ways, start);
		}

		return best.Ways == 0 ? -1 : best.Low;
	}

	/// <summary>One candidate window, scored and kept if it is the best so far.</summary>
	/// <remarks>
	/// <para>
	/// Scored by <em>which</em> ways out it admits and not by how many, which is the
	/// distinction the third wrong answer turned on. Two windows admitting the same ways are
	/// the same answer, and the lower one is the better of two same answers: what the higher
	/// one holds extra belongs to a way the row already answers for, and the chain answers
	/// for it exactly as well. That is a state whose only way out is "any identifier
	/// character" — ASCII holds sixty-three of them and a window up in Latin-1 holds a
	/// hundred and twenty-eight, and the sixty-three are the ones anybody types.
	/// </para>
	/// <para>
	/// Where the ways differ the count decides, and then the characters — which is what tells
	/// a row over Cyrillic letters from a row over the single <c>'='</c> standing beside
	/// them. A window above ASCII is slid down as far as it goes without losing a character
	/// it holds, so that it sits on its alphabet rather than one reach past it.
	/// </para>
	/// </remarks>
	static void Weigh(
		ref (int Low, long Ways, int Chars) best,
		IReadOnlyList<(IReadOnlyList<CharRange> On, int To)> ways,
		int low)
	{
		var high  = low + Reach - 1;
		var taken = 0L;
		var held  = 0;
		var least = int.MaxValue;
		var most  = -1;

		for (var at = 0; at < ways.Count; at++)
		{
			var inside = false;

			foreach (var range in ways[at].On)
			{
				var from = Math.Max((int)range.From, low);
				var to   = Math.Min((int)range.To, high);

				if (from > to)
					continue;

				inside = true;
				held  += to - from + 1;
				least  = Math.Min(least, from);
				most   = Math.Max(most, to);
			}

			// Sixty-four ways out is more than any state here has, and a state with more
			// simply shares one bit between two of them — which costs a worse window and
			// never a wrong one.
			if (inside)
				taken |= 1L << (at & 63);
		}

		if (taken == 0)
			return;

		var at2 = low == 0 ? 0 : Math.Max(Reach, Math.Min(least, most - Reach + 1));

		if (best.Ways != 0)
		{
			// The same ways: keep whichever sits lower. Different ways: more of them, and
			// then more characters.
			if (taken == best.Ways)
			{
				if (at2 >= best.Low)
					return;
			}
			else
			{
				var mine  = Count(taken);
				var found = Count(best.Ways);

				if (mine < found || mine == found && held <= best.Chars)
					return;
			}
		}

		best = (at2, taken, held);
	}

	static int Count(long bits)
	{
		var many = 0;

		while (bits != 0)
		{
			bits &= bits - 1;
			many++;
		}

		return many;
	}

	static void Numbers<T>(Writer text, IReadOnlyList<T> values)
	{
		var line = new StringBuilder("	");

		foreach (var value in values)
		{
			line.Append(value).Append(", ");

			if (line.Length < 92)
				continue;

			text.Line(line.ToString().TrimEnd());
			line.Clear().Append('	');
		}

		if (line.Length > 1)
			text.Line(line.ToString().TrimEnd());
	}

	static void Accepting(Writer text, LexicalAutomaton machine, string tag)
	{
		text.Line("/// <summary>The kind each state accepts, one-based; 0 where it accepts none.</summary>");
		text.Line($"static readonly int[] Scan{tag}_Accepts =");

		using (text.Braces("", ";"))
		{
			var row = new StringBuilder("\t");

			for (var state = 0; state < machine.Next.Count; state++)
			{
				row.Append(machine.Accepts[state] + 1).Append(", ");

				if (row.Length < 92)
					continue;

				text.Line(row.ToString().TrimEnd());
				row.Clear().Append('\t');
			}

			if (row.Length > 1)
				text.Line(row.ToString().TrimEnd());
		}
	}

	/// <summary>
	/// How many states one method may hold.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Not a size in lines but a size in basic blocks, which is the measure the JIT reads.
	/// Written as one method, <c>SqlStandard92</c>'s scanner came to 26,637 bytes of IL and
	/// the runtime said what it thought of that: <c>Tier-0 switched MinOpts</c> — never
	/// optimized, and re-compiled worse rather than better — while the syntactic machine
	/// beside it, four thousand bytes after its own division, reached <c>Tier1 with
	/// Synthesized PGO</c>.
	/// </para>
	/// <para>
	/// Direct code was chosen over a table on the strength of 1,034 tests against 473,616
	/// table cells. The 1,034 was right and the conclusion wrong: a test is not a block, and
	/// 528 <c>case</c> labels are a great many blocks. So the states are divided the way the
	/// syntactic machine's are, and each part is a method the JIT will look at.
	/// </para>
	/// </remarks>
	const int Held = 96;

	static void Scanner(Writer text, LexicalAutomaton machine, string tag, IReadOnlyList<int> lows)
	{
		text.Line("/// <summary>");
		text.Line("/// One token: its kind by way of <paramref name=\"kind\"/>, and where it ends.");
		text.Line("/// </summary>");
		text.Line("/// <remarks>");
		text.Line("/// Longest match. The machine is run until it can go no further and the last state");
		text.Line("/// that accepted is the answer, which is why a keyword beats the identifier it starts");
		text.Line("/// and <c>&gt;=</c> beats <c>&gt;</c> without either being written down as a rule.");
		text.Line("/// </remarks>");
		text.Line("/// <returns>Where the token ends, or <paramref name=\"pos\"/> where none begins here.</returns>");

		text.Line(
			$"public static int Scan{tag}(global::System.ReadOnlySpan<char> text, int pos, out int kind)");

		using (text.Braces())
		{
			text.Line("var state = 0;");
			text.Line("var end   = pos;");
			text.Line("var found = 0;");
			text.Line("var p     = pos;");
			text.Line();

			var parts = (machine.Next.Count + Held - 1) / Held;

			using (text.Braces("while (p < text.Length)", ""))
			{
				text.Line("var c    = text[p];");
				text.Line($"var row  = Scan{tag}_States[state];");
				text.Line("var at   = c - (int)(row >> 32);");
				text.Line("int next;");
				text.Line();

				// One compare against a constant, because every row is the same width — what
				// varies is where it starts, and that is the half of the descriptor worth
				// loading. What falls outside is what the chains were always for: a Unicode
				// category, and the gap on either side of what one state admits. Both are off
				// the path an ordinary input takes.
				text.Line($"if ((uint)at < {Reach}u)");

				using (text.Indent())
					text.Line($"next = Scan{tag}_Cells[(int)row + at];");

				text.Line("else");

				using (text.Braces())
				{
					text.Line("next =");

					using (text.Indent())
						for (var part = 0; part < parts; part++)
							text.Line(
								part == parts - 1
									? $"Scan{tag}_Part{part}(state, c);"
									: $"state < {(part + 1) * Held} ? Scan{tag}_Part{part}(state, c) :");
				}

				text.Line();
				text.Line("if (next < 0)");

				using (text.Indent())
					text.Line("goto Done;");

				text.Line();
				text.Line("state = next;");
				text.Line("p++;");
				text.Line();
				text.Line($"var accepts = Scan{tag}_Accepts[state];");
				text.Line();
				text.Line("if (accepts != 0)");

				using (text.Indent())
					text.Line("(end, found) = (p, accepts);");
			}

			text.Line();
			text.Line("Done:");
			text.Line("kind = found;");
			text.Line();
			text.Line("return end;");
		}

		for (var part = 0; part * Held < machine.Next.Count; part++)
		{
			var first = part * Held;
			var last  = Math.Min((part + 1) * Held, machine.Next.Count) - 1;

			text.Line();
			text.Line($"/// <summary>Where states {first} to {last} go, or -1 for nowhere.</summary>");

			using (text.Braces($"static int Scan{tag}_Part{part}(int state, char c)"))
			using (text.Braces("switch (state)", ""))
			{
				for (var state = first; state <= last; state++)
				{
					var ways = machine.From(state);

					if (ways.Count == 0)
						continue;

					// Only the characters the table does not answer for reach here, which is
					// everything above ASCII and, for a state that admits nothing near the top
					// of it, the gap above its own last character. So these are written as they
					// always were: a chain, over a set the table has already narrowed.
					// Whether a character below ASCII can reach this case at all. It cannot
					// where the state's row begins at zero: the row is 128 wide, so every
					// such character was answered before the chain was called, and a set
					// test here may read the half above ASCII and nothing else.
					Reached = lows[state] != 0;

					text.Line($"case {state}:");

					using (text.Indent())
					{
						foreach (var (on, to) in ways)
							text.Line($"if ({Test(on)}) return {to};");

						text.Line("return -1;");
					}
				}

				text.Line("default: return -1;");
			}
		}
	}

	/// <summary>
	/// A test over <c>c</c> for a set of ranges.
	/// </summary>
	/// <remarks>
	/// Comparisons while there are few enough to read, and a searched array of bounds beyond
	/// that — which is what a Unicode category comes to, and what most of the wide sets here
	/// are. The same two shapes the character machine uses, for the same reason: below the
	/// threshold the branches predict and above it the search is shorter than the branches.
	/// </remarks>
	static string Test(IReadOnlyList<CharRange> ranges)
	{
		if (ranges.Count > Written)
			return $"{Searched(ranges)}";

		return string.Join(
			" || ",
			ranges.Select(range =>
				range.From == range.To
					? $"c == {CSharpEmitter.Char(range.From)}"
					: $"c >= {CSharpEmitter.Char(range.From)} && c <= {CSharpEmitter.Char(range.To)}"));
	}

	/// <summary>How many ranges are worth writing out before a search is shorter.</summary>
	const int Written = 4;

	static string Tag = "";

	/// <summary>
	/// A wide set, cut at the top of ASCII so that its halves can be shared apart.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Whole, these sets are what a generated lexer is mostly made of: <c>SqlStandard92</c>
	/// had sixty-seven of them and they were <b>65% of the file</b>. They are that many
	/// because a keyword trie has a state per prefix and each one's "any letter that is not
	/// one I branch on" is a set of its own — four hundred ranges, written out again for
	/// every state.
	/// </para>
	/// <para>
	/// And they are nearly the same set. A trie branches on the letters that begin the words
	/// of a language, which are ASCII in every language that has keywords; above ASCII all
	/// sixty-seven are the same Unicode letters. Cut at <c>U+0080</c> there are <b>three</b>
	/// distinct upper halves among the sixty-seven, one of which covers sixty-four — so the
	/// half that is enormous is written three times and the half that differs is a handful of
	/// characters.
	/// </para>
	/// <para>
	/// Nothing about the reading changes: the halves are searched by the same parity rule,
	/// and which one to search is a comparison against a constant.
	/// </para>
	/// </remarks>
	static string Searched(IReadOnlyList<CharRange> ranges)
	{
		var low  = new List<CharRange>();
		var high = new List<CharRange>();

		foreach (var range in ranges)
		{
			if (range.From < Ascii)
				low.Add(new CharRange(range.From, (char)Math.Min((int)range.To, Ascii - 1)));

			if (range.To >= Ascii)
				high.Add(new CharRange((char)Math.Max((int)range.From, Ascii), range.To));
		}

		// A bitmap is eight kilobytes whatever it holds, so it is spent only where it is
		// paid for: on a set with hundreds of ranges, which is what a Unicode category is.
		// A smaller one keeps the parity search — a set of thirty-six ranges is 144 bytes of
		// bounds against eight kilobytes, and a grammar naming many small classes would
		// otherwise put a bitmap in the assembly for each of them.
		var above = high.Count > Bitmapped
			? $"(Scan{Tag}_High{Field(high)}[c >> 3] & (1 << (c & 7))) != 0"
			: $"Scan{Tag}_Between(c, Scan{Tag}_Bounds{Edge(high)})";

		// Written out rather than called: behind a method taking a span, every character
		// pays for materializing the half it will not read, which measured as a fifth of the
		// time on an input that is all keywords. And where ASCII cannot arrive there is
		// nothing to choose between, so the test is what is left of it.
		return Reached
			? $"(c < {Ascii} ? (Scan{Tag}_Low{Below(low)}[c >> 6] & (1UL << (c & 63))) != 0 : {above})"
			: above;
	}

	/// <summary>
	/// How many ranges make a bitmap worth eight kilobytes.
	/// </summary>
	/// <remarks>
	/// A Unicode category is four hundred ranges and a class somebody wrote out is a dozen;
	/// there is no continuum here to cut in the middle of, so the number only has to fall
	/// between them. Sixty-four, which is also about where the parity search stops being six
	/// steps and starts being ten.
	/// </remarks>
	const int Bitmapped = 64;

	/// <summary>The field holding one run of alternating bounds.</summary>
	static int Edge(IReadOnlyList<CharRange> ranges)
	{
		var bounds = string.Join(
			", ",
			ranges.SelectMany(range =>
				new[] { CSharpEmitter.Char(range.From), CSharpEmitter.Char((char)(range.To + 1)) }));

		if (!Edged.TryGetValue(bounds, out var at))
		{
			Edged[bounds] = at = Edges.Count;
			Edges.Add(bounds);
		}

		return at;
	}

	/// <summary>The ASCII half, as the 128 bits it is.</summary>
	/// <remarks>
	/// Two numbers, because that is all 128 characters take. This is the half a keyword
	/// trie's states disagree about — they branch on the letters that begin words — so there
	/// are as many of these as there are states asking, and each is sixteen bytes.
	/// </remarks>
	static int Below(IReadOnlyList<CharRange> ranges)
	{
		var bits = new ulong[2];

		foreach (var range in ranges)
			for (var c = (int)range.From; c <= range.To && c < Ascii; c++)
				bits[c >> 6] |= 1UL << (c & 63);

		var text = $"0x{bits[0]:X16}UL, 0x{bits[1]:X16}UL";

		if (!Lows.TryGetValue(text, out var at))
		{
			Lows[text] = at = Low.Count;
			Low.Add(text);
		}

		return at;
	}

	/// <summary>Where ASCII stops and the categories begin.</summary>
	const int Ascii = 0x80;

	/// <summary>
	/// The half above ASCII, worked out here and printed as the bits it is.
	/// </summary>
	/// <remarks>
	/// Eight kilobytes covering the whole sixteen-bit alphabet — every script of the plane
	/// and not one of them, which is what a set named by a Unicode category holds. Printed
	/// as a byte literal behind a <c>ReadOnlySpan&lt;byte&gt;</c>, which the compiler puts
	/// in the assembly's own data rather than in an array: nothing is allocated, nothing
	/// runs when the type is loaded, and reading it is a load and a bit test.
	/// </remarks>
	static int Field(IReadOnlyList<CharRange> ranges)
	{
		var bits = new byte[8192];

		foreach (var range in ranges)
			for (var c = (int)range.From; c <= range.To; c++)
				bits[c >> 3] |= (byte)(1 << (c & 7));

		var text = string.Join(",", bits);

		if (!Named.TryGetValue(text, out var at))
		{
			Named[text] = at = Bounds.Count;
			Bounds.Add(text);
		}

		return at;
	}

	/// <summary>The smallest writer that will do, so this file owes the emitter nothing.</summary>
	sealed class Writer
	{
		readonly StringBuilder _text = new();
		int _depth;

		public void Line(string line = "")
		{
			if (line.Length > 0)
				_text.Append('\t', _depth);

			_text.Append(line).Append("\r\n");
		}

		public IDisposable Indent() => new Block(this, null, null);

		public IDisposable Braces(string head = "", string? tail = null) =>
			new Block(this, head, tail ?? "");

		public void Add(Writer other) => _text.Append(other._text);

		public override string ToString() => _text.ToString();

		sealed class Block : IDisposable
		{
			readonly Writer  _writer;
			readonly string? _tail;

			public Block(Writer writer, string? head, string? tail)
			{
				_writer = writer;
				_tail   = tail;

				if (head is not null)
				{
					if (head.Length > 0)
						writer.Line(head);

					writer.Line("{");
				}

				writer._depth++;
			}

			public void Dispose()
			{
				_writer._depth--;

				if (_tail is not null)
					_writer.Line("}" + _tail);
			}
		}
	}
}
