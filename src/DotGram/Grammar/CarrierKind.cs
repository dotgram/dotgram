using System;

namespace DotGram.Grammar;

/// <summary>
/// How a generated reader carries what it has read until the author's constructions run.
/// </summary>
/// <remarks>
/// <para>
/// The reader recognizes the same way whichever is chosen; what differs is where the
/// pieces of a value wait and when a <c>=&gt;</c> is called (<c>docs/next.md</c>, the
/// redesign). The default is the generator's choice between the first two; the others are
/// the author's to choose, with what each gives up written beside it.
/// </para>
/// </remarks>
public enum CarrierKind
{
	/// <summary>
	/// Chosen by the generator, and the default: <see cref="Immediate"/> where it keeps §3.7
	/// for every parse that succeeds, <see cref="Tape"/> everywhere else.
	/// </summary>
	/// <remarks>
	/// A machine is carried immediately where every rule it builds is read only for the
	/// derivation that stands, or for one the whole parse then fails on, and where no rule of
	/// it can be read again after it has answered. What that gives up is the one case the
	/// tape still covers: a parse that fails has already run the constructions of what it
	/// read before failing. Said by <c>GRAM5012</c>, either way.
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
	/// expected, and that reading runs what the first ran. For authors who know their
	/// factories are pure; never chosen for them.
	/// </summary>
	Immediate,
}
