using System;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Emit;

/// <summary>
/// What a rule lifted out of another one's element changes for the engine: where what it captures
/// is kept (docs/syntax.md §8.2, <see cref="RecognitionGraph.Lifted"/>).
/// </summary>
/// <remarks>
/// The rule is a call like any other, with a frame of its own: a recovery in it runs in it, and a
/// guard in it reads from where it was called. What it captures is the enclosing rule's value, in
/// that rule's slots as the layout numbers them through the call, so its entries name the frame of
/// the call whose value they are part of, and the materializer finds them where it would have found
/// them had nothing been lifted.
/// </remarks>
sealed partial class Machine
{
	/// <summary>The frame the rule being compiled writes its captures under: its own call, or the one it lends to.</summary>
	string _frame = "call";

	/// <summary>
	/// The call a rule's captures are written under, as an expression over <c>call</c>: one step out
	/// for every lift out of an element between it and the rule whose value they are part of.
	/// </summary>
	string FrameOf(RuleSymbol rule)
	{
		var frame = "call";

		while (_graph.Lifted.TryGetValue(rule, out var lift) && lift.Transparent)
		{
			frame = $"entries[{frame}].CallIndex";
			rule  = lift.Parent;
		}

		return frame;
	}

	/// <summary>
	/// Where a rule lifted out of an element keeps its captures: the slot of the rule whose value
	/// they are part of at which its call stands; null for every other rule.
	/// </summary>
	int? SlotsLent(RuleSymbol rule)
	{
		if (!_graph.Lifted.TryGetValue(rule, out var lift) || !lift.Transparent)
			return null;

		var owner = _graph.ValueOwner(rule);

		if (!_captureOffsets.TryGetValue(owner, out var offset))
			throw new InvalidOperationException(
				$"A rule lifted out of '{owner.Name}' is numbered before it is.");

		var call = NodeWalk.Descendants(_graph.Bodies[lift.Parent])
			.First(node => node is Node.Call(var called, _) && called == rule);
		var layout = CaptureLayout.Of(
			_graph.Bodies[owner],
			other => _graph.Results[other].Count > 0 || _graph.Types.ContainsKey(other),
			_graph.Folds.TryGetValue(owner, out var fold) ? fold.Loop : null,
			_graph.LiftedBody);

		return offset + layout.Before(call);
	}
}
