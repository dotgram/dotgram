using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Grammar;
using DotGram.Grammar.Emit;
using DotGram.Grammar.Parsing;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.Language.Tests;

/// <summary>
/// Every grammar in the repository is shown by the editor with exactly the diagnostics the
/// generator's compiler reports for it, compiled as the generator compiles it: the same numbers,
/// messages, severities and places, the compiler's places narrowed as the editor narrows them;
/// and a grammar in a <c>[Gram]</c> is shown in its C# file what Roslyn shows of those there.
/// </summary>
/// <remarks>
/// <para>
/// The compiler is handed the host's options as the generator hands them over (<see cref="Compiled"/>),
/// read by the reader the generator reads them by (<see cref="GramHostOptions"/>), and the editor
/// what it reads of the host itself; an option the editor left out was a grammar shown what its
/// build does not report — a carrier named in the attribute, which silences what the generator
/// says of the one it would choose. Left out on both sides is only what needs the host's C# to
/// mean anything: a location type, which locates the rules whose types implement it.
/// </para>
/// <para>
/// The resolver is the one input this cannot hold. The generator asks the host's compilation; the
/// editor resolves no C# type, and leaves what only a no from its resolver would report to the
/// build (<see cref="UnseenHostSymbolResolver"/>). The compiler is run the same way here,
/// so what the host's C# would answer — a constructor the captures fill, a recognizer that takes
/// the input view — is not compared, and a divergence there is not caught here; what the
/// generator asks a compilation is DotGram.Tests' to hold.
/// </para>
/// <para>
/// A grammar in an attribute is compared once more in its C# file, through the extension's own
/// entry point (<see cref="EmbeddedGrammarService"/>): the compiler's diagnostics placed in the
/// file and handed to Roslyn, which applies the file's <c>#pragma warning</c> and the project's
/// options to them as it does to the generator's (<see cref="RoslynReport"/>). Compared as text
/// alone, a grammar under <c>#pragma warning disable</c> was shown a warning its build had silenced.
/// </para>
/// <para>
/// The grammars are <see cref="EditorCorpusTests"/>' own: every <c>.gram</c> file and every
/// grammar written into a <c>[Gram("…")]</c> attribute. A grammar in an attribute is read with
/// the options of that attribute, its class bound alone with the generator's attributes beside
/// it; a <c>.gram</c> file with those of the class whose attribute names it, where one does.
/// </para>
/// </remarks>
public sealed class EditorParityTests
{
	public static TheoryData<string> Grammars()
	{
		var data = new TheoryData<string>();

		foreach (var grammar in Corpus())
			data.Add(grammar.Name);

		return data;
	}

	[Theory]
	[MemberData(nameof(Grammars))]
	public void The_editor_shows_the_diagnostics_the_compiler_reports(string name)
	{
		var grammar  = Corpus().Single(one => one.Name == name);
		var compiled = GramCompiler.Compile(grammar.Text, Compiled(grammar.Options, null, null));
		var analysed = GramLanguageService.Analyze(
			grammar.Text, grammar.Options is { } options ? GramAnalysisOptions.Of(options) : null);
		var tokens   = GramLexer.Tokenize(grammar.Text, RoslynCSharpScanner.Instance).Tokens;

		// Only the compiler's side is narrowed here: the editor's must come narrowed already.
		Assert.Equal(
			GramLanguageService.NormalizeDiagnostics(compiled.Diagnostics, tokens).Select(Shown),
			analysed.Diagnostics.Select(Shown));
	}

	public static TheoryData<string> Hosts()
	{
		var data = new TheoryData<string>();

		foreach (var file in EditorCorpusTests.Files("*.cs"))
			if (File.ReadAllText(file).Contains("Gram", StringComparison.Ordinal))
				data.Add(EditorCorpusTests.Relative(file));

		return data;
	}

	[Theory]
	[MemberData(nameof(Hosts))]
	public async Task The_editor_shows_in_the_C_sharp_file_what_the_build_reports(string file)
	{
		var cancellation = TestContext.Current.CancellationToken;
		var (tree, compilation) = Host(Path.Combine(EditorCorpusTests.Root, file), cancellation);
		var model    = compilation.GetSemanticModel(tree);
		var root     = await tree.GetRootAsync(cancellation);
		var reported = new List<(GramDiagnostic Diagnostic, SyntaxTree? Tree, TextSpan Span)>();

		foreach (var grammar in EmbeddedGrammarFinder.Find(model, root, cancellation))
		{
			var attribute = grammar.Token.Parent?.FirstAncestorOrSelf<AttributeSyntax>();
			var options   = attribute is null ? null : Options(model, attribute);
			var compiled  = GramCompiler.Compile(grammar.AnalysisText, Compiled(options, grammar.Options?.Own, grammar.Options?.Cuts));
			var tokens    = GramLexer.Tokenize(
				grammar.AnalysisText,
				RoslynCSharpScanner.Instance,
				GramCompiler.CutsOf(grammar.Options?.Own, grammar.Options?.Cuts),
				grammar.AnalysisText.Length,
				null).Tokens;

			// Placed as the generator places a diagnostic in the literal: where the literal holds
			// its place, and over the whole literal where it does not.
			foreach (var diagnostic in GramLanguageService.NormalizeDiagnostics(compiled.Diagnostics, tokens))
				if (diagnostic.Position <= grammar.Text.Length)
					reported.Add((
						diagnostic,
						tree,
						grammar.SourceMap.TryMap(diagnostic.Position, diagnostic.Length, out var span) ? span : grammar.Token.Span));
		}

		var built = await RoslynReport.Shown(compilation, reported, cancellation);
		var shown = EmbeddedGrammarService.Analyze(model, root, cancellation)
			.SelectMany(static analysis => analysis.Diagnostics)
			.Select(static one => (one.Diagnostic.Id, one.Diagnostic.Message, one.Diagnostic.Severity, one.Span));

		Assert.Equal(Ordered(built), Ordered(shown));
	}

	static IEnumerable<(string Id, string Message, GramSeverity Severity, TextSpan Span)> Ordered(
		IEnumerable<(string Id, string Message, GramSeverity Severity, TextSpan Span)> diagnostics)
	{
		return diagnostics.OrderBy(static one => one.Span.Start).ThenBy(static one => one.Id, StringComparer.Ordinal)
			.ThenBy(static one => one.Message, StringComparer.Ordinal);
	}

	/// <summary>
	/// The compiler's options as the generator hands them over for a host's reading, by the one
	/// mapping both use (<see cref="GramHostOptions.ApplyTo"/>), with the class it compiles into;
	/// less what needs the host's C# (see the remarks above).
	/// </summary>
	static GramCompilerOptions Compiled(GramHostReading? host, int? own, IReadOnlyList<int>? cuts)
	{
		var options = new GramCompilerOptions
		{
			CSharpScanner  = RoslynCSharpScanner.Instance,
			SymbolResolver = UnseenHostSymbolResolver.Instance,
			Own            = own,
			Cuts           = cuts,
		};

		if (host is null)
			return options;

		host.Options.ApplyTo(options);
		options.ClassName = host.ClassName;
		options.Namespace = host.Namespace;

		return options;
	}

	/// <summary>A C# file compiled alone, with the generator's attributes beside it.</summary>
	static (SyntaxTree Tree, CSharpCompilation Compilation) Host(string path, CancellationToken cancellation)
	{
		var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(path), path: path, cancellationToken: cancellation);

		return (tree, CSharpCompilation.Create(
			"Parity",
			[
				tree,
				CSharpSyntaxTree.ParseText(SupportEmitter.Attributes, cancellationToken: cancellation),
				CSharpSyntaxTree.ParseText(SupportEmitter.EmbeddedAttribute, cancellationToken: cancellation),
			],
			[MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)));
	}

	/// <summary>
	/// A reference that does not resolve is reported by the compiler over the whole reference and
	/// shown by the editor under its first name, which is the narrowing the corpus comparison
	/// holds the editor to: here it changes something, so the comparison above is not idle.
	/// </summary>
	[Theory]
	[InlineData("A = Missing.Word\nparse A", "Missing")]
	[InlineData("A = Base.Missing.Word\nparse A", "Base")]
	public void A_qualified_reference_that_does_not_resolve_is_shown_under_its_first_name(string text, string first)
	{
		var compiled = GramCompiler.Compile(text, new GramCompilerOptions { CSharpScanner = RoslynCSharpScanner.Instance });
		var analysed = GramLanguageService.Analyze(text);
		var tokens   = GramLexer.Tokenize(text, RoslynCSharpScanner.Instance).Tokens;
		var reported = Assert.Single(compiled.Diagnostics, static diagnostic => diagnostic.Id == "GRAM3002");
		var shown    = Assert.Single(analysed.Diagnostics, static diagnostic => diagnostic.Id == "GRAM3002");

		Assert.Equal(text.IndexOf(first, StringComparison.Ordinal), shown.Position);
		Assert.Equal(first.Length, shown.Length);
		Assert.True(reported.Length > shown.Length, $"The compiler reports {reported.Length} characters; nothing is narrowed.");
		Assert.Equal(
			GramLanguageService.NormalizeDiagnostics(compiled.Diagnostics, tokens).Select(Shown),
			analysed.Diagnostics.Select(Shown));
	}

	[Fact]
	public void The_corpus_reads_host_options()
	{
		var corpus = Corpus();

		Assert.Equal(EditorCorpusTests.Grammars().Count, corpus.Count);
		Assert.Contains(corpus, static grammar => grammar.Options is { Options.Lexical: true });
		Assert.Contains(corpus, static grammar => grammar.Name.EndsWith(".gram", StringComparison.Ordinal) && grammar.Options is not null);
	}

	static (string Id, string Message, GramSeverity Severity, int Position, int Length) Shown(GramDiagnostic diagnostic)
	{
		return (diagnostic.Id, diagnostic.Message, diagnostic.Severity, diagnostic.Position, diagnostic.Length);
	}

	sealed record Grammar(string Name, string Text, GramHostReading? Options);

	static IReadOnlyList<Grammar> Corpus()
	{
		return _corpus ??= Read();
	}

	static IReadOnlyList<Grammar>? _corpus;

	static IReadOnlyList<Grammar> Read()
	{
		var attributes = new List<Grammar>();
		var files      = new List<(string Path, GramHostReading Options)>();
		var support    = new[]
		{
			CSharpSyntaxTree.ParseText(SupportEmitter.Attributes),
			CSharpSyntaxTree.ParseText(SupportEmitter.EmbeddedAttribute),
		};
		var references = new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) };

		foreach (var file in EditorCorpusTests.Files("*.cs"))
		{
			var source = File.ReadAllText(file);

			if (!source.Contains("Gram", StringComparison.Ordinal))
				continue;

			var tree  = CSharpSyntaxTree.ParseText(source);
			var model = CSharpCompilation.Create("Parity", [tree, .. support], references).GetSemanticModel(tree);
			var index = 0;

			foreach (var attribute in tree.GetRoot().DescendantNodes().OfType<AttributeSyntax>())
			{
				var name = attribute.Name switch
				{
					QualifiedNameSyntax qualified   => qualified.Right.Identifier.ValueText,
					AliasQualifiedNameSyntax global => global.Name.Identifier.ValueText,
					SimpleNameSyntax simple         => simple.Identifier.ValueText,
					_                               => "",
				};

				// Counted as the corpus counts them, so that the names are the same names.
				if (name is not ("Gram" or "GramAttribute") ||
					attribute.ArgumentList?.Arguments.FirstOrDefault() is not { NameEquals: null } argument)
					continue;

				var place   = $"{EditorCorpusTests.Relative(file)}#{++index}";
				var options = Options(model, attribute);

				if (EditorCorpusTests.Constant(argument.Expression) is not { } grammar)
					continue;

				if (grammar.EndsWith(".gram", StringComparison.OrdinalIgnoreCase) && grammar.IndexOf('\n') < 0)
				{
					if (options is not null)
						files.Add((grammar, options));

					continue;
				}

				attributes.Add(new Grammar(place, grammar, options));
			}
		}

		var result = new List<Grammar>();

		foreach (var file in EditorCorpusTests.Files("*.gram"))
		{
			var host = files.Where(one => GramIncludes.Matches(file, one.Path)).Select(static one => one.Options).ToArray();

			// A file two classes compile with different options has no one reading to hold.
			result.Add(new Grammar(EditorCorpusTests.Relative(file), File.ReadAllText(file), host.Distinct().Count() == 1 ? host[0] : null));
		}

		result.AddRange(attributes);

		return result;
	}

	/// <summary>What the reading an attribute is says, as the generator and the editor read it.</summary>
	static GramHostReading? Options(SemanticModel model, AttributeSyntax attribute)
	{
		if (attribute.Parent?.Parent is not TypeDeclarationSyntax declaration ||
			model.GetDeclaredSymbol(declaration) is not INamedTypeSymbol type)
			return null;

		var reading = type.GetAttributes().FirstOrDefault(candidate =>
			candidate.ApplicationSyntaxReference?.Span == attribute.Span &&
			candidate.AttributeClass?.ToDisplayString() == GramIncludes.GramAttribute);

		if (reading is null)
			return null;

		var primary = GramIncludes.Primary(type);

		return GramHostReading.Of(
			GramHostOptions.Read(reading, type, primary is null || primary == reading ? null : GramHostOptions.Read(primary, type, first: null)),
			type);
	}
}
