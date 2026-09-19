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

			// One test of the reading, no entry, its failure routed like the look-behind's.
			Node.Reading                               => true,

			// Nothing read and nothing kept, whichever half of a split grammar it lands in.
			Node.Glue                                  => true,

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
			Node.Choice { Selection: not null }        => false,
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
			Node.Atomic(var kept)                      => kept is Node.Choice(var options) { Selection: null }
			                                              ? AllSilentWithin(options, following)
			                                              : SilentWithin(kept, following),

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
		// One estimate and two proofs, and the order is not an accident: the estimate only
		// chooses between two shapes that both mean the repetition, and the proofs are what
		// say the silent one is available at all. Too heavy to unroll answers no here and
		// the general machinery stays, which is the safe direction for a guess to fail in.
		Unrolls(repeat) &&
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
		if (!Determinism.Distinguishable(alternatives, _graph))
			return null;

		var tests = new string[alternatives.Count];

		for (var i = 0; i < alternatives.Count; i++)
			tests[i] = RangesTest(FirstSets.Of(alternatives[i], _graph).Ranges, Tabulate);

		return tests;
	}

	/// <summary>
	/// How many groups of alternatives make a first character worth switching on, and how
	/// many characters that switch may name.
	/// </summary>
	/// <remarks>
	/// Below four groups the chain is already about as short: it does not test the
	/// alternatives one by one but the <em>groups</em> one by one, because
	/// <see cref="Skipped"/> jumps a failed character test past everything that begins the
	/// same way. Four groups is four tests against one switch, and that is where a switch
	/// starts being the shorter road rather than merely a different one.
	///
	/// The character cap is what keeps a switch a switch. A jump table is a table, and a
	/// group whose first set is a Unicode letter category would name tens of thousands of
	/// them; the sets this is aimed at are alphabets — fifty-two characters for a list of
	/// case-insensitive keywords, which is what standard SQL's reserved words come to.
	/// </remarks>
	const int Grouped = 4;
	const int Switched = 128;

	/// <summary>
	/// The alternatives gathered by what they can begin with — or null where the first
	/// character does not divide them.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <see cref="Predictive"/> asks whether one character decides <em>which</em> alternative
	/// this is, and a list of keywords never lets it: <c>AND</c>, <c>ALL</c>, <c>ANY</c> and
	/// <c>AS</c> all begin with <c>A</c>. But one character does decide which <em>group</em>
	/// it is, and that is the useful half of the same fact. Sixty keywords are twenty-six
	/// groups, and the parse can be put into the right one by a switch rather than walked
	/// into by testing the groups in turn.
	/// </para>
	/// <para>
	/// A group is the alternatives a character can begin, in the order they were written:
	/// the characters are cut wherever any first set begins or ends, and between two cuts
	/// every character begins the same alternatives. An alternative outside the chosen
	/// group cannot match here whatever order it was written in, so leaving it untried is
	/// not a reordering of the choice but the removal of alternatives that were going to
	/// fail. Within a group the written order is kept exactly, and so is every way back
	/// between them. Sets that overlap without being equal — <c>NOT? BETWEEN</c>,
	/// <c>NOT? IN</c> and <c>NOT? LIKE</c>, which share <c>NOT</c> and nothing else — put
	/// the shared character in a group of the three and each other character in a group
	/// of its own, which is the switch a person writes; before, they were the one shape
	/// that could not be dispatched, and a predicate's tail was tried alternative by
	/// alternative on every clause of the SQL yardstick.
	/// </para>
	/// <para>
	/// Every member of a group admits every character that chooses it, which is what makes
	/// the group's own chain cheaper than the same chain standing alone — see the
	/// <c>proven</c> argument of <c>CompileChainedChoice</c>. Both halves of that follow
	/// from the switch being the only way in.
	/// </para>
	/// </remarks>
	/// <param name="least">
	/// How many groups make a switch worth writing. Four for the automaton, where the
	/// alternatives it would replace are tests over characters; two for the reader, where
	/// each is a call into a method that refuses, and a person would have switched on the
	/// token.
	/// </param>
	/// <summary>
	/// The alternatives in the order a chain of first-character tests reads them, narrowest
	/// first — or null where one character does not tell every one of them from every other.
	/// </summary>
	/// <remarks>
	/// <para>
	/// What <see cref="Dispatchable"/> refuses for being too wide rather than for not deciding:
	/// a switch names its characters, and a class written as its complement, <c>[^ '"' | '\\']</c>,
	/// is sixty-five thousand of them. The first character decides all the same, and where it
	/// does, no alternative but the one it chose can begin here, so there is nothing to come
	/// back for either — the way the ordered reading opened into such a choice led nowhere.
	/// </para>
	/// <para>
	/// Written order is not kept, and need not be: where no two alternatives can begin with the
	/// same character, at most one of them can match, whichever is tried first. Narrowest first
	/// so that the widest, usually a complement, is the one read without a test of its own.
	/// </para>
	/// </remarks>
	/// <summary>
	/// Whether no alternative can hold where another one did: what each can begin with at a
	/// position — its first set, or for a lookahead what it looks for, the end of the input
	/// included — is disjoint from what every other can.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Then a way into the choice leads nowhere: coming back for the next alternative asks it to
	/// hold at the position where the one taken held, and nothing it can begin with is there.
	/// <c>(eol | ?=eof)</c> is the shape: <c>eol</c> read a character, and the end of the input
	/// is not one.
	/// </para>
	/// <para>
	/// Wider than <see cref="Chainable"/> by the lookaheads, which read nothing and so have no
	/// first set a switch or a chain could test before trying them; tried in order they need
	/// none, since the order no longer matters to what holds.
	/// </para>
	/// </remarks>
	internal bool Exclusive(IReadOnlyList<Node> alternatives)
	{
		var seen = new List<(FirstSets.First? Set, bool End)>(alternatives.Count);

		foreach (var alternative in alternatives)
		{
			if (Begins(alternative) is not { } one)
				return false;

			foreach (var other in seen)
				if (one.End && other.End || one.Set is { } mine && other.Set is { } theirs && mine.Overlaps(theirs))
					return false;

			seen.Add(one);
		}

		return true;

		// What an alternative can begin with, or null where that is not known: a lookahead for
		// the end holds only there, a lookahead for something that reads holds where that thing
		// begins, and anything else where its first set says.
		(FirstSets.First? Set, bool End)? Begins(Node node) => node switch
		{
			Node.Lookahead(false, var body) when Everything(body) => (null, true),
			Node.Lookahead(true, Node.Lookahead(false, var body)) when Everything(body) => (null, true),
			// `eof` called as a rule rather than written in place: FIX's `(Separator | eof)`.
			Node.Call(var called, { Count: 0 }) when _graph.Bodies.TryGetValue(called, out var body) &&
				body is Node.Lookahead(false, var any) && Everything(any) => (null, true),
			Node.Lookahead(true, var body) => Consumes(body) is { } set ? (set, false) : null,
			_ => Decidable(node) is { Ends: false } set ? (set, false) : null,
		};

		FirstSets.First? Consumes(Node body) =>
			!FirstSets.Nullable(body, _graph) && FirstSets.Of(body, _graph) is { Nothing: false, Anything: false } set ? set : null;

		// One character of anything, which only the end of the input refuses: `eof` is `?!any`.
		bool Everything(Node body) =>
			!FirstSets.Nullable(body, _graph) &&
			FirstSets.Of(body, _graph) is var set &&
			(set.Anything || set.Ranges.Count == 1 && set.Ranges[0].From == char.MinValue && set.Ranges[0].To == char.MaxValue) &&
			body is Node.Element;
	}

	/// <summary>
	/// Whether a choice is all text, each pair of whose texts can be decided where they differ
	/// and never returned to — what <see cref="LiteralRun"/> asks, of more choices.
	/// </summary>
	/// <remarks>
	/// <para>
	/// For the reader's way back only; the engine's <see cref="LiteralRun"/> compiles the run
	/// itself and is left as it is. Two things more are text here. A text captured or built
	/// from, <c>"?1" => @(true) | "?0" => @(false)</c>, which is the same text to the choice.
	/// And a text that ignores case, compared ignoring case: <c>"abc"i</c> begins
	/// <c>"ab"i</c>, and the character the longer one went on with is wanted by what follows
	/// in either case or not at all.
	/// </para>
	/// <para>
	/// Where the reader reads such a choice in order and opens no way, a failure after it has
	/// nothing to come back for: the texts that differ cannot both match, and a shorter text
	/// written after a longer one it begins leaves a character nothing after it takes.
	/// </para>
	/// </remarks>
	internal bool SettledText(IReadOnlyList<Node> alternatives, FirstSets.First following)
	{
		var texts = new List<Node.Literal>(alternatives.Count);

		foreach (var alternative in alternatives)
		{
			if (Bare(alternative) is not Node.Literal { Text.Length: > 0 } text)
				return false;

			texts.Add(text);
		}

		if (texts.Count < 2)
			return false;

		for (var i = 0; i < texts.Count; i++)
			for (var j = i + 1; j < texts.Count; j++)
				if (!Settled(texts[i], texts[j]))
					return false;

		return true;

		bool Settled(Node.Literal first, Node.Literal second)
		{
			if (!first.IgnoreCase && !second.IgnoreCase)
				return PrefixSettled(first.Text, second.Text, following);

			var how = StringComparison.OrdinalIgnoreCase;

			if (first.Text.Length == second.Text.Length)
				return !string.Equals(first.Text, second.Text, how);

			if (first.Text.Length < second.Text.Length)
				return !second.Text.StartsWith(first.Text, how);

			if (!first.Text.StartsWith(second.Text, how))
				return true;

			var carriedOn = first.Text[second.Text.Length];
			var upper     = char.ToUpperInvariant(carriedOn);
			var lower     = char.ToLowerInvariant(carriedOn);

			return following.IsKnown &&
				!following.Overlaps(FirstSets.First.Chars(FirstSets.First.Normalized(
					[new CharRange(carriedOn, carriedOn), new CharRange(upper, upper), new CharRange(lower, lower)])));
		}
	}

	/// <summary>
	/// Alternatives that all begin with the seam and that the first character after it tells
	/// apart — each with what it begins with past the seam, narrowest first — or null.
	/// </summary>
	/// <remarks>
	/// A fold's operators are written that way: <c>trivia &amp; '+' &amp; trivia &amp; r: Expr =&gt; …
	/// | trivia &amp; '-' &amp; …</c>. Each alternative begins with the same run of trivia, so no first
	/// set tells them apart, and the ordered reading opened a way into them. Past the trivia the
	/// operator does. The alternatives are not taken apart for it — their constructions are keyed
	/// by the fold — so the reader looks past the seam once and reads the alternative it chose
	/// whole, trivia included. That is sound where the seam leaves nothing to come back to and
	/// builds nothing (<see cref="ReadOnce"/>): read twice from one place, it answers the same.
	/// </remarks>
	internal List<(FirstSets.First Set, Node Node)>? PastTheSeam(IReadOnlyList<Node> alternatives)
	{
		if (alternatives.Count < 2 || _seam is not { } seam || !ReadOnce(seam))
			return null;

		var rests = new List<Node>(alternatives.Count);

		foreach (var alternative in alternatives)
		{
			if (Bare(alternative) is not Node.Sequence(var parts) || parts.Count < 2 ||
				parts[0] is not Node.Call(var called, { Count: 0 }) || !ReferenceEquals(called, seam))
				return null;

			rests.Add(parts.Count == 2 ? parts[1] : new Node.Sequence([.. parts.Skip(1)]));
		}

		if (Chainable(rests) is not { } chain)
			return null;

		// Chainable ordered the rests; hand the alternatives back in that order.
		return [.. chain.Select(one => (one.Set, alternatives[rests.IndexOf(one.Node)]))];
	}

	/// <summary>
	/// Whether the seam's reading opens no way and builds nothing: an atomic group, or a run of
	/// one class that never gives a character back — `trivia = [' ' | '\t']*` as most grammars
	/// write it, whose run the seam analysis settles.
	/// </summary>
	bool ReadOnce(RuleSymbol seam)
	{
		if (!_graph.Bodies.TryGetValue(seam, out var body))
			return false;

		if (_results.QualifiedOf(seam) is not null)
			return false;

		switch (Bare(body))
		{
			case Node.Atomic:
				return true;

			case Node.Repeat repeat when RunTest(repeat.Body) is not null:
				_follows ??= FollowSets.Of(_graph);

				return Determinism.NeverGivesBack(
					repeat,
					_follows.TryGetValue(seam, out var after) ? after : FollowSets.Continuation.All,
					_graph, FollowSets.SeamOf(seam, _graph));

			default:
				return false;
		}
	}

	internal List<(FirstSets.First Set, Node Node)>? Chainable(IReadOnlyList<Node> alternatives)
	{
		if (alternatives.Count < 2)
			return null;

		var chain = new List<(FirstSets.First Set, Node Node, long Width, int Index)>(alternatives.Count);

		for (var i = 0; i < alternatives.Count; i++)
		{
			if (Decidable(alternatives[i]) is not { Ends: false } set)
				return null;

			foreach (var other in chain)
				if (other.Set.Overlaps(set))
					return null;

			var width = 0L;

			foreach (var range in set.Ranges)
				width += range.To - range.From + 1;

			chain.Add((set, alternatives[i], width, i));
		}

		return [.. chain.OrderBy(static one => one.Width).ThenBy(static one => one.Index).Select(static one => (one.Set, one.Node))];
	}

	List<(FirstSets.First Set, List<Node> Members)>? Dispatchable(IReadOnlyList<Node> alternatives, int least = Grouped)
	{
		if (alternatives.Count < least)
			return null;

		var sets = new FirstSets.First[alternatives.Count];
		var cuts = new SortedSet<int>();

		for (var i = 0; i < alternatives.Count; i++)
		{
			// `Ends` is not a character and cannot be switched on; the rest of what makes a
			// first set unusable `Decidable` already refuses.
			if (Decidable(alternatives[i]) is not { Ends: false } set)
				return null;

			sets[i] = set;

			foreach (var range in set.Ranges)
			{
				cuts.Add(range.From);
				cuts.Add(range.To + 1);
			}
		}

		// Between two cuts every character begins the same alternatives, and a group is
		// those alternatives — one group per distinct set of them, its characters the
		// stretches that chose it, in the order the characters come.
		var groups = new List<(List<CharRange> Ranges, List<Node> Members)>();
		var index  = new Dictionary<string, int>(StringComparer.Ordinal);
		var named  = 0;
		var from   = -1;

		foreach (var cut in cuts)
		{
			if (from >= 0)
			{
				var key     = "";
				var members = new List<Node>();

				for (var i = 0; i < sets.Length; i++)
					if (Admits(sets[i], (char)from))
					{
						key += i.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",";
						members.Add(alternatives[i]);
					}

				if (key.Length > 0)
				{
					named += cut - from;

					if (named > Switched)
						return null;

					if (!index.TryGetValue(key, out var at))
					{
						index[key] = at = groups.Count;
						groups.Add(([], members));
					}

					groups[at].Ranges.Add(new CharRange((char)from, (char)(cut - 1)));
				}
			}

			from = cut;
		}

		if (groups.Count < least)
			return null;

		return groups.ConvertAll(static group => (FirstSets.First.Chars(group.Ranges), group.Members));

		static bool Admits(FirstSets.First set, char c)
		{
			foreach (var range in set.Ranges)
				if (range.From <= c && c <= range.To)
					return true;

			return false;
		}
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

		return first.Anything || first.Nothing || FirstSets.Nullable(alternative, _graph)
			? null
			: first;
	}

	/// <summary>A test over <c>c</c> for membership of a set of ranges.</summary>
	/// <remarks>
	/// Three shapes and the widest set has one too: comparisons while there are few enough
	/// to read, a table while the set stays inside ASCII, and a searched array of bounds for
	/// everything else — which is what a Unicode category is, and what used to be rendered
	/// as nothing at all.
	/// </remarks>
	string RangesTest(
		IReadOnlyList<CharRange> ranges, Func<IReadOnlyList<CharRange>, string?>? tabulate = null)
	{
		if (tabulate?.Invoke(ranges) is { } table)
			return TableTest(table);

		if (ranges.Count > Emitted)
			return $"{Search}({Wide(ranges)}, c)";

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
				var test = CSharpEmitter.Test(element, Tabulate);

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
			case Node.Choice(var alternatives) { Selection: null }:
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
