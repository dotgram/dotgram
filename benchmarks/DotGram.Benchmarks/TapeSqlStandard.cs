using System;

using DotGram.Handwritten;
using DotGram.Sql;
using DotGram.Sql.Standard;

namespace DotGram.Benchmarks;

/// <summary>
/// <see cref="SqlStandardParser"/>'s grammar (SQL:2023) compiled on the tape: the same file, the same
/// tree, and every <c>=&gt;</c> recorded as it is read and run by a walk once the parse is accepted,
/// where the shipped parser runs each the moment its alternative is read. The counterpart of
/// <see cref="TapeSql"/>, which is SQL-92 the same way: the package ships the immediate carrier and
/// this project carries the tape copy.
/// </summary>
/// <remarks>
/// <para>
/// The shipped parser was on the tape until 2026-10-06 and this class was the immediate copy it was
/// measured against (<c>ImmediateSqlStandard</c>; <c>benchmarks/results/2026-09-27-sql2023-immediate</c>).
/// The carriers have changed places and the comparison is kept: the stand's <c>sql/</c> rows read
/// each text by hand, by the shipped parser and by this copy, and <c>--sql2023-stack</c> holds the two
/// carriers' frames against each other. It is not the shipped parser: a caller who wants SQL:2023 on
/// the tape compiles the grammar with <c>Carrier = GramCarrier.Tape</c> as this class does.
/// </para>
/// <para>
/// It is carried whole, one class and no suppression: the tape is the carrier the generator falls
/// back to, so there is no GRAM5007 to hear and nothing to measure the tape against itself by mistake.
/// </para>
/// </remarks>
[Gram("SqlStandard.gram", Lexical = true, Carrier = GramCarrier.Tape)]
public static partial class TapeSqlStandard
{
}
