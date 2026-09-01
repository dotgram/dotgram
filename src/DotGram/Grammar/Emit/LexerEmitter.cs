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
/// <b>But a row per state where the characters are near each other</b>, which was measured
/// too. Grouping by target says what a state's ways out <em>are</em>; it does not say they
/// have to be asked one at a time. A state whose edges live inside a small window of
/// characters is one subtraction, one unsigned compare and one load — see <c>Dense</c> —
/// and the window is what keeps this from becoming the dense table again: a Unicode
/// category or an "anything but a quote" stays a chain, because a row for one of those is
/// most of a plane.
/// </para>
/// <para>
/// It is worth 1.02x to 1.24x over <c>SqlStandard92</c>'s corpus, interleaved, for 367 rows
/// and 12,000 cells — 24 kilobytes of <c>short</c>. The first state alone was forty-four
/// tests, every mark the language writes and both cases of every letter a keyword begins
/// with, and it is entered once per token.
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
		Rows.Clear();
		Rowed.Clear();
		Wide = false;

		// The scanner is written first and the sets it asked for after it, because which sets
		// there are is only known once every state has been written.
		var body = new Writer();

		Scanner(body, machine, tag);

		var text = new Writer();

		Accepting(text, machine, tag);
		text.Line();
		Sets(text, tag);
		Rowsets(text, tag);
		Between(text, tag);
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

		foreach (var (bounds, at) in Bounds.Select((one, at) => (one, at)))
			text.Line($"static readonly char[] Scan{tag}_Set{at} = {{ {bounds} }};");

		text.Line();
	}

	static readonly List<string>             Bounds = [];
	static readonly Dictionary<string, int>  Named  = [];
	static readonly List<string>             Rows   = [];
	static readonly Dictionary<string, int>  Rowed  = [];

	/// <summary>Whether any state is numbered past what a <c>short</c> holds.</summary>
	static bool Wide;

	/// <summary>
	/// The rows a dense state is read out of, one static field each.
	/// </summary>
	/// <remarks>
	/// <c>short</c> rather than <c>int</c> wherever the states fit in one, which is every
	/// grammar seen here and then some: halving the table is what keeps a hot row in cache.
	/// The same row asked for twice is one field, which the keyword trie makes worth doing —
	/// many of its states admit exactly the letters that continue a word.
	/// </remarks>
	static void Rowsets(Writer text, string tag)
	{
		if (Rows.Count == 0)
			return;

		foreach (var (row, at) in Rows.Select((one, at) => (one, at)))
		{
			text.Line($"static readonly {(Wide ? "int" : "short")}[] Scan{tag}_Row{at} =");

			using (text.Braces("", ";"))
			{
				var line = new StringBuilder("	");

				foreach (var cell in row.Split(' '))
				{
					line.Append(cell).Append(", ");

					if (line.Length < 92)
						continue;

					text.Line(line.ToString().TrimEnd());
					line.Clear().Append('	');
				}

				if (line.Length > 1)
					text.Line(line.ToString().TrimEnd());
			}
		}

		text.Line();
	}

	/// <summary>Membership of a set too wide to write out, by searching its bounds.</summary>
	/// <remarks>
	/// The bounds alternate: a range's first character and one past its last. So a character
	/// is inside the set exactly where the number of bounds at or below it is odd, which a
	/// binary search answers in a handful of steps however many ranges there are. This is
	/// what a Unicode category costs here.
	/// </remarks>
	static void Between(Writer text, string tag)
	{
		text.Line("/// <summary>Whether a character is inside a set given as alternating bounds.</summary>");
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

	static void Scanner(Writer text, LexicalAutomaton machine, string tag)
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
				text.Line("var c = text[p];");
				text.Line();
				text.Line("var next =");

				using (text.Indent())
					for (var part = 0; part < parts; part++)
						text.Line(
							part == parts - 1
								? $"Scan{tag}_Part{part}(state, c);"
								: $"state < {(part + 1) * Held} ? Scan{tag}_Part{part}(state, c) :");

				text.Line();
				text.Line("if (next < 0)");

				using (text.Indent())
					text.Line("goto Done;");

				text.Line();
				text.Line("state = next;");
				text.Line("p++;");
				text.Line();
				text.Line($"if (Scan{tag}_Accepts[state] != 0)");

				using (text.Indent())
					text.Line($"(end, found) = (p, Scan{tag}_Accepts[state]);");
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

					var dense = Dense(ways);

					text.Line($"case {state}:");

					using (text.Indent())
					{
						if (dense is null)
						{
							foreach (var (on, to) in ways)
								text.Line($"if ({Test(on)}) return {to};");

							text.Line("return -1;");

							continue;
						}

						var (field, low, cells, rest) = dense.Value;
						var read = $"(uint)(c - {low}) < {cells}u ? {field}[c - {low}] : -1";

						// Every way out of a state leads somewhere different and no character
						// takes two of them, so what is left over may be asked after the row
						// rather than before it — see `Dense`.
						if (rest.Count == 0)
						{
							text.Line($"return {read};");

							continue;
						}

						text.Line($"var next{state} = {read};");
						text.Line($"if (next{state} >= 0) return next{state};");

						foreach (var (on, to) in rest)
							text.Line($"if ({Test(on)}) return {to};");

						text.Line("return -1;");
					}
				}

				text.Line("default: return -1;");
			}
		}
	}

	/// <summary>
	/// A state read out of a row instead of asked about, where that is the shorter question.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The first state of <c>SqlStandard92</c>'s scanner is forty-four tests and some sixty
	/// comparisons — every mark the language writes and both cases of every letter a keyword
	/// begins with. As a row it is one subtraction, one unsigned compare and one load.
	/// </para>
	/// <para>
	/// <b>Why the leftovers may be asked afterwards.</b> This is a deterministic machine over
	/// an alphabet of atoms: each character belongs to exactly one atom and each atom leads to
	/// one state, so no character satisfies two of a state's tests. The chain's order carries
	/// no meaning, and any subset of it may be lifted out and the rest left where it was.
	/// </para>
	/// <para>
	/// What is lifted is what fits a small window: an edge every range of which lies under
	/// <see cref="Reach"/>. That leaves out the wide sets — a Unicode category, or the
	/// "anything but a quote" of a string body — which is right twice over, since a row for
	/// one of those is most of a plane and the binary search it uses is already short.
	/// </para>
	/// </remarks>
	static (string Field, int Low, int Cells, IReadOnlyList<(IReadOnlyList<CharRange> On, int To)> Left)? Dense(
		IReadOnlyList<(IReadOnlyList<CharRange> On, int To)> ways)
	{
		var near = new List<(IReadOnlyList<CharRange> On, int To)>();
		var far  = new List<(IReadOnlyList<CharRange> On, int To)>();

		foreach (var way in ways)
			(way.On.All(range => range.To <= Reach) ? near : far).Add(way);

		// What the chain would have asked, in comparisons: a single character is one, a range
		// is two. The row is a subtraction, an unsigned compare and a load, so one comparison
		// is not worth replacing and two already are.
		var asked = near.Sum(way => way.On.Sum(range => range.From == range.To ? 1 : 2));

		if (asked < 2)
			return null;

		var low  = near.Min(way => way.On.Min(range => (int)range.From));
		var high = near.Max(way => way.On.Max(range => (int)range.To));

		var cells = high - low + 1;

		if (cells > Cells)
			return null;

		var row = new int[cells];

		for (var at = 0; at < cells; at++)
			row[at] = -1;

		foreach (var (on, to) in near)
			foreach (var range in on)
				for (var c = (int)range.From; c <= range.To; c++)
					row[c - low] = to;

		var text = string.Join(" ", row);

		Wide |= high > short.MaxValue;

		if (!Rowed.TryGetValue(text, out var found))
		{
			Rowed[text] = found = Rows.Count;
			Rows.Add(text);
		}

		return ($"Scan{Tag}_Row{found}", low, cells, far);
	}

	/// <summary>How far above the ASCII marks a row may reach before it is not one.</summary>
	/// <remarks>
	/// Latin Extended-A, so that a grammar naming accented letters one by one still gets a
	/// row, and a category or an "anything at all" does not.
	/// </remarks>
	const int Reach = 0x017F;

	/// <summary>How wide the window may be before the row costs more than it saves.</summary>
	/// <remarks>
	/// The only threshold left. How <em>many</em> ways out are worth a row was measured
	/// rather than reasoned about, and the answer was one: rowing every state that has a near
	/// edge beat rowing only the wide ones at every size tried, six, three and two — and it
	/// beat the plain chain by 1.03x to 1.22x over `SqlStandard92`'s corpus, interleaved. Two
	/// comparisons are not cheaper than a subtraction and a load, and 528 chains that each
	/// predict well still occupy the predictor.
	/// </remarks>
	const int Cells = 320;

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

	static string Searched(IReadOnlyList<CharRange> ranges)
	{
		var bounds = string.Join(
			", ",
			ranges.SelectMany(range =>
				new[] { CSharpEmitter.Char(range.From), CSharpEmitter.Char((char)(range.To + 1)) }));

		// The same set asked for twice is the same field. A lexer asks for very few distinct
		// wide sets — one per character class the grammar wrote — and asks for each of them
		// from a great many states.
		if (!Named.TryGetValue(bounds, out var at))
		{
			Named[bounds] = at = Bounds.Count;
			Bounds.Add(bounds);
		}

		return $"Scan{Tag}_Between(c, Scan{Tag}_Set{at})";
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
