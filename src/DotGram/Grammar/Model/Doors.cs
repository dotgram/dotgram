using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Model;

/// <summary>
/// Whether matching something can leave a way back into the middle of it.
/// </summary>
/// <remarks>
/// <para>
/// A repetition leaves the door its runs and turns are given back through, a choice leaves
/// the one an alternative is resumed by, and a negative lookahead leaves the entry its own
/// failure is read off. A literal and an element match where they stand or fail there, and
/// a failure that reaches past them has nothing to come back to. A call leaves whatever
/// its callee leaves and nothing of its own: the arena's <c>Call</c> entry is a frame to
/// restore while unwinding, never a state to resume at.
/// </para>
/// <para>
/// Two very different questions turn on the answer, which is why it is here rather than in
/// either of the places that ask it. Whether a capture may keep its start in a variable —
/// where nothing can come back into the close, nothing can have overwritten it. And whether
/// two alternatives sharing a leading operand mean the same as one alternative with an
/// optional tail — where the operand cannot give back it has one reading, so the two orders
/// contain the same one thing and factoring is invisible.
/// </para>
/// </remarks>
public static class Doors
{
	/// <summary>The same question of every rule, settled rather than walked.</summary>
	/// <remarks>
	/// A rule may reach itself, so the answer for one is the answer for the others and a
	/// walk of a call would go round. Settling from no rule leaving a door is what makes
	/// that terminate, and it is also right: a cycle has to pass a repetition or a choice
	/// to come back round, and either of those answers yes on its own.
	/// </remarks>
	public static Dictionary<RuleSymbol, bool> ByRule(
		IReadOnlyList<RuleSymbol> rules, IReadOnlyDictionary<RuleSymbol, Node> bodies)
	{
		if (rules is null)
			throw new ArgumentNullException(nameof(rules));

		if (bodies is null)
			throw new ArgumentNullException(nameof(bodies));

		var doors = rules.ToDictionary(static rule => rule, static _ => false);

		for (var settling = true; settling;)
		{
			settling = false;

			foreach (var rule in rules)
			{
				if (doors[rule] || !bodies.TryGetValue(rule, out var body) || !LeavesOne(body, doors))
					continue;

				doors[rule] = true;
				settling    = true;
			}
		}

		return doors;
	}

	/// <summary>Whether this node leaves one, given the answer for every rule.</summary>
	/// <remarks>A callee nothing knows about is assumed to leave one.</remarks>
	public static bool LeavesOne(Node node, IReadOnlyDictionary<RuleSymbol, bool> doors) =>
		node switch
		{
			Node.Repeat                     => true,
			Node.Lookahead(var positive, _) => !positive,
			Node.Call(var called, _)        => !doors.TryGetValue(called, out var door) || door,
			Node.Choice(var alternatives)   => alternatives.Count > 1 ||
			                                   alternatives.Any(one => LeavesOne(one, doors)),
			Node.Sequence(var parts)        => parts.Any(part => LeavesOne(part, doors)),
			// Nothing comes back into the middle of one: an atomic group commits its
			// first reading, so a failure that reaches past it has the group to give
			// back and nowhere inside it to resume. Walking in asks what the braces
			// have already answered — the same question `Determinism` used to ask of
			// them and stopped.
			Node.Atomic                     => false,
			Node.Marked(var body, _)        => LeavesOne(body, doors),
			Node.Capture(_, var captured)   => LeavesOne(captured, doors),
			Node.Construct(var built, _)    => LeavesOne(built, doors),
			_                               => false,
		};
}
