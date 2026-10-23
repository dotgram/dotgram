using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Emit;

/// <summary>
/// What a grammar that names <c>eos</c> publishes: its forms over a sequence of segments, read as
/// the segments with the joiner after each (<see cref="GrammarNormalizer.Joiner"/>), through the
/// forms over text that the rest of the emitter writes for it, private and under names of their
/// own (<see cref="Publication.EmittedName"/>).
/// </summary>
/// <remarks>
/// <para>
/// A boundary has no character, and the joiner is how one is read without one: no character test
/// of such a grammar takes it but <c>eos</c>, so every reading of a character stops there as at the
/// end of the input. That holds only where no segment brings the character itself, so a segment
/// that holds it is refused before anything is read: <c>Parse</c> throws, and the forms that answer
/// say so at its place.
/// </para>
/// <para>
/// Positions are those of the joined text: one for each character and one for each boundary, so a
/// position before an <c>eos</c> and after it differ. A line is a segment and a column an offset in
/// it, both counted from 1, as the support types count lines elsewhere — the joiner is the one line
/// ending, and <c>\n</c> inside a segment is a character like another.
/// </para>
/// </remarks>
public static partial class CSharpEmitter
{
	/// <summary>The access a publication is declared with, whatever its forms over text are.</summary>
	static string PublishedAccess(Publication publication)
	{
		return AccessOf(publication with { Joined = false });
	}

	/// <summary>The joiner as C# writes it.</summary>
	static string JoinerLiteral => Char(GrammarNormalizer.Joiner);

	/// <summary>
	/// What a grammar that names <c>eos</c> carries once per class: the segments joined into the
	/// text its forms over text read, and what a segment that holds the joiner is told.
	/// </summary>
	static string SegmentSupport(bool recognizers)
	{
		return (recognizers ? SegmentCut : "") + $$"""
			/// <summary>What a segment that holds U+0000 is told: in this grammar that is where a segment ends.</summary>
			private const string Holds_DotGram = "A segment holds U+0000, which this grammar reads as the end of a segment and never as a character of one.";

			/// <summary>
			/// The segments as the text the parser reads, or a refusal of the first segment that holds U+0000,
			/// which says where it holds it as a line, the segment, and a column, the offset in it.
			/// </summary>
			private static string Joined_DotGram(global::System.Collections.Generic.IReadOnlyList<string> input)
			{
				var text = Joined_DotGram(input, out var refused);

				if (refused >= 0)
				{
					var line = 0;

					while ((input[line] ?? "").IndexOf({{JoinerLiteral}}) < 0)
						line++;

					throw new global::System.FormatException(
						Holds_DotGram.Substring(0, Holds_DotGram.Length - 1) + " at line " +
						(line + 1).ToString(global::System.Globalization.CultureInfo.InvariantCulture) + ", column " +
						(input[line].IndexOf({{JoinerLiteral}}) + 1).ToString(global::System.Globalization.CultureInfo.InvariantCulture) + ".");
				}

				return text;
			}

			/// <summary>
			/// The segments as the text the parser reads: each with U+0000 after it, where the grammar's
			/// <c>eos</c> reads the end of it. Where a segment holds U+0000 itself, the text is empty and
			/// <paramref name="refused"/> is where that character would have stood; -1 otherwise.
			/// </summary>
			private static string Joined_DotGram(global::System.Collections.Generic.IReadOnlyList<string> input, out long refused)
			{
				if (input is null)
					throw new global::System.ArgumentNullException(nameof(input));

				var length = 0;

				for (var i = 0; i < input.Count; i++)
					length = checked(length + (input[i]?.Length ?? 0) + 1);

				var text = new char[length];
				var at   = 0;

				for (var i = 0; i < input.Count; i++)
				{
					var segment = input[i] ?? "";
					var holds   = segment.IndexOf({{JoinerLiteral}});

					if (holds >= 0)
					{
						refused = at + holds;

						return "";
					}

					segment.CopyTo(0, text, at, segment.Length);

					at         += segment.Length;
					text[at++]  = {{JoinerLiteral}};
				}

				refused = -1;

				return new string(text);
			}

			/// <summary>
			/// Where a position of the text the parser reads is: the segment, and the offset in it, both
			/// counted from 1. A position past the last segment is the start of one more.
			/// </summary>
			private static string At_DotGram(string text, long position)
			{
				var line  = 1;
				var start = 0L;

				for (var at = 0; at < position && at < text.Length; at++)
					if (text[at] == {{JoinerLiteral}})
					{
						line++;
						start = at + 1;
					}

				return "line " + line.ToString(global::System.Globalization.CultureInfo.InvariantCulture) +
					", column " + (position - start + 1).ToString(global::System.Globalization.CultureInfo.InvariantCulture);
			}

			/// <summary>A sequence of segments as a list, which it may already be.</summary>
			private static global::System.Collections.Generic.IReadOnlyList<string> Listed_DotGram(global::System.Collections.Generic.IEnumerable<string> input)
			{
				if (input is null)
					throw new global::System.ArgumentNullException(nameof(input));

				return input as global::System.Collections.Generic.IReadOnlyList<string> ?? new global::System.Collections.Generic.List<string>(input);
			}

			/// <summary>A reader as the text the parser reads, a line a segment; a line that holds U+0000 throws.</summary>
			private static string Read_DotGram(global::System.IO.TextReader input)
			{
				return new SegmentReader_DotGram(input, true).ReadToEnd();
			}

			/// <summary>
			/// A reader as the text the parser reads, a line a segment, or, where a line holds U+0000, an
			/// empty text, where that character would have stood and what the line is told.
			/// </summary>
			private static string Read_DotGram(global::System.IO.TextReader input, out long refused, out string why)
			{
				var reader = new SegmentReader_DotGram(input, false);
				var text   = reader.ReadToEnd();

				refused = reader.Refused;
				why     = refused < 0 ? "" : SegmentReader_DotGram.Holds(reader.RefusedLine, reader.RefusedColumn);

				return refused < 0 ? text : "";
			}

			/// <summary>
			/// A reader read as a sequence of lines, each a segment: every line ending — <c>\r\n</c>,
			/// <c>\n</c> or <c>\r</c> — becomes the end of a segment, and a last line without one ends
			/// its segment as well, as <c>File.ReadLines</c> splits a file.
			/// </summary>
			private sealed class SegmentReader_DotGram : global::System.IO.TextReader
			{
				private readonly global::System.IO.TextReader _input;
				private readonly bool _throws;

				private readonly char[] _block = new char[4096];
				private int    _at;
				private int    _filled;
				private bool   _cr;
				private bool   _open;
				private bool   _ended;
				private long   _position;
				private int    _line = 1;
				private int    _column;

				public SegmentReader_DotGram(global::System.IO.TextReader input, bool throws)
				{
					_input  = input ?? throw new global::System.ArgumentNullException(nameof(input));
					_throws = throws;
				}

				/// <summary>Where a line held U+0000, in the text read, or -1.</summary>
				public long Refused { get; private set; } = -1;

				/// <summary>Which line held it, from 1.</summary>
				public int RefusedLine { get; private set; }

				/// <summary>Where in that line it stood, from 1.</summary>
				public int RefusedColumn { get; private set; }

				/// <summary>What a line that holds U+0000 is told: the line, and the column it stands at.</summary>
				public static string Holds(int line, int column)
				{
					return "A line holds U+0000, which this grammar reads as the end of a segment and never as a character of one at line " +
						line.ToString(global::System.Globalization.CultureInfo.InvariantCulture) + ", column " +
						column.ToString(global::System.Globalization.CultureInfo.InvariantCulture) + ".";
				}

				public override int Read(char[] buffer, int index, int count)
				{
					var written = 0;

					while (written < count && !_ended)
					{
						if (_at == _filled)
						{
							_filled = _input.Read(_block, 0, _block.Length);
							_at     = 0;

							if (_filled <= 0)
							{
								_filled = 0;
								_ended  = true;

								// A last line without its ending ends its segment all the same.
								if (_open)
								{
									buffer[index + written++] = '\0';
									_open = false;
								}

								break;
							}
						}

						var c = _block[_at++];

						// The `\n` of a `\r\n` whose `\r` was the end of a block, or of the last read.
						if (_cr && c == '\n')
						{
							_cr = false;
							continue;
						}

						_cr = c == '\r';

						if (c == '\r' || c == '\n')
						{
							buffer[index + written++] = '\0';
							_open   = false;
							_column = 0;
							_line++;
						}
						else if (c == '\0')
						{
							Refused       = _position;
							RefusedLine   = _line;
							RefusedColumn = _column + 1;
							_ended        = true;

							if (_throws)
								throw new global::System.FormatException(Holds(_line, _column + 1));

							break;
						}
						else
						{
							buffer[index + written++] = c;
							_open = true;
							_column++;
						}

						_position++;
					}

					return written;
				}
			}
			""";
	}

	/// <summary>What a recognizer of the host is handed in place of the text: the segment it is called in.</summary>
	internal const string SegmentOf = "SegmentOf_DotGram";

	/// <summary>What puts the position a recognizer of the host moved back into the whole text.</summary>
	internal const string Rebased = "Rebased_DotGram";

	/// <summary>
	/// The input a recognizer of the host is handed in a grammar that names <c>eos</c>: the segment
	/// it is called in and nothing else, so that it can read neither the end of a segment nor another
	/// segment's text, behind it or ahead. The position it is handed is in that segment, and is put
	/// back into the whole text when it returns (<see cref="Rebased"/>).
	/// </summary>
	/// <remarks>
	/// Where the segment begins and ends is kept by the caller, and found again only where the
	/// position is outside it: a reading that goes forward scans each segment once, however many
	/// recognizers it calls.
	/// </remarks>
	static string SegmentCut => $$"""
		/// <summary>
		/// The segment <paramref name="position"/> is in, which is all a recognizer of the host reads, with
		/// the position moved into it. <paramref name="from"/> to <paramref name="end"/> is where that
		/// segment was last found to be: it holds no end of a segment, and <paramref name="end"/> is its own.
		/// </summary>
		private static global::System.ReadOnlySpan<char> SegmentOf_DotGram(global::System.ReadOnlySpan<char> text, ref int position, ref int from, ref int end)
		{
			if (position < from || position > end)
			{
				var ends = global::System.MemoryExtensions.IndexOf(text.Slice(position), {{JoinerLiteral}});

				from = global::System.MemoryExtensions.LastIndexOf(text.Slice(0, position), {{JoinerLiteral}}) + 1;
				end  = ends < 0 ? text.Length : position + ends;
			}

			position -= from;

			return text.Slice(from, end - from);
		}

		/// <summary>What a recognizer of the host answered, with the position it moved put back into the whole text.</summary>
		private static bool Rebased_DotGram(bool read, ref int position, int from)
		{
			position += from;

			return read;
		}


		""";

	/// <summary>
	/// The publication's forms over segments: a sequence materialized as a list, and the list joined.
	/// </summary>
	/// <param name="value">What a reading of the publication's rule hands back.</param>
	/// <param name="takes">The context parameter the forms over text take, or empty.</param>
	/// <param name="streamed">
	/// What the publication's streamed reading hands back, where it has one: its forms over a
	/// sequence and over a reader are then the streamed ones (<see cref="EmitOverLines"/>), and only
	/// the list is read whole here. Null where it does not stream.
	/// </param>
	static void EmitOverSegments(Writer file, Publication publication, string value, string takes, string? streamed)
	{
		var access = PublishedAccess(publication);
		var method = publication.MethodName;
		var hidden = publication.EmittedName;
		var gives  = takes.Length > 0 ? ", context" : "";
		var name   = publication.Rule.Declaration?.Name ?? publication.Rule.Name;
		var reader = "global::System.IO.TextReader";

		// Each input, what it is read as — the text, or the text with where it was refused and what
		// it is told — and whether a streamed reading takes it in place of the whole one.
		var forms = new List<(string Input, string Text, string Refusing, string Why, bool Streams)>
		{
			("global::System.Collections.Generic.IReadOnlyList<string>",
				"Joined_DotGram(input)", "Joined_DotGram(input, out var refused)", "Holds_DotGram", false),
			("global::System.Collections.Generic.IEnumerable<string>",
				"Joined_DotGram(Listed_DotGram(input))", "Joined_DotGram(Listed_DotGram(input), out var refused)", "Holds_DotGram", true),
			(reader,
				"Read_DotGram(input)", "Read_DotGram(input, out var refused, out var why)", "why", true),
		};

		foreach (var (input, text, refusing, why, streams) in forms)
		{
			var instead = streams && streamed is not null;

			// Over a reader, a streamed publication reads it through the lines adapter as it goes;
			// over a sequence it was written beside the reader's (EmitOverLines).
			if (instead && input == reader)
			{
				file.Line($"/// <summary>Reads <c>{name}</c> from a reader, a line a segment, as it is asked for (docs/syntax.md §6.3).</summary>");
				file.Line("/// <remarks>");
				file.Line("/// Every line ending — <c>\\r\\n</c>, <c>\\n</c> or <c>\\r</c> — is the end of a segment, and so is the");
				file.Line("/// end of a last line without one; a line that holds U+0000 throws a");
				file.Line("/// <see cref=\"global::System.FormatException\"/> naming it when it is reached.");
				file.Line("/// </remarks>");

				using (file.Block($"{access} static {streamed} {method}({input} input{takes})"))
					file.Line($"return {hidden}(new SegmentReader_DotGram(input, true){gives});");

				file.Line();
			}

			switch (publication.Kind)
			{
				case PublishKind.Yield:
				{
					var type = $"global::System.Collections.Generic.IEnumerable<{publication.ResultType!.Name}>";

					file.Line($"/// <summary>Lazily parses consecutive elements of the segments; malformed input throws during enumeration.</summary>");
					file.Line("/// <exception cref=\"global::System.FormatException\">A segment holds U+0000.</exception>");

					using (file.Block($"{access} static {type} {method}({input} input{takes})"))
						file.Line($"return {hidden}({text}{gives});");

					file.Line();

					break;
				}

				case PublishKind.Parse:
				{
					var match = $"{MatchType}<{value}>";

					if (!instead)
					{
						file.Line($"/// <summary>Parses the segments as <c>{name}</c>.</summary>");
						file.Line("/// <exception cref=\"global::System.FormatException\">");
						file.Line($"/// The segments are not <c>{name}</c>, or one holds U+0000. <c>Try{method}</c> answers instead.");
						file.Line("/// </exception>");

						using (file.Block($"{access} static {value} {method}({input} input{takes})"))
							file.Line($"return {hidden}({text}{gives});");

						file.Line();
					}

					file.Line($"/// <summary>Parses the segments as <c>{name}</c>, answering rather than throwing.</summary>");

					using (file.Block($"{access} static {match} Try{method}({input} input{takes})"))
					{
						file.Line($"var text = {refusing};");
						file.Line();
						file.Line("return refused < 0");
						file.Line($"\t? Try{hidden}(text{gives})");
						file.Line($"\t: {match}.Failed({OutcomeType}.NoMatch, {why}, refused, null, null);");
					}

					file.Line();
					file.Line($"/// <summary>Parses the segments as <c>{name}</c>, answering only whether they are one.</summary>");

					using (file.Block($"{access} static bool Try{method}({input} input{takes}, out {value} value)"))
					{
						file.Line($"var text = {refusing};");
						file.Line();

						using (file.Block("if (refused >= 0)"))
						{
							file.Line("value = default!;");
							file.Line("return false;");
						}

						file.Line();
						file.Line($"return Try{hidden}(text{gives}, out value);");
					}

					file.Line();

					break;
				}

				case PublishKind.Find:
				{
					if (instead)
						break;

					var type = $"global::System.Collections.Generic.IEnumerable<{MatchType}<{value}>>";

					file.Line($"/// <summary>Every occurrence of <c>{name}</c> in the segments, in order, found as it is asked for.</summary>");
					file.Line("/// <exception cref=\"global::System.FormatException\">A segment holds U+0000.</exception>");

					using (file.Block($"{access} static {type} {method}({input} input{takes})"))
						file.Line($"return {hidden}({text}{gives});");

					file.Line();

					break;
				}
			}
		}
	}

	/// <summary>
	/// The support types a streamed, a located or a traced reading uses, as a grammar that names
	/// <c>eos</c> reads them: a sequence of lines is a sequence of segments, each followed by the
	/// joiner where a line had its newline, and refused where it holds the joiner; and a line ends at
	/// the joiner alone.
	/// </summary>
	static string Segmented(string support)
	{
		var line = support.IndexOf("\r\n", StringComparison.Ordinal) >= 0 ? "\r\n" : "\n";
		var made = support;

		foreach (var (from, to) in SegmentedLines)
			made = made.Replace(from, to.Replace("\n", line));

		if (string.Equals(made, support, StringComparison.Ordinal))
			throw new InvalidOperationException("A support type was asked for over segments and has no line ending to change.");

		return made;
	}

	/// <summary>
	/// What <see cref="Segmented(string)"/> changes, each written exactly as the support types write
	/// it, and on one line of them.
	/// </summary>
	static readonly (string From, string To)[] SegmentedLines =
	[
		// Lines: the joiner where a line had its newline, and no segment that holds it, refused at the
		// line it is and the column it holds it at.
		("buffer[index + written] = '\\n';", $"buffer[index + written] = {JoinerLiteral};"),
		("private bool   _disposed;", "private bool   _disposed;\n\tprivate int    _count;"),
		("_line   = _lines.Current ?? \"\";",
		 "_line   = _lines.Current ?? \"\";\n" +
		 "\t\t_count++;\n\n" +
		 $"\t\tif (_line.IndexOf({JoinerLiteral}) >= 0)\n" +
		 "\t\t\tthrow new global::System.FormatException(\n" +
		 "\t\t\t\t\"A segment holds U+0000, which this grammar reads as the end of a segment and never as a character of one at line \" +\n" +
		 "\t\t\t\t_count.ToString(global::System.Globalization.CultureInfo.InvariantCulture) + \", column \" +\n" +
		 $"\t\t\t\t(_line.IndexOf({JoinerLiteral}) + 1).ToString(global::System.Globalization.CultureInfo.InvariantCulture) + \".\");"),

		// Window: a line ends at the joiner, and at nothing else.
		("return c == '\\r' || c == '\\n' && !(at > 0 ? _buffer[at - 1] == '\\r' : _droppedCr);", $"return c == {JoinerLiteral};"),
		("if (_buffer[at] == '\\n' || _buffer[at] == '\\r')", $"if (_buffer[at] == {JoinerLiteral})"),

		// Located_DotGram: the same, counted on and back.
		("if (c != '\\r' && c != '\\n')", $"if (c != {JoinerLiteral})"),
		("if (c == '\\r' || _at == 0 || text[_at - 1] != '\\r')", $"if (c == {JoinerLiteral})"),
		("if (text[at] == '\\r' || text[at] == '\\n' && (at == 0 || text[at - 1] != '\\r'))", $"if (text[at] == {JoinerLiteral})"),
		("text[_start - 1] != '\\n' && text[_start - 1] != '\\r'", $"text[_start - 1] != {JoinerLiteral}"),

		// GramRead.Locate, which the trace says positions with: a line ends at the joiner alone.
		("if (text[at] == '\\n')", $"if (text[at] == {JoinerLiteral})"),
	];
}
