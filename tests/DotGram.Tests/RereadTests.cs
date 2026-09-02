using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Grammar.Binding;
using DotGram.Grammar.Emit;
using DotGram.Grammar.Model;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// A terminal that builds a value out of parts of itself, over token kinds.
/// </summary>
/// <remarks>
/// <para>
/// <c>Hex : @string = "0x"i &amp; '_'* &amp; t: HexRun =&gt; @(t.Replace("_", ""))</c> says
/// three things at once: what a hexadecimal literal looks like, which part of it is the
/// number, and that the separators come out. A lexer answers the first and hands over
/// <c>0x_1F</c> whole, so the parts the other two named are gone.
/// </para>
/// <para>
/// The answer is to read it twice — once by the lexer, for where it ends, and once by the
/// rule's own character machine over exactly that text, for what it is worth. Checked here
/// by running both parsers over the same inputs and requiring the same answer, because the
/// claim is precisely that the two build the same values.
/// </para>
/// </remarks>
public sealed class RereadTests
{
	const string Grammar =
		"""
		trivia = { ' '* }

		namespace Lexical
		{
			trivia = none

			HexRun = ['0'..'9' | 'a'..'f' | 'A'..'F'] & ('_'* & ['0'..'9' | 'a'..'f' | 'A'..'F'])*
			DecRun = ['0'..'9'] & ('_'* & ['0'..'9'])*

			// Each of these builds from a part of itself, and none of the parts survives
			// becoming one token.
			Hex  : @long = "0x"i & '_'* & t: HexRun => @(long.Parse(t.Replace("_", ""), System.Globalization.NumberStyles.HexNumber))
			Dec  : @long = t: DecRun                => @(long.Parse(t.Replace("_", "")))
			Text : @string = '"' & t: [^ '"']* & '"' => @(t)

			Name = ['a'..'z'] & ['a'..'z']*
		}

		Number : @long = h: Lexical.Hex => @(h) | d: Lexical.Dec => @(d)

		Pair : @string = k: Lexical.Name & '=' & n: Number
		    => @(k + ":" + n.ToString(System.Globalization.CultureInfo.InvariantCulture))

		Quoted : @string = t: Lexical.Text => @("<" + t + ">")

		Item : @string = a: Pair => @(a) | b: Quoted => @(b)

		Start : @string[] = (p: Item)* & eof => @(p)

		parse Start
		""";

	[Theory]
	[InlineData("a=1")]
	[InlineData("a=0x1F")]
	[InlineData("a=0x_1f b=1_000")]
	[InlineData("x=10 y=0XFF z=7")]
	[InlineData("\"hello\"")]
	[InlineData("a=1 \"two\" b=0x2")]
	[InlineData("")]
	[InlineData("a=")]
	[InlineData("a=0x")]
	[InlineData("=1")]
	[InlineData("a=1 b")]
	public void A_terminal_that_builds_says_the_same_thing_either_way(string input)
	{
		Assert.Equal(Characters(input), Kinds(input));
	}

	/// <summary>And what it says is what the grammar means, not merely the same twice.</summary>
	/// <remarks>
	/// Two parsers agreeing on <c>null</c> everywhere would pass the theory above. This is
	/// the other half: the separators come out, the base is read, and the quotes are gone —
	/// all three being things the token's own text does not say.
	/// </remarks>
	[Theory]
	[InlineData("a=0x_1f", "a:31")]
	[InlineData("a=1_000", "a:1000")]
	[InlineData("\"hi\"",  "<hi>")]
	public void And_it_says_what_the_rule_says(string input, string expected)
	{
		Assert.Equal(expected, Kinds(input));
		Assert.Equal(expected, Characters(input));
	}

	/// <summary>The split keeps such a rule, and says which ones it kept.</summary>
	[Fact]
	public void The_split_names_the_terminals_it_will_read_again()
	{
		var split = LexicalSplit.Of(Graph(Grammar));

		Assert.NotNull(split);
		Assert.Empty(split.Blocked);

		Assert.Equal(
			["Dec", "Hex", "Text"],
			split.Valued.Select(rule => rule.Name).OrderBy(name => name, StringComparer.Ordinal));

		// Kept in the syntactic graph, and kept typed — a call to one is a call, so the arena
		// records where it began and ended. What it no longer has is members: nothing the
		// syntactic machine walks builds it.
		foreach (var rule in split.Valued)
		{
			Assert.Contains(rule, split.Syntax.Rules);
			Assert.True(split.Syntax.Types.ContainsKey(rule));
			Assert.Empty(split.Syntax.Results[rule]);
		}
	}

	static RecognitionGraph Graph(string grammar) =>
		GrammarNormalizer.Normalize(
			GrammarBinder.Bind(
				GramParser.Parse(
					GramLexer.Tokenize(grammar, DotGram.Generation.RoslynCSharpScanner.Instance)).File!));

	/// <summary>What the ordinary parser makes of it.</summary>
	static string Characters(string input)
	{
		var source = EmittedCode.Compile(
			DotGram.Grammar.GramCompiler.Compile(
				Grammar,
				new DotGram.Grammar.GramCompilerOptions
				{
					ClassName     = "Grammar",
					CSharpScanner = DotGram.Generation.RoslynCSharpScanner.Instance,
				}).Sources.Single().Text);

		return Told(source.GetType("Grammar")!.GetMethod("TryParseStart", [typeof(string)])!.Invoke(null, [input])!);
	}

	/// <summary>And what the split one makes of it, reading each terminal a second time.</summary>
	static string Kinds(string input)
	{
		var split = LexicalSplit.Of(Graph(Grammar));

		Assert.NotNull(split);
		Assert.Empty(split.Blocked);

		var diagnostics = new List<DotGram.Grammar.GramDiagnostic>();
		var text = CSharpEmitter.Emit(split.Syntax, "Grammar", null, null, diagnostics, null, split);

		var grammar = EmittedCode.Compile(text).GetType("Grammar")!;

		return Told(grammar.GetMethod("TryParseStart", [typeof(string)])!.Invoke(null, [input])!);
	}

	/// <summary>A match said as one string, so that two of them can be compared.</summary>
	static string Told(object match)
	{
		var type = match.GetType();

		if (!(bool)type.GetProperty("IsSuccess")!.GetValue(match)!)
			return $"refused at {type.GetProperty("Position")!.GetValue(match)}";

		return string.Join("|", (string[])type.GetProperty("Value")!.GetValue(match)!);
	}
}
