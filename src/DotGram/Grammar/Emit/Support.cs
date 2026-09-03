using System;
using System.Collections.Generic;
using System.Text;

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
	/// <summary>
	/// Emitted into a host whose grammar names it — a rule of that type, or a construction
	/// asking for <c>parserSpan</c>.
	/// </summary>
	internal const string SourceSpanStruct = """
		/// <summary>
		/// Half-open range of the input: [Start, End).
		/// </summary>
		/// <remarks>
		/// <para>
		/// What a capture of type <c>SourceSpan</c> comes back as, and what a
		/// <c>recover</c> factory asking for <c>parserSpan</c> is handed (docs/syntax.md
		/// §8.2). Two integers rather than the text between them: the text is already in the
		/// caller's hands, and cutting a string out of it is the one allocation a parse
		/// cannot avoid on the caller's behalf.
		/// </para>
		/// <para>
		/// Ours rather than <c>System.Range</c>, because a consumer building for an older
		/// framework very likely polyfills that one and two definitions of it in a
		/// compilation is an error. Nested in the host class rather than put in a namespace,
		/// because a type in a namespace has to be internal for two assemblies not to
		/// collide over it — and internal is what a public method may not return. Here the
		/// name belongs to the host, so neither is a problem.
		/// </para>
		/// <para>
		/// It says where, not when. A span outlives nothing: read it against the same text
		/// that was parsed, and against nothing else.
		/// </para>
		/// </remarks>
		public readonly struct SourceSpan
		{
			public SourceSpan(int start, int length)
			{
				Start  = start;
				Length = length;
			}

			/// <summary>Where it begins.</summary>
			public int Start { get; }

			/// <summary>How much of the input it covers.</summary>
			public int Length { get; }

			/// <summary>One past the last item, so an empty span has End equal to Start.</summary>
			public int End { get { return Start + Length; } }

			/// <summary>The text it covers, taken from the text it was measured against.</summary>
			public global::System.ReadOnlySpan<char> On(global::System.ReadOnlySpan<char> text)
			{
				return text.Slice(Start, Length);
			}

			public override string ToString() { return "[" + Start + ".." + End + ")"; }
		}
		""";

	/// <summary>The name a rule may not take, like <see cref="MatchType"/>.</summary>
	internal const string OutcomeType = "Outcome";

	/// <summary>
	/// What kind of answer a publication gave (§7.5).
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>IsSuccess</c> answers the question most callers ask and stays what it was; this
	/// answers the one the two failures differ on. Both are exact: a match either
	/// happened or did not, and a failure either ran out of input or met input that did
	/// not fit — the same test the wrapper already chooses its message by, which is the
	/// point. A windowed read asks the window instead, where the input may go on past
	/// what is held.
	/// </para>
	/// <para>
	/// §7.5 also names <c>Error</c> — a failure past a commit point (§8.2). It is not
	/// here, and deliberately: the arena does not keep "the furthest failure stands past
	/// a commit that still holds", and a flag set where a commit happens would call an
	/// abandoned alternative's commit an error. An outcome that is sometimes wrong is
	/// worse than one the caller has to ask about, so this says only what it knows (see
	/// docs/next.md).
	/// </para>
	/// </remarks>
	internal const string OutcomeEnum = """
		/// <summary>What kind of answer a publication gave (docs/syntax.md §7.5).</summary>
		public enum Outcome
		{
			/// <summary>The input matched, and <c>Value</c> holds what it built.</summary>
			Success,

			/// <summary>The input was there and did not fit.</summary>
			NoMatch,

			/// <summary>The input ran out where more was needed.</summary>
			Starved,
		}
		""";

	internal const string MatchStruct = """
		/// <summary>What a publication answers with: the value, or why there is none.</summary>
		public readonly struct Match<T>
		{
			/// <summary>What would have fit at the furthest position, or null.</summary>
			private readonly string[]? _expected;

			/// <summary>The arrays that tied with it, or null where none did.</summary>
			private readonly global::System.Collections.Generic.List<string[]>? _tied;

			/// <summary>
			/// What <c>Error</c> says when nothing named what would have fit. A literal
			/// chosen where the match failed, so naming it costs nothing.
			/// </summary>
			private readonly string? _otherwise;

			private Match(
				Outcome outcome, T value, long position, int length,
				string[]? expected, global::System.Collections.Generic.List<string[]>? tied,
				string? otherwise)
			{
				Outcome    = outcome;
				Value      = value;
				Position   = position;
				Length     = length;
				_expected  = expected;
				_tied      = tied;
				_otherwise = otherwise;
			}

			/// <summary>Whether there is a value.</summary>
			public bool IsSuccess { get { return Outcome == Outcome.Success; } }

			/// <summary>
			/// Which kind of answer this is: the value, input that did not fit, or input
			/// that ran out (docs/syntax.md §7.5).
			/// </summary>
			public Outcome Outcome { get; }

			/// <summary>What was recognized. Meaningless unless <c>IsSuccess</c>.</summary>
			public T Value { get; }

			/// <summary>
			/// Why nothing was recognized, or null.
			/// </summary>
			/// <remarks>
			/// Built where it is asked for rather than where the match failed, and built
			/// again on each ask. A caller that only wants to know whether the input
			/// matched pays for none of it, which is most callers and every one in a loop
			/// over input that is expected to fail sometimes.
			/// </remarks>
			public string? Error
			{
				get
				{
					if (IsSuccess)
						return null;

					var expected = _expected;

					if (_tied != null)
					{
						var total = expected == null ? 0 : expected.Length;

						foreach (var each in _tied)
							total += each.Length;

						var merged = new string[total];
						var at     = 0;

						if (expected != null)
						{
							expected.CopyTo(merged, 0);
							at = expected.Length;
						}

						foreach (var each in _tied)
						{
							each.CopyTo(merged, at);
							at += each.Length;
						}

						expected = merged;
					}

					if (expected == null || expected.Length == 0)
						return _otherwise;

					// Two sites may ask for the same thing — a literal written in two rules,
					// or the `<` that opens a type argument list in more than one — and a
					// reader is owed one mention of it rather than one per site. Copied
					// rather than compacted where it stands: without a tie this array is the
					// one the generator declared `static readonly`, and rewriting that would
					// change what every later failure says.
					if (expected.Length > 1)
					{
						var unique = new string[expected.Length];
						var kept   = 0;

						for (var i = 0; i < expected.Length; i++)
						{
							var seen = false;

							for (var j = 0; j < kept; j++)
								if (string.Equals(
										unique[j], expected[i], global::System.StringComparison.Ordinal))
								{
									seen = true;
									break;
								}

							if (!seen)
								unique[kept++] = expected[i];
						}

						if (kept < expected.Length)
						{
							expected = new string[kept];
							global::System.Array.Copy(unique, expected, kept);
						}
					}

					if (expected.Length == 1)
						return "Expected " + expected[0] + ".";

					return "Expected " +
						string.Join(", ", expected, 0, expected.Length - 1) +
						" or " + expected[expected.Length - 1] + ".";
				}
			}

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
				return new Match<T>(Outcome.Success, value, position, length, null, null, null);
			}

			internal static Match<T> Failed(
				Outcome outcome, string otherwise, long position, string[]? expected,
				global::System.Collections.Generic.List<string[]>? tied)
			{
				return new Match<T>(outcome, default!, position, 0, expected, tied, otherwise);
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
	internal static string FailureStructWith(
		bool reach, bool starved = false, bool expected = false, bool expectedMore = false) =>
		Lines.Normalize(FailureStruct)
			.Replace(
				"\t{{reach}}" + Lines.Ending,
				reach ? Lines.Normalize(ReachField) + Lines.Ending : "")
			.Replace(
				"\t{{starved}}" + Lines.Ending,
				starved ? Lines.Normalize(StarvedField) + Lines.Ending : "")
			.Replace(
				"\t{{expected}}" + Lines.Ending,
				expected ? Lines.Normalize(ExpectedField) + Lines.Ending : "")
			.Replace(
				"\t{{expectedMore}}" + Lines.Ending,
				expectedMore ? Lines.Normalize(ExpectedMoreField) + Lines.Ending : "");

	const string FailureStruct = """
		/// <summary>Where a match got before it gave up, and why.</summary>
		struct Failure
		{
			/// <summary>
			/// The furthest position the input was followed to. Zero on a match that
			/// succeeded without ever backtracking, and meaningless unless one failed.
			/// </summary>
			public int Position;

			/// <summary>
			/// Where something wanted more input than remained, one past the position, or
			/// zero — what <c>Outcome</c> tells "the input ran out" from "the input did
			/// not fit" by (§7.5).
			/// </summary>
			/// <remarks>
			/// <para>
			/// A position rather than a flag, and written straight here rather than
			/// threaded through <c>Fail:</c> like <c>Expected</c>: what the boundary asks
			/// is whether the <em>furthest</em> failure ran out, so a room check that
			/// failed somewhere the parse later got past answers by not matching
			/// <c>Position</c>. Nothing has to be adopted, nothing has to be cleared, and
			/// the automaton's own unwinding is untouched — which is why this costs a
			/// store on a failure path and nothing anywhere else.
			/// </para>
			/// <para>
			/// One past, because a zeroed struct has to mean "nowhere" and zero is a
			/// position. Only a test wanting more than one character writes it: one
			/// wanting a single character can only fail for want of room at the very end
			/// of the input, which the boundary reads off <c>Position</c> itself.
			/// </para>
			/// </remarks>
			// A grammar whose every test wants one character never writes this — and a
			// field nothing assigns is a warning in somebody else's build, which for a
			// build that treats warnings as errors is a broken compilation of a file
			// they did not write.
			#pragma warning disable 0649
			public int OutOfInput;
			#pragma warning restore 0649
			{{reach}}
			{{starved}}
			{{expected}}
			{{expectedMore}}
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
	/// What would have fit where the match gave up, or null when nothing at the furthest
	/// position was a plain literal or element test.
	/// </summary>
	/// <remarks>
	/// Set beside <c>Position</c>, at the same moment and by the same reasoning: replaced
	/// on a new furthest position, added to on a tie with the current one, left alone
	/// otherwise. Threaded the same way — a second field on a struct already passed by
	/// <c>ref</c>, not a new parameter.
	/// <para>
	/// The ordinary case — a furthest position that keeps advancing, which is most of
	/// them, including every attempt that goes on to succeed — costs a reference
	/// assignment and nothing more: a plain array straight from wherever the generator
	/// already declared it, not a copy. <see cref="ExpectedMoreField"/> is the one that
	/// costs something, and only a grammar that can actually reach a tie carries it.
	/// </para>
	/// </remarks>
	const string ExpectedField = """

			/// <summary>
			/// What would have fit at the furthest position, or null. Meaningless unless
			/// the match failed. A reference into one of the generator's own arrays, not
			/// a copy of it.
			/// </summary>
			public string[]? Expected;
		""";

	/// <summary>
	/// The field a grammar needs beside <see cref="ExpectedField"/> only if it can ever
	/// reach a genuine tie for the furthest position — a flat-lowered grammar's single,
	/// unconditional attempt never can (<c>Machine.Flat.cs</c>'s own <c>Fail:</c>), so it
	/// declares <see cref="ExpectedField"/> alone and would otherwise carry a field
	/// nothing ever assigns.
	/// </summary>
	const string ExpectedMoreField = """

			/// <summary>
			/// A second array and beyond, where more than one terminal tied for the
			/// furthest position. Null until an actual tie needs one.
			/// </summary>
			public global::System.Collections.Generic.List<string[]>? ExpectedMore;
		""";

	/// <summary>The reusable state owned by the automaton.</summary>
	const string ParserRuntimeTemplate = """
		private sealed class Parser
		{
			internal readonly ParserArena Entries = new ParserArena();
			object?[] _values = global::System.Array.Empty<object?>();
			/*TYPED_FIELDS*/
			/*CACHE_FIELD*/
			int[] _linkHeads = global::System.Array.Empty<int>();
			int[] _linkNexts = global::System.Array.Empty<int>();

			/// <summary>
			/// The calls whose values the accepted derivation reaches, in the order they were
			/// reached from the root.
			/// </summary>
			/// <remarks>
			/// Written afresh on every materialization and read back to front, so unlike the
			/// link tables it needs no initial value — what is past the count written this
			/// time is never looked at.
			/// </remarks>
			int[] _owners = global::System.Array.Empty<int>();
			/*MARK_FIELD*/
			internal int LinkedUpTo;
			int _valuesUsed;

			internal object?[] Materialization(int count)
			{
				// Doubled, not sized to fit exactly: a `when` guard calls this once per
				// evaluation, and a repeated record with a guard inside the repeat would
				// otherwise pay for a fresh copy of the whole table on every turn, turning
				// an O(n) parse back into the O(n^2) the incremental materializer exists to
				// avoid.
				if (_values.Length < count)
					global::System.Array.Resize(ref _values, global::System.Math.Max(count, _values.Length * 2));
				/*TYPED_RESIZE*/
				/*CACHE_RESIZE*/

				// Grown here, alongside the value table, rather than where the links are
				// read — a guard that finds everything it needs already built calls this
				// and nothing else, and a link table sized only where it is read would fall
				// out of step with `_valuesUsed`, which is exactly what Reset and Truncate
				// walk off the end of.
				Grow(ref _linkHeads, count);
				Grow(ref _linkNexts, count);

				// No `Grow`: nothing here is carried between calls, so a plain resize is
				// what it needs and the -1 fill would be work for nobody.
				if (_owners.Length < count)
					global::System.Array.Resize(ref _owners, global::System.Math.Max(count, _owners.Length * 2));

				/*MARK_RESIZE*/
				_valuesUsed = count;

				return _values;
			}

			/*TYPED_ACCESS*/
			/*CACHE_ACCESS*/
			internal int[] MaterializationHeads() => _linkHeads;
			internal int[] MaterializationNexts() => _linkNexts;
			internal int[] MaterializationOwners() => _owners;
			/*MARK_ACCESS*/

			// Grown, not rebuilt: a link written for an index below `count` on an earlier,
			// smaller call is still the answer for that index, and re-zeroing it would erase
			// it. Only the newly reachable slots need a fresh -1.
			static void Grow(ref int[] links, int count)
			{
				if (links.Length < count)
				{
					var from = links.Length;

					global::System.Array.Resize(ref links, global::System.Math.Max(count, from * 2));

					for (var i = from; i < links.Length; i++)
						links[i] = -1;
				}
			}

			/*CACHE_TRUNCATE*/
			internal void Reset()
			{
				Entries.Clear();
				global::System.Array.Clear(_values, 0, _valuesUsed);
				/*TYPED_RESET*/
				/*CACHE_RESET*/

				// A rule call that captures nothing this parse never writes its own head, so
				// whatever a previous parse through the same pooled slot left there has to be
				// cleared here instead — otherwise a lookup falls through to a stale chain from
				// an earlier parse, one that can splice into a cycle once enough reuse has
				// pointed two heads at each other.
				for (var i = 0; i < _valuesUsed; i++)
				{
					_linkHeads[i] = -1;
					_linkNexts[i] = -1;
				}

				_valuesUsed = 0;
				LinkedUpTo = 0;
			}
		}

		private sealed class ParserArena
		{
			ParserEntry[] _items = global::System.Array.Empty<ParserEntry>();

			internal int Count { get; private set; }

			internal int Capacity { get { return _items.Length; } }

			internal ParserEntry this[int index]
			{
				get => _items[index];
				set => _items[index] = value;
			}

			internal void Add(ParserEntry entry)
			{
				if (Count == _items.Length)
					global::System.Array.Resize(ref _items, Count == 0 ? 16 : Count * 2);

				_items[Count++] = entry;
			}

			internal void RemoveAt(int index)
			{
				var after = Count - index - 1;

				if (after > 0)
					global::System.Array.Copy(_items, index + 1, _items, index, after);

				Count--;
			}

			internal void RemoveRange(int index, int count)
			{
				if (count == 0)
					return;

				var after = Count - index - count;

				if (after > 0)
					global::System.Array.Copy(_items, index + count, _items, index, after);

				Count -= count;
			}

			internal void Clear()
			{
				Count = 0;
			}
		}

		private readonly struct ParserEntry
		{
			internal const int Choice = 1;
			internal const int Call   = 2;
			internal const int Atomic = 3;
			internal const int Repeat = 4;
			internal const int Lookahead = 5;
			internal const int Capture = 6;
			internal const int Construct = 7;
			internal const int Completed = 8;
			internal const int RuleCapture = 9;
			internal const int Dead = 10;
			internal const int Recovery = 11;
			internal const int PendingRecovery = 12;
			internal const int Run = 13;

			/// <summary>
			/// The one way out a settled repetition keeps standing, in place of a
			/// <see cref="Choice"/> per turn.
			/// </summary>
			/// <remarks>
			/// Valid only while it is the loop's latest: the <see cref="Repeat"/> entry it
			/// points back to holds, in its rule-index field, where the last completed turn
			/// ended, and an exit whose own position no longer matches is history — popped
			/// past, never resumed. That is what turns a failure that used to resume one
			/// exit per completed turn, re-reading the suffix each time, into one that
			/// resumes a single exit and skips the rest.
			/// </remarks>
			internal const int LoopExit = 14;

			/// <summary>
			/// A completed turn of a counted repetition, standing where unwinding can see
			/// it.
			/// </summary>
			/// <remarks>
			/// The count in a <see cref="Repeat"/> entry is rewritten in place, and an
			/// in-place rewrite survives backtracking that the turn it counted does not:
			/// resume an alternative inside a completed turn and the body re-completes,
			/// counting the same turn twice — <c>X{2}</c> read two of a thing the input
			/// held one of. Popping this entry is what un-counts the turn, at the exact
			/// moment the parse abandons it.
			/// </remarks>
			internal const int TurnDone = 15;

			/// <summary>
			/// Where a capture began, standing where unwinding can take it away again.
			/// </summary>
			/// <remarks>
			/// A start kept in a variable is right for exactly as long as nothing opens the
			/// same capture between the opening and the close. Two things do: a rule that
			/// reaches itself, and a repetition whose next turn begins before a door inside
			/// the turn before it has been passed. Both leave the variable holding a start
			/// the parse has given back, and backtracking restores the arena and nothing
			/// else — so the start goes in the arena, and the close finds its own by
			/// counting these against the <see cref="Capture"/> entries that closed them,
			/// the way brackets are counted. Marking an opening closed in place would not
			/// do: an in-place rewrite survives backtracking that the close it recorded
			/// does not, which is the same thing <see cref="TurnDone"/> exists to avoid.
			/// </remarks>
			internal const int CaptureOpen = 16;

			/// <summary>A mark going up over what follows, and the one taking it down again.</summary>
			/// <remarks>
			/// Inert while the text is read: nothing dispatches on these and nothing restores
			/// anything when unwinding pops one — being gone <em>is</em> the restoration, and
			/// it is why a mark needs no save-and-restore of its own. What reads them is the
			/// walk that runs the factories once a derivation is accepted, over an arena that
			/// by then holds only what was accepted (§7.8).
			/// </remarks>
			internal const int StateSet = 17;

			/// <inheritdoc cref="StateSet"/>
			internal const int StateEnd = 18;

			internal ParserEntry(
				int kind, int state, int position, int callIndex, int atomicIndex,
				int repeatIndex, int lookaheadIndex, int value, int ruleIndex = -1/*POWER_PARAMETER*/)
			{
				Kind        = kind;
				State       = state;
				Position    = position;
				CallIndex   = callIndex;
				AtomicIndex = atomicIndex;
				RepeatIndex = repeatIndex;
				LookaheadIndex = lookaheadIndex;
				Value       = value;
				RuleIndex   = ruleIndex;
				/*POWER_ASSIGNMENT*/
			}

			internal int Kind        { get; }
			internal int State       { get; }
			internal int Position    { get; }
			internal int CallIndex   { get; }
			internal int AtomicIndex { get; }
			internal int RepeatIndex { get; }
			internal int LookaheadIndex { get; }
			internal int Value       { get; }
			internal int RuleIndex   { get; }
			/*POWER_PROPERTY*/
		}

		static partial void RentParser(ref Parser parser);
		static partial void ReturnParser(Parser parser);

		/// <summary>The last parser this thread used, kept for the next parse on it.</summary>
		/// <remarks>
		/// A parse allocates nothing it can help — the arena, the value table and the links
		/// are all grown once and reused — but that is only true of a parser that outlives
		/// the parse. Without this, every call built the whole machinery from nothing and
		/// grew the arena from empty by doubling, which for a parse of any size costs more
		/// than the parse.
		/// <para>
		/// One slot, taken out of the field while it is in use, so a parse reached from
		/// inside another — a guard that parses, a value that does — gets its own rather
		/// than sharing. A parser larger than <c>KeptEntries</c> is let go instead of kept,
		/// so a truly outsized input does not leave every thread holding its arena for
		/// ever. The bound is generous on purpose, and by measurement: at 4,096 an
		/// ordinary 12 KB document sat just over it, so every parse of it rebuilt the
		/// machinery — 1.13 ms and 3.8 MB against 0.85 ms and 315 KB kept, the difference
		/// being everything but the tree. Trimming the tables instead of dropping them
		/// was tried and measured slower than either: the trim is itself large-object
		/// allocation, once per parse. At 65,536 entries the retained machinery is a few
		/// megabytes — the working set of a parser whose documents are that size — and
		/// anything past it is the pathology the letting-go is for.
		/// </para>
		/// </remarks>
		[global::System.ThreadStatic]
		static Parser? _spareParser;

		const int KeptEntries = 65536;

		static Parser Recycled()
		{
			var spare = _spareParser;

			if (spare == null)
				return new Parser();

			_spareParser = null;

			return spare;
		}

		static void Recycle(Parser parser)
		{
			if (parser.Entries.Capacity <= KeptEntries)
				_spareParser = parser;
		}

		/// <summary>
		/// One line per step of the automaton, on standard error, when the build defines
		/// <c>DOTGRAM_TRACE</c> — nothing else to configure, and when it does not, the
		/// calls are removed at their sites, arguments and all.
		/// </summary>
		[global::System.Diagnostics.Conditional("DOTGRAM_TRACE")]
		static void Trace(string action, int state, int position, int arena)
		{
			global::System.Console.Error.WriteLine(
				".Gram " + action + " state=" + state.ToString() +
				" at " + position.ToString() + " arena=" + arena.ToString());
		}

		/// <summary>
		/// The same line with the rule it happened in and a window of the input around
		/// the position, the caret marking the position itself.
		/// </summary>
		[global::System.Diagnostics.Conditional("DOTGRAM_TRACE")]
		static void Trace(
			string action, int state, int position, int arena,
			global::System.ReadOnlySpan<char> text, string rule)
		{
			var from   = position > 16 ? position - 16 : 0;
			var to     = position + 16 < text.Length ? position + 16 : text.Length;
			var window =
				(from < position && position <= text.Length ? text.Slice(from, position - from).ToString() : "") +
				"^" +
				(position >= 0 && position < to ? text.Slice(position, to - position).ToString() : "");

			window = window
				.Replace("\r", "\\r")
				.Replace("\n", "\\n")
				.Replace("\t", "\\t");

			global::System.Console.Error.WriteLine(
				".Gram " + action + (rule.Length > 0 ? " in " + rule : "") +
				" state=" + state.ToString() + " at " + position.ToString() +
				" \"" + window + "\" arena=" + arena.ToString());
		}
		""";

	/// <summary>
	/// A table of its own for every type a rule's value can have.
	/// </summary>
	/// <remarks>
	/// <para>
	/// One automaton serves every rule, so what completed at a position could be any of
	/// their types, and one table held them all — which is what <c>object?</c> is for, and
	/// what boxes every value that is a struct. A <c>: @int</c> inside a repetition pays for
	/// it once a turn.
	/// </para>
	/// <para>
	/// So each type gets its own array instead, indexed the same way, and the object table
	/// keeps only the mark that says a value is reachable and unbuilt — a reference either
	/// way, and never a value. Nothing is boxed on the way in, and nothing is cast on the way
	/// out. What it costs is an array per type rather than one, pooled with the parser and
	/// grown once.
	/// </para>
	/// </remarks>
	internal static string ParserRuntime(
		bool powers, bool caches, bool marks, IReadOnlyList<string> valueTypes)
	{
		var fields = new StringBuilder();
		var resize = new StringBuilder();
		var access = new StringBuilder();
		var reset  = new StringBuilder();

		for (var i = 0; i < valueTypes.Count; i++)
		{
			fields.Append(valueTypes[i]).Append("[] _values").Append(i)
				.Append(" = global::System.Array.Empty<").Append(valueTypes[i]).Append(">();");
			resize.Append("if (_values").Append(i).Append(".Length < count)\n\tglobal::System.Array.Resize(ref _values")
				.Append(i).Append(", global::System.Math.Max(count, _values").Append(i).Append(".Length * 2));");
			access.Append("internal ").Append(valueTypes[i]).Append("[] Materialization").Append(i)
				.Append("() { return _values").Append(i).Append("; }\n");

			// Cleared with the parser, not merely overwritten by the next parse: a pooled
			// parser that kept a typed table full of the previous document's values was
			// holding that document's whole tree alive from a thread-static field.
			reset.Append("global::System.Array.Clear(_values").Append(i)
				.Append(", 0, global::System.Math.Min(_valuesUsed, _values").Append(i).Append(".Length));");

			if (i + 1 >= valueTypes.Count)
				continue;

			fields.Append('\n');
			resize.Append('\n');
			reset.Append('\n');
		}

		var runtime = ParserRuntimeTemplate
			.Replace("/*POWER_PARAMETER*/", powers ? ", int power = 0" : "")
			.Replace("\t\t/*POWER_ASSIGNMENT*/\r\n", powers ? "\t\tPower       = power;\r\n" : "")
			.Replace("\t\t/*POWER_ASSIGNMENT*/\n", powers ? "\t\tPower       = power;\n" : "")
			.Replace("\t/*POWER_PROPERTY*/\r\n", powers ? "\tinternal int Power       { get; }\r\n" : "")
			.Replace("\t/*POWER_PROPERTY*/\n", powers ? "\tinternal int Power       { get; }\n" : "");

		runtime = CacheRuntime(runtime, "TYPED_FIELDS", fields.ToString(), valueTypes.Count > 0);
		runtime = CacheRuntime(runtime, "TYPED_RESIZE", resize.ToString(), valueTypes.Count > 0);
		runtime = CacheRuntime(runtime, "TYPED_ACCESS", access.ToString(), valueTypes.Count > 0);
		runtime = CacheRuntime(runtime, "TYPED_RESET", reset.ToString(), valueTypes.Count > 0);

		runtime = CacheRuntime(runtime, "CACHE_FIELD",
			"bool[] _built = global::System.Array.Empty<bool>();", caches);
		runtime = CacheRuntime(runtime, "CACHE_RESIZE",
			"if (_built.Length < count)\n\tglobal::System.Array.Resize(ref _built, global::System.Math.Max(count, _built.Length * 2));", caches);
		runtime = CacheRuntime(runtime, "CACHE_ACCESS",
			"internal bool[] Materialized() => _built;\n", caches);
		runtime = CacheRuntime(runtime, "CACHE_TRUNCATE",
			"internal void Truncate(int count, ParserArena entries)\n{\n\tif (count < _valuesUsed)\n\t{\n\t\t// Descending, and checked against the arena rather than assumed: a link\n\t\t// prepended by the derivation being discarded may still be the head for\n\t\t// its call, and popping it here — the same order it was pushed in — is\n\t\t// what stops that call's chain from pointing at a slot the next\n\t\t// derivation through it is about to reuse for something else entirely.\n\t\tfor (var i = _valuesUsed - 1; i >= count; i--)\n\t\t{\n\t\t\tvar callIndex = entries[i].CallIndex;\n\n\t\t\tif (callIndex >= 0 && _linkHeads[callIndex] == i)\n\t\t\t\t_linkHeads[callIndex] = _linkNexts[i];\n\n\t\t\t_linkHeads[i] = -1;\n\t\t\t_linkNexts[i] = -1;\n\t\t}\n\n\t\tglobal::System.Array.Clear(_values, count, _valuesUsed - count);\n\t\tglobal::System.Array.Clear(_built, count, _valuesUsed - count);\n\n\t\t_valuesUsed = count;\n\t}\n\n\tif (count < LinkedUpTo)\n\t\tLinkedUpTo = count;\n}\n", caches);
		runtime = CacheRuntime(runtime, "CACHE_RESET",
			"global::System.Array.Clear(_built, 0, _valuesUsed);", caches);

		// One int per arena slot, and it says two things without conflicting: at a `StateSet`
		// it is the mark that encloses it, and everywhere else the innermost mark standing
		// over it. Nothing else can sit at a `StateSet`'s own index, so the two readings
		// never meet — and that is what turns "which marks are in force here" from a scan
		// into following a chain. Not grown like the link tables: nothing is carried from
		// one materialization to the next.
		runtime = CacheRuntime(runtime, "MARK_FIELD",
			"int[] _marks = global::System.Array.Empty<int>();", marks);
		runtime = CacheRuntime(runtime, "MARK_RESIZE",
			"if (_marks.Length < count)\n\tglobal::System.Array.Resize(ref _marks, global::System.Math.Max(count, _marks.Length * 2));", marks);
		runtime = CacheRuntime(runtime, "MARK_ACCESS",
			"internal int[] MaterializationMarks() { return _marks; }", marks);

		return runtime;
	}

	static string CacheRuntime(string runtime, string marker, string replacement, bool caches)
	{
		foreach (var ending in new[] { "\r\n", "\n" })
		{
			var token = "/*" + marker + "*/";
			var at = runtime.IndexOf(token, StringComparison.Ordinal);

			if (at < 0)
				continue;

			var line = runtime.LastIndexOf('\n', at);
			var indent = runtime.Substring(line + 1, at - line - 1);
			var whole = indent + token + ending;

			if (runtime.IndexOf(whole, StringComparison.Ordinal) < 0)
				continue;

			var written = caches
				? indent + replacement.Replace("\n", ending + indent) + ending
				: "";

			return runtime.Replace(whole, written);
		}

		return runtime;
	}

	/// <summary>
	/// The typed value tables a direct materialization writes into, one per type a rule
	/// can produce and indexed by record — the same tables the engine keeps in its
	/// <c>Parser</c>, kept here without the arena around them. Rented per parse and kept
	/// per thread, cleared on the way back so a pooled table holds no document alive.
	/// </summary>
	/// <remarks>
	/// Each table holds its values in a one-field struct rather than directly. An array of
	/// a reference type is covariant in .NET — a <c>Derived[]</c> is a <c>Base[]</c> — so
	/// every store into one asks the runtime whether the value fits the array it is going
	/// into, and the answer cannot be known at compile time for a table held in a field.
	/// The check was a tenth of what building a tree cost. An array of structs is not
	/// covariant, and a store into a field of one asks nothing.
	/// </remarks>
	internal static string DirectValuesClass(IReadOnlyList<string> valueTypes, string? stateType = null)
	{
		var text = new StringBuilder();

		text.Append("sealed class DirectValues\n{\n");

		for (var i = 0; i < valueTypes.Count; i++)
			text.Append("\tinternal Held<").Append(valueTypes[i]).Append(">[] V").Append(i)
				.Append(" = new Held<").Append(valueTypes[i]).Append(">[16];\n");

		text.Append("\tinternal bool[] Live   = new bool[16];\n");
		text.Append("\tinternal int[]  Starts = new int[16];\n");
		text.Append("\tinternal bool[] Built  = new bool[16];\n");

		if (stateType is not null)
		{
			text.Append("\tinternal ").Append(stateType).Append("[] MarkState = new ").Append(stateType).Append("[8];\n");
		}

		text.Append("\tint _used;\n\n");
		text.Append("\t[global::System.ThreadStatic]\n\tstatic DirectValues? _spare;\n\n");
		text.Append("\tinternal static DirectValues Rent()\n\t{\n\t\tvar spare = _spare;\n\n\t\tif (spare == null)\n\t\t\treturn new DirectValues();\n\n\t\t_spare = null;\n\n\t\treturn spare;\n\t}\n\n");
		text.Append("\tinternal static void Return(DirectValues values)\n\t{\n");

		for (var i = 0; i < valueTypes.Count; i++)
			text.Append("\t\tglobal::System.Array.Clear(values.V").Append(i).Append(", 0, global::System.Math.Min(values._used, values.V").Append(i).Append(".Length));\n");

		text.Append("\t\tglobal::System.Array.Clear(values.Built, 0, global::System.Math.Min(values._used, values.Built.Length));\n");

		text.Append("\t\tvalues._used = 0;\n\t\t_spare = values;\n\t}\n\n");
		text.Append("\t/// <summary>Room for a value at every index below the count; what was built stays built.</summary>\n");
		text.Append("\tinternal void Room(int count, bool live = true)\n\t{\n\t\tif (count > _used) _used = count;\n");
		text.Append("\t\tif (Live.Length < count)\n\t\t{\n\t\t\tLive   = new bool[global::System.Math.Max(count, Live.Length * 2)];\n\t\t\tStarts = new int[Live.Length];\n\t\t\tvar built = new bool[Live.Length];\n\t\t\tglobal::System.Array.Copy(Built, built, Built.Length);\n\t\t\tBuilt  = built;\n\t\t}\n\t\telse if (live)\n\t\t\tglobal::System.Array.Clear(Live, 0, count);\n");

		for (var i = 0; i < valueTypes.Count; i++)
			text.Append("\t\tif (V").Append(i).Append(".Length < count)\n\t\t\tglobal::System.Array.Resize(ref V").Append(i)
				.Append(", global::System.Math.Max(count, V").Append(i).Append(".Length * 2));\n");

		text.Append("\t}\n}\n\n");

		text.Append("/// <summary>One value in a table, in a struct so that storing it asks nothing.</summary>\n");
		text.Append("#pragma warning disable CS0649 // a table nothing writes still declares the field\n");
		text.Append("struct Held<T>\n{\n\tinternal T Value;\n}\n");
		text.Append("#pragma warning restore CS0649\n");

		return text.ToString().Replace("\n", Lines.Ending);
	}

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

public static partial class CSharpEmitter
{
	/// <summary>
	/// What a direct rendering needs beside the methods it writes: the tape of ways back,
	/// the refusal recorder, and the walk that says how far a literal run matched.
	/// </summary>
	/// <remarks>
	/// One copy per file, untagged: every direct rendering in the file shares it, and a
	/// machine rendered the other way never names it. The tape is rented per parse and
	/// kept per thread, the way the engine keeps its parser.
	/// </remarks>
	internal const string DirectSupport = """
		/// <summary>The ways back still open in a direct parse (Machine.Direct.cs).</summary>
		/// <remarks>
		/// Two integers per way: the alternative in force, and the last one there is. A
		/// way whose two are equal is spent — it stays on the tape so that a replay reads
		/// the same decisions in the same places, and is never taken again.
		/// </remarks>
		sealed class Ways
		{
			internal int[] Items = new int[32];

			/// <summary>How many ways are on the tape.</summary>
			internal int Count;

			/// <summary>The next way a replay reads; equal to <see cref="Count"/> when nothing is being replayed.</summary>
			internal int Cursor;

			/// <summary>How many lookaheads are open, during which no refusal is recorded.</summary>
			internal int Lookahead;

			/// <summary>
			/// What was recognized, for building values with once the parse has accepted: one
			/// record per completed valued rule, written after its children, each starting
			/// with its own length so that a walk from the front steps from record to record.
			/// </summary>
			internal int[] Log = new int[64];

			/// <summary>How much of the log is written.</summary>
			internal int LogCount;

			/// <summary>Where the record most recently finished begins: the value a caller captures.</summary>
			internal int Last = -1;

			/// <summary>
			/// How much of the log the values built for a guard still stand for: a record
			/// below this that was built need not be built again, and one above it was
			/// written since — the log was put back past it and has grown again.
			/// </summary>
			internal int Built;

			/// <summary>
			/// Captures collected while a rule runs and gathered into its record at the end:
			/// three integers each — the slot, and either a record and -1, or a start and end.
			/// </summary>
			internal int[] Refs = new int[48];

			/// <summary>How much of the side stack is in use.</summary>
			internal int RefsCount;

			int _record;

			[global::System.ThreadStatic]
			static Ways? _spare;

			internal static Ways Rent()
			{
				var spare = _spare;

				if (spare == null)
					return new Ways();

				_spare = null;
				spare.Count = 0;
				spare.Cursor = 0;
				spare.Lookahead = 0;
				spare.LogCount  = 0;
				spare.RefsCount = 0;
				spare.Last      = -1;
				spare.Built     = 0;

				return spare;
			}

			internal static void Return(Ways ways)
			{
				_spare = ways;
			}

			/// <summary>Opens a way at the end of the tape, in force at its first alternative.</summary>
			internal int Open(int last) => Open(0, last);

			/// <summary>Opens a way at the end of the tape, in force at <paramref name="at"/>.</summary>
			internal int Open(int at, int last)
			{
				if (Count * 2 + 2 > Items.Length)
					global::System.Array.Resize(ref Items, Items.Length * 2);

				Items[Count * 2]     = at;
				Items[Count * 2 + 1] = last;
				Count++;
				Cursor = Count;

				return Count - 1;
			}

			/// <summary>
			/// Takes the latest way decided since <paramref name="segment"/> that still has an
			/// alternative left, drops everything decided after it, and sets the replay to
			/// begin at the segment. False when none is left, and then nothing moves.
			/// </summary>
			/// <remarks>
			/// Only what stands before the cursor is the construct's own. During a replay the
			/// tape past the cursor is the future — decisions of what comes after, waiting to
			/// be read again — and a construct that fails on the way there, exactly as it did
			/// the first time, must leave that future alone.
			/// </remarks>
			internal bool Retry(int segment)
			{
				for (var way = Cursor - 1; way >= segment; way--)
				{
					if (Items[way * 2] < Items[way * 2 + 1])
					{
						Items[way * 2]++;
						Count  = way + 1;
						Cursor = segment;

						return true;
					}
				}

				return false;
			}

			/// <summary>
			/// Moves a way on to its next alternative once the one in force is spent, and
			/// drops what that alternative decided: the next one starts from nothing.
			/// </summary>
			internal void Next(int way, int value)
			{
				Items[way * 2] = value;
				Count  = way + 1;
				Cursor = way + 1;
			}

			/// <summary>
			/// <see cref="Next(int, int)"/>, and the way now reaches <paramref name="last"/>:
			/// as far as the alternative it moved to could be mended from.
			/// </summary>
			internal void Next(int way, int value, int last)
			{
				Items[way * 2]     = value;
				Items[way * 2 + 1] = last;
				Count  = way + 1;
				Cursor = way + 1;
			}

			/// <summary>Spends every way decided since the segment, keeping its decision.</summary>
			internal void Seal(int segment)
			{
				for (var way = segment; way < Cursor; way++)
					Items[way * 2 + 1] = Items[way * 2];
			}

			/// <summary>
			/// Opens a record: its length is written when it ends.
			/// </summary>
			/// <remarks>
			/// One number says which rule wrote it and which of that rule's alternatives,
			/// because the walk at the end wants both together and asking twice cost a
			/// switch inside a switch — two jump tables where a record needs one.
			/// </remarks>
			internal void Begin(int arm, int start, int end)
			{
				if (LogCount + 4 > Log.Length)
					global::System.Array.Resize(ref Log, Log.Length * 2 + 4);

				_record = LogCount;
				Log[LogCount++] = 0;
				Log[LogCount++] = arm;
				Log[LogCount++] = start;
				Log[LogCount++] = end;
			}

			internal void Put(int value)
			{
				if (LogCount + 1 > Log.Length)
					global::System.Array.Resize(ref Log, Log.Length * 2 + 1);

				Log[LogCount++] = value;
			}

			internal void Put(int a, int b)
			{
				if (LogCount + 2 > Log.Length)
					global::System.Array.Resize(ref Log, Log.Length * 2 + 2);

				Log[LogCount++] = a;
				Log[LogCount++] = b;
			}

			/// <summary>Closes the record: its length goes in front, and it becomes the last.</summary>
			internal void End(int refs)
			{
				Log[_record] = LogCount - _record;
				Last         = _record;
				RefsCount    = refs;
			}

			/// <summary>
			/// A mark placed or taken away (docs/syntax.md §7.8): a record of its own in the
			/// log, so that what was put back with the log takes its marks with it. The kind
			/// is -1 where the mark opens and -2 where it closes; nothing captures one.
			/// </summary>
			internal void Mark(int kind, int site, int at)
			{
				if (LogCount + 5 > Log.Length)
					global::System.Array.Resize(ref Log, Log.Length * 2 + 5);

				Log[LogCount++] = 5;
				Log[LogCount++] = kind;
				Log[LogCount++] = site;
				Log[LogCount++] = at;
				Log[LogCount++] = at;
			}

			/// <summary>A capture made inside a repetition, kept until the rule gathers it.</summary>
			internal void Push(int slot, int a, int b)
			{
				if (RefsCount + 3 > Refs.Length)
					global::System.Array.Resize(ref Refs, Refs.Length * 2 + 3);

				Refs[RefsCount++] = slot;
				Refs[RefsCount++] = a;
				Refs[RefsCount++] = b;
			}

			/// <summary>
			/// Writes what was pushed for the given slots since <paramref name="from"/>: how
			/// many, then each one — the record alone where <paramref name="pairs"/> is false,
			/// the start and end where it is true.
			/// </summary>
			internal void Collect(int from, long slots, bool pairs)
			{
				var count = 0;

				for (var at = from; at < RefsCount; at += 3)
					if ((slots & (1L << Refs[at])) != 0)
						count++;

				Put(count);

				for (var at = from; at < RefsCount; at += 3)
					if ((slots & (1L << Refs[at])) != 0)
					{
						if (pairs)
							Put(Refs[at + 1], Refs[at + 2]);
						else
							Put(Refs[at + 1]);
					}
			}
		}

		/// <summary>Records a refusal against the furthest one seen, as the engine's Fail does.</summary>
		static void Refuse_DotGram(ref Failure failure, int at, string[]? expected, Ways ways)
		{
			if (ways.Lookahead > 0)
				return;

			if (at > failure.Position)
			{
				failure.Position     = at;
				failure.Expected     = expected;
				failure.ExpectedMore = null;
			}
			else if (at == failure.Position && expected != null)
			{
				(failure.ExpectedMore ??= new global::System.Collections.Generic.List<string[]>()).Add(expected);
			}
		}

		/// <summary>How much of a run matched, asked only when it did not.</summary>
		static int Reach_DotGram(
			global::System.ReadOnlySpan<char> text, int pos, global::System.ReadOnlySpan<char> want)
		{
			var room = text.Length - pos;

			if (want.Length < room)
				room = want.Length;

			var at = 0;

			while (at < room && text[pos + at] == want[at])
				at++;

			return pos + at;
		}
		""";
}
