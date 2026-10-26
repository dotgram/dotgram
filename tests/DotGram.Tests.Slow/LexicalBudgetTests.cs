using System;
using System.Diagnostics;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A lexer the automaton would take too long to build is not built: the grammar is read over
/// characters, which answers as the lexer would have, the reason says why, and the generator stops
/// in bounded time.
/// </summary>
/// <remarks>
/// <para>
/// Too large to build is not an answer about a pattern. Read as "not regular" or "not read as
/// written" it once cut a grammar on a wrong premise: a terminal whose tail was too large to build
/// was begun by the lexer, which read its beginning greedily and took the characters the tail
/// needed. Each row reaches the budget through another of the questions the inventory asks, and
/// each is held to the grammar compiled without the request, byte for byte, with the budget's
/// reason among the diagnostics.
/// </para>
/// <para>
/// A bound in seconds would say what a runner is like; that the grammar is compiled as it is
/// without the request is what the budget promises.
/// </para>
/// </remarks>
[Collection(typeof(Alone))]
public sealed class LexicalBudgetTests
{
	static string Grammar(string terminals, string item = "t: Lexical.T => @(t)")
	{
		return
			$$"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				{{terminals}}
			}
			Item : @string = {{item}}
			Root : @string = items: Item* => @(string.Join("|", items))
			parse Root
			""";
	}

	static GramCompilation Compiled(string grammar, bool lexical)
	{
		return GramCompiler.Compile(grammar, new GramCompilerOptions { ClassName = "Grammar", Lexical = lexical, CSharpScanner = RoslynCSharpScanner.Instance });
	}

	/// <summary>Sixty-four turns of an until of sixty-three <c>a</c>s, each owing its run's prefixes to the turns after it.</summary>
	static string Turns(int runs)
	{
		return Grammar($"T = {{ ((?!\"{new string('a', 63)}\" & ['a'|'b'])* & 'a'){{{runs}}} & 'b' }}");
	}

	public static readonly TheoryData<string, string> Rows = new()
	{
		// The tail of a terminal the lexer begins: asked whether it is read by the automaton (Unread).
		{ "tail", Grammar("Tail = (('a'{64}){64}){64} | 'a'\n\t\tT = 'a'+ & Tail") },

		// A terminal in braces: asked whether it is regular, and whether it is read as written.
		{ "braces", Grammar("T = { (('a'{64}){64}){64} | 'a' }") },

		// A terminal that binds text: asked what begins it, before the inventory is taken (BoundTextRefusals).
		{ "binding", Grammar("T = { $q: (('a'{64}){64}){64} | 'a' } & 'b'") },

		// Every pattern small, the machine of all of them too large: the subset construction itself.
		{ "subsets", Grammar("T = ['a'|'b']* & 'a' & ['a'|'b']{18}") },

		// What a run hands on, past the budget for states owing.
		{ "debts", Turns(64) },
	};

	[Theory]
	[MemberData(nameof(Rows))]
	public void A_lexer_past_the_budget_is_left_to_the_characters(string row, string grammar)
	{
		var clock = Stopwatch.StartNew();
		var cut   = Compiled(grammar, lexical: true);

		TestContext.Current.TestOutputHelper?.WriteLine($"{row}: {clock.ElapsedMilliseconds} ms; {string.Join("; ", cut.Diagnostics.Select(static one => one.Id + " " + one.Message))}");

		Assert.DoesNotContain("Tokenize_DotGram(", cut.Sources[0].Text, StringComparison.Ordinal);
		Assert.Contains(cut.Diagnostics, static one => one.Id == "GRAM5004" && one.Message.Contains("larger than a lexer is built for", StringComparison.Ordinal));
		Assert.Equal(Compiled(grammar, lexical: false).Sources.Select(static one => one.Text), cut.Sources.Select(static one => one.Text));
	}

	/// <summary>The grammar the budget was first found wanting on: <c>aa</c> is one <c>T</c>, the tail being its second <c>a</c>.</summary>
	[Fact]
	public void A_terminal_whose_tail_is_too_large_to_build_reads_as_over_characters()
	{
		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Tail = (('a'{64}){64}){64} | 'a'
				T    = 'a'+ & Tail
			}
			Root = Lexical.T
			parse Root
			""";

		var cut = Compiled(grammar, lexical: true);

		Assert.Contains(cut.Diagnostics, static one => one.Id == "GRAM5004" && one.Message.Contains("larger than a lexer is built for", StringComparison.Ordinal));
		Assert.DoesNotContain(cut.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var assembly = EmittedCode.Compile(cut.Sources[0].Text, sourceParts: cut.Sources.Skip(1).Select(static one => one.Text));

		Assert.True(EmittedCode.Match(assembly, "Grammar", "TryParseRoot", "aa").IsSuccess);
	}

	/// <summary>Sixteen turns stay inside the budget and are cut.</summary>
	[Fact]
	public void Turns_inside_the_budget_are_cut()
	{
		Assert.Contains("Tokenize_DotGram(", Compiled(Turns(16), lexical: true).Sources[0].Text, StringComparison.Ordinal);
	}
}
