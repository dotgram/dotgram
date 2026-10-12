using System;

using DotGram;

namespace DotGram.Sql.Standard;

/// <summary>
/// SQL-92, kept for now. The expression layer of standard SQL — <c>&lt;value expression&gt;</c> and
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
/// <b>What it reads it builds as the tree of <c>DotGram.Sql.Ast</c></b>, the one
/// <see cref="SqlStandardParser"/> builds, and in the shape that parser gives the same text: the same
/// records, members, list order, identifier styles, folds and brackets, so a text both read is one
/// tree, and <c>Sql2023Writer</c> prints it back. <c>ParseSelect</c> gives a <c>Statement.Select</c>,
/// <c>ParseQuery</c> a <c>Query</c>, and <c>ParseSearchCondition</c> and <c>ParseValueExpression</c> an
/// <c>Expression</c>. What SQL-92 has and SQL:2003 took out — a bit string literal, <c>BIT</c> and
/// <c>BIT VARYING</c>, a union join — is SQL-92's own in that tree (<c>Ast/SqlAst.Sql92.cs</c>). An interval
/// qualifier is read as §10.1 writes it, and a length or precision is no larger than the tree's
/// <c>int</c>: what SQL-92 does not say is refused, and nothing it reads is left without a record.
/// </para>
/// <para>
/// <b>Temporary.</b> The standard's parser is <c>SqlStandardParser</c>, written from SQL:2023 down
/// through the earlier editions (docs/design/sql-parsers.md). This one stays while the benchmarks
/// hold it against a hand-written recognizer, and goes when they do not; T-SQL no longer includes it.
/// </para>
/// <para>
/// <b>It is carried immediately, by name.</b> A node is built the moment its rule has been read, and
/// nothing is recorded on a tape and replayed once the parse is accepted. Most of its building rules
/// are read for derivations the parse may give up — an ordered choice tries the longer alternative
/// first — and that is what the request costs: a construction runs for a reading that is then
/// abandoned, and an exception thrown there would escape the publication. None is: the records'
/// constructors and the helpers of <c>Nodes</c> it calls build from what they are given, throw on
/// nothing the grammar hands them and read no host state, so a derivation given up leaves nothing a
/// later one could read. Held to
/// the tape by an agreement run over the ScriptDom corpus, each statement whole and cut, every string
/// of the tests, every accepted text cut at each word boundary and given a stray token, through all
/// four entries: the same trees, the same messages, nothing thrown; the benchmarks compile the same
/// grammar on the tape (<c>TapeSql</c>) and hold the two carriers to each other. What it buys is the
/// time the tape spent recording and replaying, for more bytes allocated where a reading is abandoned
/// part-way — and, on a text refused late, after a long accepted prefix, the time those constructions
/// took.
/// </para>
/// </remarks>
[Gram("SqlStandard92.gram", Lexical = true, Carrier = GramCarrier.Immediate)]
public abstract partial class Sql92Parser
{
}
