using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Plan;

/// <summary>What one place of a reader records where it refuses, as the printer decided it.</summary>
/// <remarks>
/// <para>
/// A class a kind of place, each deriving from this one: a kind added later, or a fact added to a
/// kind, joins them without touching what reads the others. A reader of the plan asks for the kinds
/// it knows and lets the rest be.
/// </para>
/// <para>
/// The items are said as a refusal says them, before any target writes them in its own notation; a
/// rule's own words keep the mark they are told apart by. Which set of a file they are, by number, is
/// the printer's to say where it prints them.
/// </para>
/// </remarks>
class RecordSite
{
	/// <summary>What the place names as wanted there; empty where it names nothing.</summary>
	public IReadOnlyList<string> Items { get; init; } = [];

	/// <summary>Whether what is named is noted and not acted on: where it does not hold the reading goes on.</summary>
	public bool Noted { get; init; }

	/// <summary>
	/// Whether the quiet reading writes down how far it got here; not on a path only the quiet reading
	/// takes, where the reading that records says nothing of its own.
	/// </summary>
	public bool Furthest { get; init; } = true;

	/// <summary>Whether the place records its set at all; not where it is named for another place that records for it.</summary>
	public bool Records { get; init; } = true;

	/// <summary>Whether another answer for the same place says what this one says.</summary>
	public virtual bool SaysAs(RecordSite other)
	{
		return other.GetType() == GetType() && other.Noted == Noted && other.Furthest == Furthest && other.Records == Records &&
			other.Items.SequenceEqual(Items, StringComparer.Ordinal);
	}

	/// <summary>The kind and what it names, for a message about the place.</summary>
	public override string ToString()
	{
		return $"{GetType().Name} [{string.Join(" | ", Items)}]{(Noted ? " noted" : "")}{(Furthest ? "" : " not furthest")}{(Records ? "" : " not recorded")}";
	}
}

/// <summary>A literal, a set of characters, a look behind or a text read again: what it accepts.</summary>
sealed class TerminalSite : RecordSite;

/// <summary>
/// The last of a run of literals, refused for all of them where it fails: the literals still agreeing
/// at the deepest character any of them reached, said as a set. The literals themselves record nothing.
/// </summary>
sealed class RunSite : RecordSite;

/// <summary>
/// A choice dispatched by the item in hand — a switch on it, a table of groups, a chain of tests, the
/// item past the seam — refused as one where no alternative begins with it: the set of what each
/// alternative begins with, merged into one display.
/// </summary>
sealed class DispatchSite : RecordSite
{
	/// <summary>What each group the item tells apart begins with, in the order they are tried.</summary>
	public IReadOnlyList<IReadOnlyList<CharRange>> Begins { get; init; } = [];

	/// <summary>The alternatives of each group, by where they stand in the choice, tried in that order.</summary>
	public IReadOnlyList<IReadOnlyList<int>> Members { get; init; } = [];

	/// <summary>
	/// The group read by the call its one alternative begins with, which tests the item itself, and
	/// where that call refuses at the choice's place the choice's set is recorded there (over what the
	/// call said, where an <see cref="OverSite"/> says so); -1 where every group is tested.
	/// </summary>
	public int Called { get; init; } = -1;

	/// <summary>Whether the item told apart is the one past the seam every alternative begins with.</summary>
	public bool Seam { get; init; }

	/// <inheritdoc/>
	public override bool SaysAs(RecordSite other)
	{
		return base.SaysAs(other) && other is DispatchSite { Begins: var begins, Members: var members } && begins.Count == Begins.Count &&
			begins.Zip(Begins, static (one, two) => one.SequenceEqual(two)).All(static same => same) &&
			members.Count == Members.Count && members.Zip(Members, static (one, two) => one.SequenceEqual(two)).All(static same => same) &&
			((DispatchSite)other).Called == Called && ((DispatchSite)other).Seam == Seam;
	}
}

/// <summary>
/// The widest group of a dispatched choice, read by the call it begins with: what the group begins
/// with, said over the call's own refusal where the call refused at the same place.
/// </summary>
sealed class OverSite : RecordSite;

/// <summary>
/// A dispatched choice with one group led by a guard: what the choice wanted without that group, named
/// where the guard would not let it read.
/// </summary>
sealed class UnguardedSite : RecordSite;

/// <summary>A group of a dispatched choice whose members begin with a guard: what the other groups begin with, which the guard refuses with.</summary>
sealed class GuardedGroupSite : RecordSite;

/// <summary>
/// A choice tried in order, looked at first: the item in hand begins none of its alternatives, and it is
/// refused as one, with what they begin with merged into one display.
/// </summary>
sealed class PredictedSite : RecordSite
{
	/// <summary>What the alternatives begin with together: the item in hand outside it refuses the choice.</summary>
	public IReadOnlyList<CharRange> Begins { get; init; } = [];
}

/// <summary>An optional's door that does not open: what its alternatives begin with, noted, and the reading goes on with nothing.</summary>
sealed class OptionalSite : RecordSite
{
	/// <summary>What the alternatives but the empty one begin with: the item in hand outside it shuts the door.</summary>
	public IReadOnlyList<CharRange> Begins { get; init; } = [];
}

/// <summary>A repetition short of its fewest turns: what a turn would have begun with.</summary>
sealed class ShortSite : RecordSite;

/// <summary>
/// A repetition's door: a turn past the fewest not tried where the item in hand cannot begin it, and
/// nothing recorded there; what follows the repetition records where it refuses. Where the door shuts
/// below the fewest, what the turn would have begun with is said there (<see cref="RecordSite.Items"/>):
/// noted, where the reading goes on to fail after the loop, or refused at the door.
/// </summary>
sealed class DoorSite : RecordSite
{
	/// <summary>What a turn must begin with to be tried past the fewest.</summary>
	public IReadOnlyList<CharRange> Begins { get; init; } = [];

	/// <summary>
	/// Whether the door is shut on the quiet reading alone: the reading that records tries the turn
	/// anyway, so that what refused it is said.
	/// </summary>
	public bool QuietOnly { get; init; }

	/// <summary>Whether the door is shut only once the fewest turns are read; below them the turn is tried.</summary>
	public bool PastFewest { get; init; }

	/// <summary>Whether the repetition is a run of characters read by one test a character.</summary>
	public bool Scanned { get; init; }

	/// <inheritdoc/>
	public override bool SaysAs(RecordSite other)
	{
		return base.SaysAs(other) && other is DoorSite door && door.QuietOnly == QuietOnly && door.PastFewest == PastFewest &&
			door.Scanned == Scanned && door.Begins.SequenceEqual(Begins);
	}
}

/// <summary>What a fold's loop could have gone on with, noted where its turns stop.</summary>
sealed class TailsSite : RecordSite;

/// <summary>A look refused, said by the name of the rule it is the body of.</summary>
sealed class NamedSite : RecordSite;

/// <summary>A call of a rule that says its own refusal, refused where the rule read nothing: the rule's own words.</summary>
sealed class MessageSite : RecordSite;

/// <summary>
/// The end of the input or of a segment, wanted: a look that refuses there, or an entry that reads the
/// whole input and is left with some.
/// </summary>
sealed class EndSite : RecordSite;

/// <summary>
/// A look whose refusal names nothing: what is said there is left to what else is read that far. Where
/// it is not <see cref="RecordSite.Records"/>, the refusal is not recorded at all — a look in the seam, or
/// one that begins a turn — and otherwise it is recorded where the look stands, naming nothing.
/// </summary>
sealed class SilentSite : RecordSite;

/// <summary>
/// Every place one machine's readers can record what was expected, and what each records there, as
/// the printer decided it.
/// </summary>
/// <remarks>
/// <para>
/// Keyed as <see cref="WriteDecisions"/> are, by the rule whose reader it is in and the node — for a
/// choice the first of the alternatives it is read among, with how many they are — and by the kind
/// of place, since one node is more than one: a literal names its own set and, as the last of a run,
/// the run's. The node of a settled copy is keyed as the node of the rule it was copied from, and the
/// copy as that rule, so that a place of the copy and the same place of the rule are one.
/// </para>
/// <para>
/// Filled by the planning stage's pass over the rules; the printer reads it, and a place it prints
/// that was not filled throws. The same place answered twice otherwise throws as well. An entry's
/// reading is printed only, around a call of its rule made for it, and its places are noted as it is
/// printed.
/// </para>
/// </remarks>
sealed class RecordSites
{
	readonly Dictionary<Key, (RuleSymbol Owner, Node Node, int Count, RecordSite Site)> _sites = [];

	/// <summary>How many places are held.</summary>
	public int Count => _sites.Count;

	/// <summary>Every place held: the rule whose reader it is in, the node, how many alternatives (0 but for a choice), and what it records.</summary>
	public IEnumerable<(RuleSymbol Owner, Node Node, int Count, RecordSite Site)> All()
	{
		return _sites.Values;
	}

	/// <summary>Holds what a place records, where the same place has not been answered otherwise; the answer held.</summary>
	/// <param name="count">How many alternatives a choice is read among, starting at <paramref name="node"/>; 0 for any other place.</param>
	/// <exception cref="InvalidOperationException">The place was answered otherwise before.</exception>
	public T Record<T>(RuleSymbol owner, Node node, int count, T site)
		where T : RecordSite
	{
		var key = new Key(owner, node, count, typeof(T));

		if (_sites.TryGetValue(key, out var held))
		{
			if (!held.Site.SaysAs(site))
				throw new InvalidOperationException(
					$"A place of '{owner.Name}' at {node} records two things: {held.Site} and {site}.");

			return (T)held.Site;
		}

		_sites.Add(key, (owner, node, count, site));

		return site;
	}

	/// <summary>What a place records, where it is held; null where it is not.</summary>
	public T? Held<T>(RuleSymbol owner, Node node, int count)
		where T : RecordSite
	{
		return _sites.TryGetValue(new Key(owner, node, count, typeof(T)), out var held) ? (T)held.Site : null;
	}

	/// <summary>What a place records.</summary>
	/// <exception cref="InvalidOperationException">The place was never answered: the planning stage did not read it.</exception>
	public T Of<T>(RuleSymbol owner, Node node, int count)
		where T : RecordSite
	{
		if (!_sites.TryGetValue(new Key(owner, node, count, typeof(T)), out var held))
			throw new InvalidOperationException(
				$"A place of '{owner.Name}' at {node} ({typeof(T).Name}) is printed where the plan did not say what it records.");

		return (T)held.Site;
	}

	/// <summary>A place: the rule and the node by identity, how many alternatives, and the kind.</summary>
	readonly record struct Key(RuleSymbol Owner, Node Node, int Count, Type Kind)
	{
		public bool Equals(Key other)
		{
			return ReferenceEquals(Owner, other.Owner) && ReferenceEquals(Node, other.Node) && Count == other.Count && Kind == other.Kind;
		}

		public override int GetHashCode()
		{
			return unchecked(((RuntimeHelpers.GetHashCode(Owner) * 397 ^ RuntimeHelpers.GetHashCode(Node)) * 397 ^ Count) * 397 ^ Kind.GetHashCode());
		}
	}
}
