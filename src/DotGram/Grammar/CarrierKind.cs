using System;

namespace DotGram.Grammar;

/// <summary>
/// How a generated reader carries what it has read until the author's constructions run.
/// </summary>
/// <remarks>
/// <para>
/// The reader recognizes the same way whichever is chosen; what differs is where the
/// pieces of a value wait and when a <c>=&gt;</c> is called (<c>docs/next.md</c>, the
/// redesign). The default keeps §7.3's promise — a construction runs once, for the
/// derivation that was accepted — and the others are the author's to choose, with what
/// each gives up written beside it.
/// </para>
/// </remarks>
public enum CarrierKind
{
	/// <summary>
	/// Records on a tape, built into values by a walk once the parse has been accepted. The
	/// carrier that streams, finds and recovers, and the default.
	/// </summary>
	Tape,

	/// <summary>
	/// No deferral: a <c>=&gt;</c> runs the moment its alternative has been read, and an
	/// alternative abandoned afterwards has already run it. One input and one grammar give
	/// one sequence of calls every time, so nothing is nondeterministic — but a factory is
	/// called once per derivation <em>tried</em> rather than once per derivation accepted,
	/// which a pure allocation never notices and a counter does. For authors who know their
	/// factories are pure; never chosen for them.
	/// </summary>
	Immediate,

	/// <summary>
	/// Deferral without a tape: what a rule read is kept in a typed shape of its own, and
	/// the author's constructions run over those shapes once the parse is accepted. Keeps
	/// §7.3 as the tape does, and pays for it in fields of a known type rather than in a
	/// log and a walk over it.
	/// </summary>
	/// <remarks>
	/// Written one shape at a time, and it carries none of them yet: a grammar asking for
	/// it is compiled on the tape and told which shape it was refused for. Not offered on
	/// <c>[Gram]</c> until there is something to offer.
	/// </remarks>
	Mixed,
}
