using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Emit;

/// <summary>
/// Reading through a window: the overloads of §6.3, and what decides a grammar gets them.
/// </summary>
/// <remarks>
/// <para>
/// The parse over a string is one machine over one span. This is the other half — the
/// same grammar read a part at a time, with everything that follows from the input not
/// being all there: a result that ran into the end of the window is not an answer yet,
/// a record is handed over as it is read and cannot be taken back, and the parts are run
/// in order rather than compiled into one machine.
/// </para>
/// <para>
/// Kept together because they answer one question between them — what may be streamed,
/// and how — and because the two paths must agree about a grammar or the same input
/// reads two ways. The tests that hold them to that give one feed both doors and compare.
/// </para>
/// </remarks>
public static partial class CSharpEmitter
{
	/// <summary>
	/// <c>find</c> over a reader: the same occurrences, out of input that is never all
	/// there at once.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The same loop as the one over a string, with one thing added: a result that ran
	/// into the end of the window is not an answer yet. The input could have carried it
	/// further — <c>['0'..'9']+</c> stopped at a buffer boundary is two occurrences where
	/// there is one — and a failure that reached the end could have matched. So anything
	/// that touches the end is provisional while the reader has more, and the window is
	/// read into and the question asked again from the same place.
	/// </para>
	/// <para>
	/// That is also what keeps <c>eof</c> honest: the end of a full buffer looks exactly
	/// like the end of the input to a recognizer, and the only thing that can tell them
	/// apart is whether the reader is finished.
	/// </para>
	/// </remarks>
	/// <param name="tracing">
	/// Where the build traces, what it numbers events by: each start is then a reading told to the
	/// sink set where the call is made, its positions counted from the beginning of the input.
	/// </param>
	/// <param name="machine">What reads the rule: <c>methods</c> or <c>engine</c>.</param>
	/// <param name="built">The type of the value the rule builds, where it builds one.</param>
	static void EmitStreamingFind(
		Writer file, ResultTypes results, Publication publication, string method, string name,
		string match, string hands, bool builds, string takes,
		TraceTables? tracing = null, string machine = "engine", string? built = null)
	{
		var recognized = builds ? "recognized" : "window.Text(start, end - start)";
		var returns    = $"global::System.Collections.Generic.IEnumerable<{match}>";

		file.Line($"/// <summary>Every occurrence of <c>{name}</c> in a reader, read as it is asked for.</summary>");
		file.Line("/// <remarks>");
		file.Line("/// The input is read through a buffer that is reused, so what is held is what the");
		file.Line($"/// occurrence being read needs and not the input (docs/syntax.md §6.3).");
		file.Line("/// </remarks>");

		// Where the build traces, the sink set where the call is made is handed to the iterator,
		// whose body runs at the first MoveNext.
		if (tracing is not null)
		{
			using (file.Block($"{AccessOf(publication)} static {returns} {method}(global::System.IO.TextReader input{takes})"))
				file.Line($"return {method}_DotGram(input{(takes.Length > 0 ? ", context" : "")}, Tracing_DotGram.Value);");

			file.Line();
		}

		using (file.Block(tracing is null
			? $"{AccessOf(publication)} static {returns} {method}(global::System.IO.TextReader input{takes})"
			: $"static {returns} {method}_DotGram(global::System.IO.TextReader input{takes}, GramTrace? trace)"))
		{
			file.Line($"var window = new {WindowType}(input, {WindowSize});");
			file.Line("var start  = 0;");
			file.Line();

			using (file.Block("while (true)"))
			{
				file.Line($"var failure = new {FailureType}();");
				file.Line();

				if (tracing is null)
					file.Line($"var end = {MethodOf(publication.Rule)}(window.Span(), start{hands});");
				else
				{
					file.Line(TraceBegin("read", name, "start", machine, "null", null, finding: true, sink: "trace", offset: "(int)window.Offset"));
					TracedRead(file, $"{MethodOf(publication.Rule)}(window.Span(), start{hands})", built, declare: true, sink: "trace");
				}

				file.Line();

				// Reaching the end of what is held is not the same as reaching the end of the
				// input, and only the reader knows which one it was.
				using (file.Block(
					"if (((end < 0 ? failure.Position : end) >= window.Length || failure.Starved) && " +
					"!window.Ended)"))
				{
					file.Line("window.Extend(ref start);");
					file.Line("continue;");
				}

				file.Line();

				using (file.Block("if (end < 0)"))
				{
					file.Line("if (start >= window.Length)");
					file.Then("yield break;");
					file.Line();
					file.Line("start++;");
					file.Line("continue;");
				}

				file.Line();
				file.Line(
					$"yield return {match}.Success({recognized}, window.Offset + start, end - start);");
				file.Line();
				file.Line("// A rule that matches nothing would otherwise find it for ever.");
				file.Line("start = end > start ? end : start + 1;");
				file.Line();
				file.Line("if (start > window.Length)");
				file.Then("yield break;");
			}
		}

		file.Line();

		EmitOverLines(
			file, AccessOf(publication), $"global::System.Collections.Generic.IEnumerable<{match}>", method,
			results.MemberOf(method), name, takes);
	}

	/// <summary>
	/// The same method again, taking lines rather than a reader (§6.3).
	/// </summary>
	/// <remarks>
	/// One line, because a sequence of lines is a reader once the terminators are put
	/// back. Written out rather than left to the caller so that <c>File.ReadLines</c> and
	/// a <c>List&lt;string&gt;</c> are as ordinary an input as a string is.
	/// </remarks>
	static void EmitOverLines(
		Writer file, string access, string returns, string method, string called, string name, string takes = "")
	{
		file.Line($"/// <summary>The same, over a sequence of lines (docs/syntax.md §6.3).</summary>");
		file.Line("/// <remarks>");
		file.Line("/// The lines are read as though they were a file, with a newline put back on the");
		file.Line($"/// end of each: a sequence of lines has had its terminators taken off, and <c>{name}</c>");
		file.Line("/// may well be looking for one.");
		file.Line("/// </remarks>");

		using (file.Block(
			$"{access} static {returns} {method}(" +
			$"global::System.Collections.Generic.IEnumerable<string> input{takes})"))
		{
			// Through the class: this overload's own parameters are called `input` and `context`, and a
			// publication may be named either.
			var call = $"{called}(lines_DotGram{(takes.Length > 0 ? ", context" : "")})";

			if (returns.StartsWith("global::System.Collections.Generic.IEnumerable<", StringComparison.Ordinal))
			{
				// Lazy: the enumerator is taken at the first MoveNext and let go when the
				// caller's loop ends, breaks or throws. The null check stays eager.
				file.Line("if (input is null)");
				file.Then("throw new global::System.ArgumentNullException(nameof(input));");
				file.Line();
				file.Line($"return OverLines_DotGram(input{(takes.Length > 0 ? ", context" : "")});");
				file.Line();

				var element = returns.Substring(returns.IndexOf('<') + 1, returns.Length - returns.IndexOf('<') - 2);

				using (file.Block(
					$"static global::System.Collections.Generic.IEnumerable<{element}> OverLines_DotGram(" +
					$"global::System.Collections.Generic.IEnumerable<string> input{takes})"))
				{
					file.Line($"using var lines_DotGram = new {LinesType}(input);");
					file.Line();
					file.Line($"foreach (var item_DotGram in {call})");
					file.Then("yield return item_DotGram;");
				}
			}
			else
			{
				file.Line($"using var lines_DotGram = new {LinesType}(input);");
				file.Line();
				file.Line($"return {call};");
			}
		}
	}

	/// <summary>
	/// Whether this publication gets a reader overload (§6.3).
	/// </summary>
	/// <remarks>
	/// <c>find</c> only, for now. It is the directive the analysis has least to prove
	/// about: an occurrence is looked for and then handed over, so the window may move at
	/// every one of them, and all that is left to ask is whether one occurrence fits in a
	/// window at all. <c>parse</c> needs the decomposition <see cref="Retention.PlanFor"/>
	/// does, because what lets it move is a committed repetition in the middle of it.
	/// </remarks>
	/// <summary>
	/// <c>parse</c> over a reader: the rule's parts, handed over as they are read.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The stages are run in order rather than compiled into one machine, and that is the
	/// whole difference from the parse over a string. A machine may backtrack anywhere
	/// inside the rule; a stream may not go back past what it has handed over. What makes
	/// the two agree is the mark: at every boundary of the marked repetition the complete
	/// continuation gets first refusal, and an element actually accepted or recovered is then
	/// committed. Any other repetition is read for as long as its element matches, which is
	/// what the string reads where the analysis let it stream, and the other stages once each.
	/// </para>
	/// <para>
	/// Each stage reads through the window with the same provisional rule <c>find</c> uses:
	/// a result that ran into the end of what is held is not an answer while the reader has
	/// more.
	/// </para>
	/// </remarks>
	/// <param name="tracing">
	/// Where the build traces, what it numbers events by: each part read is then a reading told to
	/// the sink set where the call is made, its positions counted from the beginning of the input.
	/// What is only asked — whether the trivia, the rest of the rule or the synchronization stands
	/// next — is not.
	/// </param>
	static void EmitStreamingParse(
		Writer file, RecognitionGraph graph, Publication publication, ResultTypes results,
		IReadOnlyList<Stage> stages, IReadOnlyList<string> parts, string? trivia,
		Recovery? recovery, string? sync, string factory, Func<int, string?> continuation,
		bool input, Func<RuleSymbol, string> recognize, TraceTables? tracing = null)
	{
		var element = graph.Types[publication.Rule];

		element = element.Substring(0, element.Length - "[]".Length);

		var returns = $"global::System.Collections.Generic.IEnumerable<{element}>";

		file.Line($"/// <summary>Reads <c>{publication.Rule.Name}</c> from a reader, a part at a time.</summary>");
		file.Line("/// <remarks>");
		file.Line("/// The input is read through a buffer that is reused, so what is held is the part");
		file.Line("/// being read and not the input. Each element is handed over as it is read, which");
		file.Line("/// is why the repetition has to be marked 'recover' (docs/syntax.md §6.3, §8.2).");
		file.Line("/// </remarks>");

		// Where the build traces, the sink set where the call is made is handed to the iterator,
		// whose body runs at the first MoveNext.
		if (tracing is not null)
		{
			using (file.Block($"{AccessOf(publication)} static {returns} {publication.MethodName}(global::System.IO.TextReader input)"))
				file.Line($"return {publication.MethodName}_DotGram(input, Tracing_DotGram.Value);");

			file.Line();
		}

		using (file.Block(tracing is null
			? $"{AccessOf(publication)} static {returns} {publication.MethodName}(global::System.IO.TextReader input)"
			: $"static {returns} {publication.MethodName}_DotGram(global::System.IO.TextReader input, GramTrace? trace)"))
		{
			file.Line($"var window = new {WindowType}(input, {WindowSize});");
			file.Line("var start  = 0;");

			// The furthest any reading got, which is where a refusal is reported, as over a
			// string: a part that fell short of it is not what went wrong.
			file.Line("var furthest = 0L;");

			// The trivia a whole parse allows before the rule.
			if (trivia is not null)
				Skip(trivia, "Lead");

			for (var i = 0; i < stages.Count; i++)
			{
				var stage  = stages[i];
				var method = stage.Rule is { } rule
					? recognize(rule)
					: parts[i];
				var built  = stage.Rule is { } called ? results.QualifiedOf(called) : null;

				// Only the repetition that was marked steps over a bad element. Another one in
				// the same rule is an ordinary repetition, read for as long as its element
				// matches, as it is over a string.
				var marked = stage.Repeated && recovery is not null && graph.Recoveries.ContainsKey(stage.Node);

				// How many elements it may have, held to as over a string: fewer than its least
				// is a refusal, and its most ends it. The marked one counts what it stepped over
				// too, which its ordinal already does.
				var (least, most) = stage.Node is Node.Repeat(_, var min, var max) ? (min, max) : (0, (int?)null);
				var counted       = stage.Repeated && !marked && (least > 0 || most is not null);
				var count         = marked ? $"ordinal{i}" : $"count{i}";

				file.Line();
				file.Line("// " + Comment(stage));

				// Outside the loop, because it counts the elements of the whole repetition
				// and a rejected one holds its place in the numbering (§8.2).
				if (marked)
					file.Line($"var ordinal{i} = 0;");

				if (counted)
					file.Line($"var count{i} = 0;");

				var loop = stage.Repeated ? file.Block("while (true)") : null;

				if ((counted || marked) && most is { } limit)
				{
					file.Line($"if ({count} >= {limit})");
					file.Then("break;");
					file.Line();
				}

				// The seam of a spaced collection, skipped before anything else is asked
				// at this turn — the continuation probe and the element both begin past
				// it — growing the window while the trivia may continue beyond it.
				if (stage.Repeated && stage.Seam is { } seamRule)
				{
					file.Line($"var seamFailure{i} = new {FailureType}();");
					file.Line($"int seamEnd{i};");
					file.Line();

					using (file.Block("while (true)"))
					{
						file.Line($"seamFailure{i} = new {FailureType}();");
						file.Line(
							$"seamEnd{i}     = {recognize(seamRule)}" +
							$"(window.Span(), start, ref seamFailure{i});");
						file.Line();

						using (file.Block(
							$"if (((seamEnd{i} < 0 ? seamFailure{i}.Position : seamEnd{i}) >= window.Length || " +
							$"seamFailure{i}.Starved) && !window.Ended)"))
						{
							file.Line("window.Extend(ref start);");
							file.Line("continue;");
						}

						file.Line();
						file.Line("break;");
					}

					file.Line();
					Furthest($"seamFailure{i}");
					file.Line($"if (seamEnd{i} >= 0)");
					file.Then($"start = seamEnd{i};");
					file.Line();
				}

				if (stage.Repeated && continuation(i) is { } probe)
				{
					// Not before it has its least: until then an element is owed, and what does not
					// read as one is a bad one, which is how a string reads it too.
					using (marked && least > 0 ? file.Block($"if ({count} >= {least})") : null)
					{
						file.Line($"var continuationFailure{i} = new {FailureType}();");
						file.Line($"var continuationEnd{i} = -1;");
						file.Line();

						using (file.Block("while (true)"))
						{
							file.Line($"continuationFailure{i} = new {FailureType}();");
							file.Line($"continuationEnd{i} = {probe}(window.Span(), start, ref continuationFailure{i});");
							file.Line();

							using (file.Block(
								$"if (((continuationEnd{i} < 0 ? continuationFailure{i}.Position : continuationEnd{i}) " +
								$">= window.Length || continuationFailure{i}.Starved) && !window.Ended)"))
							{
								file.Line("window.Extend(ref start);");
								file.Line("continue;");
							}

							file.Line();
							file.Line("break;");
						}

						file.Line();
						Furthest($"continuationFailure{i}");
						file.Line($"if (continuationEnd{i} >= 0)");
						file.Then("break;");
					}

					file.Line();
				}

				Read(i, stage, method, built, marked, counted);

				loop?.Dispose();

				if ((counted || marked) && least > 0)
				{
					file.Line();
					file.Line($"if ({count} < {least})");
					file.Then(
						"throw new global::System.FormatException(" +
						$"\"Input does not match '{EscapeDiagnostic(Named(stage))}' at \" + " +
						"(furthest > window.Offset + start ? furthest : window.Offset + start).ToString(" +
						"global::System.Globalization.CultureInfo.InvariantCulture) + \".\");");
				}
			}

			// And the trivia it allows after it.
			if (trivia is not null)
				Skip(trivia, "Trail");

			// A parse reads the whole input, as over a string: what is left after the last part
			// is not the rule's, and the window may not yet hold whether anything is.
			file.Line();

			using (file.Block("while (start >= window.Length && !window.Ended)"))
				file.Line("window.Extend(ref start);");

			file.Line();
			file.Line("if (start < window.Length)");
			file.Then(
				"throw new global::System.FormatException(" +
				"\"Expected end of input at \" + (furthest > window.Offset + start ? furthest : window.Offset + start).ToString(" +
				"global::System.Globalization.CultureInfo.InvariantCulture) + \".\");");

			// The furthest a reading that failed got, kept as the window moves.
			void Furthest(string failure, bool spaced = true)
			{
				file.Line($"if (window.Offset + {failure}.Position > furthest)");
				file.Then($"furthest = window.Offset + {failure}.Position;");

				if (spaced)
					file.Line();
			}

			// Trivia read where nothing is handed over, growing the window while it may go on.
			void Skip(string recognizer, string tag)
			{
				var hands = $"ref triviaFailure{tag}" + (input ? ", null!" : "");

				file.Line();
				file.Line($"var triviaFailure{tag} = new {FailureType}();");
				file.Line($"int triviaEnd{tag};");
				file.Line();

				using (file.Block("while (true)"))
				{
					file.Line($"triviaFailure{tag} = new {FailureType}();");
					file.Line($"triviaEnd{tag} = {recognizer}(window.Span(), start, {hands});");
					file.Line();

					using (file.Block(
						$"if (((triviaEnd{tag} < 0 ? triviaFailure{tag}.Position : triviaEnd{tag}) >= window.Length || " +
						$"triviaFailure{tag}.Starved) && !window.Ended)"))
					{
						file.Line("window.Extend(ref start);");
						file.Line("continue;");
					}

					file.Line();
					file.Line("break;");
				}

				file.Line();
				Furthest($"triviaFailure{tag}");
				file.Line($"if (triviaEnd{tag} >= 0)");
				file.Then($"start = triviaEnd{tag};");
			}

			// Names per stage rather than one set reused: a stage that is not a repetition
			// declares its locals in the method's own scope, and two of them cannot share.
			void Read(int i, Stage stage, string method, string? built, bool marked, bool counted)
			{
				// No whole input from a window, and nothing here can ask for one: a
				// publication whose rules do is refused a stream (Retention, GRAM5001).
				var hands = (built is null ? "ref failure" + i : $"ref failure{i}, out value{i}") +
					(input ? ", null!" : "");

				file.Line($"var failure{i} = new {FailureType}();");

				if (built is not null)
					file.Line($"{built} value{i} = default!;");

				file.Line($"int end{i};");
				file.Line();

				using (file.Block("while (true)"))
				{
					file.Line($"failure{i} = new {FailureType}();");

					if (tracing is null)
						file.Line($"end{i}     = {method}(window.Span(), start, {hands});");
					else
					{
						file.Line(TraceBegin(
							$"read{i}", Named(stage), "start", "engine", "null", null, sink: "trace",
							offset: "(int)window.Offset", failure: $"failure{i}"));
						TracedRead(
							file, $"{method}(window.Span(), start, {hands})", null, declare: false, sink: "trace",
							read: $"read{i}", failure: $"failure{i}", end: $"end{i}");
					}

					file.Line();

					// The same provisional rule `find` reads by: what ran into the end of the
					// window is not an answer while the reader has more.
					using (file.Block(
						$"if (((end{i} < 0 ? failure{i}.Position : end{i}) >= window.Length || " +
						$"failure{i}.Starved) && !window.Ended)"))
					{
						file.Line("window.Extend(ref start);");
						file.Line("continue;");
					}

					file.Line();
					file.Line("break;");
				}

				file.Line();

				// A bad element of the marked one is stepped over, and what it failed at goes with it.
				if (marked)
				{
					using (file.Block($"if (end{i} >= 0)"))
						Furthest($"failure{i}", spaced: false);

					file.Line();
				}
				else
					Furthest($"failure{i}");

				if (marked && recovery is not null)
				{
					// The complete continuation was tried before the element. A failed
					// element with remaining input is therefore an error, including a
					// mismatch on its first token. EOF cannot supply another recovery.
					using (file.Block($"if (end{i} < 0)"))
					{
						// The end, where no bad element can begin: what it failed at is the parse's.
						using (file.Block("if (start >= window.Length && window.Ended)"))
						{
							Furthest($"failure{i}", spaced: false);
							file.Line("break;");
						}
						file.Line();
						file.Line("var from = start;");
						file.Line("var to   = start;");
						file.Line("var at   = -1;");

						// How far past its start the element got, which stays true when the
						// window moves under it while the synchronization is looked for.
						if (recovery.Words || recovery.Asks.Contains("parserFailurePosition") ||
							recovery.Asks.Contains("parserFailureLine") || recovery.Asks.Contains("parserFailureColumn"))
							file.Line($"var reached = failure{i}.Reach - from;");
						file.Line();

						using (file.Block("while (true)"))
						{
							using (file.Block("while (to <= window.Length)"))
							{
								file.Line($"var syncFailure = new {FailureType}();");
								file.Line($"at = {sync}(window.Span(), to, ref syncFailure);");
								file.Line();

								// Retry this candidate after refill: advancing here skips the first
								// new character or loses a multi-character separator split by a read.
								using (file.Block("if ((syncFailure.Starved || (at < 0 ? syncFailure.Position : at) >= window.Length) && !window.Ended)"))
								{
									file.Line("at = -1;");
									file.Line("break;");
								}
								file.Line();
								file.Line("if (at > to)");
								file.Then("break;");
								file.Line();
								file.Line("at = -1;");
								file.Line("to++;");
							}

							file.Line();
							file.Line("if (at >= 0 || window.Ended)");
							file.Then("break;");
							file.Line();

							// Extended from `from` and not from `start`: what is being
							// looked for is where this element ends, and the element
							// begins at `from`. Dropping to `start` would throw away the
							// front of the very thing about to be handed over — and put
							// `from` before the window, which is nowhere.
							file.Line("var ahead = to - from;");
							file.Line("var after = start - from;");
							file.Line();
							file.Line("window.Extend(ref from);");
							file.Line();
							file.Line("start = from + after;");
							file.Line("to    = from + ahead;");
						}

						file.Line();
						file.Line("if (at < 0)");
						file.Then("to = at = window.Length;");
						file.Line();

						EmitStreamedRejection(
							file, graph, publication, recovery, factory, i, Named(stage));

						file.Line("start = at;");
						file.Line("continue;");
					}
				}
				else if (stage.Repeated)
				{
					file.Line($"if (end{i} < 0)");
					file.Then("break;");
				}
				else
				{
					file.Line($"if (end{i} < 0)");
					file.Then(
						"throw new global::System.FormatException(" +
						$"\"Input does not match '{EscapeDiagnostic(Named(stage))}' at \" + " +
						"furthest.ToString(global::System.Globalization.CultureInfo.InvariantCulture) + \".\");");
				}

				file.Line();

				if (built is not null)
					file.Line($"yield return value{i};");

				if (marked)
					file.Line($"ordinal{i}++;");

				if (counted)
					file.Line($"count{i}++;");

				file.Line($"start = end{i};");
			}
		}

		file.Line();

		EmitOverLines(
			file,
			AccessOf(publication),
			$"global::System.Collections.Generic.IEnumerable<{element}>",
			publication.MethodName,
			results.MemberOf(publication.MethodName),
			publication.Rule.Name);
	}

	static string Comment(Stage stage)
	{
		return stage.Rule is { } rule
			? (stage.Repeated ? "every " : "") + rule.Name
			: stage.Node.ToString();
	}

	static string Named(Stage stage)
	{
		return stage.Rule?.Name ?? stage.Node.ToString();
	}

	static string EscapeDiagnostic(string value)
	{
		return value.Replace("\\", "\\\\").Replace("\"", "\\\"");
	}

	/// <summary>
	/// What a streamed parse does with an element it could not read (§8.2, §8.3).
	/// </summary>
	/// <remarks>
	/// The same two answers as over a string, reached the same way: with a <c>=&gt;</c> the
	/// rejection takes its place in the sequence, and without one it is dropped and told to
	/// the hook. The names are supplied from the window rather than from a span, because
	/// only the window knows how far into the whole input it is — a line number counted
	/// inside the buffer would restart every time the buffer moved.
	/// </remarks>
	static void EmitStreamedRejection(
		Writer file, RecognitionGraph graph, Publication publication, Recovery recovery,
		string factory, int stage, string element)
	{
		// The element is read on a failure of its own, so all that is recorded at the place is its.
		var sets   = $"Expecting_DotGram(ref failure{stage}, failure{stage}.Reach, -1, 0)";
		var once   = Machine.WordsOnce(recovery, sets);
		var wanted = once is null ? $"Wanted_DotGram({sets})" : "wanted";

		string Supplied(string name)
		{
			return name switch
			{
				"parserText" => "window.Text(from, to - from)",
				"parserPosition" => "window.Offset + from",
				"parserOrdinal" => $"ordinal{stage}",
				"parserLine" => "window.LineAt(from)",
				"parserColumn" => "window.ColumnAt(from)",
				// Where in what was read, on the same terms as everywhere else — an extent, not
				// an absolute offset, which is what `parserPosition` beside it is for. It used to
				// add the window's offset and narrow the sum to an `int`, which meant one name
				// with two meanings and, past two gigabytes, a silently wrong one. Nothing
				// reaches this now: a grammar that asks for a span is refused a stream.
				"parserSpan" => "new SourceSpan(from, to - from)",
				"parserFailurePosition" => "window.Offset + from + reached",
				"parserFailureLine" => "window.LineAt(from + reached)",
				"parserFailureColumn" => "window.ColumnAt(from + reached)",
				"parserExpected" => $"Expected_DotGram({wanted}, \"{element}\")",
				"parserMessage" => $"Rejected_DotGram({wanted}, \"{element}\", " +
					"window.LineAt(from + reached).ToString(global::System.Globalization.CultureInfo.InvariantCulture) + \":\" + window.ColumnAt(from + reached).ToString(global::System.Globalization.CultureInfo.InvariantCulture))",
				_ => "default",
			};
		}

		if (recovery.Factory is null)
		{
			file.Line(
				$"{RecoveredMethod}(\"{element}\", {Supplied("parserText")}, " +
				$"{Supplied("parserPosition")}, {Supplied("parserLine")}, " +
				$"{Supplied("parserColumn")}, ordinal{stage}, {Supplied("parserMessage")});");
		}
		else
		{
			var arguments = new List<string>();

			foreach (var name in recovery.Asks)
				arguments.Add(Supplied(name));

			if (once is not null)
				file.Line(once);

			file.Line($"yield return {factory}({string.Join(", ", arguments)});");
		}

		file.Line($"ordinal{stage}++;");
	}

	/// <summary>
	/// One part of a streamed <c>parse</c>: what to recognize, and what becomes of it.
	/// </summary>
	/// <param name="Rule">The rule to call, or null for a part that yields nothing.</param>
	/// <param name="Repeated">Whether it is read until it stops matching.</param>
	/// <param name="Node">What it came from, for the comment above it.</param>
	/// <param name="Seam">
	/// For a repeated stage of a spaced collection: the trivia rule the driver skips
	/// before each element, §4.5's seam between the turns.
	/// </param>
	sealed record Stage(RuleSymbol? Rule, bool Repeated, Node Node, RuleSymbol? Seam = null);

	/// <summary>
	/// A published rule broken into the parts a streamed parse reads one at a time.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Only the operands of the rule itself, and only ones that are calls: what a stage
	/// hands over has to be a whole value of a rule, because that is the unit the parse
	/// commits to when it yields. Anything else in the sequence — <c>eof</c>, a literal
	/// separator, <c>trivia</c> — is recognized and contributes nothing, which is exactly
	/// what §4.1 case 2 already says of it.
	/// </para>
	/// <para>
	/// Null when the rule is not that shape. There is no attempt to be clever here: a
	/// grammar this cannot decompose gets the reason and no overload, which is better than
	/// a driver that quietly reads something else.
	/// </para>
	/// </remarks>
	/// <summary>Whether a call is the rule's own seam — the trivia §4.5 weaves.</summary>
	static bool IsSeamOf(RecognitionGraph graph, RuleSymbol rule, RuleSymbol called)
	{
		return graph.Trivia.TryGetValue(rule, out var seam) &&
		seam is Node.Call(var trivia, _) &&
		ReferenceEquals(trivia, called);
	}

	static IReadOnlyList<Stage>? StagesOf(RecognitionGraph graph, RuleSymbol rule)
	{
		var parts  = graph.PartsOf(rule);
		var stages = new List<Stage>(parts.Count);

		foreach (var part in parts)
			switch (part)
			{
				case Node.Capture(_, Node.Call(var called, _)):
					stages.Add(new Stage(called, Repeated: false, part));
					break;

				case Node.Repeat(Node.Capture(_, Node.Call(var called, _)), _, _):
					stages.Add(new Stage(called, Repeated: true, part));
					break;

				// A spaced collection: the turn §4.5 seamed. The driver reads the seam
				// and the element in turn, the same two reads the in-memory engine makes.
				case Node.Repeat(
					Node.Sequence([Node.Call(var seam, _), Node.Capture(_, Node.Call(var spaced, _))]),
					_, _) when IsSeamOf(graph, rule, seam):
					stages.Add(new Stage(spaced, Repeated: true, part, seam));
					break;

				// Consumes and yields nothing. A capture of something that is not a call
				// would be a value with no rule to recognize it on its own, so it is not one
				// of these — it is what makes the rule undecomposable.
				case Node.Capture:
					return null;

				default:
					stages.Add(new Stage(null, Repeated: false, part));
					break;
			}

		return stages;
	}

	/// <summary>Whether anything in this grammar reads through a window.</summary>
	static bool Streaming(RecognitionGraph graph, bool overKinds = false)
	{
		foreach (var publication in graph.Publications)
			if (Streams(graph, publication, overKinds))
				return true;

		return false;
	}

	/// <summary>
	/// Whether a publication can be read a window at a time.
	/// </summary>
	/// <remarks>
	/// Not over kinds. A window is a stretch of characters, and a machine over tokens is
	/// handed the tokens a lexer already found — there is no window to grow and nothing to
	/// grow it from. Refused here rather than emitted and left not to compile, which is
	/// where it was found: three call sites in the streamed forms handed the recognizer
	/// characters where it now wants the text the tokens came from.
	/// <para>
	/// Nor where a reading reaches a binding (<c>$q: X</c>): a reference compares text bound
	/// earlier, and a window may have let it go. Left out rather than refused, because this form
	/// is offered beside every <c>find</c> unasked, and the string form reads the same grammar.
	/// </para>
	/// </remarks>
	static bool Streams(RecognitionGraph graph, Publication publication, bool overKinds = false)
	{
		return !overKinds && publication.Kind != PublishKind.Yield && !graph.ReachesBoundText(publication.Rule) &&
		(publication.Kind != PublishKind.Parse || publication.ResultType is null) &&
		(publication.Kind == PublishKind.Find
			? Retention.Reads(graph, publication.Rule) is null &&
				Retention.ExtentOf(graph).TryGetValue(publication.Rule, out var extent) &&
				extent != LineExtent.Beyond
			: Retention.StreamedParse(graph, publication.Rule) is null);
	}
}
