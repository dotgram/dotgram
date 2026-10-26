using System;
using System.Diagnostics;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A lexer the automaton would take too long to build is not built: the grammar is read over
/// characters, which answers as the lexer would have, and the generator stops in bounded time.
/// </summary>
/// <remarks>
/// Sixty-four turns of an until of sixty-three <c>a</c>s, each owing its run's prefixes to the turns
/// after it, walk more states owing than the budget allows; sixteen turns stay inside it and are
/// cut. A bound in seconds would say what a runner is like; that the grammar is compiled as it is
/// without the request is what the budget promises. Every construction the inventory asks of the
/// terminal stops at the budget, a few seconds each here.
/// </remarks>
[Collection(typeof(Alone))]
public sealed class LexicalBudgetTests
{
	static string Grammar(int runs)
	{
		return
			$$"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				T = { ((?!"{{new string('a', 63)}}" & ['a'|'b'])* & 'a'){{{runs}}} & 'b' }
			}
			Item : @string = t: Lexical.T => @(t)
			Root : @string = items: Item* => @(string.Join("|", items))
			parse Root
			""";
	}

	static GramCompilation Compiled(int runs, bool lexical)
	{
		return GramCompiler.Compile(Grammar(runs), new GramCompilerOptions { ClassName = "Grammar", Lexical = lexical, CSharpScanner = RoslynCSharpScanner.Instance });
	}

	[Fact]
	public void A_lexer_past_the_budget_is_left_to_the_characters()
	{
		var clock = Stopwatch.StartNew();
		var cut   = Compiled(64, lexical: true);

		TestContext.Current.TestOutputHelper?.WriteLine($"64 turns: {clock.ElapsedMilliseconds} ms");

		Assert.DoesNotContain("Tokenize_DotGram(", cut.Sources[0].Text, StringComparison.Ordinal);
		// Which refusal says so depends on which of the inventory's questions met the budget first:
		// asked whether the terminal is regular at all, it is told no, and is then refused as exact.
		Assert.Contains(cut.Diagnostics, static one => one.Id == "GRAM5004");
		Assert.Equal(Compiled(64, lexical: false).Sources.Select(static one => one.Text), cut.Sources.Select(static one => one.Text));

		Assert.Contains("Tokenize_DotGram(", Compiled(16, lexical: true).Sources[0].Text, StringComparison.Ordinal);
	}
}
