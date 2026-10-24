using System;
using System.Linq;
using System.Text;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A diamond of rules that bind — each a choice of two that call the next — is cut in time linear in
/// its depth, not in the power of it.
/// </summary>
/// <remarks>
/// The beginning of a rule is remembered, and a choice of two alternatives that begin as one rule does
/// begins as that rule does once. Without that the split of the grammar into tokens visits every path
/// down the diamond, twice as many for each level. Held as a ratio of the cut of a deep diamond to the
/// cut of a shallow one, taken in one run (<see cref="ScalingClock"/>): six levels more are about a
/// third more work where the cut is linear and sixty-four times as much where it is not, and a bound
/// in seconds says what a runner is like more than what the generator does.
/// </remarks>
[Collection(typeof(Alone))]
public sealed class DiamondCutScalingTests
{
	const int Shallow = 14;

	const int Deep = 20;

	/// <summary>Between the third more of a linear cut and the sixty-four times of an exponential one.</summary>
	const double Limit = 8;

	[Fact]
	public void A_diamond_of_rules_that_bind_is_cut_in_time_that_does_not_double_with_its_depth()
	{
		Assert.Contains("Tokenize_DotGram(", Cut(Shallow), StringComparison.Ordinal);

		var reading = ScalingClock.Settle(() => Cut(Shallow), () => Cut(Deep), Limit);

		Assert.True(
			reading.Within,
			$"A diamond of {Deep} took {reading.Ratio:F1} times as long to cut as one of {Shallow} " +
			$"({reading.Shorter / 1000:F1} ms against {reading.Longer / 1000:F1} ms), on every one of " +
			$"{ScalingClock.Attempts} measurements ({reading.Readings}).");
	}

	static string Cut(int depth)
	{
		return GramCompiler.Compile(Diamond(depth), new GramCompilerOptions { Lexical = true, CSharpScanner = RoslynCSharpScanner.Instance }).Sources[0].Text;
	}

	static string Diamond(int depth)
	{
		var rules = new StringBuilder();

		for (var k = 0; k < depth; k++)
		{
			rules.Append(k == 0 ? $"\tR{k} = {{ $q{k}: (A{k + 1} | B{k + 1}) & $q{k} }}\n" : $"\tR{k} = $q{k}: (A{k + 1} | B{k + 1}) & $q{k}\n");
			rules.Append($"\tA{k + 1} = R{k + 1} & 'a'\n");
			rules.Append($"\tB{k + 1} = R{k + 1} & 'b'\n");
		}

		rules.Append($"\tR{depth} = 'x' | 'y'\n");

		return "trivia = { ' '* }\nnamespace Lexical\n{\n\ttrivia = none\n" + rules +
			"}\nItem : @string = t: Lexical.R0 => @(t)\nRoot : @string = items: Item* => @(string.Join(\"|\", items))\nparse Root\n";
	}
}
