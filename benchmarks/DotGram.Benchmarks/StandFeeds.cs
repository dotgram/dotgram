using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using DotGram.Examples.Feeds;
using DotGram.Handwritten.Feeds;

namespace DotGram.Benchmarks;

// feeds/stock-count (finance-24 for performance-ff's C2, 2026-09-18): the stock count of
// examples/DotGram.Examples/Feeds, read by the example's generated parser and by
// HandStockCount, which is the base. A count is a line to an item and a closing line that says
// how many there were; a broken line costs itself and not the file.
//
// Three forms of each input: the string, and a TextReader at the default buffer (4096) and at
// 64 characters, where a line crosses a refill and the reader's own bookkeeping is what is
// timed. Three inputs: the four-line count of the example's header, a thousand good lines, and
// the same with every tenth line broken (the closing line counting the good ones).
//
// The stand references DotGram.Examples for this, unlike the URL benchmark, which copies a
// grammar: a copy of an example is a second parser to keep in step with the shipped one, and
// the hand parser of this row references the same example for the same value types.

static partial class Stand
{
	/// <summary>The three inputs of the stock count: the example's four lines, a thousand good ones, and the same with every tenth broken.</summary>
	static (string Name, string Text)[] StockInputs() =>
	[
		("small",  "apples: 12\npears: 7\nplums seven\nEND 3\n"),
		("good",   string.Concat(Enumerable.Range(0, 1000).Select(static i => $"item{i}: {i % 100}\n")) + "END 1000\n"),
		("broken", string.Concat(Enumerable.Range(0, 1000).Select(static i => i % 10 == 9 ? "x 1\n" : $"item{i}: {i % 100}\n")) + "END 900\n"),
	];

	static IEnumerable<Workload> FeedWorkloads() => StockInputs().SelectMany(static input => StockCountRows(input.Name, input.Text));

	/// <summary>
	/// The stock count of two builds against each other, with this tree's hand parser as the control:
	/// the same rows as <see cref="FeedWorkloads"/>, the generated parsers loaded from each side.
	/// </summary>
	static IEnumerable<Workload> PairedFeeds(PairedSide before, PairedSide after)
	{
		foreach (var (name, text) in StockInputs())
		{
			yield return PairedStockRow($"stock-count.{name}.text",
				() => HandStockCount.TryRead(text, out var count, out _) ? count : null,
				before.StockText(text), after.StockText(text));

			yield return PairedStockRow($"stock-count.{name}.reader",
				() => HandStockCount.TryRead(new StringReader(text), out var count, out _) ? count : null,
				before.StockReader(text, null), after.StockReader(text, null));

			yield return PairedStockRow($"stock-count.{name}.reader64",
				() => HandStockCount.TryRead(new StringReader(text), out var count, out _, 64) ? count : null,
				before.StockReader(text, 64), after.StockReader(text, 64));
		}
	}

	static Workload PairedStockRow(string name, Func<StockCount?> hand, Func<object> before, Func<object> after)
	{
		return new Workload(
			"feeds",
			name,
			[
				new Reading("hand",   () => hand() is null ? 0 : 1),
				new Reading("before", () => PairedSide.StockSummary(before()).Ok ? 1 : 0),
				new Reading("after",  () => PairedSide.StockSummary(after()).Ok ? 1 : 0),
			],
			() =>
			{
				var control = hand();
				var b       = PairedSide.StockSummary(before());
				var a       = PairedSide.StockSummary(after());

				if (control is null || !b.Ok || !a.Ok)
					return $"  every side must read it: hand {(control is null ? "refuses" : "reads")}, before {(b.Ok ? "reads" : "refuses")}, after {(a.Ok ? "reads" : "refuses")}";

				return b == (true, control.Total, control.Lines.Count) && a == b
					? null
					: $"  the sides read different counts: hand {control.Total} over {control.Lines.Count} lines, before {b.Total} over {b.Lines}, after {a.Total} over {a.Lines}";
			});
	}

	static IEnumerable<Workload> StockCountRows(string name, string text)
	{
		yield return StockCountRow($"stock-count.{name}.text",
			() => StockCountReader.TryParseCount(text),
			() => HandStockCount.TryRead(text, out var count, out _) ? count : null);

		yield return StockCountRow($"stock-count.{name}.reader",
			() => StockCountReader.TryParseCount(new StringReader(text)),
			() => HandStockCount.TryRead(new StringReader(text), out var count, out _) ? count : null);

		yield return StockCountRow($"stock-count.{name}.reader64",
			() => StockCountReader.TryParseCount(new StringReader(text), 64),
			() => HandStockCount.TryRead(new StringReader(text), out var count, out _, 64) ? count : null);
	}

	static Workload StockCountRow(string name, Func<StockCountReader.Match<StockCount>> generated, Func<StockCount?> hand)
	{
		return new Workload(
			"feeds",
			name,
			[
				new Reading("hand",      () => Weigh(hand())),
				new Reading("generated", () => generated() is { IsSuccess: true } match ? Weigh(match.Value) : 0),
			],
			() =>
			{
				var byHand = hand();
				var result = generated();

				if (byHand is null || !result.IsSuccess)
					return $"  hand {(byHand is null ? "refuses" : "reads")}, generated {(result.IsSuccess ? "reads" : "refuses")}";

				var value = result.Value!;

				if (byHand.Total != value.Total || !byHand.Lines.SequenceEqual(value.Lines))
					return $"  hand read total {byHand.Total} over {byHand.Lines.Count} lines, generated total {value.Total} over {value.Lines.Count} lines";

				return null;
			});

		static int Weigh(StockCount? count) => count is null ? 0 : count.Lines.Count + count.Total;
	}
}
