using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace DotGram.Benchmarks;

static partial class Stand
{
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
