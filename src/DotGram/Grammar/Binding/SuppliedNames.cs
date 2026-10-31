using System;
using System.Collections.Generic;
using System.Linq;

namespace DotGram.Grammar.Binding;

/// <summary>
/// The names the parser supplies to a <c>=&gt;</c>, a <c>when</c> and a <c>recover</c>
/// factory, in the order a failure factory takes them (docs/syntax.md §8.2).
/// </summary>
/// <remarks>
/// <para>
/// Here rather than beside the recovery model that used to own it, because both halves of
/// the compiler need the same answer and only one of them could reach it there. The binder
/// asks so that an argument naming one of these resolves — §2 makes a bare name in an
/// argument list a grammar name, and these are among the names a rule has. The emitter asks
/// so that a capture may not take one, and so that a factory naming one is handed it.
/// </para>
/// <para>
/// One list rather than two that have to agree. The order is the recovery factory's own
/// parameter order, which is the only place the order is load-bearing.
/// </para>
/// </remarks>
public static class SuppliedNames
{
	public static readonly IReadOnlyList<string> All =
		[
			"parserText", "parserPosition", "parserOrdinal",
			"parserLine", "parserColumn",   "parserSpan", "parserMessage",
			"parserExpected", "parserFailurePosition", "parserFailureLine", "parserFailureColumn",
			"parserInput", "parserState", "parserMarks",
		];

	/// <summary>What a construction's <c>=&gt;</c> is handed, besides the two a <c>state</c> brings.</summary>
	public static readonly IReadOnlyCollection<string> ToConstructions =
		new HashSet<string>(["parserText", "parserSpan", "parserInput"], StringComparer.Ordinal);

	/// <summary>
	/// What a <c>when</c> is handed. A guard runs while the text is read: what stands over a reading is
	/// not settled until it is accepted, and the input is what a value keeps, which a guard builds none of.
	/// </summary>
	public static readonly IReadOnlyCollection<string> ToGuards =
		new HashSet<string>(["parserText", "parserSpan"], StringComparer.Ordinal);

	/// <summary>What only a grammar that declares <c>state : @T</c> hands over.</summary>
	public static readonly IReadOnlyCollection<string> WithState =
		new HashSet<string>(["parserState", "parserMarks"], StringComparer.Ordinal);

	/// <summary>What a recovery's factory, <c>recover Sync =&gt; …</c>, is handed: everything but the marks.</summary>
	public static readonly IReadOnlyCollection<string> ToRecoveries =
		new HashSet<string>(All.Where(static name => name is not ("parserState" or "parserMarks")), StringComparer.Ordinal);
}
