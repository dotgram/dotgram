using System;

using DotGram.Parsers.Sql;

namespace DotGram.Benchmarks;

/// <summary>
/// <see cref="SqlStandard92"/>'s grammar compiled with the mixed carrier: the same file,
/// the same tree, and every construction run after the parse is accepted as the tape runs
/// them — over a typed shape per rule rather than over a log.
/// </summary>
/// <remarks>
/// The third reading in the yardstick, and the one the second stage exists to measure:
/// what deferral costs when it is done well. The tape is above it and the immediate
/// carrier, which defers nothing, is below. A build that says GRAM5007 about this file is
/// a build where the column below is the tape and means nothing.
/// </remarks>
[Gram("SqlStandard92.gram", Lexical = true, Carrier = GramCarrier.Mixed)]
public static partial class MixedSql
{
}
