using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using DotGram.Grammar;
using DotGram.Grammar.Emit;
using DotGram.Grammar.Parsing;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Xunit;

namespace DotGram.Language.Tests;

/// <summary>
/// Every grammar in the repository is shown by the editor with exactly the diagnostics the
/// generator's compiler reports for it, given the same options: the same numbers, messages,
/// severities and places, the compiler's places narrowed as the editor narrows them.
/// </summary>
/// <remarks>
/// <para>
/// The same options are what the editor reads of the grammar's host, read by the reader the
/// generator reads them by (<see cref="GramHostOptions"/>), and the same resolver and scanner:
/// the permissive resolver, which the editor has, and Roslyn's scanner of <c>@(…)</c>. What
/// the editor does not take from the host — the carrier, the part size, a location type — and
/// the resolver it does not have are differences of input, not of analysis, and are left out
/// on both sides; this holds the analysis.
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
		var lexical  = grammar.Options?.Lexical ?? false;
		var compiled = GramCompiler.Compile(grammar.Text, new GramCompilerOptions
		{
			CSharpScanner = RoslynCSharpScanner.Instance,
			Lexical       = lexical,
		});
		var analysed = GramLanguageService.Analyze(grammar.Text, new GramAnalysisOptions { Lexical = lexical });
		var tokens   = GramLexer.Tokenize(grammar.Text, RoslynCSharpScanner.Instance).Tokens;

		Assert.Equal(
			GramLanguageService.NormalizeDiagnostics(compiled.Diagnostics, tokens).Select(Shown),
			GramLanguageService.NormalizeDiagnostics(analysed.Diagnostics, tokens).Select(Shown));
	}

	[Fact]
	public void The_corpus_reads_host_options()
	{
		var corpus = Corpus();

		Assert.Equal(EditorCorpusTests.Grammars().Count, corpus.Count);
		Assert.Contains(corpus, static grammar => grammar.Options is { Lexical: true });
		Assert.Contains(corpus, static grammar => grammar.Name.EndsWith(".gram", StringComparison.Ordinal) && grammar.Options is not null);
	}

	static (string Id, string Message, GramSeverity Severity, int Position, int Length) Shown(GramDiagnostic diagnostic)
	{
		return (diagnostic.Id, diagnostic.Message, diagnostic.Severity, diagnostic.Position, diagnostic.Length);
	}

	sealed record Grammar(string Name, string Text, GramHostOptions? Options);

	static IReadOnlyList<Grammar> Corpus()
	{
		return _corpus ??= Read();
	}

	static IReadOnlyList<Grammar>? _corpus;

	static IReadOnlyList<Grammar> Read()
	{
		var attributes = new List<Grammar>();
		var files      = new List<(string Path, GramHostOptions Options)>();
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
	static GramHostOptions? Options(SemanticModel model, AttributeSyntax attribute)
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

		return GramHostOptions.Read(reading, type, primary is null || primary == reading ? null : GramHostOptions.Read(primary, type, first: null));
	}
}
