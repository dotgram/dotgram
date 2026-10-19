using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// An alternative of an ordered choice that is a call to a rule beginning with fixed text is not
/// gone into where the text is not there: the fixed characters are tested first, and where one of
/// them is missing the parse fails as the rule would have failed on it — at the same position, with
/// the same expectation — and goes on to the next alternative without the call, the capture, or the
/// rule's own reading. A string that is either a raw one (three quotes) or a plain one is the shape:
/// every plain string went into the raw rule and out again.
/// </summary>
public sealed class NecessaryPrefixTests
{
	/// <summary>Strings as the expression language writes them, read over characters.</summary>
	const string Strings =
		"""
		trivia = { ' '* }
		RawText : @string = $q: '"'{3,} & [^ '"'] & ({ [^ '"']+ } | ?!$q & '"'+ & [^ '"'])* & $q => @(parserText)
		Str : @string = { r: RawText | t: PlainText } => @(r ?? t!)
		PlainText : @string = '"' & ?!('"'{5}) & parts: TextPart* & '"' => @(string.Concat(parts))
		TextPart : @string = '\\' & e: any => @(e) | t: { [^ '"' | '\\']+ } => @(t)
		Item : @string = s: Str => @("s:" + s) | n: ['a'..'z']+ => @("n:" + n)
		Items : @string = items: Item* => @(string.Join("|", items))
		parse Items
		""";

	/// <summary>The same strings as one token, cut by the lexer and read again by the rule.</summary>
	const string LexicalStrings =
		"""
		trivia = { ' '* }
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
	/// Inputs that take the raw alternative, the plain one, neither, and that run out inside each:
	/// where the guard says no at the first, the second and the third quote, and where it says yes
	/// and the raw rule still refuses.
	/// </summary>
	static readonly string[] Inputs =
	[
		"\"ab\"",
		"\"\"",
		"\"\" \"\"",
		"\"\"\"ab\"\"\"",
		"\"\"\"\"ab\"\"\"\"",
		"\"ab\" x \"\"\"c\"\"\"",
		"\"a\\\"b\"",
		"",
		"x",
		"\"",
		"\"a",
		"\"\"\"",
		"\"\"\"a",
		"\"\"\"\"",
		"\"\"\"\"\"",
		"\"\"\"\"\"\"",
		"\"\"\"\"\"\"\"",
		"\"\"\"ab\"\"",
		"\"\"\"ab\"\"\"\"",
		"\"ab\"\"",
		"\"a\\",
		"\"ab\" \"",
		"\"ab\" \"\"\"",
		"x \"\"\"\"",
	];

	/// <summary>
	/// What each input came to before the guard was written, by the engine: the outcome, the
	/// position and the words of a refusal. The guard changes none of it, the ties included — a
	/// lone quote expects the raw rule's second quote and then what a plain string's text may be.
	/// </summary>
	const string Engine =
		"""""
		"\"ab\"" => Success at 0: [s:ab]
		"\"\"" => Success at 0: [s:]
		"\"\" \"\"" => Success at 0: [s:|s:]
		"\"\"\"ab\"\"\"" => Success at 0: [s:"""ab"""]
		"\"\"\"\"ab\"\"\"\"" => Success at 0: [s:""""ab""""]
		"\"ab\" x \"\"\"c\"\"\"" => Success at 0: [s:ab|n:x|s:"""c"""]
		"\"a\\\"b\"" => Success at 0: [s:a"b]
		"" => Success at 0: []
		"x" => Success at 0: [n:x]
		"\"" => Starved at 1: [Expected '"' or ['\\' | '\u0000'..'!' | '#'..'[' | ']'..'￿'].]
		"\"a" => Starved at 2: [Expected ['\\' | '\u0000'..'!' | '#'..'[' | ']'..'￿'] or '"'.]
		"\"\"\"" => Starved at 3: [Expected [^ '"'], '"' or ['\\' | '\u0000'..'!' | '#'..'[' | ']'..'￿'].]
		"\"\"\"a" => Starved at 4: [Expected [^ '"'], '"', $q or ['\\' | '\u0000'..'!' | '#'..'[' | ']'..'￿'].]
		"\"\"\"\"" => Success at 0: [s:|s:]
		"\"\"\"\"\"" => Starved at 5: [Expected [^ '"'], '"' or ['\\' | '\u0000'..'!' | '#'..'[' | ']'..'￿'].]
		"\"\"\"\"\"\"" => Starved at 6: [Expected [^ '"'].]
		"\"\"\"\"\"\"\"" => Starved at 7: [Expected [^ '"'].]
		"\"\"\"ab\"\"" => Starved at 7: [Expected [^ '"'], '"' or ['\\' | '\u0000'..'!' | '#'..'[' | ']'..'￿'].]
		"\"\"\"ab\"\"\"\"" => Starved at 9: [Expected '"' or ['\\' | '\u0000'..'!' | '#'..'[' | ']'..'￿'].]
		"\"ab\"\"" => Starved at 5: [Expected '"' or ['\\' | '\u0000'..'!' | '#'..'[' | ']'..'￿'].]
		"\"a\\" => Starved at 3: [Expected any.]
		"\"ab\" \"" => Starved at 6: [Expected '"' or ['\\' | '\u0000'..'!' | '#'..'[' | ']'..'￿'].]
		"\"ab\" \"\"\"" => Starved at 8: [Expected [^ '"'], '"' or ['\\' | '\u0000'..'!' | '#'..'[' | ']'..'￿'].]
		"x \"\"\"\"" => Success at 0: [n:x|s:|s:]
		""""";

	/// <summary>The same by the reader, which names two classes its own way.</summary>
	const string Reader =
		"""""
		"\"ab\"" => Success at 0: [s:ab]
		"\"\"" => Success at 0: [s:]
		"\"\" \"\"" => Success at 0: [s:|s:]
		"\"\"\"ab\"\"\"" => Success at 0: [s:"""ab"""]
		"\"\"\"\"ab\"\"\"\"" => Success at 0: [s:""""ab""""]
		"\"ab\" x \"\"\"c\"\"\"" => Success at 0: [s:ab|n:x|s:"""c"""]
		"\"a\\\"b\"" => Success at 0: [s:a"b]
		"" => Success at 0: []
		"x" => Success at 0: [n:x]
		"\"" => Starved at 1: [Expected '"' or ['\\' | '\u0000'..'!' | '#'..'[' | ']'..'￿'].]
		"\"a" => Starved at 2: [Expected ['\\' | '\u0000'..'!' | '#'..'[' | ']'..'￿'] or '"'.]
		"\"\"\"" => Starved at 3: [Expected [^ '"'], '"' or ['\\' | '\u0000'..'!' | '#'..'[' | ']'..'￿'].]
		"\"\"\"a" => Starved at 4: [Expected ['\u0000'..'!' | '#'..'￿' | '"'], $q, ['\\' | '\u0000'..'!' | '#'..'[' | ']'..'￿'] or '"'.]
		"\"\"\"\"" => Success at 0: [s:|s:]
		"\"\"\"\"\"" => Starved at 5: [Expected [^ '"'], '"' or ['\\' | '\u0000'..'!' | '#'..'[' | ']'..'￿'].]
		"\"\"\"\"\"\"" => Starved at 6: [Expected [^ '"'].]
		"\"\"\"\"\"\"\"" => Starved at 7: [Expected [^ '"'].]
		"\"\"\"ab\"\"" => Starved at 7: [Expected [^ '"'], '"' or ['\\' | '\u0000'..'!' | '#'..'[' | ']'..'￿'].]
		"\"\"\"ab\"\"\"\"" => Starved at 9: [Expected '"' or ['\\' | '\u0000'..'!' | '#'..'[' | ']'..'￿'].]
		"\"ab\"\"" => Starved at 5: [Expected '"' or ['\\' | '\u0000'..'!' | '#'..'[' | ']'..'￿'].]
		"\"a\\" => Starved at 3: [Expected [^ ].]
		"\"ab\" \"" => Starved at 6: [Expected '"' or ['\\' | '\u0000'..'!' | '#'..'[' | ']'..'￿'].]
		"\"ab\" \"\"\"" => Starved at 8: [Expected [^ '"'], '"' or ['\\' | '\u0000'..'!' | '#'..'[' | ']'..'￿'].]
		"x \"\"\"\"" => Success at 0: [n:x|s:|s:]
		""""";

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_refusal_is_where_it_was_and_says_what_it_said(bool direct)
	{
		var assembly = Compiled(Strings, direct);
		var actual   = string.Join("\n", Inputs.Select(input => Line(assembly, input)));

		// The lines of the file, whatever its line endings came to.
		Assert.True((direct ? Reader : Engine).ReplaceLineEndings("\n") == actual, $"{(direct ? "reader" : "engine")}:\n{actual}");
	}

	/// <summary>
	/// The guard stands where a called alternative begins with fixed text a later one does not: in
	/// the engine over characters, and in the rule's reading again of a token the lexer cut.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_called_alternative_that_begins_with_fixed_text_is_tested_before_it_is_gone_into(bool direct)
	{
		if (!direct)
			Assert.Contains("p -= 3;", Source(Strings, direct, lexical: false), StringComparison.Ordinal);

		Assert.Contains("p -= 3;", Source(LexicalStrings, direct, lexical: true), StringComparison.Ordinal);
	}

	/// <summary>
	/// No guard where one character says as much — a rule that begins with one fixed character, which
	/// the chain's own test before the alternative already asks — nor where every later alternative
	/// begins with the same text, so that an input failing the test has nowhere else to go; nor under
	/// tracing, where the trace shows every rule that was called.
	/// </summary>
	[Theory]
	[InlineData("Other : @string = '\"' & [^ '\"']+ & '!' => @(parserText)", true, false)]
	[InlineData("Other : @string = '\"'{3,} & [^ '\"']+ & '\"'{3} => @(parserText)", false, false)]
	[InlineData("Other : @string = '\"'{3,} & [^ '\"']+ & '\"'{3} => @(parserText)", false, true)]
	public void No_guard_where_one_character_decides_or_nothing_else_can_stand(string other, bool otherFirst, bool traced)
	{
		var grammar =
			"""
			trivia = { ' '* }
			RawText : @string = $q: '"'{3,} & [^ '"'] & ({ [^ '"']+ } | ?!$q & '"'+ & [^ '"'])* & $q => @(parserText)
			Item : @string = s: Str => @("s:" + s) | n: ['a'..'z']+ => @("n:" + n)
			Items : @string = items: Item* => @(string.Join("|", items))
			parse Items
			""" +
			"\n" + other +
			"\n" + (otherFirst ? "Str : @string = { t: Other | r: RawText } => @(t ?? r!)" : "Str : @string = { r: RawText | t: Other } => @(r ?? t!)");

		Assert.DoesNotContain("p -= ", Source(grammar, direct: false, lexical: false, traced), StringComparison.Ordinal);
	}

	/// <summary>
	/// A chain of as many alternatives as a switch needs groups is a list, and a list is not
	/// guarded: a test before every member would double it, and the method could pass the JIT's
	/// budget — a choice of nine hundred rules each beginning with its own number did.
	/// </summary>
	[Fact]
	public void A_long_chain_is_not_guarded()
	{
		const string grammar =
			"""
			trivia = none
			RawText : @string = $q: '"'{3,} & [^ '"'] & ({ [^ '"']+ } | ?!$q & '"'+ & [^ '"'])* & $q => @(parserText)
			Marked : @string = '"' & '!' & [^ '"']* & '"' => @(parserText)
			Tagged : @string = '"' & '#' & [^ '"']* & '"' => @(parserText)
			PlainText : @string = '"' & [^ '"']* & '"' => @(parserText)
			Str : @string = { r: RawText | m: Marked | g: Tagged | t: PlainText } => @(r ?? m ?? g ?? t!)
			Item : @string = s: Str => @("s:" + s) | n: ['a'..'z']+ => @("n:" + n)
			Items : @string = items: Item* => @(string.Join("|", items))
			parse Items
			""";

		Assert.DoesNotContain("p -= ", Source(grammar, direct: false, lexical: false), StringComparison.Ordinal);

		// Three of them are a chain, and the first is tested.
		var three = string.Join("\n", grammar.Split('\n').Where(static line => !line.StartsWith("Tagged", StringComparison.Ordinal)))
			.Replace(" | g: Tagged", "", StringComparison.Ordinal)
			.Replace(" ?? g", "", StringComparison.Ordinal);

		Assert.Contains("p -= 3;", Source(three, direct: false, lexical: false), StringComparison.Ordinal);
	}

	[Fact]
	public void The_guard_is_not_written_under_tracing()
	{
		Assert.DoesNotContain("p -= 3;", Source(Strings, direct: false, lexical: false, traced: true), StringComparison.Ordinal);
	}

	static string Line(Assembly assembly, string input)
	{
		var (isSuccess, value, error, position) = EmittedCode.Match(assembly, "Grammar", "TryParseItems", input);
		var outcome = EmittedCode.Outcome(assembly, "Grammar", "TryParseItems", input);

		return $"\"{Escaped(input)}\" => {outcome} at {position}: [{(isSuccess ? value : error)}]";
	}

	static string Escaped(string input)
	{
		return input.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);
	}

	static Assembly Compiled(string grammar, bool direct, bool lexical = false)
	{
		var result = Generated(grammar, direct, lexical);

		return EmittedCode.Compile(result.Sources[0].Text, sourceParts: result.Sources.Skip(1).Select(static source => source.Text));
	}

	static string Source(string grammar, bool direct, bool lexical, bool traced = false)
	{
		return Generated(grammar, direct, lexical, traced).Sources[0].Text;
	}

	static GramCompilation Generated(string grammar, bool direct, bool lexical, bool traced = false)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions
		{
			Direct        = direct,
			Lexical       = lexical,
			Trace         = traced,
			CSharpScanner = RoslynCSharpScanner.Instance,
		});

		EmittedCode.Quiet(result.Diagnostics);

		return result;
	}
}
