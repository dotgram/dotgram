using System;

using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// What the reading of a token through the alternative that binds nothing leaves out must have
/// changed nothing the rule read through could see: a state mark the terminal's own reading would
/// have placed keeps the value read as written. And the one lookahead the automaton reads, the
/// "until" shape, is read exactly only where its delimiter follows; elsewhere the rule reads the
/// token again.
/// </summary>
public sealed partial class BackReferenceTests
{
	/// <summary>
	/// The terminal places a mark around its choice, and the plain rule's value counts the marks
	/// it stands under: read through the plain rule alone it would see none. Twin, base and
	/// branch say "1".
	/// </summary>
	[Fact]
	public void A_terminal_that_places_a_state_mark_is_valued_as_written()
	{
		BackReferenceEmission.RequireBoth();

		const string grammar =
			"""
			state : @int
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				Raw : @string = $q: ['x'..'y']+ & ':' & $q => @(parserText)
				T : @string = { r: Raw | p: Plain } with state @(7) => @(r ?? p!)
				Plain : @string = 'a' => @(parserState.Length.ToString())
			}
			Root : @string = s: Lexical.T & ('z' with state @(8))? => @(s)
			parse Root
			""";

		var (assembly, source) = Lexical(grammar, direct: true);
		var twin = Compiled(grammar, CarrierKind.Auto, direct: true);

		Assert.Contains("Scan_Tag", source, StringComparison.Ordinal);
		Assert.DoesNotContain("_For_Lexical_T_DotGram(", source, StringComparison.Ordinal);

		foreach (var input in new[] { "a", "az", "x:x" })
			Assert.Equal(EmittedCode.Match(twin, "Grammar", "TryParseRoot", input).Value, EmittedCode.Match(assembly, "Grammar", "TryParseRoot", input).Value);

		Assert.Equal("1", EmittedCode.Match(assembly, "Grammar", "TryParseRoot", "a").Value);
	}

	/// <summary>
	/// <c>(?!"ab" &amp; ['a' | 'b'])* &amp; 'b'</c> refuses <c>ab</c> over characters at its first
	/// character, where the automaton, reading "the strings <c>ab</c> does not occur in", takes
	/// <c>a</c> and then the <c>b</c>: not exact, so the token is read again. With the delimiter
	/// following the repetition the two readings are one, and the skip stands — unless the delimiter
	/// can overlap itself, <c>"aa"</c>, where the rule stops the run at its first beginning and the
	/// automaton lets the run take an <c>a</c> first.
	/// </summary>
	[Theory]
	[InlineData("T = { $q: 'x'+ & ':' & $q | (?!\"ab\" & ['a' | 'b'])* & 'b' }", "ab,aab,abb,b,bb", false)]
	[InlineData("T = { $q: 'x'+ & ':' & $q | 'c' & (?!\"ab\" & ['a' | 'b'])* }", "cab,ca,cb,cba,c", false)]
	[InlineData("T = { $q: 'x'+ & ':' & $q | 'c' & (?!\"ab\" & ['a' | 'b'])* & \"ab\" }", "cab,caab,cbab,cabab,cb", true)]
	[InlineData("T = { $q: 'x'+ & ':' & $q | 'c' & (?!\"aa\" & ['a' | 'b'])* & \"aa\" }", "caaa,caa,cbaa,cab,caaaa", false)]
	public void An_until_is_exact_only_where_its_delimiter_follows(string rules, string inputs, bool skips)
	{
		BackReferenceEmission.RequireBoth();

		var grammar = Rooted(rules);
		var (assembly, source) = Lexical(grammar, direct: true);
		var twin = Compiled(grammar, CarrierKind.Auto, direct: true);

		Assert.Equal(skips, source.Contains("Scan_Tag", StringComparison.Ordinal));

		foreach (var input in inputs.Split(','))
			Assert.Equal(EmittedCode.Match(twin, "Grammar", "TryParseRoot", input).IsSuccess, EmittedCode.Match(assembly, "Grammar", "TryParseRoot", input).IsSuccess);
	}

	/// <summary>
	/// The documented contract of a token, and not a defect of the skip: a terminal's token is the
	/// alternative's longest match, where the rule's own reading of it may stop short. <c>'a'+ &amp;
	/// ('a' &amp; 'b')?</c> reads <c>aab</c> whole as a token, plain or beside a binding alternative
	/// alike, where over characters the run takes both <c>a</c> and the rest is refused.
	/// </summary>
	[Fact]
	public void A_token_is_the_longest_match_beside_a_binding_alternative_as_it_is_alone()
	{
		BackReferenceEmission.RequireBoth();

		var bound = Rooted("T = { $q: 'x'+ & ':' & $q | 'a'+ & ('a' & 'b')? }");
		var plain = Rooted("T = { 'a'+ & ('a' & 'b')? }");
		var (beside, source) = Lexical(bound, direct: true);
		var (alone, _)       = Lexical(plain, direct: true);
		var twin = Compiled(bound, CarrierKind.Auto, direct: true);

		Assert.Contains("Scan_Tag", source, StringComparison.Ordinal);

		foreach (var input in new[] { "aab", "aaab", "ab", "aa", "a" })
			Assert.Equal(EmittedCode.Match(alone, "Grammar", "TryParseRoot", input).IsSuccess, EmittedCode.Match(beside, "Grammar", "TryParseRoot", input).IsSuccess);

		Assert.True(EmittedCode.Match(beside, "Grammar", "TryParseRoot", "aab").IsSuccess);
		Assert.False(EmittedCode.Match(twin, "Grammar", "TryParseRoot", "aab").IsSuccess);
	}
}
