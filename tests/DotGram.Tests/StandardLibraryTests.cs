using System;
using System.Collections.Generic;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Parsing;
using DotGram.Language;

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

	/// <summary>
	/// Nothing the author left open can be closed by the library that follows: it is read after
	/// their text, not as a continuation of it, so every report stays inside what they wrote.
	/// </summary>
	[Theory]
	[InlineData("A = 'a' /* never closed", GramLexer.UnterminatedComment, 8)]
	[InlineData("A = 'a' /*",              GramLexer.UnterminatedComment, 8)]
	[InlineData("A = 'a' /* *",            GramLexer.UnterminatedComment, 8)]
	[InlineData("A = \"abc\\",            GramLexer.UnterminatedString,  4)]
	[InlineData("A = '\\",                 GramLexer.UnterminatedCharacter, 4)]
	[InlineData("A = '\\u00",              GramLexer.UnterminatedCharacter, 4)]
	[InlineData("A = [\\p{Lu",             GramLexer.MalformedCategory,   5)]
	[InlineData("A = @(Foo(1, \"x\"",      GramLexer.UnterminatedExpression, 4)]
	[InlineData("A = @(Foo(1",             GramLexer.UnterminatedExpression, 4)]
	public void An_unterminated_construct_is_reported_inside_the_authors_text(string grammar, string id, int position)
	{
		var compiled = GramCompiler.Compile(grammar, Scanning);

		Assert.Contains(compiled.Diagnostics, d => d.Id == id && d.Position == position);
		Assert.All(compiled.Diagnostics, d => AssertInside(d, grammar.Length));
	}

	[Theory]
	[InlineData("A = ('a'")]
	[InlineData("A = ['a'")]
	[InlineData("A = 'a'{2")]
	[InlineData("A = ('a' | ")]
	[InlineData("A = 'a' &")]
	[InlineData("A = \"x\" $")]
	[InlineData("A = 'a'; namespace N {")]
	public void An_open_bracket_or_operator_does_not_reach_into_the_library(string grammar)
	{
		var compiled = GramCompiler.Compile(grammar, Scanning);

		Assert.NotEmpty(compiled.Diagnostics);
		Assert.All(compiled.Diagnostics, d => AssertInside(d, grammar.Length));
	}

	/// <summary>
	/// A diagnostic ends inside the text it is about. The parser marks a missing token at the end
	/// of the file with a one-character extent starting at the end, which is allowed.
	/// </summary>
	static void AssertInside(GramDiagnostic d, int length)
	{
		var atEnd = d.Position == length && d.Length <= 1;

		Assert.True(
			atEnd || d.Position + d.Length <= length,
			$"{d.Id} at {d.Position}+{d.Length} of {length}: {d.Message}");
	}

	[Fact]
	public void The_library_after_malformed_text_keeps_its_own_positions()
	{
		foreach (var grammar in new[] { "A = ('a'", "A = 'a' /* open", "A = \"x\\", "A = @(F(1" })
		{
			var text   = StandardLibrary.SplicedOnto(grammar);
			var parsed = GramCompiler.Read(grammar, RoslynCSharpScanner.Instance);
			var std    = Assert.Single(parsed.File.Decls.OfType<Decl.Namespace>(), n => n.Name == StandardLibrary.Name);

			Assert.Equal(text.IndexOf("namespace Std", StringComparison.Ordinal), std.At.Position);
			Assert.NotEmpty(std.Decls);

			foreach (var rule in std.Decls.OfType<Decl.Rule>())
				Assert.StartsWith(rule.Name, text.Substring(rule.At.Position), StringComparison.Ordinal);

			Assert.All(parsed.Diagnostics, d => AssertInside(d, grammar.Length));
		}
	}

	[Fact]
	public void Both_tokenizers_refuse_null()
	{
		Assert.Throws<ArgumentNullException>(() => GramLexer.Tokenize(null!));
		Assert.Throws<ArgumentNullException>(() => GramLexer.Tokenize(null!, null, 0, null));
		Assert.Throws<ArgumentNullException>(() => GramLexer.Tokenize(null!, null, (IReadOnlyList<int>?)null, 0, null));
	}

	/// <summary>
	/// Grammars the host included sit between the author's text and the library, in wrappers of
	/// their own; what the author left open is not closed by them.
	/// </summary>
	[Theory]
	[InlineData("A = 'a' /* open", GramLexer.UnterminatedComment)]
	[InlineData("A = ('a'",        "GRAM2001")]
	[InlineData("A = \"x\\",       GramLexer.UnterminatedString)]
	public void An_included_grammar_is_not_reached_by_the_authors_open_construct(string own, string id)
	{
		const string included = "Word = ['a'..'z']+ /* closed */\n";

		var (text, _) = GrammarSplice.Join(
			new GrammarSplice.Part(own, null, null), [new GrammarSplice.Part(included, "Base", null)]);

		var options = new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance, Own = own.Length };
		var read    = GramCompiler.Read(text, options.CSharpScanner, options.Own);
		var start   = text.IndexOf("namespace Base", StringComparison.Ordinal);
		var included_   = Assert.Single(read.File.Decls.OfType<Decl.Namespace>(), n => n.Name == "Base");

		Assert.Contains(read.Diagnostics, d => d.Id == id && d.Position <= own.Length);
		Assert.All(read.Diagnostics, d => AssertInside(d, own.Length));
		Assert.Equal(start, included_.At.Position);

		var word = Assert.Single(included_.Decls.OfType<Decl.Rule>());

		Assert.Equal("Word", word.Name);
		Assert.Equal(text.IndexOf("Word =", StringComparison.Ordinal), word.At.Position);

		Assert.Contains(GramCompiler.Compile(text, options).Diagnostics, d => d.Id == id);
	}

	/// <summary>
	/// An included grammar that leaves a construct open ends it at its own end, and the next one
	/// is read whole — for the compiler and for the editor alike.
	/// </summary>
	[Theory]
	[InlineData("W1 = 'a' /* open\n", GramLexer.UnterminatedComment)]
	[InlineData("W1 = ('a'\n",        "GRAM2001")]
	[InlineData("W1 = \"x\\\n",       GramLexer.UnterminatedString)]
	public void An_included_grammar_does_not_reach_the_next_one(string first, string id)
	{
		const string own    = "A = 'a'\n";
		const string second = "W2 = ['a'..'z']+\n";

		var (text, map) = GrammarSplice.Join(
			new GrammarSplice.Part(own, null, null),
			[new GrammarSplice.Part(first, "One", null), new GrammarSplice.Part(second, "Two", null)]);

		var cuts = GrammarSplice.Cuts(text, map.Segments.Skip(1).Select(one => (one.Start, one.Length)));

		Assert.Equal(map.Segments[2].Start - "\nnamespace Two\n{\n".Length, cuts[0]);

		var read = GramCompiler.Read(text, RoslynCSharpScanner.Instance, own.Length, cuts);
		var two  = Assert.Single(read.File.Decls.OfType<Decl.Namespace>(), n => n.Name == "Two");
		var rule = Assert.Single(two.Decls.OfType<Decl.Rule>());

		Assert.Equal("W2", rule.Name);
		Assert.Equal(text.IndexOf("W2 =", StringComparison.Ordinal), rule.At.Position);
		Assert.Contains(read.Diagnostics, d => d.Id == id && d.Position >= map.Segments[1].Start && d.Position <= cuts[0]);
		Assert.DoesNotContain(read.Diagnostics, d => d.Position > cuts[0] && d.Position < text.Length);

		var options = new GramAnalysisOptions { Own = own.Length, Cuts = cuts };
		var tokens  = GramLexer.Tokenize(text, RoslynCSharpScanner.Instance, [own.Length, .. cuts], text.Length, null);

		Assert.Contains(tokens.Tokens, t => t.Kind == TokenKind.Identifier && t.Value == "W2");
		Assert.Contains(GramLanguageService.Analyze(text, options).Symbols, item => item.Name == "W2");
	}

	/// <summary>In the editor an author's open comment ends with their text, and the included symbols stay.</summary>
	[Fact]
	public void The_editor_reads_the_authors_open_comment_apart_from_the_included_grammars()
	{
		const string own = "A = Base.Word /* open\n";

		var (text, map) = GrammarSplice.Join(
			new GrammarSplice.Part(own, null, null),
			[new GrammarSplice.Part("Word = ['a'..'z']+\n", "Base", null)]);

		var document = GramLanguageService.Analyze(
			text,
			new GramAnalysisOptions
			{
				Own  = own.Length,
				Cuts = GrammarSplice.Cuts(text, map.Segments.Skip(1).Select(one => (one.Start, one.Length))),
			});

		Assert.Contains(document.Symbols, item => item.Name == "Word");
		Assert.Contains(document.Diagnostics, d => d.Id == GramLexer.UnterminatedComment);
	}
}
