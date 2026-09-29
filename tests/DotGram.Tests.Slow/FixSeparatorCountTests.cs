using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

using DotGram.Finance.Fix;
using DotGram.Finance.Fix.Fix44;
using DotGram.Generation;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A FIX field with a long run of spaces in it costs its length, counted, under both framings: read
/// as a value, and refused and recovered past.
/// </summary>
/// <remarks>
/// <para>
/// <b>What it holds.</b> Under log framing the separator is <c>' '* &amp; '|' &amp; ' '*</c>. A value
/// asked it at every character, and at every character of a run of spaces it read the rest of the run
/// before finding no bar; a field that did not read was recovered past by trying it at every space.
/// So <c>"broken" + 4096 spaces + "x | 55=END"</c> read the run about eight million times, three
/// orders of magnitude slower than the hand-written parser, and a value with the same spaces in it
/// was as slow. The generator now reads such a separator's runs once (<c>PaddedSeparatorCountTests</c>
/// holds the shape without FIX); this holds the package's own grammar to it.
/// </para>
/// <para>
/// <b>Counted, not timed</b> (D144). The grammar is <c>FixGrammar.cs</c> as it ships, run through the
/// generator here with <c>DOTGRAM_COUNTS</c> defined, which counts every time a reading asks its
/// buffer for room: once for each character it reads. It is compiled against the package under a
/// name the package shows its internals to, since the code beside the grammar uses them. What is
/// counted is what reads through a buffer — a reader, a stream, and octets held whole — which is every
/// reading but a string's, read as a span with nothing to count.
/// </para>
/// </remarks>
public sealed class FixSeparatorCountTests
{
	/// <summary>Four sizes, a doubling apart.</summary>
	static readonly int[] Sizes = [512, 1024, 2048, 4096];

	/// <summary>How much more a doubling may add than the doubling before it.</summary>
	const double Slack = 2.2;

	/// <summary>A few asks more at the ends of a buffer's blocks, which a count that stays flat still feels.</summary>
	const int Blocks = 16;

	[Theory]
	[InlineData(false, "a value")]
	[InlineData(false, "a bad field")]
	[InlineData(true,  "a value")]
	[InlineData(true,  "a bad field")]
	public void A_run_of_spaces_is_read_a_bounded_number_of_times(bool log, string shape)
	{
		var read = 0;

		foreach (var (name, reading) in Readings(log))
		{
			var counts = new List<long>();

			foreach (var size in Sizes)
			{
				var separator = log ? " | " : "\u0001";
				var text      = shape == "a value"
					? "58=a" + new string(' ', size) + "b" + separator + "55=END"
					: "broken" + new string(' ', size) + "x" + separator + "55=END";

				Reset();

				var fields = reading(text);

				Assert.Equal(2, fields.Length);
				Assert.Equal(shape == "a value", fields[0] is not FixField.Invalid);
				Assert.Equal("END", Assert.IsType<FixField.Text>(fields[1]).Value);

				counts.Add(Reset());

				// Or the counter is not being written and everything below passes on nothing. A count that
				// stays flat is fine: an SOH field is recovered past by a search that reads a block at a time.
				Assert.True(counts[0] > 0, $"{shape}, {name}: nothing was counted.");

				if (counts.Count < 3)
					continue;

				var earlier = counts[^2] - counts[^3];
				var later   = counts[^1] - counts[^2];

				Assert.True(
					later <= earlier * Slack + Blocks,
					$"{shape}, {name}, {(log ? "log" : "SOH")} framing: the reading asked for room " +
					$"{string.Join(", ", counts)} times at {string.Join(", ", Sizes.Take(counts.Count))} " +
					$"spaces, so a doubling added {later} where the one before added {earlier}.");
			}

			read++;
		}

		// ParseFields and ReadFields, each over a reader, a stream, an array and memory.
		Assert.Equal(8, read);
	}

	/// <summary>
	/// Every reading of a framing that reads through a buffer, as <c>FixParser</c> calls it, answering
	/// the fields it read.
	/// </summary>
	static IEnumerable<(string Name, Func<string, FixField[]> Read)> Readings(bool log)
	{
		var reading = Grammar.GetNestedType("FixReading")!;

		foreach (var method in Grammar.GetMethods(BindingFlags.Public | BindingFlags.Static))
		{
			var parameters = method.GetParameters();

			if (method.Name != (log ? "ParseLogFields" : "ParseFields") && method.Name != (log ? "ReadLogFields" : "ReadFields") ||
				parameters.Length < 2 || parameters[1].ParameterType != reading)
				continue;

			var input = parameters[0].ParameterType;

			if (input != typeof(TextReader) && input != typeof(Stream) && input != typeof(byte[]) &&
				input != typeof(ReadOnlyMemory<byte>))
				continue;

			yield return ($"{method.Name}({input.Name})", text =>
			{
				var bytes = Encoding.Latin1.GetBytes(text);
				var given = input == typeof(TextReader) ? new StringReader(text)
					: input == typeof(Stream) ? new MemoryStream(bytes)
					: input == typeof(byte[]) ? bytes
					: (object)new ReadOnlyMemory<byte>(bytes);
				var context = Activator.CreateInstance(reading, [Fix44Context.Default]);
				var result  = method.Invoke(null, [given, context, .. parameters.Skip(2).Select(static _ => (object?)null)])!;

				return [.. ((IEnumerable)result).Cast<FixField>()];
			});
		}
	}

	/// <summary>What the buffers counted since the last time, set back to zero.</summary>
	static long Reset()
	{
		var total = 0L;

		foreach (var nested in Grammar.GetNestedTypes(BindingFlags.NonPublic))
			if (nested.GetField("CountEnsured", BindingFlags.NonPublic | BindingFlags.Static) is { } counter)
			{
				total += (long)counter.GetValue(null)!;
				counter.SetValue(null, 0L);
			}

		return total;
	}

	/// <summary>The counted copy of <c>FixGrammar</c>.</summary>
	static Type Grammar => _grammar.Value;

	static readonly Lazy<Type> _grammar = new(Compile);

	static Type Compile()
	{
		var parse = CSharpParseOptions.Default
			.WithLanguageVersion(LanguageVersion.Latest)
			.WithPreprocessorSymbols("DOTGRAM_COUNTS");
		var host  = CSharpSyntaxTree.ParseText(File.ReadAllText(Host()), parse, "FixGrammar.cs");

		// The package by name: what this process had loaded when the list was first taken need not
		// have included it yet.
		var package    = typeof(FixField).Assembly.Location;
		var references = EmittedCode.References
			.Where(reference => !string.Equals(reference.Display, package, StringComparison.OrdinalIgnoreCase))
			.Append(MetadataReference.CreateFromFile(package));

		// The name the package shows its internals to, which the code beside the grammar reads.
		var compilation = CSharpCompilation.Create(
			"DotGram.Finance.Tests",
			[host],
			references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

		CSharpGeneratorDriver
			.Create([new GramGenerator().AsSourceGenerator()], parseOptions: parse)
			.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var reported);

		Assert.DoesNotContain(reported, one => one.Severity == DiagnosticSeverity.Error);

		// Or the counters are not in what was generated and every count is zero, which is flat.
		Assert.Contains(output.SyntaxTrees, tree => tree.ToString().Contains("CountEnsured", StringComparison.Ordinal));

		using var stream = new MemoryStream();

		var result = output.Emit(stream);

		Assert.True(
			result.Success,
			"The counted FixGrammar did not compile:\n" +
			string.Join("\n", result.Diagnostics.Where(one => one.Severity == DiagnosticSeverity.Error)));

		return Assembly.Load(stream.ToArray()).GetType("DotGram.Finance.Fix.FixGrammar")!;
	}

	static string Host([CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "..", "src", "DotGram.Finance", "Fix", "FixGrammar.cs"));
	}
}
