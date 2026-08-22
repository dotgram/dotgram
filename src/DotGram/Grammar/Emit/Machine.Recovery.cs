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
/// turn: the element began, failed, and the parse walks to the next place the grammar says
/// one can start rather than ending the run there (docs/syntax.md §8.2). That is a
/// different machine from an ordinary repetition and reads as one, which is why it is here
/// rather than beside it — along with the entries it leaves behind and the hook they are
/// reported through once the parse has been accepted.
/// </remarks>
sealed partial class Machine
{
	int CompileRecoveringRepeat(
		Node.Repeat repeatNode, RecoveryPlan recovery, int next, FirstSets.First following)
	{
		var (body, min, max) = repeatNode;

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
		var inner     = Compile(body, after, FirstSets.Of(body, _graph).Or(following));
		var sync      = Compile(recovery.Recovery.Sync, synced, FirstSets.First.All);

		atEntry.Line("var repeatIndex = entries.Count;");
		atEntry.Line("entries.Add(new ParserEntry(ParserEntry.Repeat, 0, p, call, atomic, repeat, lookahead, 0));");
		atEntry.Line("repeat = repeatIndex;");
		atEntry.Line($"goto {Label(loop)};");

		atLoop.Line("global::System.Diagnostics.Debug.Assert(repeat >= 0 && repeat < entries.Count);");
		atLoop.Line("var repeating = entries[repeat];");

		if (max is { } limit)
			atLoop.Line($"if (repeating.Value >= {limit}) goto {Label(exit)};");

		atLoop.Line($"if (repeating.Value >= {min})");
		atLoop.Then($"entries.Add(new ParserEntry(ParserEntry.Choice, {attempt}, p, call, atomic, repeat, lookahead, 0));");
		atLoop.Line($"if (repeating.Value >= {min}) goto {Label(exit)};");
		atLoop.Line($"goto {Label(attempt)};");

		atAttempt.Line("reach = p;");
		atAttempt.Line("owned = false;");
		atAttempt.Line($"entries.Add(new ParserEntry(ParserEntry.Choice, {asked}, p, call, atomic, repeat, lookahead, 0));");
		atAttempt.Line($"goto {Label(inner)};");

		atAsked.Line("global::System.Diagnostics.Debug.Assert(repeat >= 0 && repeat < entries.Count);");
		atAsked.Line("if (!owned && reach <= p) goto Fail;");
		atAsked.Line(
			$"entries.Add(new ParserEntry(ParserEntry.PendingRecovery, {asked}, p, call, reach, repeat, lookahead, 0));");
		atAsked.Line($"goto {Label(scan)};");

		atScan.Line($"if (p >= text.Length) goto {Label(recovered)};");
		atScan.Line("syncFrom = p;");
		atScan.Line($"entries.Add(new ParserEntry(ParserEntry.Choice, {advance}, p, call, atomic, repeat, lookahead, 0));");
		atScan.Line($"goto {Label(sync)};");

		atSynced.Line($"if (p <= syncFrom) goto Fail;");
		atSynced.Line($"goto {Label(recovered)};");

		atAdvance.Line("p++;");
		atAdvance.Line($"goto {Label(scan)};");

		atRecovered.Line("global::System.Diagnostics.Debug.Assert(repeat >= 0 && repeat < entries.Count);");
		atRecovered.Line("var recoveryFrom = p;");
		atRecovered.Line("var recoveryReach = p;");
		atRecovered.Line("var recoveryTo = p;");
		atRecovered.Line("var recoveryBoundary = false;");
		atRecovered.Line("for (var recoveryAt = entries.Count - 1; recoveryAt > repeat; recoveryAt--)");
		using (atRecovered.Block(""))
		{
			atRecovered.Line("var candidate = entries[recoveryAt];");
			atRecovered.Line($"if (candidate.Kind == ParserEntry.PendingRecovery && candidate.State == {asked})");
			using (atRecovered.Block(""))
			{
				atRecovered.Line("recoveryFrom = candidate.Position;");
				atRecovered.Line("recoveryReach = candidate.AtomicIndex;");
			}
			atRecovered.Line($"if (!recoveryBoundary && candidate.Kind == ParserEntry.Choice && candidate.State == {advance})");
			using (atRecovered.Block(""))
			{
				atRecovered.Line("recoveryTo = candidate.Position;");
				atRecovered.Line("recoveryBoundary = true;");
			}
		}
		DeactivateChoices(atRecovered, "repeat");
		atRecovered.Line(
			$"entries.Add(new ParserEntry(ParserEntry.Recovery, {recovery.Id}, recoveryFrom, call, recoveryReach, " +
			"repeat, lookahead, recoveryTo, entries[repeat].Value));");
		atRecovered.Line("var recoveredRepeat = entries[repeat];");
		atRecovered.Line(
			"entries[repeat] = new ParserEntry(ParserEntry.Repeat, 0, recoveredRepeat.Position, " +
			"recoveredRepeat.CallIndex, recoveredRepeat.AtomicIndex, recoveredRepeat.RepeatIndex, " +
			"recoveredRepeat.LookaheadIndex, recoveredRepeat.Value + 1);");
		atRecovered.Line($"goto {Label(loop)};");

		DeactivateChoices(atAfter, "repeat");
		atAfter.Line("var acceptedRepeat = entries[repeat];");
		atAfter.Line(
			"entries[repeat] = new ParserEntry(ParserEntry.Repeat, 0, acceptedRepeat.Position, " +
			"acceptedRepeat.CallIndex, acceptedRepeat.AtomicIndex, acceptedRepeat.RepeatIndex, " +
			"acceptedRepeat.LookaheadIndex, acceptedRepeat.Value + 1);");
		atAfter.Line($"goto {Label(loop)};");

		LeaveRepeat(atExit, next);

		return entry;
	}

	static void DeactivateChoices(Writer writer, string from)
	{
		using (writer.Block($"for (var choiceAt = entries.Count - 1; choiceAt > {from}; choiceAt--)"))
		{
			writer.Line("var choice = entries[choiceAt];");
			writer.Line("if (choice.Kind != ParserEntry.Choice) continue;");
			writer.Line(
				"entries[choiceAt] = new ParserEntry(ParserEntry.Dead, choice.State, choice.Position, " +
				"choice.CallIndex, choice.AtomicIndex, choice.RepeatIndex, choice.LookaheadIndex, " +
				"choice.Value, choice.RuleIndex);");
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

				file.Line(
					$"{ValueInto(RecoveredType(plan), "recoveryAt")} = " +
					$"{plan.Method}({string.Join(", ", arguments)});");
			}

			file.Line("break;");
		}
	}

	static string RecoverySupplied(string name, RecoveryPlan plan) => name switch
	{
		"parserText"     => "text.Slice(recovered.Position, recovered.Value - recovered.Position).ToString()",
		"parserPosition" => "recovered.Position",
		"parserOrdinal"  => "recovered.RuleIndex",
		"parserLine"     => "LineAt(text, recovered.Position)",
		"parserColumn"   => "ColumnAt(text, recovered.Position)",
		"parserSpan"     => "new SourceSpan(recovered.Position, recovered.Value - recovered.Position)",
		"parserMessage"  => $"\"Input does not match '{Escape(plan.Element?.Name ?? "an element")}' at \" + " +
			"recovered.AtomicIndex.ToString(global::System.Globalization.CultureInfo.InvariantCulture) + \".\"",
		_                => "default",
	};

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
