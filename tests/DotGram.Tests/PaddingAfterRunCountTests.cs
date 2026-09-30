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
/// A run up to a padded delimiter, followed by padding of its own and then the delimiter's stop or
/// the end, costs a reading its length, counted: accepted, and refused.
/// </summary>
/// <remarks>
/// <para>
/// <b>What it holds.</b> <c>Text &amp; ' '* &amp; ('|' &amp; ' '* | eof)</c>, with <c>Text</c> a
/// <c>(?!Separator &amp; any)+</c> over <c>' '* &amp; '|' &amp; ' '*</c>; and the same written as a
/// statement, its trailing spaces, then <c>;</c> or the end. What follows the run begins with a
/// space, which the run's turns read too, so nothing proved the run need never give back. The
/// reader read it a turn at a time, asking the separator at every character and so reading the rest
/// of a run of spaces each time; and every give-back replayed the run. The engine kept one entry for
/// the run and gave back a character at a time, and each give-back into spaces had <c>' '*</c> read
/// them again. So the reader was quadratic where it accepted and cubic where a whole parse refused,
/// and the engine quadratic where it refused.
/// </para>
/// <para>
/// A shorter reading of the run can only fail or come to the same place
/// (<c>Determinism.NeverGivesBack</c>, the padded lead), so both now scan it once and keep no way
/// back into it.
/// </para>
/// <para>
/// <b>Counted, not timed</b> (D144), as <see cref="PaddedSeparatorCountTests"/> counts: every time a
/// reading asks its buffer for room, under <c>DOTGRAM_COUNTS</c>. Four sizes a doubling apart, and
/// the count a doubling adds may at most double as well.
/// </para>
/// </remarks>
public sealed class PaddingAfterRunCountTests
{
	/// <summary>The shape with nothing around it but a key.</summary>
	const string Items = """
		Separator = ' '* & '|' & ' '*
		Text      = (?!Separator & any)+
		Key       = ['a'..'z']+

		Item  : @string   = Key & '=' & Text & ' '* & ('|' & ' '* | eof) => @("item")
		Items : @string[] = Item*

		parse Items stream bytes
		parse Items as ReadItems stream bytes yield : @string
		""";

	/// <summary>A statement, its trailing spaces, then a semicolon or the end.</summary>
	const string Statements = """
		Semicolon = ' '* & ';'
		Statement : @string = (?!Semicolon & any)+ & ' '* & (';' | eof) => @("item")
		Items     : @string[] = Statement*

		parse Items stream bytes
		parse Items as ReadItems stream bytes yield : @string
		""";

	/// <summary>Four sizes, a doubling apart.</summary>
	static readonly int[] Sizes = [128, 256, 512, 1024];

	/// <summary>How much more a doubling may add than the doubling before it.</summary>
	const double Slack = 2.2;

	/// <summary>A few asks more at the ends of a buffer's blocks.</summary>
	const int Blocks = 16;

	public static TheoryData<string, bool, CarrierKind> Cases()
	{
		var cases = new TheoryData<string, bool, CarrierKind>();

		foreach (var grammar in new[] { nameof(Items), nameof(Statements) })
			foreach (var direct in new[] { true, false })
				foreach (var carrier in new[] { CarrierKind.Tape, CarrierKind.Immediate })
					cases.Add(grammar, direct, carrier);

		return cases;
	}

	[Theory]
	[MemberData(nameof(Cases))]
	public void A_run_before_padding_is_read_a_bounded_number_of_times(string grammar, bool direct, CarrierKind carrier)
	{
		var statements = grammar == nameof(Statements);
		var parser     = Counted(statements ? Statements : Items, direct, carrier);
		var read       = 0;

		// Accepted with the spaces inside the value, accepted with them trailing it, and refused
		// after both: the refusal is what gave back into every space.
		var shapes = statements
			? new (string Name, Func<string, string> Text, string[]? Expected)[]
			{
				("inside",   n => "a" + n + "b ; c",   ["item", "item"]),
				("trailing", n => "a" + n + "b" + n,  ["item"]),
				("refused",  n => "a" + n + "b" + n + ";;", null),
			}
			:
			[
				("inside",   n => "k=a" + n + "b | k=v", ["item", "item"]),
				("trailing", n => "k=a" + n + "b" + n,   ["item"]),
				("refused",  n => "k=a" + n + "b" + n + "|=", null),
			];

		foreach (var (shape, text, expected) in shapes)
			foreach (var (name, reading) in Readings(parser))
			{
				var counts = new List<long>();

				foreach (var size in Sizes)
				{
					Reset(parser);

					Assert.Equal(expected, reading(text(new string(' ', size))));

					counts.Add(Reset(parser));

					// Or the counter is not being written and everything below passes on nothing.
					Assert.True(counts[0] > 0, $"{shape}, {name}: nothing was counted.");

					if (counts.Count < 3)
						continue;

					var earlier = counts[^2] - counts[^3];
					var later   = counts[^1] - counts[^2];

					Assert.True(
						later <= earlier * Slack + Blocks,
						$"{grammar}, {shape}, {name}, direct: {direct}, {carrier}: the reading asked for room " +
						$"{string.Join(", ", counts)} times at {string.Join(", ", Sizes.Take(counts.Count))} " +
						$"spaces, so a doubling added {later} where the one before added {earlier}.");
				}

				read++;
			}

		// Or no reading was found and everything above passed on nothing.
		Assert.True(read >= 18, $"only {read} readings were counted.");
	}

	/// <summary>
	/// Every reading of <c>Items</c> that reads through a buffer, as a caller calls it: the values it
	/// read, or null where it refused.
	/// </summary>
	static IEnumerable<(string Name, Func<string, string[]?> Read)> Readings(Type parser)
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
				{
					// A yield refuses by throwing where it meets what it cannot read.
					try
					{
						return [.. yielded.Cast<string>()];
					}
					catch (FormatException)
					{
						return null;
					}
				}

				var type = result.GetType();

				return (bool)type.GetProperty("IsSuccess")!.GetValue(result)!
					? (string[])type.GetProperty("Value")!.GetValue(result)!
					: null;
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
	static Type Counted(string grammar, bool direct, CarrierKind carrier)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
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
