using System;

namespace DotGram.Grammar.Emit;

public static partial class CSharpEmitter
{
	/// <summary>
	/// The three sinks a trace build carries ready-made: why an input was refused, an indented log
	/// of what the reading did, and how often and how long each rule was read.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>GramWhy</c> does not work out what the message should have been. It keeps what each
	/// refusal at the furthest position was refused by — the stack of rules open there, linked
	/// frame to frame so that a refusal costs one entry however deep it is — and when the reading
	/// ends it keeps only those whose set is one of the sets the failure ended with, by reference.
	/// The message is the match's own, handed over by the call that answered with it. So what it
	/// says and what <c>Match.Error</c> says cannot disagree: every merging rule of the real
	/// refusal — a covered set dropped, a lookahead's refusals not counted, an <c>on fail</c> word
	/// taking the place of a set — has already been applied to what it is held against.
	/// </para>
	/// <para>
	/// None of them throws for what it is told, in whatever order: an exit with no entry, an end
	/// with no beginning and a number past the tables are kept or passed over. What they write
	/// to — <c>GramTraceLog</c>'s writer — is the caller's, and what it throws goes out of the
	/// reading as anything a sink throws does.
	/// </para>
	/// <para>
	/// Each is written for a reading at a time and is not safe to share between readings that
	/// run at once. A reading begun inside another — a construction that parses again — is
	/// bracketed by its own beginning and end; the log shows it nested, the profile counts it,
	/// and <c>GramWhy</c> explains the outermost.
	/// </para>
	/// </remarks>
	internal const string TraceSinks = """
		/// <summary>
		/// Why the last call watched was refused: the match's own message, and the rules that were
		/// open where the input was followed furthest, each with what it wanted there.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Explains the recording reading, the one whose refusal a match reports. A position is
		/// a character of the input with its line and column, a token having been turned into
		/// where it begins.
		/// </para>
		/// <para>
		/// Where a rule entered a second time at a position it had already failed at is answered
		/// from memory, the path through the second caller is not shown: only the first. A
		/// repetition marked <c>recover</c> is explained element by element in
		/// <see cref="Elements"/>, each with what it wanted where it stopped, and then the
		/// refusal itself if the reading still failed.
		/// </para>
		/// </remarks>
		public sealed class GramWhy : GramTrace
		{
			int _depth;
			GramRead? _read;
			Frame_DotGram? _top;
			int _furthest = -1;
			readonly global::System.Collections.Generic.List<Candidate_DotGram> _candidates =
				new global::System.Collections.Generic.List<Candidate_DotGram>();
			int _since = -1;
			readonly global::System.Collections.Generic.List<Candidate_DotGram> _sinceCandidates =
				new global::System.Collections.Generic.List<Candidate_DotGram>();
			Frame_DotGram? _frontier;
			int _frontierAt = -1;
			readonly global::System.Collections.Generic.List<Element> _elements =
				new global::System.Collections.Generic.List<Element>();

			GramRead? _pendingRead;
			Path[]? _pendingPaths;
			string _pendingCause = "";
			int _unlexed = -1;

			/// <summary>Whether the last call watched answered with a refusal.</summary>
			public bool IsRefused { get; private set; }

			/// <summary>The refusal's message: the match's <c>Error</c>, word for word.</summary>
			public string? Message { get; private set; }

			/// <summary>Where the refusal is, as the match says it.</summary>
			public long Position { get; private set; }

			/// <summary>The line of <see cref="Position"/>, from one, or zero where the text is not known.</summary>
			public int Line { get; private set; }

			/// <summary>The column of <see cref="Position"/>, from one, or zero where the text is not known.</summary>
			public int Column { get; private set; }

			/// <summary>What the explanation rests on, in a sentence.</summary>
			public string Cause { get; private set; } = "";

			/// <summary>The rules open where the input was followed furthest, each with what it wanted.</summary>
			public global::System.Collections.Generic.IReadOnlyList<Path> Paths { get; private set; } = new Path[0];

			/// <summary>The elements a repetition marked <c>recover</c> stepped over, in the order it met them.</summary>
			public global::System.Collections.Generic.IReadOnlyList<Element> Elements { get; private set; } = new Element[0];

			/// <summary>One stack of rules, outermost first, and what the innermost wanted.</summary>
			public sealed class Path
			{
				internal Path(string[] rules, int[] at, int[] lines, int[] columns, string[]? expected, string? guard)
				{
					Rules     = rules;
					Positions = at;
					Lines     = lines;
					Columns   = columns;
					Expected  = expected;
					Guard     = guard;
				}

				/// <summary>The rules, outermost first.</summary>
				public global::System.Collections.Generic.IReadOnlyList<string> Rules { get; }

				/// <summary>Where each was entered, in characters of the input.</summary>
				public global::System.Collections.Generic.IReadOnlyList<int> Positions { get; }

				/// <summary>The line each was entered on, from one.</summary>
				public global::System.Collections.Generic.IReadOnlyList<int> Lines { get; }

				/// <summary>The column each was entered at, from one.</summary>
				public global::System.Collections.Generic.IReadOnlyList<int> Columns { get; }

				/// <summary>What would have fit, as the refusal named it, or null where it named nothing.</summary>
				public global::System.Collections.Generic.IReadOnlyList<string>? Expected { get; }

				/// <summary>The guard that refused, where a guard did.</summary>
				public string? Guard { get; }

				/// <summary>The path on one line.</summary>
				public override string ToString()
				{
					var text = new global::System.Text.StringBuilder();

					if (Guard != null)
						text.Append("when ").Append(Guard).Append(" said no");
					else if (Expected != null && Expected.Count > 0)
						text.Append("wanted ").Append(Wanted_DotGram(Expected));
					else
						text.Append("refused");

					text.Append(" in ");

					var count = Rules.Count;

					for (var i = 0; i < count; i++)
					{
						// The middle of a stack thousands deep says nothing the ends do not.
						if (count > 16 && i == 4)
						{
							text.Append("... ").Append((count - 14).ToString(global::System.Globalization.CultureInfo.InvariantCulture)).Append(" more > ");
							i = count - 11;
						}

						text.Append(Rules[i]).Append(' ')
							.Append(Lines[i].ToString(global::System.Globalization.CultureInfo.InvariantCulture)).Append(':')
							.Append(Columns[i].ToString(global::System.Globalization.CultureInfo.InvariantCulture));

						if (i < count - 1)
							text.Append(" > ");
					}

					if (count == 0)
						text.Append("the reading itself");

					return text.ToString();
				}
			}

			/// <summary>An element a repetition marked <c>recover</c> stepped over, and why it was refused.</summary>
			public sealed class Element
			{
				internal Element(string rule, int from, int to, int reach, Path[] paths)
				{
					Rule  = rule;
					From  = from;
					To    = to;
					Reach = reach;
					Paths = paths;
				}

				/// <summary>The rule whose repetition it was.</summary>
				public string Rule { get; }

				/// <summary>Where it began, in characters of the input.</summary>
				public int From { get; }

				/// <summary>Where reading went on after it.</summary>
				public int To { get; }

				/// <summary>How far it got.</summary>
				public int Reach { get; }

				/// <summary>The rules open where it got furthest, each with what it wanted.</summary>
				public global::System.Collections.Generic.IReadOnlyList<Path> Paths { get; }
			}

			sealed class Frame_DotGram
			{
				internal readonly int Rule;
				internal readonly int At;
				internal readonly Frame_DotGram? Parent;
				internal readonly int Depth;

				internal Frame_DotGram(int rule, int at, Frame_DotGram? parent)
				{
					Rule   = rule;
					At     = at;
					Parent = parent;
					Depth  = parent == null ? 1 : parent.Depth + 1;
				}
			}

			struct Candidate_DotGram
			{
				internal Frame_DotGram? Frame;
				internal string[]? Set;
				internal int Guard;
			}

			/// <inheritdoc/>
			public override void Begin(GramRead read)
			{
				_depth++;

				if (_depth != 1)
					return;

				// A call begins: what the last one said is not this one's.
				IsRefused = false;
				Message   = null;
				Position  = 0;
				Line      = 0;
				Column    = 0;
				Cause     = "";
				Paths     = new Path[0];
				Elements  = new Element[0];

				if (read.Quiet)
				{
					_read = null;

					return;
				}

				_read         = read;
				_top          = null;
				_furthest     = -1;
				_since        = -1;
				_frontier     = null;
				_frontierAt   = -1;
				_candidates.Clear();
				_sinceCandidates.Clear();
				_elements.Clear();

				// A reading after a character no token begins with reads the tokens before it, to
				// say which rules were reading there: what the lexer's stop said stands until then.
				if (_unlexed >= 0)
					return;

				_pendingRead  = null;
				_pendingPaths = null;
				_pendingCause = "";
			}

			bool Watching
			{
				get { return _depth == 1 && _read != null; }
			}

			/// <inheritdoc/>
			public override void Enter(int rule, int position)
			{
				if (!Watching)
					return;

				_top = new Frame_DotGram(rule, position, _top);

				if (position >= _frontierAt)
				{
					_frontierAt = position;
					_frontier   = _top;
				}
			}

			/// <inheritdoc/>
			public override void Exit(int rule, int position, int end)
			{
				if (!Watching)
					return;

				// A frame left open — by an exception, or a reading carried off — is closed with the
				// first one under it that this exit is the exit of.
				for (var frame = _top; frame != null; frame = frame.Parent)
					if (frame.Rule == rule && frame.At == position)
					{
						_top = frame.Parent;

						return;
					}
			}

			/// <inheritdoc/>
			public override void Guard(int guard, int position, bool passed)
			{
				if (Watching && !passed)
					Consider(position, null, guard);
			}

			/// <inheritdoc/>
			public override void Refused(int position, string[]? expected)
			{
				if (Watching)
					Consider(position, expected, -1);
			}

			void Consider(int position, string[]? set, int guard)
			{
				Keep(_candidates, ref _furthest, position, set, guard);
				Keep(_sinceCandidates, ref _since, position, set, guard);
			}

			void Keep(global::System.Collections.Generic.List<Candidate_DotGram> kept, ref int furthest, int position, string[]? set, int guard)
			{
				if (position < furthest)
					return;

				if (position > furthest)
				{
					furthest = position;
					kept.Clear();
				}

				if (kept.Count > 0)
				{
					var last = kept[kept.Count - 1];

					if (ReferenceEquals(last.Frame, _top) && ReferenceEquals(last.Set, set) && last.Guard == guard)
						return;
				}

				var candidate = new Candidate_DotGram();

				candidate.Frame = _top;
				candidate.Set   = set;
				candidate.Guard = guard;

				kept.Add(candidate);
			}

			/// <inheritdoc/>
			public override void Recovered(int rule, int from, int to, int reach)
			{
				if (!Watching)
					return;

				var read  = _read!;
				var paths = new global::System.Collections.Generic.List<Path>();

				if (_since == reach)
					foreach (var candidate in _sinceCandidates)
						paths.Add(PathOf(read, candidate.Frame, candidate.Set, candidate.Guard));

				if (_elements.Count < 1000)
					_elements.Add(new Element(
						read.RuleName(rule), read.CharacterOf(from), read.CharacterOf(to), read.CharacterOf(reach),
						Distinct(paths)));

				_since = -1;
				_sinceCandidates.Clear();
			}

			/// <inheritdoc/>
			public override void End(
				GramRead read, int end, int position, string[]? expected,
				global::System.Collections.Generic.IReadOnlyList<string[]>? expectedMore)
			{
				// An end it was not told the beginning of leaves its count where it was.
				if (_depth > 0)
					_depth--;

				if (_depth != 0 || read == null || !ReferenceEquals(read, _read))
					return;

				_read    = null;
				Elements = _elements.ToArray();

				if (end >= 0)
				{
					// What a character no token begins with said stands where the tokens before it
					// read whole: they refused nothing where they ran out.
					if (_unlexed < 0)
					{
						_pendingRead  = null;
						_pendingPaths = null;
					}

					return;
				}

				var paths = new global::System.Collections.Generic.List<Path>();
				var sets  = expected != null || expectedMore != null && expectedMore.Count > 0;
				var cause = "";

				if (_furthest == position)
				{
					foreach (var candidate in _candidates)
						if (candidate.Guard < 0 && (sets
							? candidate.Set != null && Holds(expected, expectedMore, candidate.Set)
							: candidate.Set == null))
							paths.Add(PathOf(read, candidate.Frame, candidate.Set, -1));

					if (paths.Count > 0)
						cause = sets
							? "The rules below were reading there, each wanting what it names."
							: "The rules below were reading there and refused without naming what would fit.";

					var guarded = 0;

					foreach (var candidate in _candidates)
						if (candidate.Guard >= 0)
						{
							paths.Add(PathOf(read, candidate.Frame, null, candidate.Guard));
							guarded++;
						}

					if (cause.Length == 0 && guarded > 0)
						cause = "A guard refused there.";
				}

				if (paths.Count == 0 && !sets && _frontier != null && _frontierAt >= position)
				{
					paths.Add(PathOf(read, _frontier, null, -1));
					cause = "The input ended inside the rules below.";
				}
				else if (paths.Count == 0)
					cause = "No rule reported the refusal it ended with.";

				// The tokens before a character none begins with, read for this: the rules reading
				// where they ran out, where they got that far.
				if (_unlexed >= 0)
				{
					if (paths.Count == 0 || read.CharacterOf(position) != _unlexed)
					{
						_candidates.Clear();
						_sinceCandidates.Clear();
						_top      = null;
						_frontier = null;

						return;
					}

					cause = "No token of the grammar begins at that character. The rules below were reading up to it" +
						(sets ? ", each wanting what it names." : ".");
				}

				_pendingRead  = read;
				_pendingPaths = Distinct(paths);
				_pendingCause = cause;

				_candidates.Clear();
				_sinceCandidates.Clear();
				_top      = null;
				_frontier = null;
			}

			/// <inheritdoc/>
			public override void Unlexed(string text, int position)
			{
				if (_depth != 0)
					return;

				_pendingRead  = new GramRead(this, "", false, false, 0, "methods", text, null, 0, 0, 0);
				_pendingPaths = new Path[0];
				_pendingCause = "No token of the grammar begins at that character, so no rule was read.";
				_unlexed      = position;
				_elements.Clear();
				Elements      = new Element[0];
			}

			/// <inheritdoc/>
			public override void Rejected(long position, string message)
			{
				if (_depth != 0)
					return;

				_unlexed  = -1;
				IsRefused = true;
				Message   = message;
				Position  = position;
				Line      = 0;
				Column    = 0;
				Paths     = _pendingPaths ?? new Path[0];
				Cause     = _pendingPaths != null
					? _pendingCause
					: "The call refused before reading anything.";

				var read = _pendingRead;

				if (read != null && read.Text != null && position >= 0 && position <= read.Text.Length)
				{
					int line;
					int column;

					read.Locate((int)position, out line, out column);

					Line   = line;
					Column = column;
				}

				_pendingRead  = null;
				_pendingPaths = null;
				_pendingCause = "";
			}

			/// <summary>The message, where it is, why, and the rules that were reading there.</summary>
			public override string ToString()
			{
				var text = new global::System.Text.StringBuilder();

				if (!IsRefused)
				{
					text.Append("Nothing was refused.");
				}
				else
				{
					text.Append(Message);

					if (Line > 0)
						text.Append(" (line ").Append(Line.ToString(global::System.Globalization.CultureInfo.InvariantCulture))
							.Append(", column ").Append(Column.ToString(global::System.Globalization.CultureInfo.InvariantCulture)).Append(')');

					text.Append(global::System.Environment.NewLine).Append(Cause);

					foreach (var path in Paths)
						text.Append(global::System.Environment.NewLine).Append("  ").Append(path.ToString());
				}

				foreach (var element in Elements)
				{
					text.Append(global::System.Environment.NewLine)
						.Append("Stepped over an element of ").Append(element.Rule)
						.Append(" from ").Append(element.From.ToString(global::System.Globalization.CultureInfo.InvariantCulture))
						.Append(" to ").Append(element.To.ToString(global::System.Globalization.CultureInfo.InvariantCulture)).Append(':');

					foreach (var path in element.Paths)
						text.Append(global::System.Environment.NewLine).Append("  ").Append(path.ToString());
				}

				return text.ToString();
			}

			static bool Holds(string[]? expected, global::System.Collections.Generic.IReadOnlyList<string[]>? more, string[] set)
			{
				if (ReferenceEquals(expected, set))
					return true;

				if (more != null)
					for (var i = 0; i < more.Count; i++)
						if (ReferenceEquals(more[i], set))
							return true;

				return false;
			}

			static Path PathOf(GramRead read, Frame_DotGram? frame, string[]? set, int guard)
			{
				var count   = frame == null ? 0 : frame.Depth;
				var rules   = new string[count];
				var at      = new int[count];
				var lines   = new int[count];
				var columns = new int[count];

				for (var i = count - 1; frame != null; i--, frame = frame.Parent)
				{
					var character = read.CharacterOf(frame.At);
					int line;
					int column;

					read.Locate(character, out line, out column);

					rules[i]   = read.RuleName(frame.Rule);
					at[i]      = character;
					lines[i]   = line;
					columns[i] = column;
				}

				return new Path(rules, at, lines, columns, set, guard >= 0 ? read.GuardText(guard) : null);
			}

			static Path[] Distinct(global::System.Collections.Generic.List<Path> paths)
			{
				var kept = new global::System.Collections.Generic.List<Path>(paths.Count);
				var seen = new global::System.Collections.Generic.HashSet<string>(global::System.StringComparer.Ordinal);

				foreach (var path in paths)
					if (seen.Add(path.ToString()))
						kept.Add(path);

				return kept.ToArray();
			}

			static string Wanted_DotGram(global::System.Collections.Generic.IReadOnlyList<string> expected)
			{
				var text = new global::System.Text.StringBuilder();

				for (var i = 0; i < expected.Count; i++)
				{
					if (i > 0)
						text.Append(i == expected.Count - 1 ? " or " : ", ");

					var one = expected[i];

					text.Append(one.Length > 0 && one[0] == '\u0000' ? one.Substring(1) : one);
				}

				return text.ToString();
			}
		}

		/// <summary>
		/// What a reading did, one line a thing it did, indented by how deep it was: each rule
		/// entered and left, each refusal and what it wanted, each guard asked.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Written as it happens, up to a budget of lines and then counted. The starts a
		/// <c>find</c> tries and refuses are summed up in one line rather than written each.
		/// </para>
		/// <para>
		/// The writer is the caller's, and what it throws — a writer disposed before the scope
		/// is — goes out of the reading as anything a sink throws does: give it one that lives
		/// as long as the scope.
		/// </para>
		/// </remarks>
		public sealed class GramTraceLog : GramTrace
		{
			readonly global::System.IO.TextWriter _writer;
			readonly global::System.Collections.Generic.List<GramRead> _reads =
				new global::System.Collections.Generic.List<GramRead>();
			readonly global::System.Collections.Generic.List<int> _bases =
				new global::System.Collections.Generic.List<int>();
			global::System.Collections.Generic.List<string>? _held;
			int _depth;
			long _written;
			long _dropped;
			long _noted;
			int _refusedStarts;
			int _firstRefused;
			int _lastRefused;

			/// <summary>A log written to <paramref name="writer"/>, of at most <paramref name="budget"/> lines.</summary>
			public GramTraceLog(global::System.IO.TextWriter writer, int budget = 10000)
			{
				_writer = writer ?? throw new global::System.ArgumentNullException(nameof(writer));
				Budget  = budget;
			}

			/// <summary>How many lines are written before the rest is only counted.</summary>
			public int Budget { get; }

			/// <summary>How many lines were not written for the budget.</summary>
			public long Dropped
			{
				get { return _dropped; }
			}

			/// <inheritdoc/>
			public override void Begin(GramRead read)
			{
				// One start of a find is held back until it is known to have found something, and the
				// starts it refused before then are one line.
				if (_reads.Count == 0 && read.Finding)
					_held = new global::System.Collections.Generic.List<string>();
				else if (_reads.Count == 0)
					Summed();

				Write((read.Quiet ? "quiet" : "recording") + " reading of " + read.Publication + " at " + Where(read, read.Start) +
					(read.Machine == "methods" ? "" : " (on the " + read.Machine + ")"));

				_reads.Add(read);
				_bases.Add(_depth);
			}

			/// <inheritdoc/>
			public override void End(
				GramRead read, int end, int position, string[]? expected,
				global::System.Collections.Generic.IReadOnlyList<string[]>? expectedMore)
			{
				var last = _reads.Count - 1;

				if (last >= 0)
				{
					_depth = _bases[last];
					_reads.RemoveAt(last);
					_bases.RemoveAt(last);
				}

				if (read != null)
					Write(end >= 0
						? "read to " + Where(read, end)
						: "refused, the furthest refusal at " + Where(read, position) +
							(expected != null ? ": " + Wanted(expected) : ""));

				if (_reads.Count > 0)
					return;

				var held = _held;

				_held = null;

				if (held != null && end < 0 && read != null)
				{
					if (_refusedStarts++ == 0)
						_firstRefused = read.Start;

					_lastRefused = read.Start;
				}
				else if (held != null)
				{
					Summed();

					foreach (var line in held)
						Write(line);
				}

				Noted();
			}

			/// <inheritdoc/>
			public override void Enter(int rule, int position)
			{
				var read = Current;

				Write(read == null
					? "#" + rule.ToString(global::System.Globalization.CultureInfo.InvariantCulture)
					: read.RuleName(rule) + " " + Where(read, position) + Window(read, position));
				_depth++;
			}

			/// <inheritdoc/>
			public override void Exit(int rule, int position, int end)
			{
				if (_depth > (_bases.Count > 0 ? _bases[_bases.Count - 1] : 0))
					_depth--;

				var read = Current;

				if (read != null)
					Write(read.RuleName(rule) + " " + (end >= 0
						? "read to " + Where(read, end)
						: end == Thrown ? "left by an exception" : "failed"));
			}

			/// <inheritdoc/>
			public override void Retracted(int rule, int position)
			{
				var read = Current;

				if (read != null)
					Write(read.RuleName(rule) + " " + Where(read, position) + " taken back");
			}

			/// <inheritdoc/>
			public override void Remembered(int rule, int position)
			{
				var read = Current;

				if (read != null)
					Write(read.RuleName(rule) + " " + Where(read, position) + " failed here before, and fails again unread");
			}

			/// <inheritdoc/>
			public override void Guard(int guard, int position, bool passed)
			{
				var read = Current;

				if (read != null)
					Write("when " + read.GuardText(guard) + " at " + Where(read, position) + ": " + (passed ? "yes" : "no"));
			}

			/// <inheritdoc/>
			public override void Refused(int position, string[]? expected)
			{
				var read = Current;

				if (read != null)
					Write("refused at " + Where(read, position) + (expected != null ? ": " + Wanted(expected) : ""));
			}

			/// <inheritdoc/>
			public override void Recovered(int rule, int from, int to, int reach)
			{
				var read = Current;

				if (read != null)
					Write(read.RuleName(rule) + " stepped over an element from " + Where(read, from) + " to " +
						Where(read, to) + ", which got to " + Where(read, reach));
			}

			/// <inheritdoc/>
			public override void Deepened(int position)
			{
				var read = Current;

				if (read != null)
					Write("the stack runs low at " + Where(read, position) + ": the reading goes on on a stack of its own");
			}

			/// <inheritdoc/>
			public override void Unlexed(string text, int position)
			{
				Summed();
				Write("no token begins at character " + position.ToString(global::System.Globalization.CultureInfo.InvariantCulture));
			}

			/// <inheritdoc/>
			public override void Rejected(long position, string message)
			{
				if (_reads.Count > 0)
					return;

				Summed();
				Write("refused at " + position.ToString(global::System.Globalization.CultureInfo.InvariantCulture) + ": " + message);
				Noted();
			}

			GramRead? Current
			{
				get { return _reads.Count == 0 ? null : _reads[_reads.Count - 1]; }
			}

			void Write(string line)
			{
				var indent = _depth <= 40
					? new string(' ', _depth * 2)
					: new string(' ', 80) + "[" + _depth.ToString(global::System.Globalization.CultureInfo.InvariantCulture) + "] ";

				var held = _held;

				if (held != null)
				{
					if (held.Count < Budget)
						held.Add(indent + line);

					return;
				}

				if (_written >= Budget)
				{
					_dropped++;

					return;
				}

				_written++;
				_writer.WriteLine(indent + line);
			}

			/// <summary>The starts a find refused since the last thing it found, as one line.</summary>
			void Summed()
			{
				if (_refusedStarts == 0)
					return;

				var count = _refusedStarts;

				_refusedStarts = 0;

				Write(count.ToString(global::System.Globalization.CultureInfo.InvariantCulture) + " starts refused from " +
					_firstRefused.ToString(global::System.Globalization.CultureInfo.InvariantCulture) + " to " +
					_lastRefused.ToString(global::System.Globalization.CultureInfo.InvariantCulture));
			}

			/// <summary>What the budget kept out, said once a call ends.</summary>
			void Noted()
			{
				if (_dropped == _noted || _written < Budget)
					return;

				_writer.WriteLine("... " + (_dropped - _noted).ToString(global::System.Globalization.CultureInfo.InvariantCulture) +
					" more lines, past the budget of " + Budget.ToString(global::System.Globalization.CultureInfo.InvariantCulture));
				_noted = _dropped;
			}

			static string Where(GramRead read, int position)
			{
				if (position < 0)
					return "-";

				var character = read.CharacterOf(position);
				int line;
				int column;

				read.Locate(character, out line, out column);

				return line.ToString(global::System.Globalization.CultureInfo.InvariantCulture) + ":" +
					column.ToString(global::System.Globalization.CultureInfo.InvariantCulture);
			}

			static string Window(GramRead read, int position)
			{
				var text = read.Text;

				if (text == null)
					return "";

				var from = read.CharacterOf(position);

				if (from < 0 || from > text.Length)
					return "";

				var length = text.Length - from < 24 ? text.Length - from : 24;
				var shown  = new global::System.Text.StringBuilder(length + 8);

				shown.Append(" \"");

				for (var i = 0; i < length; i++)
				{
					var one = text[from + i];

					shown.Append(one == '\n' ? "\\n" : one == '\r' ? "\\r" : one == '\t' ? "\\t" : one.ToString());
				}

				shown.Append(length < text.Length - from ? "...\"" : "\"");

				return shown.ToString();
			}

			static string Wanted(string[] expected)
			{
				var text = new global::System.Text.StringBuilder();

				for (var i = 0; i < expected.Length; i++)
				{
					if (i > 0)
						text.Append(", ");

					var one = expected[i];

					text.Append(one.Length > 0 && one[0] == '\u0000' ? one.Substring(1) : one);
				}

				return text.ToString();
			}
		}

		/// <summary>
		/// How often each rule was entered, how often it read and failed, how often it was asked
		/// again where it had been before, and how long it took: in quiet readings and in
		/// recording ones apart, which are different programs.
		/// </summary>
		/// <remarks>
		/// Timed in the trace build, whose every rule reports its return: the numbers are the
		/// proportions of the parser, a little slower than it.
		/// </remarks>
		public sealed class GramProfile : GramTrace
		{
			// By the rule's number, which the tables of the grammar number densely from nought.
			Row?[] _rows = new Row?[TraceRules_DotGram.Length];
			int[] _active = new int[TraceRules_DotGram.Length];
			readonly global::System.Collections.Generic.List<Row> _entered =
				new global::System.Collections.Generic.List<Row>();
			readonly global::System.Collections.Generic.List<Open_DotGram> _open =
				new global::System.Collections.Generic.List<Open_DotGram>();
			readonly global::System.Collections.Generic.List<GramRead> _reads =
				new global::System.Collections.Generic.List<GramRead>();
			readonly global::System.Collections.Generic.List<int> _bases =
				new global::System.Collections.Generic.List<int>();
			readonly global::System.Collections.Generic.List<global::System.Collections.Generic.HashSet<long>> _seen =
				new global::System.Collections.Generic.List<global::System.Collections.Generic.HashSet<long>>();

			/// <summary>Every rule entered, with its counts, in the order they were first entered.</summary>
			public global::System.Collections.Generic.IReadOnlyCollection<Row> Rows
			{
				get { return _entered; }
			}

			/// <summary>How many readings there were, quiet and recording.</summary>
			public long Readings { get; private set; }

			/// <summary>False: the profile reads no message, and spares a refused call the wording of one.</summary>
			public override bool HearsRejections
			{
				get { return false; }
			}

			/// <summary>One rule's counts.</summary>
			public sealed class Row
			{
				internal Row(string rule)
				{
					Rule      = rule;
					Quiet     = new Counts();
					Recording = new Counts();
				}

				/// <summary>The rule.</summary>
				public string Rule { get; }

				/// <summary>In quiet readings: a <c>TryParse</c>'s first, a yes-or-no form's, a find's.</summary>
				public Counts Quiet { get; }

				/// <summary>In recording readings: what a refusal's message is made of.</summary>
				public Counts Recording { get; }
			}

			/// <summary>What a rule did in one kind of reading.</summary>
			public sealed class Counts
			{
				/// <summary>How many times it was entered.</summary>
				public long Entries { get; internal set; }

				/// <summary>How many of those read something and returned.</summary>
				public long Successes { get; internal set; }

				/// <summary>How many failed.</summary>
				public long Failures { get; internal set; }

				/// <summary>How many entries were answered from the memory of a failure there.</summary>
				public long Remembered { get; internal set; }

				/// <summary>
				/// How many of its successes the engine took back, going back to before the rule
				/// ended or inside it.
				/// </summary>
				public long Retracted { get; internal set; }

				/// <summary>How many entries were at a position the same reading had entered it at before.</summary>
				public long Rereads { get; internal set; }

				/// <summary>The time inside it and everything it called, counted once where it called itself.</summary>
				public double InclusiveMilliseconds { get; internal set; }

				/// <summary>The time inside it and not in a rule it called.</summary>
				public double ExclusiveMilliseconds { get; internal set; }

				/// <summary>The deepest stack of rules it was entered on.</summary>
				public int MaxDepth { get; internal set; }
			}

			struct Open_DotGram
			{
				internal int Rule;
				internal int At;
				internal long Started;
				internal long Children;
				internal bool Quiet;
			}

			/// <inheritdoc/>
			public override void Begin(GramRead read)
			{
				Readings++;
				_reads.Add(read);
				_bases.Add(_open.Count);
				_seen.Add(new global::System.Collections.Generic.HashSet<long>());
			}

			/// <inheritdoc/>
			public override void End(
				GramRead read, int end, int position, string[]? expected,
				global::System.Collections.Generic.IReadOnlyList<string[]>? expectedMore)
			{
				var last = _reads.Count - 1;

				if (last < 0)
					return;

				// What a reading left open is closed with it, timed to here and counted as nothing.
				while (_open.Count > _bases[last])
					Close(-1, false);

				_reads.RemoveAt(last);
				_bases.RemoveAt(last);
				_seen.RemoveAt(last);
			}

			/// <inheritdoc/>
			public override void Enter(int rule, int position)
			{
				if (rule < 0)
					return;

				var quiet  = Quiet;
				var counts = CountsOf(rule, quiet);

				counts.Entries++;

				if (_seen.Count > 0 && !_seen[_seen.Count - 1].Add(((long)rule << 32) | (uint)position))
					counts.Rereads++;

				var depth = _open.Count - (_bases.Count > 0 ? _bases[_bases.Count - 1] : 0) + 1;

				if (depth > counts.MaxDepth)
					counts.MaxDepth = depth;

				Room(rule);
				_active[rule]++;

				var open = new Open_DotGram();

				open.Rule    = rule;
				open.At      = position;
				open.Quiet   = quiet;
				open.Started = global::System.Diagnostics.Stopwatch.GetTimestamp();

				_open.Add(open);
			}

			/// <inheritdoc/>
			public override void Exit(int rule, int position, int end)
			{
				var floor = _bases.Count > 0 ? _bases[_bases.Count - 1] : 0;

				for (var at = _open.Count - 1; at >= floor; at--)
					if (_open[at].Rule == rule && _open[at].At == position)
					{
						while (_open.Count - 1 > at)
							Close(-1, false);

						// A rule an exception left neither read nor failed: timed, and not counted.
						Close(end, end != Thrown);

						return;
					}
			}

			/// <inheritdoc/>
			public override void Remembered(int rule, int position)
			{
				if (rule >= 0)
					CountsOf(rule, Quiet).Remembered++;
			}

			/// <inheritdoc/>
			public override void Retracted(int rule, int position)
			{
				if (rule >= 0)
					CountsOf(rule, Quiet).Retracted++;
			}

			bool Quiet
			{
				get { return _reads.Count > 0 && _reads[_reads.Count - 1].Quiet; }
			}

			void Close(int end, bool counted)
			{
				var last = _open.Count - 1;
				var open = _open[last];
				var now  = global::System.Diagnostics.Stopwatch.GetTimestamp();
				var took = now - open.Started;

				_open.RemoveAt(last);

				var counts = CountsOf(open.Rule, open.Quiet);

				if (counted)
				{
					if (end >= 0)
						counts.Successes++;
					else
						counts.Failures++;
				}

				counts.ExclusiveMilliseconds += Milliseconds(took - open.Children);

				if (--_active[open.Rule] == 0)
					counts.InclusiveMilliseconds += Milliseconds(took);

				if (last > 0)
				{
					var parent = _open[last - 1];

					parent.Children += took;
					_open[last - 1]  = parent;
				}
			}

			Counts CountsOf(int rule, bool quiet)
			{
				Room(rule);

				var row = _rows[rule];

				if (row == null)
				{
					var read = _reads.Count > 0 ? _reads[_reads.Count - 1] : null;

					row = new Row(read != null
						? read.RuleName(rule)
						: (uint)rule < (uint)TraceRules_DotGram.Length ? TraceRules_DotGram[rule] : "#" + rule.ToString(global::System.Globalization.CultureInfo.InvariantCulture));
					_rows[rule] = row;
					_entered.Add(row);
				}

				return quiet ? row.Quiet : row.Recording;
			}

			/// <summary>Room for a rule's number past the tables, which only a sink shared by mistake can hand in.</summary>
			void Room(int rule)
			{
				if (rule < _rows.Length)
					return;

				global::System.Array.Resize(ref _rows, rule + 1);
				global::System.Array.Resize(ref _active, rule + 1);
			}

			static double Milliseconds(long ticks)
			{
				return ticks * 1000.0 / global::System.Diagnostics.Stopwatch.Frequency;
			}

			/// <summary>Writes the counts as two tables, recording readings and quiet ones, the costliest rule first.</summary>
			public void WriteTo(global::System.IO.TextWriter writer)
			{
				Table(writer, "recording readings", false);
				Table(writer, "quiet readings", true);
			}

			/// <summary>The counts as <see cref="WriteTo"/> writes them.</summary>
			public override string ToString()
			{
				var writer = new global::System.IO.StringWriter(global::System.Globalization.CultureInfo.InvariantCulture);

				WriteTo(writer);

				return writer.ToString();
			}

			void Table(global::System.IO.TextWriter writer, string title, bool quiet)
			{
				var rows = new global::System.Collections.Generic.List<Row>();

				foreach (var row in _entered)
					if ((quiet ? row.Quiet : row.Recording).Entries > 0 || (quiet ? row.Quiet : row.Recording).Remembered > 0)
						rows.Add(row);

				rows.Sort((a, b) =>
				{
					var x = quiet ? a.Quiet : a.Recording;
					var y = quiet ? b.Quiet : b.Recording;
					var order = y.InclusiveMilliseconds.CompareTo(x.InclusiveMilliseconds);

					return order != 0 ? order : string.CompareOrdinal(a.Rule, b.Rule);
				});

				writer.WriteLine(title + ":");
				writer.WriteLine("rule\tentries\tread\tfailed\ttaken back\tremembered\treread\tinclusive ms\texclusive ms\tmax depth");

				foreach (var row in rows)
				{
					var counts = quiet ? row.Quiet : row.Recording;

					writer.WriteLine(string.Join("\t", new[]
					{
						row.Rule,
						counts.Entries.ToString(global::System.Globalization.CultureInfo.InvariantCulture),
						counts.Successes.ToString(global::System.Globalization.CultureInfo.InvariantCulture),
						counts.Failures.ToString(global::System.Globalization.CultureInfo.InvariantCulture),
						counts.Retracted.ToString(global::System.Globalization.CultureInfo.InvariantCulture),
						counts.Remembered.ToString(global::System.Globalization.CultureInfo.InvariantCulture),
						counts.Rereads.ToString(global::System.Globalization.CultureInfo.InvariantCulture),
						counts.InclusiveMilliseconds.ToString("F3", global::System.Globalization.CultureInfo.InvariantCulture),
						counts.ExclusiveMilliseconds.ToString("F3", global::System.Globalization.CultureInfo.InvariantCulture),
						counts.MaxDepth.ToString(global::System.Globalization.CultureInfo.InvariantCulture),
					}));
				}

				writer.WriteLine();
			}
		}

		""";
}
