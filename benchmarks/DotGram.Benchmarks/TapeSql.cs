using System;

using DotGram.Handwritten;
using DotGram.Sql;
using DotGram.Sql.Standard;

namespace DotGram.Benchmarks;

/// <summary>
/// <see cref="Sql92Parser"/>'s grammar compiled immediately: the same file, the same tree,
/// and every <c>=&gt;</c> run the moment its alternative is read rather than after the
/// parse is accepted.
/// </summary>
/// <remarks>
/// <para>
/// This is the ceiling the redesign measures against (<c>docs/next.md</c>). A parser written by
/// hand builds where it reads, and if this one comes out level with it then the whole of the
/// remaining gap between generated and handwritten is the price of deferral; if it does not, the
/// remainder is in recognition and is worth knowing before a deferred carrier is written. The
/// handwritten mark is <c>HandSqlStandard</c> against <c>SqlStandardParser</c>, over SQL:2023.
/// </para>
/// <para>
/// It is not the shipped parser and could not be: immediate construction calls a factory once
/// per derivation tried, and the tree's factories happen to be pure, which is
/// what makes the comparison fair rather than what makes it safe.
/// </para>
/// </remarks>
// GRAM5015 is right about this request and it is wanted anyway, for the reason the second
// paragraph gives: a factory runs once per derivation tried, including the ones the parse
// gives up, and this tree's factories are pure, so such a reading leaves nothing behind.
// The grammar is a file rather than a literal, so the message lands in the .gram and no
// pragma here reaches it; the suppression is NoWarn in this project, scoped to this host.
[Gram("SqlStandard92.gram", Lexical = true, Carrier = GramCarrier.Immediate)]
public static partial class ImmediateSql
{
}
