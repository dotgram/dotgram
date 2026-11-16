using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Language;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What a generated parser says about input it refuses, held against the record of what it
/// said before (<see cref="RefusalCorpus"/>), and what reading a refusal twice costs.
/// </summary>
public sealed class RefusalTests
{
	/// <summary>One reading in five of the record, a different rendering for each shape.</summary>
	/// <remarks>The whole record is held by <c>DotGram.Tests.Slow</c>, before any change to how failures are recorded.</remarks>
	[Fact]
	public void Every_refusal_in_a_sample_is_the_one_it_was()
	{
		RefusalCorpus.AssertSample();
	}

	/// <summary>
	/// A loop that ends where its turn cannot begin, and the end refused after it: every
	/// rendering says what the turn wanted and what the end did, where the engine says it. The
	/// reader, which does not try a turn it can see will not begin, used to record nothing there
	/// and answer that the input did not match, where the input began.
	/// </summary>
	[Theory]
	[InlineData("[b;;", 3)]
	[InlineData("[ b ; ;", 6)]
	[InlineData("[b;x", 3)]
	public void A_loop_before_the_end_is_refused_where_the_engine_refuses_it(string input, int at)
	{
		const string Grammar =
			"trivia = ' '*\nItem : @string = t: 'b' => @(t)\nStart : @string[] = '[' & (Item & ';')* & eof\nparse Start\n";

		foreach (var (carrier, direct) in new[] { (CarrierKind.Deferred, false), (CarrierKind.Deferred, true), (CarrierKind.Immediate, true) })
		{
			var compilation = GramCompiler.Compile(Grammar, new GramCompilerOptions
			{
				ClassName = "Grammar", Carrier = carrier, Direct = direct, CSharpScanner = RoslynCSharpScanner.Instance,
			});
			EmittedCode.Quiet(compilation.Diagnostics);

			var match = EmittedCode.Match(EmittedCode.Compile(Assert.Single(compilation.Sources).Text), "Grammar", "TryParseStart", input);

			Assert.False(match.IsSuccess);
			Assert.Equal(at, match.Position);
			Assert.Equal("Expected 'b' or end of input.", match.Error);
		}
	}

	/// <summary>
	/// A guard that says no at the furthest place, where nothing else is named, is what the
	/// refusal says — in every rendering, over a string and over a reader — and where something
	/// else is named there, that is what would have fit and the guard is not mentioned.
	/// </summary>
	/// <remarks>
	/// The guard used to be recorded naming nothing. At the end of a line that was all there, its
	/// last field refused by a <c>when</c>, the message was the one for nothing named at the end:
	/// "Expected more input.", as though the line had been cut short; anywhere else it was
	/// "Input does not match", which says nothing either.
	/// </remarks>
	[Theory]
	[InlineData("",                                                                       "when @(Long(value)) said no.")]
	[InlineData("| key: ['a'..'z']+ & '=' & '\"' & [^ '\"']* & '\"' & ['0'..'9'] => @(key)", "Expected ['0'..'9'].")]
	public void A_guard_that_said_no_is_what_the_refusal_says_where_nothing_else_is_named(string sibling, string error)
	{
		var grammar = $$"""
			Line : @string = key: ['a'..'z']+ & '=' & '"' & value: [^ '"']* & '"' & when @(Long(value)) => @(key)
			                 {{sibling}}
			parse Line
			parse Line as Buffered stream
			""";

		foreach (var (carrier, direct) in new[] { (CarrierKind.Deferred, false), (CarrierKind.Deferred, true), (CarrierKind.Immediate, true) })
		{
			var compilation = GramCompiler.Compile(grammar, new GramCompilerOptions
			{
				ClassName = "Grammar", Carrier = carrier, Direct = direct, CSharpScanner = RoslynCSharpScanner.Instance,
			});

			EmittedCode.Quiet(compilation.Diagnostics);

			// The whole line is there; the guard refuses its value at the end of it.
			const string Input = "k=\"ab\"";

			var assembly = EmittedCode.Compile(
				Assert.Single(compilation.Sources).Text,
				declarationMembers: "static bool Long(string value) { return value.Length > 2; }");
			var match    = EmittedCode.Match(assembly, "Grammar", "TryParseLine", Input);

			Assert.False(match.IsSuccess);
			Assert.Equal(Input.Length, match.Position);
			Assert.Equal(error, match.Error);

			var buffered = assembly.GetType("Grammar")!
				.GetMethod("TryBuffered", [typeof(TextReader), typeof(int?), typeof(int?)])!
				.Invoke(null, [new StringReader(Input), null, null])!;

			Assert.Equal(error, (string?)buffered.GetType().GetProperty("Error")!.GetValue(buffered));
		}
	}

	/// <summary>
	/// Guards that said no never push a named terminal out of what a refusal says, however many
	/// times they are asked at the furthest place.
	/// </summary>
	/// <remarks>
	/// The sets that tie at a place are kept up to a bound, and a guard is recorded there like a
	/// terminal. Fifteen choices of two guards that pass, before two that refuse, ask the refusing
	/// pair once for each of 2^15 ways: recorded each time, they filled the list before <c>'z'</c>
	/// was reached, and the refusal named the guards alone where it used to name <c>'z'</c>.
	/// </remarks>
	[Fact]
	public void Guards_asked_again_and_again_do_not_push_a_terminal_out()
	{
		var grammar =
			"Start = " + string.Concat(Enumerable.Repeat("(when @(true) | when @(1 == 1)) & ", 15)) +
			"(when @(false) | when @(1 == 2)) | 'z'\nparse Start\n";

		foreach (var error in Errors(grammar, ""))
			Assert.Equal("Expected 'z'.", error);
	}

	/// <summary>
	/// A guard inside the trivia that says no where the input ran out is said the same by every
	/// rendering: the engine too records it as a terminal of the trivia is recorded there.
	/// </summary>
	[Fact]
	public void A_guard_in_the_trivia_is_said_alike_in_every_rendering()
	{
		const string Grammar = "trivia = { (' ' & when @(false))* }\nStart = 'a'\nparse Start\n";

		foreach (var error in Errors(Grammar, "a "))
			Assert.Equal("when @(false) said no.", error);
	}

	/// <summary>
	/// A guard is named by its C# as written: whitespace between its tokens is said once, and what
	/// a string or a character literal holds is left as it is.
	/// </summary>
	[Fact]
	public void A_guard_is_named_by_its_CSharp_and_its_literals_are_left_whole()
	{
		const string Grammar = "Start = t: 'x' & when @(t  ==\n  \"a  b\" || t == \"\\\"  \" || t == @\"a  \"\"  b\" || t[0] == ' ')\nparse Start\n";

		foreach (var error in Errors(Grammar, "x"))
			Assert.Equal("when @(t == \"a  b\" || t == \"\\\"  \" || t == @\"a  \"\"  b\" || t[0] == ' ') said no.", error);
	}

	/// <summary>What <c>TryParseStart</c> says of <paramref name="input"/> on the engine and on the methods of either carrier.</summary>
	static IEnumerable<string?> Errors(string grammar, string input)
	{
		foreach (var (carrier, direct) in new[] { (CarrierKind.Deferred, false), (CarrierKind.Deferred, true), (CarrierKind.Immediate, true) })
		{
			var compilation = GramCompiler.Compile(grammar, new GramCompilerOptions
			{
				ClassName = "Grammar", Carrier = carrier, Direct = direct, CSharpScanner = RoslynCSharpScanner.Instance,
			});

			EmittedCode.Quiet(compilation.Diagnostics);

			var match = EmittedCode.Match(EmittedCode.Compile(Assert.Single(compilation.Sources).Text), "Grammar", "TryParseStart", input);

			Assert.False(match.IsSuccess);

			yield return match.Error;
		}
	}

	/// <summary>
	/// A refusal is read twice, and on the immediate carrier the first reading has run what it
	/// read; the second, made for what the refusal says, hands out no value and runs nothing
	/// (<c>Failure.Unasked</c>). So a refusal costs the constructions of its accepted prefix
	/// once, as an acceptance costs its own once.
	/// </summary>
	[Fact]
	public void A_refusal_runs_the_constructions_of_its_prefix_once_as_an_acceptance_does()
	{
		// Recursive, so that it is read by methods and not lowered to a flat rendering, which
		// runs its constructions only once it has accepted.
		const string Counting =
			"Start : @string = n: Name & '=' => @(n)\n" +
			"                | '(' & s: Start & ')' => @(s)\n" +
			"Name : @string = t: ['a'..'z']+ => @(Hit(t))\n" +
			"parse Start\n";

		var result = GramCompiler.Compile(Counting, new GramCompilerOptions
		{
			ClassName     = "Probe",
			Namespace     = "Refused",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Carrier       = CarrierKind.Immediate,
		});

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		var source = Assert.Single(result.Sources).Text;

		Assert.Contains("ImmediateValues_DotGram", source, StringComparison.Ordinal);

		var probe = EmittedCode.Compile(
			source, "Probe", "Refused",
			"public static int Hits; static string Hit(string t) { Hits++; return t; }");

		var hits = probe.GetType("Refused.Probe")!.GetField("Hits")!;

		Assert.True(EmittedCode.Match(probe, "Refused.Probe", "TryParseStart", "ab=").IsSuccess);
		Assert.Equal(1, (int)hits.GetValue(null)!);

		hits.SetValue(null, 0);

		Assert.False(EmittedCode.Match(probe, "Refused.Probe", "TryParseStart", "ab").IsSuccess);
		Assert.Equal(1, (int)hits.GetValue(null)!);
	}

	/// <summary>
	/// A context is read twice over only where it can be put back (§7.7): with a <c>Mark()</c>
	/// and a <c>Rollback</c> a refused input runs its guard in both readings, and the second
	/// begins from the state the first began with; without them it is read once.
	/// </summary>
	[Theory]
	[InlineData(false, 1)]
	[InlineData(true, 2)]
	public void A_context_is_read_again_only_where_it_can_be_put_back(bool rewinds, int guards)
	{
		var pair = rewinds ? "internal int Mark() => Count; internal void Rollback(int at) => Count = at; " : "";

		// Through the generator a build runs, where the symbol resolver can find the pair.
		var assembly = GeneratorDriverTests.Build(
			"public sealed class Words { public static int Guards; public int Count; " + pair +
			"public bool Add() { Guards++; Count++; return true; } }\n" +
			"[DotGram.Gram(\"context : @Words\\nStart = ['a'..'z'] & when @(context.Add()) & '!'\\nparse Start\")]\n" +
			"public partial class Rewound { }\n");

		var words   = assembly.GetType("Words")!;
		var context = Activator.CreateInstance(words)!;
		var match   = assembly.GetType("Rewound")!.GetMethod("TryParseStart", [typeof(string), words])!.Invoke(null, ["a?", context])!;

		Assert.False((bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!);
		Assert.Equal(guards, (int)words.GetField("Guards")!.GetValue(null)!);

		// Put back before the second reading, the context holds what one reading wrote.
		Assert.Equal(1, (int)words.GetField("Count")!.GetValue(context)!);
	}
}
