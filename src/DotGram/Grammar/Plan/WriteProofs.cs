using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Plan;

/// <summary>
/// The proofs that a choice or a repetition a reader reads writes no way on the tape: functions of
/// the graph, the node and what follows it, and of nothing a printer holds.
/// </summary>
/// <remarks>
/// <para>
/// A way is what a reading comes back to for another answer. A choice none of whose other answers
/// could hold where one did, and a repetition that is never asked for a shorter reading, need none,
/// and writing one costs every parse that passes. These are the questions that say so; the planning
/// stage asks them once for every place a reader stands (<see cref="WriteDecisions"/>), and whoever
/// prints the reader reads the answer.
/// </para>
/// <para>
/// One per machine, because what an alternative wants of the input is remembered rule by rule
/// (<see cref="WantsToken"/>).
/// </para>
/// </remarks>
sealed class WriteProofs(RecognitionGraph graph)
{
	readonly RecognitionGraph _graph = graph;

	/// <summary>
	/// Whether a choice of <paramref name="alternatives"/>, followed by what begins with
	/// <paramref name="following"/>, writes a way — and where it does not, the proof that settled it:
	/// one run of text (<see cref="LiteralRun"/>), alternatives no two of which can hold at one place
	/// (<see cref="Exclusive"/>), or a run of text the engine's run does not take (<see cref="SettledText"/>).
	/// </summary>
	/// <remarks>Asked in that order, and the first that holds is the answer.</remarks>
	public WriteDecision Choice(IReadOnlyList<Node> alternatives, FirstSets.First following)
	{
		if (LiteralRun(alternatives, alternatives.Count - 1, following) == alternatives.Count)
			return WriteDecision.By(WriteProof.LiteralRun);

		if (Exclusive(alternatives))
			return WriteDecision.By(WriteProof.Exclusive);

		if (SettledText(alternatives, following))
			return WriteDecision.By(WriteProof.SettledText);

		return WriteDecision.Writes;
	}

	/// <summary>
	/// Whether a repetition writes a way — and where it does not, the proof that settled it: as many
	/// turns as it may take are the turns it must (<see cref="WriteProof.Fixed"/>), what follows can
	/// never fail and want a turn back (<see cref="Determinism.NeverGivesBack"/>), or, where the turn
	/// after a one-character stop is the one in question, what follows the stop cannot
	/// (<see cref="Determinism.NeverGivesBackPast"/>).
	/// </summary>
	/// <param name="taught">What follows the repetition, as the proofs that ask whether it can fail read it.</param>
	/// <param name="seam">The trivia of the rule it is read in, where it has some.</param>
	/// <param name="stopped">The stop the repetition is read past, and what follows the stop; null where it is not.</param>
	public WriteDecision Repetition(
		Node.Repeat repeat, FollowSets.Continuation taught, RuleSymbol? seam, (char Stop, FollowSets.Continuation Beyond)? stopped)
	{
		if (repeat.Max == repeat.Min)
			return WriteDecision.By(WriteProof.Fixed);

		if (Determinism.NeverGivesBack(repeat, taught, _graph, seam))
			return WriteDecision.By(WriteProof.NeverGivesBack);

		if (stopped is { } past && Determinism.NeverGivesBackPast(repeat, past.Stop, past.Beyond, _graph))
			return WriteDecision.By(WriteProof.NeverGivesBackPast);

		return WriteDecision.Writes;
	}

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
	/// Wider than <c>Machine.Chainable</c> by the lookaheads, which read nothing and so have no
	/// first set a switch or a chain could test before trying them; tried in order they need
	/// none, since the order no longer matters to what holds.
	/// </para>
	/// </remarks>
	public bool Exclusive(IReadOnlyList<Node> alternatives)
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
		(FirstSets.First? Set, bool End)? Begins(Node node)
		{
			return node switch
			{
				Node.Lookahead(false, var body) when Everything(body) => (null, true),
				Node.Lookahead(true, Node.Lookahead(false, var body)) when Everything(body) => (null, true),
				// `eof` called as a rule rather than written in place: FIX's `(Separator | eof)`.
				Node.Call(var called, { Count: 0 }) when _graph.Bodies.TryGetValue(called, out var body) &&
					body is Node.Lookahead(false, var any) && Everything(any) => (null, true),
				Node.Lookahead(true, var body) => Consumes(body) is { } set ? (set, false) : null,
				_ => Decidable(node) is { Ends: false } set ? (set, false) : null,
			};
		}

		FirstSets.First? Consumes(Node body)
		{
			return !FirstSets.Nullable(body, _graph) && !FirstSets.LedByRecovery(body, _graph) &&
				FirstSets.Of(body, _graph) is { Nothing: false, Anything: false } set ? set : null;
		}

		// One character of anything, which only the end of the input refuses: `eof` is `?!any`.
		bool Everything(Node body)
		{
			return !FirstSets.Nullable(body, _graph) &&
			FirstSets.Of(body, _graph) is var set &&
			(set.Anything || set.Ranges.Count == 1 && set.Ranges[0].From == char.MinValue && set.Ranges[0].To == char.MaxValue) &&
			body is Node.Element;
		}
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
	public static bool SettledText(IReadOnlyList<Node> alternatives, FirstSets.First following)
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

			// Texts that some character tells apart cannot both match: no character is read
			// by both at that place.
			var shorter = Math.Min(first.Text.Length, second.Text.Length);

			for (var i = 0; i < shorter; i++)
				if (!Accepted(first, i).Intersect(Accepted(second, i)).Any())
					return true;

			if (first.Text.Length <= second.Text.Length)
				return false;

			return following.IsKnown &&
				!following.Overlaps(FirstSets.First.Chars(
					Accepted(first, second.Text.Length).Select(static one => new CharRange(one, one))));
		}

		// What a text reads at one place: the character, or every case of it (CaseFold).
		static IReadOnlyList<char> Accepted(Node.Literal literal, int at)
		{
			return literal.IgnoreCase ? CaseFold.Of(literal.Text[at]) : [literal.Text[at]];
		}
	}

	public static int LiteralRun(IReadOnlyList<Node> alternatives, int at, FirstSets.First following)
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
	public static bool PrefixSettled(string first, string second, FirstSets.First following)
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
	public FirstSets.First? Decidable(Node alternative)
	{
		var first = FirstSets.Of(alternative, _graph);

		// One that may stand still through a recognizer of the host may match at the end of the
		// input, where no token stands (FirstSets.StandsStill) — unless it looks for one first.
		// Nor one that may begin with a repetition that recovers, which begins with anything.
		return first.Anything || first.Nothing || FirstSets.Nullable(alternative, _graph) ||
			FirstSets.LedByRecovery(alternative, _graph) ||
			FirstSets.StandsStill(alternative, _graph) && !WantsToken(alternative)
			? null
			: first;
	}

	/// <summary>
	/// Whether <paramref name="node"/> cannot match at the end of the input however its recognizers
	/// answer: it begins by reading a character, or by looking for something that does.
	/// </summary>
	/// <remarks>
	/// What keeps the door of <c>CsExpr = ?="@(" &amp; @CSharp</c>: the look is the recognizer's
	/// first set said in the grammar, and a door that did not take it at its word would call the
	/// host at every place an expression may stand.
	/// </remarks>
	readonly Dictionary<RuleSymbol, bool> _wantsToken = [];

	bool WantsToken(Node node)
	{
		// Follows calls, so a chain of them is as deep as the grammar makes it (DeepStack).
		if (DeepStack.Low)
			return DeepStack.Run(() => WantsToken(node));

		// Whatever cannot match nothing, recognizers answering nothing or not, reads a character.
		if (!FirstSets.Nullable(node, _graph) && !FirstSets.StandsStill(node, _graph))
			return true;

		switch (node)
		{
			case Node.Sequence(var parts):
				foreach (var part in parts)
				{
					if (WantsToken(part))
						return true;
				}

				return false;

			case Node.Choice { Selection: null } choice:
				return choice.Nodes.Count > 0 && choice.Nodes.All(one => WantsToken(one));

			case Node.Repeat   (var body, var min, _):  return min > 0 && WantsToken(body);
			case Node.Capture  (_,        var body):    return WantsToken(body);
			case Node.Construct(var body, _):           return WantsToken(body);
			case Node.Atomic   (var body):              return WantsToken(body);
			case Node.Marked   (var body, _):           return WantsToken(body);
			case Node.Bind     (_,        var body, _): return WantsToken(body);
			case Node.Lookahead(true,     var body):    return WantsToken(body);

			// Once a rule: a rule met again on the way down is taken not to want one, which is never
			// wrong — the door is only left off.
			case Node.Call(var rule, _):
				if (_wantsToken.TryGetValue(rule, out var known))
					return known;

				_wantsToken[rule] = false;

				return _wantsToken[rule] = _graph.Bodies.TryGetValue(rule, out var called) && WantsToken(called);

			default:
				return false;
		}
	}

	/// <summary>A node without the captures and constructions around it.</summary>
	internal static Node Bare(Node node)
	{
		return node switch
		{
			Node.Capture(_, var body) => Bare(body),
			Node.Construct(var body, _) => Bare(body),
			_ => node,
		};
	}
}
