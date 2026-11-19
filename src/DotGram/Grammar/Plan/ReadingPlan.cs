using System;
using System.Collections.Generic;

using DotGram.Grammar.Binding;

namespace DotGram.Grammar.Plan;

/// <summary>
/// What the emitter decided about a file before it wrote a line of it: which machines the file
/// holds, how each is read and carried, where its readers recurse, and what the target could not
/// read at all.
/// </summary>
/// <remarks>
/// <para>
/// The answers are worked out once, by the planning stage, from the graph, the target's
/// capabilities and the options; the code that prints a parser reads them here and decides none of
/// them again. A plan that refuses something (<see cref="Refusals"/>) is not printed: what the
/// target cannot read is said to the author, and no parser is written for it.
/// </para>
/// <para>
/// The facts are named for what they mean, not for the code that prints them, so that a printer of
/// another language can read the same plan. They are held in classes rather than one closed list of
/// cases, so that a fact added later joins them without touching what reads the ones there.
/// </para>
/// </remarks>
sealed class ReadingPlan(IReadOnlyList<MachinePlan> machines, IReadOnlyList<GramDiagnostic> refusals)
{
	/// <summary>Every machine the file holds, in the order they are written: one entry a machine, and no machine without one.</summary>
	public IReadOnlyList<MachinePlan> Machines { get; } = machines;

	/// <summary>What the target cannot read, said where the grammar asks for it: empty where the plan is printed.</summary>
	public IReadOnlyList<GramDiagnostic> Refusals { get; } = refusals;

	/// <summary>Whether the target cannot read the file as it is asked for, so that nothing is printed.</summary>
	public bool Refused => Refusals.Count > 0;
}

/// <summary>How a machine reads its input.</summary>
enum ReadingStrategy
{
	/// <summary>States and jumps, with no arena: a reading that never recurses, backtracks or holds a construction back.</summary>
	Flat,

	/// <summary>A method a rule, written as a hand-written reader would be.</summary>
	Reader,

	/// <summary>The machine of jumps over an arena, which reads what the other two cannot.</summary>
	Jump,
}

/// <summary>One machine of a file, and what was decided about it.</summary>
sealed class MachinePlan
{
	/// <summary>The name its engine is written under: a name no other machine of the file has.</summary>
	public required string Name { get; init; }

	/// <summary>The publications it reads; none for the machine that reads a token again, or for a grammar with nothing published.</summary>
	public required IReadOnlyList<Publication> Publications { get; init; }

	/// <summary>How it reads.</summary>
	public required ReadingStrategy Strategy { get; init; }

	/// <summary>
	/// Why a cheaper way of reading was not taken, in words an author can act on: why it is not flat,
	/// and for the jump machine why the reader was not taken either. Null for a flat machine.
	/// </summary>
	public string? Reason { get; init; }

	/// <summary>
	/// Where the target cannot read the machine the way it was asked to — a jump machine for a target
	/// with no jumps — what is said of it; null where it can.
	/// </summary>
	public string? Refusal { get; init; }

	/// <summary>Whether it reads only the forms of a flat machine's publications that begin where they are told.</summary>
	public bool PositionalOnly { get; init; }

	/// <summary>
	/// Of each publication a flat machine reads, whether it is lowered with its value; empty for any
	/// other machine.
	/// </summary>
	public IReadOnlyDictionary<Publication, bool> FlatValued { get; init; } = new Dictionary<Publication, bool>();

	/// <summary>How its readers carry what they read; null where it has no readers to carry.</summary>
	public CarrierPlan? Carrier { get; init; }

	/// <summary>The calls its readers make that close a cycle (a stack check before each); null where it has no readers.</summary>
	public IReadOnlyCollection<(RuleSymbol From, RuleSymbol To)>? BackEdges { get; init; }

	/// <summary>Whether its readers defer building after a loop's second give-back, and where they do not; null where it has no readers.</summary>
	public DeferralPlan? Deferral { get; init; }

	/// <summary>Whether each choice and repetition its readers read writes a way on the tape, and the proof where not; null where it has no readers.</summary>
	public WriteDecisions? Writes { get; init; }
}

/// <summary>How a machine's readers carry what they read.</summary>
sealed class CarrierPlan
{
	/// <summary>What the author asked for: <see cref="CarrierKind.Auto"/> is carried as the immediate carrier where that one can.</summary>
	public required CarrierKind Requested { get; init; }

	/// <summary>Whether the readers build as they read; otherwise what they read is on the tape and built once it is accepted.</summary>
	public required bool Immediate { get; init; }

	/// <summary>Why the immediate carrier, asked for, could not carry the machine; null where it could or was not asked.</summary>
	public string? Refusal { get; init; }

	/// <summary>
	/// What the immediate carrier would say of a machine that has no readers, where the file has no
	/// reader at all and the immediate carrier was asked for by name; null otherwise.
	/// </summary>
	public string? WouldRefuse { get; init; }
}

/// <summary>Where a reader writes a loop that asks for another reading.</summary>
enum RetrySite
{
	/// <summary>A rule's own way back (<c>Read_X</c> around <c>Read_X_Body</c>).</summary>
	Rule,

	/// <summary>An entry's reading of the whole input, or from a position.</summary>
	Entry,

	/// <summary>One alternative of a choice over characters.</summary>
	Alternative,

	/// <summary>One turn of a repetition.</summary>
	Turn,

	/// <summary>An atomic group, sealed once it has answered.</summary>
	Atomic,

	/// <summary>A lookahead's subject, sealed once it has answered.</summary>
	Lookahead,

	/// <summary>The step of a repetition marked <c>recover</c>.</summary>
	Yield,
}

/// <summary>
/// Whether a machine's readers read unbuilt once a loop has given a way back twice, and, for every
/// rule they read and every kind of loop it can write, why such a loop does not.
/// </summary>
/// <remarks>
/// Facts of the graph and the carrier: the loops themselves are found as the readers are written, and
/// each asks here what was decided for its rule and its kind.
/// </remarks>
sealed class DeferralPlan(bool defersOnGiveBack, IReadOnlyDictionary<(RuleSymbol Owner, RetrySite Site), string?> excluded)
{
	/// <summary>Whether the machine defers at all: carried immediately, over a grammar with no context and no recovery.</summary>
	public bool DefersOnGiveBack { get; } = defersOnGiveBack;

	/// <summary>Why a loop of <paramref name="owner"/> at <paramref name="site"/> does not defer, or null where it does.</summary>
	/// <exception cref="InvalidOperationException">The plan has no answer for the rule: it was not planned as one the readers read.</exception>
	public string? Excluded(RuleSymbol owner, RetrySite site)
	{
		if (!excluded.TryGetValue((owner, site), out var why))
			throw new InvalidOperationException($"The plan says nothing of a loop of '{owner.Name}' at {site}: the rule was not planned as one the readers read.");

		return why;
	}
}
