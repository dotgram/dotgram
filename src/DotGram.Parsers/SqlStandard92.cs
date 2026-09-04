using System;

using DotGram;

namespace DotGram.Parsers;

/// <summary>
/// The expression layer of standard SQL — <c>&lt;value expression&gt;</c> and
/// <c>&lt;search condition&gt;</c> — as ISO/IEC 9075:1992 defines them.
/// </summary>
/// <remarks>
/// <para>
/// The bottom of the language, and deliberately only the bottom. SQL divides into
/// expressions, clauses and statements, and the two above cannot be written honestly
/// without the one below: a <c>SELECT</c> is mostly places where an expression stands.
/// So this is the expression layer, finished and testable on its own, and the query
/// level goes above it later.
/// </para>
/// <para>
/// <b>The rule names are the standard's, production for production.</b>
/// <c>SearchCondition</c>, <c>BooleanTerm</c>, <c>BooleanFactor</c>, <c>BooleanTest</c>,
/// <c>BooleanPrimary</c>, <c>Predicate</c>, <c>RowValueConstructor</c>,
/// <c>ValueExpression</c>, <c>Term</c>, <c>Factor</c>, <c>ValueExpressionPrimary</c> —
/// so that a reader with the standard open can follow one against the other, and so that
/// a disagreement about what something should do is settled by a section number rather
/// than by taste. Where a name would have come from an implementation's object model
/// instead, it did not.
/// </para>
/// <para>
/// The standard's edition is 1992, which is the compact core the later ones extend, and
/// what is taken from later editions is marked where it is taken.
/// </para>
/// <para>
/// <b>One divergence, and it is written where it happens.</b> §6.11 gives four value
/// towers — numeric, string, datetime, interval — that share a bottom, so
/// <c>a + b</c> belongs to two of them at once and only the types of <c>a</c> and
/// <c>b</c> say which. That is not a defect in the standard: §6.11 describes syntax
/// modulo type resolution, and a parser has no types. The four are one untyped ladder
/// here, which is what every implementation does.
/// </para>
/// <para>
/// Nothing is built yet. The grammar recognizes and the tree comes later, deliberately:
/// the shape of the node classes is a decision of its own, and getting the language
/// right first is what makes that decision about the tree rather than about the parse.
/// </para>
/// </remarks>
[Gram("SqlStandard92.gram", Lexical = true)]
public static partial class SqlStandard92
{
}
