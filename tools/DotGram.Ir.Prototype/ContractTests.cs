using System;

using DotGram.Grammar.Model;

using Xunit;

namespace DotGram.Ir.Prototype;

/// <summary>
/// What the execution tree promises, each promise a test: the evaluator reads the lowered
/// grammar, and the C# parser the generator wrote for the same grammar says what is right.
/// </summary>
public sealed class ContractTests
{
	/// <summary>
	/// The whole of it at once: for every fixture and input the evaluator and the generated C#
	/// parser give the same verdict, the same value, the same furthest refusal, and the same host
	/// calls in the same order.
	/// </summary>
	/// <remarks>
	/// One difference is the generated parser's own and is allowed for: an input it refuses it
	/// reads a second time, to say what was expected, and that reading asks the guards again
	/// (it builds nothing). So after a refusal its record is the evaluator's followed by guards only.
	/// </remarks>
	[Theory]
	[MemberData(nameof(Cases.Inputs), MemberType = typeof(Cases))]
	public void TheEvaluatorAnswersAsTheGeneratedParser(string name, string input)
	{
		var fixture = Cases.Fixtures[name];

		var (ours, ourLog, _) = Reading.Evaluated(fixture.Grammar, fixture.Entry, input, fixture.Guards);
		var (theirs, theirLog, _) = Reading.Generated(fixture.Parser, fixture.Entry, input);

		Assert.Equal(theirs.Line, ours.Line);

		if (ours.Accepted)
			Assert.Equal(theirLog, ourLog);
		else if (!fixture.Defers)
		{
			Assert.Equal(ourLog, theirLog.Take(ourLog.Count));
			Assert.All(theirLog.Skip(ourLog.Count), static call => Assert.StartsWith("g", call, StringComparison.Ordinal));
		}
	}

	/// <summary>
	/// The proofs that leave a choice or a repetition closed change what a reading costs and
	/// nothing else: with every one of them open the verdict, the value and the refusal are the same.
	/// </summary>
	[Theory]
	[MemberData(nameof(Cases.Inputs), MemberType = typeof(Cases))]
	public void ClosingADecisionChangesNoAnswer(string name, string input)
	{
		var fixture = Cases.Fixtures[name];

		var (proved, _, _) = Reading.Evaluated(fixture.Grammar, fixture.Entry, input, fixture.Guards);
		var (open, _, _)   = Reading.Evaluated(fixture.Grammar, fixture.Entry, input, fixture.Guards, options: new IrLowering.Options(AllResumable: true));

		Assert.Equal(proved.Line, open.Line);
	}

	/// <summary>
	/// Resumption. A call that has read is asked again when what follows it fails: "xy" is
	/// <c>Name</c> as "x" and then <c>'y'</c>, though <c>Name</c> answers "xy" first.
	/// </summary>
	/// <remarks>
	/// The tree has no suspended call. <c>Name</c>'s choice writes which alternative it took on
	/// the tape; the failure of <c>'y'</c> moves that decision on and the body of <c>Start</c>
	/// is run again from its first position, where <c>Name</c> reads the tape and takes "x".
	/// One decision, one more run.
	/// </remarks>
	[Fact]
	public void Resumption_ACallIsAskedAgain()
	{
		var (outcome, _, evaluator) = Reading.Evaluated(Grammars.Resumption, "ParseStart", "xy");

		Assert.True(outcome.Accepted);
		Assert.Equal(1, evaluator.Ways.Most);
		Assert.Equal(1, evaluator.Ways.Retries);

		var program = Reading.Program(Grammars.Resumption);

		Assert.True(program.Functions.Single(static function => function.Name == "Name").Decides);
		Assert.IsType<IrStmt.Replay>(program.Functions.Single(static function => function.Name == "Start").Body);

		// With recursion in Name, a decision per level and still one more run.
		(outcome, _, evaluator) = Reading.Evaluated(Grammars.ResumptionRecursive, "ParseStart", "xxxxxxxxy");

		Assert.True(outcome.Accepted);
		Assert.Equal(1, evaluator.Ways.Retries);
	}

	/// <summary>
	/// Commit. What braces hold is read once: the decisions made inside are off the tape when the
	/// braces have read, so nothing after them can ask for another reading — and until they have
	/// read, they are a replay region of their own.
	/// </summary>
	[Fact]
	public void Commit_TheFirstReadingIsTheOnlyOne()
	{
		// { Name } & 'y': Name is "xy" and stays "xy".
		var (outcome, _, evaluator) = Reading.Evaluated(Grammars.Commit, "ParseStart", "xy");

		Assert.False(outcome.Accepted);
		Assert.Equal(2, outcome.Position);
		Assert.Equal(0, evaluator.Ways.Retries);
		Assert.Equal(0, evaluator.Ways.Count);

		// { Name & 'y' }: inside the braces Name is still asked again.
		(outcome, _, evaluator) = Reading.Evaluated(Grammars.CommitInner, "ParseStart", "xy");

		Assert.True(outcome.Accepted);
		Assert.Equal(1, evaluator.Ways.Retries);
		Assert.Equal(0, evaluator.Ways.Count);

		// A function that only reaches a decision through braces writes nothing a caller can see.
		Assert.False(Reading.Program(Grammars.Commit).Functions.Single(static function => function.Name == "Start").Decides);
	}

	/// <summary>
	/// Rollback of captures. An alternative or a turn that fails leaves no capture behind: the
	/// slots it may have written are put back, by name, where it fails.
	/// </summary>
	/// <remarks>
	/// The resets are in the tree, one list an alternative, so a printer writes them out and
	/// nothing is undone by a mechanism the target would have to have. Lowered without them the
	/// same grammar hands the construction a capture from the alternative that failed.
	/// </remarks>
	[Fact]
	public void Rollback_AFailedAlternativeLeavesNoCapture()
	{
		// (a: 'x' & 'z' | b: 'x' & 'y'): on "xy" the first alternative captured a, then failed.
		var (outcome, _, _) = Reading.Evaluated(Grammars.CaptureRollbackChoice, "ParseStart", "xy");

		Assert.Equal("A 2 c0(null,\"x\")", outcome.Line);

		var (stale, _, _) = Reading.Evaluated(Grammars.CaptureRollbackChoice, "ParseStart", "xy", options: new IrLowering.Options(OmitResets: true));

		Assert.Equal("A 2 c0(\"x\",\"x\")", stale.Line);
	}

	/// <summary>
	/// Rollback of bindings. A text bound on a reading that is given back is bound again by the
	/// next one, and a reference after it reads the new text: in <c>$q: Name &amp; 'b'? &amp; '-' &amp; $q</c>
	/// "ab-a" is read with <c>$q</c> = "a" once "ab" has failed to come again after the dash.
	/// </summary>
	[Fact]
	public void Rollback_AReadingGivenBackTakesItsBindingWithIt()
	{
		var (outcome, _, evaluator) = Reading.Evaluated(Grammars.BindingRollback, "ParseStart", "ab-a");

		Assert.True(outcome.Accepted);
		Assert.Equal(1, evaluator.Ways.Retries);

		Assert.False(Reading.Evaluated(Grammars.BindingRollback, "ParseStart", "ab-b").Outcome.Accepted);

		// And a count by the bound text's length: five letters want five '=' at the least.
		Assert.True(Reading.Evaluated(Grammars.Fence, "ParseHeading", "Title\n=====\n").Outcome.Accepted);
		Assert.Equal(9, Reading.Evaluated(Grammars.Fence, "ParseHeading", "Title\n===\n").Outcome.Position);
	}

	/// <summary>
	/// Action timing. A construction runs when the reading reaches it, every time the reading
	/// reaches it: once for a reading that is then given back, and once more each time a replay
	/// passes it. It is not held until the input is accepted, and what it built for a reading
	/// given back is dropped, not undone.
	/// </summary>
	/// <remarks>
	/// On "xy": <c>Name</c> builds from "xy", <c>'y'</c> fails, the body is run again, <c>Name</c>
	/// builds from "x", and <c>Start</c> builds. Three calls for a value made of two. The generated
	/// C# parser, which builds while it reads here, makes the same three in the same order.
	/// </remarks>
	[Fact]
	public void Actions_RunWhenReachedAndAgainOnEveryReplay()
	{
		var (outcome, log, _) = Reading.Evaluated(Grammars.ActionTiming, "ParseStart", "xy");

		Assert.Equal("A 2 c0(c1(\"x\"))", outcome.Line);
		Assert.Equal(["c1(\"xy\")", "c1(\"x\")", "c0(c1(\"x\"))"], log);
		Assert.Equal(log, Reading.Generated(typeof(ActionTimingParser), "ParseStart", "xy").Log);

		// A repetition that gives a turn back runs the turns it keeps a second time.
		(_, log, _) = Reading.Evaluated(Grammars.Loop, "ParseStart", "ab");

		Assert.Equal(["c1(\"a\")", "c1(\"b\")", "c1(\"a\")", "c0([c1(\"a\")],\"b\")"], log);

		// Nothing is built inside a look, positive or negative, though what it calls is read.
		(_, log, _) = Reading.Evaluated(Grammars.Look, "ParseStart", "ab");

		Assert.Equal(["c2(\"a\")", "c2(\"b\")", "c0(c2(\"a\"),c2(\"b\"))"], log);
	}

	/// <summary>
	/// Guards. A guard is asked where the reading reaches it, over the values read so far, and
	/// its answer decides the reading: a no is a failure like any other, recorded as a refusal
	/// at the position it was asked at. It is asked again on every replay.
	/// </summary>
	[Fact]
	public void Guards_AreAskedOnEveryReadingAndDecideIt()
	{
		var guard = Cases.Fixtures["GuardTiming"].Guards;

		var (outcome, log, _) = Reading.Evaluated(Grammars.GuardTiming, "ParseStart", "xyy", guard);

		// Word reads "xyy", the guard says no, Word is asked again and reads "xy".
		Assert.Equal("A 3 c0(c2(\"xy\"))", outcome.Line);
		Assert.Equal(["g1(\"xyy\")=false", "g1(\"xy\")=true", "c2(\"xy\")", "c0(c2(\"xy\"))"], log);
	}

	/// <summary>
	/// Exceptions. What a construction throws leaves the reading at once and reaches the caller
	/// as it was thrown: nothing catches it, nothing is put back, and no other reading is tried —
	/// though here another reading would not have run that construction at all.
	/// </summary>
	/// <remarks>
	/// On "xy" the construction of <c>Name</c> over "xy" is the one of the reading that is given
	/// back; the accepted reading builds from "x". The exception still escapes, from the
	/// evaluator and from the generated parser alike, and both had made the same calls by then.
	/// </remarks>
	[Fact]
	public void Exceptions_FromAnActionLeaveTheReadingEvenIfItWouldBeGivenBack()
	{
		const string thrower = "c1(\"xy\")";

		var host      = new RecordingHost(throwOn: thrower);
		var evaluator = new IrEvaluator(Reading.Program(Grammars.ActionTiming), host);

		Assert.Equal(thrower, Assert.Throws<TwinException>(() => evaluator.Parse("ParseStart", "xy")).Message);
		Assert.Equal([thrower], host.Log);

		Assert.Equal(thrower, Assert.Throws<TwinException>(() => Reading.Generated(typeof(ActionTimingParser), "ParseStart", "xy", thrower)).Message);
	}

	/// <summary>
	/// Refusal recording. Every failed match says where and what it expected; what is kept is the
	/// furthest position and everything expected there, across alternatives and replays. A text
	/// is refused where the input first disagrees with it, not where it began. Nothing inside a
	/// look is recorded; a look that fails is recorded where it stands.
	/// </summary>
	[Fact]
	public void Refusals_KeepTheFurthestPositionAndWhatWasExpectedThere()
	{
		// "xz": "xy" disagrees at 1, and 'y' after "x" fails at 1.
		var outcome = Reading.Evaluated(Grammars.Resumption, "ParseStart", "xz").Outcome;

		Assert.Equal(1, outcome.Position);
		Assert.Equal(["'y'", "\"xy\""], outcome.Expected);

		var error = Reading.Generated(typeof(ResumptionParser), "ParseStart", "xz").Error;

		Assert.Contains("\"xy\"", error, StringComparison.Ordinal);
		Assert.Contains("'y'", error, StringComparison.Ordinal);

		// After the replay: 'y' matched, and the end of the input was expected where "z" stands.
		outcome = Reading.Evaluated(Grammars.Resumption, "ParseStart", "xyz").Outcome;

		Assert.Equal(2, outcome.Position);
		Assert.Equal(["'y'", "end of input"], outcome.Expected);

		// A negative look that fails is recorded at its own position, and what failed inside it is not.
		outcome = Reading.Evaluated(Grammars.Look, "ParseStart", "abc").Outcome;

		Assert.Equal(1, outcome.Position);
		Assert.Equal(["not Pair"], outcome.Expected);
	}

	/// <summary>
	/// Slot lifetime across replay. A slot holds nothing each time the scope that declares it is
	/// entered, and a replay enters the function's scope again: what a run captured is not there
	/// in the next run, whether or not the next run captures it.
	/// </summary>
	/// <remarks>
	/// <c>(a: 'x')? &amp; b: "xy"</c> on "xy": the first run takes the option and captures a,
	/// "xy" fails, the option's decision is moved on and the body is run again without it. The
	/// construction is handed nothing for a. No reset does this — the grammar is lowered here
	/// without any — only the scope.
	/// </remarks>
	[Fact]
	public void Slots_HoldNothingWhenTheirScopeIsEnteredAgain()
	{
		var (outcome, _, evaluator) = Reading.Evaluated(Grammars.CaptureRollback, "ParseStart", "xy", options: new IrLowering.Options(OmitResets: true));

		Assert.Equal("A 2 c0(null,\"xy\")", outcome.Line);
		Assert.Equal(1, evaluator.Ways.Retries);
	}

	/// <summary>
	/// Single assignment, in a loop. A slot of a position or a value is assigned once in one
	/// lifetime, and the evaluator refuses a second assignment. A loop breaks that in two ways,
	/// and the tree has an answer for each: what every turn writes and nothing after the loop
	/// reads is declared in a scope inside the turn; what a turn writes for something after the
	/// loop to read cannot be single-assignment at all, and is a slot of its own kind.
	/// </summary>
	[Fact]
	public void SingleAssignment_InALoopNeedsATurnScopeOrARebinding()
	{
		var program = new IrProgram();
		var letter  = new IrStmt.MatchSet([new CharRange('a', 'z')], false, 0);

		program.Expected.Add("letter");

		IrFunction Function(IrSlotKind kind, bool scoped)
		{
			var slot = new IrSlot(0, "at", kind, null);

			IrStmt turn = new IrStmt.Block([new IrStmt.Assign(slot, IrExpr.Position.Instance), letter]);

			if (scoped)
				turn = new IrStmt.Scope([slot], turn);

			return new IrFunction(0, "Loop")
			{
				Body      = new IrStmt.Scope(scoped ? [] : [slot], new IrStmt.Repeat(turn, new IrExpr.Number(0), null, [], false)),
				SlotCount = 1,
			};
		}

		bool Reads(IrFunction function)
		{
			program.Entries["Parse"] = function;

			return new IrEvaluator(program, new RecordingHost()).Parse("Parse", "ab").Accepted;
		}

		// Declared once for the function and assigned by every turn: the second turn is refused.
		Assert.Throws<IrContractViolation>(() => Reads(Function(IrSlotKind.Position, scoped: false)));

		// Declared in the turn: a new lifetime each turn.
		Assert.True(Reads(Function(IrSlotKind.Position, scoped: true)));

		// Read after the loop, so it cannot be the turn's: a slot that says it is written again.
		Assert.True(Reads(Function(IrSlotKind.Rebound, scoped: false)));

		// Which is what a binding inside a repetition is lowered to, and only that.
		var rebound = Reading.Program(Grammars.Rebinding).Functions.Single(static function => function.Name == "Start");
		var slots   = ((IrStmt.Scope)rebound.Body).Slots;

		Assert.All(slots, static slot => Assert.Equal(IrSlotKind.Rebound, slot.Kind));
		Assert.True(Reading.Evaluated(Grammars.Rebinding, "ParseStart", "aabb").Outcome.Accepted);

		// A capture inside a repetition is neither: it is a sequence, added to each turn.
		var looped = Reading.Program(Grammars.Loop).Functions.Single(static function => function.Name == "Start");

		Assert.Equal(IrSlotKind.List, ((IrStmt.Scope)((IrStmt.Replay)looped.Body).Body).Slots.Single(static slot => slot.Name == "items").Kind);
	}

	/// <summary>
	/// Single assignment, in a resumed invocation. A replay runs the body again in the same
	/// invocation, so a slot assigned before the decision that is moved on is assigned again:
	/// single assignment holds only because the replay region encloses the scope, never the
	/// other way round. With the scope outside the region the second run is refused.
	/// </summary>
	[Fact]
	public void SingleAssignment_UnderReplayNeedsTheScopeInsideTheRegion()
	{
		var program = new IrProgram();

		program.Expected.AddRange(["\"xy\"", "'x'", "'y'", "end of input"]);

		IrFunction Function(bool scopeInside)
		{
			var slot = new IrSlot(0, "at", IrSlotKind.Position, null);

			// at = p; ("xy" | 'x') resumable; 'y'; end
			IrStmt body = new IrStmt.Block(
			[
				new IrStmt.Assign(slot, IrExpr.Position.Instance),
				new IrStmt.Choice([new IrStmt.MatchText("xy", false, 0), new IrStmt.MatchText("x", false, 1)], [[], []], Resumable: true),
				new IrStmt.MatchText("y", false, 2),
				new IrStmt.MatchEnd(3),
			]);

			return new IrFunction(0, "Start")
			{
				Body = scopeInside
					? new IrStmt.Replay(new IrStmt.Scope([slot], body))
					: new IrStmt.Scope([slot], new IrStmt.Replay(body)),
				SlotCount = 1,
			};
		}

		bool Reads(IrFunction function)
		{
			program.Entries["Parse"] = function;

			return new IrEvaluator(program, new RecordingHost()).Parse("Parse", "xy").Accepted;
		}

		Assert.True(Reads(Function(scopeInside: true)));
		Assert.Throws<IrContractViolation>(() => Reads(Function(scopeInside: false)));

		// Every function the lowering writes that can be replayed has it the right way round.
		foreach (var fixture in Cases.Fixtures.Values)
			foreach (var function in Reading.Program(fixture.Grammar).Functions)
				if (function.Body is IrStmt.Replay replay)
					Assert.IsType<IrStmt.Scope>(replay.Body);
	}

	/// <summary>
	/// A sequence capture that no turn ever added to is an empty sequence, not nothing — also
	/// where the repetition that would have filled it stands in an option that was not taken.
	/// </summary>
	[Fact]
	public void Sequences_ThatWereNeverReachedAreEmpty()
	{
		Assert.Equal("A 2 c0(c4(c8(c9(null,[]))))", Reading.Evaluated(Grammars.Json, "ParseJson", "{}").Outcome.Line);
	}

	/// <summary>
	/// What a replay costs is counted, not timed: the statements run for a nested input grow
	/// with its length and not faster, where the grammar's decisions are all closed.
	/// </summary>
	[Fact]
	public void Json_IsReadWithoutADecisionOnTheTape()
	{
		var program = Reading.Program(Grammars.Json);

		Assert.DoesNotContain(program.Functions, static function => function.Decides);

		long Steps(int size)
		{
			var input = "[" + string.Join(",", Enumerable.Repeat("{\"a\":[1,2,3]}", size)) + "]";
			var (outcome, _, evaluator) = Reading.Evaluated(Grammars.Json, "ParseJson", input);

			Assert.True(outcome.Accepted);

			return evaluator.Steps;
		}

		var small = Steps(100);
		var large = Steps(200);

		Assert.InRange(large, small, small * 2 + 100);
	}
}
