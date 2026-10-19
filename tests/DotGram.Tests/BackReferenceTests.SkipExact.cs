using System;

using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A token is read whole by the automaton, and the rule not read again, only where the automaton's
/// reading of the alternative that binds nothing is the rule's: a semantic guard the automaton
/// cannot run, an atomic group inside that cannot give back where the automaton would, keep the
/// rule reading every token again. Each is asked against the reading over characters.
/// </summary>
public sealed partial class BackReferenceTests
{
	/// <summary>
	/// A guard the automaton takes as an empty match: <c>a</c> is refused over characters, where
	/// the guard runs, and must be refused over tokens too — the rule reads the token again.
	/// </summary>
	[Theory]
	[InlineData("T = { $q: 'x' & $q | 'a' & when @(parserText.Length == 0) }", "a")]
	[InlineData("T = { $q: 'x' & $q | G }\n\tG = 'a' & when @(parserText.Length == 0)", "a")]
	public void An_alternative_with_a_guard_is_read_again(string rules, string input)
	{
		BackReferenceEmission.RequireBoth();

		var grammar = Rooted(rules);
		var (assembly, source) = Lexical(grammar, direct: true);
		var twin = Compiled(grammar, CarrierKind.Auto, direct: true);

		Assert.DoesNotContain("Scan_Tag", source, StringComparison.Ordinal);
		Assert.False(EmittedCode.Match(twin, "Grammar", "TryParseRoot", input).IsSuccess);
		Assert.Equal(EmittedCode.Match(twin, "Grammar", "TryParseRoot", input).IsSuccess, EmittedCode.Match(assembly, "Grammar", "TryParseRoot", input).IsSuccess);
	}

	/// <summary>
	/// An atomic group nested in the alternative: <c>{ 'a'+ } &amp; 'a'</c> refuses <c>aa</c> over
	/// characters, the run having taken both and given nothing back, where the automaton would
	/// split them; a bounded run, <c>{ 'a'{2,5} }</c>, refuses six where five and one is not three and
	/// three. Such an alternative is read again — unless the group is a run of one character, with no
	/// upper bound, that nothing after it can begin with but the run itself again, which gives back
	/// nothing anyone would take: the string's run, and the escape beside it, keep the skip.
	/// </summary>
	[Theory]
	[InlineData("T = { $q: 'x' & $q | { 'a'+ } & 'a' }", "aa", false, false)]
	[InlineData("T = { $q: 'x' & $q | ({ 'a'+ } & 'b'?)* & 'a' }", "aa", false, false)]
	[InlineData("T = { $q: 'x' & $q | ({ 'a'{2,5} })* & 'c' }", "aaaaaac", false, false)]
	[InlineData("T = { $q: 'x' & $q | ({ 'a'{2,5} })* & 'c' }", "aaaaaaac", false, true)]
	[InlineData("T = { $q: 'x' & $q | R & 'a' }\n\tR = { 'a'+ }", "aa", false, false)]
	[InlineData("T = { $q: 'x' & $q | { 'a'+ } & 'b' }", "aab", true, true)]
	[InlineData("T = { $q: 'x' & $q | ({ 'a'+ } | 'b')* & 'c' }", "aabaac", true, true)]
	[InlineData("T = { $q: 'x' & $q | '\"' & ('\\\\' & any | { [^ '\"' | '\\\\']+ })* & '\"' }", "\"ab\\\"c\"", true, true)]
	public void An_alternative_with_an_atomic_group_inside_is_read_again_unless_the_group_gives_nothing_back(string rules, string input, bool skips, bool accepted)
	{
		BackReferenceEmission.RequireBoth();

		var grammar = Rooted(rules);
		var (assembly, source) = Lexical(grammar, direct: true);
		var twin = Compiled(grammar, CarrierKind.Auto, direct: true);

		Assert.Equal(skips, source.Contains("Scan_Tag", StringComparison.Ordinal));
		Assert.Equal(accepted, EmittedCode.Match(twin, "Grammar", "TryParseRoot", input).IsSuccess);
		Assert.Equal(accepted, EmittedCode.Match(assembly, "Grammar", "TryParseRoot", input).IsSuccess);
	}

	static string Rooted(string rules)
	{
		return "trivia = { ' '* }\nnamespace Lexical\n{\n\ttrivia = none\n\t" + rules + "\n}\nRoot = Lexical.T\nparse Root\n";
	}
}
