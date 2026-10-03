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

		// Its own locals (RecoveryLocals): a recovering repetition inside the element of another,
		// or one in the synchronization it looks for, keeps to its own.
		// Numbered by the place it is compiled at rather than by the plan: a repetition compiled at
		// two places is two sets of them.
		var id        = _recoveringStates.Count;
		var again     = Reenters(repeatNode);
		_recoveringStates.Add((recovery, asked, again));
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
		// tried there: what is recorded at that place since is theirs. Where the repetition can be
		// read again inside its own element, a record of the turn on the arena instead of in the
		// locals, which a reading inside would run over: what stood here, and how far the reading
		// this one is inside had got, which this turn's count is about to begin afresh over.
		if (again)
			atLoop.Line(
				$"entries.Add(new ParserEntry(ParserEntry.Turn, {id}, failure.Position, failure.ExpectedMore?.Count ?? 0, " +
				$"reach{id}, repeat, lookahead, 0));");
		else if (recovery.Recovery.Explains)
			atLoop.Line($"stood{id} = failure.Position; tied{id} = failure.ExpectedMore?.Count ?? 0;");

		if (max is { } limit)
			atLoop.Line($"if (repeating.Value >= {limit}) goto {Label(atLoop, exit)};");

		atLoop.Line($"if (repeating.Value >= {min})");
		atLoop.Then($"entries.Add(new ParserEntry(ParserEntry.Choice, {Resuming(atLoop, attempt)}, p, call, atomic, repeat, lookahead, 0));");
		atLoop.Line($"if (repeating.Value >= {min}) goto {Label(atLoop, exit)};");
		atLoop.Line($"goto {Label(atLoop, attempt)};");

		atAttempt.Line("reach = p;");

		// The continuation tried since the turn began is the enclosing reading's too: how far it
		// got goes into the record before this turn's count begins. The record is on top: the
		// way to here was pushed over it and has been taken.
		if (again)
		{
			atAttempt.Line("var turnAt = entries[entries.Count - 1];");
			atAttempt.Line("global::System.Diagnostics.Debug.Assert(turnAt.Kind == ParserEntry.Turn);");
			atAttempt.Line($"if (reach{id} > turnAt.AtomicIndex)");
			atAttempt.Then(
				$"entries[entries.Count - 1] = new ParserEntry(ParserEntry.Turn, {id}, turnAt.Position, turnAt.CallIndex, " +
				$"reach{id}, turnAt.RepeatIndex, turnAt.LookaheadIndex, 0);");
		}

		atAttempt.Line($"reach{id} = p;");
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
		{
			if (again)
			{
				// The turn's record is on top again: the way back to here was pushed over it.
				atAsked.Line("var turnOf = entries[entries.Count - 1];");
				atAsked.Line("global::System.Diagnostics.Debug.Assert(turnOf.Kind == ParserEntry.Turn);");
				atAsked.Line($"expecting{id} = Expecting_DotGram(ref failure, reach{id}, turnOf.Position, turnOf.CallIndex);");
			}
			else
				atAsked.Line($"expecting{id} = Expecting_DotGram(ref failure, reach{id}, stood{id}, tied{id});");
		}

		atAsked.Line(
			$"entries.Add(new ParserEntry(ParserEntry.PendingRecovery, {Resuming(atAsked, asked)}, p, call, reach{id}, repeat, lookahead, 0));");
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

		if (again)
			TurnEnds(atRecovered, id);

		// Kept by the entry the element is recorded as, until it is built after the parse.
		if (recovery.Recovery.Explains)
			atRecovered.Line($"{Keep("parser")}[entries.Count] = expecting{id};");

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

		if (again)
			TurnEnds(atAfter, id);

		atAfter.Line("var acceptedRepeat = entries[repeat];");
		atAfter.Line(
			"entries[repeat] = new ParserEntry(ParserEntry.Repeat, 0, acceptedRepeat.Position, " +
			"acceptedRepeat.CallIndex, acceptedRepeat.AtomicIndex, acceptedRepeat.RepeatIndex, " +
			"acceptedRepeat.LookaheadIndex, acceptedRepeat.Value + 1);");
		atAfter.Line($"goto {Label(atAfter, loop)};");

		LeaveRepeat(atExit, next);

		return entry;
	}

	/// <summary>
	/// Where a turn of a repetition read again inside its own element ends, matched or stepped
	/// over: how far the reading it is inside had got is put back under what this turn got to —
	/// or, where the turn was read inside a look ahead, put back as it was: nothing a look ahead
	/// reads counts as how far an element got, and the turn's own count began where it began.
	/// </summary>
	/// <remarks>
	/// The turn's record stays where it is — what the turn recognized is above it and is kept —
	/// so it is found, the latest of this repetition's own reading (its <c>RepeatIndex</c>); a
	/// reading inside, of the same repetition, has a repetition entry of its own.
	/// </remarks>
	static void TurnEnds(Writer writer, int id)
	{
		using (writer.Block("for (var turnAt = entries.Count - 1; turnAt > repeat; turnAt--)"))
		{
			writer.Line("var ended = entries[turnAt];");
			writer.Line($"if (ended.Kind != ParserEntry.Turn || ended.State != {id} || ended.RepeatIndex != repeat) continue;");
			writer.Line($"if (ended.LookaheadIndex >= 0 || ended.AtomicIndex > reach{id}) reach{id} = ended.AtomicIndex;");
			writer.Line("break;");
		}
	}

	/// <summary>
	/// Where unwinding takes a turn's record away (<see cref="Reenters"/>): the reading it was
	/// inside gets back how far it had got, under what was reached since — or as it was, where the
	/// turn was read inside a look ahead (<see cref="TurnEnds"/>).
	/// </summary>
	void TurnsUnwound(Writer file)
	{
		if (!KeepsTurns)
			return;

		using (file.Block("if (entry.Kind == ParserEntry.Turn)"))
		{
			using (file.Block("switch (entry.State)"))
				for (var id = 0; id < _recoveringStates.Count; id++)
					if (_recoveringStates[id].Again)
					{
						file.Line($"case {id}:");
						file.Line($"\tif (entry.LookaheadIndex >= 0 || entry.AtomicIndex > reach{id}) reach{id} = entry.AtomicIndex;");
						file.Line("\tbreak;");
					}

			file.Line("continue;");
		}

		file.Line();
	}

	/// <summary>
	/// Whether a repetition marked <c>recover</c> can be read again while one of its turns is open:
	/// inside its element, the continuation tried at its boundaries or its synchronization, which
	/// is a rule that holds it calling itself — through a look ahead into that rule, or by
	/// recursion.
	/// </summary>
	/// <remarks>
	/// Where it cannot, its turn's state is kept in locals, a set for each repetition, as it
	/// always was. Where it can, the reading inside would run over them, so each turn keeps what
	/// it needs on the arena (<c>ParserEntry.Turn</c>), where unwinding finds it.
	/// </remarks>
	bool Reenters(Node.Repeat repeatNode)
	{
		var holders = new HashSet<RuleSymbol>();
		var pending = new Stack<RuleSymbol>();

		foreach (var pair in _graph.Bodies)
			if (NodeWalk.Descendants(pair.Value).Any(node => ReferenceEquals(node, repeatNode)))
				pending.Push(pair.Key);

		// A rule compiled in place holds what it holds wherever it is called.
		while (pending.Count > 0)
		{
			var rule = pending.Pop();

			if (!holders.Add(rule) || !CanInline(rule))
				continue;

			foreach (var pair in _graph.Bodies)
				if (NodeWalk.Descendants(pair.Value).Any(node => node is Node.Call(var called, _) && called == rule))
					pending.Push(pair.Key);
		}

		var again = holders.Overlaps(_graph.Recursive);

		KeepsTurns |= again;

		return again;
	}

	/// <summary>Whether a repetition this machine compiled keeps its turns on the arena (<see cref="Reenters"/>).</summary>
	public bool KeepsTurns { get; private set; }

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
	/// how many sets tie there, for <c>Expecting_DotGram</c> to tell the element's own from them,
	/// and for <c>Refuse_DotGram</c> to record a set the turn wants again though it was said there
	/// before. A reader's: it keeps them on the failure, and puts back what an enclosing turn had
	/// there where its repetition is left (<see cref="ReaderWriter"/>'s recovering loop).
	/// </summary>
	internal const string Stand = "failure.Stood = failure.Position; failure.Tied = failure.ExpectedMore?.Count ?? 0;";

	/// <summary>Every recovering repetition the engine compiled, with the state its element's failure lands in.</summary>
	readonly List<(RecoveryPlan Plan, int Asked, bool Again)> _recoveringStates = [];

	/// <summary>
	/// The engine's locals of each recovering repetition: how far its element got, and where it
	/// explains its element, what stood where its turn began and what it took for the element.
	/// </summary>
	/// <remarks>
	/// One set per repetition, not one for the engine: a repetition inside the element of another,
	/// or in the synchronization another looks for, ran over the outer one's when they were shared —
	/// the outer element was told how far the inner one got, and what the inner one wanted. A
	/// repetition reached again inside its own element (a rule that calls itself through it) keeps
	/// what stood where each turn began on the arena instead, and puts back how far the enclosing
	/// turn had got wherever an inner one ends (<see cref="Reenters"/>).
	/// </remarks>
	void RecoveryLocals(Writer file)
	{
		for (var id = 0; id < _recoveringStates.Count; id++)
		{
			var (plan, asked, again) = _recoveringStates[id];

			file.Line($"var reach{id} = 0;");

			if (plan.Recovery.Explains && Written(asked))
			{
				// A repetition read again inside its own element keeps these on the arena.
				if (!again)
				{
					file.Line($"var stood{id} = -1;");
					file.Line($"var tied{id} = 0;");
				}

				file.Line($"string[]? expecting{id} = null;");
			}
		}
	}

	/// <summary>How far each recovering repetition's element got, raised where the engine fails.</summary>
	void RecoveryReaches(Writer file)
	{
		for (var id = 0; id < _recoveringStates.Count; id++)
		{
			file.Line($"if (lookahead < 0 && p > reach{id})");
			file.Then($"reach{id} = p;");
		}
	}

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
