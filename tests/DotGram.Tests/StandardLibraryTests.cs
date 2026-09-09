using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Parsing;

using Microsoft.CodeAnalysis;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// The standard library (§5.2): a grammar that writes <c>using Std;</c> has its lexemes,
/// and one that does not has nothing of it.
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
				using Std;

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
				using Std;

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

	[Fact]
	public void Numbers_are_read_as_the_invariant_culture_reads_them()
	{
		var built = GeneratorDriverTests.Build(""""
			using DotGram;

			[Gram("""
				using Std;

				Value : @double = d: Std.Double & eof => @(d)
				parse Value
				""")]
			public static partial class Doubles;
			"""");

		var parse = built.GetType("Doubles")!.GetMethod("ParseValue", [typeof(string)])!;

		Assert.Equal(1.5e3, parse.Invoke(null, ["1.5e3"]));
	}

	/// <summary>
	/// Without the <c>using</c> the library is not there: a name that happens to be one of
	/// its is undefined, as it would be in any grammar that never wrote it.
	/// </summary>
	[Fact]
	public void Without_the_using_the_library_is_not_there()
	{
		var compiled = GramCompiler.Compile("""
			Start = Std.Integer
			parse Start
			""", Scanning);

		var diagnostic = Assert.Single(compiled.Diagnostics.Where(static one => one.Severity == GramSeverity.Error));

		Assert.Equal(Grammar.Binding.GrammarBinder.UndefinedName, diagnostic.Id);
	}

	/// <summary>
	/// Nothing is said about the lexemes a grammar does not call: the whole library comes
	/// along for one rule of it, and the rest being unreached is the expected case.
	/// </summary>
	[Fact]
	public void The_lexemes_a_grammar_does_not_call_are_not_remarked_on()
	{
		var compiled = GramCompiler.Compile("""
			using Std;

			Start = Std.Digits & eof
			parse Start
			""", Scanning);

		Assert.Empty(compiled.Diagnostics);
	}

	/// <summary>
	/// A grammar that asks for the library keeps it when it is included by another: the
	/// <c>using</c> travels inside the namespace it is spliced into, and the includer's
	/// compilation answers it the same way.
	/// </summary>
	[Fact]
	public void An_included_grammar_keeps_its_library()
	{
		var built = GeneratorDriverTests.Build(""""
			using DotGram;

			[Gram("""
				using Std;

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
