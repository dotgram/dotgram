using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using BenchmarkDotNet.Attributes;

using DotGram;

namespace DotGram.Benchmarks;

/// <summary>One part of the feed: the opening line, a record, or the closing line.</summary>
public abstract record Part;

/// <summary>The line a feed opens with.</summary>
public sealed record Opening(string Source) : Part;

/// <summary>A record.</summary>
public sealed record Record(string Symbol, string Quantity) : Part;

/// <summary>The line a feed closes with.</summary>
public sealed record Closing(string Count) : Part;

[Gram("""
	@using DotGram.Benchmarks;

	Feed    : @Part[] = Header & Row* & Trailer & eof

	Header  : @Part = "H" & '|' & source: Text & eol => @(new Opening(source))
	Row     : @Part = "R" & '|' & symbol: Text & '|' & qty: Digit+ & eol
	               => @(new Record(symbol, qty))
	Trailer : @Part = "T" & '|' & count: Digit+ & eol => @(new Closing(count))

	Text    = [^ '|' | '\r' | '\n']+
	Digit   = ['0'..'9']

	parse Feed
	""")]
public static partial class Feed;

/// <summary>
/// The same feed read three ways: all in memory, from a reader, and from lines.
/// </summary>
/// <remarks>
/// <para>
/// What this is for is the cost of the window, and the window is the only thing that
/// differs — same grammar, same records, same values built. The string overload has the
/// whole input in hand and indexes into it; the other two read through a buffer of 4096
/// characters that is reused, so what they hold is one record and not the file.
/// </para>
/// <para>
/// The allocation column is the one to read. The string case has to hold the input and
/// every part at once, so it grows with the feed; the streamed cases hold one part at a
/// time, and the parts are counted rather than kept — which is what a caller who is
/// writing to a database or adding up a column actually does. A number that only says
/// which is faster would miss the point of streaming entirely.
/// </para>
/// </remarks>
[MemoryDiagnoser]
public class StreamingBenchmarks
{
	/// <summary>How many records the feed carries.</summary>
	/// <remarks>
	/// Two sizes, because the interesting number is not either of them but the shape
	/// between: memory that grows with the input against memory that does not.
	/// </remarks>
	[Params(100, 10_000)]
	public int Records { get; set; }

	string   _text  = "";
	string[] _lines = [];

	[GlobalSetup]
	public void Setup()
	{
		var text = new StringBuilder("H|ACME\n");

		for (var i = 0; i < Records; i++)
			// Varying width, which is what a real feed has — and what walks the window
			// boundary through every offset inside a record rather than parking it at one.
			text.Append("R|AAPL|").Append(i % 1000).Append('\n');

		_text  = text.Append("T|").Append(Records).Append('\n').ToString();
		_lines = _text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

		// The three doors must give the same answer, or the numbers are about three
		// different things. Checked once, outside the measurement.
		var whole = Count(Feed.ParseFeed(_text));

		if (whole != Count(Feed.ParseFeed(new StringReader(_text))) || whole != Count(Feed.ParseFeed(_lines)))
			throw new InvalidOperationException("The three overloads disagree about the same feed.");
	}

	/// <summary>
	/// What a caller does with the parts, without keeping any of them.
	/// </summary>
	/// <remarks>
	/// Adding them up rather than collecting them, because collecting them would hold the
	/// whole feed in every case and hide the difference the benchmark is about.
	/// </remarks>
	static int Count(IEnumerable<Part> parts)
	{
		var records = 0;

		foreach (var part in parts)
			if (part is Record)
				records++;

		return records;
	}

	[Benchmark(Baseline = true, Description = "string")]
	public int Whole() => Count(Feed.ParseFeed(_text));

	[Benchmark(Description = "TextReader")]
	public int Reader() => Count(Feed.ParseFeed(new StringReader(_text)));

	[Benchmark(Description = "IEnumerable<string>")]
	public int Lines() => Count(Feed.ParseFeed(_lines));
}
