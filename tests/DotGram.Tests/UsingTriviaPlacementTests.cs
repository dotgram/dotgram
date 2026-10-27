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
