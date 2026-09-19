using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Handwritten.Fix;

namespace DotGram.Benchmarks;

static partial class Stand
{
	/// <summary>
	/// FIX read whole from a stream or a text reader, the array of its fields at the end (performance-ff, C4c, 2026-09-19):
	/// <c>FixParser.Parse(Stream)</c>, which the <c>.stream</c> rows read, yields a window at a time by another driver, and
	/// the whole-stream form is the one a change to the buffered machines moves. The hand parser is this process's own,
	/// read from a memory stream as the <c>.stream</c> rows read it.
	/// </summary>
	static IEnumerable<Workload> PairedWholeStreams(PairedSide before, PairedSide after)
	{
		var order = "8=FIX.4.4\u00019=65\u000135=D\u000111=ORDER\u000155=ABC\u000154=1\u000160=20260915-12:00:00\u000138=100\u000140=2\u000144=12.50\u000110=000\u0001";

		foreach (var (name, text) in new[]
		{
			("One", "55=ABC\u0001"),
			("Order", order),
			("BinaryMany", string.Concat(Enumerable.Repeat("95=3\u000196=a\u0001b\u0001", 64))),
			("Orders128", string.Concat(Enumerable.Repeat(order, 128))),
			("slope-0", FixSlopeText(0)),
			("slope-16", FixSlopeText(16)),
		})
		{
			var bytes = Encoding.Latin1.GetBytes(text);

			foreach (var (form, reader) in new[] { ("whole-stream", false), ("whole-reader", true) })
			{
				yield return PairedFixForm($"{name}.{form}",
					() => HandFixParser.Parse(new MemoryStream(bytes, false)),
					before.FixWholeCount(reader, bytes, text),
					after.FixWholeCount(reader, bytes, text));
			}
		}
	}
}
