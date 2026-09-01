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
/// The lexical machine, checked by compiling what it wrote and running it.
/// </summary>
/// <remarks>
/// The same standard the rest of the emitter is held to. Asserting on the text would say
/// the generator agrees with itself; compiling it says it is valid C#, and running it says
/// the machine recognizes what the grammar claims — including the two things a lexer is for,
/// longest match and one answer per string.
/// </remarks>
public sealed class LexerEmitterTests
{
	const string Sql =
		"""
		wordboundary = ['a'..'z' | 'A'..'Z' | '0'..'9' | '_']
		trivia = ' '*
		namespace Lexical
		{
			trivia = none
			Digits = ['0'..'9'] & ['0'..'9']*
			Number = Digits & ('.' & Digits)?
			Name   = ['a'..'z'] & ['a'..'z' | '0'..'9']*
		}
		Start = ("select" | "set") & Lexical.Name & '=' & "<=" & Lexical.Digits & Lexical.Number
		parse Start
		""";

	static (TerminalInventory Inventory, Func<string, int, (int Kind, int End)> Scan) Built(string grammar)
	{
		var split = LexicalSplit.Of(
			GrammarNormalizer.Normalize(
				GrammarBinder.Bind(
					GramParser.Parse(
						GramLexer.Tokenize(grammar, DotGram.Generation.RoslynCSharpScanner.Instance)).File!)));

		Assert.NotNull(split);
		Assert.NotNull(split.Inventory.Machine);

		var source =
			// The harness declares the other half as `public partial class`, so this half
			// matches it — the emitted code is a body and does not care what holds it.
			"public partial class Lexer\r\n{\r\n" +
			string.Join(
				"\r\n",
				LexerEmitter.Emit(split.Inventory.Machine).Replace("\r\n", "\n").Split('\n')
					.Select(line => line.Length == 0 ? line : "\t" + line)) +
			// Reflection cannot carry a span, so the wrapper takes a string and the emitted
			// method keeps the signature it will really have.
			"\r\n\tpublic static int Over(string text, int pos, out int kind) =>" +
			"\r\n\t\tScan(global::System.MemoryExtensions.AsSpan(text), pos, out kind);\r\n}\r\n";

		var method = EmittedCode.Compile(source, "Lexer").GetType("Lexer")!.GetMethod("Over")!;

		return (split.Inventory, (text, at) =>
		{
			var arguments = new object?[] { text, at, null };
			var end       = (int)method.Invoke(null, arguments)!;

			return ((int)arguments[2]!, end);
		});
	}

	/// <summary>The machine reads the longest token, and says which patterns it was.</summary>
	/// <remarks>
	/// <c>select</c> beats <c>sel</c> and beats the identifier it begins, <c>&lt;=</c> beats
	/// <c>&lt;</c>, and <c>1.5</c> beats <c>1</c> — none of it written down as a rule, all of
	/// it falling out of running to a stop and keeping the last state that accepted.
	/// </remarks>
	[Theory]
	[InlineData("select",  "\"select\"")]
	[InlineData("selects", "Name")]
	[InlineData("set",     "\"set\"")]
	[InlineData("<=",      "\"<=\"")]
	[InlineData("=",       "'='")]
	public void The_longest_match_wins(string input, string expected)
	{
		var (inventory, scan) = Built(Sql);
		var (kind, end)       = scan(input, 0);

		Assert.Equal(input.Length, end);
		Assert.Contains(expected, Patterns(inventory, kind));
	}

	/// <summary>And a token that several patterns match says all of them.</summary>
	/// <remarks>
	/// The whole of why a kind is a set. <c>10</c> is a <c>Digits</c> and a <c>Number</c>;
	/// <c>1.5</c> is only a <c>Number</c>; <c>select</c> is a keyword and, because the
	/// grammar's <c>Name</c> would have matched it, a <c>Name</c> as well — which is what lets
	/// a non-reserved word stand where a name does.
	/// </remarks>
	[Theory]
	[InlineData("10",     new[] { "Digits", "Number" })]
	[InlineData("1.5",    new[] { "Number" })]
	[InlineData("select", new[] { "\"select\"", "Name" })]
	[InlineData("abc",    new[] { "Name" })]
	public void A_kind_names_every_pattern_that_matched(string input, string[] expected)
	{
		var (inventory, scan) = Built(Sql);
		var (kind, _)         = scan(input, 0);

		Assert.Equal(expected.OrderBy(one => one), Patterns(inventory, kind).OrderBy(one => one));
	}

	/// <summary>Where nothing begins, the machine says so rather than guessing.</summary>
	[Fact]
	public void Nothing_where_nothing_begins()
	{
		var (_, scan)   = Built(Sql);
		var (kind, end) = scan("#", 0);

		Assert.Equal(0, kind);
		Assert.Equal(0, end);
	}

	/// <summary>
	/// The lexer writes nothing into an arena, which is the signal the boundary is right.
	/// </summary>
	/// <remarks>
	/// Not an optimization. A lexical machine that needed a way back would mean a pattern had
	/// been admitted that is not a regular language, and the split would be wrong somewhere
	/// upstream of here. It is cheaper to check than any benchmark and it fails loudly.
	/// </remarks>
	[Fact]
	public void The_lexer_writes_no_arena()
	{
		var split = LexicalSplit.Of(
			GrammarNormalizer.Normalize(
				GrammarBinder.Bind(
					GramParser.Parse(
						GramLexer.Tokenize(Sql, DotGram.Generation.RoslynCSharpScanner.Instance)).File!)));

		var source = LexerEmitter.Emit(split!.Inventory.Machine!);

		Assert.DoesNotContain("entries", source, StringComparison.Ordinal);
		Assert.DoesNotContain("ParserEntry", source, StringComparison.Ordinal);

		// And the wide sets are fields rather than expressions. Inline they were an
		// allocation per character per test, and the generated lexer came out seventeen times
		// slower than the hand-written one it replaced.
		Assert.DoesNotContain("new char[]", source, StringComparison.Ordinal);
	}

	static string[] Patterns(TerminalInventory inventory, int kind) =>
		[.. inventory.Kinds.Single(one => one.Number == kind).Matched.Select(one => one.ToString())];
}
