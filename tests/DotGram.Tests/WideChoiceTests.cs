using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A choice of many alternatives, each led by a literal of its own, at every width from one to
/// thirty-two, on every carrier and every way of handing the input over.
/// </summary>
/// <remarks>
/// <para>
/// Wide enough, such a choice is decided by a prefix table, whose miss path was written for the
/// engine: it read the engine's lookahead count and the tie list of the engine's failure, which a
/// tape or immediate reader declares only when something else of the grammar needs them. Sixteen
/// one-character alternatives on the tape generated code that did not compile.
/// </para>
/// <para>
/// The value oracle is the alternative's own number; acceptance is held against
/// <see cref="ReferenceInterpreter"/> on the same choice without values.
/// </para>
/// </remarks>
public sealed class WideChoiceTests
{
	const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEF";

	public static TheoryData<string, int> Widths()
	{
		var data = new TheoryData<string, int>();

		// Every width for one character each, the shape of the report, and for literals that
		// share their first characters; the other shapes at the widths either side of where a
		// table begins, and the widest.
		foreach (var shape in new[] { "single", "shared" })
			for (var width = 1; width <= 32; width++)
				data.Add(shape, width);

		foreach (var shape in new[] { "double", "class", "sequence" })
			foreach (var width in new[] { 1, 2, 3, 4, 5, 16, 32 })
				data.Add(shape, width);

		return data;
	}

	/// <summary>Alternative <paramref name="i"/>'s body, without its value.</summary>
	static string Alternative(string shape, int i)
	{
		var one = Alphabet[i];

		return shape switch
		{
			// One character each: the shape of the report.
			"single"   => $"'{one}'",
			// Two characters each, no two sharing a first.
			"double"   => $"\"{one}{char.ToUpperInvariant(one)}\"",
			// Two characters each, four sharing every first: a table more than one row deep.
			"shared"   => $"\"{Alphabet[i / 4]}{i % 4}\"",
			// A class of one character each, which no prefix table takes.
			"class"    => $"['{one}' | '{char.ToUpperInvariant(one)}']",
			// A literal followed by more reading, so a miss is decided by the table and a hit
			// still has a way to fail after it.
			"sequence" => $"'{one}' & ['0'..'9']*",
			_          => throw new ArgumentOutOfRangeException(nameof(shape)),
		};
	}

	/// <summary>
	/// The choice twice: <c>Start</c> with each alternative's number as its value, and
	/// <c>Plain</c>, the same choice building nothing.
	/// </summary>
	static string Grammar(string shape, int width)
	{
		var alternatives = Enumerable.Range(0, width).Select(i => Alternative(shape, i)).ToArray();
		var valued       = alternatives.Select(static (alternative, i) => $"{alternative} => @({i + 1})");

		return $"""
			Start : @int = {string.Join("\n\t| ", valued)}
			Plain = {string.Join("\n\t| ", alternatives)}
			parse Start stream bytes
			parse Plain
			""";
	}

	/// <summary>Every alternative's first reading, and the inputs around them that it must refuse or stop short of.</summary>
	static IEnumerable<string> Inputs(string shape, int width)
	{
		var found = new List<string> { "", "~", "0", "a~", "zz", "A0", "a0a" };

		for (var i = 0; i < width; i++)
		{
			var one = Alphabet[i].ToString();

			found.Add(one);
			found.Add(one + char.ToUpperInvariant(Alphabet[i]));
			found.Add(one + "7");
			found.Add(one + "12");
			found.Add(Alphabet[i / 4] + (i % 4).ToString());
		}

		found.Add(Alphabet[Math.Min(width, Alphabet.Length - 1)].ToString());

		return found.Distinct();
	}

	[Theory]
	[MemberData(nameof(Widths))]
	public void A_wide_choice_reads_as_the_semantics_say(string shape, int width)
	{
		var grammar = Grammar(shape, width);
		var graph   = Normalized(grammar);
		var plain   = graph.Rules.First(static rule => rule.Name == "Plain");

		foreach (var carrier in new[] { "engine", "tape", "immediate" })
		{
			var host = Compile(grammar, carrier);

			foreach (var input in Inputs(shape, width))
			{
				var context  = $"{shape} x{width}, {carrier}, input \"{input}\"";
				var accepted = ReferenceInterpreter.Parses(graph, plain, input);
				var expected = accepted ? Number(shape, input) : (int?)null;

				Assert.True(accepted == Accepts(host, input), context + ": the recognizer and the semantics disagree");

				Same(expected, Text(host, input), context + ", text");
				Same(expected, Streamed(host, input, bytes: false), context + ", chars");
				Same(expected, Streamed(host, input, bytes: true), context + ", bytes");

				// Begun at the start and not held to the end: the alternative the first reading took.
				// A publication lowered to a flat reading has no such entry (§6.3).
				var end = ReferenceInterpreter.Reads(graph, plain, input, 0);

				if (Positional(host))
					Same(end < 0 ? null : Number(shape, input.Substring(0, end)), Positioned(host, input), context + ", positioned");
			}
		}
	}

	/// <summary>The number of the first alternative that reads <paramref name="input"/> whole.</summary>
	static int Number(string shape, string input)
	{
		return shape switch
		{
			"shared" => Alphabet.IndexOf(input[0]) * 4 + (input[1] - '0') + 1,
			// A class holds the letter and its capital, so a capital is held by the alternative of
			// its small letter, which comes first, and by its own.
			"class"  => Alphabet.IndexOf(char.ToLowerInvariant(input[0])) is var small and >= 0 ? small + 1 : Alphabet.IndexOf(input[0]) + 1,
			_        => Alphabet.IndexOf(input[0]) + 1,
		};
	}

	static void Same(int? expected, int? actual, string context)
	{
		Assert.True(
			expected == actual,
			$"{context}: expected {expected?.ToString() ?? "a rejection"}, read {actual?.ToString() ?? "a rejection"}");
	}

	static Type Compile(string grammar, string carrier)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Direct        = carrier != "engine",
			Carrier       = carrier switch { "tape" => CarrierKind.Tape, "immediate" => CarrierKind.Immediate, _ => CarrierKind.Auto },
			BufferedInput = true,
			BufferedBytes = true,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		Assert.DoesNotContain(
			result.Diagnostics,
			static one => one.Severity != GramSeverity.Info && one.Id != GramCompiler.CarrierForced);

		return EmittedCode.Compile(
			result.Sources[0].Text,
			sourceParts: result.Sources.Skip(1).Select(static source => source.Text)).GetType("Grammar")!;
	}

	static bool Accepts(Type host, string input)
	{
		var match = host.GetMethod("TryParsePlain", [typeof(string)])!.Invoke(null, [input])!;

		return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!;
	}

	static int? Text(Type host, string input)
	{
		var match = host.GetMethod("TryParseStart", [typeof(string)])!.Invoke(null, [input])!;

		return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!
			? (int)match.GetType().GetProperty("Value")!.GetValue(match)!
			: null;
	}

	/// <summary>The same through a reader or a stream, a character or a byte at a time.</summary>
	static int? Streamed(Type host, string input, bool bytes)
	{
		using var reader = new OneCharReader(input);
		using var stream = new OneByteStream(Encoding.ASCII.GetBytes(input));

		var method = host.GetMethod("TryParseStart", [bytes ? typeof(Stream) : typeof(TextReader), typeof(int?), typeof(int?)]);

		Assert.NotNull(method);

		var match = method.Invoke(null, [bytes ? stream : reader, 1, 1024])!;

		return (bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!
			? (int)match.GetType().GetProperty("Value")!.GetValue(match)!
			: null;
	}

	static bool Positional(Type host)
	{
		return host.GetMethod("TryParseStart", [typeof(string), typeof(int)]) is not null;
	}

	static int? Positioned(Type host, string input)
	{
		var (read, value, _) = EmittedCode.Answered(host.Assembly, "Grammar", "TryParseStart", input, 0);

		return read ? (int)value! : null;
	}

	static RecognitionGraph Normalized(string text)
	{
		return GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(GramLexer.Tokenize(text, RoslynCSharpScanner.Instance)).File));
	}

	sealed class OneCharReader(string input) : StringReader(input)
	{
		public override int Read(char[] buffer, int index, int count)
		{
			return base.Read(buffer, index, Math.Min(1, count));
		}
	}

	sealed class OneByteStream(byte[] bytes) : MemoryStream(bytes)
	{
		public override int Read(byte[] buffer, int offset, int count)
		{
			return base.Read(buffer, offset, Math.Min(1, count));
		}

		public override int Read(Span<byte> buffer)
		{
			return base.Read(buffer[..Math.Min(1, buffer.Length)]);
		}
	}
}
