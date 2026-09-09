using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Parsing;

using Microsoft.CodeAnalysis;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The standard library (§5.2): every grammar has it under <c>Std</c>, and <c>using Std;</c>
/// opens the namespace as it opens any other.
/// </summary>
public sealed class StandardLibraryTests
{
	/// <summary>The library carries C#, so reading it takes a scanner that reads C#.</summary>
	static GramCompilerOptions Scanning => new() { CSharpScanner = RoslynCSharpScanner.Instance };

	[Fact]
	public void A_grammar_that_asks_for_the_library_reads_its_lexemes()
	{
		var built = GeneratorDriverTests.Build(""""
			using DotGram;

			[Gram("""
				trivia = Std.Spacing?

				Sum : @int = a: Std.Integer & '+' & b: Std.Integer & eof => @(a + b)
				parse Sum
				""")]
			public static partial class Adds;
			"""");

		var parse = built.GetType("Adds")!.GetMethod("ParseSum", [typeof(string)])!;

		Assert.Equal(42, parse.Invoke(null, ["40 + 2"]));
	}

	/// <summary>
	/// A comment and a string are parameterized by what delimits them, which is all that
	/// differs between one language's and the next.
	/// </summary>
	[Fact]
	public void Delimiters_are_the_calling_grammars_to_choose()
	{
		var built = GeneratorDriverTests.Build(""""
			using DotGram;

			[Gram("""
				trivia = { (Std.Spacing | Std.LineComment("--") | Std.BlockComment("/*", "*/"))* }

				Two : @string[] = a: Std.Quoted('\'') & b: Std.Escaped('"', '\\') & eof => @(new[] { a, b })
				parse Two
				""")]
			public static partial class Strings;
			"""");

		var parse = built.GetType("Strings")!.GetMethod("ParseTwo", [typeof(string)])!;
		var read  = (string[])parse.Invoke(null, ["/* a */ 'it''s' -- and\n \"say \\\"hi\\\"\""])!;

		// As written, escapes and all: what an escape stands for is the language's business.
		Assert.Equal(["'it''s'", "\"say \\\"hi\\\"\""], read);
	}

	/// <summary>
	/// A string is one token in a grammar read over tokens, and reads the same over
	/// characters — the doubled quote, the escaped one, the escaped escape, and the
	/// unterminated string that is no string at all.
	/// </summary>
	/// <remarks>
	/// The shapes here are what the lexer's until machine was taught for the library: two
	/// idioms in one pattern, an idiom nested in a repetition, and an escape pair as the
	/// item of one. Each of those was refused, or read wrongly, before it was.
	/// </remarks>
	[Theory]
	[InlineData(true,  "'it''s'",          "'it''s'")]
	[InlineData(false, "'it''s'",          "'it''s'")]
	[InlineData(true,  "\"a\\\"b\\\\\"",   "\"a\\\"b\\\\\"")]
	[InlineData(false, "\"a\\\"b\\\\\"",   "\"a\\\"b\\\\\"")]
	[InlineData(true,  "'abc",             null)]
	[InlineData(false, "'abc",             null)]
	[InlineData(true,  "\"a\\\"",          null)]
	[InlineData(false, "\"a\\\"",          null)]
	public void A_string_reads_the_same_over_tokens_and_over_characters(bool lexical, string input, string? expected)
	{
		var built = GeneratorDriverTests.Build($$""""
			using DotGram;

			[Gram("""
				trivia = { Std.Spacing* }

				Text : @string = s: (Std.Quoted('\'') | Std.Escaped('"', '\\')) & eof => @(s)
				parse Text
				""", Lexical = {{(lexical ? "true" : "false")}})]
			public static partial class Strings;
			"""");

		var parse = built.GetType("Strings")!.GetMethod("TryParseText", [typeof(string)])!;
		var read  = parse.Invoke(null, [input])!;
		var ok    = (bool)read.GetType().GetProperty("IsSuccess")!.GetValue(read)!;

		Assert.Equal(expected is not null, ok);

		if (expected is not null)
			Assert.Equal(expected, read.GetType().GetProperty("Value")!.GetValue(read));
	}

	[Fact]
	public void Numbers_are_read_as_the_invariant_culture_reads_them()
	{
		var built = GeneratorDriverTests.Build(""""
			using DotGram;

			[Gram("""
				Value : @double = d: Std.Double & eof => @(d)
				parse Value
				""")]
			public static partial class Doubles;
			"""");

		var parse = built.GetType("Doubles")!.GetMethod("ParseValue", [typeof(string)])!;

		Assert.Equal(1.5e3, parse.Invoke(null, ["1.5e3"]));
	}

	/// <summary>
	/// The library is there with nothing declared, named in full; a <c>using</c> opens it,
	/// and then a name of its is a name of the grammar's — on the terms every <c>using</c>
	/// has: at the top of the file the grammar's own rule wins over the opened one, and a
	/// namespace that opens the library and declares one of its names is refused (§5).
	/// </summary>
	[Fact]
	public void The_library_is_named_in_full_and_a_using_opens_it()
	{
		EmittedCode.Quiet(GramCompiler.Compile("""
			Start = Std.Integer & eof
			parse Start
			""", Scanning).Diagnostics);

		EmittedCode.Quiet(GramCompiler.Compile("""
			using Std;

			Start = Integer & eof
			parse Start
			""", Scanning).Diagnostics);

		EmittedCode.Quiet(GramCompiler.Compile("""
			using Std;

			Integer = ['0'..'9']+
			Start   = Integer & eof
			parse Start
			""", Scanning).Diagnostics);

		var nested = GramCompiler.Compile("""
			namespace Own
			{
				using Std;

				Integer = ['0'..'9']+
			}

			Start = Own.Integer & eof
			parse Start
			""", Scanning);

		Assert.Contains(nested.Diagnostics, static one => one.Id == Grammar.Binding.GrammarBinder.ShadowsEnclosingRule);
	}

	/// <summary>
	/// Nothing is said about the lexemes a grammar does not call, and nothing of them is
	/// generated: the whole library comes along for every grammar, and the unreached
	/// remainder is the expected case.
	/// </summary>
	[Fact]
	public void The_lexemes_a_grammar_does_not_call_leave_no_trace()
	{
		var compiled = GramCompiler.Compile("""
			Start = Std.Digits & eof
			parse Start
			""", Scanning);

		EmittedCode.Quiet(compiled.Diagnostics);

		var written = string.Concat(compiled.Sources.Select(static one => one.Text));

		Assert.Contains("Digits", written, StringComparison.Ordinal);
		Assert.DoesNotContain("Identifier", written, StringComparison.Ordinal);
		Assert.DoesNotContain("BlockComment", written, StringComparison.Ordinal);
	}

	/// <summary>
	/// A grammar that uses the library keeps it when it is included by another: the
	/// includer's compilation has the same library, and a name in the included grammar
	/// resolves to it from inside the namespace it was spliced into.
	/// </summary>
	[Fact]
	public void An_included_grammar_keeps_its_library()
	{
		var built = GeneratorDriverTests.Build(""""
			using DotGram;

			[Gram("""
				Count : @int = n: Std.Integer => @(n)
				parse Count
				""")]
			public static partial class Counts;

			[GramInclude(typeof(Counts), As = "C")]
			[Gram("""
				using C;

				Pair : @int = a: C.Count & ',' & b: C.Count & eof => @(a * b)
				parse Pair
				""")]
			public static partial class Pairs;
			"""");

		var parse = built.GetType("Pairs")!.GetMethod("ParsePair", [typeof(string)])!;

		Assert.Equal(12, parse.Invoke(null, ["3,4"]));
	}

	/// <summary>
	/// The library the documentation lists is the library: every rule in §5.2's table is in
	/// <c>Std.gram</c>, and every rule in <c>Std.gram</c> is in the table.
	/// </summary>
	[Fact]
	public void The_documentation_lists_the_library()
	{
		var parsed = GramParser.Parse(GramLexer.Tokenize(StandardLibrary.Text, RoslynCSharpScanner.Instance));

		Assert.Empty(parsed.Diagnostics);

		var declared = parsed.File.Decls
			.OfType<Decl.Rule>()
			.Select(static rule => rule.Name)
			.Where(static name => name != "trivia")
			.OrderBy(static name => name, StringComparer.Ordinal)
			.ToArray();

		var document = System.IO.File.ReadAllText(ContentsTests.Specification);
		var start    = document.IndexOf("### 5.2 ", StringComparison.Ordinal);
		var end      = document.IndexOf("\n## ", start, StringComparison.Ordinal);
		var section  = document.Substring(start, end - start);

		var listed = section
			.Split('\n')
			.Where(static line => line.StartsWith("| `", StringComparison.Ordinal))
			.Select(static line => line.Substring(3, line.IndexOf('`', 3) - 3))
			.Select(static cell => cell.IndexOf('(') is var at && at >= 0 ? cell.Substring(0, at) : cell)
			.OrderBy(static name => name, StringComparer.Ordinal)
			.ToArray();

		Assert.Equal(declared, listed);
	}
}
