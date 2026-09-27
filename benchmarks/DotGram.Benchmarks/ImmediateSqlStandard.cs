using System;

using DotGram.Handwritten;
using DotGram.Sql;
using DotGram.Sql.Standard;

namespace DotGram.Benchmarks;

/// <summary>
/// <see cref="SqlStandardParser"/>'s grammar (SQL:2023) compiled immediately: the same file, the
/// same tree, and every <c>=&gt;</c> run the moment its alternative is read rather than after the
/// parse is accepted. The counterpart of <see cref="ImmediateSql"/>, which is SQL-92.
/// </summary>
/// <remarks>
/// <para>
/// The 2026-09-05 comparison on SQL-92 read the immediate carrier at 1.1-1.6x of the hand-written
/// parser against the shipped tape's 2.1-3.0x; this grammar has not been measured that way. If it
/// compiles at all: SQL:2023 has 302 value tables and eight value types where SQL-92 has far fewer,
/// so a wider dense store is exercised here that the SQL-92 comparison never touched.
/// </para>
/// <para>
/// It is not the shipped parser and could not be: immediate construction calls a factory once
/// per derivation tried, and the tree's factories happen to be pure, which is
/// what makes the comparison fair rather than what makes it safe.
/// </para>
/// </remarks>
[Gram("SqlStandard.gram", Lexical = true, Carrier = GramCarrier.Immediate)]
public static partial class ImmediateSqlStandard
{
}
