using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Emit;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A refused address costs its length in whitespace read, counted, where it used to cost two to the
/// power of its words.
/// </summary>
/// <remarks>
/// <para>
/// <b>What it holds.</b> Three shapes, each of them exponential until 2026-09-29, and none of them
/// `(A+)*`: in each a space stood between two things that could both take it. A display name with
/// dots, <c>a . a . a &lt;b@c</c>, let the space after a dot be either the phrase's own `CFWS` or the next
/// word's leading one, two readings a gap: 41 seconds at twenty-two words. The same with a comment
/// before each word, <c>(c) a.(c) a</c>, was worse, 30 seconds at twelve. And an obsolete route,
/// <c>&lt;(a) ,(a) ,</c>, let the space after a comment be the end of that `CFWS` or a `CFWS` of its own:
/// 2.5 seconds at twenty. <see cref="WebRefusalScalingTests"/> and <see cref="WebRefusalSweepTests"/>
/// had none of them — a timed shape finds only what it was written for.
/// </para>
/// <para>
/// <b>Counted, not timed</b> (D144): every whitespace character the parser reads, which a reading
/// that re-cuts a gap reads again. The shipped grammar is read from its file and compiled here with
/// one guard added to <c>Wsp</c> that counts and always holds; everything else is the grammar and the
/// hand-written code beside it, character for character. The count is a function of the grammar and
/// the generator, so it is what the shipped parser does, without a clock or a quiet machine.
/// </para>
/// <para>
/// <b>Linear, and not merely not exponential.</b> Four sizes a doubling apart, and the count each
/// doubling adds may at most double as well. Now it doubles exactly: the dotted name read by
/// <c>TryParseList</c> is 42, 90, 186 and 378 characters at 4, 8, 16 and 32 words. Before, the second
/// rung already read 29 to 500 times what the first did (389 and 13,925; with comments 2,863 and 1,421,875;
/// the route 149 and 4,357), so the regression fails in milliseconds rather than running the last
/// rung for minutes. And the square that stood beside the exponential — a phrase or local part given
/// back a word per retry, n retries of n words — fails it too: 78, 228 and 768 with comments, each
/// doubling adding 3.6 times the one before.
/// </para>
/// </remarks>
public sealed class WebRefusalCountTests
{
	/// <summary>Four sizes, a doubling apart.</summary>
	static readonly int[] Sizes = [4, 8, 16, 32];

	/// <summary>How much more a doubling may add than the doubling before it.</summary>
	/// <remarks>Two is linear; the margin is for the ends of the text, which a short rung still feels.</remarks>
	const double Slack = 2.2;

	[Theory]
	[InlineData("a display name with dots",       "TryParseList")]
	[InlineData("a display name with dots",       "Mailbox.TryParse")]
	[InlineData("dots and a comment before words", "TryParseList")]
	[InlineData("dots and a comment before words", "Mailbox.TryParse")]
	[InlineData("an obsolete route of comments",   "TryParseList")]
	[InlineData("an obsolete route of comments",   "Mailbox.TryParse")]
	public void A_refusal_reads_its_whitespace_a_bounded_number_of_times(string shape, string reader)
	{
		var read   = Reader(reader);
		var counts = new List<long>();

		foreach (var size in Sizes)
		{
			var text = Refused(shape, size);

			Counted.GetField("Ticks")!.SetValue(null, 0L);
			Assert.False(read(text), $"{shape}: `{text}` was read, and it is meant to be refused.");
			counts.Add((long)Counted.GetField("Ticks")!.GetValue(null)!);

			// Checked at every rung and not once at the end, so that a grammar gone exponential fails
			// here in milliseconds rather than running the last rung for minutes.
			if (counts.Count >= 3)
			{
				var earlier = counts[^2] - counts[^3];
				var later   = counts[^1] - counts[^2];

				Assert.True(
					earlier > 0 && later <= earlier * Slack,
					$"{shape}, {reader}: the whitespace read at {string.Join(", ", Sizes.Take(counts.Count))} " +
					$"units was {string.Join(", ", counts)}, so a doubling added {later} where the one " +
					$"before added {earlier}. Either a space can be taken by two things standing next to " +
					$"it — seal the gap so that only one side owns it — or a run is given back an element " +
					$"a retry where nothing after it could begin with one: seal the run.");
			}
			else if (counts.Count == 2)
			{
				Assert.True(
					counts[1] <= counts[0] * 3,
					$"{shape}, {reader}: {counts[0]} whitespace characters read at {Sizes[0]} units and " +
					$"{counts[1]} at {Sizes[1]}.");
			}
		}
	}

	static string Refused(string shape, int size)
	{
		return shape switch
		{
			"a display name with dots"        => string.Join(" . ", Enumerable.Repeat("a", size)) + " <b@c",
			"dots and a comment before words" => string.Join(".", Enumerable.Repeat("(c) a", size)) + " <b@c",
			"an obsolete route of comments"   => "<" + string.Concat(Enumerable.Repeat("(a) ,", size)) + "x",
			_                                 => throw new ArgumentOutOfRangeException(nameof(shape)),
		};
	}

	/// <summary>A public reader of the counted assembly: what a caller of the package calls.</summary>
	static Func<string, bool> Reader(string reader)
	{
		var address = Counted.Assembly.GetType("DotGram.Web.EmailAddress")!;
		var owner   = reader == "Mailbox.TryParse" ? address.GetNestedType("Mailbox")! : address;
		var name    = reader == "Mailbox.TryParse" ? "TryParse" : reader;
		var method  = owner.GetMethod(name, BindingFlags.Public | BindingFlags.Static)!;

		return text => (bool)method.Invoke(null, [text, null])!;
	}

	/// <summary>The probe's counter, in an assembly holding RFC 5322's grammar and code with one guard added.</summary>
	static Type Counted => _counted.Value;

	static readonly Lazy<Type> _counted = new(Compile);

	/// <summary>What <c>Wsp</c> is, and what it becomes here.</summary>
	const string Wsp        = "Wsp  = [' ' | '\\t']";
	const string CountedWsp = "Wsp  = [' ' | '\\t'] & when @(Rfc5322Probe.Tick())";

	const string Probe =
		"namespace DotGram.Web\n" +
		"{\n" +
		"\tpublic static class Rfc5322Probe\n" +
		"\t{\n" +
		"\t\t[System.ThreadStatic] public static long Ticks;\n" +
		"\t\tpublic static bool Tick() { Ticks++; return true; }\n" +
		"\t}\n" +
		"}\n";

	static Type Compile()
	{
		var web     = Web();
		var file    = File.ReadAllText(Path.Combine(web, "Rfc5322.cs"));
		var open    = file.IndexOf("[Gram(\"\"\"", StringComparison.Ordinal) + "[Gram(\"\"\"".Length;
		var close   = file.IndexOf("\"\"\")]", open, StringComparison.Ordinal);
		var grammar = file[open..close];

		// Or the splice below does nothing and the count is zero at every size, which is flat.
		Assert.Contains(Wsp, grammar, StringComparison.Ordinal);

		var generated = GramCompiler.Compile(grammar.Replace(Wsp, CountedWsp), new GramCompilerOptions
		{
			ClassName     = "Rfc5322",
			Namespace     = "DotGram.Web",
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		Assert.DoesNotContain(generated.Diagnostics, one => one.Severity == GramSeverity.Error);

		var parse = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Latest);
		var trees = generated.Sources
			.Select(source => source.Text)
			.Concat([GramCompiler.EmitMarkerAttributes().Text, SupportEmitter.EmbeddedAttribute, file, File.ReadAllText(Path.Combine(web, "Structural.cs")), Probe])
			.Select(text => CSharpSyntaxTree.ParseText(text, parse));

		// The package's own assembly is left out: the counted copy declares every type it does.
		var references = EmittedCode.References.Where(
			reference => !string.Equals(Path.GetFileName(reference.Display), "DotGram.Web.dll", StringComparison.OrdinalIgnoreCase));

		var compilation = CSharpCompilation.Create(
			"DotGram.Tests.Rfc5322Counted",
			trees,
			references,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

		using var stream = new MemoryStream();

		var result = compilation.Emit(stream);

		Assert.True(
			result.Success,
			"The counted RFC 5322 did not compile:\n" +
			string.Join("\n", result.Diagnostics.Where(one => one.Severity == DiagnosticSeverity.Error)));

		return Assembly.Load(stream.ToArray()).GetType("DotGram.Web.Rfc5322Probe")!;
	}

	static string Web([CallerFilePath] string here = "")
	{
		return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(here)!, "..", "..", "src", "DotGram.Web"));
	}
}
