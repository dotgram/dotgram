using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

using DotGram.Examples.Feeds;
using DotGram.Handwritten.Feeds;

namespace DotGram.Benchmarks;

/// <summary>
/// The stock count read at 4, 100, 1,000 and 10,000 lines, by the hand parser and by the generated one,
/// as a string and through a reader, with none broken and with every tenth line broken: <c>stock-slope</c>.
/// Rough, in one process, for the shape of the curve — a time that grows with the square of the lines is
/// the generator's, one that grows with the lines and a large coefficient is the line's.
/// </summary>
static class StockSlope
{
	public static void Run()
	{
		Console.WriteLine("| lines | broken | form | hand µs | generated µs | generated / hand | hand ns/line | generated ns/line |");
		Console.WriteLine("| ---: | --- | --- | ---: | ---: | ---: | ---: | ---: |");

		foreach (var lines in new[] { 4, 100, 1000, 10000 })
			foreach (var broken in new[] { false, true })
			{
				var text = Text(lines, broken);

				foreach (var (form, hand, generated) in new (string, Func<bool>, Func<bool>)[]
				{
					("text",   () => HandStockCount.TryRead(text, out _, out _),
					           () => StockCountReader.TryParseCount(text).IsSuccess),
					("reader", () => HandStockCount.TryRead(new StringReader(text), out _, out _),
					           () => StockCountReader.TryParseCount(new StringReader(text)).IsSuccess),
				})
				{
					if (!hand() || !generated())
						throw new InvalidOperationException($"{lines} lines, {form}: a reading refuses the count.");

					var h = Time(hand);
					var g = Time(generated);

					Console.WriteLine(string.Create(CultureInfo.InvariantCulture,
						$"| {lines:N0} | {(broken ? "every 10th" : "none")} | {form} | {h / 1000:F1} | {g / 1000:F1} | {g / h:F1}x | {h / lines:F0} | {g / lines:F0} |"));
				}
			}
	}

	static string Text(int lines, bool broken)
	{
		return Stand.StockText(lines, broken);
	}

	/// <summary>Nanoseconds a call takes: warmed for 100 ms, then timed for at least 300 ms and 3 calls.</summary>
	static double Time(Func<bool> run)
	{
		var watch = Stopwatch.StartNew();

		while (watch.ElapsedMilliseconds < 100)
			run();

		var calls = 0;

		watch.Restart();

		do
		{
			run();
			calls++;
		}
		while (watch.ElapsedMilliseconds < 300 || calls < 3);

		return watch.Elapsed.TotalMilliseconds * 1e6 / calls;
	}
}
