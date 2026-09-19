using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace DotGram.Benchmarks;

static partial class Stand
{
	/// <summary>
	/// The tape of each directory's build at a hundred terms and at the ladder row, in microseconds a call, the builds read in
	/// the same rounds (a round is about ten milliseconds of one build's one row, the builds taken in turn), so that what the
	/// machine does between one build and the next is the same for all. Unpinned: a rough figure, of use for a lean of some
	/// tens of per cent when the medians of fifteen rounds agree, and no finer.
	/// </summary>
	public static void ElRows(string[] directories)
	{
		var texts = new[]
		{
			("terms100", "(int x) => x" + string.Concat(Enumerable.Repeat(" + x", 100))),
			("ladder", "(int x, int y) => (x + y) * 3 - x / 5"),
		};

		var calls = directories.Select(directory => texts.Select(one => new PairedSide(directory, directory).El("TryParseLambda", one.Item2, immediate: false)).ToArray()).ToArray();

		var batch = new int[texts.Length];

		// Three seconds of each row of each build first, and the calls that fill about ten milliseconds from that.
		for (var t = 0; t < texts.Length; t++)
		{
			for (var d = 0; d < calls.Length; d++)
			{
				var warm = Stopwatch.StartNew();

				while (warm.Elapsed < TimeSpan.FromSeconds(3))
					calls[d][t]();
			}

			var probe = Stopwatch.StartNew();
			var n     = 0;

			while (probe.Elapsed < TimeSpan.FromMilliseconds(50))
			{
				calls[0][t]();
				n++;
			}

			batch[t] = Math.Max(1, (int)(n / 5));
		}

		var rounds = new List<double>[directories.Length, texts.Length];

		for (var d = 0; d < directories.Length; d++)
			for (var t = 0; t < texts.Length; t++)
				rounds[d, t] = [];

		for (var round = 0; round < 15; round++)
		{
			for (var t = 0; t < texts.Length; t++)
			{
				for (var k = 0; k < directories.Length; k++)
				{
					var d     = (k + round) % directories.Length;
					var watch = Stopwatch.StartNew();

					for (var i = 0; i < batch[t]; i++)
						calls[d][t]();

					rounds[d, t].Add(watch.Elapsed.TotalMilliseconds * 1000 / batch[t]);
				}
			}
		}

		for (var d = 0; d < directories.Length; d++)
		{
			var cells = new List<string>();

			for (var t = 0; t < texts.Length; t++)
			{
				var sorted = rounds[d, t].Order().ToList();

				cells.Add(string.Create(CultureInfo.InvariantCulture, $"{texts[t].Item1} {sorted[sorted.Count / 2]:F2} us ({sorted[0]:F2}..{sorted[^1]:F2})"));
			}

			Console.WriteLine($"{directories[d]} tape {string.Join(", ", cells)}");
		}
	}

	/// <summary>
	/// The tape of each directory's build at a thousand terms of the linearity chain, in microseconds a call: the median of
	/// seven rounds of a hundred calls, after three seconds of warm-up. Not pinned and not held to a control, so only a large effect is read
	/// from it; the paired stand is what says the small ones.
	/// </summary>
	public static void ElTerms(string[] directories)
	{
		var text = "(int x) => x" + string.Concat(Enumerable.Repeat(" + x", 1000));

		foreach (var directory in directories)
		{
			var side = new PairedSide(directory, directory);
			var call = side.El("TryParseLambda", text, immediate: false);

			if (call() != 1)
				throw new InvalidOperationException($"{directory}: the chain of a thousand terms is not accepted.");

			// Three seconds of calls first: the runtime promotes a method after a delay in which it sees no new one, and a
			// figure taken before that is the instrumented code's, steady as it looks.
			var warm = Stopwatch.StartNew();

			while (warm.Elapsed < TimeSpan.FromSeconds(3))
				call();

			var rounds = new List<double>();

			for (var round = 0; round < 7; round++)
			{
				var watch = Stopwatch.StartNew();

				for (var i = 0; i < 100; i++)
					call();

				rounds.Add(watch.Elapsed.TotalMilliseconds * 1000 / 100);
			}

			rounds.Sort();
			Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"{directory} tape terms1000 {rounds[rounds.Count / 2]:F0} us (rounds {rounds[0]:F0}..{rounds[^1]:F0})"));
		}
	}
}
