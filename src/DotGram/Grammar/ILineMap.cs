using System;

namespace DotGram.Grammar;

/// <summary>
/// Where a position in the grammar is, as a file and a line somebody can open (§7.6).
/// </summary>
/// <remarks>
/// <para>
/// The generated file carries `#line` directives over the C# a grammar handed across —
/// a `when`, a `=>` — so that the C# compiler's own complaints about that code land on
/// the grammar's line rather than inside a machine-written file. Nothing else about
/// generation needs to know where anything came from.
/// </para>
/// <para>
/// A seam because the answer depends on how the grammar reached the compiler. A `.gram`
/// file maps a position onto itself. A grammar written into a `[Gram("…")]` attribute is
/// a string inside somebody's C# file, and finding which line of that file a position
/// falls on is a question about a syntax tree — so the shell answers it
/// (.claude/rules/grammar-half.md).
/// </para>
/// </remarks>
public interface ILineMap
{
	/// <summary>
	/// Where <paramref name="position"/> is, or false where it cannot be placed.
	/// </summary>
	/// <param name="line">1-based, as `#line` counts.</param>
	/// <param name="column">
	/// 1-based. The emitter pads the line it writes out to this column, so that a squiggle
	/// under one argument of a `=>` lands under that argument and not at the start of it.
	/// </param>
	/// <remarks>
	/// False rather than a guess: a directive pointing at the wrong line is worse than no
	/// directive at all, because the error then names a place the author will read and
	/// find nothing wrong with.
	/// </remarks>
	bool TryMap(int position, out string file, out int line, out int column);
}
