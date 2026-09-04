using System;

using DotGram.Parsers;

namespace DotGram.Benchmarks;

/// <summary>
/// <see cref="SqlStandard92"/>'s grammar compiled eagerly: the same file, the same tree,
/// and every <c>=&gt;</c> run the moment its alternative is read rather than after the
/// parse is accepted.
/// </summary>
/// <remarks>
/// <para>
/// This is the ceiling the redesign measures against (<c>docs/next.md</c>). The hand-written
/// parser in <see cref="HandSqlTokens"/> builds where it reads, and if this parser comes out
/// level with it then the whole of the remaining gap between generated and hand-written is
/// the price of deferral; if it does not, the remainder is in recognition and is worth
/// knowing before a deferred carrier is written.
/// </para>
/// <para>
/// It is not the shipped parser and could not be: eager construction calls a factory once
/// per derivation tried, and <see cref="SqlNode"/>'s factories happen to be pure, which is
/// what makes the comparison fair rather than what makes it safe.
/// </para>
/// </remarks>
[Gram("SqlStandard92.gram", Lexical = true, Carrier = GramCarrier.Eager)]
public static partial class EagerSql
{
}
