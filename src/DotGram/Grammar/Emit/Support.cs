using System;

namespace DotGram.Grammar.Emit;

/// <summary>
/// The support library a generated parser carries, as text.
/// </summary>
/// <remarks>
/// <para>
/// This is the one thing .Gram ships that is not a parser: the handful of types every
/// generated file needs — what a publication answers with, what a failure records, what a
/// streamed parse reads through. There is no runtime package (docs/syntax.md §6.2), so
/// they are emitted into the consumer's own compilation, and every rule about that
/// applies here and almost nowhere else in the emitter: `internal` or nested, `global::`
/// on everything, and no language feature the consumer's compiler might not have.
/// </para>
/// <para>
/// Kept apart from the code that decides <i>which</i> of them a grammar needs, because
/// they are a different kind of thing: this file is read as C# that will exist, the
/// emitter is read as C# that writes it.
/// </para>
/// </remarks>
public static partial class CSharpEmitter
{
	/// <summary>What a publication answers with. The name a rule may not take.</summary>
	internal const string MatchType = "Match";

	/// <summary>What a streamed parse reads through. The name a rule may not take.</summary>
	internal const string WindowType = "Window";

	/// <summary>
	/// What tells "the input ran out" from "this does not match", for a windowed parse.
	/// </summary>
	/// <remarks>
	/// Carried only where something reads through a window. A rule that wanted one more
	/// character fails at the position that character would have gone in, which over a
	/// window is before the end of what is held — indistinguishable, without this, from an
	/// element that genuinely broke there.
	/// </remarks>
	const string StarvedField = """

			/// <summary>Whether the match stopped because the input did, not because it did not match.</summary>
			public bool Starved;
		""";

	/// <summary>
	/// How much of a reader a window holds before it has to grow.
	/// </summary>
	/// <remarks>
	/// A page. Small enough that a short input costs one allocation of no consequence,
	/// large enough that a line-oriented feed never grows it — and where an element is
	/// longer than this, growing is what the retention analysis promised would be
	/// bounded, not what it promised would not happen.
	/// </remarks>
	const int WindowSize = 4096;

	/// <summary>
	/// A reader read through a buffer that is reused, and the reason streaming works at
	/// all.
	/// </summary>
	/// <remarks>
	/// <para>
	/// §6.3 fixes what has to be held: a call reaches back not at all and a rule reaches
	/// back exactly as far as it has consumed, so what the window must keep is the extent
	/// of the outermost rule still in progress. Everything before where the parse is now
	/// can be dropped, and this drops it — by moving what is left to the front of the same
	/// buffer, so a feed of a million records reads through one array.
	/// </para>
	/// <para>
	/// It is not a line reader, deliberately. The retention analysis measures in lines
	/// because that is what bounds a feed, but nothing here knows what a line is: two
	/// records may share one, one record may span three, and a grammar with no line
	/// terminators anywhere streams exactly as well. What the window answers is "how much
	/// can still be seen", which is the only question the machine asks of it.
	/// </para>
	/// <para>
	/// Growing rather than failing when an element does not fit: the analysis has already
	/// refused the grammars whose elements are unbounded (§6.3), so a grow here is a long
	/// record rather than a runaway.
	/// </para>
	/// </remarks>
	/// <summary>What a sequence of lines is read through. The name a rule may not take.</summary>
	internal const string LinesType = "Lines";

	/// <summary>
	/// A sequence of lines, read as though it were a file.
	/// </summary>
	/// <remarks>
	/// <para>
	/// §6.3 lists <c>IEnumerable&lt;string&gt;</c> beside <c>TextReader</c>, and this is
	/// the whole of the difference between them: a reader carries its terminators and a
	/// sequence of lines has had them taken off. So they are put back, as <c>\n</c>, and
	/// everything downstream is the reader case unchanged.
	/// </para>
	/// <para>
	/// Which terminator is a decision and not a detail. <c>\n</c> because a grammar's
	/// <c>eol</c> matches it, because it is what the lines came from more often than not,
	/// and because the alternative — putting back what was taken off — is not knowable:
	/// the sequence does not say, and <c>File.ReadLines</c> would not tell it.
	/// </para>
	/// </remarks>
	internal const string LinesClass = """
		/// <summary>A sequence of lines, read as though it were a file.</summary>
		sealed class Lines : global::System.IO.TextReader
		{
			private readonly global::System.Collections.Generic.IEnumerator<string> _lines;

			private string _line = "";
			private int    _at;
			private bool   _ended;

			public Lines(global::System.Collections.Generic.IEnumerable<string> lines)
			{
				_lines = lines.GetEnumerator();
			}

			public override int Read(char[] buffer, int index, int count)
			{
				var written = 0;

				while (written < count)
				{
					if (_at >= _line.Length && !Next())
						break;

					var taking = global::System.Math.Min(count - written, _line.Length - _at);

					_line.CopyTo(_at, buffer, index + written, taking);

					_at     += taking;
					written += taking;
				}

				return written;
			}

			/// <summary>
			/// The next line, with the terminator the sequence does not carry put back.
			/// </summary>
			private bool Next()
			{
				if (_ended)
					return false;

				if (!_lines.MoveNext())
				{
					_ended = true;

					return false;
				}

				_line = _lines.Current + "\n";
				_at    = 0;

				return true;
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
					_lines.Dispose();

				base.Dispose(disposing);
			}
		}
		""";

	internal const string WindowClass = """
		/// <summary>A reader, read through a buffer that is reused.</summary>
		sealed class Window
		{
			private readonly global::System.IO.TextReader _input;

			private char[] _buffer;
			private int    _filled;
			private long   _offset;
			private bool   _ended;

			/// <summary>Line terminators dropped with what the window no longer holds.</summary>
			private int _lines;

			/// <summary>Where the last of them was, or -1 when none has been dropped.</summary>
			private long _break = -1;

			public Window(global::System.IO.TextReader input, int capacity)
			{
				_input  = input;
				_buffer = new char[capacity];
			}

			/// <summary>How much of the input the window is holding.</summary>
			public int Length
			{
				get { return _filled; }
			}

			/// <summary>Whether the reader has been read to its end.</summary>
			public bool Ended
			{
				get { return _ended; }
			}

			/// <summary>Where the window begins in the whole input.</summary>
			public long Offset
			{
				get { return _offset; }
			}

			/// <summary>
			/// What the window holds.
			/// </summary>
			/// <remarks>
			/// Called where it is passed and never kept in a local: a span cannot live in
			/// an iterator's state, and every published streaming method is an iterator.
			/// </remarks>
			public global::System.ReadOnlySpan<char> Span()
			{
				return new global::System.ReadOnlySpan<char>(_buffer, 0, _filled);
			}

			/// <summary>A stretch of the window, as a string.</summary>
			public string Text(int from, int length)
			{
				return new string(_buffer, from, length);
			}

			/// <summary>
			/// Which line of the whole input a position in the window is on, from 1.
			/// </summary>
			/// <remarks>
			/// The window's own contents plus what was dropped before it. Counting only
			/// what is held would restart the numbering every time the buffer moved.
			/// </remarks>
			public int LineAt(int position)
			{
				var line = _lines + 1;

				for (var at = 0; at < position; at++)
					if (_buffer[at] == '\n')
						line++;

				return line;
			}

			/// <summary>How far into its line a position is, from 1.</summary>
			public int ColumnAt(int position)
			{
				var start = -1;

				for (var at = 0; at < position; at++)
					if (_buffer[at] == '\n')
						start = at;

				// The line began before the window did, so the length of what is held is
				// only part of the answer and the rest is where the last dropped
				// terminator was.
				return start < 0
					? (int)(_offset + position - _break)
					: position - start;
			}

			/// <summary>
			/// Reads more of the input, dropping what is before <paramref name="from"/> to
			/// make room for it and moving <paramref name="from"/> with what is kept.
			/// </summary>
			/// <returns>Whether anything new arrived.</returns>
			public bool Extend(ref int from)
			{
				if (_ended)
					return false;

				// Room is made by dropping what is behind `from` first, and the buffer only
				// grows when there is nothing behind it to drop — an element genuinely
				// larger than the window. Growing while a prefix was still droppable made
				// the buffer double every time an element straddled the end of it, so a
				// long feed ended up holding most of itself.
				if (from > 0)
				{
					// What is about to be dropped is where a line number comes from, so it
					// is counted on the way out. Without this a position past the first
					// window would be reported as a line near the top of the file.
					for (var at = 0; at < from; at++)
						if (_buffer[at] == '\n')
						{
							_lines++;
							_break = _offset + at;
						}

					global::System.Array.Copy(_buffer, from, _buffer, 0, _filled - from);

					_filled -= from;
					_offset += from;
					from     = 0;
				}
				else if (_filled == _buffer.Length)
				{
					var grown = new char[_buffer.Length * 2];

					global::System.Array.Copy(_buffer, 0, grown, 0, _filled);

					_buffer = grown;
				}

				var read = _input.Read(_buffer, _filled, _buffer.Length - _filled);

				if (read <= 0)
				{
					_ended = true;

					return false;
				}

				_filled += read;

				return true;
			}
		}
		""";

	/// <summary>
	/// One answer from a publication: what was recognized, or why nothing was.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Unconstrained, because a rule may declare itself <c>: @int</c>. <c>Value</c> is
	/// therefore <c>T</c> and not <c>T?</c> — an unconstrained <c>T?</c> needs a language
	/// version this generator may not assume — and <c>IsSuccess</c> is what says whether
	/// there is one. On a failure it is <c>default</c>.
	/// </para>
	/// <para>
	/// One type for the whole grammar rather than one per rule: only the value differs,
	/// and a per-rule copy of the same four members would be noise in every generated
	/// file.
	/// </para>
	/// </remarks>
	internal const string MatchStruct = """
		/// <summary>What a publication answers with: the value, or why there is none.</summary>
		public readonly struct Match<T>
		{
			private Match(bool isSuccess, T value, string? error, long position, int length)
			{
				IsSuccess = isSuccess;
				Value     = value;
				Error     = error;
				Position  = position;
				Length    = length;
			}

			/// <summary>Whether there is a value.</summary>
			public bool IsSuccess { get; }

			/// <summary>What was recognized. Meaningless unless <c>IsSuccess</c>.</summary>
			public T Value { get; }

			/// <summary>Why nothing was recognized, or null.</summary>
			public string? Error { get; }

			/// <summary>
			/// Where the match began, or how far the input could be followed before it
			/// failed. An offset into the whole input, so a <c>long</c>: an input may be a
			/// file larger than an <c>int</c> can index (docs/syntax.md §6.3).
			/// </summary>
			public long Position { get; }

			/// <summary>
			/// How much was matched, in input items, and zero when nothing was. An extent
			/// into a buffer rather than an offset into the input, so an <c>int</c>.
			/// </summary>
			public int Length { get; }

			internal static Match<T> Success(T value, long position, int length)
			{
				return new Match<T>(true, value, null, position, length);
			}

			internal static Match<T> Failed(string error, long position)
			{
				return new Match<T>(false, default!, error, position, 0);
			}
		}
		""";

	/// <summary>
	/// What every recognizer carries so that a failure can be described.
	/// </summary>
	/// <remarks>
	/// The name a rule may not take: <see cref="ResultTypes"/> renames its own type
	/// instead if a grammar declares a rule called this.
	/// </remarks>
	internal const string FailureType = "Failure";

	/// <summary>
	/// The record of the best failure a match saw, threaded through every recognizer.
	/// </summary>
	/// <remarks>
	/// A struct passed by <c>ref</c> rather than more <c>out</c> parameters, and that is
	/// the point of it: what a failure has to say can grow, and each addition would
	/// otherwise be another parameter on every recognizer and another edit at every call
	/// site.
	/// </remarks>
	internal static string FailureStructWith(bool reach, bool starved = false) =>
		Lines.Normalize(FailureStruct)
			.Replace(
				"\t{{reach}}" + Lines.Ending,
				reach ? Lines.Normalize(ReachField) + Lines.Ending : "")
			.Replace(
				"\t{{starved}}" + Lines.Ending,
				starved ? Lines.Normalize(StarvedField) + Lines.Ending : "");

	const string FailureStruct = """
		/// <summary>Where a match got before it gave up, and why.</summary>
		struct Failure
		{
			/// <summary>
			/// The furthest position the input was followed to. Zero on a match that
			/// succeeded without ever backtracking, and meaningless unless one failed.
			/// </summary>
			public int Position;
			{{reach}}
			{{starved}}
		}
		""";

	/// <summary>
	/// The field a recovering grammar needs beside <c>Position</c>, and one that a grammar
	/// not using recovery does not carry.
	/// </summary>
	/// <remarks>
	/// <c>Position</c> is the whole parse's furthest and something further along may
	/// already have raised it, so it cannot say whether <b>this</b> element began. This one
	/// is reset where each element begins, which is exactly the question §8.2 asks.
	/// </remarks>
	const string ReachField = """

			/// <summary>How far the element a recovering repetition last began got.</summary>
			public int Reach;
		""";

	/// <summary>
	/// Grows the backtracking stack. Emitted once per class, next to the recognizers
	/// that share it.
	/// </summary>
	internal const string GrowHelper = """
		static int[] Grow(global::System.Span<int> from)
		{
			var bigger = new int[from.Length * 2];

			from.CopyTo(bigger);

			return bigger;
		}
		""";

	/// <summary>The reusable state owned by the unified automaton.</summary>
	internal const string ParserRuntime = """
		private sealed class Parser
		{
			internal readonly global::System.Collections.Generic.List<ParserEntry> Entries =
				new global::System.Collections.Generic.List<ParserEntry>();

			internal void Reset() => Entries.Clear();
		}

		private readonly struct ParserEntry
		{
			internal const int Choice = 1;
			internal const int Call   = 2;
			internal const int Atomic = 3;
			internal const int Repeat = 4;
			internal const int Lookahead = 5;

			internal ParserEntry(
				int kind, int state, int position, int callIndex, int atomicIndex,
				int repeatIndex, int lookaheadIndex, int value)
			{
				Kind        = kind;
				State       = state;
				Position    = position;
				CallIndex   = callIndex;
				AtomicIndex = atomicIndex;
				RepeatIndex = repeatIndex;
				LookaheadIndex = lookaheadIndex;
				Value       = value;
			}

			internal int Kind        { get; }
			internal int State       { get; }
			internal int Position    { get; }
			internal int CallIndex   { get; }
			internal int AtomicIndex { get; }
			internal int RepeatIndex { get; }
			internal int LookaheadIndex { get; }
			internal int Value       { get; }
		}

		static partial void RentParser(ref Parser parser);
		static partial void ReturnParser(Parser parser);

		[global::System.Diagnostics.Conditional("DOTGRAM_TRACE")]
		static void Trace(string action, int state, int position, int arena)
		{
			global::System.Diagnostics.Debug.WriteLine(
				".Gram " + action + " state=" + state.ToString() +
				" position=" + position.ToString() + " arena=" + arena.ToString());
		}
		""";

	/// <summary>The out-of-band channel a <c>recover</c> without a <c>=&gt;</c> reports on.</summary>
	internal const string RecoveredMethod = "OnRecovered";

	/// <summary>
	/// What §8.3 promises a grammar that has no type to spare: the broken element is
	/// dropped from the sequence and reported here instead.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A <c>partial void</c> of the classic kind — C# 3, so nothing is assumed about the
	/// consumer's language version, and unlike the C# 9 form an implementation is
	/// optional. Where there is none the compiler removes the declaration, every call to
	/// it, <b>and everything in the argument lists</b>: the element's text is never
	/// materialized, its line is never counted, and a feed of a hundred million records
	/// pays nothing for a channel nobody listens on.
	/// </para>
	/// <para>
	/// One hook per host class rather than per rule, which is why it is told which rule
	/// rejected the element. Emitted always for a grammar that recovers without a
	/// <c>=&gt;</c>, so what the consumer compiles never depends on how it was built.
	/// </para>
	/// </remarks>
	internal const string RecoveredHook = """
		/// <summary>
		/// Called for each element a recovering repetition could not read (docs/syntax.md
		/// §8.3). Implement it in your own half of this class to be told; leave it alone
		/// and every call to it, arguments included, is removed at compile time.
		/// </summary>
		/// <param name="rule">The rule the element should have been.</param>
		/// <param name="text">The input it covered, up to where the parse picked up again.</param>
		/// <param name="position">Where it started, as an offset from the beginning.</param>
		/// <param name="line">Which line it started on, counting from 1.</param>
		/// <param name="column">How far into that line, counting from 1.</param>
		/// <param name="ordinal">Which element of the repetition it was, counting rejected ones, from 0.</param>
		/// <param name="message">Why it was rejected.</param>
		static partial void OnRecovered(
			string rule, string text, long position, int line, int column, int ordinal, string message);
		""";

	/// <summary>
	/// Where a position is, for a person. Emitted once per class, and only for a grammar
	/// whose <c>recover</c> asked to be told (§8.2).
	/// </summary>
	/// <remarks>
	/// <para>
	/// Both from 1, which is where an editor starts counting and therefore where a message
	/// a person will act on has to. <c>\r\n</c> needs no case of its own: the line ends at
	/// the <c>\n</c>, and the <c>\r</c> before it is the last column of the line it ends.
	/// </para>
	/// <para>
	/// Two functions rather than one method with two <c>out</c> parameters, because both
	/// are used as <b>arguments</b>. A call to an unimplemented <c>partial void</c> is
	/// removed along with everything in its argument list, so an out-of-band report nobody
	/// listens for costs neither of these scans. A statement before the call would have
	/// survived the erasure and scanned anyway.
	/// </para>
	/// </remarks>
	internal const string LocateHelper = """
		/// <summary>Which line a position is on, counting from 1.</summary>
		static int LineAt(global::System.ReadOnlySpan<char> text, int position)
		{
			var line = 1;

			for (var at = 0; at < position; at++)
				if (text[at] == '\n')
					line++;

			return line;
		}

		/// <summary>How far into its line a position is, counting from 1.</summary>
		static int ColumnAt(global::System.ReadOnlySpan<char> text, int position)
		{
			var column = 1;

			for (var at = 0; at < position; at++)
				column = text[at] == '\n' ? 1 : column + 1;

			return column;
		}
		""";
}
