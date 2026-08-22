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
/// Two answers today: whether the node is silent (§ <c>Machine.Silent</c>) and, where it
/// is a repetition, whether it is possessive (§ <c>Machine.Possessive</c>). Both are
/// following-dependent, which is why they could not live in <c>ExecutionPlan</c>; both are
/// booleans, which is why two nodes reached under different followings but the same two
/// answers can share one region instead of two.
/// </remarks>
public readonly record struct DecisionClass(bool Silent, bool Possessive);

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
/// A region for every node a publication can reach, under every following it can reach it
/// with.
/// </summary>
/// <remarks>
/// <para>
/// Walked from <see cref="RecognitionGraph.Publications"/> outward through
/// <see cref="Node.Call"/>, rather than over every rule the way <see cref="ExecutionPlan"/>
/// and <c>Machine</c>'s own compilation are. <see cref="ExecutionPlan"/>'s remarks record
/// why that walk failed there: the decision it was tried for does not depend on the
/// caller, so leaving an unpublished rule out of it just left that rule uncompiled. A
/// region is the caller's context by definition, so this is the walk that gets to start
/// from the publications instead.
/// </para>
/// <para>
/// <paramref name="classify"/> is asked, not answered here: turning a node and a following
/// into a <see cref="DecisionClass"/> means calling <c>Machine.Silent</c> and
/// <c>Machine.Possessive</c>, which read <c>Machine</c>'s own tables and cannot be
/// duplicated here without the two ever being allowed to disagree — the mistake
/// <c>FollowSets.Precedes</c>'s remarks already warn about. What belongs here instead is
/// the threading itself: a sequence followed by the rest of the sequence, a repetition's
/// body followed by itself or by what follows the repetition, a call crossed into rather
/// than answered for — the same walk <see cref="FollowSets.Of"/> makes, kept in step with
/// it for the same reason.
/// </para>
/// <para>
/// Two different followings that classify a node the same way merge into one region by
/// simply landing on the same <see cref="Region"/> twice; no dictionary keyed on
/// <see cref="FirstSets.First"/> decides this; <see cref="FirstSets.First"/> holds a
/// <c>List</c> of ranges and is not safe to hash or compare that way. What keeps the walk
/// itself from running forever is the same guard <c>Machine.Deterministic(RuleSymbol, …)</c>
/// and <see cref="FirstSets.Of(Node, RecognitionGraph)"/>'s own <c>Call</c> case already
/// use: a rule already on the way down is not entered again, so a cycle is cut where it
/// closes rather than remembered forever.
/// </para>
/// </remarks>
public static class Regions
{
	/// <summary>Every region a publication can reach, classified by <paramref name="classify"/>.</summary>
	public static IReadOnlyCollection<Region> Of(
		RecognitionGraph graph, Func<Node, FirstSets.First, DecisionClass> classify)
	{
		if (graph is null)
			throw new ArgumentNullException(nameof(graph));

		if (classify is null)
			throw new ArgumentNullException(nameof(classify));

		var regions = new HashSet<Region>(RegionIdentity.Instance);

		foreach (var publication in graph.Publications)
			WalkRule(
				publication.Rule,
				publication.Kind == PublishKind.Parse ? FirstSets.First.End : FirstSets.First.All,
				[]);

		return regions;

		void WalkRule(RuleSymbol rule, FirstSets.First following, HashSet<RuleSymbol> path)
		{
			if (!path.Add(rule))
				return;

			if (graph.Bodies.TryGetValue(rule, out var body))
				WalkNode(body, following, path);

			path.Remove(rule);
		}

		void WalkNode(Node node, FirstSets.First following, HashSet<RuleSymbol> path)
		{
			regions.Add(new Region(node, classify(node, following)));

			switch (node)
			{
				case Node.Sequence(var parts):
				{
					var after = following;

					for (var i = parts.Count - 1; i >= 0; i--)
					{
						WalkNode(parts[i], after, path);

						after = FollowSets.Precedes(parts[i], after, graph);
					}

					return;
				}

				case Node.Choice(var alternatives):
					foreach (var alternative in alternatives)
						WalkNode(alternative, following, path);

					return;

				// A turn is followed by another turn, or by whatever the repetition is.
				case Node.Repeat(var body, _, _):
					WalkNode(body, FirstSets.Of(body, graph).Or(following), path);

					return;

				case Node.Capture(_, var captured): WalkNode(captured, following, path); return;
				case Node.Construct(var built, _):  WalkNode(built,    following, path); return;
				case Node.Atomic(var kept):         WalkNode(kept,     following, path); return;

				// What is inside is read and given back, so what follows it is read again by
				// whatever comes next — which this cannot see from here.
				case Node.Lookahead(_, var operand):
					WalkNode(operand, FirstSets.First.All, path);

					return;

				case Node.Call(var called, var arguments):
				{
					foreach (var argument in arguments)
						WalkNode(argument, following, path);

					WalkRule(called, following, path);

					return;
				}
			}
		}
	}
}
