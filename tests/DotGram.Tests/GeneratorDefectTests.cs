using System;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Lexical readings that must agree with the reading over characters: an "until" read by the
/// lexer's automaton, a terminal that places a state mark its own value reads, and a token whose
/// value is read again over its text.
/// </summary>
public sealed class GeneratorDefectTests
{
	/// <summary>
	/// <c>(?!L &amp; X)*</c> stops where L begins, and the lookahead sees past what the repetition
	/// consumed. The automaton reads the strings L does not occur in, which is that only where L
	/// follows the repetition and the run cannot end inside an occurrence of L: <c>ab</c> is refused
	/// by <c>(?!"ab" &amp; ['a' | 'b'])* &amp; 'b'</c> at its first character, and <c>caaaa</c> by
	/// <c>(?!"aaa" &amp; any)* &amp; "aaa"</c>, whose run stops before the second character. Read
	/// with the delimiter that closes it, the until is one machine and exact; standing anywhere
	/// else, a delimiter of more than one character makes the terminal no regular pattern, and the
	/// grammar is compiled over characters, saying so.
	/// </summary>
	[Theory]
	[InlineData("T = { (?!\"ab\" & ['a' | 'b'])* & 'b' }", "ab,aab,abb,b,bb,ba", false)]
	[InlineData("T = { 'c' & (?!\"ab\" & ['a' | 'b'])* }", "cab,ca,cb,cba,c,cbb", false)]
	[InlineData("T = { 'c' & (?!\"aaa\" & any)* & \"aaa\" }", "caaaa,caaa,cbaaa,caabaaa,caaaaa,cbaab", true)]
	[InlineData("T = { 'c' & (?!\"aa\" & ['a' | 'b'])* & \"aa\" }", "caaa,caa,cbaa,cab,caaaa,cbabaa", true)]
	[InlineData("T = { 'c' & (?!\"ab\" & ['a' | 'b'])* & \"ab\" }", "cab,caab,cbab,cabab,cb", true)]
	[InlineData("T = { 'c' & (?!\"abab\" & ['a' | 'b'])* & \"abab\" }", "cabab,cababab,cbabab,caabab,cabaabab", true)]
	[InlineData("T = { 'c' & (?!\"aa\" & 'b')* & \"aa\" }", "caa,cbaa,cbbaa,caaa,cba", false)]
	[InlineData("T = { 'c' & (?!\"ab\" & 'b')* & \"ab\" }", "cab,cbab,cbbab,caab,cba", true)]
	[InlineData("T = { '/' & (?!\"*/\" & any)* & \"*/\" }", "/*/,/a*/,/**/,/a**/,/*/*/", true)]
	[InlineData("T = { 'c' & (?!'x' & any)* } | 'x'", "cab,cax,c,x", true)]
	[InlineData("T = 'c' & (?!\"ab\" & ['a' | 'b'])* & Std.Nested('b', 'z')", "cabz,cbz,caabz,cbbzz,cz", false)]
	public void An_until_reads_as_over_characters(string rules, string inputs, bool cut)
	{
		var grammar = Rooted(rules);
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Lexical       = true,
			Direct        = true,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		Assert.Equal(!cut, result.Diagnostics.Any(static one => one.Id == GramCompiler.NotCut));

		var assembly = EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));
		var twin = Compiled(grammar, direct: true);

		foreach (var input in inputs.Split(','))
		{
			var over  = EmittedCode.Match(twin, "Grammar", "TryParseRoot", input);
			var token = EmittedCode.Match(assembly, "Grammar", "TryParseRoot", input);

			Assert.True(over.IsSuccess == token.IsSuccess && Equals(over.Value, token.Value), $"\"{input}\": over characters {over}, over tokens {token}");
		}
	}

	/// <summary>
	/// A terminal that places a state mark and a value that reads <c>parserState</c>, where the
	/// syntax places none: the emitted parser compiles and the value sees the terminal's own mark.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_terminal_with_state_read_by_its_value_compiles(bool direct)
	{
		const string grammar =
			"""
			state : @int
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				T : @string = { 'a'+ } with state @(7) => @(parserState.Length.ToString())
			}
			Root : @string = s: Lexical.T => @(s)
			parse Root
			""";

		var (assembly, _) = Lexical(grammar, direct);
		var twin = Compiled(grammar, direct);

		foreach (var input in new[] { "a", "aa", "", "b" })
		{
			var over  = EmittedCode.Match(twin, "Grammar", "TryParseRoot", input);
			var token = EmittedCode.Match(assembly, "Grammar", "TryParseRoot", input);

			Assert.True(over.IsSuccess == token.IsSuccess && Equals(over.Value, token.Value), $"\"{input}\": over characters {over}, over tokens {token}");
		}
	}

	/// <summary>
	/// A token the lexer accepts is one whose value the reading again over its text reads too:
	/// <c>('b'+){2}</c> takes <c>bb</c> over characters, the first run giving one back.
	/// </summary>
	[Theory]
	[InlineData("T : @string = { ('b'+){2} } => @(parserText)", "bb,bbb,b,")]
	[InlineData("T : @string = { ('b'+){2} & 'c' } => @(parserText)", "bbc,bbbc,bc")]
	[InlineData("T : @string = { ('b'+)+ & 'b' } => @(parserText)", "bb,bbb,b")]
	[InlineData("T : @string = { ('b'+){2,3} } => @(parserText)", "bb,bbb,bbbb,b")]
	[InlineData("T : @string = { ('b'+){2} }", "bb,bbb,b")]
	[InlineData("T = { ('b'+){2} }", "bb,bbb,b")]
	[InlineData("T = { ('a' & 'b'+){2} }", "abab,abbab,ab,aba")]
	[InlineData("T : @string = { ('b' | 'b' & 'b')+ & 'c' } => @(parserText)", "bc,bbc,bbbc")]
	public void A_token_the_lexer_accepts_is_valued_as_over_characters(string rules, string inputs)
	{
		var grammar = Rooted(rules).Replace("Root = Lexical.T", "Root : @string = s: Lexical.T => @(s ?? \"<null>\")", StringComparison.Ordinal);

		foreach (var direct in new[] { true, false })
		{
			var (assembly, _) = Lexical(grammar, direct);
			var twin = Compiled(grammar, direct);

			foreach (var input in inputs.Split(','))
			{
				var over  = EmittedCode.Match(twin, "Grammar", "TryParseRoot", input);
				var token = EmittedCode.Match(assembly, "Grammar", "TryParseRoot", input);

				Assert.True(over.IsSuccess == token.IsSuccess && Equals(over.Value, token.Value), $"\"{input}\" (direct: {direct}): over characters {over}, over tokens {token}");
			}
		}
	}

	/// <summary>
	/// A plain terminal the automaton reads wider than its rule — a guard it takes as an empty match,
	/// a choice whose earlier alternative stops short of a later one inside an atomic group — is
	/// measured by its own rule after the lexer: success, value and the place of a refusal are the
	/// reading over characters', and <c>bbb</c> is no longer a success with no value.
	/// </summary>
	[Theory]
	[InlineData("T : @string = { ('b' | 'b' & 'b'){2} } => @(parserText)", "bbb,bb,bbbb,b,b b")]
	[InlineData("T = { ('b' | 'b' & 'b'){2} }", "bbb,bb,bbbb,b")]
	[InlineData("T : @string = { 'a'+ & when @(parserText.Length < 3) } => @(parserText)", "aa,aaa,a,aa aa,aaaa")]
	[InlineData("T : @string = { 'a' | 'a' & 'b' } => @(parserText)", "a,ab,a a")]
	public void A_plain_terminal_read_wider_by_the_automaton_is_measured_by_its_rule(string rules, string inputs)
	{
		var grammar = "trivia = { ' '* }\nnamespace Lexical\n{\n\ttrivia = none\n\t" + rules +
			"\n}\nRoot : @string = s: Lexical.T+ => @(string.Join(\"|\", s))\nparse Root\n";

		foreach (var direct in new[] { true, false })
		{
			var (assembly, _) = Lexical(grammar, direct);
			var twin = Compiled(grammar, direct);

			foreach (var input in inputs.Split(','))
			{
				var over  = EmittedCode.Match(twin, "Grammar", "TryParseRoot", input);
				var token = EmittedCode.Match(assembly, "Grammar", "TryParseRoot", input);

				Assert.True(
					over.IsSuccess == token.IsSuccess && Equals(over.Value, token.Value),
					$"\"{input}\" (direct: {direct}): over characters {over}, over tokens {token}");
			}
		}
	}

	/// <summary>
	/// A guard in a terminal that is not atomic cannot be measured by its rule, whose reading the
	/// syntax could come back into: the grammar is compiled over characters, saying so, and the
	/// guard is respected.
	/// </summary>
	[Fact]
	public void A_guard_in_a_terminal_that_is_not_atomic_keeps_the_grammar_over_characters()
	{
		var grammar = "trivia = { ' '* }\nnamespace Lexical\n{\n\ttrivia = none\n\tT = 'a'+ & when @(parserText.Length < 3)\n}\nRoot = Lexical.T+\nparse Root\n";
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Lexical       = true,
			Direct        = true,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		Assert.Contains(result.Diagnostics, static one => one.Id == GramCompiler.NotCut);

		var assembly = EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static one => one.Text));

		var twin = Compiled(grammar, direct: true);

		foreach (var input in new[] { "aa", "aaa", "a", "aa aa", "" })
			Assert.Equal(EmittedCode.Match(twin, "Grammar", "TryParseRoot", input).IsSuccess, EmittedCode.Match(assembly, "Grammar", "TryParseRoot", input).IsSuccess);
	}

	static string Rooted(string rules)
	{
		return "trivia = { ' '* }\nnamespace Lexical\n{\n\ttrivia = none\n\t" + rules + "\n}\nRoot = Lexical.T\nparse Root\n";
	}

	static (Assembly Assembly, string Source) Lexical(string grammar, bool direct)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Lexical       = true,
			Direct        = direct,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);
		Assert.DoesNotContain(result.Diagnostics, static one => one.Id == GramCompiler.NotCut);

		var source = result.Sources[0].Text;

		return (EmittedCode.Compile(source, sourceParts: result.Sources.Skip(1).Select(static one => one.Text)), source);
	}

	static Assembly Compiled(string grammar, bool direct)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Direct        = direct,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static source => source.Text));
	}
}
