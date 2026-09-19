using System;
using System.Collections.Generic;

namespace DotGram.Benchmarks;

static partial class Stand
{
	/// <summary>
	/// The document grammar (Documents.cs), read at each side's own build of it. It has no hand-written
	/// parser, so "hand" is this process's own <c>Config.Read</c>: the constant a pair is held against,
	/// not a reference for the generated code.
	/// </summary>
	static IEnumerable<Workload> PairedConfig(PairedSide before, PairedSide after)
	{
		foreach (var (name, text) in new[] { ("dense", Documents.Dense), ("spaced", Documents.Spaced), ("commented", Documents.Commented) })
		{
			yield return new Workload("config", name,
				[
					new Reading("hand",   () => Config.Read(text).Length),
					new Reading("before", before.ConfigRead(text)),
					new Reading("after",  after.ConfigRead(text)),
				],
				() =>
				{
					var h = Config.Read(text).Length;
					var b = before.ConfigRead(text)();
					var a = after.ConfigRead(text)();

					return h == 400 && b == 400 && a == 400 ? null : $"  hand {h}, before {b}, after {a}, and the document has 400 settings";
				});
		}
	}
}
