using System;

using DotGram.Generation;
using DotGram.Grammar;

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
	public void Every_refusal_in_a_sample_is_the_one_it_was() =>
		RefusalCorpus.AssertSample();

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

		foreach (var (carrier, direct) in new[] { (CarrierKind.Tape, false), (CarrierKind.Tape, true), (CarrierKind.Immediate, true) })
		{
			var compilation = GramCompiler.Compile(Grammar, new GramCompilerOptions
			{
				ClassName = "Grammar", Carrier = carrier, Direct = direct, CSharpScanner = RoslynCSharpScanner.Instance,
			});
			EmittedCode.Quiet(compilation.Diagnostics);

			var match = EmittedCode.Match(EmittedCode.Compile(Assert.Single(compilation.Sources).Text), "Grammar", "TryParseStart", input);

			Assert.False(match.IsSuccess);
			Assert.Equal(at, match.Position);
			Assert.Equal("Expected 'b' or eof.", match.Error);
		}
	}

	/// <summary>
	/// A refusal is read twice, and on the immediate carrier what the first reading ran the
	/// second runs again — once more, and only where the parse refused.
	/// </summary>
	[Fact]
	public void A_refusal_runs_a_construction_once_more_and_an_acceptance_does_not()
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

		Assert.Contains("ImmediateValues", source, StringComparison.Ordinal);

		var probe = EmittedCode.Compile(
			source, "Probe", "Refused",
			"public static int Hits; static string Hit(string t) { Hits++; return t; }");

		var hits = probe.GetType("Refused.Probe")!.GetField("Hits")!;

		Assert.True(EmittedCode.Match(probe, "Refused.Probe", "TryParseStart", "ab=").IsSuccess);
		Assert.Equal(1, (int)hits.GetValue(null)!);

		hits.SetValue(null, 0);

		Assert.False(EmittedCode.Match(probe, "Refused.Probe", "TryParseStart", "ab").IsSuccess);
		Assert.Equal(2, (int)hits.GetValue(null)!);
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
