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
/// <b>Direct code and not a table, and that was measured.</b> <c>SqlStandard92</c> comes to
/// 528 states over an alphabet of 897 atoms: a dense table is 473,616 cells, and merging
/// atoms that neighbour each other leaves 186,342 tests because the atoms alternate — a
/// keyword's letters cut the alphabet everywhere a category is dense. Grouped by target it
/// is 1,034 tests, forty-three at the widest state, which is smaller than the syntactic
/// machine it feeds.
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

		// The scanner is written first and the sets it asked for after it, because which sets
		// there are is only known once every state has been written.
		var body = new Writer();

		Scanner(body, machine, tag);

		var text = new Writer();

		Accepting(text, machine, tag);
		text.Line();
		Sets(text, tag);
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
