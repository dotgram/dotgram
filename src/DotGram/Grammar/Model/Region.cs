using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

namespace DotGram.Grammar.Model;

/// <summary>
/// The caller-dependent decisions a node's compiled shape rests on, collapsed to what
/// actually distinguishes one compilation from another.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="Silent"/> and <see cref="Possessive"/> answer § <c>Machine.Silent</c> and
/// § <c>Machine.Possessive</c> — what storage a node needs. <see cref="Deterministic"/>
/// answers § <c>Machine.Deterministic</c> — whether the node has one way to match, which is
/// both a question in its own right and what <see cref="Committed"/> is threaded with.
/// </para>
/// <para>
/// <see cref="Committed"/> is the fourth need docs/next.md asks for: whether, by the time
/// recognition reaches this node, every choice on the way here has already closed — so a
/// construction here could run now rather than waiting for the parse to be accepted. It is
/// not following-dependent the way the others are; it is dependent on everything between
/// the nearest publication or commit point and here, which is why it is threaded downward
/// through the walk rather than asked of a node on its own.
/// </para>
/// </remarks>
public readonly record struct DecisionClass(bool Silent, bool Possessive, bool Deterministic, bool Committed);

/// <summary>
/// A node, and the bucket of caller context its compiled shape was decided from.
/// </summary>
public sealed record Region(Node Node, DecisionClass Class);

/// <summary>Holds a <see cref="Region"/> apart by which node it names, not what it looks like.</summary>
/// <remarks>
/// The same reason <see cref="NodeIdentity"/> exists: <see cref="Node"/> compares by value,
/// so two structurally equal nodes reached under equal classes would collapse into one
/// region if this compared <see cref="Region.Node"/> the way <c>record</c> equality does.
/// </remarks>
sealed class RegionIdentity : IEqualityComparer<Region>
{
	public static readonly RegionIdentity Instance = new();

	public bool Equals(Region? x, Region? y) =>
		x is not null && y is not null && ReferenceEquals(x.Node, y.Node) && x.Class == y.Class;

	public int GetHashCode(Region region) =>
		RuntimeHelpers.GetHashCode(region.Node) * 397 ^ region.Class.GetHashCode();
}

/// <summary>
/// A region for every node a publication can reach, under every following and commitment
/// it can reach it with.
/// </summary>
/// <remarks>
/// <para>
/// Walked from <see cref="RecognitionGraph.Publications"/> outward through
/// <see cref="Node.Call"/>, rather than over every rule the way <see cref="ExecutionPlan"/>
/// and <c>Machine</c>'s own compilation are. <see cref="ExecutionPlan"/>'s remarks record
/// why that walk failed there: the decisions it was tried for do not depend on the caller,
/// so leaving an unpublished rule out of it just left that rule uncompiled. A region is the
/// caller's context by definition, so this is the walk that gets to start from the
/// publications instead.
/// </para>
/// <para>
/// <paramref name="classify"/> and <paramref name="predictive"/> are asked, not answered
/// here: turning a node, a following and a commitment into a <see cref="DecisionClass"/>
/// means calling <c>Machine.Silent</c>, <c>Machine.Possessive</c> and
/// <c>Machine.Deterministic</c>, which read <c>Machine</c>'s own tables and cannot be
/// duplicated here without the two ever being allowed to disagree — the mistake
/// <c>FollowSets.Precedes</c>'s remarks already warn about. What belongs here instead is
/// the threading itself.
/// </para>
/// <para>
/// <c>following</c> threads the way <see cref="FollowSets.Of"/> and <c>Machine.Compile</c>
/// already do: backward over a sequence, unchanged over a choice's alternatives, widened by
/// a repetition's own first set going into its body, reset to <c>All</c> going into a
/// lookahead.
/// </para>
/// <para>
/// <c>committed</c> threads the other way — forward, in the order recognition actually
/// runs — and is reset rather than merely combined at exactly one place: an
/// <see cref="Node.Atomic"/> group commits everything before it once its body succeeds, so
/// whatever came in stops mattering and what follows starts from <c>true</c>. Everywhere
/// else, what a node hands to whatever comes after it is what came in, narrowed by whether
/// the node itself had only one way to go — its own <see cref="DecisionClass.Deterministic"/>.
/// A sequence's parts see that narrowing accumulate left to right, which is the one place
/// this walk runs a node's operands in the opposite order from the following it threads
/// them with. A choice hands its incoming commitment to an alternative unchanged only where
/// <paramref name="predictive"/> says the alternatives cannot be confused for one another;
/// otherwise every alternative starts uncommitted, because a later failure could still send
/// recognition back to try a different one.
/// </para>
/// <para>
/// Two different followings, or two different commitments, that classify a node the same
/// way merge into one region by simply landing on the same <see cref="Region"/> twice; no
/// dictionary keyed on <see cref="FirstSets.First"/> decides this — <see cref="FirstSets.First"/>
/// holds a <c>List</c> of ranges and is not safe to hash or compare that way. What keeps the
/// walk itself from running forever is the same guard <c>Machine.Deterministic(RuleSymbol,
/// …)</c> and <see cref="FirstSets.Of(Node, RecognitionGraph)"/>'s own <c>Call</c> case
/// already use: a rule already on the way down is not entered again, so a cycle is cut
/// where it closes rather than remembered forever.
/// </para>
/// </remarks>
public static class Regions
{
	/// <summary>Every region a publication can reach, classified by <paramref name="classify"/>.</summary>
	public static IReadOnlyCollection<Region> Of(
		RecognitionGraph graph,
		Func<Node, FirstSets.First, bool, DecisionClass> classify,
		Func<IReadOnlyList<Node>, bool> predictive)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		if (classify is null)
			throw new ArgumentNullException(nameof(classify));

		if (predictive is null)
			throw new ArgumentNullException(nameof(predictive));

		var regions = new HashSet<Region>(RegionIdentity.Instance);

		foreach (var publication in graph.Publications)
			WalkRule(
				publication.Rule,
				publication.Kind == PublishKind.Parse ? FirstSets.First.End : FirstSets.First.All,
				committed: true,
				[]);

		return regions;

		void WalkRule(RuleSymbol rule, FirstSets.First following, bool committed, HashSet<RuleSymbol> path)
		{
			if (!path.Add(rule))
				return;

			if (graph.Bodies.TryGetValue(rule, out var body))
				WalkNode(body, following, committed, path);

			path.Remove(rule);
		}

		// Returns what a sibling that follows this node may take as its own incoming
		// commitment — always true just past an atomic group's close, and otherwise what
		// came in, narrowed by whether this node itself had only one way to go.
		bool WalkNode(Node node, FirstSets.First following, bool committed, HashSet<RuleSymbol> path)
		{
			var @class = classify(node, following, committed);

			regions.Add(new Region(node, @class));

			switch (node)
			{
				case Node.Sequence(var parts):
				{
					var followingAt = new FirstSets.First[parts.Count];
					var after       = following;

					for (var i = parts.Count - 1; i >= 0; i--)
					{
						followingAt[i] = after;
						after          = FollowSets.Precedes(parts[i], after, graph);
					}

					// Left to right, and it has to be: whether the third part starts
					// committed depends on whether the first two ever offered a way back,
					// which is not known until they have been asked.
					var committedNow = committed;

					for (var i = 0; i < parts.Count; i++)
						committedNow = WalkNode(parts[i], followingAt[i], committedNow, path);

					break;
				}

				case Node.Choice(var alternatives):
				{
					var committedIntoEach = committed && predictive(alternatives);

					foreach (var alternative in alternatives)
						WalkNode(alternative, following, committedIntoEach, path);

					break;
				}

				// A turn is followed by another turn, or by whatever the repetition is. A
				// turn starts committed only where the repetition is possessive — the same
				// fact §Machine.Possessive already proves, read back off this node's own
				// class rather than asked for twice.
				case Node.Repeat(var body, _, _):
					WalkNode(body, FirstSets.Of(body, graph).Or(following), committed && @class.Possessive, path);

					break;

				case Node.Capture(_, var captured): WalkNode(captured, following, committed, path); break;
				case Node.Construct(var built, _):  WalkNode(built,    following, committed, path); break;

				// The body sees what came in unchanged — a choice inside `{ … }` is exactly
				// as reversible as it would be outside it, right up until the group as a
				// whole succeeds.
				case Node.Atomic(var kept):
					WalkNode(kept, following, committed, path);

					break;

				// What is inside is read and given back, so what follows it is read again by
				// whatever comes next — which this cannot see from here.
				case Node.Lookahead(_, var operand):
					WalkNode(operand, FirstSets.First.All, committed, path);

					break;

				case Node.Call(var called, var arguments):
				{
					foreach (var argument in arguments)
						WalkNode(argument, following, committed, path);

					WalkRule(called, following, committed, path);

					break;
				}
			}

			// An atomic group is the one place this is not "what came in, narrowed": its
			// own close is what makes everything before it irreversible, whatever that was.
			return node is Node.Atomic ? true : committed && @class.Deterministic;
		}
	}
}
