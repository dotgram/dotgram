using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A search over a long text full of raw strings and elements that bind takes time in proportion to
/// the text: four times the text in well under sixteen times the time.
/// </summary>
/// <remarks>
/// Held as a ratio of the search over the longer text to the one over the shorter, taken in one run
/// (<see cref="ScalingClock"/>), where it used to be the best of fifteen wall-clock timings among the
/// tests that run on every change: a ratio read between runs of a loaded machine is a measurement and
/// belongs with the others. What the search finds is still held in <c>DotGram.Tests</c>
/// (<c>BackReferenceTests.A_search_over_a_long_text_finds_what_the_text_holds</c>).
/// </remarks>
[Collection(typeof(Alone))]
public sealed class BackReferenceSearchScalingTests
{
	/// <summary>Between the four of a linear search and the sixteen of a quadratic one.</summary>
	const double Limit = 10;

	const string RawString = "RawString = { $q: '\"'{3,} } & { (?!$q & any)* } & $q & ?!'\"'";

	const string Element = "Element = '<' & $n: Name & '>' & (Element | Text)* & \"</\" & $n & '>'\nName = ['a'..'z']+\nText = { [^ '<']+ }";

	[Theory]
	[InlineData(RawString, "RawString", "x = \"\"\"ab\"c\"\"\"; y = \"\"\"\"d\"\"\"e\"\"\"\"; ", true)]
	[InlineData(RawString, "RawString", "x = \"\"\"ab\"c\"\"\"; y = \"\"\"\"d\"\"\"e\"\"\"\"; ", false)]
	[InlineData(Element, "Element", "t <ab>x<c>y</c></ab> <d></e> ", true)]
	[InlineData(Element, "Element", "t <ab>x<c>y</c></ab> <d></e> ", false)]
	public void A_search_over_a_long_text_takes_time_in_proportion_to_the_text(string grammar, string rule, string block, bool direct)
	{
		if (direct)
			BackReferenceEmission.RequireReader();
		else
			BackReferenceEmission.RequireBoth();

		var assembly = Compiled(grammar + $"\nfind {rule}\n", direct);
		var shorter  = string.Concat(Enumerable.Repeat(block, 25_000 / block.Length));
		var longer   = string.Concat(Enumerable.Repeat(block, 100_000 / block.Length));

		var reading = ScalingClock.Settle(
			() => EmittedCode.FoundAt(assembly, "Grammar", "Find" + rule, shorter),
			() => EmittedCode.FoundAt(assembly, "Grammar", "Find" + rule, longer),
			Limit);

		Assert.True(
			reading.Within,
			$"Four times the text took {reading.Ratio:F1} times as long to search " +
			$"({reading.Shorter / 1000:F1} ms against {reading.Longer / 1000:F1} ms), on every one of " +
			$"{ScalingClock.Attempts} measurements ({reading.Readings}).");
	}

	static System.Reflection.Assembly Compiled(string grammar, bool direct)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Carrier       = CarrierKind.Auto,
			Direct        = direct,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static source => source.Text));
	}
}
