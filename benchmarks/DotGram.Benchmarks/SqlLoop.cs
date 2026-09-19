using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

using DotGram.Handwritten;
using DotGram.Sql.Standard;

namespace DotGram.Benchmarks;

/// <summary>
/// One SQL:2023 statement read over and over by one reading alone, so that a profiler sees that
/// reading and nothing else: <c>sql-loop generated|hand [calls]</c>.
/// </summary>
/// <remarks>
/// The statement is the stand's <c>select20</c> row, twenty columns and a condition. The word has no
/// dashes because dotTrace takes an argument that begins with one for its own; start it on the
/// apphost by full path (.claude/rules/profiling.md §4). The calls are warmed until the tier-1 code
/// is in, the heap collected, and then timed with what they allocated.
/// </remarks>
static class SqlLoop
{
	public static readonly string Select20 =
		"SELECT " + string.Join(", ", Enumerable.Range(0, 20).Select(static i => "a" + i)) + " FROM t WHERE a0 = 1";

	public static void Run(string which, int calls)
	{
		Func<bool> read = which switch
		{
			"generated" => static () => SqlStandardParser.TryParseQueryExpression(Select20).IsSuccess,
			"hand"      => static () => HandSqlStandard.TryParseQueryExpression(Select20, out _),
			_           => throw new ArgumentException($"'{which}' is not generated or hand."),
		};

		if (!read())
			throw new InvalidOperationException($"{which} does not read the statement.");

		var sink  = 0;
		var watch = Stopwatch.StartNew();

		while (watch.Elapsed.TotalSeconds < 3)
			sink += read() ? 1 : 0;

		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();

		var allocated = GC.GetTotalAllocatedBytes(true);

		watch.Restart();

		for (var call = 0; call < calls; call++)
			sink += read() ? 1 : 0;

		watch.Stop();

		allocated = GC.GetTotalAllocatedBytes(true) - allocated;

		Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
			$"{which}: {calls} calls in {watch.Elapsed.TotalSeconds:F2} s, {watch.Elapsed.TotalMilliseconds * 1e6 / calls:F0} ns a call, " +
			$"{allocated / (double)calls:F0} B a call (sink {sink})"));
	}
}
