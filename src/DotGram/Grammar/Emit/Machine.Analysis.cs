using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// What the compiler proves about a grammar before deciding how to write it.
/// </summary>
/// <remarks>
/// <para>
/// Every one of these answers the same shape of question: is the general machinery needed
/// here, or can something cheaper be shown to mean the same? Whether a choice is settled by
/// one character, whether a repetition can ever be asked to give a turn back, whether a
/// body writes anything into the arena at all. The direction of proof is always that the
/// machinery is <em>not</em> needed — an analysis that cannot decide says so, and what gets
/// written is what would have been written anyway.
/// </para>
/// <para>
/// They are gathered because they belong together and because of where they are going.
/// `ExecutionPlan` holds the one decision that does not depend on a caller; these are the
/// ones that do, and they move there when a region can carry a context of its own. Until
/// then they are asked during compilation, from a context threaded down the tree.
/// </para>
/// </remarks>
sealed partial class Machine
{
	/// <summary>
	/// What must begin the input where <paramref name="node"/> begins, given what must
	/// begin it where the node ends.
	/// </summary>
	/// <remarks>
	/// A node that must consume something answers for itself. One that may match nothing
	/// leaves the question to what comes after it as well as to itself, so the two are taken
	/// together — the direction that admits too much, and so proves too little, rather than
	/// the one that proves something false.
	/// </remarks>
	FirstSets.First Precedes(Node node, FirstSets.First after)
	{
		var first = FirstSets.Of(node, _graph);

		return first.Nothing                      ? after :
			FirstSets.Nullable(node, _graph) ? first.Or(after) :
			first;
	}

	int WeightOfAll(IReadOnlyList<Node> nodes, int budget)
	{
		var total = 0;

		foreach (var node in nodes)
		{
			total += Weight(node, budget - total);

			if (total > budget)
				break;
		}

		return total;
	}

	/// <summary>
	/// Whether a node writes nothing into the arena, so that its failure is nobody's business
	/// but its own.
	/// </summary>
	/// <remarks>
	/// The arena is what a failure is unwound through: an entry written on the way in is
	/// taken back on the way out, and jumping past the dispatcher would leave it there. A
	/// node that writes none — text matched against the input, alternatives one character
	/// tells apart, rules small enough to be compiled in place — has nothing to take back,
	/// and its failure can go straight wherever the caller wants it.
	/// <para>
	/// An external recognizer writes nothing either: it is compiled as one <c>if (!method(
	/// text, ref p)) goto Fail;</c>, and docs/syntax.md §7.2 requires the method itself to
	/// restore <c>pos</c> on any outcome but success — the same promise a literal keeps by
	/// never moving <c>p</c> before its test fails. Nothing here can see whether the promise
	/// was kept; that is on the C# it is trusting, same as every other guarantee §7.2 asks
	/// for and does not check.
	/// </para>
	/// <para>
	/// A rule that climbs precedence is refused wholesale: its states are re-entered with
	/// binding powers, and the climb's bookkeeping is exactly the arena traffic silence
	/// claims not to have. The refusal is the owning rule's, not the grammar's — one
	/// <c>&lt;&lt;</c> anywhere used to cost every rule in the file its proofs, which is how
	/// an optional two rules away from the climb was found renting a parser.
	/// </para>
	/// </remarks>
	bool Silent(Node node, FollowSets.Continuation following) =>
		!(_owners.TryGetValue(node, out var owner) && _graph.Climbing.ContainsKey(owner)) &&
		node switch
		{
			Node.Empty or Node.Literal or Node.Element or Node.External => true,

			// One comparison against the character behind, no entry — its compile already
			// routes failure through `_fail` like every other silent node.
			Node.Behind                                => true,

			// A lookahead over a silent body needs no entry either: the body writes
			// nothing, so entering is a checkpoint local and leaving is putting the
			// position back — both directions, since a negative lookahead's failure is
			// its body succeeding. "Anything" for the body's own continuation: what
			// follows the lookahead does not follow the body, which is rewound.
			Node.Lookahead(_, var seen)                => SilentWithin(seen, FollowSets.Continuation.All),

			// A capture kept in locals writes nothing — sound only where nothing ever
			// backtracks over it, which is what every other case here already proves,
			// and only the flat-value rendering compiles it that way. A capture of a
			// flat-valued call is the call's body compiled in place, silent when it is.
			Node.Capture(_, var captured)              => _valuesInLocals &&
			                                              (SiteCallee(node) is { } called
			                                                  ? Silent(_graph.Bodies[called], following)
			                                                  : Silent(captured, following)),

			// The single construction a flat-value method runs at Accept, once the
			// whole parse is decided — deferred construction kept, no entry written.
			Node.Construct(var built, _)               => _valuesInLocals && Silent(built, following),

			Node.Sequence(var parts)                   => AllSilent(parts, following),
			// Three ways a choice writes nothing. One character telling every alternative
			// apart is the first, and the second is the whole choice being one run of
			// literals: `CompileLiterals` decides those where their texts differ and never
			// comes back, so it writes no way back either — which `LiteralRun` is already
			// the test for, since it admits a run only where every pair in it is settled.
			// The third is the checkpoint class: a choice that does need coming back to,
			// whose way back three locals hold — sound only where failure routes through
			// `Fail:`, which is what <see cref="_checkpointsAllowed"/> stands for, and
			// only in the valueless rendering, whose retries have no captures to unset.
			Node.Choice(var alternatives)              => Predictive(alternatives) is not null &&
			                                              AllSilent(alternatives, following, sequence: false) ||
			                                              LiteralRun(
			                                                  alternatives,
			                                                  alternatives.Count - 1,
			                                                  following.Plain) == alternatives.Count ||
			                                              CheckpointSilent(alternatives, following),
			// A scanner call is one method call that writes nothing; failing one already
			// goes through `_fail`. Otherwise the call is silent when its inlined body is.
			Node.Call(var rule, _)                     => ScannerOf(rule) is not null ||
			                                              CanInline(rule) &&
			                                              _graph.Bodies.TryGetValue(rule, out var called) &&
			                                              Silent(called, following),

			// A repetition inside another is silent exactly when it is itself the loop and
			// nothing else — which is the same question, asked of it. `Path = ('/' & Segment)*`
			// with `Segment` a repetition of its own is the shape this was refusing, and it is
			// the shape most path-like grammars are written in.
			Node.Repeat repeat                         => SilentRepeat(repeat, following),

			// An atomic group is first-match-commits, and that is a shape locals can hold:
			// try each alternative in order through the give-back door, and the first that
			// matches is final — nothing ever comes back, which is what "atomic" says.
			// The alternatives may share prefixes freely; what each must be is silent.
			// Not where the machine recovers: §8.2's discriminator rests on the commit
			// marking the element owned, and that mark is the engine's.
			Node.Atomic(var kept)                      => _recoveries.Count == 0 &&
			                                              (kept is Node.Choice(var options)
			                                              ? AllSilentWithin(options, following)
			                                              : SilentWithin(kept, following)),

			_                                          => false,
		};

	/// <summary>
	/// Whether a choice neither of the first two ways admitted may still keep its way
	/// back in locals — the checkpoint class. Asked last, so a run of literals or a
	/// predicted choice keeps the form it always had. Answering yes marks the machine
	/// as one whose failures can tie (<see cref="Ties"/>), which the emitted
	/// <c>Failure</c> struct and the wrapper both need to know before a line of the
	/// method is rendered.
	/// </summary>
	bool CheckpointSilent(IReadOnlyList<Node> alternatives, FollowSets.Continuation following)
	{
		if (!_checkpointsAllowed || _valuesInLocals ||
			!AllSilent(alternatives, following, sequence: false))
			return false;

		Ties = true;

		return true;
	}

	/// <summary>
	/// <see cref="Silent"/>, inside a construct whose failures leave by a door rather
	/// than through <c>Fail:</c> — where a pending checkpoint site would be jumped past,
	/// so none may open. The compile of each such construct puts the same flag down.
	/// </summary>
	bool SilentWithin(Node node, FollowSets.Continuation following)
	{
		var checkpoints = _checkpointsAllowed;

		_checkpointsAllowed = false;

		try
		{
			return Silent(node, following);
		}
		finally
		{
			_checkpointsAllowed = checkpoints;
		}
	}

	/// <summary>The alternatives' half of <see cref="SilentWithin"/>.</summary>
	bool AllSilentWithin(IReadOnlyList<Node> nodes, FollowSets.Continuation following)
	{
		var checkpoints = _checkpointsAllowed;

		_checkpointsAllowed = false;

		try
		{
			return AllSilent(nodes, following, sequence: false);
		}
		finally
		{
			_checkpointsAllowed = checkpoints;
		}
	}

	/// <summary>
	/// Whether a repetition is a loop and nothing else — no entry, no count, no way back.
	/// </summary>
	/// <remarks>
	/// Asked in two places and it has to answer the same in both: here, to know whether the
	/// thing around it writes nothing, and at the point of compiling it, to decide what to
	/// write. Different answers would mean jumping past entries that were made after all.
	/// </remarks>
	bool SilentRepeat(Node.Repeat repeat, FollowSets.Continuation following) =>
		(repeat.Max ?? repeat.Min + 1) * Weight(repeat.Body, Unrollable) <= Unrollable &&
		Possessive(repeat.Body, following) &&
		SilentWithin(
			repeat.Body,
			following.Or(new FollowSets.Continuation(
				FirstSets.Of(repeat.Body, _graph), FirstSets.Of(repeat.Body, _graph))));

	/// <summary>
	/// Every one of them, each followed by what follows it.
	/// </summary>
	/// <remarks>
	/// Threaded the way compilation threads it, because it is the same question about the
	/// same nodes: a part of a sequence is followed by the rest of the sequence, and an
	/// alternative of a choice is followed by whatever the choice is.
	/// </remarks>
	bool AllSilent(IReadOnlyList<Node> nodes, FollowSets.Continuation following, bool sequence = true)
	{
		var after = following;

		for (var i = nodes.Count - 1; i >= 0; i--)
		{
			if (!Silent(nodes[i], after))
				return false;

			if (sequence)
				after = FollowSets.Precedes(nodes[i], after, _graph, _seam);
		}

		return true;
	}

	/// <summary>
	/// How much a repetition may be written out one after another rather than looped, counted
	/// in the states the turns would come to.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Unrolling is what removes the count, and with it the last thing the arena was holding
	/// for a repetition that needs it for nothing else. Generated size is not a cost this
	/// project minimizes, but it is not unbounded either, and it does not add — it multiplies.
	/// <c>(H16 &amp; ':'){6}</c> is six copies of <c>H16</c>, each of which is
	/// <c>Hex{1,4}</c>, and the rule that holds it has nine alternatives; counting turns
	/// alone would call each of those small and arrive at hundreds of copies of one character
	/// test.
	/// </para>
	/// <para>
	/// So the budget is turns times what a turn weighs, and a turn weighs what it will
	/// actually be written as — through the calls that are compiled in place, and through the
	/// repetitions inside it, which multiply in their turn.
	/// </para>
	/// </remarks>
	const int Unrollable = 24;

	/// <summary>
	/// About how many states a node will come to, stopping once that is more than is being
	/// asked about.
	/// </summary>
	int Weight(Node node, int budget)
	{
		if (budget <= 0)
			return 1;

		switch (node)
		{
			case Node.Empty:
				return 0;

			case Node.Sequence(var parts):
				return WeightOfAll(parts, budget);

			case Node.Choice(var alternatives):
				return WeightOfAll(alternatives, budget);

			case Node.Capture(_, var captured):
				return 1 + Weight(captured, budget - 1);

			case Node.Construct(var built, _):
				return 1 + Weight(built, budget - 1);

			case Node.Atomic(var kept):
				return 1 + Weight(kept, budget - 1);

			case Node.Marked(var kept, _):
				return 1 + Weight(kept, budget - 1);

			case Node.Lookahead(_, var seen):
				return 1 + Weight(seen, budget - 1);

			// An unbounded one is written once and gone round, so what it weighs is a turn
			// and the going round; a bounded one is written out as many times as it is
			// allowed to happen.
			case Node.Repeat(var body, _, var max):
				return (max ?? 2) * Weight(body, budget);

			case Node.Call(var rule, _) when CanInline(rule) && _graph.Bodies.TryGetValue(rule, out var called):
				return Weight(called, budget);

			default:
				return 1;
		}
	}

	/// <summary>
	/// Whether a repetition can be run to its end and never asked to give any of it back.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A repetition normally leaves one resume point per turn, because a later failure may
	/// mean it went one turn too far. Two facts together say it never did.
	/// </para>
	/// <para>
	/// The first is that what follows cannot begin with what the body begins with. Every
	/// place the repetition could stop short is a place a turn began, so the character there
	/// is one the body starts with; the continuation would have to start with that same
	/// character and, by disjointness, cannot. The second is that the body matches in one way
	/// only. Without it the first is not enough: a body that can match two lengths can end
	/// the repetition somewhere no turn ever began, and nothing has been said about the
	/// character there. <c>("ab" | "a")*</c> against <c>aab</c> is that case, and it is why
	/// the length has to be settled before the first sets are allowed to decide anything.
	/// </para>
	/// <para>
	/// Both are asked of what is known here. An unknown first set is "anything", which
	/// overlaps; an unknown continuation is nothing, which proves nothing; either answers no,
	/// and the general machinery stays.
	/// </para>
	/// </remarks>
	/// <summary>
	/// Whether a repetition need never hand a completed turn back.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Weaker than <see cref="Possessive"/>, deliberately. That one licenses compiling a
	/// repetition as a plain loop with nothing recorded, so the body must match one way
	/// only. This licenses removing just the repetition's own ways back — everything the
	/// body records stays recorded — and for that the first sets suffice: an exit at a
	/// completed turn's start would have the continuation begin where the turn began,
	/// on a character the turn's first element read, and disjointness says it cannot.
	/// </para>
	/// <para>
	/// A body that leads with the seam is compared past it. Both the turn and the
	/// continuation begin by reading the same trivia, so the characters that decide are
	/// the ones after it — <see cref="FollowSets.Continuation.AfterSeam"/>'s half. Two
	/// more things must then hold: the continuation must not be able to start <em>inside</em>
	/// what the seam consumed, which <see cref="Contained"/> bounds, and the rest of the
	/// turn must consume — a turn that is all trivia decides nothing.
	/// </para>
	/// </remarks>
	bool NeverGivesBack(Node.Repeat repeat, FollowSets.Continuation following)
	{
		var body = repeat.Body;

		if (FirstSets.Nullable(body, _graph))
			return false;

		if (_seam is not null &&
			body is Node.Sequence(var parts) && parts.Count > 1 &&
			parts[0] is Node.Call(var called, _) && ReferenceEquals(called, _seam))
		{
			var contained = Contained(_seam);
			var rest      = parts.Count == 2 ? parts[1] : new Node.Sequence([.. parts.Skip(1)]);
			var decides   = FirstSets.Of(rest, _graph);

			return !FirstSets.Nullable(rest, _graph) &&
				!decides.Overlaps(following.AfterSeam) &&
				!following.AfterSeam.Overlaps(contained);
		}

		return !FirstSets.Of(body, _graph).Overlaps(following.Plain);
	}

	/// <summary>
	/// The characters a continuation could meet by starting inside a span the seam
	/// consumed, rather than after it.
	/// </summary>
	/// <remarks>
	/// A star's shorter readings stop at unit boundaries, so what a boundary can stand
	/// before is a unit's first character — as long as every unit is rigid. A unit that
	/// can itself match several lengths, a comment with a body being the one that
	/// matters, makes a boundary of every position it spans, and everything it can hold
	/// is the answer. An atomic seam has one reading and no boundaries at all, which is
	/// the door §3's braces already give an author whose trivia holds comments.
	/// </remarks>
	FirstSets.First Contained(RuleSymbol seam)
	{
		if (!_graph.Bodies.TryGetValue(seam, out var body))
			return FirstSets.First.All;

		return body switch
		{
			Node.Atomic                 => FirstSets.First.None,
			Node.Empty                  => FirstSets.First.None,
			Node.Repeat(var unit, _, _) => Boundaries(unit),
			_                           => FirstSets.First.All,
		};
	}

	FirstSets.First Boundaries(Node unit) => unit switch
	{
		Node.Element                => FirstSets.Of(unit, _graph),
		Node.Literal(var text)      => text.Length == 0
			? FirstSets.First.None
			: FirstSets.First.Chars([new CharRange(text[0], text[0])]),
		Node.Choice(var alternatives) => alternatives.Aggregate(
			FirstSets.First.None, (set, alternative) => set.Or(Boundaries(alternative))),
		Node.Sequence(var sequenceParts) when sequenceParts.All(
			static part => part is Node.Literal or Node.Element)
			=> FirstSets.Of(unit, _graph),
		_ => FirstSets.First.All,
	};

	/// <summary>
	/// Whether a repetition of this body may run to its end and never be asked to give a
	/// turn back — asked of the model, which is where the question lives now.
	/// </summary>
	bool Possessive(Node body, FollowSets.Continuation following) =>
		Determinism.Possessive(body, following, _graph, _seam);

	/// <summary>
	/// The character tests that decide a choice outright, or null where the input does not.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A choice normally has to be able to come back: it takes the first alternative that
	/// starts and, if that one fails later, tries the next. The entry it leaves behind is
	/// what makes coming back possible, and it is written on every visit whether or not
	/// anything ever comes back for it.
	/// </para>
	/// <para>
	/// Nothing ever does when the alternatives cannot begin with the same character. Suppose
	/// the character at hand belongs to the first set of one alternative and that alternative
	/// then fails. Any other alternative that could match here would have to begin with that
	/// same character, and by disjointness none does — so the choice fails with it, and the
	/// entry that would have been popped to discover this is pure cost. One character decides
	/// which alternative it is, and having decided, there is no second reading to keep.
	/// </para>
	/// <para>
	/// Every alternative must also consume something. An alternative that can match nothing
	/// matches everywhere, so it stays reachable after another has failed, and that is
	/// exactly the alternative an entry is needed for. First sets are approximate in the
	/// direction that says "anything" when unsure, and two of those overlap, so an
	/// alternative this cannot read gives up the optimization rather than mis-taking it.
	/// </para>
	/// </remarks>
	string[]? Predictive(IReadOnlyList<Node> alternatives)
	{
		if (!Determinism.Distinguishable(alternatives, _graph, Emitted))
			return null;

		var tests = new string[alternatives.Count];

		for (var i = 0; i < alternatives.Count; i++)
			tests[i] = RangesTest(FirstSets.Of(alternatives[i], _graph).Ranges);

		return tests;
	}

	/// <summary>
	/// How many alternatives ending at <paramref name="at"/> are plain text, up to two or
	/// more.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Two is where it starts paying: one literal alternative is compiled as it always was,
	/// and the saving is in not writing down where to come back to between one literal and
	/// the next.
	/// </para>
	/// <para>
	/// A run is decided where its members differ, in the order they were written, and never
	/// comes back — so it is admitted only where no pair of them needs coming back for. Two
	/// that begin differently never do; a pair where one begins the other is the case
	/// <see cref="PrefixSettled"/> decides.
	/// </para>
	/// </remarks>
	/// <summary>
	/// How many alternatives ending at <paramref name="at"/> are plain text and may be
	/// compiled as one, whether or not any of them needs a way back.
	/// </summary>
	/// <remarks>
	/// <see cref="LiteralRun"/> asks whether the run needs no way back at all, which is what
	/// decides whether the choice is silent. This asks the wider question the compiler needs:
	/// whether the texts can be compared together, sharing their prefix and testing only
	/// their tails. A later alternative that continues an earlier one — `"http" | "https"` —
	/// is admitted here and refused there, because it is compiled with a way back to the
	/// continuation rather than without one.
	///
	/// The other direction stays with <see cref="PrefixSettled"/>: an earlier alternative
	/// that is <em>longer</em> may need coming back to the shorter one written after it, and
	/// whether it does depends on what follows the choice, which is not a fact about the
	/// texts.
	/// </remarks>
	static int LiteralGroup(IReadOnlyList<Node> alternatives, int at, FirstSets.First following)
	{
		var run = 0;

		while (at - run >= 0 && alternatives[at - run] is Node.Literal { IgnoreCase: false })
			run++;

		if (run < 2)
			return 0;

		for (var i = at - run + 1; i <= at; i++)
			for (var j = i + 1; j <= at; j++)
				if (alternatives[i] is Node.Literal(var earlier) &&
					alternatives[j] is Node.Literal(var later) &&
					!later.StartsWith(earlier, StringComparison.Ordinal) &&
					!PrefixSettled(earlier, later, following))
				{
					return 0;
				}

		return run;
	}

	static int LiteralRun(IReadOnlyList<Node> alternatives, int at, FirstSets.First following)
	{
		var run = 0;

		// An ignore-case literal opts out of this run: its shared-prefix read would have
		// to be case-folded too, and its own comparison already differs from an ordinary
		// literal's — left for the general path, the same first-cut choice `Predictive`
		// makes for it via `First.All` (docs/status.md).
		while (at - run >= 0 && alternatives[at - run] is Node.Literal { IgnoreCase: false })
			run++;

		if (run < 2)
			return 0;

		for (var i = at - run + 1; i <= at; i++)
			for (var j = i + 1; j <= at; j++)
				if (alternatives[i] is Node.Literal(var one) &&
					alternatives[j] is Node.Literal(var other) &&
					!PrefixSettled(one, other, following))
				{
					return 0;
				}

		return run;
	}

	/// <summary>
	/// Whether two literal alternatives, <paramref name="first"/> written before
	/// <paramref name="second"/>, can be decided where they differ and never returned to.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Only a pair where one begins the other is in question; anything else differs at a
	/// character, and the character decides it.
	/// </para>
	/// <para>
	/// Written longer-first, the shorter reading is the one that would be come back for,
	/// and where it stands is known exactly: at the character the longer went on with. If
	/// what follows the choice cannot begin with that character, the shorter reading fails
	/// wherever it is tried, and an entry that leads only to a failure is one nothing needs.
	/// <c>"https" | "http"</c> before <c>"://"</c> is that: taking <c>"http"</c> leaves an
	/// <c>'s'</c>, and <c>"://"</c> does not begin with one.
	/// </para>
	/// <para>
	/// Written shorter-first it is the reverse, and the entry is the whole of what makes the
	/// longer reachable: <c>"http" | "https"</c> takes <c>"http"</c> first, and only coming
	/// back for the second alternative can ever match the extra character. §11 of
	/// docs/syntax.md promises alternatives are never reordered, so this is a fact about the
	/// grammar as written and not one to be optimized away.
	/// </para>
	/// <para>
	/// An unknown following — a complement, a category, a predicate — says nothing that can
	/// be held to, and the general machinery stays. The end of the input is not unknown: no
	/// character is the end of the text, so nothing that must read a character can begin
	/// there.
	/// </para>
	/// </remarks>
	static bool PrefixSettled(string first, string second, FirstSets.First following)
	{
		if (first.Length == second.Length)
			return !string.Equals(first, second, StringComparison.Ordinal);

		if (first.Length < second.Length)
			return !second.StartsWith(first, StringComparison.Ordinal);

		if (!first.StartsWith(second, StringComparison.Ordinal))
			return true;

		var carriedOn = first[second.Length];

		return following.IsKnown &&
			!following.Overlaps(new FirstSets.First(false, false, [new CharRange(carriedOn, carriedOn)]));
	}

	/// <summary>What a run of literal alternatives can begin with.</summary>
	static FirstSets.First Begins(IReadOnlyList<Node> alternatives, int from, int to)
	{
		var ranges = new List<CharRange>();

		for (var i = from; i <= to; i++)
			if (alternatives[i] is Node.Literal(var text))
			{
				if (text.Length == 0)
					return FirstSets.First.All;

				ranges.Add(new CharRange(text[0], text[0]));
			}

		return FirstSets.First.Chars(ranges);
	}

	/// <summary>
	/// What an alternative must begin with, where that is known well enough to turn one
	/// away by.
	/// </summary>
	/// <remarks>
	/// Three things make it unusable, and all three are the approximation admitting it does
	/// not know: a first set of "anything" excludes no character, one of "nothing" describes
	/// something that consumes none, and an alternative that can match empty matches
	/// everywhere whatever its first set says. Any of them and the alternative is tried as it
	/// always was.
	/// </remarks>
	FirstSets.First? Decidable(Node alternative)
	{
		var first = FirstSets.Of(alternative, _graph);

		return first.Anything || first.Nothing || first.Ranges.Count > Emitted ||
			FirstSets.Nullable(alternative, _graph)
				? null
				: first;
	}

	/// <summary>
	/// The widest first set a rendered test may be written from.
	/// </summary>
	/// <remarks>
	/// Guards the two places an analysis result becomes source text — <see cref="Predictive"/>
	/// and <see cref="Decidable"/> — and nothing else: an analysis that only compares sets is
	/// better off exact whatever their size.
	/// </remarks>
	const int Emitted = 8;

	/// <summary>A test over <c>c</c> for membership of a set of ranges.</summary>
	static string RangesTest(IReadOnlyList<CharRange> ranges)
	{
		var tests = new string[ranges.Count];

		for (var i = 0; i < ranges.Count; i++)
			tests[i] = ranges[i].IsSingle
				? $"c == {CSharpEmitter.Char(ranges[i].From)}"
				: $"(c >= {CSharpEmitter.Char(ranges[i].From)} && c <= {CSharpEmitter.Char(ranges[i].To)})";

		return string.Join(" || ", tests);
	}

	/// <summary>
	/// The character test a repetition's body is, or null where the body is anything more.
	/// </summary>
	/// <remarks>
	/// A body that consumes exactly one character and keeps nothing is the case where the
	/// general machinery is pure overhead: it has no choice to resume, no capture to record
	/// and no frame to return to, so every iteration's arena traffic is bookkeeping about
	/// nothing. The test is written against <c>c</c>, like every other element test.
	/// </remarks>
	string? RunTest(Node body)
	{
		switch (body)
		{
			case Node.Element element:
			{
				var test = CSharpEmitter.Test(element);

				return test == "false" ? null : test;
			}

			case Node.Literal(var value) { IgnoreCase: false } when value.Length == 1:
				return $"c == {CSharpEmitter.Char(value[0])}";

			// A rule that is inlined anyway is its body written somewhere else, and a grammar
			// names its character classes far more often than it spells them out.
			case Node.Call(var rule, _) when CanInline(rule):
				return RunTest(_graph.Bodies[rule]);

			case Node.Sequence(var nodes) when nodes.Count == 1:
				return RunTest(nodes[0]);

			// Alternatives that each consume exactly one character and keep nothing are a
			// disjunction, not a choice: whichever one matched, the position afterwards is the
			// same and so is the continuation, so there is nothing to come back to.
			case Node.Choice(var alternatives):
			{
				var tests = new string[alternatives.Count];

				for (var i = 0; i < alternatives.Count; i++)
					if (RunTest(alternatives[i]) is { } test)
						tests[i] = test == "true" ? "true" : $"({test})";
					else
						return null;

				return string.Join(" || ", tests);
			}

			default:
				return null;
		}
	}
}
