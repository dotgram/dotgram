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
	static void EmitStreamingFind(
		Writer file, Publication publication, string method, string name,
		string match, string hands, bool builds)
	{
		var recognized = builds ? "recognized" : "window.Text(start, end - start)";

		file.Line($"/// <summary>Every occurrence of <c>{name}</c> in a reader, read as it is asked for.</summary>");
		file.Line("/// <remarks>");
		file.Line("/// The input is read through a buffer that is reused, so what is held is what the");
		file.Line($"/// occurrence being read needs and not the input (docs/syntax.md §6.3).");
		file.Line("/// </remarks>");

		using (file.Block(
			$"public static global::System.Collections.Generic.IEnumerable<{match}> {method}(" +
			"global::System.IO.TextReader input)"))
		{
			file.Line($"var window = new {WindowType}(input, {WindowSize});");
			file.Line("var start  = 0;");
			file.Line();

			using (file.Block("while (true)"))
			{
				file.Line($"var failure = new {FailureType}();");
				file.Line();
				file.Line($"var end = {MethodOf(publication.Rule)}(window.Span(), start{hands});");
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

		EmitOverLines(file, $"global::System.Collections.Generic.IEnumerable<{match}>", method, name);
	}

	/// <summary>
	/// The same method again, taking lines rather than a reader (§6.3).
	/// </summary>
	/// <remarks>
	/// One line, because a sequence of lines is a reader once the terminators are put
	/// back. Written out rather than left to the caller so that <c>File.ReadLines</c> and
	/// a <c>List&lt;string&gt;</c> are as ordinary an input as a string is.
	/// </remarks>
	static void EmitOverLines(Writer file, string returns, string method, string name)
	{
		file.Line($"/// <summary>The same, over a sequence of lines (docs/syntax.md §6.3).</summary>");
		file.Line("/// <remarks>");
		file.Line("/// The lines are read as though they were a file, with a newline put back on the");
		file.Line($"/// end of each: a sequence of lines has had its terminators taken off, and <c>{name}</c>");
		file.Line("/// may well be looking for one.");
		file.Line("/// </remarks>");

		using (file.Block(
			$"public static {returns} {method}(" +
			"global::System.Collections.Generic.IEnumerable<string> input)"))
		{
			file.Line($"return {method}(new {LinesType}(input));");
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
/// the two agree is the mark: at every boundary the complete continuation gets first
/// refusal, and an element actually accepted or recovered is then committed. The stages
/// around it are read once each.
	/// </para>
	/// <para>
	/// Each stage reads through the window with the same provisional rule <c>find</c> uses:
	/// a result that ran into the end of what is held is not an answer while the reader has
	/// more.
	/// </para>
	/// </remarks>
	static void EmitStreamingParse(
		Writer file, RecognitionGraph graph, Publication publication, ResultTypes results,
		IReadOnlyList<Stage> stages, IReadOnlyList<string> parts,
		Recovery? recovery, string? sync, string factory, Func<int, string?> continuation)
	{
		var element = graph.Types[publication.Rule];

		element = element.Substring(0, element.Length - "[]".Length);

		file.Line($"/// <summary>Reads <c>{publication.Rule.Name}</c> from a reader, a part at a time.</summary>");
		file.Line("/// <remarks>");
		file.Line("/// The input is read through a buffer that is reused, so what is held is the part");
		file.Line("/// being read and not the input. Each element is handed over as it is read, which");
		file.Line("/// is why the repetition has to be marked 'recover' (docs/syntax.md §6.3, §8.2).");
		file.Line("/// </remarks>");

		using (file.Block(
			$"public static global::System.Collections.Generic.IEnumerable<{element}> " +
			$"{publication.MethodName}(global::System.IO.TextReader input)"))
		{
			file.Line($"var window = new {WindowType}(input, {WindowSize});");
			file.Line("var start  = 0;");

			for (var i = 0; i < stages.Count; i++)
			{
				var stage  = stages[i];
				var method = stage.Rule is { } rule ? MethodOf(rule) : parts[i];
				var built  = stage.Rule is { } called ? results.QualifiedOf(called) : null;

				file.Line();
				file.Line("// " + Comment(stage));

				// Outside the loop, because it counts the elements of the whole repetition
				// and a rejected one holds its place in the numbering (§8.2).
				if (stage.Repeated && recovery is not null)
					file.Line($"var ordinal{i} = 0;");

				var loop = stage.Repeated ? file.Block("while (true)") : null;

				if (stage.Repeated && continuation(i) is { } probe)
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
					file.Line($"if (continuationEnd{i} >= 0)");
					file.Then("break;");
					file.Line();
				}

				Read(i, stage, method, built);

				loop?.Dispose();
			}

			// Names per stage rather than one set reused: a stage that is not a repetition
			// declares its locals in the method's own scope, and two of them cannot share.
			void Read(int i, Stage stage, string method, string? built)
			{
				var hands = built is null ? "ref failure" + i : $"ref failure{i}, out value{i}";

				file.Line($"var failure{i} = new {FailureType}();");

				if (built is not null)
					file.Line($"{built} value{i} = default!;");

				file.Line($"int end{i};");
				file.Line();

				using (file.Block("while (true)"))
				{
					file.Line($"failure{i} = new {FailureType}();");
					file.Line($"end{i}     = {method}(window.Span(), start, {hands});");
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

				if (stage.Repeated && recovery is not null)
				{
					// §8.2 in a stream. An element that never began ends the repetition; one
					// that began and broke is an error to step over, and where to pick up
					// again is the synchronization expression — looked for in the window,
					// reading more of it when the search runs out of what is held.
					using (file.Block($"if (end{i} < 0)"))
					{
						file.Line($"if (failure{i}.Reach <= start)");
						file.Then("break;");
						file.Line();
						file.Line("var from = start;");
						file.Line("var to   = start;");
						file.Line("var at   = -1;");
						file.Line();

						using (file.Block("while (true)"))
						{
							using (file.Block("while (to <= window.Length)"))
							{
								file.Line($"at = {sync}(window.Span(), to);");
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
						$"(window.Offset + failure{i}.Position).ToString(" +
						"global::System.Globalization.CultureInfo.InvariantCulture) + \".\");");
				}

				file.Line();

				if (built is not null)
					file.Line($"yield return value{i};");

				if (stage.Repeated && recovery is not null)
					file.Line($"ordinal{i}++;");

				file.Line($"start = end{i};");
			}
		}

		file.Line();

		EmitOverLines(
			file,
			$"global::System.Collections.Generic.IEnumerable<{element}>",
			publication.MethodName,
			publication.Rule.Name);
	}

	static string Comment(Stage stage) =>
		stage.Rule is { } rule
			? (stage.Repeated ? "every " : "") + rule.Name
			: stage.Node.ToString();

	static string Named(Stage stage) => stage.Rule?.Name ?? stage.Node.ToString();

	static string EscapeDiagnostic(string value) =>
		value.Replace("\\", "\\\\").Replace("\"", "\\\"");

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
		string Supplied(string name) => name switch
		{
			"parserText"     => "window.Text(from, to - from)",
			"parserPosition" => "window.Offset + from",
			"parserOrdinal"  => $"ordinal{stage}",
			"parserLine"     => "window.LineAt(from)",
			"parserColumn"   => "window.ColumnAt(from)",
			"parserSpan"     => "new global::DotGram.SourceSpan((int)(window.Offset + from), to - from)",
			"parserMessage"  => $"\"Input does not match '{element}' at \" + " +
				$"(window.Offset + failure{stage}.Reach).ToString(" +
				"global::System.Globalization.CultureInfo.InvariantCulture) + \".\"",
			_                => "default",
		};

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
	sealed record Stage(RuleSymbol? Rule, bool Repeated, Node Node);

	/// <summary>
	/// A published rule broken into the parts a streamed parse reads one at a time.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Only the operands of the rule itself, and only ones that are calls: what a stage
	/// hands over has to be a whole value of a rule, because that is the unit the parse
	/// commits to when it yields. Anything else in the sequence — <c>eof</c>, a literal
	/// separator, <c>Trivia</c> — is recognized and contributes nothing, which is exactly
	/// what §4.1 case 2 already says of it.
	/// </para>
	/// <para>
	/// Null when the rule is not that shape. There is no attempt to be clever here: a
	/// grammar this cannot decompose gets the reason and no overload, which is better than
	/// a driver that quietly reads something else.
	/// </para>
	/// </remarks>
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
	static bool Streaming(RecognitionGraph graph)
	{
		foreach (var publication in graph.Publications)
			if (Streams(graph, publication))
				return true;

		return false;
	}

	static bool Streams(RecognitionGraph graph, Publication publication) =>
		publication.Kind == PublishKind.Find
			? Retention.Reads(graph) is null &&
				Retention.ExtentOf(graph).TryGetValue(publication.Rule, out var extent) &&
				extent != LineExtent.Beyond
			: Retention.StreamedParse(graph, publication.Rule) is null;
}
