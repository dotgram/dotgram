using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Examples.Feeds;

namespace DotGram.Benchmarks;

static partial class Stand
{
	/// <summary>
	/// The recovering feed (RecoveringFeedExample.cs), of two builds against each other: a thousand rows, every tenth of
	/// them one it rejects in the second input. It has no hand-written parser, so "hand" is this process's own
	/// <c>RecoveringFeedReader.Read</c>, the constant a pair is held against.
	/// </summary>
	static IEnumerable<Workload> PairedRecovering(PairedSide before, PairedSide after)
	{
		foreach (var (name, broken) in new[] { ("good", false), ("broken", true) })
		{
			var text = "H|2026-08-13|ACME\n"
				+ string.Concat(Enumerable.Range(0, 1000).Select(i => broken && i % 10 == 9 ? "R|MSFT|two hundred|2026-08-12\n" : "R|AAPL|100|2026-08-12\n"))
				+ "T|1000\n";
			var b = before.RecoveringFeed(text);
			var a = after.RecoveringFeed(text);

			yield return new Workload("feeds", $"recovering.{name}",
				[
					new Reading("control", () => RecoveringFeedReader.Read(text).Count),
					new Reading("before", b),
					new Reading("after",  a),
				],
				() => RecoveringFeedReader.Read(text).Count == 1000 && b() == 1000 && a() == 1000
					? null
					: $"  the feed has 1000 lines: this build {RecoveringFeedReader.Read(text).Count}, before {b()}, after {a()}");
		}
	}
}
