using System;
using System.Linq;

using DotGram.Generation;
using DotGram.Grammar;
using DotGram.Grammar.Binding;
using DotGram.Grammar.Parsing;

using Xunit;

namespace DotGram.Tests;

/// <summary>
/// Which <c>trivia</c> a namespace sees does not depend on where its <c>using</c> is written
/// (§4.5): the namespace's own chain first — itself, then outwards — and only where none of
/// those declares one, what the chain imports.
/// </summary>
/// <remarks>
/// A <c>using</c> at the top of the file and one at the top of a <c>namespace</c> block bring
/// in names alike (§5), so they bring in <c>trivia</c> alike. The chain comes first so that
/// §4.5's own example holds at either placement: a syntax namespace that imports a lexical one
/// to call its tokens keeps the whitespace the grammar around it declared, and is not handed
/// the lexical namespace's <c>trivia = none</c> with them.
/// </remarks>
public sealed class UsingTriviaPlacementTests
{
	/// <summary>The global namespace and Syntax declare no trivia; Lexical declares a space-skipping one.</summary>
	const string Lexical = "namespace Lexical { trivia = { ' '* }\n Word = ['a'..'z']+ & '.' }\n";

	const string AtTheTop =
		"using Lexical;\n" + Lexical +
		"Start = Word & Word & eof\n" +
		"parse Start\n";

	const string Nested =
		Lexical +
		"namespace Syntax { using Lexical;\n Start = Word & Word & eof }\n" +
		"parse Syntax.Start\n";

	[Fact]
	public void A_using_at_the_top_of_the_file_brings_in_the_imported_trivia()
	{
		var model = Bind(AtTheTop);

		Assert.Same(Declared(model, "Lexical"), model.Trivia[model.Root]);
		Assert.True(Accepts(AtTheTop, "Start", "a. b."));
	}

	[Fact]
	public void A_using_inside_a_namespace_brings_in_the_imported_trivia_the_same_way()
	{
		var model  = Bind(Nested);
		var syntax = model.Root.Nested.Single(static one => one.Name == "Syntax");

		Assert.Same(Declared(model, "Lexical"), model.Trivia[syntax]);
		Assert.True(Accepts(Nested, "Syntax.Start", "a. b."));
	}

	/// <summary>
	/// §4.5's example: the grammar declares whitespace, and the syntax namespace imports a
	/// lexical one that declares none. The enclosing declaration wins at either placement.
	/// </summary>
	[Fact]
	public void The_namespace_chain_comes_before_the_imports_at_either_placement()
	{
		var top =
			"using Lexical;\n" +
			"trivia = { ' '* }\n" +
			"namespace Lexical { trivia = none\n Word = ['a'..'z']+ }\n" +
			"Start = Word & Word & eof\n" +
			"parse Start\n";

		var nested =
			"trivia = { ' '* }\n" +
			"namespace Lexical { trivia = none\n Word = ['a'..'z']+ }\n" +
			"namespace Syntax { using Lexical;\n Start = Word & Word & eof }\n" +
			"parse Syntax.Start\n";

		var model  = Bind(nested);
		var syntax = model.Root.Nested.Single(static one => one.Name == "Syntax");

		Assert.Same(model.Root.Rules["trivia"], model.Trivia[syntax]);
		Assert.True(Accepts(top, "Start", "ab cd"));
		Assert.True(Accepts(nested, "Syntax.Start", "ab cd"));
	}

	/// <summary>An inner namespace's own import is nearer than one written outside it.</summary>
	[Fact]
	public void The_nearest_import_wins()
	{
		var model = Bind(
			"using Far;\n" +
			"namespace Far { trivia = { ' '* } }\n" +
			"namespace Near { trivia = { '_'* } }\n" +
			"namespace Syntax { using Near;\n Start = 'a' & 'b' }\n" +
			"parse Syntax.Start\n");

		var syntax = model.Root.Nested.Single(static one => one.Name == "Syntax");

		Assert.Same(Declared(model, "Near"), model.Trivia[syntax]);
		Assert.Same(Declared(model, "Far"), model.Trivia[model.Root]);
	}

	const string TwoTrivia =
		"namespace Spaces { trivia = { ' '* } }\n" +
		"namespace Underscores { trivia = { '_'* } }\n";

	/// <summary>
	/// Two imports at one scope that each declare a trivia: the order of the <c>using</c>s would
	/// decide, and §5 says it never decides between two imports. An error at the second, at either
	/// placement.
	/// </summary>
	[Theory]
	[InlineData("using Spaces;\nusing Underscores;\n" + TwoTrivia + "Start = 'a' & 'b'\nparse Start\n", "using Underscores;")]
	[InlineData("using Underscores;\nusing Spaces;\n" + TwoTrivia + "Start = 'a' & 'b'\nparse Start\n", "using Spaces;")]
	[InlineData(TwoTrivia + "namespace Syntax { using Spaces;\n using Underscores;\n Start = 'a' & 'b' }\nparse Syntax.Start\n", "using Underscores;")]
	[InlineData(TwoTrivia + "namespace Syntax { using Underscores;\n using Spaces;\n Start = 'a' & 'b' }\nparse Syntax.Start\n", "using Spaces;")]
	public void Two_imports_at_one_scope_that_declare_trivia_are_ambiguous(string grammar, string second)
	{
		var model = GrammarBinder.Bind(GramParser.Parse(GramLexer.Tokenize(grammar, RoslynCSharpScanner.Instance)).File);
		var told  = Assert.Single(model.Diagnostics);

		Assert.Equal(GrammarBinder.AmbiguousTrivia, told.Id);
		Assert.Equal(GramSeverity.Error, told.Severity);
		Assert.Equal(grammar.IndexOf(second, System.StringComparison.Ordinal), told.Position);
	}

	/// <summary>What the scope declares settles it, and so does an import nearer than the other.</summary>
	[Fact]
	public void A_declaration_or_a_nearer_import_settles_it()
	{
		var declared =
			TwoTrivia +
			"namespace Syntax { using Spaces;\n using Underscores;\n trivia = Underscores.trivia\n Start = 'a' & 'b' & eof }\n" +
			"parse Syntax.Start\n";

		var nearer =
			"using Spaces;\n" + TwoTrivia +
			"namespace Syntax { using Underscores;\n Start = 'a' & 'b' & eof }\n" +
			"parse Syntax.Start\n";

		Bind(declared);
		Bind(nearer);

		Assert.True(Accepts(declared, "Syntax.Start", "a_b"));
		Assert.True(Accepts(nearer, "Syntax.Start", "a_b"));
		Assert.False(Accepts(nearer, "Syntax.Start", "a b"));
	}

	/// <summary>One namespace imported twice is one trivia, not two.</summary>
	[Fact]
	public void The_same_namespace_imported_at_one_scope_twice_is_not_ambiguous()
	{
		Bind(TwoTrivia + "namespace Syntax { using Spaces;\n using Spaces;\n Start = 'a' & 'b' }\nparse Syntax.Start\n");
	}

	static RuleSymbol Declared(GrammarModel model, string ns)
	{
		return model.Root.Nested.Single(one => one.Name == ns).Rules["trivia"];
	}

	static GrammarModel Bind(string source)
	{
		var model = GrammarBinder.Bind(GramParser.Parse(GramLexer.Tokenize(source, RoslynCSharpScanner.Instance)).File);

		Assert.Empty(model.Diagnostics);

		return model;
	}

	static bool Accepts(string grammar, string rule, string input)
	{
		var result = GramCompiler.Compile(grammar, new GramCompilerOptions { ClassName = "Grammar", CSharpScanner = RoslynCSharpScanner.Instance });

		Assert.DoesNotContain(result.Diagnostics, static one => one.Severity == GramSeverity.Error);

		var assembly = EmittedCode.Compile(result.Sources[0].Text);

		return EmittedCode.Match(assembly, "Grammar", "TryParse" + rule.Split('.').Last(), input).IsSuccess;
	}
}
