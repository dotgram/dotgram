using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A separator that is a stop between runs of padding costs a reading its length, counted: in a
/// value that runs up to it, and in a bad element recovered past it.
/// </summary>
/// <remarks>
/// <para>
/// <b>What it holds.</b> <c>' '* &amp; '|' &amp; ' '*</c> is a FIX log's separator, and the shape is
/// FIX's with nothing of FIX left: a value that runs up to the separator, <c>(?!Separator &amp;
/// any)+</c>, and a repetition that recovers past it. The engine has read both in one pass since it
/// learned the delimiter scan; the reader, which the direct readings are, did not. It asked the
/// separator at every character of the value, and at every character of a run of spaces the
/// separator read the rest of the run before it found no stop; and it searched for a bad element's
/// end the same way, trying the separator at every space. A run of n spaces was n²/2 characters
/// read either way: 4096 of them took 29 ms over a string and 180 over a stream, against about 18
/// and 100 microseconds now.
/// </para>
/// <para>
/// <b>Counted, not timed</b> (D144): every time a reading asks its buffer for room, which it does for
/// each character it reads, under <c>DOTGRAM_COUNTS</c>. So what is counted is what reads through a
/// buffer — a reader, a stream, and bytes held whole, which the reader reads over a buffer of its
/// own. A string is read as a span, where nothing can count; its reading is the same rendering as
/// the bytes' over other helpers, and is the one row this does not hold.
/// </para>
/// <para>
/// <b>Linear.</b> Four sizes a doubling apart, and the count a doubling adds may at most double as
/// well. Before, the second rung already read four times the first.
/// </para>
/// </remarks>
public sealed class PaddedSeparatorCountTests
{
	const string Grammar = """
		Separator = ' '* & '|' & ' '*
		Text      = (?!Separator & any)+
		Key       = ['a'..'z']+

		Item  : @string   = Key & '=' & Text & (Separator | eof) => @("item")
		Items : @string[] = Item* recover Separator => @("bad")

		parse Items stream bytes
		parse Items as ReadItems stream bytes yield : @string
		""";

	/// <summary>Four sizes, a doubling apart.</summary>
	static readonly int[] Sizes = [512, 1024, 2048, 4096];

	/// <summary>How much more a doubling may add than the doubling before it.</summary>
	/// <remarks>Two is linear; the margin is for what a short rung reads beside the run.</remarks>
	const double Slack = 2.2;

	/// <summary>A few asks more at the ends of a buffer's blocks, which a count that stays flat still feels.</summary>
	const int Blocks = 16;

	[Theory]
	[InlineData(true,  CarrierKind.Tape)]
	[InlineData(true,  CarrierKind.Immediate)]
	[InlineData(false, CarrierKind.Tape)]
	[InlineData(false, CarrierKind.Immediate)]
	public void A_run_of_padding_is_read_a_bounded_number_of_times(bool direct, CarrierKind carrier)
	{
		var parser = Counted(direct, carrier);
		var read   = 0;

		foreach (var shape in new[] { "a value", "a bad element" })
			foreach (var (name, reading) in Readings(parser))
			{
				var counts = new List<long>();

				foreach (var size in Sizes)
				{
					var text = shape == "a value"
						? "k=a" + new string(' ', size) + "b | k=v"
						: "bad" + new string(' ', size) + "x | k=v";

					Reset(parser);

					string[] expected = shape == "a value" ? ["item", "item"] : ["bad", "item"];

					Assert.Equal(expected, reading(text));

					counts.Add(Reset(parser));

					// Or the counter is not being written and everything below passes on nothing.
					Assert.True(counts[0] > 0, $"{shape}, {name}: nothing was counted.");

					if (counts.Count < 3)
						continue;

					var earlier = counts[^2] - counts[^3];
					var later   = counts[^1] - counts[^2];

					Assert.True(
						later <= earlier * Slack + Blocks,
						$"{shape}, {name}, direct: {direct}, {carrier}: the reading asked for room " +
						$"{string.Join(", ", counts)} times at {string.Join(", ", Sizes.Take(counts.Count))} " +
						$"spaces, so a doubling added {later} where the one before added {earlier}.");
				}

				read++;
			}

		// Or no reading was found and everything above passed on nothing.
		Assert.True(read >= 8, $"only {read} readings were counted.");
	}

	/// <summary>
	/// Every reading of <c>Items</c> that reads through a buffer, as a caller calls it, answering the
	/// values it read.
	/// </summary>
	static IEnumerable<(string Name, Func<string, string[]> Read)> Readings(Type parser)
	{
		foreach (var method in parser.GetMethods(BindingFlags.Public | BindingFlags.Static))
		{
			var parameters = method.GetParameters();

			if (method.Name is not ("TryParseItems" or "ReadItems") || parameters.Length == 0)
				continue;

			var input = parameters[0].ParameterType;

			if (input != typeof(TextReader) && input != typeof(Stream) && input != typeof(byte[]))
				continue;

			// The overloads that read a part of their input say where it is; these read all of it.
			if (parameters.Skip(1).Any(one => !one.ParameterType.IsGenericType ||
				one.ParameterType.GetGenericTypeDefinition() != typeof(Nullable<>)))
				continue;

			yield return ($"{method.Name}({input.Name})", text =>
			{
				var bytes = Encoding.Latin1.GetBytes(text);
				var given = input == typeof(TextReader) ? new StringReader(text)
					: input == typeof(Stream) ? new MemoryStream(bytes)
					: (object)bytes;
				var result = method.Invoke(null, [given, .. parameters.Skip(1).Select(static _ => (object?)null)])!;

				if (result is IEnumerable yielded and not string)
					return [.. yielded.Cast<string>()];

				var type = result.GetType();

				Assert.True((bool)type.GetProperty("IsSuccess")!.GetValue(result)!, $"{method.Name}({input.Name}) refused `{text}`.");

				return (string[])type.GetProperty("Value")!.GetValue(result)!;
			});
		}
	}

	/// <summary>What the buffers counted since the last time, set back to zero.</summary>
	static long Reset(Type parser)
	{
		var total = 0L;

		foreach (var nested in parser.GetNestedTypes(BindingFlags.NonPublic))
			if (nested.GetField("CountEnsured", BindingFlags.NonPublic | BindingFlags.Static) is { } counter)
			{
				total += (long)counter.GetValue(null)!;
				counter.SetValue(null, 0L);
			}

		return total;
	}

	/// <summary>The grammar compiled as a consumer would get it, with the counters turned on.</summary>
	static Type Counted(bool direct, CarrierKind carrier)
	{
		var result = GramCompiler.Compile(Grammar, new GramCompilerOptions
		{
			ClassName     = "Counted",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Direct        = direct,
			Carrier       = carrier,
			BufferedInput = true,
			BufferedBytes = true,
		});

		EmittedCode.Quiet(result.Diagnostics);

		var source = Assert.Single(result.Sources).Text;

		Assert.Contains("#if DOTGRAM_COUNTS", source, StringComparison.Ordinal);

		return EmittedCode.Compile(source, "Counted", symbols: ["DOTGRAM_COUNTS"]).GetType("Counted")!;
	}
}
