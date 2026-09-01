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
		trivia = ' '*

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

		// Both halves in one class: the scanner the automaton wrote, and the syntactic
		// machine the ordinary emitter wrote from the rewritten graph.
		var text =
			CSharpEmitter.Emit(split.Syntax, "Grammar", null, null, diagnostics, null, overKinds: true)
				.TrimEnd().TrimEnd('}') +
			string.Join(
				"\r\n",
				LexerEmitter.Emit(split.Inventory.Machine!, "_Lex").Replace("\r\n", "\n").Split('\n')
					.Select(line => line.Length == 0 ? line : "\t" + line)) +
			// Reflection cannot carry a span, so a string door is opened onto the scanner.
			"\r\n\tpublic static int Over(string text, int pos, out int kind) =>" +
			"\r\n\t\tScan_Lex(global::System.MemoryExtensions.AsSpan(text), pos, out kind);\r\n}\r\n";

		var compiled = EmittedCode.Compile(text);
		var grammar  = compiled.GetType("Grammar")!;
		var scan     = grammar.GetMethod("Over")!;

		var (tokens, starts, lengths) = Scanned(input, (source, at) =>
		{
			var arguments = new object?[] { source, at, null };
			var end       = (int)scan.Invoke(null, arguments)!;

			return ((int)arguments[2]!, end);
		});

		var method = grammar.GetMethod(
			"TryParseStart",
			[typeof(string), typeof(string), typeof(int[]), typeof(int[])])!;

		return Told(method.Invoke(null, [input, tokens, starts, lengths])!);
	}

	/// <summary>
	/// The tokens, trivia skipped by hand.
	/// </summary>
	/// <remarks>
	/// The last hand-written thing, and it is here rather than generated because trivia is
	/// skipped and not reported: it is no pattern and has no kind. Giving it to the machine
	/// too is what wires a split grammar to a one-string entry point, and is not done yet.
	/// </remarks>
	static (string Kinds, int[] Starts, int[] Lengths) Scanned(
		string text, Func<string, int, (int Kind, int End)> scan)
	{
		var kinds   = new char[text.Length + 1];
		var starts  = new int[text.Length + 1];
		var lengths = new int[text.Length + 1];
		var count   = 0;
		var p       = 0;

		while (true)
		{
			while (p < text.Length && text[p] == ' ')
				p++;

			if (p >= text.Length)
				break;

			var (kind, end) = scan(text, p);

			if (kind == 0 || end <= p)
				break;

			kinds  [count] = (char)kind;
			starts [count] = p;
			lengths[count] = end - p;
			count++;

			p = end;
		}

		return (new string(kinds, 0, count), starts, lengths);
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
