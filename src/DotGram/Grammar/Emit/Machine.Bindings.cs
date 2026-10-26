using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// Where a rule keeps what its bindings recorded (<c>$q: X</c>), and the comparison a
/// reference (<c>$q</c>) makes against it.
/// </summary>
/// <remarks>
/// <para>
/// A binding is kept the way a text capture is: the position its body began at and the one
/// it ended at, in a slot of the rule's (<see cref="CaptureLayout.SlotOf(TextBinding)"/>).
/// The slot is numbered after every capture of the rule, so a grammar with no binding numbers
/// its captures as it always did, and no member is ever made of it.
/// </para>
/// <para>
/// The comparison is one method the machine writes once, beside everything else it writes,
/// and only where a reference is read: a grammar with no reference carries none of it.
/// </para>
/// </remarks>
sealed partial class Machine
{
	readonly Dictionary<RuleSymbol, CaptureLayout> _bindingLayouts = [];

	/// <summary>The layout a rule's bindings are numbered in, which numbers its captures the same way the rest of the machine does.</summary>
	CaptureLayout BindingLayout(RuleSymbol rule)
	{
		if (!_bindingLayouts.TryGetValue(rule, out var layout))
		{
			layout = CaptureLayout.Of(
				_graph.Bodies[rule], other => _graph.Results[other].Count > 0 || _graph.Types.ContainsKey(other),
				lifted: _graph.LiftedBody);

			_bindingLayouts[rule] = layout;
		}

		return layout;
	}

	/// <summary>The rule-local slot a binding of <paramref name="rule"/> is kept in.</summary>
	/// <remarks>
	/// Every reference reads a binding of its own rule, which lowering made sure of: one that
	/// names a binding the rule does not hold is a defect upstream, and said so here.
	/// </remarks>
	int BindingSlot(RuleSymbol rule, TextBinding binding)
	{
		var slot = BindingLayout(rule).SlotOf(binding);

		if (slot < 0)
			throw new InvalidOperationException(
				$"'{rule.Name}' reads {binding}, which it does not bind.");

		return slot;
	}

	/// <summary>Whether a rule-local slot is a binding's, which no member holds.</summary>
	bool IsBindingSlot(RuleSymbol rule, int slot)
	{
		return BindingLayout(rule).IsBinding(slot);
	}

	/// <summary>The bindings a node reads by itself: a reference its own, a count the lengths it is bounded by.</summary>
	IEnumerable<TextBinding> BindingsRead(Node node)
	{
		switch (node)
		{
			case Node.Same(var binding, _):
				yield return binding;
				break;

			case Node.Counted(_, var min, var max):
				if (min is CountBound.Length(var least, _))
					yield return least;

				if (max is CountBound.Length(var most, _))
					yield return most;

				break;
		}
	}

	bool _sameTextWritten;

	/// <summary>
	/// The method that says whether the text at a position is the text a binding recorded,
	/// written the first time it is asked for.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Positions only: the recorded text is read where it stands in the input, so a reference
	/// allocates nothing. A binding that recorded nothing matches anywhere and reads nothing,
	/// and one longer than what is left of the input does not match.
	/// </para>
	/// <para>
	/// Over a buffer the comparison is made in its window. Room at the reference is asked for
	/// first, the one call that may read on and move what the window holds; the binding stands
	/// before the reference and the window has not let it go (<see cref="BindingLiveAtTurn"/>,
	/// <see cref="CanReleaseBuffered"/>), so both spans taken after it are held, and neither
	/// outlives the call.
	/// </para>
	/// </remarks>
	internal string SameText()
	{
		var name = Tag.Named("Same");

		if (_sameTextWritten)
			return name;

		_sameTextWritten = true;

		var helper = new Writer(0);

		helper.Line("/// <summary>");
		helper.Line("/// Whether the text at <paramref name=\"at\"/> is the text from <paramref name=\"from\"/> to");
		helper.Line("/// <paramref name=\"to\"/>, item by item and ordinally: what a back-reference matches.");
		helper.Line("/// </summary>");

		if (BufferedInput)
		{
			using (helper.Block($"static bool {name}({InputType} text, int at, int from, int to)"))
			{
				helper.Line("var length = to - from;");
				helper.Line();
				helper.Line("// Room at the reference first: the one call that may read on. The binding before it");
				helper.Line("// is held then too, since nothing lets the window go past a binding still to be read.");
				helper.Line("return text.Ensure(at, length) &&");
				helper.Line("\tglobal::System.MemoryExtensions.SequenceEqual(text.Slice(at, length), text.Slice(from, length));");
			}
		}
		else
		{
			using (helper.Block($"static bool {name}(global::System.ReadOnlySpan<char> text, int at, int from, int to)"))
			{
				helper.Line("var length = to - from;");
				helper.Line();
				helper.Line("return (uint)(at + length) <= (uint)text.Length &&");
				helper.Line("\tglobal::System.MemoryExtensions.SequenceEqual(text.Slice(at, length), text.Slice(from, length));");
			}
		}

		_extra.Add(helper.ToString());

		return name;
	}
}
