using System.Diagnostics;
using System.Globalization;

using DotGram.Handwritten;
using DotGram.Sql.Standard;
using DotGram.Sql.TransactSql;

// Whether the nested-bracket quadratic is a property of the refusal or of reading brackets at all,
// and whether the hand-written parser has the same exponent on the same input.
//
// The exponent is read off sizes that double, by two quantities that need no quiet machine: bytes
// allocated on this thread, which is a count, and elapsed time, which here only has to tell 2x from
// 4x. A quotient that doubles as the input doubles is a square; one that stays at two is linear.
// Nothing here is a price -- it is an exponent screen, and the ratio against the hand parser stays
// where the stand measured it.
//
// Both sides run on a 64 MiB thread: the hand parser has no depth guard at all and dies at 314
// levels on the default stack, and the generated one would divert to a stack of its own, which would
// put a thread creation inside the reading being timed.
var side = args.Length > 0 ? args[0] : "generated";

var thread = new Thread(Screen, 64 * 1024 * 1024);

thread.Start();
thread.Join();

void Screen()
{
	foreach (var shape in new[] { "refused", "closed" })
	{
		Console.WriteLine();
		Console.WriteLine(side + ", " + shape + ":");
		Console.WriteLine("     n        chars        bytes   bytes/prev          ns   ns/prev  outcome");

		var lastBytes = 0.0;
		var lastTime  = 0.0;

		foreach (var n in new[] { 100, 200, 400, 800 })
		{
			// T-SQL is asked for a statement, so the brackets sit in a select list; the standard's
			// side is asked for a search condition, which is what the counts were taken through.
			var head = side == "tsql" ? "SELECT " : "";
			var body = side == "tsql" ? "1" : "a = 1";

			var text = shape == "closed"
				? head + new string('(', n) + body + new string(')', n)
				: head + new string('(', n) + body;

			// Warmed until the reading is stable: for about the first 300 ms of a process everything
			// runs several times slower (benchmarks/README.md), which would read as an exponent.
			var outcome = "";

			for (var warm = 0; warm < 3; warm++)
				outcome = Read(text);

			var before = GC.GetAllocatedBytesForCurrentThread();
			var watch  = Stopwatch.StartNew();
			var rounds = 0;

			while (watch.ElapsedMilliseconds < 200)
			{
				Read(text);
				rounds++;
			}

			watch.Stop();

			var bytes = (GC.GetAllocatedBytesForCurrentThread() - before) / (double)rounds;
			var time  = watch.Elapsed.TotalNanoseconds / rounds;

			Console.WriteLine(string.Create(
				CultureInfo.InvariantCulture,
				$"{n,6} {text.Length,12} {bytes,12:N0} {(lastBytes > 0 ? bytes / lastBytes : 0),12:N2} " +
				$"{time,11:N0} {(lastTime > 0 ? time / lastTime : 0),9:N2}  {outcome}"));

			lastBytes = bytes;
			lastTime  = time;
		}
	}
}

string Read(string text)
{
	if (side == "hand")
		return HandSqlStandard.TryParseSearchCondition(text, out _) ? "read" : "refused";

	if (side == "tsql")
		return TransactSqlParser.TryParseStatement(text).IsSuccess ? "read" : "refused";

	return SqlStandardParser.TryParseSearchCondition(text).IsSuccess ? "read" : "refused";
}
