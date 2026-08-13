using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// Identity comparison for graph nodes. They are records, so their built-in equality is
/// by value — two structurally identical captures in one rule would collide as keys.
/// netstandard2.0 has no <c>ReferenceEqualityComparer</c> of its own.
/// </summary>
sealed class NodeIdentity : IEqualityComparer<Node>
{
	public static readonly NodeIdentity Instance = new();

	public bool Equals(Node? x, Node? y) => ReferenceEquals(x, y);

	public int GetHashCode(Node node) => RuntimeHelpers.GetHashCode(node);
}

/// <summary>
/// One capture of a rule body. <paramref name="Rule"/> is the rule whose value it takes,
/// or null when what it captures is text.
/// </summary>
public sealed record CaptureSlot(int Index, string Name, RuleSymbol? Rule);

/// <summary>
/// Where a rule body's captures are, numbered in the order the notation writes them.
/// </summary>
/// <remarks>
/// <para>
/// Source order is not a presentation choice. A backtracking machine has to forget what
/// an abandoned attempt captured, and numbering the slots in the order they can be
/// written makes "everything written since this point" a contiguous suffix — which is a
/// constant the generator can compute, rather than a journal the running parser has to
/// keep. <see cref="Before"/> is that point, for the two nodes that can be returned to.
/// </para>
/// <para>
/// Computed here rather than in the emitter because normalization needs the same
/// numbering to decide what a rule's result type is, and two walks that had to agree
/// would eventually stop agreeing.
/// </para>
/// </remarks>
public sealed class CaptureLayout
{
	readonly List<CaptureSlot>     _slots  = [];
	readonly Dictionary<Node, int> _slotOf = new(NodeIdentity.Instance);
	readonly Dictionary<Node, int> _before = new(NodeIdentity.Instance);
	readonly Dictionary<Node, int> _after  = new(NodeIdentity.Instance);

	CaptureLayout()
	{
	}

	/// <summary>Empty: a rule with no captures yields the text it matched.</summary>
	public static readonly CaptureLayout None = new();

	public IReadOnlyList<CaptureSlot> Slots => _slots;

	public bool IsEmpty => _slots.Count == 0;

	/// <summary>The slot a <see cref="Node.Capture"/> writes.</summary>
	public int SlotOf(Node capture) => _slotOf[capture];

	/// <summary>
	/// How many slots were already settled when this node began — so the slots from here
	/// on are exactly the ones an attempt at it could have written.
	/// </summary>
	public int Before(Node node) => _before.TryGetValue(node, out var count) ? count : 0;

	/// <summary>
	/// How many slots were settled once this node was done — so the slots from here on are
	/// the ones written after it, and nothing of its own is among them.
	/// </summary>
	public int After(Node node) => _after.TryGetValue(node, out var count) ? count : 0;

	/// <param name="buildsValue">Whether a rule has a value of its own rather than text.</param>
	public static CaptureLayout Of(Node body, Func<RuleSymbol, bool> buildsValue)
	{
		var layout = new CaptureLayout();

		layout.Walk(body, buildsValue);

		return layout;
	}

	void Walk(Node node, Func<RuleSymbol, bool> buildsValue)
	{
		switch (node)
		{
			case Node.Capture(var name, var captured):

				_slotOf[node] = _slots.Count;

				_slots.Add(new CaptureSlot(
					_slots.Count,
					name,
					captured is Node.Call(var called, _) && buildsValue(called) ? called : null));

				Walk(captured, buildsValue);
				break;

			// The two nodes an attempt can be abandoned at, and so the two that need to
			// know where their slots start.
			case Node.Choice(var alternatives):

				_before[node] = _slots.Count;

				foreach (var alternative in alternatives)
					Walk(alternative, buildsValue);

				break;

			case Node.Repeat(var repeated, _, _):

				_before[node] = _slots.Count;

				Walk(repeated, buildsValue);

				_after[node] = _slots.Count;
				break;

			case Node.Sequence(var nodes):

				foreach (var child in nodes)
					Walk(child, buildsValue);

				break;

			case Node.Construct(var built, _):

				Walk(built, buildsValue);
				break;

			// A lookahead consumes nothing and is compiled as a separate machine, with its
			// captures stripped: there is no slot of this rule's for it to write.
			default:
				break;
		}
	}
}

/// <summary>
/// One member of a rule's result: a capture name, and the slots that can fill it.
/// </summary>
/// <remarks>
/// More than one slot when the same name is captured in more than one alternative —
/// <c>(a: X | a: Y)</c> is one member filled from whichever alternative matched.
/// </remarks>
public sealed record ResultMember(
	string Name, RuleSymbol? Rule, bool IsOptional, IReadOnlyList<int> Slots);
