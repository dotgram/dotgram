using System;
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
/// or null when what it captures is text; <paramref name="IsSequence"/> says the capture
/// is under a repetition and so collects rather than holds.
/// </summary>
/// <param name="IsSequence">Whether the slot is written more than once, and so collects.</param>
/// <param name="InFold">
/// Why it collects, where it does: a slot under a fold's loop is written once per step
/// and read one step at a time, so what the author's <c>=&gt;</c> is handed is one of
/// what it collected — a value, not the sequence. A slot under an ordinary repetition
/// collects for the author too.
/// </param>
public sealed record CaptureSlot(
	int Index, string Name, RuleSymbol? Rule, bool IsSequence, bool InFold = false)
{
	/// <summary>What the author sees: whether the name holds a sequence in their C#.</summary>
	public bool Collects => IsSequence && !InFold;
}

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
	/// <summary>Every slot that collects rather than holds.</summary>
	public IEnumerable<CaptureSlot> Sequences
	{
		get { return _slots.Where(static slot => slot.IsSequence); }
	}

	/// <param name="fold">
	/// The loop a left-recursive rule was rewritten into, if it was. Everything captured
	/// under it collects — text as well as values — because a fold step's <c>=&gt;</c> is
	/// applied once per iteration and needs that iteration's captures, not the last
	/// one's.
	/// </param>
	public static CaptureLayout Of(Node body, Func<RuleSymbol, bool> buildsValue, Node? fold = null)
	{
		var layout = new CaptureLayout { _fold = fold };

		layout.Walk(body, buildsValue, repeated: false, inFold: false);

		return layout;
	}

	Node? _fold;

	/// <param name="repeated">
	/// Whether a repetition that may run more than once encloses this node. What is
	/// captured under one is written once per iteration, which is a sequence rather than
	/// a value — §7.3, and the reason a bound of one does not count.
	/// </param>
	void Walk(Node node, Func<RuleSymbol, bool> buildsValue, bool repeated, bool inFold)
	{
		switch (node)
		{
			case Node.Capture(var name, var captured):
			{
				var called = captured is Node.Call(var rule, _) && buildsValue(rule) ? rule : null;

				_slotOf[node] = _slots.Count;

				_slots.Add(new CaptureSlot(
					_slots.Count, name, called,
					IsSequence: inFold || (repeated && called is not null),
					InFold: inFold));

				Walk(captured, buildsValue, repeated, inFold);
				break;
			}

			// The two nodes an attempt can be abandoned at, and so the two that need to
			// know where their slots start.
			case Node.Choice(var alternatives):

				_before[node] = _slots.Count;

				foreach (var alternative in alternatives)
					Walk(alternative, buildsValue, repeated, inFold);

				break;

			case Node.Repeat(var body, _, var max):

				_before[node] = _slots.Count;

				Walk(body, buildsValue, repeated || max != 1, inFold || ReferenceEquals(node, _fold));

				_after[node] = _slots.Count;
				break;

			case Node.Sequence(var nodes):

				foreach (var child in nodes)
					Walk(child, buildsValue, repeated, inFold);

				break;

			case Node.Atomic(var body):

				Walk(body, buildsValue, repeated, inFold);
				break;

			case Node.Marked(var marked, _):

				Walk(marked, buildsValue, repeated, inFold);
				break;

			// A guard is not a slot, but it needs a place in the numbering all the same:
			// what it may look at is what was captured before it, and "before" is this.
			case Node.Guard:

				_before[node] = _slots.Count;
				break;

			// An alternative that builds needs its own range: what its `=>` may name is
			// what that alternative captured, not what a sibling did.
			case Node.Construct(var built, _):

				_before[node] = _slots.Count;

				Walk(built, buildsValue, repeated, inFold);

				_after[node] = _slots.Count;
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
	string Name, RuleSymbol? Rule, bool IsSequence, bool IsOptional, IReadOnlyList<int> Slots);

/// <summary>
/// What a repetition marked <c>recover</c> was told (§8.2): where to resume after a
/// broken element, and how to turn it into one of the sequence.
/// </summary>
public sealed record Recovery(Node Sync, string? Factory)
{
	/// <summary>
	/// The names §8.2 supplies to a failure factory, in the order it takes them.
	/// </summary>
	/// <remarks>
	/// Supplied rather than captured: a broken element captured nothing, so everything the
	/// factory can be told about it is known here and nowhere else.
	/// </remarks>
	public static IReadOnlyList<string> Supplied => SuppliedNames.All;

	/// <summary>
	/// Which of <see cref="Supplied"/> this factory asked for, in that order.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Read out of the C# rather than declared, because §8.2 matches by name and §8.2 also
	/// says counting lines is done only when a name that needs it was asked for. A
	/// parameter nobody named would be a scan of the input per rejected element, paid for
	/// nothing.
	/// </para>
	/// <para>
	/// The search is a whole-word one over the text, so it over-approximates: <c>line</c>
	/// inside a string literal counts as having been asked for. That direction is the safe
	/// one — a name that was written is always found, and a name that was not costs an
	/// unused parameter. Reading it exactly would mean lexing C# here, which is the host's
	/// job and not this half's.
	/// </para>
	/// </remarks>
	public IReadOnlyList<string> Asks
	{
		get
		{
			if (Factory is not { } factory)
				return [];

			var asked = new List<string>();

			foreach (var name in Supplied)
				if (Mentions(factory, name))
					asked.Add(name);

			return asked;
		}
	}

	static bool Mentions(string csharp, string name)
	{
		for (var at = csharp.IndexOf(name, StringComparison.Ordinal); at >= 0;
			 at = csharp.IndexOf(name, at + 1, StringComparison.Ordinal))
		{
			if ((at == 0 || !IsPartOfAName(csharp[at - 1])) &&
				(at + name.Length == csharp.Length || !IsPartOfAName(csharp[at + name.Length])))
			{
				return true;
			}
		}

		return false;
	}

	static bool IsPartOfAName(char c) => char.IsLetterOrDigit(c) || c is '_' or '@';
}

/// <summary>
/// What a left-recursive rule became: the loop of its tails, and which capture of each
/// tail is the value built so far (§4.3).
/// </summary>
/// <remarks>
/// The loop is an ordinary repetition and compiles as one. It is named here for the two
/// things that must know it is not the author's: a capture under it is written once per
/// fold step and consumed there, so it is a value and not a sequence; and the
/// alternatives under it take the accumulator as their first argument.
/// </remarks>
public sealed record Fold(Node Loop, IReadOnlyDictionary<Node, string> Accumulators)
{
	/// <summary>
	/// The alternatives a rule offers — the bases and the fold steps once it has been
	/// rewritten, and the plain ones when it has not.
	/// </summary>
	public static IReadOnlyList<Node> Of(Node body, Fold? fold)
	{
		if (fold is null)
			return body is Node.Choice(var alternatives) ? alternatives : [body];

		var offered = new List<Node>();

		// The rewrite made the body a sequence of the bases and the loop over the tails.
		foreach (var part in ((Node.Sequence)body).Nodes)
			offered.AddRange(Of(part is Node.Repeat(var repeated, _, _) ? repeated : part, null));

		return offered;
	}
}
