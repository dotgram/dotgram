using System;

namespace DotGram.Benchmarks;

/// <summary>
/// What one parse allocates, in bytes, said exactly rather than averaged.
/// </summary>
/// <remarks>
/// <para>
/// <c>MemoryDiagnoser</c> gives a figure per operation and rounds it; this asks the runtime
/// what the thread allocated between two points and divides by how many parses happened
/// between them. The answer is exact, and exact is what is wanted of a number that ought to
/// be nothing but the result.
/// </para>
/// <para>
/// The scenarios are chosen to take the answer apart. A URL that parses allocates its value,
/// the nested value inside it, and one string for each part it kept. A URL that does not
/// parse allocates nothing at all, which is the machinery saying it has nothing of its own.
/// A recognition without a value says the same from the other side.
/// </para>
/// </remarks>
static class Allocation
{
	public static void Report()
	{
		Console.WriteLine("bytes  parse");
		Console.WriteLine("-----  -----");

		Measure("url, whole value", "http://example.com",
			text => Urls.TryParseUrl(text).IsSuccess);

		Measure("url, every part", "https://user@example.com:8080/a/b/c?q=1&r=2#top",
			text => Urls.TryParseUrl(text).IsSuccess);

		Measure("url, host and path", "https://example.com/a/b/c",
			text => Urls.TryParseUrl(text).IsSuccess);

		Measure("url, no match", "https://exa mple.com/",
			text => Urls.TryParseUrl(text).IsSuccess);

		Measure("forty letters, one string", new string('x', 40),
			text => CallCost.Called.ParseStart(text) is not null);

		Measure("forty letters, parser not kept", new string('x', 40),
			text => CallCost.Unpooled.ParseStart(text) is not null);

		// A string of n characters is 22 + 2n bytes, rounded up to eight. Asking twice, at
		// two lengths, says whether what comes back is the one string it looks like or two.
		Measure("a hundred letters, one string", new string('x', 100),
			text => CallCost.Called.ParseStart(text) is not null);
	}

	static void Measure(string what, string text, Func<string, bool> parse)
	{
		// Warm: the first parse of a thread builds the parser it will then keep, and the
		// first call of a method jits it. Neither is what a parse costs.
		for (var i = 0; i < 200; i++)
			parse(text);

		const int Runs = 1000;

		var before = GC.GetAllocatedBytesForCurrentThread();

		for (var i = 0; i < Runs; i++)
			parse(text);

		var after = GC.GetAllocatedBytesForCurrentThread();

		Console.WriteLine($"{(after - before) / (double)Runs,5:0.0}  {what}");
	}
}
