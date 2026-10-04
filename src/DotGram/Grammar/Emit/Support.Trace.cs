using System;

namespace DotGram.Grammar.Emit;

/// <summary>
/// What a trace build carries beside its parser, as text (GramCompilerOptions.Trace).
/// </summary>
/// <remarks>
/// <para>
/// Nested in the class the parser is written into, and public there, for the reason
/// <c>SourceSpan</c> is: a type in a namespace would have to be internal, and an internal type
/// cannot be what a public <c>Tracing</c> takes (CS0051), while two assemblies' copies of it
/// would collide where one sees the other's internals (CS0436). A host whose readings are
/// traced each carries its own; a sink that serves two of them is an adapter over both.
/// </para>
/// <para>
/// None of it is written where the build is not a trace build, and none of it costs a reading
/// that has no sink more than a field read where an event would be.
/// </para>
/// </remarks>
public static partial class CSharpEmitter
{
	/// <summary>
	/// The sink, the reading it is told about, and the scope that sets it.
	/// </summary>
	/// <remarks>
	/// An event is a virtual call with numbers: a rule is its index in the tables written beside
	/// this, a position is in the reading's own unit. Nothing is allocated per event; a reading
	/// with a sink allocates its <c>GramRead</c>, and over a span the copy of the text in it.
	/// </remarks>
	internal const string TraceApi = """
		/// <summary>
		/// What a traced reading of this grammar does, told one call a thing it does (docs/syntax.md, §6.9).
		/// </summary>
		/// <remarks>
		/// <para>
		/// Set with <see cref="Tracing"/>. Every reading begun in the same flow of control reports to
		/// it: one begun by a construction or a guard while another is read, one carried onto a
		/// stack of its own, one awaited in a task. Every method does nothing unless overridden.
		/// </para>
		/// <para>
		/// A position is in the reading's own unit: a character, or a token where the grammar is
		/// read as tokens, which <see cref="GramRead.CharacterOf"/> turns into a character. A rule
		/// is a number, named by <see cref="GramRead.RuleName"/>; a rule that only hands on another
		/// rule's value is compiled into the rules that call it and reports where it stood all the
		/// same. A reading on the engine rather than on methods reports its beginning and its end
		/// and nothing between them yet.
		/// </para>
		/// <para>
		/// A trace build is a slightly different program from the one it observes: every return of
		/// a rule reports it, so what is timed is the trace build. What it answers is the same.
		/// </para>
		/// <para>
		/// A sink is the host's own code, as a guard or a construction is: what one of its methods
		/// throws goes out of the call that was reading, and the reading stops there. Whatever
		/// leaves a reading by an exception — a sink, a guard, a construction — the sink is told
		/// all the same that every rule it was told was entered is left, innermost first, with
		/// <see cref="Thrown"/> for an end, and then that the reading ended, refused. So every
		/// <see cref="Enter"/> has its <see cref="Exit"/> and every <see cref="Begin"/> its
		/// <see cref="End"/>. What a sink throws while it is told this is not let out: the
		/// exception that left the reading is.
		/// </para>
		/// </remarks>
		public abstract class GramTrace
		{
			/// <summary>
			/// The end <see cref="Exit"/> is told for a rule an exception left: neither read nor
			/// failed, abandoned.
			/// </summary>
			public const int Thrown = -2;

			/// <summary>
			/// A reading begins. A call may read more than once: a <c>TryParse</c> that refuses
			/// reads quietly first and again recording, and a <c>find</c> reads at every start.
			/// </summary>
			public virtual void Begin(GramRead read)
			{
			}

			/// <summary>
			/// The reading ends where <paramref name="end"/> says, or -1 where it refused, with the
			/// furthest refusal it recorded: the position and the sets the match's message is
			/// worded from. The arrays are the parser's own and are not to be written into.
			/// </summary>
			public virtual void End(
				GramRead read, int end, int position, string[]? expected,
				global::System.Collections.Generic.IReadOnlyList<string[]>? expectedMore)
			{
			}

			/// <summary>A rule is entered at a position.</summary>
			public virtual void Enter(int rule, int position)
			{
			}

			/// <summary>
			/// The rule entered at <paramref name="position"/> ends at <paramref name="end"/>, or
			/// fails where that is -1, or was left by an exception where it is <see cref="Thrown"/>.
			/// </summary>
			public virtual void Exit(int rule, int position, int end)
			{
			}

			/// <summary>
			/// A rule entered where it has already failed in this reading fails at once, without
			/// being read again: the reading remembers. It is neither entered nor exited.
			/// </summary>
			public virtual void Remembered(int rule, int position)
			{
			}

			/// <summary>A guard (<c>when</c>) was asked at a position, and let the reading on or not.</summary>
			public virtual void Guard(int guard, int position, bool passed)
			{
			}

			/// <summary>
			/// A recording reading could not go on at a position, wanting one of
			/// <paramref name="expected"/>, or saying nothing of what where it is null. Quiet
			/// readings and lookaheads refuse without a word, as their refusals are never the
			/// answer.
			/// </summary>
			public virtual void Refused(int position, string[]? expected)
			{
			}

			/// <summary>
			/// A repetition marked <c>recover</c> stepped over an element it could not read: from
			/// where the element began to where reading goes on, the element having got as far as
			/// <paramref name="reach"/>.
			/// </summary>
			public virtual void Recovered(int rule, int from, int to, int reach)
			{
			}

			/// <summary>The reading ran its stack low at a position and goes on on a stack of its own.</summary>
			public virtual void Deepened(int position)
			{
			}

			/// <summary>
			/// No token of the grammar begins at a character of <paramref name="text"/>, so the
			/// input was refused before any rule was read.
			/// </summary>
			public virtual void Unlexed(string text, int position)
			{
			}

			/// <summary>
			/// A call answered with a refusal: where, as the match says it, and the match's
			/// message.
			/// </summary>
			public virtual void Rejected(long position, string message)
			{
			}
		}

		/// <summary>One reading of an input, as a sink is told it began.</summary>
		/// <remarks>
		/// The reading's own bookkeeping as well: every event goes through it to the sink, and it
		/// keeps the rules entered and not yet left, so that an exception that leaves the reading
		/// can leave them for the sink.
		/// </remarks>
		public sealed class GramRead
		{
			readonly int[]? _starts;
			readonly int _count;
			readonly int _end;
			int[]? _lines;
			int[]? _openRules;
			int[]? _openAt;
			int _open;

			/// <summary>The sink this reading reports to.</summary>
			internal readonly GramTrace Sink;

			internal GramRead(
				GramTrace sink, string publication, bool quiet, bool finding, int start, string machine, string? text,
				int[]? starts, int count, int end)
			{
				Sink        = sink;
				Publication = publication;
				Quiet       = quiet;
				Finding     = finding;
				Start       = start;
				Machine     = machine;
				Text        = text;
				_starts     = starts;
				_count      = count;
				_end        = end;
			}

			/// <summary>The rule read, as the grammar publishes it.</summary>
			public string Publication { get; }

			/// <summary>
			/// Whether the reading records nothing, which is a different program from one that
			/// does: the first reading of a <c>TryParse</c>, a form that answers only yes or no,
			/// each start of a <c>find</c>.
			/// </summary>
			public bool Quiet { get; }

			/// <summary>Whether this is one of the starts a <c>find</c> tries.</summary>
			public bool Finding { get; }

			/// <summary>Where the reading begins, in its own unit.</summary>
			public int Start { get; }

			/// <summary>
			/// What reads it: <c>methods</c>, whose rules report what they do, or <c>engine</c> or
			/// <c>flat</c>, which report nothing between the beginning and the end yet.
			/// </summary>
			public string Machine { get; }

			/// <summary>Whether a position is a token rather than a character.</summary>
			public bool OverTokens
			{
				get { return _starts != null; }
			}

			/// <summary>The text read, where there is one to show.</summary>
			public string? Text { get; }

			/// <summary>Every rule's name, by the number its events carry.</summary>
			public global::System.Collections.Generic.IReadOnlyList<string> Rules
			{
				get { return TraceRules_DotGram; }
			}

			/// <summary>Every guard's C#, by the number its events carry.</summary>
			public global::System.Collections.Generic.IReadOnlyList<string> Guards
			{
				get { return TraceGuards_DotGram; }
			}

			/// <summary>A rule's name, from the number its events carry.</summary>
			public string RuleName(int rule)
			{
				return (uint)rule < (uint)TraceRules_DotGram.Length
					? TraceRules_DotGram[rule]
					: "#" + rule.ToString(global::System.Globalization.CultureInfo.InvariantCulture);
			}

			/// <summary>A guard's C#, from the number its events carry.</summary>
			public string GuardText(int guard)
			{
				return (uint)guard < (uint)TraceGuards_DotGram.Length
					? TraceGuards_DotGram[guard]
					: "#" + guard.ToString(global::System.Globalization.CultureInfo.InvariantCulture);
			}

			/// <summary>
			/// Whether a rule only hands on another rule's value, and so is compiled into the
			/// rules that call it: its events are written where it was called.
			/// </summary>
			public bool Forwards(int rule)
			{
				return (uint)rule < (uint)TraceForwarders_DotGram.Length && TraceForwarders_DotGram[rule];
			}

			/// <summary>
			/// Where a position is, in characters of the input: itself over characters, and over
			/// tokens where its token begins — past the last, where the tokens end. Good until
			/// the reading ends.
			/// </summary>
			public int CharacterOf(int position)
			{
				if (_starts == null)
					return position;

				if (position < 0)
					return 0;

				return position < _count ? _starts[position] : _end;
			}

			/// <summary>The line and column of a character, both counted from one.</summary>
			public void Locate(int character, out int line, out int column)
			{
				line   = 1;
				column = character + 1;

				var text = Text;

				if (text == null)
					return;

				if (_lines == null)
				{
					var found = new global::System.Collections.Generic.List<int>();

					found.Add(0);

					for (var at = 0; at < text.Length; at++)
						if (text[at] == '\n')
							found.Add(at + 1);

					_lines = found.ToArray();
				}

				var low  = 0;
				var high = _lines.Length - 1;

				while (low < high)
				{
					var middle = (low + high + 1) / 2;

					if (_lines[middle] <= character)
						low = middle;
					else
						high = middle - 1;
				}

				line   = low + 1;
				column = character - _lines[low] + 1;
			}

			/// <summary>A rule is entered: kept as open, and the sink told.</summary>
			internal void Enter(int rule, int position)
			{
				if (_openRules == null || _openAt == null)
				{
					_openRules = new int[32];
					_openAt    = new int[32];
				}
				else if (_open == _openRules.Length)
				{
					global::System.Array.Resize(ref _openRules, _open * 2);
					global::System.Array.Resize(ref _openAt, _open * 2);
				}

				_openRules[_open] = rule;
				_openAt[_open]    = position;
				_open++;

				Sink.Enter(rule, position);
			}

			/// <summary>The innermost rule open is left: no longer kept, and the sink told.</summary>
			internal void Exit(int rule, int position, int end)
			{
				if (_open > 0)
					_open--;

				Sink.Exit(rule, position, end);
			}

			/// <summary>
			/// An exception leaves the reading: every rule still open is left, innermost first, as
			/// thrown. What the sink throws meanwhile is dropped, the exception leaving being the
			/// one that is let out.
			/// </summary>
			internal void Unwind()
			{
				while (_open > 0)
				{
					_open--;

					try
					{
						Sink.Exit(_openRules![_open], _openAt![_open], GramTrace.Thrown);
					}
					catch (global::System.Exception)
					{
					}
				}
			}

			/// <summary>A rule answered from memory, told.</summary>
			internal void Remembered(int rule, int position)
			{
				Sink.Remembered(rule, position);
			}

			/// <summary>A guard asked, told.</summary>
			internal void Guard(int guard, int position, bool passed)
			{
				Sink.Guard(guard, position, passed);
			}

			/// <summary>A refusal recorded, told.</summary>
			internal void Refused(int position, string[]? expected)
			{
				Sink.Refused(position, expected);
			}

			/// <summary>An element stepped over, told.</summary>
			internal void Recovered(int rule, int from, int to, int reach)
			{
				Sink.Recovered(rule, from, to, reach);
			}

			/// <summary>A move to another stack, told.</summary>
			internal void Deepened(int position)
			{
				Sink.Deepened(position);
			}
		}

		/// <summary>The sink readings in this flow of control report to, where one is set.</summary>
		static readonly global::System.Threading.AsyncLocal<GramTrace?> Tracing_DotGram =
			new global::System.Threading.AsyncLocal<GramTrace?>();

		/// <summary>
		/// Sends what every reading begun in this flow of control does to <paramref name="sink"/>,
		/// until the scope is disposed, which puts back the sink that was there before.
		/// </summary>
		/// <remarks>
		/// The flow is the <c>ExecutionContext</c>'s: it reaches a task started inside the scope
		/// and a reading carried onto a stack of its own, and not a thread started otherwise. A
		/// null sink stops the tracing inside the scope.
		/// </remarks>
		public static global::System.IDisposable Tracing(GramTrace? sink)
		{
			var scope = new TraceScope_DotGram(Tracing_DotGram.Value);

			Tracing_DotGram.Value = sink;

			return scope;
		}

		/// <summary>Puts the sink back that a <see cref="Tracing"/> found, once.</summary>
		sealed class TraceScope_DotGram : global::System.IDisposable
		{
			readonly GramTrace? _previous;
			bool _disposed;

			internal TraceScope_DotGram(GramTrace? previous)
			{
				_previous = previous;
			}

			public void Dispose()
			{
				if (_disposed)
					return;

				_disposed = true;
				Tracing_DotGram.Value = _previous;
			}
		}

		/// <summary>
		/// A reading begins: where a sink is set, the sink is told, and the failure carries the
		/// reading, through which every event of it goes.
		/// </summary>
		static GramRead? Began_DotGram(
			ref Failure failure, GramTrace? sink, string publication, bool finding, int start, string machine, string? text,
			int[]? starts, int count, int end)
		{
			if (sink == null)
				return null;

			var read = new GramRead(sink, publication, {{quiet}}, finding, start, machine, text, starts, count, end);

			failure.Trace = read;
			sink.Begin(read);

			return read;
		}

		/// <summary>A reading ends: the sink is told where, and what the failure recorded.</summary>
		static void Ended_DotGram(ref Failure failure, GramRead? read, int end)
		{
			if (read != null)
				read.Sink.End(read, end, failure.Position, failure.Expected, {{more}});
		}

		/// <summary>
		/// A reading an exception leaves ends all the same, for the sink: every rule open left as
		/// thrown, then the reading told as a refusal, and the exception let through untouched.
		/// </summary>
		/// <remarks>
		/// Asked as the exception's filter, before anything between here and where it was thrown
		/// is unwound; what the sink throws here is dropped, the runtime would drop it anyway.
		/// </remarks>
		static bool Thrown_DotGram(ref Failure failure, GramRead? read)
		{
			if (read == null)
				return false;

			read.Unwind();

			try
			{
				Ended_DotGram(ref failure, read, -1);
			}
			catch (global::System.Exception)
			{
			}

			return false;
		}

		/// <summary>A call answers with a refusal: the sink is told what the match says.</summary>
		static {{match}}<T> Rejected_DotGram<T>({{match}}<T> match)
		{
			var sink = Tracing_DotGram.Value;

			if (sink != null && !match.IsSuccess)
				sink.Rejected(match.Position, match.Error ?? "");

			return match;
		}

		""";

	/// <summary>Where the lexer stops a call before any rule is read.</summary>
	internal const string TraceUnlexed = """
		/// <summary>No token begins at a character: the sink is told before the refusal is answered.</summary>
		static void Unlexed_DotGram(string text, int position)
		{
			var sink = Tracing_DotGram.Value;

			if (sink != null)
				sink.Unlexed(text, position);
		}

		""";

	/// <summary>
	/// The field a trace build's failure carries: the sink a reading took when it began, which goes
	/// wherever the failure goes — into every refusal and onto every stack a reading moves to.
	/// </summary>
	const string TraceField = """

			/// <summary>The reading the sink was told began, which every event goes through; null where no sink is set.</summary>
			{{suppress:Trace}}
			public GramRead? Trace;
			{{restore:Trace}}
		""";

	/// <summary>The reader's own: what a rule's return and a forwarding rule's frame write.</summary>
	internal const string TraceReaderMethods = """

		/// <summary>A rule, or a frame a forwarding rule stood for, is entered: told, and the position handed on.</summary>
		[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		int Entered_DotGram(int rule, int pos)
		{
			var trace = failure.Trace;

			if (trace != null)
				trace.Enter(rule, pos);

			return pos;
		}

		/// <summary>A rule, or a frame a forwarding rule stood for, ends: told, and the end handed on.</summary>
		[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
		int Exited_DotGram(int rule, int pos, int end)
		{
			var trace = failure.Trace;

			if (trace != null)
				trace.Exit(rule, pos, end);

			return end;
		}

		/// <summary>A guard was asked: told, and its answer handed on.</summary>
		bool Guarded_DotGram(int guard, int pos, bool passed)
		{
			var trace = failure.Trace;

			if (trace != null)
				trace.Guard(guard, pos, passed);

			return passed;
		}
		""";
}
