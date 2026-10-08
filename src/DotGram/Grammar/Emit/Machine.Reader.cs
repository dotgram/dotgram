using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Emit;

/// <summary>
/// The reader: a grammar as the methods a person would have written.
/// </summary>
/// <remarks>
/// <para>
/// <b>Why statements rather than regions.</b> A rendering by methods grown out of the
/// automaton keeps its vocabulary: a rule is one method, but inside it every construct is a
/// labelled region and every failure is a jump. That shape is right for one machine of a
/// thousand states, which is a graph and nothing else; for a method it is a graph nobody
/// asked for, and taking its dead jumps, labels, marks and locals back out means asking a
/// question about one construct of the whole method.
/// </para>
/// <para>
/// Here a construct is a statement. A sequence is statements one after another; a failure
/// is <c>return -1</c>; a repetition is a <c>while</c>; a choice is a switch on what the
/// alternatives begin with, or one attempt after another where the first token does not
/// divide them. An alternative that can fail halfway and has a sibling after it becomes a
/// method of its own, because that is how a caller learns it failed without a jump — and
/// where the choice dispatches, no alternative needs one, since failing the alternative the
/// token chose is failing the choice.
/// </para>
/// <para>
/// <b>What it reads.</b> Values, handed to the carrier (<c>Machine.Direct.Values.cs</c>
/// and the carriers beside it); guards, marks and folds; a rule written with binding powers,
/// entered at a strength (§4.3.1); and the tape of ways back that reading characters needs
/// — over kinds only a rule marked <c>?</c> keeps one, since everywhere else a rule's answer
/// stands (§4). What it does not read the engine keeps: <see cref="CanDirect"/> is the gate,
/// refusing a stream, a <c>find</c>, a recovery, a captured lookahead, a call with
/// arguments, an external recognizer that keeps a value and a guard it cannot hand what the
/// guard names, and it says why (<see cref="Refusal"/>) rather than guesses.
/// </para>
/// </remarks>
sealed partial class Machine
{
	/// <summary>The refusal recorder the emitted readers call.</summary>
	const string Refusing = "Refuse_DotGram";

	/// <summary>The seams of the grammar and every rule they reach: read, by the engine, without recording what they refuse.</summary>
	internal HashSet<RuleSymbol> SeamReached => _seamReached ??= ReachedFromSeams();

	HashSet<RuleSymbol>? _seamReached;

	/// <summary>Whether a node stands in the body of a rule the seam reaches.</summary>
	bool InSeam(Node node)
	{
		return (_seamNodes ??= NodeWalk.ByIdentity(SeamReached.Where(_graph.Bodies.ContainsKey).SelectMany(rule => NodeWalk.Descendants(_graph.Bodies[rule])))).Contains(node);
	}

	HashSet<Node>? _seamNodes;

	HashSet<RuleSymbol> ReachedFromSeams()
	{
		var reached = new HashSet<RuleSymbol>();
		var pending = new Stack<RuleSymbol>();

		foreach (var trivia in _graph.Trivia.Values)
			if (trivia is Node.Call(var seam, _) && reached.Add(seam))
				pending.Push(seam);

		while (pending.Count > 0)
			if (_graph.Bodies.TryGetValue(pending.Pop(), out var body))
				foreach (var node in NodeWalk.Descendants(body))
					if (node is Node.Call(var called, _) && reached.Add(called))
						pending.Push(called);

		return reached;
	}

	bool _readerWays = true;

	/// <summary>
	/// Whether a rule is a token or a few tokens and nothing else — no rule under it, no
	/// repetition, no guard, no look — so that its reader is what a hand-written parser would
	/// write inline where it is called.
	/// </summary>
	bool Trivial(RuleSymbol rule)
	{
		var leaves = 0;

		foreach (var node in NodeWalk.Descendants(_graph.Bodies[rule]))
		{
			switch (node)
			{
				case Node.Call:
				case Node.Repeat:
				case Node.Guard:
				case Node.Lookahead:
				case Node.Behind:
				case Node.Reading:
				case Node.External:
					return false;

				case Node.Literal:
				case Node.Element:
					leaves++;
					break;
			}
		}

		return leaves > 0 && leaves <= 8;
	}

	/// <summary>
	/// The rules a way back has to be written into: the ones that put something on the
	/// tape, and the ones that call them.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A way back is a loop that calls the body again after moving the tape on, and it can
	/// only do something where there is something on the tape to move. A rule that opens no
	/// way, and calls nothing that opens one, has one reading and is written as one method.
	/// </para>
	/// <para>
	/// A caller is included even though the callee has a way back of its own: the callee
	/// answered and the caller failed after it, and asking the callee for its next answer
	/// is the caller running again from the top with the tape moved on.
	/// </para>
	/// </remarks>
	/// <summary>
	/// The rules that put something on the tape, once it is known — null while the first
	/// pass is still finding out, which is read as "assume any of them might".
	/// </summary>
	HashSet<RuleSymbol>? _opens;

	HashSet<RuleSymbol> Opens(IReadOnlyList<RuleSymbol> rules, HashSet<RuleSymbol> opens)
	{
		// A rule that calls one is one, which takes as many passes as the call chain is
		// deep and settles because nothing is ever taken out.
		//
		// Not over kinds, where a rule's answer stands (§4). There a rule that gives back
		// does so inside itself: the loop around it asks its body again until it answers or
		// until the tape has nothing left to move on, and seals what it opened the moment it
		// does answer. So by the time such a rule has returned there is nothing under it for
		// a caller to retry — it was sealed or it was exhausted — and a caller that opens no
		// way of its own needs no way back into it. The whole of an expression grammar's
		// ladder was written twice over for one repetition at the bottom of it.
		for (var again = !OverKinds; again; )
		{
			again = false;

			foreach (var rule in rules)
				if (!opens.Contains(rule) &&
					NodeWalk.Descendants(_graph.Bodies[rule])
						.Any(one => one is Node.Call(var called, _) && opens.Contains(called)))
				{
					opens.Add(rule);
					again = true;
				}
		}

		return opens;
	}

	/// <summary>
	/// Whether anything under a node can put something on the tape: a rule it calls that
	/// does, once the first pass has said which those are.
	/// </summary>
	bool Opens(Node node)
	{
		return _opens is null ||
		NodeWalk.Descendants(node).Any(one => one is Node.Call(var called, _) && _opens.Contains(called));
	}

	/// <summary>
	/// What may follow a rule where it is called, which is what says whether a run inside
	/// it can ever be asked for a shorter reading.
	/// </summary>
	FollowSets.Continuation FollowOf(RuleSymbol rule)
	{
		_follows ??= FollowSets.Of(_graph);

		return _follows.TryGetValue(rule, out var following)
			? following
			: FollowSets.Continuation.All;
	}

	/// <summary>
	/// How many methods the rule being written has been cut into.
	/// </summary>
	/// <remarks>
	/// One counter for the rule and not one per writer. A part may extract a part of its
	/// own — a choice inside an alternative, a repetition inside one — and a counter
	/// belonging to the writer numbered that one from zero again, giving two methods one
	/// name.
	/// </remarks>
	int _readerPart;

	/// <summary>
	/// Whether every rule method counts its entries, for a test that asks how often a rule was
	/// entered rather than how long a reading took.
	/// </summary>
	/// <remarks>
	/// Set only where the compilation defines <c>DOTGRAM_COUNTS</c> (<c>GramGenerator</c>), and
	/// written inside an <c>#if</c> of that symbol all the same. A field a rule is bytes in every
	/// parser, where the counters every parser carries are a handful a machine, so a parser built
	/// without the symbol is written exactly as it was.
	/// </remarks>
	internal bool CountsRules;

	/// <summary>
	/// What the file's trace build numbers its events by, where it is one: then every rule's method
	/// reports its entry and each of its returns, a remembered failure, a guard asked, an element
	/// stepped over and a hand-off to another stack, and the frames of the forwarding rules the
	/// graph collapsed (GramCompilerOptions.Trace). Null writes none of it.
	/// </summary>
	internal TraceTables? Tracing;

	/// <summary>The counter of entries to a rule's method (<see cref="CountsRules"/>).</summary>
	static string EnteredOf(RuleSymbol rule)
	{
		return "CountEntered_" + CSharpEmitter.IdentifierOf(rule);
	}

	/// <summary>The publications the readers were settled for (<see cref="Settle"/>), or null before they were.</summary>
	IReadOnlyList<Publication>? _settledFor;

	/// <summary>
	/// What the readers need to know before any of them is written, found once: the rules they
	/// reach, the back edges, what the guards need, the arms, which rules open a way back — and
	/// from those, what the carrier's gates say (<see cref="AskGates"/>) and which rules remember
	/// where they failed (<see cref="ChooseMemo"/>).
	/// </summary>
	/// <remarks>
	/// Apart from <see cref="RenderReader"/>, which asks it first. Asked again for the same
	/// publications, it does nothing; asked for others, it refuses, since what it found would be
	/// another machine's.
	/// </remarks>
	internal void Settle(IReadOnlyList<Publication> publications)
	{
		if (_settledFor is not null)
		{
			if (!_settledFor.SequenceEqual(publications))
				throw new InvalidOperationException("The readers were settled for other publications.");

			return;
		}

		_settledFor = publications;

		var rules = DirectRules(publications);

		BackEdges(publications);
		DirectGuardNeeds(rules);
		DirectArms(rules);

		_directRules = rules;

		// Observe which rules open a way without retaining provisional source. Keep the
		// traversal: it also creates helpers and gathers state needed before carrier selection.
		var opens = new HashSet<RuleSymbol>();

		foreach (var rule in rules)
		{
			_seam       = FollowSets.SeamOf(rule, _graph);
			_readerPart = 0;

			var reader = new ReaderWriter(this, rule, analyzing: true);
			reader.Render(_graph.Bodies[rule], FollowOf(rule));

			if (reader.ObservedOpen)
				opens.Add(rule);
		}

		if (Reporting)
			OpenedHere = [.. opens];

		// Opens adds the callers to the set it is handed; the gates want those that opened one
		// themselves as well.
		var own = new HashSet<RuleSymbol>(opens);

		_opens = Opens(rules, opens);

		// And the gates are asked now, knowing which rules open a way: what they say decides no
		// carrier, and is said of the grammar (GateReasons).
		AskGates(rules, _opens, own);

		// Which rules remember where they failed, knowing which of them give back (Machine.Memo.cs).
		ChooseMemo(rules);

		// The first pass asked the carrier about its calls, and the carrier asked demand — before the
		// rules that give back were known, so without them (GivingBack). Forgotten here, on both
		// sides, so that the readers are written against the report that has them.
		_demands = null;
		(Carrier as ImmediateCarrier)?.Resettled();
	}

	/// <summary>Every rule of a reading, each as a method, with the entries above them.</summary>
	public string RenderReader(IReadOnlyList<Publication> publications)
	{
		Settle(publications);

		if (_opens is not { } opens)
			throw new InvalidOperationException("The readers were not settled.");

		// What the loops decide is noted as they are written, and written once: a rendering asked
		// for again says it again from the start.
		RetryLoops.Clear();

		var file  = new Writer(0);
		var rules = _directRules;
		var seen  = new HashSet<RuleSymbol>();

		// Render again with the selected carrier and known open rules. Append each rule
		// and its parts immediately so completed method strings need not all stay alive.
		var entries = new Writer(0);
		var entryPoints = new List<(RuleSymbol Rule, bool Ends)>();

		using (file.Indent())
		{
			foreach (var rule in rules)
			{
				_seam       = FollowSets.SeamOf(rule, _graph);
				_readerPart = 0;

				var reader = new ReaderWriter(this, rule);
				var tape   = opens.Contains(rule);
				var inner  = tape ? ReaderOf(rule) + "_Body" : ReaderOf(rule);

				if (tape)
					RenderWayBack(
						file, rule, DirectStrength(rule), seal: OverKinds,
						deepens: Deepens(rule) ? rule : null);

				file.Line(
					tape
						? $"/// <summary>What <c>{rule.Name}</c> is, one reading of it at a time.</summary>"
						: $"/// <summary><c>{rule.Name}</c>, read by a method of its own.</summary>");

				// A rule that is a token or a choice of tokens is what a hand-written parser
				// writes as a test where it stands; asked for as a call, it costs the call. The
				// JIT inlines a method this small on its own only while the refusals inside it
				// keep it under its budget, and a choice of six tokens with six of them does not.
				//
				// A body is asked for from one place and one only — the way back into it, written
				// just above — so the same thing is said of every one of them, whatever its size.
				// Nothing is written twice by it: a method the runtime never calls is a method it
				// never compiles, so what the reader has is one method a rule where it had two,
				// which is the shape a person writing the parser would have written to begin with.
				// Worth a quarter of a character parse over the notation grammar, and it is halved
				// calls rather than anything cleverer: `Name` reaches `Identifier` reaches a class,
				// with a seam between each, and every one of those was two calls where it needed
				// one.
				if (tape || Trivial(rule))
					file.Line(
						"[global::System.Runtime.CompilerServices.MethodImpl(" +
						"global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]");

				// The element of a repetition marked `recover` the reader reads is read once a turn
				// from the loop, and the turn is all the loop does: left to itself the JIT folds the
				// element's whole chain into the loop and the loop into the entry (FIX's text form:
				// the hot chain from 13.4 to 20.9 KB, +4..+9% a field), where a method of its own was
				// the boundary a way back used to draw. A person writing the parser would call it.
				else if (_recoveryReads.Values.Any(read => ReferenceEquals(read.Plan.Element, rule)))
					file.Line(
						"[global::System.Runtime.CompilerServices.MethodImpl(" +
						"global::System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]");

				// A remembered rule's body is written whole first: every failure in it is sent through
				// the one place that remembers it, and one that never fails has nothing to remember.
				// A trace build's body sends them there itself, leaving the rule as it goes.
				var slot = !tape && _memo.TryGetValue(rule, out var bit) ? bit : -1;

				reader.Memoised = slot >= 0 && Tracing is not null;

				var body = slot >= 0 ? reader.Render(_graph.Bodies[rule], FollowOf(rule)) : null;

				if (body is not null && !body.Contains(Tracing is null ? "return -1;" : "goto Failed;", StringComparison.Ordinal))
					slot = -1;

				using (file.Block($"public int {inner}(int pos{DirectStrength(rule)})"))
				{
					if (slot >= 0)
						MemoAsked(file, rule, slot);

					// The rule the way back into itself goes through, so the probe stands here and
					// not at the call: one line a rule instead of one at every place that calls it.
					if (!tape && Deepens(rule))
						Probe(file, rule);

					// After the probe: a reading carried onto a stack of its own enters the rule
					// again there, and that is the one entry.
					if (CountsRules)
					{
						file.Line("#if DOTGRAM_COUNTS");
						file.Line($"{EnteredOf(rule)}++;");
						file.Line("#endif");
						file.Line();
					}

					// Entered once it is read, here: a rule remembered as failing is not, and one
					// carried onto another stack is entered there.
					if (Tracing is not null)
					{
						file.Line($"failure.Trace?.Enter({Tracing.RuleOf(rule)}, pos);");
						file.Line();
					}

					if (body is null)
						reader.Render(file, _graph.Bodies[rule], FollowOf(rule));
					else if (slot < 0 || !MemoBody(file, rule, slot, body))
						file.Write(body);
				}

				file.Line();

				foreach (var (name, taken, part) in reader.Parts)
				{
					// A part only a failure reaches is kept out of the method that calls it, so
					// that the hot one stays small (EmitRecovering).
					if (reader.Cold.Contains(name))
					{
						file.Line($"/// <summary>What <c>{rule.Name}</c> does with an element it could not read: steps over it to the next synchronization.</summary>");
						file.Line("[global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]");
					}
					else
						file.Line($"/// <summary>One alternative of <c>{rule.Name}</c>, read where it stood.</summary>");

					using (file.Block($"public int {name}(int pos{taken})"))
					{
						file.Write(part);
					}

					file.Line();
				}
			}

			foreach (var publication in publications)
			{
				if (!seen.Add(publication.Rule))
					continue;

				// A yield's step is read from where the driver stands and ends where the element
				// does: the positional entry, and not one that asks for the end.
				var ends = publication.Kind != PublishKind.Yield;

				entryPoints.Add((publication.Rule, ends));
				RenderReaderEntryBody(file, publication.Rule, ends);

				// And the entry that begins where it is told and demands no end, which is what a
				// positional overload calls. Only for a whole parse: `find` already reads from a
				// position, and asking for both would write one method twice.
				if (publication.Kind == PublishKind.Parse)
				{
					entryPoints.Add((publication.Rule, false));
					RenderReaderEntryBody(file, publication.Rule, ends: false);
				}
			}

			// Include entry trivia under both whole-input and positional continuations.
			// A body or wrapper can use replay state even without opening a new way.
			_readerWays = Carrier is not ImmediateCarrier || Memoises ||
				file.ToString().Contains("ways.", StringComparison.Ordinal);
		}

		foreach (var (rule, ends) in entryPoints)
			RenderReaderEntry(entries, rule, ends);

		RenderReaderStruct(file);
		file.Write(entries.ToString());

		if (rules.Any(Valued))
		{
			if (UsesLocating)
			{
				PerCallBuilder = Carrier.RenderBuilder(rules);
			}
			else
			{
				file.Write(Carrier.RenderBuilder(rules));
				file.Line();
			}
		}

		return file.ToString();
	}

	/// <summary>
	/// The way back into a rule: the tape, as a loop.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Over characters a rule's answer does not stand. A repetition that swallowed a
	/// character something after it needed has to hand it back, and an alternative that
	/// matched has to give way to the next when what follows cannot be read — which is
	/// asking a rule that has already answered for its next answer.
	/// </para>
	/// <para>
	/// The rendering this replaces wrote that as a label at the top of the rule and a jump
	/// to it from the bottom. Here the rule is two methods: what it is, and the way back
	/// into it. The body is called until it answers or until the tape has nothing left to
	/// move on, and the tape is what makes a second call different from the first — every
	/// decision the body took is recorded on it, and a replay reads them back rather than
	/// taking them again (<c>Support.cs</c>, <c>Ways.Retry</c>).
	/// </para>
	/// <para>
	/// One segment for the rule and not one per construct, which the rendering beside this
	/// one has. <c>Retry</c> takes the latest way with an alternative left wherever it
	/// stands, so a segment per rule reaches every way opened inside it; what the finer
	/// segments buy is running less of the rule again, and that is a measurement to make
	/// rather than a thing to assume.
	/// </para>
	/// </remarks>
	void RenderWayBack(
		Writer file, RuleSymbol rule, string strength, bool seal = false, RuleSymbol? deepens = null)
	{
		RenderWayBack(
			file, rule, ReaderOf(rule), $"/// <summary><c>{rule.Name}</c>, and the way back into it.</summary>",
			strength, seal, deepens);
	}

	/// <summary>Whether a way back into this rule can reach it again, and so deepen the stack.</summary>
	bool Deepens(RuleSymbol rule)
	{
		foreach (var (_, called) in _backEdges)
			if (ReferenceEquals(called, rule))
				return true;

		return false;
	}

	/// <summary>
	/// The stack, probed once in every <see cref="Interval"/> entries to a rule that can
	/// reach itself.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A grammar that recurses on input the author did not write can be handed a thousand
	/// brackets, so the probe has to be here. How often is arithmetic, and it has been got wrong
	/// twice. It was 64, on the reasoning that "the runtime reserves far more than sixty-four
	/// frames of a reader", which was never measured and is false: the whole slow suite died of
	/// it on every run and was reported as a pass. It was then going to be 16, which is 86 KiB of
	/// levels against a 114.4 KiB budget of which the hand-off needs 27.8, and crashes
	/// deterministically (D141, 2026-09-24).
	/// </para>
	/// <para>
	/// <b>The condition has four terms, and the first three were each wrong once:</b>
	/// </para>
	/// <code>
	///     interval x bytes-a-level  +  the hand-off's own stack  &lt;=  the budget
	/// </code>
	/// <list type="bullet">
	/// <item><description>
	/// <b>The budget is 114.4 KiB</b> for a reader, measured by spending it and bisecting on
	/// survival. It is not the 128 KiB <c>TryEnsureSufficientExecutionStack</c> asks for, which
	/// counts the operating system's 12 KiB guard page, and not the 88 to 92 KiB of reserve
	/// below the pointer, which leaves out committed pages that are still free. <b>Nor is it a
	/// constant of the runtime: it depends on how the stack is spent.</b> The same room taken in
	/// one <c>stackalloc</c> instead of in recursion is 97.4 KiB, because Windows commits ahead
	/// of a growing stack and moves the guard as it goes. A reader recurses, so 114.4 is the
	/// figure that applies — and it may not be carried to anything that does not.
	/// </description></item>
	/// <item><description>
	/// <b>The worst level we have measured is 5.40 KiB</b>, SQL:2023 in a DEBUG build at tier-0.
	/// <b>Taken before bafdcf00</b>, which made the towers' four value types classes and took a
	/// quarter off the level: Release is 3.36 KiB now rather than 4.50, and Debug 4.15 rather
	/// than 5.40. The arithmetic below is left on the older, wider figures, so it reserves
	/// against a level the parser no longer has and the real margin is better than it says.
	/// <b>Do not re-derive from 5.40 or 4.50</b>: if this is ever re-taken, take it afresh.
	/// Release is 4.50 and tier-1 is 2.38; neither is the number to use, because a consumer
	/// debugs their application and a first call runs at tier-0.
	/// </description></item>
	/// <item><description>
	/// <b>The hand-off costs 27.8 KiB on the first call in a process</b> and 5.1 KiB on every
	/// call after, measured on the emitted <c>Deepen</c> by ballast at its entry, bisected on
	/// survival, at three ballast widths so the burner's own overhead is solved for rather than
	/// assumed. A bare <c>new Thread</c> with Start and Join is only 2.7 KiB, so nearly all of
	/// the cold figure is compiling the hand-off path, on the leaving thread at its deepest
	/// point. <b>The cold one is the one a budget carries</b>: a process's first deep input is
	/// exactly when it is paid.
	/// </description></item>
	/// </list>
	/// <para>
	/// Four levels is 21.6 KiB, so 21.6 + 27.8 = <b>49.4 against 114.4, a margin of 2.3x</b>.
	/// Sixteen levels is 86.4 + 27.8 = 114.2, which is the budget to within a rounding — and 16
	/// does die, deterministically, which is the arithmetic and the crash agreeing.
	/// </para>
	/// <para>
	/// <b>This covers the grammars we have measured and not every grammar, and that is a cost
	/// chosen here rather than a fact about the world.</b> Bytes-a-level is a property of the
	/// code the JIT makes from what this emitter writes: it varies five-fold between our own two
	/// SQL dialects and two to three times between the tiers of one of them, and nothing here can
	/// compute it for a grammar nobody has run. What says so before a consumer finds out is
	/// <c>StackFrameBudgetTests</c>, which measures the level of each of our grammars and holds
	/// it to this same arithmetic. If that test fails, this number is what has to move.
	/// </para>
	/// <para>
	/// The counter is carried onto the new stack, so the phase travels with the reading rather
	/// than restarting. That is why survival is not a function of depth — at 16, depth 200 on a
	/// 512 KiB stack died while 150 and 300 lived — and why <c>StackDepthTests</c> sweeps depths
	/// instead of choosing one.
	/// </para>
	/// <para>
	/// <b>It is not carried back.</b> The reading that handed over is still on the stack that ran
	/// low, and what it reads next may be the recursion itself. T-SQL's <c>CASE … ELSE CASE …</c>
	/// enters a probing rule four times a level, so a probe once in four entries fell on the same
	/// entries every level: on the condition and the <c>THEN</c>, which were carried off and came
	/// back, never on the <c>ELSE</c> that holds the next level, which went on down the low stack
	/// unprobed until it overflowed. So the hand-off comes back with the counter at zero, and the
	/// next entry on that stack probes; one that finds it still low hands over and comes back at
	/// zero again, so below the margin every entry probes, and above it the interval resumes.
	/// </para>
	/// </remarks>
	/// <summary>Entries between two probes of the stack. A power of two: the test is a mask.</summary>
	internal const int Interval = 4;

	void Probe(Writer file, RuleSymbol rule)
	{
		var power = _graph.Climbing.ContainsKey(rule) ? ", power" : ", 0";

		file.Line($"if ((probes++ & {Interval - 1}) == 0 && !EnoughStack_DotGram{_tag}())");
		file.Then($"return Deepen_DotGram{_tag}(pos, {DeepOf(rule)}{power});");
		file.Line();
	}

	/// <summary>Which of the rules that probe the stack this one is, for the switch that resumes it.</summary>
	int DeepOf(RuleSymbol rule)
	{
		if (!_deep.TryGetValue(rule, out var which))
			_deep[rule] = which = _deep.Count;

		return which;
	}

	readonly Dictionary<RuleSymbol, int> _deep = [];

	bool? _probes;

	/// <param name="seal">
	/// Whether the ways the body opened are sealed once it has answered: a rule marked
	/// <c>?</c> over kinds gives back inside itself, and once it has answered the answer
	/// stands. Sealed rather than dropped, so that a replay reads the same decisions.
	/// </param>
	/// <param name="owner">The rule whose reading the loop asks again: the rule itself, or the one an entry reads.</param>
	/// <param name="entry">Whether the loop is an entry's, around its reading of the rule, rather than the rule's own.</param>
	void RenderWayBack(
		Writer file, RuleSymbol owner, string name, string summary, string strength, bool seal = false,
		RuleSymbol? deepens = null, bool entry = false)
	{
		var defers = Deferral(owner, entry ? RetrySite.Entry : RetrySite.Rule, noting: true).Defers && Tracing is null;

		file.Line(summary);

		using (file.Block($"public int {name}(int pos{strength})"))
		{
			if (deepens is not null)
				Probe(file, deepens);

			file.Line("var s  = ways.Cursor;");
			if (defers)
				file.Line(DeclareDeferral(""));
			foreach (var line in Carrier.MarkRecords("lm"))
				file.Line(line);
			foreach (var line in Carrier.MarkGathered(null, "rb"))
				file.Line(line);
			file.Line();

			var restore = new List<string>(Carrier.UnwindRecords("lm"));

			restore.AddRange(Carrier.UnwindGathered(null, "rb"));

			using (file.Block("while (true)"))
			{
				file.Line(
					$"var q = {name}_Body(pos{(strength.Length > 0 ? ", power" : "")});");
				file.Line();
				if (defers)
				{
					Stood(file, "q", "", restore, seal ? ["ways.Seal(s);", "", "return q;"] : ["return q;"]);
				}
				else if (seal)
				{
					using (file.Block("if (q >= 0)"))
					{
						file.Line("ways.Seal(s);");
						file.Line();
						file.Line("return q;");
					}
				}
				else
				{
					file.Line("if (q >= 0)");
					file.Then("return q;");
				}

				file.Line();
				foreach (var line in restore)
					file.Line(line);

				// A loop that defers is new text, written without the empty line the others keep
				// where there is nothing to put back; those are written as they always were.
				if (!defers || restore.Count > 0)
					file.Line();

				if (defers)
				{
					using (file.Block("if (ways.Cursor > s && ways.Retry(s))"))
						Retried(file, "");

					file.Line();
					Undeferred(file, "");
				}
				else
				{
					file.Line("if (ways.Cursor > s && ways.Retry(s))");
					file.Then("continue;");
				}

				file.Line();
				file.Line("return -1;");
			}
		}

		file.Line();
	}

	/// <summary>
	/// The local of a loop that asks for another reading, saying where its attempts stand since a
	/// way was given back (<see cref="DefersOnGiveBack"/>): 0 building, as every loop begins; 1
	/// building still, one way given back; 2 reading unbuilt, the count raised by this loop; 3
	/// reading the attempt that stood once more, with the failure kept to be put back
	/// (<c>ImmediateCarrier.ReplayMethods</c>). Declared beside the loop's cursor mark, <c>s</c>
	/// with the same suffix, so that the two are read together.
	/// </summary>
	/// <remarks>
	/// The first way given back is read building, as it always was, and only the second raises the
	/// count: one give-back is how a choice reads — an alternative that opened a way fails, the way
	/// is flipped, the next alternative stands — and an accepted address list gives one back for
	/// every member it reads. Deferring there would read every such member twice. What grows with
	/// the input is the give-back that is given back again, and that is where the count is raised:
	/// a refused list builds its first two attempts and no more, twice the first and not its
	/// square.
	/// </remarks>
	static string DeclareDeferral(string suffix)
	{
		return $"var u{suffix}  = 0;";
	}

	/// <summary>
	/// What a loop does with an attempt that reached somewhere: answers with it — or, where the
	/// attempts since the second give-back were read unbuilt, puts back what a failed attempt
	/// puts back, sets the cursor to the loop's own mark and reads once more with the count at
	/// zero (<c>Stood_DotGram</c>). The ways from the mark on hold exactly what the attempt that
	/// stood decided, so the reading replays them, fails nowhere, and builds what the attempt only
	/// recognized. Through the same call as every attempt, so that the body is inlined into its
	/// loop once; and beside the attempt only one compare of the local, the rest being a call the
	/// hot path never makes.
	/// </summary>
	/// <param name="restore">
	/// What the failure branch of the same loop puts back before it flips a way: the records to
	/// their mark, the gathered members to theirs, the positions a part wrote. The one list for
	/// both, so that the replay cannot put back less than a retry does.
	/// </param>
	/// <param name="answer">What ends the loop with the attempt: a return, a break; a seal before either where the loop seals.</param>
	static void Stood(Writer code, string took, string suffix, IReadOnlyList<string> restore, IReadOnlyList<string> answer)
	{
		using (code.Block($"if ({took} >= 0)"))
		{
			using (code.Block($"if (u{suffix} > 1)"))
			{
				code.Line($"u{suffix} = Stood_DotGram(u{suffix}, s{suffix});");
				code.Line();

				if (restore.Count == 0)
				{
					code.Line($"if (u{suffix} > 0)");
					code.Then("continue;");
				}
				else
				{
					using (code.Block($"if (u{suffix} > 0)"))
					{
						foreach (var line in restore)
							code.Line(line);

						code.Line();
						code.Line("continue;");
					}
				}
			}

			code.Line();
			foreach (var line in answer)
				code.Line(line);
		}
	}

	/// <summary>What a loop does once a way has been flipped: moves where it stands (<c>Retried_DotGram</c>) and reads the next attempt.</summary>
	static void Retried(Writer code, string suffix)
	{
		code.Line($"u{suffix} = Retried_DotGram(u{suffix});");
		code.Line();
		code.Line("continue;");
	}

	/// <summary>A loop that ends without an answer puts back what it raised (<c>Undeferred_DotGram</c>).</summary>
	static void Undeferred(Writer code, string suffix)
	{
		code.Line($"Undeferred_DotGram(u{suffix});");
	}

	/// <summary>The whole input, handed to a reader that may have to go on with it elsewhere.</summary>
	string WholeParameter => Probes ? ", global::System.ReadOnlyMemory<char> parserWhole" : "";

	internal string WholeArgument => Probes ? ", parserWhole" : "";

	/// <summary>
	/// Whether any rule this machine reads can reach itself, and so whether its reader
	/// probes the stack and is handed the input to go on with elsewhere.
	/// </summary>
	/// <remarks>
	/// Asked of the call graph rather than of the back edges, because the publications are
	/// written before the recognizers are and the back edges are found while writing them.
	/// A back edge means a cycle, so this is true wherever one is, which is what the two
	/// halves of the signature have to agree about.
	/// </remarks>
	internal bool Probes
	{
		get
		{
			if (_probes is { } known)
				return known;

			var calls = new CallGraph(_rules, Reached);

			_probes = false;

			foreach (var rule in _rules)
				if (calls.Recurses(rule))
				{
					_probes = true;

					break;
				}

			return _probes.Value;

			IEnumerable<RuleSymbol> Reached(RuleSymbol rule)
			{
				if (!_graph.Bodies.TryGetValue(rule, out var body))
					yield break;

				foreach (var node in NodeWalk.Descendants(body))
					if (node is Node.Call(var called, _) && _rules.Contains(called))
						yield return called;
			}
		}
	}

	/// <summary>
	/// A reading that ran the stack low, carried onto a stack of its own and gone on with
	/// there rather than thrown away.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The reader is a <c>ref struct</c> and holds the input as a span, and a span belongs
	/// to the thread whose stack it was made on. So what crosses is everything else — the
	/// tape, the tables, the registers, the failure, the position — together with the input
	/// as a <c>ReadOnlyMemory</c>, and the reader is made again on the other side.
	/// </para>
	/// <para>
	/// One thread a low stack rather than one a parse: the probe fires only when the runtime
	/// says the margin has gone, so a reading that never goes deep never makes one. A reading
	/// that goes deeper still probes again on the new stack and takes another, and so on for as
	/// long as there is memory to take one with. Every hand-off from one thread goes to the same
	/// one while it lingers (<see cref="RenderLingering"/>).
	/// </para>
	/// <para>
	/// What is not carried is a reading over a window (§6.3): there is no whole input to
	/// hand over, and the stack running out there is what it always was.
	/// </para>
	/// </remarks>
	void RenderDeepening(
		Writer file, IReadOnlyList<(string Type, string Name)> state,
		IReadOnlyList<(string Type, string Name)> registers)
	{
		var carried = new List<(string Type, string Name)>(state) { (CSharpEmitter.FailureType, "failure") };

		carried.AddRange(registers);

		file.Line();
		file.Line("/// <summary>Whether there is stack left to read one more level with.</summary>");
		file.Line("/// <remarks>");
		file.Line("/// `TryEnsureSufficientExecutionStack` is .NET Core's and .NET Standard 2.1's. Where");
		file.Line("/// it is not there — .NET Framework, netstandard2.0 — the older pair answers the same");
		file.Line("/// question by throwing, which is caught here and answered as a no. That costs an");
		file.Line("/// exception on the probe that finds the margin gone, and on no other: the reading is");
		file.Line("/// handed to a stack of its own at that point and goes on probing there.");
		file.Line("/// </remarks>");

		using (file.Block($"static bool EnoughStack_DotGram{_tag}()"))
		{
			file.Line("#if NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER");
			file.Line(
				"return global::System.Runtime.CompilerServices.RuntimeHelpers" +
				".TryEnsureSufficientExecutionStack();");
			file.Line("#else");

			using (file.Block("try"))
			{
				file.Line(
					"global::System.Runtime.CompilerServices.RuntimeHelpers" +
					".EnsureSufficientExecutionStack();");
				file.Line();
				file.Line("return true;");
			}

			using (file.Block("catch (global::System.InsufficientExecutionStackException)"))
				file.Line("return false;");

			file.Line("#endif");
		}

		file.Line();
		file.Line("/// <summary>Carries this reading onto a stack of its own and answers with what it read.</summary>");

		using (file.Block($"internal int Deepen_DotGram{_tag}(int pos, int which, int power)"))
		{
			file.Line("// Nothing to carry it onto: a reading over a window has no whole input.");
			file.Line("if (this.whole.IsEmpty)");
			file.Then("throw new global::System.InsufficientExecutionStackException();");
			file.Line();

			if (_stacks > 0)
			{
				file.Line("// As many as the author said were enough, and no more.");
				file.Line($"if (this.stacks >= {_stacks})");
				file.Then("throw new global::System.InsufficientExecutionStackException();");
				file.Line();
			}

			// The sink travels with the failure, and is told before the reading moves.
			if (Tracing is not null)
			{
				file.Line("failure.Trace?.Deepened(pos);");
				file.Line();
			}

			file.Line($"var deep = Deep_DotGram{_tag}.Take();");
			file.Line();
			file.Line("deep.whole  = this.whole;");
			if (_readerWays)
				file.Line("deep.ways   = this.ways;");

			foreach (var (_, name) in carried)
				file.Line($"deep.{name} = this.{name};");

			file.Line("deep.probes = this.probes;");

			if (_stacks > 0)
				file.Line("deep.stacks = this.stacks + 1;");

			file.Line("deep.pos    = pos;");
			file.Line("deep.which  = which;");
			file.Line("deep.power  = power;");
			file.Line();
			file.Line("deep.Go();");
			file.Line();

			// Back into the reading that asked: what it hands over is readonly and did not
			// move, and what it works with did.
			file.Line("this.failure = deep.failure;");

			foreach (var (_, name) in registers)
				file.Line($"this.{name} = deep.{name};");

			// Not the count the other stack ended on: this one is as low as it was when the probe
			// fired, so whatever is read on it next probes on its first entry, and every entry
			// after that until the reading climbs back above the margin (Interval).
			file.Line("// Still on the stack that ran low: the next entry probes it again.");
			file.Line("this.probes = 0;");

			file.Line();
			file.Line("var thrown = deep.thrown;");
			file.Line("var end    = deep.end;");
			file.Line();
			file.Line("// It lingers for the next hand-off: what it holds of this reading goes now.");
			file.Line("deep.Clear();");
			file.Line();
			file.Line("if (thrown != null)");
			file.Then(
				"global::System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(thrown).Throw();");
			file.Line();
			file.Line("return end;");
		}
	}

	/// <summary>
	/// The thread a reading is handed to: one for every hand-off from the same thread, made on the first
	/// and kept until it has waited <see cref="Linger"/> for another.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A hand-off that comes back finds the stack as low as it was, so everything read after it on
	/// that stack is handed off too, one rule at a time: a list of a hundred thousand values that
	/// begins at the margin is a hundred thousand hand-offs. A thread made for each cost about 150
	/// microseconds, and such a list was read in eight seconds where it takes a tenth of one. Handed
	/// to the thread the last one went to, each costs a wake-up.
	/// </para>
	/// <para>
	/// It holds a stack, and the pages a deep reading touched stay with it, so it does not outlive the
	/// work by more than <see cref="Linger"/> milliseconds; what it was handed of a reading is cleared
	/// as soon as that reading has it back. The thread that made it may ask again at the moment it
	/// decides to go, and <c>deepState</c> says which came first: 0 waiting, 1 reading, 2 gone.
	/// </para>
	/// <para>
	/// <b>Whose context a reading runs in.</b> The caller's: its execution context — every
	/// <c>AsyncLocal</c> a semantic action or a <c>when</c> may read — and its cultures are captured
	/// at every hand-off and the reading runs under them, and the thread goes back to its own after,
	/// so nothing one parse set is seen by the next. What does stay with the thread from one reading
	/// to the next is its thread-static state, which in generated code is the spare stores that are
	/// kept per thread to be reused, so a reused thread is what they are for; nothing emitted assumes
	/// a fresh one.
	/// </para>
	/// <para>
	/// <b>How many there are.</b> One for every thread that handed a reading off in the last
	/// <see cref="Linger"/> milliseconds, for every parser class, and one more for every level of
	/// hand-off below that: 16 MiB of address space each, and as much of it in use as the reading
	/// touched. There is no cap; a reading this deep is rare, and each ends shortly after its last.
	/// </para>
	/// <para>
	/// <b>A wait disturbed.</b> An interrupt of the thread that handed a reading off is held until the
	/// reading is done and thrown then: returning at once would leave a completion behind for the next
	/// hand-off to take as its own, and clear fields the reading still had in hand. Anything else that
	/// ends the wait early retires the thread.
	/// </para>
	/// </remarks>
	void RenderLingering(Writer file, IReadOnlyList<(string Type, string Name)> carried)
	{
		var deep = $"Deep_DotGram{_tag}";

		file.Line("/// <summary>The thread the hand-offs of this thread go to, while it waits for another.</summary>");
		file.Line("[global::System.ThreadStatic]");
		file.Line($"static {deep}? deepLingering;");
		file.Line();
		file.Line("/// <summary>The reading, under the context it was handed with.</summary>");
		file.Line($"static readonly global::System.Threading.ContextCallback deepReadIn = ReadIn;");
		file.Line();
		file.Line("readonly global::System.Threading.SemaphoreSlim deepGo   = new global::System.Threading.SemaphoreSlim(0, 1);");
		file.Line("readonly global::System.Threading.SemaphoreSlim deepDone = new global::System.Threading.SemaphoreSlim(0, 1);");
		file.Line();
		file.Line("int deepState = 1;");
		file.Line("volatile bool deepRetired;");
		file.Line("global::System.Threading.ExecutionContext? deepContext;");
		file.Line("global::System.Globalization.CultureInfo? deepCulture;");
		file.Line("global::System.Globalization.CultureInfo? deepUiCulture;");
		file.Line();
		file.Line("/// <summary>The thread this one handed off to last, if it is still waiting, or a new one.</summary>");

		using (file.Block($"internal static {deep} Take()"))
		{
			file.Line("var deep = deepLingering;");
			file.Line();
			file.Line("if (deep != null && !deep.deepRetired && global::System.Threading.Interlocked.CompareExchange(ref deep.deepState, 1, 0) == 0)");
			file.Then("return deep;");
			file.Line();
			file.Line($"deep       = new {deep}();");
			file.Line("deepLingering = deep;");
			file.Line();
			file.Line("var thread = new global::System.Threading.Thread(deep.Loop, 16 * 1024 * 1024) { IsBackground = true };");
			file.Line();
			file.Line("thread.Start();");
			file.Line();
			file.Line("return deep;");
		}

		file.Line();
		file.Line("/// <summary>Reads on that thread, in this one's context and cultures, and waits for it.</summary>");
		file.Line("/// <remarks>");
		file.Line("/// However the wait is disturbed: an interrupt is held until the reading is done and then thrown,");
		file.Line("/// since returning early would leave its completion for the next hand-off to take as its own, and");
		file.Line("/// anything else that ends the wait retires the thread, so that it is never handed another.");
		file.Line("/// </remarks>");

		using (file.Block("internal void Go()"))
		{
			file.Line("deepContext   = global::System.Threading.ExecutionContext.Capture();");
			file.Line("deepCulture   = global::System.Globalization.CultureInfo.CurrentCulture;");
			file.Line("deepUiCulture = global::System.Globalization.CultureInfo.CurrentUICulture;");
			file.Line();
			file.Line("deepGo.Release();");
			file.Line();
			file.Line("global::System.Threading.ThreadInterruptedException? interrupted = null;");
			file.Line();

			using (file.Block("while (true)"))
			{
				using (file.Block("try"))
				{
					file.Line("deepDone.Wait();");
					file.Line();
					file.Line("break;");
				}

				using (file.Block("catch (global::System.Threading.ThreadInterruptedException caught)"))
					file.Line("interrupted = caught;");

				using (file.Block("catch"))
				{
					file.Line("deepRetired = true;");
					file.Line();
					file.Line("throw;");
				}
			}

			file.Line();

			using (file.Block("if (interrupted != null)"))
			{
				file.Line("Clear();");
				file.Line();
				file.Line("global::System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(interrupted).Throw();");
			}
		}

		file.Line();
		file.Line("/// <summary>Lets go of what it was handed of a reading.</summary>");

		using (file.Block("internal void Clear()"))
		{
			file.Line("whole  = default;");
			if (_readerWays)
				file.Line("ways   = default!;");

			foreach (var (_, name) in carried)
				file.Line($"{name} = default!;");

			file.Line("thrown = null;");
		}

		file.Line();

		using (file.Block("void Loop()"))
		{
			using (file.Block("while (true)"))
			{
				using (file.Block("try"))
				{
					using (file.Block($"if (!deepGo.Wait({Linger}))"))
					{
						file.Line("if (global::System.Threading.Interlocked.CompareExchange(ref deepState, 2, 0) == 0)");
						file.Then("return;");
						file.Line();
						file.Line("// Taken in the moment before it could go: the reading is on its way.");
						file.Line("deepGo.Wait();");
					}
				}

				using (file.Block("catch (global::System.Threading.ThreadInterruptedException)"))
				{
					file.Line("// Nobody outside holds this thread, but the grammar's own code can interrupt it.");
					file.Line("continue;");
				}

				file.Line();
				file.Line("var handed = deepContext;");
				file.Line();
				file.Line("deepContext = null;");
				file.Line();
				file.Line("if (handed == null)");
				file.Then("ReadIn(this);");
				file.Line("else");
				file.Then("global::System.Threading.ExecutionContext.Run(handed, deepReadIn, this);");
				file.Line();
				file.Line("deepCulture   = null;");
				file.Line("deepUiCulture = null;");
				file.Line();
				file.Line("global::System.Threading.Volatile.Write(ref deepState, 0);");
				file.Line("deepDone.Release();");
			}
		}

		file.Line();
		file.Line("/// <summary>The reading, in the cultures of the thread that handed it over, and this thread's own put back.</summary>");

		using (file.Block("static void ReadIn(object? state)"))
		{
			file.Line($"var deep      = ({deep})state!;");
			file.Line("var culture   = global::System.Globalization.CultureInfo.CurrentCulture;");
			file.Line("var uiCulture = global::System.Globalization.CultureInfo.CurrentUICulture;");
			file.Line();

			using (file.Block("try"))
			{
				file.Line("global::System.Globalization.CultureInfo.CurrentCulture   = deep.deepCulture!;");
				file.Line("global::System.Globalization.CultureInfo.CurrentUICulture = deep.deepUiCulture!;");
				file.Line();
				file.Line("deep.Run();");
			}

			using (file.Block("finally"))
			{
				file.Line("global::System.Globalization.CultureInfo.CurrentCulture   = culture;");
				file.Line("global::System.Globalization.CultureInfo.CurrentUICulture = uiCulture;");
			}
		}

		file.Line();
	}

	/// <summary>Milliseconds a thread a reading was handed to waits for the next before it ends.</summary>
	internal const int Linger = 100;

	/// <summary>The state of a reading, in a shape that may be handed to another thread.</summary>
	void RenderDeep(
		Writer file, IReadOnlyList<(string Type, string Name)> state,
		IReadOnlyList<(string Type, string Name)> registers)
	{
		var carried = new List<(string Type, string Name)>(state) { (CSharpEmitter.FailureType, "failure") };

		carried.AddRange(registers);

		file.Line();
		file.Line("/// <summary>A reading, in a shape another thread can pick up (Machine.Reader.cs).</summary>");

		using (file.Block($"private sealed class Deep_DotGram{_tag}"))
		{
			file.Line("internal global::System.ReadOnlyMemory<char> whole;");
			if (_readerWays)
				file.Line($"internal {WaysType} ways = default!;");

			foreach (var (type, name) in carried)
				file.Line($"internal {type} {name} = default!;");

			file.Line("internal int probes;");

			if (_stacks > 0)
				file.Line("internal int stacks;");

			file.Line("internal int pos;");
			file.Line("internal int which;");
			file.Line("internal int power;");
			file.Line("internal int end;");
			file.Line("internal global::System.Exception? thrown;");
			file.Line();
			RenderLingering(file, carried);

			using (file.Block("internal void Run()"))
			{
				using (file.Block("try"))
				{
					file.Line(
						$"var reader = new {ReaderStruct}(this.whole.Span{(_readerWays ? ", this.ways" : "")}" +
						string.Concat(state.Select(one => $", this.{one.Name}")) + ", this.whole);");
					file.Line();

					foreach (var (_, name) in carried)
						if (state.All(one => one.Name != name))
							file.Line($"reader.{name} = this.{name};");

				file.Line("reader.probes = this.probes;");

					if (_stacks > 0)
						file.Line("reader.stacks = this.stacks;");

					file.Line();

					using (file.Block("switch (this.which)"))
						foreach (var one in _deep.OrderBy(each => each.Value))
						{
							var asked = _graph.Climbing.ContainsKey(one.Key) ? ", this.power" : "";

							file.Line(
								$"case {one.Value}: this.end = reader.{ReaderOf(one.Key)}(this.pos{asked}); break;");
						}

					file.Line();

					foreach (var (_, name) in carried)
						if (state.All(one => one.Name != name))
							file.Line($"this.{name} = reader.{name};");

				file.Line("this.probes = reader.probes;");

				}

				using (file.Block("catch (global::System.Exception caught)"))
					file.Line("this.thrown = caught;");
			}
		}
	}

	/// <summary>The name of the reader the rules of this machine are members of.</summary>
	string ReaderStruct => "Reader_DotGram" + _tag;

	/// <summary>The tokens, as a reader over kinds holds them.</summary>
	internal static readonly (string Type, string Name)[] TokenState =
		[("string", "parserSource"), ("int[]", "parserStarts"), ("int[]", "parserLengths")];

	/// <summary>
	/// The reader: every rule and every part of one as a method, and what they all read from
	/// as fields — the text, the failure, the ways, whatever the carrier has them hold, and
	/// the registers the carrier has them hand values through.
	/// </summary>
	/// <remarks>
	/// A <c>ref struct</c>, so that it may hold the text as a span and live where a
	/// hand-written reader lives, on the stack of the call that made it. Before this every
	/// reader took eight parameters and passed them on to every reader it called; now a call
	/// between two of them passes a position, and the state is loaded once.
	/// </remarks>
	void RenderReaderStruct(Writer file)
	{
		var header    = new Writer(0);
		var state     = Carrier.ReaderState.ToList();
		var registers = Carrier.ReaderRegisters.ToList();

		header.Line("/// <summary>The readers of the grammar, and what they all read from, in one place: a call between them passes a position and nothing else.</summary>");

		header.Line($"private ref struct {ReaderStruct}");
		header.Line("{");

		using (header.Indent())
		{
			header.Line($"readonly {InputType} text;");
			header.Line($"internal {CSharpEmitter.FailureType} failure;");

			if (Probes)
			{
				header.Line("/// <summary>Entries to a rule that can reach itself, for the stack probe.</summary>");
				header.Line("internal int probes;");

				if (_stacks > 0)
				{
				header.Line("/// <summary>How many stacks this reading has taken past the one it began on.</summary>");
					header.Line("internal int stacks;");
				}
			}

			header.Line(_readerWays ? $"readonly {WaysType} ways;" : $"const {WaysType}? ways = null;");

			if (Memoises)
				MemoFields(header);

			if (CountsRules && _directRules is not null)
			{
				header.Line();
				header.Line("#if DOTGRAM_COUNTS");
				header.Line("// Entries to each rule's method, for a test that counts them; compiled only under");
				header.Line("// DOTGRAM_COUNTS, which no project defines, and not a supported setting.");

				foreach (var rule in _directRules)
					header.Line($"internal static long {EnteredOf(rule)};");

				header.Line("#endif");
				header.Line();
			}

			if (Probes)
			{
				header.Line("/// <summary>The whole input, for a reading that has to go on with it elsewhere.</summary>");
				header.Line("readonly global::System.ReadOnlyMemory<char> whole;");
			}

			foreach (var (type, name) in state)
				header.Line($"readonly {type} {name};");

			foreach (var (type, name) in registers)
				header.Line($"internal {type} {name};");

			header.Line();

			using (header.Block(
				$"internal {ReaderStruct}({InputType} text{(_readerWays ? $", {WaysType} ways" : "")}" +
				string.Concat(state.Select(one => $", {one.Type} {one.Name}")) + WholeParameter + ")"))
			{
				header.Line("this.text    = text;");
				header.Line("this.failure = default;");
				if (_readerWays)
					header.Line("this.ways    = ways;");

				// A C# 8 struct auto-defaults nothing, and the floor is C# 8.
				if (Probes)
					header.Line("this.probes  = 0;");

				if (_stacks > 0)
					header.Line("this.stacks  = 0;");

				foreach (var (_, name) in state)
					header.Line($"this.{name} = {name};");

				foreach (var (_, name) in registers)
					header.Line($"this.{name} = default!;");

				if (Probes)
					header.Line("this.whole   = parserWhole;");

				if (Memoises)
					MemoTaken(header);
			}

			if (Memoises)
				MemoFailedMethod(header, _looks);

			if (Probes)
				RenderDeepening(header, state, registers);

			if (Tracing is not null)
				header.Write(Lines.Normalize(CSharpEmitter.TraceReaderMethods));

			header.Line();
			if (Carrier.ReaderMethods is { Length: > 0 } methods)
				header.Write(methods);
		}

		file.Prepend(header);
		file.Line("}");

		if (Probes)
			RenderDeep(file, state, registers);

		file.Line();
	}

	/// <summary>The whole input as one rule, which is what a publication asks for.</summary>
	/// <param name="ends">
	/// Whether the input has to be finished where the rule is: what a whole parse asks and a
	/// reading that begins where it was told does not. The two are the same entry otherwise —
	/// the same renting, the same reader, the same root built — so they are written by one
	/// method and told apart by this.
	/// </param>
	void RenderReaderEntry(Writer file, RuleSymbol rule, bool ends)
	{
		_seam = FollowSets.SeamOf(rule, _graph);

		var core   = CSharpEmitter.MethodOf(rule) + (ends ? "_Whole" : "");
		var type   = _results.QualifiedOf(rule);
		var valued = type is not null;
		var value  = valued ? $", out {type} value" : "";
		var climbs = _graph.Climbing.ContainsKey(rule);
		var asked  = climbs ? ", power" : "";

		file.Line($"/// <summary>The whole input as <c>{rule.Name}</c>, read by methods.</summary>");

		// The parameters in the order the wrapper hands them: the strength beside the
		// position, then the value, the input, the tokens, the context (CSharpEmitter.EmitPublication).
		using (file.Block(
			$"static int {core}(" +
			$"{InputType} text, int pos{(climbs ? ", int power" : "")}, " +
			$"ref {CSharpEmitter.FailureType} failure{value}{InputParameter}{TokensParameter}{ContextParameter}{ReadingParameter}{WholeParameter})"))
		{
			// The tape is what the records of a parse are written on; the tables are what
			// the walk at the end builds into. A refusal inside a lookahead is kept quiet by
			// the failure's own count, not by the tape (EmitLookahead).
			if (_readerWays)
				file.Line($"var ways = {WaysType}.Rent();");

			// The store is rented where the entry builds, and also where the reader is handed
			// one anyway: a carrier that builds as it reads hands its reader the store in every
			// entry of a machine that builds anywhere, a recognizing one included.
			var handed = Carrier.ReaderState.Any(static one => one.Name == "values");
			var renting = valued || handed;

			// Where the reader is not handed the store, nothing touches it until the walk at
			// the end, and a refusal never reaches that walk: it leaves at `if (end < 0)`
			// having read, recorded and decided without a table. Renting it before the read
			// therefore costs every refusal a rent and a return of a store it does not see —
			// about 20 ns of a 42 ns refusal of a media type, measured — so it is rented
			// after the reading stands instead, and returned by a `finally` of its own so
			// that a construction which throws still hands the pool back.
			var returning = renting ? Carrier.Return().ToList() : new List<string>();

			// And only where there is a store to rent at all: a carrier that builds as it
			// reads may need none — it keeps what it built in the reader's own locals — and
			// moving nothing later would buy nothing and write an empty `finally` for it.
			var late = renting && !handed && returning.Count > 0;
			var cleanup = _readerWays || returning.Count > 0 && !late;

			if (renting && !late)
				foreach (var line in Carrier.Rent())
					file.Line(line);

			file.Line();

			using (cleanup ? file.Block("try") : null)
			{
				if (Memoises)
					MemoBegun(file);

				file.Line($"var reader = new {ReaderStruct}(text{(_readerWays ? ", ways" : "")}{Carrier.ReaderArgument}{WholeArgument});");
				file.Line();
				file.Line("reader.failure = failure;");

				// A reading nothing asks the value of — the second reading of a refused input, made
				// for its message — begins with the count raised, and the carrier that builds as it
				// reads builds only what a guard asks for (ImmediateCarrier.AroundCall). Not for a
				// rule whose own guard asks for its value so far: that one builds whenever it is read.
				if (valued && CarriesImmediately && UnaskedEntries.Contains(rule) && Demands.ReadUnasked(rule))
					file.Line("if (failure.Unasked) reader.unbuilt = 1;");

				file.Line();
				file.Line($"var end = reader.{core}_Read(pos{asked});");
				file.Line();
				file.Line("failure = reader.failure;");
				file.Line();

				if (valued)
				{
					using (file.Block("if (end < 0)"))
					{
						file.Line("value = default!;");
						file.Line();
						file.Line("return end;");
					}

					file.Line();

					if (late)
					{
						foreach (var line in Carrier.Rent())
							file.Line(line);

						file.Line();
					}

					using (late ? file.Block("try") : null)
					{
						foreach (var line in Carrier.BuildRoot(rule, type!, IsExtent(rule)))
							file.Line(line);

						if (late)
						{
							file.Line();
							file.Line("return end;");
						}
					}

					if (late)
					{
						file.Line("finally");

						using (file.Block(""))
						{
							foreach (var line in returning)
								file.Line(line);
						}
					}

					file.Line();
				}

				if (!(valued && late))
					file.Line("return end;");
			}

			if (cleanup)
			{
				file.Line("finally");

				using (file.Block(""))
				{
					if (_readerWays)
						file.Line($"{WaysType}.Return(ways);");

					// A store rented late is handed back by the `finally` that stands with
					// its rent, inside the reading that stands; nothing is owed here.
					if (!late)
						foreach (var line in returning)
							file.Line(line);
				}
			}
		}

		file.Line();

	}

	/// <summary>The entry's reading, including trivia and its continuation.</summary>
	void RenderReaderEntryBody(Writer members, RuleSymbol rule, bool ends)
	{
		_seam = FollowSets.SeamOf(rule, _graph);

		var core = CSharpEmitter.MethodOf(rule) + (ends ? "_Whole" : "");

		// Over characters the whole input is a reading like any other: the rule may have
		// answered with less than all of it, and then it is asked for its next answer
		// rather than refused. Without this the very first reading that reached the end
		// short was the last, whatever the tape still had to offer.
		var tape = _opens is null || _opens.Contains(rule) ||
			(_graph.Trivia.TryGetValue(rule, out var around) && Opens(around));

		var said = ends ? "The whole input as" : "A reading of";

		if (tape)
			RenderWayBack(members, rule, core + "_Read", $"/// <summary>{said} <c>{rule.Name}</c>, and the way back into it.</summary>", DirectStrength(rule), entry: true);

		members.Line($"/// <summary>What <c>{rule.Name}</c> is read by, whichever stack it is read on.</summary>");

		using (members.Block($"public int {core}_Read{(tape ? "_Body" : "")}(int pos{DirectStrength(rule)})"))
		{
			// A whole input is the trivia, the rule and the trivia again, because reaching the
			// end is what it is for. A reading that begins where it is told stops where the
			// rule ends (§6.3): the value's own extent is what a host reading a piece of a text
			// it holds is asking for, and the trivia after it is the next reading's leading
			// trivia.
			var reader = new ReaderWriter(this, rule);

			// A reading that begins where it is told says where the value began, which is where
			// the trivia it started on ended: the entry writes it between the two.
			if (!ends && _graph.Trivia.ContainsKey(rule) && !OverKinds)
				reader.MarkLead("failure.Began = p;");

			var body   = _graph.Trivia.TryGetValue(rule, out var seam)
				? ends
					? new Node.Sequence([seam, new Node.Call(rule, []), seam])
					: new Node.Sequence([seam, new Node.Call(rule, [])])
				: (Node)new Node.Call(rule, []);

			members.Write(reader.Render(
				body,
				ends ? FollowSets.Continuation.End : FollowSets.Continuation.All,
				entry: true,
				ends: ends));
		}

		members.Line();
	}

	/// <summary>
	/// One rule's body as statements.
	/// </summary>
	/// <remarks>
	/// The position is a local, and every construct that reads moves it. Nothing else is
	/// carried: there is no tape while the readers commit, so a construct that fails has
	/// nothing to put back but the position, and the position is the caller's own copy.
	/// </remarks>
	/// <param name="given">Positions the rule captured before this part, which it only reads.</param>
	/// <param name="taken">Positions this part captures and something after it reads.</param>
	/// <param name="handed">
	/// Whether the value built so far arrived as an argument, which every part of a folded
	/// rule but its body does.
	/// </param>
	sealed class ReaderWriter(
		Machine machine, RuleSymbol owner, IReadOnlyList<int>? given = null,
		IReadOnlyList<int>? taken = null, bool handed = false, bool analyzing = false)
	{
		/// <summary>
		/// Whether the rule was left-recursive, and so is a base and a loop of steps over it.
		/// </summary>
		/// <remarks>
		/// A step's record leads with the value built so far — the record of the base or of
		/// the step before it (§4.3) — which is one local, written after every record the
		/// rule makes and read by the next step. The rule's body and every part of it are
		/// separate methods here, so the local is handed between them by reference.
		/// </remarks>
		readonly bool _folds = machine._graph.Folds.ContainsKey(owner);

		/// <summary>
		/// Whether a record carries where it stood, which the walk reads where a factory
		/// asks for the text or the span and where a terminal is reread from its span.
		/// </summary>
		/// <remarks>
		/// Where it does, the walk reads a four-word header and a record written with two
		/// puts it two words out of step with every record after it. The start it carries
		/// is the rule's, which in a part is not that method's own <c>pos</c>: it arrives
		/// as <c>start</c>.
		/// </remarks>
		readonly bool _positions = machine._directRules is { } placed && machine.DirectPositions(placed);

		/// <summary>Whether this method is a part of a rule rather than its body.</summary>
		readonly bool _part = given is not null;

		/// <summary>
		/// Whether the rule is written with binding powers (§4.3.1), and so is entered at a
		/// strength: every method of it takes <c>power</c>, and an alternative below the
		/// strength asked for is refused without a word.
		/// </summary>
		readonly bool _climbs = machine._graph.Climbing.ContainsKey(owner);

		/// <summary>Whether this is the entry's own reading, which calls the rule at the strength it was asked.</summary>
		bool _entry;

		/// <summary>
		/// Whether this rule writes on the tape: every rule over characters, and over kinds
		/// the one marked <c>?</c> (§4), which gives back inside itself — its choices and
		/// runs are recorded, its own failures are retried, and once it has answered the
		/// answer stands and is sealed. A caller, which commits, is never sent back into it.
		/// </summary>
		readonly bool _tape = !machine.OverKinds || owner.GivesBack;

		/// <summary>
		/// Whether the rule has a <c>when</c> in it (§7.7), which reads what the rule has
		/// captured so far and the text from where the rule began: every method of the rule
		/// is handed the rule's start and its log mark for that.
		/// </summary>
		readonly bool _guarded = NodeWalk.Descendants(machine._graph.Bodies[owner]).Any(static one => one is Node.Guard);

		/// <summary>
		/// Whether anything in this reading writes a record, and so a failed attempt can
		/// leave records on the log that nothing will ever refer to.
		/// </summary>
		/// <remarks>
		/// The log is put back when it does. Nothing was wrong while it was not — the walk
		/// at the end follows references and an abandoned record has none — but the log
		/// grew with every alternative that failed, the value tables were sized to it, and
		/// returning them cleared what had never been used. Half a microsecond a parse on
		/// the SQL yardstick, measured (docs/next.md).
		/// </remarks>
		readonly bool _logs = machine._directRules is { } rules && rules.Any(machine.Valued);

		/// <summary>Whether anything the rule keeps is gathered across the turns of a repetition.</summary>
		readonly bool _gathers = machine.DirectMembers(owner)
			.Exists(one => one.Shape is MemberShape.Pieces or MemberShape.Records);

		/// <summary>
		/// Whether what this method has written on the path to here pushes a gathered member —
		/// itself, or through a part — and so whether a failure past this point gives it back
		/// (<see cref="Fails"/>). Written in the order the method is, except across the
		/// alternatives of a choice, which <see cref="PushedApart"/> keeps apart.
		/// </summary>
		bool _pushed;

		/// <summary>
		/// The alternatives of one choice with their pushes kept apart: each is rendered from
		/// what stood pushed before the choice — what the one before it pushed was given back
		/// before this one was tried, and is not on this one's path — and the choice leaves
		/// pushed whatever any of them did.
		/// </summary>
		sealed class PushedApart(ReaderWriter writer)
		{
			readonly bool _before = writer._pushed;
			bool _any;

			/// <summary>Before each alternative.</summary>
			public void Next()
			{
				_any |= writer._pushed;
				writer._pushed = _before;
			}

			/// <summary>After the last.</summary>
			public void Done()
			{
				writer._pushed |= _any;
			}
		}

		readonly RecognitionGraph _graph = machine._graph;

		/// <summary>What stands in this method already, and so is not declared in it.</summary>
		readonly HashSet<int> _handed = [.. given ?? [], .. taken ?? []];

		/// <summary>The alternatives written as methods of their own, and their bodies.</summary>
		public List<(string Name, string Taken, string Body)> Parts { get; } = [];

		/// <summary>The parts only a failure reaches, written out of line (<see cref="EmitRecovering"/>).</summary>
		public HashSet<string> Cold { get; } = [];

		/// <summary>Whether the provisional method or any of its parts writes an open way.</summary>
		public bool ObservedOpen { get; private set; }

		bool _character;

		/// <summary>Ways opened and marks taken, which only a reading over characters has.</summary>
		int _ways;
		int _marks;
		int _turns;

		/// <summary>Doors a reading that records goes through shut (TriesBehindTheDoor), counted apart so that naming them moves no other local.</summary>
		int _doors;

		/// <summary>Whether this method writes a record, and so needs the side stack mark.</summary>
		bool _records;

		/// <summary>One body as statements: a rule's own, a part of one, or an entry's.</summary>
		/// <param name="entry">
		/// Whether this body is an entry's rather than the rule's own. An entry wraps a
		/// <em>call</em> to the rule instead of being what the rule is, so the rule is called at
		/// the strength the entry was asked, and the two tails a rule writes at the end of its
		/// own body are not written here — they belong to the body this one calls.
		/// </param>
		/// <param name="ends">
		/// Whether the input has to be finished where this body is: what <c>parse</c> says and
		/// <c>find</c> does not.
		/// </param>
		/// <remarks>
		/// The two were one flag called <c>whole</c>, which was true of every entry there was
		/// because every entry read the whole input from zero. They are apart now because that
		/// is no longer the only kind: an entry asked to read from a position is an entry — it
		/// calls the rule and writes no tails — and is not an end.
		/// </remarks>
		public string Render(
			Node body, FollowSets.Continuation following, bool entry = false, bool ends = false)
		{
			var code = analyzing ? new Writer(0, "ways.Open(") : new Writer(0);
			Render(code, body, following, entry, ends);
			return code.ToString();
		}

		/// <summary>Writes a method body directly into its destination, inserting locals at its start.</summary>
		public void Render(
			Writer code, Node body, FollowSets.Continuation following, bool entry = false, bool ends = false)
		{
			var prefix = code.Length;

			_entry = entry;

			Emit(code, body, following);

			// A rule whose value is the record of its captures and nothing more has no
			// construction to write that record at, so it is written where the rule ends —
			// by the rule's body and not by a part of it, and not by the entry that reads
			// the whole input through it.
			if (!entry && !_part && machine.RecordsAtEnd(owner))
				EmitRecord(code, -1);

			// And a folding rule is worth its base and the turns over it, which is known
			// where the turns stop — the loop being an ordinary repetition, the only place
			// that knows is after the body.
			if (!entry && !_part && _folds)
				Carried(code, machine.Carrier.Folded(owner));

			if (ends)
			{
				using (code.Block($"if ({machine.NotAtEnd("p")})"))
				{
					code.Line(Refusal(machine.EndOfInputExpected()));
					code.Line(Fails());
				}
			}

			code.Line(Succeeds("p"));

			if (analyzing)
			{
				ObservedOpen |= code.Observed;
				return;
			}

			// The body tells us which locals are needed. Insert only those declarations
			// at this method's start, leaving previously emitted methods untouched.

			Declare("var p = pos;");

			foreach (var line in _hoisted)
				Declare(line);

			// A byte is read into an int, as the engine reads one, so that it compares with the
			// character a set or a literal is written in.
			if (_character)
				Declare(machine.BufferedBytes ? "var c = 0;" : "var c = '\\0';");

			// The refs mark: where this method writes a record, and where the rule gathers
			// and this is its body, which hands the mark to every part whether or not it
			// writes one itself.
			// The lists a rule gathers into, where the carrier keeps them as such: the body's
			// own, handed to every part of it.
			if (_gathers && !_part)
				foreach (var member in machine.DirectMembers(owner))
					if (member.Shape is MemberShape.Pieces or MemberShape.Records)
						foreach (var slot in member.Slots)
							foreach (var line in machine.Carrier.DeclareGathered(slot, member.Shape == MemberShape.Pieces ? "string" : machine._results.ValueOf(member.Member.Rule)))
								Declare(line);

			if (_records || (_gathers && !_part))
				foreach (var line in machine.Carrier.MarkGathered(owner, "rb"))
					Declare(line);

			// Only where something in the method names it: a rule folds, but a method of it
			// that neither writes a record nor hands the value on has nothing to do with it,
			// and a local nothing reads is an error in somebody else's build.
			if (_folds && !handed)
				foreach (var (_, name) in machine.Carrier.FoldState(owner))
					if (code.Contains(name, prefix))
					{
						Declare(machine.Carrier.DeclareAccumulator(owner));

						break;
					}

			// Where the log stood when the rule began, for a guard that builds a value from
			// what has been recorded since.
			if (_guarded && !_part)
				foreach (var line in machine.Carrier.MarkRecords("lm"))
					Declare(line);

			foreach (var slot in _kept.OrderBy(static one => one))
			{
				// A position handed in is already a name in this method.
				if (_handed.Contains(slot))
					continue;

				switch (machine.MemberOfSlot(owner, slot)?.Shape)
				{
					case MemberShape.Text:
						Declare($"var a{slot} = -1;");
						Declare($"var b{slot} = -1;");
						break;

					// Where it was pushed from is all a gathered run of text keeps: the end
					// is the position the push is written at.
					case MemberShape.Pieces:
						Declare($"var a{slot} = -1;");
						break;

					default:
						Declare(machine.Carrier.DeclareRecordLocal(slot, RuleOfSlot(slot), Optional(slot)));
						break;
				}
			}

			void Declare(string text)
			{
				prefix = code.InsertLine(prefix, text);
			}
		}

		/// <summary>
		/// Whether this writer renders a remembered rule's own method in a trace build, whose
		/// failures then go to the one place that remembers them (Machine.Memo.cs).
		/// </summary>
		internal bool Memoised;

		/// <summary>
		/// The frames of forwarding rules open where code is being written, innermost last: the
		/// rule's number and the local holding where it was entered. A trace build's only.
		/// </summary>
		readonly List<(int Rule, string At)> _frames = [];

		/// <summary>The frames open where the method this writer renders is called, which it does not close.</summary>
		List<int> _outer = [];

		int _frameLocals;

		/// <summary>Whether this writer renders a rule's own method in a trace build, and so reports its returns.</summary>
		bool Reports => machine.Tracing is not null && !_part && !_entry;

		/// <summary>
		/// What leaves this method refusing: <c>return -1;</c>, and in a trace build the frames
		/// open here closed first and the rule's own exit reported, or the way to the place a
		/// remembered rule's failures go.
		/// </summary>
		/// <remarks>
		/// Written at every return site rather than recovered from the text afterwards: a site
		/// knows which frames are open where it stands, and the text does not.
		/// </remarks>
		string Fails()
		{
			var given = GivenBack();

			if (machine.Tracing is not { } tracing)
				return given.Length == 0 ? "return -1;" : $"{{ {given} return -1; }}";

			var leave = !Reports
				? "return -1;"
				: Memoised
					? "goto Failed;"
					: $"return Exited_DotGram({tracing.RuleOf(owner)}, pos, -1);";

			if (_frames.Count == 0 && given.Length == 0)
				return leave;

			var closed = new System.Text.StringBuilder("{ ");

			if (given.Length > 0)
				closed.Append(given).Append(' ');

			for (var i = _frames.Count - 1; i >= 0; i--)
				closed.Append($"failure.Trace?.Exit({_frames[i].Rule}, {_frames[i].At}, -1); ");

			return closed.Append(leave).Append(" }").ToString();
		}

		/// <summary>What a failure here gives back before it leaves (<see cref="Fails"/>), or nothing.</summary>
		/// <remarks>
		/// What the rule pushed is given back by the rule's own body, past a push: not by a part,
		/// whose failure the turn or the choice around it puts back, and not by the entry, whose
		/// failure ends the parse.
		/// </remarks>
		string GivenBack()
		{
			return !_part && !_entry && _gathers && _pushed
				? string.Join(" ", machine.Carrier.GiveBackGathered(owner, "rb"))
				: "";
		}

		/// <summary>What leaves this method having read to <paramref name="end"/>.</summary>
		string Succeeds(string end)
		{
			return Reports
				? $"return Exited_DotGram({machine.Tracing!.RuleOf(owner)}, pos, {end});"
				: $"return {end};";
		}

		/// <summary>The rules of the frames open here, those of the caller first.</summary>
		List<int> Open()
		{
			var open = new List<int>(_outer);

			foreach (var (rule, _) in _frames)
				open.Add(rule);

			return open;
		}

		/// <summary>
		/// The frames a node stands for that are not open already, outermost first: a call
		/// under a choice that is framed by the same rule is framed only by what lies between.
		/// </summary>
		List<int> Unopened(Node node)
		{
			var chain = machine.Tracing!.FramesOf(node);

			if (chain.Count == 0)
				return [];

			var ids  = chain.Select(machine.Tracing.RuleOf).ToList();
			var open = Open();

			for (var skip = Math.Min(open.Count, ids.Count); skip > 0; skip--)
			{
				var same = true;

				for (var i = 0; i < skip && same; i++)
					same = open[open.Count - skip + i] == ids[i];

				if (same)
					return ids.Skip(skip).ToList();
			}

			return ids;
		}

		/// <summary>
		/// A call whose part is the call alone, framed by the forwarding rules it stands for: the
		/// frames entered as its position is handed in and left as its end is handed back.
		/// </summary>
		string Framed(Node.Call call, string reader, string strength)
		{
			List<int> frames = machine.Tracing is null ? [] : Unopened(call);
			var entered      = "p";

			foreach (var rule in frames)
				entered = $"Entered_DotGram({rule}, {entered})";

			var framed = $"{reader}({entered}{strength})";

			for (var i = frames.Count - 1; i >= 0; i--)
				framed = $"Exited_DotGram({frames[i]}, p, {framed})";

			return framed;
		}

		/// <param name="loaded">
		/// Whether <c>c</c> already holds <c>text[p]</c> and the position is known to be in
		/// bounds — true right after a choice's dispatch, and carried only as far as
		/// nothing has consumed. What it saves is what every alternative of a dispatched
		/// choice used to do over again: its own bounds check and its own read of the
		/// character the switch had just read.
		/// </param>
		/// <remarks>
		/// In a trace build a node a collapsed forwarding rule stood for is framed by that rule:
		/// entered before it, left after it where it reads, and left at every return inside it
		/// where it refuses (<see cref="Fails"/>). What is emitted between is what it always was.
		/// </remarks>
		void Emit(Writer code, Node node, FollowSets.Continuation following, bool loaded = false)
		{
			List<int> frames = machine.Tracing is null ? [] : Unopened(node);

			foreach (var rule in frames)
			{
				var at = $"tf{_frameLocals++}";

				code.Line($"var {at} = p;");
				code.Line($"failure.Trace?.Enter({rule}, p);");
				_frames.Add((rule, at));
			}

			EmitInside(code, node, following, loaded);

			for (var i = 0; i < frames.Count; i++)
			{
				var (rule, at) = _frames[_frames.Count - 1];

				code.Line($"failure.Trace?.Exit({rule}, {at}, p);");
				_frames.RemoveAt(_frames.Count - 1);
			}
		}

		void EmitInside(Writer code, Node node, FollowSets.Continuation following, bool loaded)
		{
			// An alternative of a rule written with binding powers is entered only at a
			// strength it allows. Refused without a word: what could not be entered here was
			// never expected here, and the alternatives after it still can be.
			if (machine._owners.TryGetValue(node, out var climbs) &&
				_graph.Climbing.TryGetValue(climbs, out var levels) &&
				levels.TryGetValue(node, out var level))
			{
				code.Line($"if ({level} < power)");
				code.Then(Fails());
				code.Line();
			}

			switch (node)
			{
				case Node.Empty:
					break;

				// Two tokens with nothing between them (`~`): over kinds the next has to begin
				// where the last ended, which the token positions say; over characters there
				// is nothing between two characters to begin with.
				case Node.Glue:
					if (machine.OverKinds)
					{
						using (code.Block(
							"if (p > 0 && p < text.Length && " +
							"parserStarts[p - 1] + parserLengths[p - 1] != parserStarts[p])"))
						{
							Refused(code, machine.DeclareExpected(machine.Displays(node)));
						}
					}

					break;

				case Node.Literal(var text) { IgnoreCase: var folded }:
					EmitLiteral(code, node, text, folded, loaded);
					break;

				case Node.Element element:
					EmitElement(code, element, loaded);
					break;

				// What follows a part is the parts after it, and past the last of them
				// whatever follows the sequence — read backwards, which is the only order
				// in which that is known.
				case Node.Sequence(var parts):
				{
					var follows = new FollowSets.Continuation[parts.Count];
					var next    = following;

					for (var i = parts.Count - 1; i >= 0; i--)
					{
						follows[i] = next;
						next       = FollowSets.Precedes(parts[i], next, _graph, machine._seam);
					}

					// The token the dispatch read is still in hand past a look, which moves
					// nothing: the part after it is the first to consume, and reads as if it
					// had been first.
					var still = loaded;

					for (var i = 0; i < parts.Count; i++)
					{
						// A repetition marked `recover` tries what follows it at every boundary
						// (§8.2), so it reads what follows it too, and the sequence ends there.
						if (Recovering(parts[i]) is (Node.Repeat recovering, RecoveryRead read) && read.Continuation.Count > 0)
						{
							EmitRecovering(code, recovering, read, following);

							break;
						}

						// A repetition that stops at one character is told what follows that character,
						// for the turn that begins with it (Determinism.NeverGivesBackPast).
						if ((parts[i] is Node.Repeat one ? one : parts[i] is Node.Capture(_, Node.Repeat held) ? held : null) is { } stopped &&
							i + 1 < parts.Count &&
							parts[i + 1] is Node.Literal { IgnoreCase: false, Text: { Length: 1 } stop })
							_stopAfter = (stopped, stop[0], follows[i + 1]);

						Emit(code, parts[i], follows[i], still);

						// Where the value begins, for a reading that begins where it is told: the entry
						// reads the trivia at the position it was handed and the rule after it, and this
						// is between the two (§6.3).
						if (i == 0 && _leadMark is { } mark)
						{
							code.Line(mark);
							_leadMark = null;
						}

						_stopAfter = null;
						still = still && parts[i] is Node.Lookahead or Node.Empty;
					}

					break;
				}

				case Node.Choice { Selection: { } selection } choice:
					EmitSelection(code, choice, selection, following);
					break;

				case Node.Choice(var alternatives):
					EmitChoice(code, alternatives, following);
					break;

				case Node.Repeat repeat:
					EmitRepeat(code, repeat, following);
					break;

				case Node.Call(var called, _):
					EmitCall(code, node, called);
					break;

				case Node.Lookahead(var positive, var inside):
					EmitLookahead(code, positive, inside, loaded);
					break;

				case Node.Capture(_, var held):
					EmitCapture(code, node, held, following, loaded);
					break;

				// A look at the character behind, which reads into `c`: where the token in
				// hand was in `c`, it is put back after.
				case Node.Behind(var boundary):
				{
					var name = machine.DeclareExpected(machine.Displays(node));

					_character = true;

					using (code.Block("if (p > 0)"))
					{
						code.Line($"c = {machine.ReadAt("p - 1")};");
						code.Line();

						using (code.Block($"if ({CSharpEmitter.Test(boundary, machine.Tabulate)})"))
							Refused(code, name);

						if (loaded)
						{
							code.Line();
							code.Line($"c = {machine.ReadAt("p")};");
						}
					}

					break;
				}

				// Which parsers keep this alternative was answered while they were generated.
				case Node.Reading(var readings):
					using (code.Block($"if (((0x{readings:X}UL >> parserReading) & 1UL) == 0UL)"))
					{
						code.Line(Refusal("null"));
						code.Line(Fails());
					}

					break;

				// A recognizer the author wrote, handed the position by reference: it says
				// yes or no, and where it said no is where it left the position.
				case Node.External external:
					using (code.Block($"if (!{machine.ExternalCall(external, "p")})"))
					{
						code.Line(Refusal("null"));
						code.Line(Fails());
					}

					break;

				case Node.Atomic(var kept):
					EmitAtomic(code, kept, following, loaded);
					break;

				case Node.Guard guard:
					EmitGuard(code, guard, loaded ? _guardRefuses : null);
					break;

				// A mark is a record of its own: it goes with the log wherever the log is put
				// back, which is the whole of what an abandoned reading owes it (§7.8).
				case Node.Marked(var marked, var text):
				{
					var site = machine.MarkSite(text);

					Carried(code, machine.Carrier.Mark(-1, site));
					Emit(code, marked, following, loaded);
					Carried(code, machine.Carrier.Mark(-2, site));

					break;
				}

				case Node.Construct(var built, _):
					Emit(code, built, following, loaded);
					EmitRecord(code, machine._constructs[node]);
					break;

				default:
					throw new InvalidOperationException(
						$"{node.GetType().Name} passed CanRead and the reader has no statement for it.");
			}
		}

		/// <summary>What the rule keeps of what it read: a record it names, or the text.</summary>
		void EmitCapture(
			Writer code, Node capture, Node held, FollowSets.Continuation following, bool loaded = false)
		{
			// Slots are numbered across the whole machine and members are numbered inside
			// one rule, so the rule's first slot is where the two meet (Machine.Direct.cs).
			var slot   = machine._captureSlots[capture] - machine._captureOffsets[owner];
			var member = machine.MemberOfSlot(owner, slot);

			if (member is null)
			{
				Emit(code, held, following, loaded);

				return;
			}

			// A capture nothing reads — `v: Primary => @(v)` hands the operand up through
			// the register, and its local was written on every reading and read on none
			// — is the reading and no local: a frame of eight references nothing reads is
			// eight words zeroed on every call, and the value expression primary of SQL
			// had exactly that.
			if (member.Shape is MemberShape.Text or MemberShape.Record && !ReadAnywhere.Contains(slot))
			{
				Emit(code, held, following, loaded);

				return;
			}

			switch (member.Shape)
			{
				case MemberShape.Text:
					code.Line($"a{slot} = p;");
					Emit(code, held, following, loaded);
					code.Line($"b{slot} = p;");
					break;

				case MemberShape.Record:
					Emit(code, held, following, loaded);
					code.Line($"r{slot} = {machine.Carrier.Last(RuleOfSlot(slot))};");
					break;

				// What a repetition gathers is pushed as it goes and collected when the
				// record is written: the pushes are on the tape, which every method of the
				// rule shares, so nothing has to be handed between them.
				case MemberShape.Pieces:
					code.Line($"a{slot} = p;");
					Emit(code, held, following, loaded);
					Carried(code, machine.Carrier.PushText(slot, $"a{slot}", "p"));
					_pushed = true;
					break;

				case MemberShape.Records:
					Emit(code, held, following, loaded);
					Carried(code, member.Member.Element is null
						? machine.Carrier.PushRecord(slot, RuleOfSlot(slot))
						: machine.Carrier.PushTagged(slot));
					_pushed = true;
					break;

				default:
					throw new InvalidOperationException(
						$"A {member.Shape} capture passed CanRead and the reader cannot keep it.");
			}

			if (member.Shape != MemberShape.Records)
				_kept.Add(slot);
		}

		/// <summary>The slots some record or guard of the rule reads, wherever it stands.</summary>
		HashSet<int> ReadAnywhere => _readAnywhere ??= Elsewhere(new Node.Empty());

		HashSet<int>? _readAnywhere;

		/// <summary>Where the rule's pushes begin: this method's own mark in the body, the handed one in a part.</summary>
		string Refs => _part && _gathers ? "refs" : "rb";

		/// <summary>
		/// The one of a member's slots that was written: the same name in two alternatives is
		/// one member with a slot per alternative, and the record takes whichever is set.
		/// </summary>
		static string First(string test, string take, IReadOnlyList<int> slots)
		{
			if (slots.Count == 1)
				return $"{take}{slots[0]}";

			var chain = "-1";

			for (var i = slots.Count - 1; i >= 0; i--)
				chain = $"{test}{slots[i]} >= 0 ? {take}{slots[i]} : {chain}";

			return $"({chain})";
		}

		/// <summary>The rule's record: which arm wrote it, and each member the factory names.</summary>
		void EmitRecord(Writer code, int factory)
		{
			// An alternative that only hands its operand up writes no record of its own, and
			// the operand's is the value: for a folded rule that is what the step after it
			// builds on, so the local still moves.
			if (factory >= 0 && machine.ForwardsInPlace(owner, factory))
			{
				if (_folds)
					Carried(code, machine.Carrier.Accumulated(owner));

				return;
			}

			_records = true;

			Carried(code, machine.Carrier.Begin(
				owner, factory,
				_positions ? (_part ? "start" : "pos") : null,
				_positions ? "p" : null));

			// A fold step's first member is the value so far, and each of the rest is the
			// one thing the step captured (§4.3).
			if (machine.IsStep(owner, factory))
				Carried(code, machine.Carrier.PutAccumulator());

			foreach (var member in machine.DirectMembers(owner, factory))
				Carried(code, member.Shape switch
				{
					MemberShape.Text    => machine.Carrier.PutText(
						member, First("a", "a", member.Slots), First("a", "b", member.Slots),
						_guardTexts.TryGetValue(member.Member.Name, out var cut) ? cut : null),
					MemberShape.Pieces  => machine.Carrier.Collect(member, Refs, true),
					MemberShape.Records => machine.Carrier.Collect(member, Refs, member.Member.Element is not null),
					_                   => machine.Carrier.PutRecord(
						member, machine.Carrier.FirstRecord(member.Slots, member.Member.Rule!)),
				});

			var built = machine.Carrier.End("rb");

			if (!_folds)
			{
				Carried(code, built);
				return;
			}

			var accumulated = machine.Carrier.Accumulated(owner);
			var merged      = MergeIntoAccumulated(built, accumulated);

			if (merged is not null)
			{
				Carried(code, merged);
				return;
			}

			Carried(code, built);
			Carried(code, accumulated);
		}

		/// <summary>
		/// Where a fold step's own record ends in a plain "<c>into = expr;</c>" and the carrier's
		/// <see cref="ValueCarrier.Accumulated"/> would only copy that same local into
		/// <c>fold</c> right after ("<c>fold = into;</c>"), the two are written as one chained
		/// assignment instead ("<c>fold = into = expr;</c>"): <c>into</c> — the type's shared
		/// "last" register (Machine.Carrier.cs, <c>Register</c>) — still ends the turn holding
		/// what it read, current for whatever else reads it, and <c>fold</c> still gets the same
		/// value, but the emitter no longer writes the value into a local Roslyn compiles, only
		/// to copy it nowhere else the very next line.
		/// </summary>
		/// <remarks>
		/// A record wrapped for <c>Unbuilding</c> ("<c>if (unbuilt == 0) into = expr; else { … }</c>")
		/// keeps its own two lines: merging inside a conditional's single statement risks
		/// misreading the wrapper for a shape it is not, and the two-line form there was never
		/// the copy this exists to remove (Accumulated only ever runs when the record was built,
		/// so the "then" branch is the only one asking, and the plain shape below already covers
		/// every grammar measured so far).
		/// </remarks>
		static string? MergeIntoAccumulated(string built, string accumulated)
		{
			if (accumulated.Length == 0 || !built.EndsWith(";", StringComparison.Ordinal) || built.Contains("if (unbuilt"))
				return null;

			var eq = built.IndexOf(" = ", StringComparison.Ordinal);
			if (eq < 0)
				return null;

			var target = built.Substring(0, eq);

			// The exact shape base Carrier.Accumulated produces (Machine.Carrier.cs) for this
			// same target; a carrier override with different text (Tape's own, which reads a
			// log rather than a bare local) is left with its own two lines, unmerged.
			if (accumulated != $"fold = {target};")
				return null;

			var expr = built.Substring(eq + 3, built.Length - eq - 3 - 1);

			return $"fold = {target} = {expr};";
		}

		readonly HashSet<int> _kept = [];

		void EmitLiteral(Writer code, Node node, string text, bool folded, bool loaded = false)
		{
			if (text.Length == 0)
				return;

			var name    = machine.DeclareExpected(machine.Displays(node));
			var closing = _closing;

			_closing = null;

			// A literal of a run records nothing of its own, and the last of them refuses for
			// the run (RefusedRun).
			if (closing is not null || ReferenceEquals(node, _quiet))
			{
				var mismatch = text.Length == 1
					? $"{machine.ReadAt("p")} != {CSharpEmitter.Char(text[0])}"
					: machine.BufferedBytes
					? $"!text.Matches(p, {Quoted(text)})"
					: $"!global::System.MemoryExtensions.SequenceEqual(text.Slice(p, {text.Length}), {Spanned(text)})";

				using (code.Block($"if ({machine.LacksRoom(text.Length)} || {mismatch})"))
				{
					if (closing is not null)
						RefusedRun(code, closing);
					else
						code.Line(Fails());
				}

				code.Line($"p += {text.Length};");

				return;
			}

			if (text.Length == 1)
			{
				// The token the dispatch read is the one this wants, and is in a register:
				// there is nothing to bound and nothing to read. And where the case label that
				// brought the reading here admits nothing but this token, there is nothing to
				// test either — the switch was the test.
				if (loaded && Chosen(FirstSets.Of(node, _graph)))
				{
					code.Line($"p += {text.Length};");

					return;
				}

				var test = CSharpEmitter.Differs(loaded ? "c" : machine.ReadAt("p"), text[0], folded);
				var room = loaded ? "" : $"{machine.Past("p")} || ";

				if (loaded)
					_character = true;

				using (code.Block($"if ({room}{test})"))
					Refused(code, name);

				code.Line($"p += {text.Length};");

				return;
			}

			// Bytes compare with a literal written in characters one by one, which the buffer does
			// where it holds them (byte literals are case-sensitive values below 256).
			// A span made and dropped inside the one comparison: nothing fills between (the span
			// contract at Machine.Cut). An ASCII literal is read from one slice, as the engine
			// does (Machine.CompileUnguarded); beyond ASCII each character is tested for what it
			// accepts, with the characters in place.
			var comparison = machine.BufferedBytes
				? $"!text.Matches(p, {Quoted(text)})"
				: folded && !Ascii(text)
				? string.Join(" || ", text.Select((one, i) => CSharpEmitter.Differs(machine.ReadAt(i == 0 ? "p" : $"p + {i}"), one, true)))
				: folded
				? machine.FoldedDiffers($"text.Slice(p, {text.Length})", text)
				: $"!global::System.MemoryExtensions.SequenceEqual(text.Slice(p, {text.Length}), {Spanned(text)})";

			// As the engine answers, so that the renderings of one grammar name one place: input
			// that ends inside the literal is refused where it began and says the input ran out;
			// a character that does not fit is refused where it is, the literal's matching
			// beginning read on the failing branch alone (Machine.Sharpen).
			using (code.Block($"if ({machine.LacksRoom(text.Length)})"))
			{
				code.Line("if (p + 1 > failure.OutOfInput) failure.OutOfInput = p + 1;");
				Refused(code, name);
			}

			using (code.Block($"if ({comparison})"))
			{
				Sharpened(code, text, folded);
				Refused(code, name);
			}

			code.Line($"p += {text.Length};");
		}

		/// <summary>The texts of the run the literal being written closes, where it closes one.</summary>
		List<string>? _closing;

		/// <summary>The literal this part is, where it is one of a run and so records nothing.</summary>
		Node? _quiet;

		/// <summary>Whether alternative <paramref name="at"/> is one of the run <paramref name="closing"/> names.</summary>
		static bool Quiet(List<string>? closing, IReadOnlyList<Node> alternatives, int at)
		{
			return closing is not null && at >= alternatives.Count - closing.Count;
		}

		/// <summary>
		/// The texts of the alternatives that end a choice, where they are two or more literals
		/// never come back to — the run the engine compiles as one (Machine.CompileLiterals).
		/// </summary>
		List<string>? Closing(IReadOnlyList<Node> alternatives, FollowSets.Continuation following)
		{
			var run = LiteralRun(alternatives, alternatives.Count - 1, following.Taught.Plain);

			if (run == 0)
				return null;

			var texts = new List<string>(run);

			foreach (var node in alternatives.Skip(alternatives.Count - run))
				if (node is Node.Literal(var text))
					texts.Add(text);

			return texts;
		}

		/// <summary>
		/// The refusal of a run of literals, written where the last of them has failed: at the
		/// deepest character any of them agreed with, naming the ones still agreeing there,
		/// as the engine places it. The literals themselves record nothing (<see cref="_quiet"/>),
		/// so what the message says, and in what order, is what the engine's does.
		/// </summary>
		void RefusedRun(Writer code, List<string> texts)
		{
			var displays = texts.Select(text => machine.Display(new Node.Literal(text))).ToList();
			var all      = machine.DeclareExpected(displays);
			var shared   = texts[0];

			foreach (var text in texts)
			{
				var common = 0;

				while (common < shared.Length && common < text.Length && shared[common] == text[common])
					common++;

				shared = shared.Substring(0, common);
			}

			// Input that ends inside what they all begin with ran out where the run began.
			if (shared.Length > 1)
				using (code.Block($"if ({machine.LacksRoom(shared.Length)})"))
				{
					code.Line("if (p + 1 > failure.OutOfInput) failure.OutOfInput = p + 1;");
					Refused(code, all);
				}

			var narrowed = $"narrowed{_calls++}";

			machine._expectedUsed.Add(all);

			code.Line($"string[]? {narrowed} = {all};");

			if (machine.Quiets)
				code.Line("if (!failure.Quiet)");

			code.Then($"p = {machine.Sharpening(texts, displays)}(text, p, ref {narrowed});");
			code.Line(Refusal(narrowed));
			code.Line(Fails());
		}

		/// <summary>
		/// Moves <c>p</c> to the character of <paramref name="text"/> that did not fit, on a branch
		/// already refusing — and only where the refusal is recorded: a quiet one is not read.
		/// </summary>
		void Sharpened(Writer code, string text, bool folded)
		{
			// The helper folds as ordinal folding does for an ASCII literal; one beyond ASCII is
			// rare enough to have its ladder written out, testing what each character accepts.
			if (folded && !Ascii(text))
			{
				if (machine.Quiets)
				{
					using (code.Block("if (!failure.Quiet)"))
						machine.Sharpen(code, text, true);
				}
				else
				{
					machine.Sharpen(code, text, true);
				}

				return;
			}

			if (machine.Quiets)
				code.Line("if (!failure.Quiet)");

			var step = $"p = {machine.Agreeing()}(text, p, {Quoted(text)}, {(folded ? "true" : "false")});";

			if (machine.Quiets)
				code.Then(step);
			else
				code.Line(step);
		}

		/// <summary>
		/// Whether every token the enclosing switch could have chosen this group by is one the
		/// set admits — so that a test against the set, made on that token, could only pass.
		/// </summary>
		bool Chosen(FirstSets.First admits)
		{
			return _dispatched is { } chosen && chosen.IsKnown && admits.IsKnown && admits.Covers(chosen);
		}

		void EmitElement(Writer code, Node.Element element, bool loaded = false)
		{
			var name  = machine.DeclareExpected(machine.Displays(element));
			var first = FirstSets.Of(element, _graph);

			_character = true;

			if (!loaded)
			{
				using (code.Block($"if ({machine.Past("p")})"))
					Refused(code, name);

				code.Line($"c = {machine.ReadAt("p")};");
			}

			var test = CSharpEmitter.Test(element, machine.Tabulate);

			// Not where the case label that brought the reading here admits nothing the
			// element would refuse: the switch was the test.
			if (!string.Equals(test, "true", StringComparison.Ordinal) && !(loaded && Chosen(first)))
				using (code.Block($"if (!({test}))"))
					Refused(code, name);

			code.Line("p++;");
		}

		void EmitCall(Writer code, Node call, RuleSymbol called)
		{
			var result = $"q{_calls++}";

			// The strength the operand is read at: what `<<` or `>>` recorded against this
			// call, everything where nothing was recorded, and for the entry's own call of
			// the rule it reads through, whatever the entry was asked.
			var strength = _entry && ReferenceEquals(called, owner) && _graph.Climbing.ContainsKey(called)
				? ", power"
				: machine.DirectStrengthOf(call, called);

			// Where the value this call reads is built otherwise than the reading around it
			// is — nobody asks for it, or a guard asks for it inside a reading nobody asks the
			// value of — the carrier says so around the call (Demand). Not around the entry's
			// own call of the rule it reads through: that call is the reading, and what the
			// reading asks of its value was decided by the entry that began it (Failure.Unasked).
			var around = call is Node.Call asked && !(_entry && ReferenceEquals(called, owner))
				? machine.Carrier.AroundCall(owner, asked, result)
				: null;

			if (around is { } before)
				code.Line(before.Before);

			// What was recorded here before the call, for a refusal said over the call's own
			// (Refuse_DotGram_Over): nothing where the furthest refusal stood short of here, and
			// otherwise how many sets had tied here, those after them being the call's.
			var stood = _refuseWith is { } refusing && _refuseOver is { } over && over != refusing ? $"f{result}" : null;

			if (stood is not null)
				code.Line($"var {stood} = failure.Position < p ? -1 : failure.ExpectedMore?.Count ?? 0;");

			code.Line(
				$"var {result} = {machine.ReaderOf(called)}(p{strength});");

			if (around is { } after)
				code.Line(after.After);

			// What the rule says about its own refusal (§4's `on fail`), where the refusal is
			// the rule's own: it was entered here and read nothing, which is what the furthest
			// position not having passed this call says. A rule that got further in failed at
			// something it wanted, and what it wanted there is the better answer.
			if (machine.SaysOf(called) is { } spoken)
			{
				var said = machine.DeclareExpected([Spoken(spoken)]);

				machine._expectedUsed.Add(said);

				code.Line(
					$"if ({result} < 0 && failure.Position <= p{(machine.Quiets ? " && !failure.Quiet" : "")}) " +
					$"{Refusing}(ref failure, p, {said});");
			}

			if (_refuseWith is { } expected)
			{
				_refuseWith = null;

				using (code.Block($"if ({result} < 0)"))
				{
					if (_refuseOver is { } covered && covered != expected)
					{
						machine._expectedUsed.Add(expected);
						machine._expectedUsed.Add(covered);

						if (_refuseUnless is var (guard, without))
						{
							_refuseUnless = null;

							RefusedUnless(code, expected, guard, without, (covered, stood!));
						}
						else
						{
							code.Line(machine.Quiets
								? $"if (!failure.Quiet) {Refusing}_Over(ref failure, p, {expected}, {covered}, {stood});"
								: $"{Refusing}_Over(ref failure, p, {expected}, {covered}, {stood});");
							code.Line(Fails());
						}
					}
					else if (_refuseUnless is var (guard, without))
					{
						_refuseUnless = null;

						RefusedUnless(code, expected, guard, without);
					}
					else
					{
						Refused(code, expected);
					}
				}

				_refuseOver = null;
			}
			else
			{
				code.Line($"if ({result} < 0) {Fails()}");
			}

			code.Line($"p = {result};");
		}

		int _calls;

		/// <summary>
		/// Alternatives in order: the one the token chooses where a token chooses, and one
		/// attempt after another where none does.
		/// </summary>
		/// <remarks>
		/// Where the choice dispatches, an alternative that fails is a choice that fails —
		/// no other could have matched here — so each is written where it stands and ends
		/// the reader. Where it does not, every alternative but the last becomes a method,
		/// because <c>-1</c> is how one tells its caller to try the next — or, where it is
		/// one call to a rule and nothing more, is that call, which answers the same way.
		/// </remarks>
		void EmitChoice(Writer code, IReadOnlyList<Node> alternatives, FollowSets.Continuation following)
		{
			if (alternatives.Count == 1)
			{
				Emit(code, alternatives[0], following);

				return;
			}

			// Before the switch: a seam in front of every alternative puts its characters in
			// every first set, and a switch on them reads all the alternatives in one group, in
			// order, with a way into them.
			if (machine.PastTheSeam(alternatives) is { } seamed)
			{
				EmitSeamChain(code, alternatives, seamed, following);

				return;
			}

			if (machine.Dispatchable(alternatives, least: 2) is { } groups)
			{
				var name = machine.DeclareExpected(machine.PredictedDisplays(alternatives));

				// A group that is one alternative led by a guard is named in a refusal only where
				// its guard would let it read: tried in order, that alternative would have asked
				// and been told no, and said nothing. Asked on the way out only, where one such
				// group stands; with more, each would want its own set, and all are named.
				var asked = Asked(alternatives, groups);

				_character = true;

				using (code.Block($"if ({machine.Past("p")})"))
				{
					if (asked is { } past)
						RefusedUnless(code, name, past.Guard, past.Without);
					else
						Refused(code, name);
				}

				code.Line($"c = {machine.ReadAt("p")};");

				// The widest group is the one the jump table exists for: name its characters
				// and the compiler builds a table spanning them, an indirect branch the
				// processor guesses at. Where that group is one alternative whose reading
				// begins with a call, it needs no labels — the call tests the character
				// itself, and a character no group holds is a character that call refuses.
				// What is left is a handful of labels the compiler compares in a line.
				var widest = Widest(groups);
				var apart  = new PushedApart(this);

				using (code.Block("switch (c)"))
				{
					for (var g = 0; g < groups.Count; g++)
					{
						if (g == widest)
							continue;

						var group  = groups[g];
						var labels = "";

						foreach (var range in group.Set.Ranges)
							for (var one = range.From; ; one++)
							{
								labels += $"case {CSharpEmitter.Char(one)}: ";

								if (one == range.To)
									break;
							}

						code.Line(labels);

						using (code.Indent())
						using (code.Block(""))
						{
							// A group is what one token cannot tell apart. Reaching it is a
							// jump table; inside it there is nothing to tell the members
							// apart by, so they are tried in order — and a group that fails
							// fails the choice, no other group's first set holding the token
							// that chose this one.
							// A guard the group begins with refuses where the token was read, and
							// what the other groups would have taken there is what the choice
							// wanted: tried in order, their refusals would have said so.
							var refused = _guardRefuses;
							_guardRefuses = GuardRefuses(alternatives, group.Members);
							_dispatched = group.Set;
							apart.Next();
							EmitAmong(code, group.Members, following, loaded: true);
							_dispatched = null;
							_guardRefuses = refused;
							code.Line("break;");
						}
					}

					code.Line("default:");

					if (widest < 0)
					{
						using (code.Indent())
						{
							if (asked is { } other)
								RefusedUnless(code, name, other.Guard, other.Without);
							else
								Refused(code, name);
						}
					}
					else
					{
						using (code.Indent())
						using (code.Block(""))
						{
							// The call that begins it answers for the characters no group
							// holds, and says so in its own words. The choice adds what it
							// wanted here, so that the message is no poorer than the one the
							// labels would have given: recorded at the position the call was
							// made from, it merges with the call's refusal where the call
							// refused there and is dropped where it read on.
							// And where the call refused right there, what it said is the widest
							// group's own first set, which the choice's set holds: said once, in
							// the choice's words, as the engine says it.
							_refuseWith = name;
							_refuseOver = machine.DeclareExpected(machine.Displays(
								new Node.Element(false, [.. groups[widest].Set.Ranges], [], [])));
							_refuseUnless = asked;
							apart.Next();
							Emit(code, groups[widest].Members[0], following);
							_refuseWith = null;
							_refuseOver = null;
							_refuseUnless = null;
							code.Line("break;");
						}
					}
				}

				apart.Done();

				return;
			}

			if (machine.Chainable(alternatives) is { } chain)
			{
				EmitChain(code, alternatives, chain, following);

				return;
			}

			if (machine.SpansAKindTable(alternatives) &&
				machine.Dispatchable(alternatives, least: 2, named: Machine.KindTableSize) is { } wide &&
				machine.KindTable(wide) is { } table)
			{
				EmitGroupTable(code, alternatives, wide, table, following);

				return;
			}

			EmitAmong(code, alternatives, following);
		}

		/// <summary>
		/// Groups too wide for a switch to name their characters, found by a table: the switch is
		/// on the group the table gives, and inside a group its members are tried in order, as
		/// inside a group of the switch.
		/// </summary>
		/// <remarks>
		/// What a token path's choices among names are: `CAST (…)`, `a0` and `f(x)` begin with
		/// kinds that overlap, and all the kinds that are not reserved words begin a name. Tried in
		/// order, each alternative was a call that refused a token its first set does not hold.
		/// Found by the table, only the alternatives that can begin with the token are tried, and
		/// a token none of them begins with refuses the choice at once.
		/// </remarks>
		void EmitGroupTable(
			Writer code, IReadOnlyList<Node> alternatives, List<(FirstSets.First Set, List<Node> Members)> groups,
			(string Name, int From) table, FollowSets.Continuation following)
		{
			var name  = machine.DeclareExpected(machine.PredictedDisplays(alternatives));
			var asked = Asked(alternatives, groups);

			_character = true;

			using (code.Block($"if ({machine.Past("p")})"))
				RefusedAsking(code, name, asked);

			code.Line($"c = {machine.ReadAt("p")};");

			// Two choices share a table where its bytes are the same, each from its own start.
			var at    = table.From == 0 ? "c" : $"c - {table.From}";
			var apart = new PushedApart(this);

			using (code.Block($"switch ((uint)({at}) < (uint){table.Name}.Length ? {table.Name}[{at}] : 0)"))
			{
				for (var g = 0; g < groups.Count; g++)
				{
					code.Line($"case {g + 1}:");

					using (code.Indent())
					using (code.Block(""))
					{
						var refused = _guardRefuses;
						_guardRefuses = GuardRefuses(alternatives, groups[g].Members);
						_dispatched   = groups[g].Set;
						apart.Next();
						EmitAmong(code, groups[g].Members, following, loaded: true);
						_dispatched   = null;
						_guardRefuses = refused;
						code.Line("break;");
					}
				}

				code.Line("default:");

				using (code.Indent())
					RefusedAsking(code, name, asked);
			}

			apart.Done();
		}

		/// <summary>
		/// The guard a dispatched choice asks before a refusal names the group it leads: where one
		/// group is a single alternative led by a guard, that guard, and what the choice wanted
		/// without it. Asked on the way out only; with more such groups, each would want its own
		/// set, and all are named.
		/// </summary>
		(Node.Guard Guard, string Without)? Asked(
			IReadOnlyList<Node> alternatives, List<(FirstSets.First Set, List<Node> Members)> groups)
		{
			var guarded = groups.Where(one => one.Members.Count == 1 && LeadingGuard(one.Members[0]) is not null).ToList();

			return guarded.Count == 1
				? (LeadingGuard(guarded[0].Members[0])!, machine.DeclareExpected(machine.PredictedDisplays(
					alternatives.Where(one => !ReferenceEquals(one, guarded[0].Members[0])).ToList())))
				: null;
		}

		/// <summary>The refusal of a dispatched choice, asking its guarded group's guard where there is one (<see cref="Asked"/>).</summary>
		void RefusedAsking(Writer code, string name, (Node.Guard Guard, string Without)? asked)
		{
			if (asked is { } one)
				RefusedUnless(code, name, one.Guard, one.Without);
			else
				Refused(code, name);
		}

		/// <summary>
		/// What a guard beginning a dispatched group refuses with: the first tokens of the other
		/// groups, which tried in order would have said what they wanted. Null where no member
		/// begins with a guard.
		/// </summary>
		string? GuardRefuses(IReadOnlyList<Node> alternatives, List<Node> members)
		{
			return members.Any(LeadsWithGuard)
				? machine.DeclareExpected(machine.PredictedDisplays(
					alternatives.Where(one => !members.Contains(one)).ToList()))
				: null;
		}

		/// <summary>
		/// Alternatives the first character tells apart, but too widely for a switch to name:
		/// a test for each but the widest, narrowest first, and the widest as what is left.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <c>(Plain | Escape)*</c> with <c>Plain = [^ '"' | '\\']</c> is one comparison with
		/// <c>'\\'</c> a character, and not a test of the complement: the sets are disjoint, so a
		/// character that is not <c>'\\'</c> can only begin <c>Plain</c>, and <c>Plain</c> says
		/// itself what it does not take. Nothing is put on the tape, since no alternative but
		/// the one the character chose could have begun here — the way the ordered reading
		/// opened led nowhere.
		/// </para>
		/// <para>
		/// The widest is read as the switch's <c>default:</c> is read, where it begins with a
		/// call: the call refuses what it does not begin with, and the choice adds what it
		/// wanted. Otherwise it is tested like the others, and the choice refuses as one.
		/// </para>
		/// </remarks>
		/// <summary>
		/// Alternatives that all begin with the seam, told apart by the character after it: the
		/// seam read once to look, the position put back, and the alternative the character chose
		/// read whole (<see cref="Machine.PastTheSeam"/>).
		/// </summary>
		/// <remarks>
		/// A character no alternative begins with past the seam refuses the choice there, where
		/// each alternative in turn would have refused it after reading the same seam. Nothing is
		/// put on the tape: past the seam only one alternative can begin.
		/// </remarks>
		void EmitSeamChain(
			Writer code, IReadOnlyList<Node> alternatives, List<(FirstSets.First Set, Node Node)> chain,
			FollowSets.Continuation following)
		{
			var name  = machine.DeclareExpected(machine.PredictedDisplays(alternatives));
			var began = $"k{_ways++}";

			code.Line($"var {began} = p;");

			// The seam alone, as the first part of any of them reads it.
			Emit(code, ((Node.Sequence)Machine.Bare(chain[0].Node)).Nodes[0], following);

			_character = true;

			using (code.Block($"if ({machine.Past("p")})"))
				Refused(code, name);

			code.Line($"c = {machine.ReadAt("p")};");

			var apart = new PushedApart(this);

			for (var i = 0; i < chain.Count; i++)
			{
				code.Line($"{(i == 0 ? "if" : "else if")} ({machine.RangesTest(chain[i].Set.Ranges, machine.Tabulate)})");

				using (code.Block(""))
				{
					code.Line($"p = {began};");
					apart.Next();
					Emit(code, chain[i].Node, following);
				}
			}

			apart.Done();

			code.Line("else");

			using (code.Block(""))
				Refused(code, name);
		}

		void EmitChain(
			Writer code, IReadOnlyList<Node> alternatives, List<(FirstSets.First Set, Node Node)> chain,
			FollowSets.Continuation following)
		{
			var name = machine.DeclareExpected(machine.PredictedDisplays(alternatives));

			_character = true;

			using (code.Block($"if ({machine.Past("p")})"))
				Refused(code, name);

			code.Line($"c = {machine.ReadAt("p")};");

			var apart = new PushedApart(this);

			for (var i = 0; i < chain.Count - 1; i++)
			{
				code.Line($"{(i == 0 ? "if" : "else if")} ({machine.RangesTest(chain[i].Set.Ranges, machine.Tabulate)})");

				using (code.Block(""))
				{
					_dispatched = chain[i].Set;
					apart.Next();
					Emit(code, chain[i].Node, following, loaded: true);
					_dispatched = null;
				}
			}

			var (widest, last) = chain[chain.Count - 1];

			code.Line("else");

			using (code.Block(""))
			{
				apart.Next();

				if (Leads(last) is { } call && machine.Decidable(call) is { Ends: false })
				{
					_refuseWith = name;
					Emit(code, last, following);
					_refuseWith = null;
				}
				else
				{
					using (code.Block($"if (!({machine.RangesTest(widest.Ranges, machine.Tabulate)}))"))
						Refused(code, name);

					_dispatched = widest;
					Emit(code, last, following, loaded: true);
					_dispatched = null;
				}
			}

			apart.Done();
		}

		/// <summary>
		/// The group to write as <c>default:</c>: the widest, where its one alternative is
		/// read by a call before anything else is read at all.
		/// </summary>
		/// <remarks>
		/// Soundness is the call's. A character reaching <c>default:</c> begins no
		/// alternative, so it begins neither this one nor — the call being first and reading
		/// something — the rule the call names, which therefore refuses it. Anything that
		/// could match before the call, a look for one, would leave that argument short.
		/// </remarks>
		int Widest(List<(FirstSets.First Set, List<Node> Members)> groups)
		{
			var found = -1;
			var width = 0;

			for (var i = 0; i < groups.Count; i++)
			{
				var wide = 0;

				foreach (var range in groups[i].Set.Ranges)
					wide += range.To - range.From + 1;

				if (wide > width)
				{
					found = i;
					width = wide;
				}
			}

			return found >= 0 && groups[found].Members.Count == 1 &&
				Leads(groups[found].Members[0]) is { } call &&
				machine.Decidable(call) is { Ends: false }
					? found
					: -1;
		}

		/// <summary>The call a part reads before it reads anything else, where it is one.</summary>
		static Node.Call? Leads(Node part)
		{
			while (true)
				switch (part)
				{
					case Node.Call call:
						return call;

					case Node.Capture(_, var held):
						part = held;
						break;

					case Node.Construct(var built, _):
						part = built;
						break;

					case Node.Sequence(var parts) when parts.Count > 0:
						part = parts[0];
						break;

					default:
						return null;
				}
		}

		/// <summary>
		/// What the next call is to record when it refuses, over what the rule it names
		/// recorded: set while the group written as <c>default:</c> is being written.
		/// </summary>
		/// <summary>The line an entry writes between the trivia it begins with and its rule.</summary>
		string? _leadMark;

		public void MarkLead(string line)
		{
			_leadMark = line;
		}

		string? _refuseWith;

		/// <summary>The call's own first set beside <see cref="_refuseWith"/>, which that one holds.</summary>
		string? _refuseOver;

		/// <summary>A repetition about to be written, the one character after it, and what follows that.</summary>
		(Node.Repeat Repeat, char Stop, FollowSets.Continuation Beyond)? _stopAfter;

		/// <summary>
		/// Alternatives with nothing to tell them apart by: one attempt after another.
		/// </summary>
		/// <remarks>
		/// Each but the last is a method, so that its failure is a number rather than a
		/// jump out of the middle of this one. Called at the top of a choice nothing can
		/// dispatch, and inside a group of one that can.
		/// </remarks>
		/// <summary>
		/// The token the enclosing <c>switch</c> chose this group by, while the group is being
		/// written: an alternative whose first test admits exactly those tokens has nothing to
		/// test, the case label having done it.
		/// </summary>
		FirstSets.First? _dispatched;

		void EmitAmong(
			Writer code, IReadOnlyList<Node> alternatives, FollowSets.Continuation following, bool loaded = false)
		{
			if (alternatives.Count == 1)
			{
				Emit(code, alternatives[0], following, loaded);

				return;
			}

			var tried = $"q{_calls++}";

			// Where the token in hand can begin none of the alternatives, the choice is
			// refused here and as one thing — the expectation the rendering beside this one
			// reports — rather than alternative by alternative, each recording its own
			// refusal at the same place and the message listing them all.
			// Not where the case label that brought the reading here admits nothing the door
			// would refuse: the switch was the door.
			if (alternatives[alternatives.Count - 1] is not Node.Empty &&
				Doorway(alternatives) is { } gate && !(loaded && Chosen(gate)))
			{
				var whole = machine.DeclareExpected(machine.PredictedDisplays(alternatives));

				if (!loaded)
				{
					using (code.Block($"if ({machine.Past("p")})"))
						Refused(code, whole);

					code.Line($"c = {machine.ReadAt("p")};");
					code.Line();
				}

				_character = true;

				using (code.Block($"if (!({machine.RangesTest(gate.Ranges, machine.Tabulate)}))"))
					Refused(code, whole);

				code.Line();
			}

			// A choice that is one run of text, where no shorter text can be wanted by what
			// follows (LiteralRun, PrefixSettled), has nothing to come back to on the tape
			// either: it is tried in order below and opens no way, as the engine compiles it.
			// `eol` is that choice wherever what follows it cannot begin with '\n'. And so is a
			// choice none of whose alternatives can hold where another held (Exclusive):
			// `(eol | ?=eof)`, whose lookahead cannot hold where `eol` read a character. And a
			// run of text the engine's run does not take (SettledText): texts built from, and
			// texts that ignore case.
			if (_tape && LiteralRun(alternatives, alternatives.Count - 1, following.Taught.Plain) != alternatives.Count &&
				!machine.Exclusive(alternatives) && !machine.SettledText(alternatives, following.Taught.Plain))
			{
				if (analyzing)
					machine.OpeningChoice(owner, alternatives, following);

				EmitChoiceOverCharacters(code, alternatives, tried, following);

				return;
			}

			// An optional is a choice whose last alternative is nothing, and trying the
			// others where the token in hand cannot begin any of them is a refusal for
			// each — recorded, and on a tie between them allocated — for a reading that was
			// always going to be the empty one. So the token is looked at first, which is
			// the door the rendering beside this one has always had.
			var door = alternatives[alternatives.Count - 1] is Node.Empty
				? Door(alternatives.Take(alternatives.Count - 1))
				: null;

			IDisposable? opened = null;

			if (door is not null)
			{
				// A door that does not open is still what the rule wanted here, and says so
				// — the rendering beside this one records the first test of the alternative
				// it did not take, and a message that leaves it out is a worse message —
				// but it is a note and not a failure: the reading goes on with nothing.
				var wanted = machine.DeclareExpected(
					machine.PredictedDisplays(alternatives.Take(alternatives.Count - 1).ToList()));

				if (!loaded)
				{
					code.Line($"if ({machine.Past("p")})");
					code.Then(Noted(wanted));
					code.Line("else");

					var outer = code.Block("");

					code.Line($"c = {machine.ReadAt("p")};");
					code.Line();
					code.Line($"if (!({door}))");
					code.Then(Noted(wanted));
					code.Line("else");

					opened = new Both(outer, code.Block(""));
				}
				else
				{
					code.Line($"if (!({door}))");
					code.Then(Noted(wanted));
					code.Line("else");

					opened = code.Block("");
				}
			}

			// Where the last alternatives are all text, the last of them refuses for all of
			// them where they have all failed (RefusedRun).
			var closing = Closing(alternatives, following);
			var apart   = new PushedApart(this);

			for (var i = 0; i < alternatives.Count - 1; i++)
			{
				apart.Next();

				var (part, undo, _) = Called(alternatives[i], following, Quiet(closing, alternatives, i));

				// The first attempt is made; each after it, only where the one before failed.
				using (i == 0 ? null : code.Block($"if ({tried} < 0)"))
				{
					// What an alternative that failed wrote is not the rule's: what it
					// pushed, the record written after it would collect, and what it
					// logged would size the value tables.
					var back   = _gathers || _logs ? _ways++ : -1;
					var marks  = new List<string>();
					var unwind = new List<string>();

					if (_gathers && back >= 0)
					{
						marks.AddRange(machine.Carrier.MarkGathered(owner, $"rr{back}"));
						unwind.AddRange(machine.Carrier.UnwindGathered(owner, $"rr{back}"));
					}

					if (_logs && back >= 0)
					{
						marks.AddRange(machine.Carrier.MarkRecords($"lm{back}"));
						unwind.AddRange(machine.Carrier.UnwindRecords($"lm{back}"));
					}

					if (undo.Length > 0)
						unwind.Add(undo);

					foreach (var line in marks)
						code.Line(line);

					if (marks.Count > 0)
						code.Line();

					code.Line(i == 0 ? $"var {tried} = {part};" : $"{tried} = {part};");

					if (unwind.Count > 0)
					{
						code.Line();

						using (code.Block($"if ({tried} < 0)"))
							foreach (var line in unwind)
								code.Line(line);
					}
				}
			}

			using (code.Block($"if ({tried} < 0)"))
			{
				// The one written in place, and the only one that can still use the token
				// the dispatch read: what came before it was a method, which reads its own.
				_closing = closing;
				apart.Next();
				Emit(code, alternatives[alternatives.Count - 1], following, loaded);
				_closing = null;

				code.Line($"{tried} = p;");
			}

			apart.Done();

			code.Line($"p = {tried};");

			opened?.Dispose();
		}

		/// <summary>Two blocks, closed in the order they were opened in reverse.</summary>
		sealed class Both(IDisposable outer, IDisposable inner) : IDisposable
		{
			public void Dispose()
			{
				inner.Dispose();
				outer.Dispose();
			}
		}

		/// <summary>
		/// The test that the token in hand can begin one of the alternatives, or null where
		/// the first sets cannot say.
		/// </summary>
		string? Door(IEnumerable<Node> alternatives)
		{
			if (Doorway(alternatives) is not { } whole)
				return null;

			_character = true;

			return machine.RangesTest(whole.Ranges, machine.Tabulate);
		}

		/// <summary>The tokens that can begin one of the alternatives, or null where the first sets cannot say.</summary>
		FirstSets.First? Doorway(IEnumerable<Node> alternatives)
		{
			FirstSets.First? whole = null;

			foreach (var alternative in alternatives)
			{
				if (machine.Decidable(alternative) is not { Ends: false } first)
					return null;

				whole = whole is null ? first : whole.Or(first);
			}

			return whole;
		}

		/// <summary>
		/// A choice over characters: which alternative was taken is on the tape, so that a
		/// failure after the choice can come back and ask for the next one.
		/// </summary>
		/// <remarks>
		/// The first reading opens a way standing at the first alternative and reaching to
		/// the last; a reading that is a replay finds the way already there and takes what
		/// it says. An alternative that fails where it stands moves the way on itself, so
		/// the run continues into the next without the tape having to be asked again.
		/// </remarks>
		void EmitChoiceOverCharacters(
			Writer code, IReadOnlyList<Node> alternatives, string tried, FollowSets.Continuation following)
		{
			var way  = $"w{_ways}";
			var took = $"d{_ways++}";

			code.Line($"var {way}  = -1;");
			code.Line($"var {took} = 0;");

			using (code.Block("if (ways.Cursor < ways.Count)"))
			{
				code.Line($"{way}  = ways.Cursor;");
				code.Line($"{took} = ways.Items[{way} * 2];");
				code.Line("ways.Cursor++;");
			}

			code.Line("else");

			using (code.Block(""))
				code.Line($"{way} = ways.Open(0, {alternatives.Count - 1});");

			code.Line();
			code.Line($"var {tried} = -1;");

			// Where the last alternatives are all text, they refuse together once they have all
			// failed (RefusedRun).
			var closing = Closing(alternatives, following);
			var apart   = new PushedApart(this);

			for (var i = 0; i < alternatives.Count; i++)
			{
				using (code.Block($"if ({tried} < 0 && {took} <= {i})"))
				{
					// An alternative is asked for every reading it has before the choice
					// moves on. Without this the way the choice stands on is spent while a
					// run inside the alternative still had a shorter reading to give, and
					// that reading becomes unreachable: the tape says the choice has moved
					// past the alternative it was in.
					var segment = _ways++;

					code.Line($"var s{segment}  = ways.Cursor;");
					foreach (var line in machine.Carrier.MarkRecords($"lm{segment}"))
						code.Line(line);
					foreach (var line in machine.Carrier.MarkGathered(owner, $"rr{segment}"))
						code.Line(line);
					code.Line();

					apart.Next();

					var (call, undo, opens) = Called(alternatives[i], following, Quiet(closing, alternatives, i));

					if (analyzing)
						machine.NoteRetrySite(owner, alternatives[i]);

					if (opens)
					{
						var defers  = Defers(RetrySite.Alternative);
						var restore = Restore(segment, undo);

						// Declared here and not with the mark above: whether there is a loop to
						// read it is known only once the alternative has been written.
						if (defers)
							code.Line(DeclareDeferral(segment.ToString()));

						using (code.Block("while (true)"))
						{
							code.Line($"{tried} = {call};");
							code.Line();
							Stood(code, tried, segment, defers, restore);
							code.Line();
							foreach (var line in restore)
								code.Line(line);

							if (!defers || restore.Count > 0)
								code.Line();
							Retried(code, segment, defers);
							code.Line("break;");
						}
					}
					else
					{
						// Nothing under it goes on the tape, so it has one reading and asking
						// for another is a loop that can only go round once.
						code.Line($"{tried} = {call};");
						code.Line();

						using (code.Block($"if ({tried} < 0)"))
						{
							LogBack(code, $"lm{segment}");
							foreach (var line in machine.Carrier.UnwindGathered(owner, $"rr{segment}"))
								code.Line(line);

							if (undo.Length > 0)
								code.Line(undo);
						}
					}

					if (i < alternatives.Count - 1)
					{
						code.Line();
						code.Line($"if ({tried} < 0)");
						code.Then($"ways.Next({way}, {i + 1}, {alternatives.Count - 1});");
					}
				}
			}

			apart.Done();

			code.Line();

			if (closing is not null)
				using (code.Block($"if ({tried} < 0)"))
				{
					// A reading that is a replay asking for another alternative entered the run
					// past its first text, where the engine, which has no way back into a run,
					// is not asked at all: nothing is recorded.
					code.Line($"if ({took} > {alternatives.Count - closing.Count})");
					code.Then(Fails());
					code.Line();
					RefusedRun(code, closing);
				}
			else
			{
				code.Line($"if ({tried} < 0)");
				code.Then(Fails());
			}

			code.Line();
			code.Line($"p = {tried};");
		}

		/// <summary>
		/// One part of a rule as a method, called: the position it reached, or -1.
		/// </summary>
		/// <remarks>
		/// <para>
		/// A method cannot see a local of the method that called it, and the normalizer
		/// makes that matter: alternatives that begin alike are factored, so a head they
		/// share is read — and captured — before the choice, and the record an alternative
		/// writes names something the alternative did not read. Those positions are handed
		/// over, which is the only thing a method has to hand anything over with.
		/// </para>
		/// <para>
		/// Two directions, told apart because the difference is worth an argument. A
		/// position the part only reads goes by value. One the part captures and something
		/// after it reads goes by reference, and is the same local seen from two methods.
		/// Measured against writing the alternative in place, handing them over costs
		/// nothing at either width tried (benchmarks/README.md).
		/// </para>
		/// </remarks>
		string Calling(Node part, FollowSets.Continuation following)
		{
			return Called(part, following).Call;
		}

		/// <summary>
		/// The call, and what has to be put back where it failed.
		/// </summary>
		/// <remarks>
		/// A position handed over by reference is written by the part whether the part goes
		/// on to answer or not, and a part that failed did not capture what it wrote there.
		/// Leaving it is the defect this exists for: the alternative after it writes a
		/// record naming the position, and gets the abandoned one.
		/// </remarks>
		(string Call, string Undo, bool Opens) Called(Node part, FollowSets.Continuation following, bool quiet = false)
		{
			var (captured, used) = Reaches(part);
			var elsewhere        = Elsewhere(part);

			var taken = captured.Where(elsewhere.Contains).OrderBy(static one => one).ToList();
			var given = used.Where(one => !captured.Contains(one)).OrderBy(static one => one).ToList();

			// A part that is one call and nothing else is that call. The method it would be
			// is the call between a position taken and a position returned, and a capture
			// no record outside reads — `p: Predicate => @(p)` is one, its construction
			// having been the identity — and the yardstick's hot path went through three of
			// them a clause. Not for a back edge, which the call as a statement guards, and
			// not in a rule read at a strength, whose alternatives refuse below theirs.
			if (taken.Count == 0 && !_climbs && Bare(part) is { } bare &&
				!machine._backEdges.Contains((owner, bare.Rule)) &&
				machine.Carrier.AroundCall(owner, bare, "q") is null)
			{
				return (
					Framed(bare, machine.ReaderOf(bare.Rule), machine.DirectStrengthOf(bare, bare.Rule)),
					"",
					machine.Opens(part));
			}

			// What the part hands back is a local of this method, so this method declares it.
			foreach (var slot in taken)
				_kept.Add(slot);

			var name  = machine.ReaderOf(owner) + "_Part" + machine._readerPart++;
			var apart = new ReaderWriter(machine, owner, given, taken, _folds, analyzing);

			apart._quiet = quiet ? part : null;
			apart._outer = Open();

			var written = apart.Render(part, following);
			ObservedOpen |= apart.ObservedOpen;
			_pushed      |= apart._pushed;

			Parts.Add((name, Handing(given, taken, "int "), written));

			foreach (var made in apart.Parts)
				Parts.Add(made);

			Cold.UnionWith(apart.Cold);

			var undo = new System.Text.StringBuilder();

			foreach (var slot in taken)
				foreach (var name2 in Names(slot))
					undo.Append(name2[0] == 'r' ? machine.Carrier.ResetRecordLocal(slot, Optional(slot)) : name2 + " = -1;").Append(' ');

			// What it wrote itself, what its own parts wrote, and what the rules it calls
			// were found to write.
			var opens = (analyzing
				? apart.ObservedOpen
				: written.Contains("ways.Open(", StringComparison.Ordinal) ||
					apart.Parts.Exists(one => one.Body.Contains("ways.Open(", StringComparison.Ordinal))) ||
				machine.Opens(part);

			return (
				$"{name}(p{Handing(given, taken, "")})",
				undo.ToString().TrimEnd(),
				opens);
		}

		/// <summary>
		/// The call a part is, where it is one call under captures and nothing more — captures
		/// into locals, that is: one a turn pushes is the work of the turn, and stays a method.
		/// A construction that only hands the operand up is nothing more too, except in a
		/// rule that folds, where the part moves the value so far along.
		/// </summary>
		Node.Call? Bare(Node part)
		{
			while (true)
			{
				if (part is Node.Capture(_, var held))
				{
					if (!Handed(machine._captureSlots[part] - machine._captureOffsets[owner]))
						return null;

					part = held;
				}
				else if (part is Node.Construct(var built, _) && !_folds &&
					machine.ForwardsInPlace(owner, machine._constructs[part]))
				{
					part = built;
				}
				else if (part is Node.Sequence(var nodes) && nodes.Count == 1)
				{
					part = nodes[0];
				}
				else
				{
					return part as Node.Call;
				}
			}
		}

		/// <summary>The positions handed over, as a signature or as a call.</summary>
		string Handing(IReadOnlyList<int> given, IReadOnlyList<int> taken, string type)
		{
			var text = new System.Text.StringBuilder();

			if (_folds)
				foreach (var (carried, name) in machine.Carrier.FoldState(owner))
					text.Append(", ref ").Append(Typed(type, carried)).Append(name);

			// The rule's start, for the records a part writes and the text a guard reads; the
			// body's is its own `pos`.
			if (_positions || _guarded)
				text.Append(", ").Append(type.Length > 0 ? "int start" : _part ? "start" : "pos");

			// And the rule's log mark, for what a guard builds.
			if (_guarded)
			{
				var declared = machine.Carrier.RecordMarks("lmark");
				var passed   = machine.Carrier.RecordMarks(_part ? "lmark" : "lm");

				for (var one = 0; one < declared.Count; one++)
					text.Append(", ").Append(type.Length > 0 ? machine.Carrier.RecordMarkType + " " + declared[one] : passed[one]);
			}

			// The strength the rule was entered at, which every method of it reads.
			if (_climbs)
				text.Append(", ").Append(type).Append("power");

			// Where the rule gathers across turns, what a record collects is everything pushed
			// since the rule began — not since the part did — so the rule's mark is handed on.
			if (_gathers)
				text.Append(machine.Carrier.GatherHanding(owner, type.Length > 0, !_part));

			foreach (var slot in given)
				foreach (var name in Names(slot))
					text.Append(", ").Append(TypeOf(type, slot, name)).Append(name);

			foreach (var slot in taken)
				foreach (var name in Names(slot))
					text.Append(", ref ").Append(TypeOf(type, slot, name)).Append(name);

			return text.ToString();
		}

		/// <summary>
		/// What a handed local is declared as: a position is an <c>int</c> whatever carries the
		/// values, a record is whatever the carrier keeps one in — and nothing at all where
		/// this is the argument list rather than the parameters.
		/// </summary>
		string TypeOf(string type, int slot, string name)
		{
			return name[0] == 'r' ? Typed(type, machine.Carrier.RecordLocalType(RuleOfSlot(slot), Optional(slot))) : type;
		}

		/// <summary>The carrier's type where a type is wanted, nothing where it is not.</summary>
		static string Typed(string type, string carried)
		{
			return type.Length > 0 ? carried : "";
		}

		/// <summary>The rule whose value a slot holds, which is what a carrier is asked about.</summary>
		/// <remarks>
		/// The rule and not the type it was projected to: two rules may build the same type,
		/// and a carrier that keeps a shape per rule has to tell them apart. The tape and the
		/// immediate carrier project it back themselves, which is where the projection
		/// belongs — it is an answer about how they carry, not about what the reader read.
		/// </remarks>
		RuleSymbol RuleOfSlot(int slot)
		{
			return machine.Carrier.ByPlace && machine.RuleAt(owner, slot) is { } read
				? read
				: machine.MemberOfSlot(owner, slot)!.Member.Rule!;
		}

		/// <summary>Whether the member a slot belongs to may be left out, which a carrier may keep a local for differently.</summary>
		bool Optional(int slot)
		{
			return machine.MemberOfSlot(owner, slot)?.Member.IsOptional == true;
		}

		/// <summary>What a position is called: two names where it is a run of text, one where it is a record.</summary>
		IEnumerable<string> Names(int slot)
		{
			switch (machine.MemberOfSlot(owner, slot)?.Shape)
			{
				case MemberShape.Text:
					yield return "a" + slot;
					yield return "b" + slot;
					break;

				case MemberShape.Pieces:
					yield return "a" + slot;
					break;

				case MemberShape.Records:
					break;

				default:
					yield return "r" + slot;
					break;
			}
		}

		/// <summary>
		/// Whether a position is kept in a local that another method could need.
		/// </summary>
		/// <remarks>
		/// What a repetition gathers is pushed onto the tape by the turn that captured it
		/// and collected from the tape by the record, and the tape is shared by every
		/// method of the rule: nothing about it crosses a method boundary in a local, so
		/// nothing about it is handed over.
		/// </remarks>
		bool Handed(int slot)
		{
			return machine.MemberOfSlot(owner, slot)?.Shape is MemberShape.Text or MemberShape.Record;
		}

		/// <summary>What a part captures, and what the records inside it read.</summary>
		(HashSet<int> Captured, HashSet<int> Used) Reaches(Node part)
		{
			var captured = new HashSet<int>();
			var used     = new HashSet<int>();

			foreach (var one in NodeWalk.Descendants(part))
			{
				if (one is Node.Capture &&
					machine._captureSlots[one] - machine._captureOffsets[owner] is var slot && Handed(slot))
				{
					captured.Add(slot);
				}

				// Every slot of a member and not its first: the record takes whichever of a
				// member's slots was written, so it names all of them.
				if (one is Node.Construct && !machine.ForwardsInPlace(owner, machine._constructs[one]))
					foreach (var member in machine.DirectMembers(owner, machine._constructs[one]))
						if (Handed(member.Slots[0]))
							foreach (var named in member.Slots)
								used.Add(named);

				// A guard reads what the rule has captured so far.
				if (one is Node.Guard guard)
					foreach (var (_, slots) in machine.GuardMembers(owner, guard))
						foreach (var named in slots)
							if (Handed(named))
								used.Add(named);
			}

			return (captured, used);
		}

		/// <summary>What the records outside this part read.</summary>
		HashSet<int> Elsewhere(Node part)
		{
			var mine = NodeWalk.ByIdentity(NodeWalk.Descendants(part));
			var used = new HashSet<int>();

			// A rule with no construction writes the record of its captures where it ends,
			// which is outside every part of it.
			if (machine.RecordsAtEnd(owner))
				foreach (var member in machine.DirectMembers(owner, -1))
					if (Handed(member.Slots[0]))
						foreach (var slot in member.Slots)
							used.Add(slot);

			foreach (var one in NodeWalk.Descendants(_graph.Bodies[owner]))
				if (!mine.Contains(one) && one is Node.Construct &&
					!machine.ForwardsInPlace(owner, machine._constructs[one]))
				{
					foreach (var member in machine.DirectMembers(owner, machine._constructs[one]))
						if (Handed(member.Slots[0]))
							foreach (var slot in member.Slots)
								used.Add(slot);
				}
				else if (!mine.Contains(one) && one is Node.Guard guard)
				{
					foreach (var (_, slots) in machine.GuardMembers(owner, guard))
						foreach (var slot in slots)
							if (Handed(slot))
								used.Add(slot);
				}

			return used;
		}

		void EmitRepeat(Writer code, Node.Repeat repeat, FollowSets.Continuation following)
		{
			if (machine._recoveryReads.TryGetValue(repeat, out var recovery))
			{
				EmitRecovering(code, repeat, recovery, following);

				return;
			}

			var (body, min, max) = repeat;

			// What a turn is followed by is another turn, or what follows the loop where
			// this was the last.
			var inside = following.Map(one => new FollowSets.Continuation(
				FirstSets.Of(body, _graph).Or(one.Plain),
				one.AfterSeam));

			// `(?!Separator & any)*` before a stop between runs of padding is a scan to the first
			// stop, as the engine's is (Machine.CompilePaddedScan): EmitScan.
			var delimiter = ReferenceEquals(_scanned, repeat) ? null : machine.PaddedDelimiter(repeat);

			if (_tape)
			{
				// A repetition that can never be asked for a shorter reading writes nothing
				// on the tape: what ends it is not a failure and there is no turn owed. The
				// rendering this replaces asked the same question in the same words; this
				// one had stopped asking, and wrote a way for every run in every grammar.
				//
				// NeverGivesBack asks whether what follows can fail and want a turn back, so it reads
				// the view (Continuation.Taught). NeverGivesBackPast asks whether what follows the
				// stop can begin with a character, which a positional stop can: the halves.
				var settled = max == min ||
					Determinism.NeverGivesBack(repeat, following.Taught, _graph, machine._seam) ||
					_stopAfter is var (stopped, stop, beyond) && ReferenceEquals(stopped, repeat) &&
					Determinism.NeverGivesBackPast(repeat, stop, beyond, _graph);

				// A body that is one character is a run whether it was spelled out or named.
				// A grammar names its classes far more often than it writes them: `Identifier
				// = Word & WordOrDigit*` is the ordinary way to write the ordinary thing, and
				// asking only whether the body was literally an element sent every one of them
				// through the machinery for turns that are not all alike.
				if (analyzing && !settled && max != 0)
					machine.OpeningRepeat(owner, repeat, following, run: machine.RunTest(body) is not null);

				if (settled && delimiter is { } scanned)
					EmitScan(code, repeat, scanned.Padding, scanned.Stop, following);
				else if (machine.RunTest(body) is { } test)
					EmitRun(code, body, test, min, max, settled);
				else
					EmitTurns(code, repeat, inside, settled);

				return;
			}

			// Carried immediately, a repetition is never asked for a shorter reading.
			if (delimiter is { } found)
			{
				EmitScan(code, repeat, found.Padding, found.Stop, following);

				return;
			}

			// At most one turn is not a loop, and writing it as one costs a counter nothing
			// reads, a test of it at the top and a jump backwards the reader never takes.
			// What a person writes is an `if`, and `?` is in every grammar there is — the
			// expression language alone had eighteen of them.
			if (min == 0 && max == 1)
			{
				EmitOnce(code, body, inside);

				return;
			}

			var turns = min > 0 || max is not null ? $"t{_calls++}" : null;

			if (turns is not null)
				code.Line($"var {turns} = 0;");

			using (code.Block("while (true)"))
			{
				if (max is { } limit)
				{
					code.Line($"if ({turns} >= {limit})");
					code.Then("break;");
					code.Line();
				}

				// The turn is not tried where the token in hand cannot begin it. What ends
				// a repetition is not a failure, and trying the turn made it one: a
				// refusal recorded at the token, and — nine levels of a ladder each asking
				// for their operator at the same token — a tie between them, which
				// allocates. This is the door the rendering beside this one has always had.
				if (Door([body]) is { } door && TriesBehindTheDoor(body, repeat))
				{
					// The reading that records tries the turn anyway, as EmitTurns does and for
					// the same reason: where the loop ends and what follows it refuses too, the
					// turn's refusal is half of the message.
					var open = $"open{_doors++}";

					code.Line($"var {open} = {machine.Within("p")};");
					code.Line();

					using (code.Block($"if ({open})"))
					{
						code.Line($"c = {machine.ReadAt("p")};");
						code.Line($"{open} = {door};");
					}

					code.Line();

					using (code.Block($"if (!{open} && failure.Quiet)"))
						code.Line("break;");

					code.Line();
				}
				else if (Door([body]) is { } closed)
				{
					// A turn owed below the minimum and not tried for the code it would run
					// (SaysBehindTheDoor) is the rule failing here: the reading that records says
					// what the turn would have begun with, as EmitTurns says it.
					var said = min > 0 && SaysBehindTheDoor(body, repeat)
						? $"if ({turns} < {min}) " + Noted(machine.DeclareExpected(machine.PredictedDisplays([body])))
						: null;

					if (said is null)
					{
						code.Line($"if ({machine.Past("p")})");
						code.Then("break;");
					}
					else
					{
						using (code.Block($"if ({machine.Past("p")})"))
						{
							code.Line(said);
							code.Line("break;");
						}
					}

					code.Line();
					code.Line($"c = {machine.ReadAt("p")};");
					code.Line();

					using (code.Block($"if (!({closed}))"))
					{
						if (said is not null)
							code.Line(said);

						// A fold's loop ending is the operator ladder saying that what stands
						// here continues nothing — which is what a reader is owed where the
						// parse then fails at this very token: `x` and then `}` was told
						// everything but that a `+` could have stood there. One note of the
						// whole ladder, and not the nine refusals this door exists to stop.
						NoteTails(code, repeat, body);

						code.Line("break;");
					}

					code.Line();
				}

				var turn = $"q{_calls++}";
				var back = _gathers || _logs ? _ways++ : -1;
				var (call, undo, _) = Called(body, inside);

				if (back >= 0)
				{
					if (_gathers)
						foreach (var line in machine.Carrier.MarkGathered(owner, $"rr{back}"))
							code.Line(line);

					if (_logs)
						foreach (var line in machine.Carrier.MarkRecords($"lm{back}"))
							code.Line(line);

					code.Line();
				}

				code.Line($"var {turn} = {call};");
				code.Line();

				using (code.Block($"if ({turn} < 0 || {turn} == p)"))
				{
					if (_gathers && back >= 0)
						foreach (var line in machine.Carrier.UnwindGathered(owner, $"rr{back}"))
							code.Line(line);

					if (_logs && back >= 0)
						LogBack(code, $"lm{back}");

					if (undo.Length > 0)
						code.Line(undo);

					code.Line("break;");
				}
				code.Line();
				code.Line($"p = {turn};");

				if (turns is not null)
					code.Line($"{turns}++;");
			}

			if (min > 0)
			{
				code.Line();
				code.Line($"if ({turns} < {min})");
				code.Then(Fails());
			}
		}

		/// <summary>
		/// A repetition of <c>(?!Separator &amp; any)</c>, where the separator is a stop between
		/// runs of padding: scanned to where the separator first matches, then finished by its own
		/// loop.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Written as turns, the loop asks the separator at every character, and at every
		/// character of a run of padding the separator reads the rest of the run before it finds
		/// no stop there: a run of n was n²/2 characters read (a FIX log field's value with a
		/// long run of spaces in it). The scan reads each once (Machine.EmitDelimiterScan), and
		/// where it stops is where the loop would have: every position before the padding in
		/// front of the first stop is one the separator does not match at, and at that position
		/// it does.
		/// </para>
		/// <para>
		/// The loop is then written as it always was and handed the last turns: as many as the
		/// fewest it may take, or all of them where the scan found fewer. So whatever it says
		/// where it ends — a refusal at the end of the input, a note, too few turns — it says as
		/// before, and for no more than the fewest turns' worth of reading again. Only a
		/// repetition nothing asks for a shorter reading: one that is asked gives back what the
		/// scan read, which the turns' ways are not there to offer.
		/// </para>
		/// </remarks>
		void EmitScan(Writer code, Node.Repeat repeat, string padding, string stop, FollowSets.Continuation following)
		{
			var min  = repeat.Min;
			var mark = $"m{_marks++}";

			_character = true;

			code.Line($"var {mark} = p;");

			// A block of its own: a rule may scan more than once, and the scan names its locals.
			using (code.Block(""))
				machine.EmitDelimiterScan(code, padding, stop);

			if (min > 0)
				code.Line($"p = p - {mark} > {min} ? p - {min} : {mark};");

			code.Line();

			_scanned = repeat;
			EmitRepeat(code, repeat, following);
			_scanned = null;
		}

		/// <summary>
		/// Whether the reading that records tries a turn its door has closed on, as the engine
		/// does, so that what refused the turn is said beside what refuses after the loop.
		/// </summary>
		/// <remarks>
		/// Only where a quiet reading comes first, which stops at the door as before. Not in a rule
		/// the engine reads as a scan, nor in the seam or what it calls, which the engine reads
		/// without recording; nor in a fold's loop, whose ending is said once for the whole ladder
		/// (NoteTails) and not by every level of it. Nor where the turn is one character, which
		/// the engine reads as a run and does not record either. Nor where the turn would run
		/// code of the host before its first item (RunsCodeBeforeItsFirstItem), whose effects
		/// would outlive the turn and could change what the quiet reading answered.
		/// </remarks>
		bool TriesBehindTheDoor(Node body, Node.Repeat? repeat)
		{
			return BehindTheDoor(body, repeat) && !machine.RunsCodeBeforeItsFirstItem(body);
		}

		/// <summary>
		/// Whether a turn behind a shut door would be tried (TriesBehindTheDoor) but for the code of
		/// the host it would run before its first item. Where the turn is owed, below a loop's
		/// minimum, the reading that records says what it would have begun with instead; an
		/// optional turn or one past the minimum is left unsaid, since a first set says nothing
		/// of the guard that would have refused it.
		/// </summary>
		bool SaysBehindTheDoor(Node body, Node.Repeat? repeat)
		{
			return BehindTheDoor(body, repeat) && machine.RunsCodeBeforeItsFirstItem(body);
		}

		bool BehindTheDoor(Node body, Node.Repeat? repeat)
		{
			return machine.Quiets && machine.ScannerOf(owner) is null && !machine.SeamReached.Contains(owner) &&
				machine.RunTest(body) is null && (repeat is null || !machine.IsFoldLoop(repeat));
		}

		/// <summary>The repetition <see cref="EmitScan"/> is writing the loop of.</summary>
		Node.Repeat? _scanned;

		/// <summary>
		/// A repetition of at most one turn: the turn, under the test that says it is there.
		/// </summary>
		/// <remarks>
		/// The same three things the loop does — the door, the turn, and putting back what a
		/// turn that read nothing gathered — with the loop taken away. A turn that fails is
		/// no failure here either: it is the repetition ending, which for this one means it
		/// was not written.
		/// </remarks>
		void EmitOnce(Writer code, Node body, FollowSets.Continuation inside)
		{
			var scopes = new Stack<IDisposable>();

			if (Door([body]) is { } door && TriesBehindTheDoor(body, null))
			{
				// The reading that records tries the turn anyway (EmitRepeat's loop says why).
				var open = $"open{_doors++}";

				_character = true;

				code.Line($"var {open} = {machine.Within("p")};");
				code.Line();

				using (code.Block($"if ({open})"))
				{
					code.Line($"c = {machine.ReadAt("p")};");
					code.Line($"{open} = {door};");
				}

				code.Line();
				scopes.Push(code.Block($"if ({open} || !failure.Quiet)"));
			}
			else if (Door([body]) is { } closed)
			{
				_character = true;

				scopes.Push(code.Block($"if ({machine.Within("p")})"));
				code.Line($"c = {machine.ReadAt("p")};");
				code.Line();
				scopes.Push(code.Block($"if ({closed})"));
			}

			var turn = $"q{_calls++}";
			var back = _gathers || _logs ? _ways++ : -1;
			var (call, undo, _) = Called(body, inside);

			if (back >= 0)
			{
				if (_gathers)
					foreach (var line in machine.Carrier.MarkGathered(owner, $"rr{back}"))
						code.Line(line);

				if (_logs)
					foreach (var line in machine.Carrier.MarkRecords($"lm{back}"))
						code.Line(line);

				code.Line();
			}

			code.Line($"var {turn} = {call};");
			code.Line();

			var undone = _gathers && back >= 0 || _logs && back >= 0 || undo.Length > 0;

			using (code.Block($"if ({turn} >= 0 && {turn} != p)"))
				code.Line($"p = {turn};");

			if (undone)
				using (code.Block("else"))
				{
					if (_gathers && back >= 0)
						foreach (var line in machine.Carrier.UnwindGathered(owner, $"rr{back}"))
							code.Line(line);

					if (_logs && back >= 0)
						LogBack(code, $"lm{back}");

					if (undo.Length > 0)
						code.Line(undo);
				}

			while (scopes.Count > 0)
				scopes.Pop().Dispose();
		}

		/// <summary>
		/// A run of one character, read where it stands and given back a character at a time.
		/// </summary>
		/// <remarks>
		/// <para>
		/// A method a turn is what a repetition of anything else costs, and for a character
		/// class it is all cost: the turn is a bounds check, a read and a test. So it is
		/// written as the loop it is — whether the class was spelled out or named, which
		/// is what <see cref="Machine.RunTest"/> answers.
		/// </para>
		/// <para>
		/// And the loop takes everything it can, which is not always what the rule wanted.
		/// What it can give back is the difference between where it stopped and the fewest
		/// turns it was allowed — one number, so the tape carries how many characters were
		/// handed back rather than a way for each. Replayed, the run reaches the same end
		/// and hands back what the tape says.
		/// </para>
		/// </remarks>
		/// <param name="test">
		/// The one-character test the body is, from <see cref="Machine.RunTest"/>.
		/// </param>
		/// <param name="settled">
		/// Whether nothing after the run can ever want a character it took, in which case
		/// there is no shorter reading to offer and nothing goes on the tape.
		/// </param>
		void EmitRun(Writer code, Node body, string test, int min, int? max, bool settled)
		{
			var name = machine.DeclareExpected(machine.Displays(body));
			var mark = $"m{_marks++}";

			// Where the run has no ceiling, no floor and nothing to give back, nobody ever
			// asks where it started, and writing it down is a local the consumer's compiler
			// would rightly warn about.
			var counted = max is not null || min > 0 || !settled;

			_character = true;

			if (counted)
				code.Line($"var {mark} = p;");

			using (code.Block("while (true)"))
			{
				if (max is { } limit)
				{
					code.Line($"if (p - {mark} >= {limit})");
					code.Then("break;");
					code.Line();
				}

				code.Line($"if ({machine.Past("p")})");
				code.Then("break;");
				code.Line();
				code.Line($"c = {machine.ReadAt("p")};");
				code.Line();

				if (!string.Equals(test, "true", StringComparison.Ordinal))
				{
					code.Line($"if (!({test}))");
					code.Then("break;");
					code.Line();
				}

				code.Line("p++;");
			}

			code.Line();

			if (min > 0)
			{
				using (code.Block($"if (p < {mark} + {min})"))
					Refused(code, name);

				code.Line();
			}

			// A run nothing can ask for a shorter reading of has nothing to give.
			if (settled)
				return;

			var floor = min == 0 ? mark : $"({mark} + {min})";
			var gave  = $"d{_ways++}";

			using (code.Block($"if (p > {floor})"))
			{
				code.Line($"var {gave} = 0;");
				code.Line();

				using (code.Block("if (ways.Cursor < ways.Count)"))
				{
					code.Line($"{gave} = ways.Items[ways.Cursor * 2];");
					code.Line("ways.Cursor++;");
				}

				code.Line("else");

				using (code.Block(""))
					code.Line($"ways.Open(p - {floor});");

				code.Line();
				code.Line($"p -= {gave};");
			}
		}

		/// <summary>The repetition marked <c>recover</c> a part is, or captures, where the reader reads it.</summary>
		(Node.Repeat, RecoveryRead)? Recovering(Node part)
		{
			var held = part is Node.Capture(_, var inner) ? inner : part;

			return held is Node.Repeat repeat && machine._recoveryReads.TryGetValue(repeat, out var read)
				? (repeat, read)
				: null;
		}

		/// <summary>
		/// A repetition marked <c>recover</c> (docs/syntax.md §8.2): at every boundary the
		/// complete continuation first, then an element, then — where both failed and input
		/// remains — a bad element, skipped past the next match of the synchronization.
		/// </summary>
		/// <remarks>
		/// <para>
		/// The continuation is the rest of the rule's sequence, which ends in <c>eof</c>, or
		/// where there is none the end of the input the publication reads to
		/// (<see cref="UnreadRecovery"/>). One that succeeds has
		/// read to the end, so the loop is over and so is the rule's sequence: nothing after
		/// it can take a turn back. An element that succeeds is committed for the same
		/// reason, as the engine's <c>DeactivateChoices</c> commits it — the ways it opened
		/// are dropped, so that no failure later can ask it for another reading.
		/// </para>
		/// <para>
		/// A bad element is a record of its own on the tape, under an arm of its own: where it
		/// began, where the synchronization began, how far it got and how many elements came
		/// before it. The walk builds it with the <c>recover</c> factory, as the engine's does
		/// from its arena entry (<see cref="MaterializeRecoveryArm"/>), and it is gathered where
		/// its siblings are. How far it got is the failure's <c>Reach</c>, which a refusal
		/// raises and the loop resets where each element begins: the engine's <c>reach</c>.
		/// </para>
		/// </remarks>
		void EmitRecovering(Writer code, Node.Repeat repeat, RecoveryRead read, FollowSets.Continuation following)
		{
			var (body, min, _) = repeat;
			var plan   = read.Plan;
			var inside = following.Map(one => new FollowSets.Continuation(FirstSets.Of(body, _graph).Or(one.Plain), one.AfterSeam));
			var turn   = $"t{_turns++}";
			var slot   = plan.Slot - machine._captureOffsets[owner];
			var began  = $"m{_ways++}";

			if (plan.Recovery.YieldStep)
			{
				EmitYieldStep(code, body, read, slot, inside);

				return;
			}

			// What an element opened on the ways is dropped at its commit. A reader carrying
			// immediately has ways only where something opens one, and then only an element that
			// can open one has anything to drop.
			var ways = machine.Carrier is not ImmediateCarrier || machine.Opens(body);

			code.Line($"var {turn} = 0;");

			if (ways)
				code.Line($"var {began} = ways.Cursor;");

			// What a turn of an enclosing recovering repetition marked on the failure, put back
			// wherever this one is left: its element is told what it wanted since its own turn began.
			var stood = read.Plan.Recovery.Explains ? $"{turn}s" : null;

			if (stood is not null)
				code.Line($"var {stood} = failure.Stood; var {stood}t = failure.Tied;");

			code.Line();

			using (code.Block("while (true)"))
			{
				// What came before the turn is built and nothing reads behind it: the buffer may let
				// it go (Machine.ReleasesTurns).
				if (machine.ReleasesTurns(owner))
				{
					code.Line("text.LetGo(p);");
					code.Line();
				}

				// What stood at the place the turn begins, for what its element is told it wanted.
				if (read.Plan.Recovery.Explains)
					code.Line(Machine.Stand);

				// The complete continuation first, once the minimum is met.
				using (min > 0 ? code.Block($"if ({turn} >= {min})") : null)
				{
					if (read.Continuation.Count == 0)
					{
						code.Line($"if ({machine.Past("p")})");
						code.Then("break;");
					}
					else
					{
						var rest = Attempt(code, new Node.Sequence(read.Continuation), following);

						code.Line($"if ({rest} >= 0)");

						using (code.Block(""))
						{
							code.Line($"p = {rest};");
							code.Line("break;");
						}
					}
				}

				code.Line();

				// An element, with how far it gets counted from where it begins.
				code.Line("failure.Reach = p;");

				var mark  = $"k{_ways++}";
				var taken = Attempt(code, body, inside, ways ? mark : null);

				code.Line($"if ({taken} >= 0)");

				using (code.Block(""))
				{
					// Committed: what it opened on the tape can never be asked for again.
					if (ways)
						code.Line($"ways.Count = ways.Cursor = {mark};");
					code.Line($"p = {taken};");
					code.Line($"{turn}++;");
					code.Line("continue;");
				}

				code.Line();

				// Neither, and nothing left: an element or the continuation is missing, which the
				// two attempts have said. What the loop opened on the tape goes with it: the
				// engine never comes back into a recovering repetition, so a reading of the rule
				// asked again may change only what came before it.
				code.Line($"if ({machine.Past("p")})");

				using (code.Block(""))
				{
					if (ways)
						code.Line($"ways.Count = ways.Cursor = {began};");

					if (stood is not null)
						code.Line($"failure.Stood = {stood}; failure.Tied = {stood}t;");

					code.Line(Fails());
				}

				code.Line();

				// A bad element: stepped over to past the next match of the synchronization, or to
				// the end, by a method of its own that only a failure calls.
				code.Line($"p = {Broken(read, slot, out var handed)}(p, {turn}{handed});");
				code.Line($"{turn}++;");
			}

			if (stood is not null)
				code.Line($"failure.Stood = {stood}; failure.Tied = {stood}t;");
		}

		/// <summary>
		/// The step a <c>yield</c> is lowered to (GrammarNormalizer.LowerYieldPublications): one turn
		/// of a repetition marked <c>recover</c>, read each time the driver is asked for the next
		/// element. The next step is the continuation, so nothing is tried before the element; where
		/// the element fails and input remains, the bad element is stepped over, numbered by the
		/// driver's own count (Failure.RecoveryOrdinal), and where nothing remains there is no element.
		/// </summary>
		void EmitYieldStep(Writer code, Node body, RecoveryRead read, int slot, FollowSets.Continuation inside)
		{
			var ways  = machine.Carrier is not ImmediateCarrier || machine.Opens(body);
			var mark  = $"k{_ways++}";

			if (read.Plan.Recovery.Explains)
				code.Line(Machine.Stand);

			code.Line("failure.Reach = p;");

			var taken = Attempt(code, body, inside, ways ? mark : null);

			code.Line($"if ({taken} >= 0)");

			using (code.Block(""))
			{
				if (ways)
					code.Line($"ways.Count = ways.Cursor = {mark};");

				code.Line($"p = {taken};");
			}

			code.Line("else");

			using (code.Block(""))
			{
				code.Line($"if ({machine.Past("p")})");
				code.Then(Fails());
				code.Line();
				code.Line($"p = {Broken(read, slot, out var handed)}(p, failure.RecoveryOrdinal{handed});");

				// The step's element is what it recovered: the item, as a read one would have been.
				code.Line($"r{slot} = {machine.Carrier.Last(RuleOfSlot(slot))};");
			}
		}

		/// <summary>
		/// The method that steps over a bad element: from where it began to the next match of the
		/// synchronization, which it moves past, or to the end; and the record of it, pushed
		/// where its siblings are. Named <c>Read_{Rule}_Broken</c>, and handed the position and
		/// how many elements came before.
		/// </summary>
		string Broken(RecoveryRead read, int slot, out string handed)
		{
			// One a marked repetition: a rule may mark more than one (§8.2).
			var before = machine._recoveryReads.Values.Count(one => one.Plan.Rule == read.Plan.Rule && one.Plan.Id < read.Plan.Id);
			var name   = machine.ReaderOf(owner) + "_Broken" + (before > 0 ? before.ToString(System.Globalization.CultureInfo.InvariantCulture) : "");
			var code = new Writer(0);

			code.Line("var p     = pos;");
			code.Line("var reach = failure.Reach;");
			code.Line("var to    = p;");

			// What would have fit where the element stopped, taken before the synchronization is
			// looked for: a candidate that begins to match and does not records further along.
			if (read.Plan.Recovery.Explains)
				code.Line("var expecting = Expecting_DotGram(ref failure, reach, failure.Stood, failure.Tied);");

			if (machine.PaddedDelimiter(read.Plan.Recovery.Sync) is not null)
				code.Line(machine.BufferedBytes ? "var c     = 0;" : "var c     = '\\0';");

			code.Line();

			using (code.Block("while (true)"))
			{
				code.Line($"if ({machine.Past("p")})");

				using (code.Block(""))
				{
					code.Line("to = p;");
					code.Line("break;");
				}

				code.Line();

				var stops = SyncStops(read.Plan.Recovery.Sync);

				// A stop between runs of padding (`' '* & '|' & ' '*`) is found as the engine finds
				// it (Machine.EmitRecoverySearch): in one pass to the first stop, from the start of the
				// padding before it. Tried at every character of a run, it read the rest of the run
				// each time, and a long run was a square. The tries it skips could only fail short of
				// the stop, which the reading then goes past.
				if (machine.PaddedDelimiter(read.Plan.Recovery.Sync) is { } delimiter)
				{
					machine.EmitDelimiterScan(code, delimiter.Padding, delimiter.Stop);
					code.Line();

					code.Line($"if ({machine.Past("p")})");

					using (code.Block(""))
					{
						code.Line("to = p;");
						code.Line("break;");
					}

					code.Line();
				}
				// Where the synchronization begins with one of a few characters, the next place it
				// can begin is searched for (Machine.EmitSearch, IndexOf) rather than tried at
				// every position: a feed with a bad line in ten pays a scan, not an attempt per
				// character.
				else if (stops is not null)
				{
					machine.EmitSearch(code, "hit", "p", stops);
					code.Line("if (hit < 0)");

					using (code.Block(""))
					{
						code.Line($"p  = {machine.EndOfInput};");
						code.Line("to = p;");
						code.Line("break;");
					}

					code.Line();
					code.Line("p = hit;");
					code.Line();
				}

				var synced = Attempt(code, read.Plan.Recovery.Sync, FollowSets.Continuation.All);

				code.Line($"if ({synced} > p)");

				using (code.Block(""))
				{
					code.Line("to = p;");
					code.Line($"p = {synced};");
					code.Line("break;");
				}

				code.Line();
				code.Line("p++;");
			}

			code.Line();

			_records = true;

			// Kept by where the element's record begins, for the walk that builds it after the parse.
			if (machine.Carrier is TapeCarrier && read.Plan.Recovery.Explains)
				code.Line($"{Keep("ways")}[ways.LogCount] = expecting;");

			foreach (var line in machine.Carrier.Recovered(
				read.Plan, slot, RuleOfSlot(slot), _positions, machine.MemberOfSlot(owner, slot)?.Member.Element is not null))
				Carried(code, line);

			// The element stepped over, told where the build traces: from where it began to where
			// reading goes on, and how far it got.
			if (machine.Tracing is { } tracing)
				code.Line($"failure.Trace?.Recovered({tracing.RuleOf(owner)}, pos, p, reach);");

			code.Line("return p;");

			// What the synchronization's call hands on of the rule's gathering is the rule's, and
			// is handed over under the same name: the tape's one mark, or the immediate carrier's
			// one a stack, whose names hold the stack's type until the file numbers it
			// (Machine.TableName) and so are asked of the carrier rather than read off the text.
			var body  = code.ToString();
			var marks = machine.Carrier is TapeCarrier
				? System.Text.RegularExpressions.Regex.IsMatch(body, @"\brb\b") ? ["rb"] : new List<string>()
				: machine.Carrier.GatherHanding(owner, declared: false, inBody: true)
					.Split(',').Select(static one => one.Trim()).Where(one => one.Length > 0 && body.Contains(one)).ToList();

			handed = string.Concat(marks.Select(static one => ", " + one));

			Parts.Add((name, ", int ordinal" + string.Concat(marks.Select(static one => ", int " + one)), body));
			Cold.Add(name);

			return name;
		}

		/// <summary>
		/// The characters a synchronization can begin with, where they are few enough to search
		/// for and it cannot match nothing; null where it has to be tried at every position.
		/// </summary>
		List<char>? SyncStops(Node sync)
		{
			if (machine.Decidable(sync) is not { IsKnown: true, Ends: false } first)
				return null;

			var stops = new List<char>(5);

			foreach (var range in first.Ranges)
				for (int one = range.From; one <= range.To; one++)
				{
					if (stops.Count == 5)
						return null;

					stops.Add((char)one);
				}

			return stops.Count > 0 ? stops : null;
		}

		/// <summary>
		/// One attempt at a part, asked for every reading it has, with what it wrote put back
		/// where it fails: the name of the position it reached, or of -1.
		/// </summary>
		/// <param name="mark">
		/// Where the attempt began on the tape's ways, declared under this name, for a caller
		/// that commits what the attempt opened.
		/// </param>
		string Attempt(Writer code, Node part, FollowSets.Continuation following, string? mark = null)
		{
			var (call, undo, opens) = Called(part, following);
			var segment = _ways++;
			var began   = mark ?? $"s{segment}";
			var took    = $"q{_calls++}";

			if (opens || mark is not null)
				code.Line($"var {began} = ways.Cursor;");

			foreach (var line in machine.Carrier.MarkRecords($"lm{segment}"))
				code.Line(line);
			foreach (var line in machine.Carrier.MarkGathered(owner, $"rr{segment}"))
				code.Line(line);

			if (!opens)
			{
				code.Line($"var {took} = {call};");
				code.Line();

				using (code.Block($"if ({took} < 0)"))
				{
					LogBack(code, $"lm{segment}");
					foreach (var line in machine.Carrier.UnwindGathered(owner, $"rr{segment}"))
						code.Line(line);

					if (undo.Length > 0)
						code.Line(undo);
				}

				code.Line();

				return took;
			}

			// A loop that asks again, noted with the others and never deferring: the recovering path is outside the deferral's scope.
			machine.Deferral(owner, RetrySite.Yield, noting: !analyzing);

			code.Line($"var {took} = -1;");
			code.Line();

			using (code.Block("while (true)"))
			{
				code.Line($"{took} = {call};");
				code.Line();
				code.Line($"if ({took} >= 0)");
				code.Then("break;");
				code.Line();
				LogBack(code, $"lm{segment}");
				foreach (var line in machine.Carrier.UnwindGathered(owner, $"rr{segment}"))
					code.Line(line);

				if (undo.Length > 0)
					code.Line(undo);

				code.Line();
				code.Line($"if (ways.Cursor > {began} && ways.Retry({began}))");
				code.Then("continue;");
				code.Line();
				code.Line("break;");
			}

			code.Line();

			return took;
		}

		/// <summary>
		/// A repetition of anything longer than one element, over characters: a way for
		/// every turn.
		/// </summary>
		/// <remarks>
		/// <para>
		/// What a run of characters gives back is a count, because every turn of it is one
		/// character and a shorter run is the same scan stopped earlier. A turn of anything
		/// else is not a character and not the same size as its neighbours, so what has to
		/// be given back is the turn itself — and the tape carries that as a way per turn,
		/// each standing at "went round again" and reaching to "stopped here".
		/// </para>
		/// <para>
		/// The way is opened before the turn rather than after it, and only where stopping
		/// is allowed: below the minimum there is nothing to offer, because stopping there
		/// is not a reading the repetition has. A turn that fails spends its own way — the
		/// way now says "stopped here" — and a turn that fails below the minimum fails the
		/// repetition.
		/// </para>
		/// </remarks>
		/// <param name="settled">
		/// Whether nothing after the loop can ever want what a turn took, in which case no
		/// turn is owed and the loop writes nothing on the tape.
		/// </param>
		void EmitTurns(Writer code, Node.Repeat repeat, FollowSets.Continuation inside, bool settled)
		{
			var (body, min, max) = repeat;

			if (max == 0)
				return;

			var turn     = min > 0 || max is not null ? $"t{_turns++}" : null;
			var nullable = FirstSets.Nullable(body, _graph);
			var (call, undo, opens) = Called(body, inside);

			if (turn is not null)
				code.Line($"var {turn} = 0;");

			using (code.Block("while (true)"))
			{
				if (max is { } limit)
				{
					code.Line($"if ({turn} >= {limit})");
					code.Then("break;");
					code.Line();
				}

				var way   = _ways++;
				var stops = min > 0 ? $"{turn} >= {min}" : null;

				// The turn is not tried where the token in hand cannot begin it — the same
				// door the reading over kinds has, and for the same two reasons: a turn that
				// cannot begin is the loop ending and not a failure to record, and a turn not
				// tried is a call not made.
				if (Door([body]) is { } door)
				{
					var open = $"o{_ways++}";

					code.Line($"var {open} = {machine.Within("p")};");
					code.Line();

					using (code.Block($"if ({open})"))
					{
						code.Line($"c = {machine.ReadAt("p")};");
						code.Line($"{open} = {door};");
					}

					code.Line();

					// Only where nothing is recorded. A reading that records tries the turn anyway,
					// as the engine does, so that what refused it is said: where the loop ends and
					// what follows it refuses too, the turn's refusal is half of the message, and
					// without it a parse refused past the loop said only that it did not match. Not
					// in a rule the engine reads as a scan — a word — nor in the seam or what it calls,
					// which the engine reads without recording what they refuse.
					// Nor a turn that would run code of the host before its first item
					// (RunsCodeBeforeItsFirstItem): what the reading that records does must not
					// change what the quiet one answered.
					var tries = machine.Quiets && machine.ScannerOf(owner) is null && !machine.SeamReached.Contains(owner) &&
						!machine.RunsCodeBeforeItsFirstItem(body);

					using (code.Block(tries ? $"if (!{open} && failure.Quiet)" : $"if (!{open})"))
					{
						// A door that does not open below the minimum is not the loop ending
						// but the rule failing, and says what it wanted.
						if (min > 0)
						{
							using (code.Block($"if ({turn} < {min})"))
								Refused(code, machine.DeclareExpected(machine.Displays(body)));

							code.Line();
						}

						NoteTails(code, repeat, body);

						code.Line("break;");
					}

					code.Line();
				}

				// A loop nothing can ask a turn back from has no way to record.
				var records = !settled;

				if (records)
				{
					code.Line($"var w{way} = -1;");
					code.Line($"var d{way} = 0;");
					code.Line();

					// Below the minimum there is nothing to offer: stopping there is not a
					// reading the repetition has.
					var offering = stops is null ? null : code.Block($"if ({stops})");

					using (code.Block("if (ways.Cursor < ways.Count)"))
					{
						code.Line($"w{way} = ways.Cursor;");
						code.Line($"d{way} = ways.Items[w{way} * 2];");
						code.Line("ways.Cursor++;");
					}

					code.Line("else");

					using (code.Block(""))
						code.Line($"w{way} = ways.Open(1);");

					code.Line();
					code.Line($"if (d{way} == 1)");
					code.Then("break;");

					offering?.Dispose();

					code.Line();
				}

				// The turn is asked for every reading it has before it is called spent,
				// which is the alternative's rule (EmitChoiceOverCharacters) over again.
				var segment = _ways++;
				var took    = $"q{_calls++}";
				var defers  = opens && Defers(RetrySite.Turn);

				if (analyzing)
					machine.NoteRetrySite(owner, body);

				if (opens)
					code.Line($"var s{segment}  = ways.Cursor;");
				if (defers)
					code.Line(DeclareDeferral(segment.ToString()));

				foreach (var line in machine.Carrier.MarkRecords($"lm{segment}"))
					code.Line(line);
				foreach (var line in machine.Carrier.MarkGathered(owner, $"rr{segment}"))
					code.Line(line);
				code.Line($"var {took} = -1;");
				code.Line();

				if (opens)
				{
					// The failure branch puts back the records and the gathered members and no
					// more: what a part wrote is undone only once the turn is given up, below.
					var restore = Restore(segment, "");

					using (code.Block("while (true)"))
					{
						code.Line($"{took} = {call};");
						code.Line();
						Stood(code, took, segment, defers, restore);
						code.Line();
						foreach (var line in restore)
							code.Line(line);
						if (!defers || restore.Count > 0)
							code.Line();
						Retried(code, segment, defers);
						code.Line("break;");
					}
				}
				else
				{
					code.Line($"{took} = {call};");
					code.Line();

					using (code.Block($"if ({took} < 0)"))
					{
						LogBack(code, $"lm{segment}");
						foreach (var line in machine.Carrier.UnwindGathered(owner, $"rr{segment}"))
							code.Line(line);
					}
				}

				code.Line();

				using (code.Block($"if ({took} < 0)"))
				{
					if (undo.Length > 0)
					{
						code.Line(undo);
						code.Line();
					}

					if (stops is null)
					{
						if (records)
							code.Line($"ways.Next(w{way}, 1);");
					}
					else
					{
						// The turn is spent, so the way that offered it now says "stopped
						// here" — and a turn short of the minimum is not a stop but a
						// refusal, which the body has already said everything about.
						if (records)
						{
							code.Line($"if ({stops})");
							code.Then($"ways.Next(w{way}, 1);");
							code.Line();
						}

						code.Line($"if ({turn} < {min})");
						code.Then(Fails());
					}

					code.Line();
					code.Line("break;");
				}

				code.Line();

				if (nullable)
				{
					code.Line($"if ({took} == p)");
					code.Then("break;");
					code.Line();
				}

				code.Line($"p = {took};");

				if (turn is not null)
					code.Line($"{turn}++;");
			}
		}

		/// <summary>
		/// A group in braces: its first reading is its only one.
		/// </summary>
		/// <remarks>
		/// Over kinds every reading is already the only one, and the braces say nothing the
		/// rendering does not. Over characters the group is asked for a reading until it has
		/// one, and then what it decided is sealed: nothing after it may come back into it.
		/// </remarks>
		void EmitAtomic(Writer code, Node kept, FollowSets.Continuation following, bool loaded)
		{
			if (!_tape)
			{
				Emit(code, kept, following, loaded);

				return;
			}

			// Inside, the group is sealed. "Nothing after it may come back into it" is a
			// statement about the continuation, and the group's contents are entitled to hear
			// it: what ends the group is the seal, so a repetition standing at the end of one is
			// never asked for a shorter reading and owes no way per turn. `trivia = { (Space |
			// LineComment | BlockComment)* }` is the shape this is written for, and it is in
			// every grammar that spaces its operands: it was opening a way for every character
			// of whitespace and sealing all of them a moment later. What follows inside the
			// group is threaded as it always was — `{ A* & B }` still hands B's first set to
			// the star before it, and `{ A* & ?!B }` what the refusal lets through.
			var segment             = _ways++;
			var took                = $"q{_calls++}";
			var (call, undo, opens) = Called(kept, FollowSets.Continuation.Sealed);

			// Where nothing under the group can open a way, there is nothing to take back into it
			// and nothing to seal: the loop never went round, the seal spent nothing, and the one
			// `ways.` it wrote was all that gave a reader carrying immediately a Ways to rent (FIX's
			// Tag, `{ ['1'..'9'] & ['0'..'9']* }`, twice a field). A failure inside is a failure of
			// the part, which whoever marked the records puts back, as for any other part.
			if (!opens)
			{
				code.Line($"var {took} = {call};");
				code.Line($"if ({took} < 0)");
				code.Then(Fails());
				code.Line($"p = {took};");

				return;
			}

			var defers  = Defers(RetrySite.Atomic);
			var restore = Restore(segment, undo);

			code.Line($"var s{segment}  = ways.Cursor;");
			if (defers)
				code.Line(DeclareDeferral(segment.ToString()));
			foreach (var line in machine.Carrier.MarkRecords($"lm{segment}"))
				code.Line(line);
			foreach (var line in machine.Carrier.MarkGathered(owner, $"rr{segment}"))
				code.Line(line);
			code.Line($"var {took} = -1;");
			code.Line();

			// The replay, where there is one, reads before the seal below: what is sealed is the
			// attempt that stands, built.
			using (code.Block("while (true)"))
			{
				code.Line($"{took} = {call};");
				code.Line();
				Stood(code, took, segment, defers, restore);
				code.Line();
				foreach (var line in restore)
					code.Line(line);

				if (!defers || restore.Count > 0)
					code.Line();
				Retried(code, segment, defers);
				code.Line("break;");
			}

			code.Line();
			code.Line($"if ({took} < 0)");
			code.Then(Fails());
			code.Line();
			code.Line($"ways.Seal(s{segment});");
			code.Line($"p = {took};");
		}

		/// <summary>
		/// Whether a loop of this rule's reading defers building on a give-back: what the machine
		/// decided for the site (<see cref="Machine.Deferral"/>), noted where the readers are being
		/// written rather than read through for the first time — and never in a trace build, whose
		/// sink would hear the attempt that stood twice.
		/// </summary>
		bool Defers(RetrySite site)
		{
			return machine.Deferral(owner, site, noting: !analyzing).Defers && machine.Tracing is null;
		}

		/// <summary>
		/// What a loop around a part puts back where the part failed, before the way is flipped:
		/// the records to the loop's mark, the gathered members to theirs, and the positions the
		/// part wrote where the site undoes those on a retry.
		/// </summary>
		List<string> Restore(int segment, string undo)
		{
			var restore = new List<string>(machine.Carrier.UnwindRecords($"lm{segment}"));

			restore.AddRange(machine.Carrier.UnwindGathered(owner, $"rr{segment}"));

			if (undo.Length > 0)
				restore.Add(undo);

			return restore;
		}

		/// <summary>
		/// A part that reached somewhere ends the loop around it — or, where the loop defers
		/// building on a give-back, is read once more to build it (<see cref="Machine.Stood"/>).
		/// </summary>
		static void Stood(Writer code, string took, int segment, bool defers, IReadOnlyList<string> restore)
		{
			if (defers)
			{
				Machine.Stood(code, took, segment.ToString(), restore, ["break;"]);

				return;
			}

			code.Line($"if ({took} >= 0)");
			code.Then("break;");
		}

		/// <summary>The way flipped and the part asked again — where the loop defers, moving where it stands first (<see cref="Machine.Retried"/>).</summary>
		static void Retried(Writer code, int segment, bool defers)
		{
			if (defers)
			{
				using (code.Block($"if (ways.Cursor > s{segment} && ways.Retry(s{segment}))"))
					Machine.Retried(code, segment.ToString());

				code.Line();
				Undeferred(code, segment.ToString());
				code.Line();

				return;
			}

			code.Line($"if (ways.Cursor > s{segment} && ways.Retry(s{segment}))");
			code.Then("continue;");
			code.Line();
		}

		/// <summary>
		/// A guard or selector helper, handed what the rule has captured so far (§7.7).
		/// </summary>
		/// <remarks>
		/// A text capture is cut from the locals that hold it; a captured rule's value is
		/// built now, from the records already in the log, and stays built — the walk at
		/// the end skips what a guard built, so no factory runs twice. The predicate itself
		/// is a method of its own under a <c>#line</c> pointing at the grammar, handed the
		/// captures by name. A selector uses the same arguments and returns a branch index.
		/// </remarks>
		string EmitGuardCall(Writer code, Node.Guard guard, Node.SwitchSelection? selection = null)
		{
			var rule       = machine._owners[guard];
			var method     = $"Recognize_DotGram{machine._tag}_Guard" + machine._guards++;
			var helper     = new Writer(0);
			var parameters = new List<string>();
			var arguments  = new List<string>();
			var text       = guard.Text;
			var begun      = _part ? "start" : "pos";
			var mark       = _part ? "lmark" : "lm";

			if (CSharpEmitter.Uses(_graph, text, "parserText"))
			{
				parameters.Add((machine.BorrowedCaptures ? machine.CaptureSpanType : "string") + " parserText");
				arguments.Add(machine.Cut(begun, $"p - {begun}"));
			}

			if (CSharpEmitter.Uses(_graph, text, "parserSpan"))
			{
				parameters.Add("SourceSpan parserSpan");
				arguments.Add(machine.Span(begun, $"p - {begun}"));
			}

			if (_graph.ContextOf(rule) is { } contract && CSharpEmitter.Uses(_graph, text, "context"))
			{
				parameters.Add($"{contract} context");
				arguments.Add("context");
			}

			// The value so far (§4.3), where a guard in a step names it: the reader holds it as
			// `fold`, a value where the carrier builds as it reads and a record to build where
			// it does not.
			if (machine.GuardAccumulator(rule, guard) is { } accumulator)
			{
				var build = machine.Carrier.BuildFold(rule, mark);

				if (build.Length > 0)
					code.Line(build);

				parameters.Add($"{_graph.Types[rule]} {ResultTypes.ParameterOf(accumulator)}");
				arguments.Add(ValueAt(rule, "fold"));
			}

			foreach (var (member, slots) in machine.GuardMembers(rule, guard))
			{
				var handed = $"g{_guardLocals++}";
				var type   = member.Rule is null ? machine.BorrowedCaptures ? machine.CaptureSpanType : "string" : machine._results.ValueOf(member.Rule);

				parameters.Add(
					$"{type}{(member.IsSequence ? "[]" : member.IsOptional && !(machine.BorrowedCaptures && member.Rule is null) ? "?" : "")} " +
					ResultTypes.ParameterOf(member));
				arguments.Add(handed);

				if (member.Rule is null)
				{
					var missing = machine.BorrowedCaptures ? machine.EmptyCapture : member.IsOptional ? "null" : "string.Empty";
					var cut     = machine.Cut($"{handed}From", $"{handed}To - {handed}From");

					// A string the construction will want as well is kept for it: declared at the
					// method's start, where the construction can see it, and taken there where it
					// stands on the positions the construction would cut (ImmediateCarrier.PutText).
					if (machine.Carrier is ImmediateCarrier && !machine.BorrowedCaptures)
					{
						code.Line($"{handed}From = {First("a", "a", slots)};");
						code.Line($"{handed}To   = {First("a", "b", slots)};");
						code.Line($"{handed} = {handed}From < 0 ? {missing} : {cut};");

						_hoisted.Add($"var {handed}From = -1;");
						_hoisted.Add($"var {handed}To   = -1;");
						_hoisted.Add(member.IsOptional ? $"string? {handed} = null;" : $"var {handed} = string.Empty;");
						_guardTexts[member.Name] = handed;

						continue;
					}

					code.Line($"var {handed}From = {First("a", "a", slots)};");
					code.Line($"var {handed}To   = {First("a", "b", slots)};");
					code.Line($"var {handed} = {handed}From < 0 ? {missing} : {cut};");

					continue;
				}

				var build = type == "SourceSpan"
					? ""
					: machine.Carrier.Materialize("{0}", mark);

				if (!member.IsSequence)
				{
					code.Line($"var {handed}At = {machine.Carrier.FirstRecord(slots, member.Rule!)};");

					if (build.Length > 0)
						code.Line($"if (!({machine.Carrier.Absent(member.Rule!, handed + "At")})) {string.Format(build, handed + "At")}");

					// The second of these reads the record WITHOUT asking whether it is there, which is
					// what Machine.GuardCaptureAdmitsAbsence buys and what makes it load-bearing: where
					// that answer is wrong, this throws IndexOutOfRangeException rather than handing the
					// guard a wrong value — and it throws under Release as much as Debug, which the
					// engine rendering's Debug.Assert does not.
					code.Line(member.IsOptional
						? $"{type}? {handed} = {machine.Carrier.Absent(member.Rule!, handed + "At")} ? default({type}?) : {ValueAt(member.Rule!, handed + "At")};"
						// Declared and converted rather than `var`: the expression may be a record local the
						// rendering typed nullable (the flat path reads `int? r1`), and `var` would inherit
						// that and then not fit the parameter. The same idiom the factory call uses, and
						// the same cast: only where `T?` is a Nullable<T> (IsReferenceType).
						: $"{type} {handed} = {(machine.IsReferenceType(type) ? "" : $"({type})")}{ValueAt(member.Rule!, handed + "At")}!;");

					continue;
				}

				// Gathered turn by turn on the tape, and collected here the way the rule's end
				// would collect them.
				machine.Carrier.Gathered(code, Refs, slots, handed, type, build, member.Rule is null);
			}

			if (selection is null)
			{
				helper.Line($"static bool {method}({string.Join(", ", parameters)}) =>");
				CSharpEmitter.Handed(helper, machine._lines, guard.At, text + ";");
			}
			else
			{
				foreach (var type in new[] { "int", "uint", "long", "ulong", "string?" })
					helper.Line($"static {type} {method}_Key({type} value) => value;");
				using (helper.Block($"static int {method}({string.Join(", ", parameters)})"))
				{
					helper.Line($"switch ({method}_Key(");
					CSharpEmitter.Handed(helper, machine._lines, guard.At, text + "))");
					using (helper.Block(""))
					{
						for (var i = 0; i < selection.Labels.Count; i++)
							helper.Line((selection.Labels[i] is { } label ? $"case {label}" : "default") + $": return {i};");
						if (!selection.Labels.Contains(null))
							helper.Line("default: return -1;");
					}
				}
			}
			machine._extra.Add(helper.ToString());
			return $"{method}({string.Join(", ", arguments)})";
		}

		void EmitSelection(Writer code, Node.Choice choice, Node.SwitchSelection selection, FollowSets.Continuation following)
		{
			string call;
			if (_tape && machine._opens is { Count: > 0 })
			{
				// Replaying a decision inside the selected body must keep its selector's
				// answer. A way before this switch discards this sealed entry on retry.
				call = $"selected{_ways++}";
				code.Line($"int {call};");
				using (code.Block("if (ways.Cursor < ways.Count)"))
					code.Line($"{call} = ways.Items[ways.Cursor++ * 2];");
				code.Line("else");
				using (code.Block(""))
				{
					var selector = EmitGuardCall(code, selection.Selector, selection);
					code.Line($"{call} = {selector};");
					code.Line($"ways.Open({call}, {call});");
				}
			}
			else
				call = EmitGuardCall(code, selection.Selector, selection);
			var apart = new PushedApart(this);
			using (code.Block($"switch ({call})"))
			{
				for (var i = 0; i < choice.Nodes.Count; i++)
				{
					code.Line($"case {i}:");
					using (code.Indent())
					using (code.Block(""))
					{
						apart.Next();
						Emit(code, choice.Nodes[i], following);
						code.Line("break;");
					}
				}
				code.Line("default:");
				using (code.Indent())
				{
					code.Line(Refusal("null"));
					code.Line(Fails());
				}
			}
			apart.Done();
		}

		void EmitGuard(Writer code, Node.Guard guard, string? expected = null)
		{
			var call = EmitGuardCall(code, guard);

			// What the guard answered, told where the build traces.
			if (machine.Tracing is { } tracing)
				call = $"Guarded_DotGram({tracing.GuardOf(guard)}, p, {call})";

			using (code.Block($"if (!{call})"))
			{
				if (expected is not null)
					machine._expectedUsed.Add(expected);

				code.Line(Refusal(expected ?? "null"));
				code.Line(Fails());
			}
		}

		/// <summary>What a guard that begins a dispatched group refuses with: the other groups' first tokens.</summary>
		string? _guardRefuses;

		/// <summary>
		/// The guard a dispatched choice asks before naming the group it leads, for the refusal
		/// the widest group's call makes (<see cref="_refuseWith"/>).
		/// </summary>
		(Node.Guard Guard, string Without)? _refuseUnless;

		/// <summary>
		/// A refusal that names <paramref name="expected"/> where the guard would let its group
		/// read, and <paramref name="without"/> where it would not. The guard is asked only when
		/// the refusal is recorded, so a reading that goes on pays nothing for it.
		/// </summary>
		void RefusedUnless(Writer code, string expected, Node.Guard guard, string without, (string Set, string Stood)? covered = null)
		{
			machine._expectedUsed.Add(expected);
			machine._expectedUsed.Add(without);

			// A text the guard reads is kept for the construction where the guard is asked on the
			// way in; asked again here, it must not take that place.
			var kept = new Dictionary<string, string>(_guardTexts, StringComparer.Ordinal);

			using (code.Block(machine.Quiets ? "if (!failure.Quiet)" : ""))
			{
				var call  = EmitGuardCall(code, guard);
				var named = ExpectedEither(call, expected, without);

				code.Line(covered is not { } over
					? $"{Refusing}(ref failure, p, {named});"
					: $"{Refusing}_Over(ref failure, p, {named}, {over.Set}, {over.Stood});");
			}

			_guardTexts.Clear();

			foreach (var one in kept)
				_guardTexts[one.Key] = one.Value;

			code.Line(Fails());
		}

		/// <summary>The guard an alternative's reading begins with, through what builds or names it.</summary>
		static Node.Guard? LeadingGuard(Node alternative)
		{
			var read = alternative;

			while (true)
			{
				switch (read)
				{
					case Node.Construct(var built, _):
						read = built;
						continue;

					case Node.Capture(_, var captured):
						read = captured;
						continue;

					case Node.Sequence(var parts) when parts.Count > 0:
						read = parts[0];
						continue;

					default:
						return read as Node.Guard;
				}
			}
		}

		/// <summary>Whether an alternative's reading begins with a guard, through what builds or names it.</summary>
		static bool LeadsWithGuard(Node alternative)
		{
			return LeadingGuard(alternative) is not null;
		}

		int _guardLocals;

		/// <summary>
		/// The text members a guard of this method cut into a string, by name, and the local
		/// holding each: what the rule's construction may take instead of cutting it again.
		/// </summary>
		readonly Dictionary<string, string> _guardTexts = new(StringComparer.Ordinal);

		/// <summary>Those locals' declarations, for the method's start.</summary>
		readonly List<string> _hoisted = [];

		/// <summary>A record's value as a guard sees it.</summary>
		string ValueAt(RuleSymbol rule, string record)
		{
			return machine.Carrier.ValueOf(rule, record);
		}

		void EmitLookahead(Writer code, bool positive, Node inside, bool loaded = false)
		{
			// A negative look whose subject cannot begin with any token the case label
			// admits has been answered by the label: `?!Reserved & RegularIdentifier` is
			// dispatched on the word's kind, and inside the group the reserved kinds chose
			// there is nothing left to look for.
			if (!positive && loaded && _dispatched is { IsKnown: true } chosen &&
				machine.Decidable(inside) is { Ends: false } subject && !subject.Overlaps(chosen))
			{
				return;
			}

			var seen = $"q{_calls++}";

			// Inside, the body is sealed — the same continuation an atomic group hands its
			// contents (EmitAtomic), and the same reason: a look is decided at its first
			// match and gives back what it read, so a repetition standing at the end of its
			// body is never asked for a shorter reading and owes no way. `?!LogSeparator`
			// over `' '* & '|' & ' '*` was opening one for the trailing run of spaces.
			var (call, undo, opens) = Called(inside, FollowSets.Continuation.Sealed);

			// What a look recorded is dropped whether it saw or not: its outcome is one bit,
			// and what it captured on the way to it is not the rule's. Over the tape, the body
			// is asked for a reading until it has one, as an atomic group's contents are — a
			// way it opened is a reading not yet tried, and `?!(Word & ?!'{')` has one wherever
			// a shorter word is not followed by the brace — and then what it decided is sealed:
			// nothing after it may reopen it, because a second reading of it can only say the
			// same. Where nothing inside can open a way there is nothing to retry or seal, and
			// nothing is written about ways.
			var mark    = _ways++;
			var sealing = _tape && opens;

			if (sealing)
				code.Line($"var s{mark}  = ways.Cursor;");

			foreach (var line in machine.Carrier.MarkRecords($"lm{mark}"))
				code.Line(line);
			foreach (var line in machine.Carrier.MarkGathered(owner, $"rr{mark}"))
				code.Line(line);
			// Whatever the look refuses on the way is not the parse's refusal: it is read and
			// given back either way. The engine records nothing inside its own lookahead; the
			// reader says so on the failure it carries, which every reading has, ways or not.
			code.Line("failure.Looking++;");
			machine._looks = true;

			if (sealing)
			{
				// A loop that asks again, noted with the others and never deferring: what a look reads is built by nothing.
				machine.Deferral(owner, RetrySite.Lookahead, noting: !analyzing);

				// Declared and not set: every way out of the loop below has just set it.
				code.Line($"int {seen};");
				code.Line();

				using (code.Block("while (true)"))
				{
					code.Line($"{seen} = {call};");
					code.Line();
					code.Line($"if ({seen} >= 0)");
					code.Then("break;");
					code.Line();
					LogBack(code, $"lm{mark}");
					foreach (var line in machine.Carrier.UnwindGathered(owner, $"rr{mark}"))
						code.Line(line);

					if (undo.Length > 0)
						code.Line(undo);

					code.Line();
					code.Line($"if (ways.Cursor > s{mark} && ways.Retry(s{mark}))");
					code.Then("continue;");
					code.Line();
					code.Line("break;");
				}
			}
			else
			{
				code.Line($"var {seen} = {call};");
			}

			code.Line("failure.Looking--;");
			code.Line();
			LogBack(code, $"lm{mark}");
			foreach (var line in machine.Carrier.UnwindGathered(owner, $"rr{mark}"))
				code.Line(line);

			if (undo.Length > 0)
				code.Line(undo);

			if (sealing)
				code.Line($"ways.Seal(s{mark});");

			code.Line();

			// A rule that is nothing but a look, the engine reads as a scan and names where it
			// refuses: `eof`. Said so here too, or a parse refused at the end by it after a loop
			// said only that the input did not match. A look inside a rule refuses silently, as it
			// did: what it says is left to what the rule reads next.
			var named = _graph.Bodies.TryGetValue(owner, out var body) &&
				body is Node.Lookahead(_, var looked) && ReferenceEquals(looked, inside) &&
				machine.ScannerOf(owner) is not null;

			// The end of the input written in place is said as the rule would be. Over kinds
			// every `eof` is: the built-in is rewritten through to its body there, and a parse
			// refused at the end after an optional said only that the input did not match,
			// where the engine names the end. Not in a rule the engine reads as a scan, nor in
			// the seam or what it calls, which the engine reads without recording.
			var ends = !positive && Machine.IsAny(inside) &&
				machine.ScannerOf(owner) is null && !machine.SeamReached.Contains(owner);

			if (ends)
			{
				using (code.Block($"if ({seen} >= 0)"))
					Refused(code, machine.EndOfInputExpected());

				return;
			}

			if (!named)
			{
				code.Line($"if ({seen} {(positive ? "<" : ">=")} 0)");
				code.Then(Fails());

				return;
			}

			using (code.Block($"if ({seen} {(positive ? "<" : ">=")} 0)"))
				Refused(code, machine.DeclareExpected([owner.Name]));
		}

		/// <summary>
		/// The log put back to a count — and with it the watermark of what a guard built,
		/// where anything builds: a record above the watermark is one written since, and
		/// a value a guard built in a derivation that was then abandoned is not the value
		/// of the record the next derivation writes at the same place.
		/// </summary>
		/// <summary>A line the carrier may have nothing to say at.</summary>
		static void Carried(Writer code, string text)
		{
			if (text.Length > 0)
				code.Line(text);
		}

		void LogBack(Writer code, string count)
		{
			foreach (var line in machine.Carrier.UnwindRecords(count))
				code.Line(line);
		}

		/// <summary>What a fold's loop could have gone round with, noted where it stops (§4.3).</summary>
		/// <remarks>
		/// A repetition ending is not a failure and records nothing, which is right — and for
		/// a repetition the author wrote it is the whole story. A fold's loop is the operator
		/// ladder: where the parse then fails at the token that ended it, what could have
		/// continued the expression is exactly what the message is missing, and the door's own
		/// first set is it. `x` and then `}` said everything but that a `+` could have stood
		/// there.
		/// </remarks>
		void NoteTails(Writer code, Node.Repeat repeat, Node body)
		{
			if (!machine.IsFoldLoop(repeat) || Doorway([body]) is not { } tails)
				return;

			code.Line(Noted(machine.DeclareExpected(
				machine.Displays(new Node.Element(false, [.. tails.Ranges], [], [])))));
		}

		/// <summary>A refusal recorded and not acted on: what was wanted here, for the message.</summary>
		string Noted(string expected)
		{
			machine._expectedUsed.Add(expected);

			// Braced where it asks first: a note stands between an `if` and its `else`, and an
			// `if` of its own there would take the `else` for itself.
			return machine.Quiets ? "{ " + Refusal(expected) + " }" : Refusal(expected);
		}

		void Refused(Writer code, string expected)
		{
			// Declaring the array is not asking for it: what is written out is what
			// something wrote a reference to, and until the reader said so the only thing
			// that ever did was the rendering beside it.
			machine._expectedUsed.Add(expected);

			code.Line(Refusal(expected));
			code.Line(Fails());
		}

		/// <summary>The statement that records a refusal here, asking first where a reading may be quiet.</summary>
		string Refusal(string expected)
		{
			return machine.Quiets
				? $"if (!failure.Quiet) {Refusing}(ref failure, p, {expected});"
				: $"{Refusing}(ref failure, p, {expected});";
		}
	}
}
