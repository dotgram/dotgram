using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;

namespace DotGram.Grammar.Plan;

/// <summary>Which proof settled that a choice or a repetition writes no way on the tape.</summary>
enum WriteProof
{
	/// <summary>None did: it writes a way.</summary>
	None,

	/// <summary>The choice is one run of text, and no shorter text can be wanted by what follows.</summary>
	LiteralRun,

	/// <summary>No alternative of the choice can hold where another held.</summary>
	Exclusive,

	/// <summary>The choice is text that some character tells apart, built from or ignoring case.</summary>
	SettledText,

	/// <summary>The repetition takes exactly as many turns as it may.</summary>
	Fixed,

	/// <summary>What follows the repetition can never fail and want a turn back.</summary>
	NeverGivesBack,

	/// <summary>What follows the one-character stop the repetition is read past can never want a turn back.</summary>
	NeverGivesBackPast,
}

/// <summary>Whether a choice or a repetition writes a way on the tape, and the proof where it does not.</summary>
sealed record WriteDecision(WriteProof Proof)
{
	/// <summary>No proof held: the reading writes a way to come back to.</summary>
	public static readonly WriteDecision Writes = new(WriteProof.None);

	/// <summary>Whether it writes a way.</summary>
	public bool WritesAWay => Proof == WriteProof.None;

	/// <summary>Settled by <paramref name="proof"/>.</summary>
	public static WriteDecision By(WriteProof proof)
	{
		return proof == WriteProof.None ? Writes : new WriteDecision(proof);
	}
}

/// <summary>
/// Every decision about a way on the tape that one machine's readers take: for each choice and
/// repetition, where it stands and what follows it there.
/// </summary>
/// <remarks>
/// <para>
/// Keyed by the rule whose reader it is in, the node — a repetition, or the first of the alternatives
/// a choice is read among, with how many they are — and what follows it there, since one node can be
/// read in two places: a rule written in place where it is called is read in each caller, and what
/// follows differs. The same key answered twice otherwise throws, and so does a key asked for and
/// never answered: a place the readers are printed in that the planning stage did not read.
/// </para>
/// <para>
/// A reader of the plan that does not know what follows a node may ask for the answer it has in
/// every place it is read (<see cref="Agreed"/>), and gets none where they differ.
/// </para>
/// </remarks>
sealed class WriteDecisions
{
	readonly Dictionary<Key, WriteDecision> _decisions = [];

	Dictionary<Key, WriteDecision?>? _agreed;

	/// <summary>How many decisions are held.</summary>
	public int Count => _decisions.Count;

	/// <summary>The proof of every place decided, one a place (<see cref="WriteProof.None"/> where it writes a way).</summary>
	public IEnumerable<WriteProof> Proofs()
	{
		foreach (var held in _decisions)
			yield return held.Value.Proof;
	}

	/// <summary>Holds the decision for a place, where the same place has not been answered otherwise.</summary>
	/// <param name="count">How many alternatives the choice is read among, starting at <paramref name="node"/>; 0 for a repetition.</param>
	/// <param name="context">What follows the node there, as <see cref="WriteContext"/> says it.</param>
	public void Record(RuleSymbol owner, Node node, int count, long context, WriteDecision decision)
	{
		var key = new Key(owner, node, count, context);

		if (_decisions.TryGetValue(key, out var held))
		{
			if (held != decision)
				throw new InvalidOperationException(
					$"A {(count == 0 ? "repetition" : "choice")} of '{owner.Name}' was decided twice in one place, as {held.Proof} and as {decision.Proof}.");

			return;
		}

		_decisions.Add(key, decision);
		_agreed = null;
	}

	/// <summary>The decision for a place where it is held already; null where it is not.</summary>
	public WriteDecision? Held(RuleSymbol owner, Node node, int count, long context)
	{
		return _decisions.TryGetValue(new Key(owner, node, count, context), out var decision) ? decision : null;
	}

	/// <summary>The decision for a place.</summary>
	/// <exception cref="InvalidOperationException">The place was never decided: the planning stage did not read it.</exception>
	public WriteDecision Of(RuleSymbol owner, Node node, int count, long context)
	{
		if (!_decisions.TryGetValue(new Key(owner, node, count, context), out var decision))
			throw new InvalidOperationException(
				$"A {(count == 0 ? "repetition" : "choice")} of '{owner.Name}' is printed where the plan did not decide it.");

		return decision;
	}

	/// <summary>
	/// The decision for a node of a rule's reader wherever it is read, where every place agrees; null
	/// where they differ or the node was not read. With no rule, wherever any reader reads the node:
	/// a rule written in place where it is called is read in its callers' readers, not its own.
	/// </summary>
	public WriteDecision? Agreed(RuleSymbol? owner, Node node, int count)
	{
		if (_agreed is null)
		{
			var agreeing = new Dictionary<Key, WriteDecision?>();

			foreach (var held in _decisions)
			{
				Agree(agreeing, held.Key with { Context = 0 }, held.Value);
				Agree(agreeing, held.Key with { Owner = null, Context = 0 }, held.Value);
			}

			_agreed = agreeing;
		}

		return _agreed.TryGetValue(new Key(owner, node, count, 0), out var agreed) ? agreed : null;

		static void Agree(Dictionary<Key, WriteDecision?> agreeing, Key place, WriteDecision decision)
		{
			agreeing[place] = agreeing.TryGetValue(place, out var other) ? other == decision ? other : null : decision;
		}
	}

	/// <summary>A place: the rule and the node by identity, how many alternatives, and what follows.</summary>
	readonly record struct Key(RuleSymbol? Owner, Node Node, int Count, long Context)
	{
		public bool Equals(Key other)
		{
			return ReferenceEquals(Owner, other.Owner) && ReferenceEquals(Node, other.Node) && Count == other.Count && Context == other.Context;
		}

		public override int GetHashCode()
		{
			return unchecked(((RuntimeHelpers.GetHashCode(Owner) * 397 ^ RuntimeHelpers.GetHashCode(Node)) * 397 ^ Count) * 397 ^ Context.GetHashCode());
		}
	}
}

/// <summary>What follows a node where a decision is taken, said as one number (<see cref="WriteDecisions"/>).</summary>
/// <remarks>
/// Structural: two continuations that read alike give the same number however they were built, since
/// the planning stage and the printer build their own.
/// </remarks>
static class WriteContext
{
	/// <summary>What a choice is decided against: what can begin after it.</summary>
	public static long Of(FirstSets.First following)
	{
		return Hash(Seed, following);
	}

	/// <summary>What a repetition is decided against: what follows it, the rule's trivia, and the stop it is read past.</summary>
	public static long Of(FollowSets.Continuation taught, RuleSymbol? seam, (char Stop, FollowSets.Continuation Beyond)? stopped)
	{
		var hash = Hash(Hash(Seed, taught), seam is null ? 0 : seam.GetHashCode());

		return stopped is { } past ? Hash(Hash(hash, past.Stop), past.Beyond) : hash;
	}

	const long Seed  = unchecked((long)0xcbf29ce484222325);
	const long Prime = 0x100000001b3;

	static long Hash(long hash, long value)
	{
		return unchecked((hash ^ value) * Prime);
	}

	static long Hash(long hash, FollowSets.Continuation continuation)
	{
		return Hash(Hash(Hash(hash, continuation.Plain), continuation.AfterSeam), continuation.Lead.GetHashCode());
	}

	static long Hash(long hash, FirstSets.First set)
	{
		hash = Hash(hash, (set.Anything ? 1 : 0) | (set.Nothing ? 2 : 0) | (set.Ends ? 4 : 0) | (set.Stops ? 8 : 0));

		foreach (var range in set.Ranges)
			hash = Hash(hash, range.From << 16 | range.To);

		return Hash(hash, set.Ranges.Count);
	}
}
