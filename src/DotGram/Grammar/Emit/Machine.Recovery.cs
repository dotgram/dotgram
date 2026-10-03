using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// What `recover` compiles to, and what it leaves for the report afterwards.
/// </summary>
/// <remarks>
/// A repetition marked <c>recover</c> is a repetition with a second way of finishing a
/// turn: the continuation failed, the element failed, and the parse walks to the next place the grammar says
/// one can start rather than ending the run there (docs/syntax.md §8.2). That is a
/// different machine from an ordinary repetition and reads as one, which is why it is here
/// rather than beside it — along with the entries it leaves behind and the hook they are
/// reported through once the parse has been accepted.
/// </remarks>
sealed partial class Machine
{
	int CompileRecoveringRepeat(
		Node.Repeat repeatNode, RecoveryPlan recovery, int next, FollowSets.Continuation following)
	{
		var (body, min, max) = repeatNode;
		if (recovery.Recovery.YieldStep) max = 1;

		if (max == 0)
			return next;

		var exit      = Reserve(out var atExit);
		var loop      = Reserve(out var atLoop);
		var attempt   = Reserve(out var atAttempt);
		var recovered = Reserve(out var atRecovered);
		var synced    = Reserve(out var atSynced);
		var advance   = Reserve(out var atAdvance);
		var scan      = Reserve(out var atScan);
		var asked     = Reserve(out var atAsked);
		var after     = Reserve(out var atAfter);
		var entry     = Reserve(out var atEntry);
		var inner     = Compile(
			body, after,
			new FollowSets.Continuation(
				FirstSets.Of(body, _graph).Or(following.Plain), FirstSets.First.All));
		var sync      = Compile(recovery.Recovery.Sync, synced, FollowSets.Continuation.All);

		atEntry.Line("var repeatIndex = entries.Count;");
		atEntry.Line("entries.Add(new ParserEntry(ParserEntry.Repeat, 0, p, call, atomic, repeat, lookahead, 0));");
		atEntry.Line("repeat = repeatIndex;");
		atEntry.Line($"goto {Label(atEntry, loop)};");

		atLoop.Line("global::System.Diagnostics.Debug.Assert(repeat >= 0 && repeat < entries.Count);");
		atLoop.Line("var repeating = entries[repeat];");

		// What stood at the place the turn begins, before the continuation and the element are
		// tried there: what is recorded at that place since is theirs.
		if (recovery.Recovery.Explains)
			atLoop.Line(Stand);

		if (max is { } limit)
			atLoop.Line($"if (repeating.Value >= {limit}) goto {Label(atLoop, exit)};");

		atLoop.Line($"if (repeating.Value >= {min})");
		atLoop.Then($"entries.Add(new ParserEntry(ParserEntry.Choice, {Resuming(atLoop, attempt)}, p, call, atomic, repeat, lookahead, 0));");
		atLoop.Line($"if (repeating.Value >= {min}) goto {Label(atLoop, exit)};");
		atLoop.Line($"goto {Label(atLoop, attempt)};");

		atAttempt.Line("reach = p;");
		atAttempt.Line($"entries.Add(new ParserEntry(ParserEntry.Choice, {Resuming(atAttempt, asked)}, p, call, atomic, repeat, lookahead, 0));");
		atAttempt.Line($"goto {Label(atAttempt, inner)};");

		atAsked.Line("global::System.Diagnostics.Debug.Assert(repeat >= 0 && repeat < entries.Count);");
		// The continuation has already failed. Any remaining input belongs to a bad
		// element, even if its first token did not match. Empty EOF cannot recover:
		// a required element or continuation is missing, and scanning would not advance.
		atAsked.Line($"if ({Short(1)}) {{ expected = null; goto Fail; }}");

		// What would have fit where the element stopped, taken now: the synchronization about to
		// be looked for records refusals of its own, further along.
		if (recovery.Recovery.Explains)
			atAsked.Line("parser.Expecting = Expecting_DotGram(ref failure, reach);");

		atAsked.Line(
			$"entries.Add(new ParserEntry(ParserEntry.PendingRecovery, {Resuming(atAsked, asked)}, p, call, reach, repeat, lookahead, 0));");
		atAsked.Line($"goto {Label(atAsked, scan)};");

		atScan.Line($"if ({Short(1)}) goto {Label(atScan, recovered)};");
		EmitRecoverySearch(atScan, recovery.Recovery.Sync);
		atScan.Line("syncFrom = p;");
		atScan.Line($"entries.Add(new ParserEntry(ParserEntry.Choice, {Resuming(atScan, advance)}, p, call, atomic, repeat, lookahead, 0));");
		atScan.Line($"goto {Label(atScan, sync)};");

		atSynced.Line($"if (p <= syncFrom) {{ expected = null; goto Fail; }}");
		atSynced.Line($"goto {Label(atSynced, recovered)};");

		atAdvance.Line("p++;");
		atAdvance.Line($"goto {Label(atAdvance, scan)};");

		atRecovered.Line("global::System.Diagnostics.Debug.Assert(repeat >= 0 && repeat < entries.Count);");
		atRecovered.Line("var recoveryFrom = p;");
		atRecovered.Line("var recoveryReach = p;");
		atRecovered.Line("var recoveryTo = p;");
		atRecovered.Line("var recoveryBoundary = false;");
		atRecovered.Line("for (var recoveryAt = entries.Count - 1; recoveryAt > repeat; recoveryAt--)");
		using (atRecovered.Block(""))
		{
			atRecovered.Line("var candidate = entries[recoveryAt];");
			// Against the mark and not the number: what the entry holds is what `Resuming`
			// wrote above, and that is the state under the number it is written with.
			atRecovered.Line(
				$"if (candidate.Kind == ParserEntry.PendingRecovery && candidate.State == {Mark(Lands, asked)})");
			using (atRecovered.Block(""))
			{
				atRecovered.Line("recoveryFrom = candidate.Position;");
				atRecovered.Line("recoveryReach = candidate.AtomicIndex;");
				atRecovered.Line("break;");
			}
			atRecovered.Line(
				"if (!recoveryBoundary && candidate.Kind == ParserEntry.Choice && " +
				$"candidate.State == {Mark(Lands, advance)})");
			using (atRecovered.Block(""))
			{
				atRecovered.Line("recoveryTo = candidate.Position;");
				atRecovered.Line("recoveryBoundary = true;");
			}
		}
		DeactivateChoices(atRecovered, asked);

		// Kept by the entry the element is recorded as, until it is built after the parse.
		if (recovery.Recovery.Explains)
			atRecovered.Line($"{Keep("parser")}[entries.Count] = parser.Expecting;");

		atRecovered.Line(
			$"entries.Add(new ParserEntry(ParserEntry.Recovery, {recovery.Id}, recoveryFrom, call, recoveryReach, " +
			"repeat, lookahead, recoveryTo, entries[repeat].Value" +
			(recovery.Recovery.YieldStep ? " + failure.RecoveryOrdinal" : "") + "));");
		atRecovered.Line("var recoveredRepeat = entries[repeat];");
		atRecovered.Line(
			"entries[repeat] = new ParserEntry(ParserEntry.Repeat, 0, recoveredRepeat.Position, " +
			"recoveredRepeat.CallIndex, recoveredRepeat.AtomicIndex, recoveredRepeat.RepeatIndex, " +
			"recoveredRepeat.LookaheadIndex, recoveredRepeat.Value + 1);");
		atRecovered.Line($"goto {Label(atRecovered, loop)};");

		DeactivateChoices(atAfter, asked);
		atAfter.Line("var acceptedRepeat = entries[repeat];");
		atAfter.Line(
			"entries[repeat] = new ParserEntry(ParserEntry.Repeat, 0, acceptedRepeat.Position, " +
			"acceptedRepeat.CallIndex, acceptedRepeat.AtomicIndex, acceptedRepeat.RepeatIndex, " +
			"acceptedRepeat.LookaheadIndex, acceptedRepeat.Value + 1);");
		atAfter.Line($"goto {Label(atAfter, loop)};");

		LeaveRepeat(atExit, next);

		return entry;
	}

	static void DeactivateChoices(Writer writer, int attempt)
	{
		using (writer.Block("for (var choiceAt = entries.Count - 1; choiceAt > repeat; choiceAt--)"))
		{
			writer.Line("var choice = entries[choiceAt];");
			writer.Line("if (choice.Kind == ParserEntry.Choice)");
			writer.Then(
				"entries[choiceAt] = new ParserEntry(ParserEntry.Dead, choice.State, choice.Position, " +
				"choice.CallIndex, choice.AtomicIndex, choice.RepeatIndex, choice.LookaheadIndex, " +
				"choice.Value, choice.RuleIndex);");
			// Earlier turns already committed their choices. The attempt marker remains
			// as Dead after success, or becomes PendingRecovery when its choice unwinds.
			// Unlike a cached arena offset, the marker follows rollback and nested loops.
			writer.Line(
				$"if (choice.RepeatIndex == repeat && choice.State == {Mark(Lands, attempt)} && " +
				"(choice.Kind == ParserEntry.Choice || choice.Kind == ParserEntry.Dead || " +
				"choice.Kind == ParserEntry.PendingRecovery)) break;");
		}
	}


	void ReportRecoveries(Writer file)
	{
		using (file.Block("for (var recoveryAt = 0; recoveryAt < entries.Count; recoveryAt++)"))
		{
			file.Line("var recovered = entries[recoveryAt];");
			file.Line("if (recovered.Kind != ParserEntry.Recovery) continue;");

			using (file.Block("switch (recovered.State)"))
				foreach (var recovery in _recoveryPlans)
					using (file.Block($"case {recovery.Id}:"))
					{
						if (recovery.Recovery.Factory is null)
							file.Line(
								$"{CSharpEmitter.RecoveredMethod}(\"{Escape(recovery.Element?.Name ?? "an element")}\", " +
								$"{RecoverySupplied("parserText", recovery)}, {RecoverySupplied("parserPosition", recovery)}, " +
								$"{RecoverySupplied("parserLine", recovery)}, {RecoverySupplied("parserColumn", recovery)}, " +
								$"{RecoverySupplied("parserOrdinal", recovery)}, {RecoverySupplied("parserMessage", recovery)});");

						file.Line("break;");
					}
		}
	}

	void MaterializeRecovery(Writer file, RecoveryPlan plan)
	{
		using (file.Block($"case {plan.Id}:"))
		{
			if (plan.Recovery.Factory is null)
			{
				file.Line(
					$"{CSharpEmitter.RecoveredMethod}(\"{Escape(plan.Element?.Name ?? "an element")}\", " +
					$"{RecoverySupplied("parserText", plan)}, {RecoverySupplied("parserPosition", plan)}, " +
					$"{RecoverySupplied("parserLine", plan)}, {RecoverySupplied("parserColumn", plan)}, " +
					$"{RecoverySupplied("parserOrdinal", plan)}, {RecoverySupplied("parserMessage", plan)});");
			}
			else if (plan.Slot >= 0)
			{
				var arguments = new List<string>();

				foreach (var name in plan.Recovery.Asks)
					arguments.Add(RecoverySupplied(name, plan));

				if (WordsOnce(plan.Recovery) is { } once)
					file.Line(once);

				file.Line(
					$"{ValueInto(RecoveredType(plan), "recoveryAt")} = " +
					$"{plan.Method}({string.Join(", ", arguments)});");
			}

			file.Line("break;");
		}
	}

	/// <summary>
	/// What a recovery hands its handler, said in characters however the machine reads.
	/// </summary>
	/// <remarks>
	/// Everything here is about <em>the input the author wrote</em> and not about what the
	/// parser is walking: a handler is given a message to show a person. So over kinds each
	/// of the five is mapped back — the position of a token is the position of its first
	/// character, and a line and a column are counted in the source rather than in the
	/// kinds, where they would all be one.
	/// </remarks>
	/// <param name="located">
	/// What counts lines on from the last place asked about: the parser's own in the engine, whose
	/// walks are one a step where a parse yields, and a walk's own in the reader's. Over buffered
	/// input the buffer counts, since only it knows what it has let go.
	/// </param>
	/// <param name="expected">
	/// What would have fit where the element stopped, as the sets recorded there when it failed:
	/// kept by the record of the element where it is built after the parse (<see cref="Keep"/>),
	/// and taken before the synchronization is looked for where it is built as it is stepped over.
	/// </param>
	string RecoverySupplied(
		string name, RecoveryPlan plan, string located = "parser.Located",
		string expected = "Recalled_DotGram(parser.Expectations, recoveryAt)")
	{
		var element = Escape(plan.Element?.Name ?? "an element");
		var wanted  = WordsOnce(plan.Recovery) is null ? $"Wanted_DotGram({expected})" : "wanted";

		return name switch
		{
			"parserText" => Cut("recovered.Position", "recovered.Value - recovered.Position"),
			"parserPosition" => At("recovered.Position"),
			"parserOrdinal" => "recovered.RuleIndex",
			"parserLine" => Line("recovered.Position", located),
			"parserColumn" => Column("recovered.Position", located),
			"parserSpan" => Span("recovered.Position", "recovered.Value - recovered.Position"),
			"parserFailurePosition" => At("recovered.AtomicIndex"),
			"parserFailureLine" => Line("recovered.AtomicIndex", located),
			"parserFailureColumn" => Column("recovered.AtomicIndex", located),
			"parserExpected" => $"Expected_DotGram({wanted}, \"{element}\")",
			"parserMessage" => $"Rejected_DotGram({wanted}, \"{element}\", {Place("recovered.AtomicIndex", located)})",
			"parserInput" => "parserInput",
			_ => "default",
		};
	}

	/// <summary>
	/// Where a factory asks both for what would have fit and for the message, the words they share,
	/// made once before the call as <c>wanted</c> — never for the hook, whose arguments go with the
	/// call nobody implements.
	/// </summary>
	internal static string? WordsOnce(Recovery recovery, string expected = "Recalled_DotGram(parser.Expectations, recoveryAt)")
	{
		return recovery.Factory is not null && recovery.Asks.Contains("parserExpected") && recovery.Asks.Contains("parserMessage")
			? $"var wanted = Wanted_DotGram({expected});"
			: null;
	}

	/// <summary>A place the machine names, as a message says it: <c>line:column</c>.</summary>
	string Place(string position, string located)
	{
		const string Invariant = "global::System.Globalization.CultureInfo.InvariantCulture";

		return $"{Line(position, located)}.ToString({Invariant}) + \":\" + {Column(position, located)}.ToString({Invariant})";
	}

	/// <summary>Which line a place the machine names is on, for a person.</summary>
	string Line(string position, string located)
	{
		return BufferedInput && !OverKinds
			? $"text.LineAt({At(position)})"
			: $"{located}.LineAt({Source}, {At(position)})";
	}

	/// <summary>How far into its line a place the machine names is, for a person.</summary>
	string Column(string position, string located)
	{
		return BufferedInput && !OverKinds
			? $"text.ColumnAt({At(position)})"
			: $"{located}.ColumnAt({Source}, {At(position)})";
	}

	/// <summary>
	/// Where what each rejected element wanted is kept until the element is built, by the record
	/// of the element — its arena entry, or where its record begins on the tape: two elements may
	/// stop at one place. On the parser or the tape, made the first time a parse rejects anything.
	/// </summary>
	internal static string Keep(string owner)
	{
		return $"({owner}.Expectations ??= new global::System.Collections.Generic.Dictionary<int, string[]?>())";
	}

	/// <summary>
	/// Where the furthest refusal stands as a turn of a repetition marked <c>recover</c> begins, and
	/// how many sets tie there, for <c>Expecting_DotGram</c> to tell the element's own from them.
	/// </summary>
	internal const string Stand = "failure.Stood = failure.Position; failure.Tied = failure.ExpectedMore?.Count ?? 0;";

	/// <summary>Whether the engine keeps what its rejected elements wanted until they are built.</summary>
	public bool KeepsExpectations => _recoveryPlans.Exists(static plan => plan.Recovery.Explains);

	/// <summary>
	/// Whether anything a group recognised outlives the group.
	/// </summary>
	/// <remarks>
	/// A capture records where it began and ended, a construction records what to build, and
	/// a call to a rule with a value records where it completed — all as entries, and all
	/// read after the parse has finished. A group whose body has none of them recognised
	/// nothing that anything later will ask about, and what it leaves in the arena is only
	/// the ways back that committing is there to close.
	/// </remarks>
}
