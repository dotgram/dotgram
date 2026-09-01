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

	/// <summary>
	/// "Everything up to a delimiter" is a pattern, though a lookahead is not.
	/// </summary>
	/// <remarks>
	/// <para>
	/// <c>'"' &amp; (?!'"' &amp; any)* &amp; '"'</c> is how a string literal is written in
	/// half the grammars there are, and <c>"/*" &amp; (?!"*/" &amp; any)* &amp; "*/"</c> is
	/// how a comment is written in all of them. The language is regular — strings in which
	/// the delimiter does not occur — but it is not one a Thompson construction reaches by
	/// following the shape, because the shape is a lookahead.
	/// </para>
	/// <para>
	/// So the idiom is recognized and built as Knuth, Morris and Pratt's automaton with its
	/// accepting state taken out, which is exactly "the delimiter does not occur". A
	/// multi-character delimiter is the case that needs the failure links: after <c>*</c>
	/// and then another <c>*</c>, one <c>*</c> is still matched.
	/// </para>
	/// </remarks>
	[Theory]
	[InlineData("/**/",       4)]
	[InlineData("/* a */",    7)]
	[InlineData("/* a * b */", 11)]
	[InlineData("/*a**/",     6)]
	[InlineData("/* a */ b",  7)]
	[InlineData("/* a",       0)]
	public void Everything_up_to_a_delimiter_is_a_pattern(string input, int expected)
	{
		var (_, scan) = Built(
			"""
			trivia = ' '*
			namespace Lexical
			{
				trivia = none
				Comment = "/*" & (?!"*/" & any)* & "*/"
			}
			Start = Lexical.Comment & ';'
			parse Start
			""");

		var (kind, end) = scan(input, 0);

		Assert.Equal(expected, end);
		Assert.Equal(expected > 0, kind != 0);
	}

	/// <summary>
	/// The states are divided into methods, because the JIT reads blocks and not lines.
	/// </summary>
	/// <remarks>
	/// Written as one method, <c>SqlStandard92</c>'s scanner was 26,637 bytes of IL and the
	/// runtime answered <c>Tier-0 switched MinOpts</c>: never optimized, and recompiled worse
	/// rather than better. Divided, no method in the file is compiled that way, and the split
	/// grammar went from losing on two of nine inputs to winning on all eighteen.
	/// </remarks>
	[Fact]
	public void The_states_are_divided_into_methods()
	{
		var split = LexicalSplit.Of(
			GrammarNormalizer.Normalize(
				GrammarBinder.Bind(
					GramParser.Parse(
						GramLexer.Tokenize(Sql, DotGram.Generation.RoslynCSharpScanner.Instance)).File!)));

		var machine = split!.Inventory.Machine!;
		var source  = LexerEmitter.Emit(machine);

		// This grammar is small enough for one part; what is asserted is that the division
		// exists and follows the states, not that this scanner needed it.
		Assert.Contains("Scan_Part0(", source, StringComparison.Ordinal);

		var parts = Enumerable
			.Range(0, machine.Next.Count)
			.Count(state => source.Contains($"static int Scan_Part{state}(", StringComparison.Ordinal));

		Assert.Equal((machine.Next.Count + 95) / 96, parts);
	}

	/// <summary>
	/// A state whose ways out sit near each other is read out of a row, not asked about.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The licence is determinism: each character belongs to exactly one atom and each atom
	/// leads to one state, so no character satisfies two of a state's tests and the chain's
	/// order carries no meaning. What that buys is that any subset may be lifted into a row
	/// with the rest left where it was.
	/// </para>
	/// <para>
	/// Asserted on the text here, unlike everything else in this file, because the claim is
	/// about the shape of the code and not about what it recognizes — every other test in this
	/// file already says the machine still answers the same. What is checked is that a wide
	/// set is <em>not</em> in a row, which is the whole of what keeps this from becoming the
	/// dense table the design rejected.
	/// </para>
	/// </remarks>
	[Fact]
	public void A_state_whose_ways_out_are_near_each_other_becomes_a_row()
	{
		var split = LexicalSplit.Of(
			GrammarNormalizer.Normalize(
				GrammarBinder.Bind(
					GramParser.Parse(
						GramLexer.Tokenize(
							"""
							trivia = ' '*
							namespace Lexical
							{
								trivia = none
								Name = ['a'..'z'] & ['a'..'z']*
								Text = '"' & [^ '"']* & '"'
							}
							Start = Lexical.Name & '=' & Lexical.Text & eof
							parse Start
							""",
							DotGram.Generation.RoslynCSharpScanner.Instance)).File!)));

		var source = LexerEmitter.Emit(split!.Inventory.Machine!);

		Assert.Contains("static readonly short[] Scan_Row0 =", source, StringComparison.Ordinal);
		Assert.Contains("Scan_Row0[c - ", source, StringComparison.Ordinal);

		// `[^ '"']` is two ranges reaching the top of the plane. A row for that would be most
		// of one, so it stays a comparison — and stays correct, since the row it sits beside
		// answers -1 for every character it does not hold.
		Assert.Contains("if (c >= ", source, StringComparison.Ordinal);
	}

	/// <summary>And one comparison is never traded for a subtraction and a load.</summary>
	/// <remarks>
	/// A state with one single-character way out has one comparison to make. The row costs
	/// three operations and a field, so it is not built — the threshold is on what the chain
	/// would ask, not on how many ways there are.
	/// </remarks>
	[Fact]
	public void But_a_single_character_is_still_a_comparison()
	{
		var split = LexicalSplit.Of(
			GrammarNormalizer.Normalize(
				GrammarBinder.Bind(
					GramParser.Parse(
						GramLexer.Tokenize(
							"""
							trivia = ' '*
							Start = "->" & '(' & eof
							parse Start
							""",
							DotGram.Generation.RoslynCSharpScanner.Instance)).File!)));

		var source = LexerEmitter.Emit(split!.Inventory.Machine!);

		// The first state admits `-` and `(`, which is two comparisons and so a row. The one
		// after `-` admits `>` and nothing else, which is one.
		Assert.Contains("static readonly short[] Scan_Row0 =", source, StringComparison.Ordinal);
		Assert.Contains("if (c == '>') return", source, StringComparison.Ordinal);
	}

	static string[] Patterns(TerminalInventory inventory, int kind) =>
		[.. inventory.Kinds.Single(one => one.Number == kind).Matched.Select(one => one.ToString())];
}
