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
/// A grammar that builds values, run over token kinds.
/// </summary>
/// <remarks>
/// <para>
/// The one thing a split changes about the machine: a position is a token and not a
/// character, so nothing may cut a value out of what it is reading. The text and the
/// extents travel beside the kinds and every value goes through them.
/// </para>
/// <para>
/// Checked by running, and against the character parser for the same grammar rather than
/// against an expectation typed here — the two must agree about the value <em>and</em>
/// about where a refusal happened, and the second is what caught the first attempt: it
/// measured the arrays' length rather than the token count, and every refusal came back at
/// character zero while the character parser said six and fourteen.
/// </para>
/// </remarks>
public sealed class ProvenanceTests
{
	const string Grammar =
		"""
		wordboundary = ['a'..'z']
		trivia = { ' '* }

		namespace Lexical
		{
			trivia = none
			Name   = ['a'..'z'] & ['a'..'z']*
			Digits = ['0'..'9'] & ['0'..'9']*
		}

		Pair  : @string   = k: Lexical.Name & '=' & v: Lexical.Digits => @(k + ":" + v)
		Start : @string[] = (p: Pair)* & eof => @(p)

		parse Start
		""";

	[Theory]
	[InlineData("a=1")]
	[InlineData("a=1 bb=22")]
	[InlineData("  x=9  y=10  ")]
	[InlineData("")]
	[InlineData("=1")]
	[InlineData("a=1 b=")]
	[InlineData("a=1 bb=22 ccc=")]
	public void A_split_grammar_builds_what_the_character_one_builds(string input)
	{
		var chars = Characters(input);
		var kinds = Kinds(input);

		Assert.Equal(chars, kinds);
	}

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

		var method = source.GetType("Grammar")!.GetMethod("TryParseStart", [typeof(string)])!;

		return Told(method.Invoke(null, [input])!);
	}

	/// <summary>And what the split one makes of the same input, tokens and all.</summary>
	static string Kinds(string input)
	{
		var split = LexicalSplit.Of(
			GrammarNormalizer.Normalize(
				GrammarBinder.Bind(
					GramParser.Parse(
						GramLexer.Tokenize(Grammar, DotGram.Generation.RoslynCSharpScanner.Instance)).File!)));

		Assert.NotNull(split);
		Assert.Empty(split.Blocked);

		var diagnostics = new List<DotGram.Grammar.GramDiagnostic>();

		// One file with both halves in it, which is what a split grammar is now: the
		// publication tokenizes and then parses, and the caller hands over a string.
		var text = CSharpEmitter.Emit(split.Syntax, "Grammar", null, null, diagnostics, null, split);

		var grammar = EmittedCode.Compile(text).GetType("Grammar")!;
		var method  = grammar.GetMethod("TryParseStart", [typeof(string)])!;

		return Told(method.Invoke(null, [input])!);
	}

	/// <summary>A match said as one string, so that two of them can be compared.</summary>
	static string Told(object match)
	{
		var type = match.GetType();

		var succeeded = (bool)type.GetProperty("IsSuccess")!.GetValue(match)!;
		var position  = type.GetProperty("Position")!.GetValue(match);

		if (!succeeded)
			return $"refused at {position}";

		var value = (string[])type.GetProperty("Value")!.GetValue(match)!;

		return string.Join("|", value);
	}
}
