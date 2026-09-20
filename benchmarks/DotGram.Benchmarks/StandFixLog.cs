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

			// A bad wire for the log form (performance-ff and the architect, 617c68e6): the same Order with the tag of one field malformed, as fix/OrderMalformed
			// is for the plain forms, so that a carrier which builds while it reads and then gives the reading up is seen on the log forms as well.
			("OrderMalformed", order.Replace("|40=2|", "|40X=2|")),
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
