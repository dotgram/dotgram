using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Emit;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A terminal that binds text in some alternatives and not in others — a raw string beside a
/// plain one — reads a token through an alternative that binds nothing whole, by the automaton,
/// where none of the alternatives that bind could have been taken: what each of those necessarily
/// begins with is run at the token's start, and a token none of them begins is not read again by
/// the rule. And the machine that reads a position again without such terminals is written only
/// where it could find a token there.
/// </summary>
public sealed partial class BackReferenceTests
{
	/// <summary>A string as the expression language writes it: a raw one, or a plain one guarded against six quotes.</summary>
	const string Strings =
		"""
		trivia = { (' ' | '\n')* }
		namespace Lexical
		{
			trivia = none
			RawText   = $q: '"'{3,} & [^ '"'] & ({ [^ '"']+ } | ?!$q & '"'+ & [^ '"'])* & $q
			Str       = { RawText | PlainText }
			PlainText = '"' & ?!('"'{5}) & TextPart* & '"'
			TextPart  = '\\' & any | { [^ '"' | '\\']+ }
			Name      = ['a'..'z']+
		}
		Item : @string = s: Lexical.Str => @("s:" + s) | n: Lexical.Name => @("n:" + n)
		Items : @string = items: Item* => @(string.Join("|", items))
		parse Items
		""";

	/// <summary>
	/// The plain string is read whole by the automaton and the rule is not read again for it; the
	/// raw one is tagged by its opening run and read again. Both forms agree with the reading over
	/// characters on every run of quotes, from none to eight, closed, run through to the end of the
	/// text, and followed by quotes alone.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_token_none_of_the_binding_alternatives_could_be_is_not_read_again(bool direct)
	{
		BackReferenceEmission.RequireBoth();

		var (assembly, source) = Lexical(Strings, direct);
		var twin = Compiled(Strings, CarrierKind.Auto, direct: true);

		// The tags' machine stands beside the first, and no machine to read a position again:
		// nothing but a string begins with a quote.
		Assert.Contains("Lex_DotGram_Tag", source, StringComparison.Ordinal);
		Assert.DoesNotContain("Lex_DotGram_Unbound", source, StringComparison.Ordinal);

		var inputs = new System.Collections.Generic.List<string>();

		for (var run = 0; run <= 8; run++)
		{
			var quotes = new string('"', run);

			inputs.Add(quotes + "ab" + quotes);
			inputs.Add(quotes + "ab");
			inputs.Add(quotes);
			inputs.Add(quotes + "ab" + quotes + "\"");
			inputs.Add(quotes + "a\"b" + quotes + " x");
		}

		inputs.AddRange(["\"a\" \"\"\"b\"\"\" \"c\"", "\"\" \"\"", "\"a\\\"b\" x", "\"a\\\\\" \"b\\\"\"", "\"\"\"a\"\"\"\"\"\"b\"\"\"", "x \"\"\"\"a\"\"\"b\"\"\"\" y", "\"a\\"]);

		foreach (var input in inputs)
			Same("strings", input, EmittedCode.Match(twin, "Grammar", "TryParseItems", input), EmittedCode.Match(assembly, "Grammar", "TryParseItems", input), direct ? "reader" : "engine");

		Assert.Equal("s:\"ab\"|s:\"\"\"\"b\"\"\"\"|n:x", EmittedCode.Match(assembly, "Grammar", "TryParseItems", "\"ab\" \"\"\"\"b\"\"\"\" x").Value);
	}

	/// <summary>
	/// Where a position read again could find nothing, no machine is written for it, and a reading
	/// the rule refuses is refused as any terminal's is: cut short at the end of the text, with the
	/// same outcome as over characters.
	/// </summary>
	[Theory]
	[InlineData("\"\"\"\"")]
	[InlineData("\"\"\"\"\"\"\"")]
	[InlineData("\"\"\"abc")]
	[InlineData("\"abc")]
	public void A_reading_refused_where_nothing_else_could_stand_is_refused_as_before(string input)
	{
		BackReferenceEmission.RequireBoth();

		var (assembly, _) = Lexical(Strings, direct: true);
		var twin = Compiled(Strings, CarrierKind.Auto, direct: true);

		// Four quotes are two empty strings, in both; the rest is refused, in both, alike.
		Assert.Equal(EmittedCode.Outcome(twin, "Grammar", "TryParseItems", input), EmittedCode.Outcome(assembly, "Grammar", "TryParseItems", input));
		Assert.Equal(EmittedCode.Match(twin, "Grammar", "TryParseItems", input).IsSuccess, EmittedCode.Match(assembly, "Grammar", "TryParseItems", input).IsSuccess);
		Assert.Equal(input == "\"\"\"\"", EmittedCode.Match(assembly, "Grammar", "TryParseItems", input).IsSuccess);
	}

	/// <summary>
	/// The machine that reads a position again is still written where another token begins like
	/// the terminal's beginning: an identifier under <c>$q: [a-z]+ &amp; ':' &amp; $q</c>.
	/// </summary>
	[Fact]
	public void The_machine_that_reads_a_position_again_is_written_where_it_can_find_a_token()
	{
		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				T  = { $q: ['a'..'z']+ & ':' & $q }
				Id = ['a'..'z']+
			}
			Item : @string = t: Lexical.T => @("t:" + t) | i: Lexical.Id => @("id:" + i) | ':' => @(":")
			Root : @string = items: Item* => @(string.Join("|", items))
			parse Root
			""";

		var (assembly, source) = Lexical(grammar, direct: true);

		Assert.Contains("Lex_DotGram_Unbound(", source, StringComparison.Ordinal);
		Assert.Equal("id:ab|:|id:ac", EmittedCode.Match(assembly, "Grammar", "TryParseRoot", "ab:ac").Value);
	}

	/// <summary>
	/// An alternative that binds nothing may not match a string that begins a later one's: the
	/// automaton would take the later, longer reading where the rule, trying them in order, takes
	/// the earlier. Such a terminal is read again for every token, and agrees with the characters.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void An_earlier_alternative_that_begins_a_later_one_keeps_the_reading_again(bool direct)
	{
		BackReferenceEmission.RequireBoth();

		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				T  = { $q: '"'{3,} & [^ '"'] & (?!$q & any)* & $q | '"' & ['a'..'z']* | '"' & ['a'..'z']* & '"' }
				Id = ['a'..'z']+
			}
			Item : @string = t: Lexical.T => @("t:" + t) | i: Lexical.Id => @("id:" + i)
			Root : @string = items: Item* => @(string.Join("|", items))
			parse Root
			""";

		var (assembly, source) = Lexical(grammar, direct);
		var twin = Compiled(grammar, CarrierKind.Auto, direct: true);

		Assert.DoesNotContain("Lex_DotGram_Tag", source, StringComparison.Ordinal);

		foreach (var input in new[] { "\"ab\"", "\"ab", "\"ab\" x", "\"\"\"a\"\"\"", "\"" })
			Same("ordered", input, EmittedCode.Match(twin, "Grammar", "TryParseRoot", input), EmittedCode.Match(assembly, "Grammar", "TryParseRoot", input), direct ? "reader" : "engine");

		// The rule's reading: the second alternative reads `"ab`, and the closing quote is a token
		// of its own, where the automaton alone would have read `"ab"` whole.
		Assert.Equal("t:\"ab|t:\"", EmittedCode.Match(assembly, "Grammar", "TryParseRoot", "\"ab\"").Value);
	}

	/// <summary>
	/// A lookahead in an alternative that binds nothing is answered by the tags only where
	/// everything it refuses begins as some binding alternative does; one that refuses more — a
	/// name after the quote, which no raw string begins with — keeps the rule reading every token
	/// again, and the answers still agree.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_lookahead_the_tags_do_not_answer_keeps_the_reading_again(bool direct)
	{
		BackReferenceEmission.RequireBoth();

		const string grammar =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				T = { $q: '"'{3,} & [^ '"'] & (?!$q & any)* & $q | '"' & ?!('x') & [^ '"']* & '"' }
			}
			Item : @string = t: Lexical.T => @("t:" + t) | 'x' => @("x")
			Root : @string = items: Item* => @(string.Join("|", items))
			parse Root
			""";

		var (assembly, source) = Lexical(grammar, direct);
		var twin = Compiled(grammar, CarrierKind.Auto, direct: true);

		Assert.DoesNotContain("Lex_DotGram_Tag", source, StringComparison.Ordinal);

		foreach (var input in new[] { "\"ab\"", "\"xab\"", "\"\"\"x\"\"\"", "\"\" x", "\"x" })
			Same("lookahead", input, EmittedCode.Match(twin, "Grammar", "TryParseRoot", input), EmittedCode.Match(assembly, "Grammar", "TryParseRoot", input), direct ? "reader" : "engine");

		Assert.False(EmittedCode.Match(assembly, "Grammar", "TryParseRoot", "\"xab\"").IsSuccess);
	}
}
