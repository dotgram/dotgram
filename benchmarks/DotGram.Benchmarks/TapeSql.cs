using System;

using DotGram.Handwritten;
using DotGram.Sql;
using DotGram.Sql.Standard;

namespace DotGram.Benchmarks;

/// <summary>
/// <see cref="Sql92Parser"/>'s grammar compiled on the tape: the same file, the same tree, and
/// every <c>=&gt;</c> recorded as it is read and run by a walk once the parse is accepted, where
/// the shipped parser runs each the moment its alternative is read. The counterpart of
/// <see cref="TapeSqlStandard"/>, which is SQL:2023 on the tape beside its shipped immediate parser.
/// </summary>
/// <remarks>
/// <para>
/// Until 2026-10-06 this class was <c>ImmediateSql</c>, the immediate carrier the shipped tape was
/// measured against: the ceiling the redesign measured towards (<c>docs/next.md</c>), a parser that
/// builds where it reads, as one written by hand does. The shipped parser is now carried that way,
/// and the comparison is kept the other way about: <c>SqlComparisonBenchmarks</c>, <c>SqlCounters</c>,
/// <c>--slope</c>, <c>--big</c> and <c>--spin</c> read each input by the shipped parser and by this
/// copy, and the stand's <c>sql92/</c> rows time the two side by side. It is not the shipped parser:
/// a caller who wants SQL-92 on the tape compiles the grammar with <c>Carrier = GramCarrier.Tape</c>
/// as this class does.
/// </para>
/// <para>
/// The tape is the carrier the generator falls back to, so there is no GRAM5007 or GRAM5015 to
/// hear here and nothing to measure the tape against itself by mistake.
/// </para>
/// </remarks>
[Gram("SqlStandard92.gram", Lexical = true, Carrier = GramCarrier.Tape)]
public static partial class TapeSql
{
}
