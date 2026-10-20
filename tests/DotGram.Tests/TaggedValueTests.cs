using System;
using System.Linq;
using System.Reflection;

using DotGram.Generation;
using DotGram.Grammar;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The value of a token of a terminal that binds text in some alternatives and not in the one
/// other — a raw string beside a plain one — is read through the other alone where the tags say no
/// binding alternative could begin the token: the tokenizer did not read such a token again, and
/// its value is not read through the choice either, but through the plain rule as a root of its own,
/// and the terminal's construction is handed it in its capture's place.
/// </summary>
public sealed class TaggedValueTests
{
	/// <summary>Strings as the expression language writes them: a raw one or a plain one, each a value.</summary>
	const string Strings =
		"""
		trivia = { ' '* }
		namespace Lexical
		{
			trivia = none
			RawText : @string   = $q: '"'{3,} & [^ '"'] & ({ [^ '"']+ } | ?!$q & '"'+ & [^ '"'])* & $q => @(parserText)
			Str : @string       = { r: RawText | t: PlainText } => @(r ?? t!)
			PlainText : @string = '"' & ?!('"'{5}) & parts: TextPart* & '"' => @(string.Concat(parts))
			TextPart : @string  = '\\' & e: any => @(e) | t: { [^ '"' | '\\']+ } => @(t)
			Name : @string      = ['a'..'z']+ => @(parserText)
		}
		Item : @string = s: Lexical.Str => @("s:" + s) | n: Lexical.Name => @("n:" + n)
		Items : @string = items: Item* => @(string.Join("|", items))
		parse Items
		""";

	/// <summary>
	/// The plain rule is read whole as a root of its own, the value is asked for through it where the
	/// tags say nothing else could stand, and every value — plain, escaped, empty, raw of three to seven
	/// quotes, and the two empty strings four quotes are — agrees with the reading over characters.
	/// </summary>
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public void A_token_the_tags_passed_is_valued_through_the_alternative_that_binds_nothing(bool direct)
	{
		var (assembly, source) = Lexical(Strings, direct);
		var twin = Compiled(Strings, direct: true);

		Assert.Contains("Reread_Lexical_PlainText_For_Lexical_Str_DotGram(", source, StringComparison.Ordinal);
		Assert.Contains("Construct_Lexical_Str(default, through)", source, StringComparison.Ordinal);

		foreach (var input in new[]
		{
			"\"ab\"", "\"\"", "\"a\\\"b\" \"c\\\\\"", "\"\"\"ab\"\"\"", "\"\"\"\"a\"\"\"b\"\"\"\"", "\"\"\"\"\"\"\"x\"\"\"\"\"\"\"",
			"\"\"\"\"", "\"ab\" x \"\"\"c\"\"\" \"\"", "x", "", "\"\" \"\" \"\"\"\"", "\"a\"\"b\"",
		})
		{
			var over   = EmittedCode.Match(twin, "Grammar", "TryParseItems", input);
			var tagged = EmittedCode.Match(assembly, "Grammar", "TryParseItems", input);

			Assert.True(over.IsSuccess == tagged.IsSuccess && Equals(over.Value, tagged.Value), $"\"{input}\": over characters {over}, tagged {tagged}");
		}

		Assert.Equal("s:ab|s:\"\"\"\"c\"\"\"\"|s:|s:a\"b", EmittedCode.Match(assembly, "Grammar", "TryParseItems", "\"ab\" \"\"\"\"c\"\"\"\" \"\" \"a\\\"b\"").Value);
	}

	/// <summary>
	/// Not where the terminal has two alternatives that bind nothing — the tags say one of them was
	/// taken, not which — nor where the construction asks for more than its captures: the value is then
	/// read as written, and still agrees.
	/// </summary>
	[Theory]
	[InlineData("Str : @string = { r: RawText | t: PlainText | n: Name } => @(r ?? t ?? n!)", "\"\"\"a\"\"\" \"b\" c")]
	[InlineData("Str : @string = { r: RawText | t: PlainText } => @(parserText + (r ?? t!))", "\"\"\"a\"\"\" \"b\"")]
	public void A_terminal_of_another_shape_is_valued_as_written(string str, string input)
	{
		var grammar = Strings.Replace("Str : @string       = { r: RawText | t: PlainText } => @(r ?? t!)", str, StringComparison.Ordinal);
		var (assembly, source) = Lexical(grammar, direct: true);
		var twin = Compiled(grammar, direct: true);

		Assert.DoesNotContain("_For_Lexical_Str_DotGram(", source, StringComparison.Ordinal);
		Assert.Equal(EmittedCode.Match(twin, "Grammar", "TryParseItems", input).Value, EmittedCode.Match(assembly, "Grammar", "TryParseItems", input).Value);
	}

	/// <summary>
	/// The rule read through is read as the atomic choice reads it — its first reading, committed
	/// to — and not whole over the token: <c>'a'+ &amp; ('a' &amp; 'b')?</c> reads <c>aa</c> of the
	/// token <c>aab</c>, and the terminal read as written then finds the token unfinished and builds
	/// nothing, as a plain terminal of the same rule does. The shortcut says the same, where a whole
	/// reading would have given back an <c>a</c> and built <c>[aab]</c>. Over characters the choice
	/// commits to <c>aa</c> and the parent takes the <c>b</c>: the token's longest match, documented.
	/// </summary>
	[Fact]
	public void The_rule_read_through_is_read_as_the_choice_commits_to_it()
	{
		const string bound =
			"""
			trivia = { ' '* }
			namespace Lexical
			{
				trivia = none
				T : @string = { r: ($q: 'x' & $q) | p: Plain } => @("[" + (r ?? p!) + "]")
				Plain : @string = 'a'+ & ('a' & 'b')? => @(parserText)
			}
			Root : @string = s: Lexical.T & 'b'? => @(s ?? "<null>")
			parse Root
			""";
		var plain = bound.Replace("{ r: ($q: 'x' & $q) | p: Plain } => @(\"[\" + (r ?? p!) + \"]\")", "{ p: Plain } => @(\"[\" + p! + \"]\")", StringComparison.Ordinal);

		var (beside, source) = Lexical(bound, direct: true);
		var (alone, _)       = Lexical(plain, direct: true);
		var twin = Compiled(bound, direct: true);

		Assert.Contains("_For_Lexical_T_DotGram(", source, StringComparison.Ordinal);
		Assert.Equal("[aa]", EmittedCode.Match(twin, "Grammar", "TryParseRoot", "aab").Value);

		foreach (var input in new[] { "aab", "aa", "a", "ab", "aaab", "aab b" })
		{
			var written = EmittedCode.Match(alone, "Grammar", "TryParseRoot", input);
			var through = EmittedCode.Match(beside, "Grammar", "TryParseRoot", input);

			Assert.True(written.IsSuccess == through.IsSuccess && Equals(written.Value, through.Value), $"\"{input}\": as written {written}, through {through}");
		}

		Assert.Equal("[aa]", EmittedCode.Match(beside, "Grammar", "TryParseRoot", "aa").Value);
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
