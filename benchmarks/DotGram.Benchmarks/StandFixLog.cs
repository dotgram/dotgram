using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Handwritten.Fix;

namespace DotGram.Benchmarks;

static partial class Stand
{
	/// <summary>
	/// The log form of FIX (<c>FixParser.ParseLog</c>): the same messages with `|` for the separator, in the three forms that read them.
	/// The hand-written parser's string form is the constant for all three; the answer of each reading is the sum of the tags.
	/// </summary>
	static IEnumerable<Workload> PairedFixLog(PairedSide before, PairedSide after)
	{
		var order = "8=FIX.4.4|9=65|35=D|11=ORDER|55=ABC|54=1|60=20260915-12:00:00|38=100|40=2|44=12.50|10=000|";

		foreach (var (name, text) in new[]
		{
			("One", "55=ABC|"),
			("Order", order),
			("Orders128", string.Concat(Enumerable.Repeat(order, 128))),
			("slope-0", ""),
			("slope-4", string.Concat(Enumerable.Repeat("55=ABC|", 4))),
			("slope-16", string.Concat(Enumerable.Repeat("55=ABC|", 16))),
		})
		{
			foreach (var form in new[] { "text", "bytes", "stream" })
			{
				yield return PairedFixForm($"{name}.log-{form}",
					() => HandFixParser.ParseLog(text),
					before.FixLog(form, text),
					after.FixLog(form, text));
			}
		}
	}
}
