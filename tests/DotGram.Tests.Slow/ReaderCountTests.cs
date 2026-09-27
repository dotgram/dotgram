using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What the materialising walk does per block of a text, counted, and flat in the blocks.
/// </summary>
/// <remarks>
/// <para>
/// <b>This is the gate <see cref="ReaderScalingTests"/> could never be.</b> That file bounds a time,
/// and a time on a shared runner cannot tell a defect from the runner: the same shape reads about 12
/// microseconds a block in Release on a quiet machine, 4.5 with tiering and PGO off, and 47 on CI, so
/// its bound is 2.0 and catches only a blow-up. A count needs no quiet machine, cannot be moved by a
/// neighbour, and is bounded here at 1.05 a block.
/// </para>
/// <para>
/// <b>What it would have caught.</b> The walk used to find the marks standing over its start by
/// replaying every open and close from the log's position zero, so a guard near the end of a long log
/// paid for the whole front of it — 512 steps a block at 25 blocks and 8,387 at 400, x4.03 a
/// doubling, while the records it listed stayed flat at forty. Nothing gated that, and the clock beside
/// it read 1.45 where its bound was 2.0.
/// </para>
/// <para>
/// <b>Why it counts its own parser rather than the expression language's.</b> The counters are emitted
/// behind <c>#if DOTGRAM_COUNTS</c> (D144), which no project defines, so no shipped parser carries
/// them; a gate compiles its own with the symbol set. A count is a function of the grammar and the
/// generator, so what this counts is what a shipped parser of this grammar would do. It is not the
/// expression language's own grammar, which cannot be compiled apart from the hand-written host it
/// belongs to — so the shape is reproduced here instead: sibling blocks under one standing mark, each
/// built at a guard while the reading is still going on, which is what a lambda body of n statements
/// is.
/// </para>
/// </remarks>
[Collection(nameof(Alone))]
public sealed class ReaderCountTests
{
	/// <summary>
	/// One mark standing over n sibling blocks, each with a guard that builds while the text is
	/// still being read.
	/// </summary>
	/// <remarks>
	/// Every piece earns its place. The mark on <c>Body</c> is what gives the walk something standing
	/// over it — without one the standing-marks pass has nothing to find and the count is zero at
	/// every size, which is flat and says nothing. The guard in <c>Item</c> is what makes a walk begin
	/// PAST that mark: a walk from the log's start reads the marks off the log it is already stepping
	/// through. And the blocks are siblings rather than nested so that the log in front of the last
	/// one grows with the text while what it must read does not.
	/// </remarks>
	const string Grammar =
		"state : @int\n" +
		"Start : @string = '(' & body: Body with state @(9) & ')' => @(body)\n" +
		"Body  : @string = items: Item+ => @(string.Concat(items))\n" +
		"Item  : @string = '{' & inner: Word & when @(!string.IsNullOrEmpty(inner)) & '}' => @(inner)\n" +
		"Word  : @string = t: ['a'..'z']+ => @(t + parserState.Length.ToString())\n" +
		"parse Start\n";

	/// <summary>What a count a block may grow by between the two sizes.</summary>
	/// <remarks>
	/// Five per cent, the same as <c>BlockScalingTests</c>, and it can be that tight because it is a
	/// count: there is no machine in it. The old walk read x4 a block over this range.
	/// </remarks>
	const double Slack = 1.05;

	[Theory]
	[InlineData("records the scan listed", "CountListed")]
	[InlineData("standing marks the walk read", "CountMarkSteps")]
	public void The_walk_does_a_fixed_amount_a_block(string what, string counter)
	{
		var parser = Counted();

		var small = Count(parser, 50, counter) / 50.0;
		var large = Count(parser, 200, counter) / 200.0;

		// Or the counter is not being written at all and everything below passes on nothing.
		Assert.True(small > 0, $"{what}: the count is zero at 50 blocks, so nothing is being counted.");

		Assert.True(
			large <= small * Slack,
			$"{what}: {large:F2} a block over 200 blocks against {small:F2} over 50, which is " +
			$"{large / small:F2} times as many for each.");
	}

	/// <summary>The counter after reading a text of that many blocks, from a fresh zero.</summary>
	static long Count(Type parser, int blocks, string counter)
	{
		var ways  = parser.GetNestedType("Ways", BindingFlags.NonPublic)!;
		var field = ways.GetField(counter, BindingFlags.NonPublic | BindingFlags.Static)
			?? throw new InvalidOperationException(
				$"the generated Ways has no {counter}; is it still emitted under DOTGRAM_COUNTS?");

		field.SetValue(null, 0L);

		var text = "(" + string.Concat(Enumerable.Repeat("{a}", blocks)) + ")";
		// By parameter type: a publication has several forms — a span, a position, a window — and
		// asking for the name alone is ambiguous.
		var read = parser.GetMethod(
			"TryParseStart",
			BindingFlags.Public | BindingFlags.Static,
			binder: null,
			[typeof(string)],
			modifiers: null)!;
		var match = read.Invoke(null, [text])!;

		Assert.True(
			(bool)match.GetType().GetProperty("IsSuccess")!.GetValue(match)!,
			$"the probe of {blocks} blocks was not read.");

		return (long)field.GetValue(null)!;
	}

	/// <summary>The same grammar a consumer would get, compiled with the counters turned on.</summary>
	static Type Counted()
	{
		var result = GramCompiler.Compile(Grammar, new GramCompilerOptions
		{
			ClassName     = "Counted",
			CSharpScanner = RoslynCSharpScanner.Instance,
			Direct        = true,
			Carrier       = CarrierKind.Tape,
		});

		Assert.DoesNotContain(result.Diagnostics, one => one.Severity == GramSeverity.Error);

		var source = Assert.Single(result.Sources).Text;

		// The `#if` is in the text of every parser; only this compilation defines the symbol.
		Assert.Contains("#if DOTGRAM_COUNTS", source, StringComparison.Ordinal);

		return EmittedCode.Compile(source, "Counted", symbols: ["DOTGRAM_COUNTS"]).GetType("Counted")!;
	}
}
