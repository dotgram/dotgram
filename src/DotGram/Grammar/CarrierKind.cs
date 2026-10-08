using System;

namespace DotGram.Grammar;

/// <summary>
/// How a generated reader carries what it has read until the author's constructions run.
/// </summary>
/// <remarks>
/// <para>
/// The reader recognizes the same way whichever is chosen; what differs is where the
/// pieces of a value wait and when a <c>=&gt;</c> is called (<c>docs/next.md</c>, the
/// redesign). The default is <see cref="Immediate"/> wherever it can carry a machine; the
/// others are the author's to choose, with what each gives up written beside it.
/// </para>
/// </remarks>
public enum CarrierKind
{
	/// <summary>
	/// The default: compiled exactly as <see cref="Immediate"/>, and on the <see cref="Tape"/>
	/// for a machine the immediate carrier refuses.
	/// </summary>
	/// <remarks>
	/// Where the tape would hold back constructions the immediate carrier runs — a rule the
	/// machine builds read for a derivation that may not stand, or read again after it has
	/// answered — the grammar is told: <c>GRAM5016</c>, a warning, where constructions share
	/// <c>context</c> with a guard, a selector or a recognizer, or a refused input can rebuild
	/// what it read on every way back; <c>GRAM5012</c>, information, for the rest. A carrier
	/// named is told neither.
	/// </remarks>
	Auto,

	/// <summary>
	/// Records on a tape, built into values by a walk once the parse has been accepted. The
	/// carrier that streams, finds and recovers, and the one that keeps §3.7 whole.
	/// </summary>
	Tape,

	/// <summary>
	/// No deferral: a <c>=&gt;</c> runs the moment its alternative has been read, and an
	/// alternative abandoned afterwards has already run it. One input and one grammar give
	/// one sequence of calls every time, so nothing is nondeterministic — but a factory is
	/// called once per derivation <em>tried</em> rather than once per derivation accepted,
	/// which a pure allocation never notices and a counter does. On input the parse refuses,
	/// up to twice per derivation tried: a refusal is read a second time to say what was
	/// expected, and that reading runs what the first ran. What a grammar that names no
	/// carrier gets wherever it can; named, it says the author has checked.
	/// </summary>
	Immediate,
}
