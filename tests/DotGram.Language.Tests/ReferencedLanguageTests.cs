using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.Language.Tests;

/// <summary>
/// A library built by the generator, as a package would be, and a project that references it:
/// what the editor colours there is the library's grammar, whatever the project's own files say.
/// </summary>
/// <remarks>
/// The library's host is internal, and its grammar is a file of the library's project that
/// includes another. The project has files of the same names holding other grammars, which a
/// discovery that read the library's <c>[Gram]</c> and <c>[GramInclude]</c> would resolve here.
/// </remarks>
public sealed class ReferencedLanguageTests
{
	const string Lexicon = """
		Let  = "let"
		Word = ['a'..'z']+
		""";

	const string Filter = """
		using Lexicon;
		trivia  = ' '*
		Keyword = Lexicon.Let
		Start   = Keyword & ' ' & name: Lexicon.Word
		parse Start
		""";

	const string DecoyLexicon = """
		Let  = "show"
		Word = ['0'..'9']+
		""";

	const string DecoyFilter = """
		trivia  = ' '*
		Keyword = "show"
		Start   = Keyword & ' ' & name: ['0'..'9']+
		parse Start
		""";

	const string Library = """
		namespace Package
		{
			[DotGram.Gram("Lexicon.gram")]
			static partial class Lexicon;

			[DotGram.Gram("Filter.gram")]
			[DotGram.GramInclude(typeof(Lexicon))]
			[DotGram.GramLanguage("e1.filter")]
			[DotGram.GramClassify("Keyword", DotGram.GramClassification.Keyword)]
			[DotGram.GramClassify("Start.name", DotGram.GramClassification.Variable)]
			static partial class FilterParser;

			public static class Filters
			{
				public static void Run([System.Diagnostics.CodeAnalysis.StringSyntax("e1.filter")] string text) { }
			}
		}
		""";

	const string Consumer = """
		static class Consumer
		{
			static void Test() => Package.Filters.Run("let customer");
		}
		""";

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public async Task ColoursFromTheLibrarysGrammarNotTheProjectsFiles(bool decoys)
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var (image, _) = Build();
		var document = Document(MetadataReference.CreateFromImage(image), decoys);
		var text     = await document.GetTextAsync(cancellationToken);
		var root     = await document.GetSyntaxRootAsync(cancellationToken) ?? throw new InvalidOperationException();
		var model    = await document.GetSemanticModelAsync(cancellationToken) ?? throw new InvalidOperationException();

		var language = Assert.Single(DslLanguageDiscovery.Discover(model.Compilation, cancellationToken).Languages);
		Assert.Equal("FilterParser", language.ParserType.Name);
		Assert.Equal(Accessibility.Internal, language.ParserType.DeclaredAccessibility);
		Assert.Equal(DslGrammarSourceKind.Embedded, language.SourceKind);
		Assert.Empty(language.IncludedGrammars);

		var result = await DslEmbeddedSiteAnalysis.AnalyzeAsync(document, root, model, cancellationToken);

		Assert.Empty(result.Diagnostics);
		Assert.Equal("Start", Assert.Single(result.Sites).EntryRule);
		Assert.Equal(
			new[] { ("Keyword", "let"), ("Variable", "customer") },
			result.Classifications.OrderBy(static item => item.Span.Start)
				.Select(item => (item.Role, text.ToString(item.Span))));
	}

	/// <summary>
	/// What a project reference compiles against is the library's reference assembly. That
	/// keeps the library's internal types and drops only its private members, so an internal
	/// host is found through it as through the library itself.
	/// </summary>
	[Fact]
	public void SeesAnInternalHostThroughAReferenceAssembly()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var (_, referenceImage) = Build();
		var reference   = MetadataReference.CreateFromImage(referenceImage);
		var compilation = CSharpCompilation.Create(
			"ReferenceAssemblyConsumer",
			[CSharpSyntaxTree.ParseText(Consumer, cancellationToken: cancellationToken)],
			[.. Framework, reference]);

		var language = Assert.Single(DslLanguageDiscovery.Discover(compilation, cancellationToken).Languages);

		Assert.Equal("e1.filter", language.Id);
		Assert.Equal(Accessibility.Internal, language.ParserType.DeclaredAccessibility);
	}

	/// <summary>The library, emitted whole and as its reference assembly.</summary>
	static (byte[] Image, byte[] ReferenceImage) Build()
	{
		var cancellationToken = TestContext.Current.CancellationToken;
		var parseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);
		var compilation  = CSharpCompilation.Create(
			"E1Package",
			[CSharpSyntaxTree.ParseText(Library, parseOptions, "Library.cs", cancellationToken: cancellationToken)],
			Framework,
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

		CSharpGeneratorDriver
			.Create(
				[Generator.Value.AsSourceGenerator()],
				additionalTexts:
				[
					new InMemoryFile(@"P:\Package\Lexicon.gram", Lexicon),
					new InMemoryFile(@"P:\Package\Filter.gram", Filter),
				],
				parseOptions: parseOptions)
			.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics, cancellationToken);

		Assert.Empty(diagnostics.Where(static one => one.Severity == DiagnosticSeverity.Error));

		using var image          = new MemoryStream();
		using var referenceImage = new MemoryStream();
		var emitted = output.Emit(image, metadataPEStream: referenceImage, cancellationToken: cancellationToken);

		Assert.True(emitted.Success, string.Join("\n", emitted.Diagnostics.Where(static one => one.Severity == DiagnosticSeverity.Error)));

		return (image.ToArray(), referenceImage.ToArray());
	}

	static Document Document(MetadataReference library, bool decoys)
	{
		var workspace = new AdhocWorkspace();
		var project   = workspace.AddProject(ProjectInfo.Create(
			ProjectId.CreateNewId(),
			VersionStamp.Default,
			"E1Consumer",
			"E1Consumer",
			LanguageNames.CSharp,
			parseOptions: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview),
			compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary),
			metadataReferences: [.. Framework, library]));
		var document = workspace.AddDocument(project.Id, "Consumer.cs", SourceText.From(Consumer));

		if (!decoys)
			return document;

		var decorated = document.Project
			.AddAdditionalDocument("Filter.gram", SourceText.From(DecoyFilter), filePath: @"P:\Consumer\Filter.gram").Project
			.AddAdditionalDocument("Lexicon.gram", SourceText.From(DecoyLexicon), filePath: @"P:\Consumer\Lexicon.gram").Project;

		return decorated.GetDocument(document.Id) ?? throw new InvalidOperationException();
	}

	/// <summary>The runtime's own assemblies: what a library and its consumer compile against.</summary>
	static ImmutableArray<MetadataReference> Framework { get; } =
	[
		.. ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
			.Split(Path.PathSeparator)
			.Where(static path => string.Equals(
				Path.GetDirectoryName(path),
				Path.GetDirectoryName(typeof(object).Assembly.Location),
				StringComparison.OrdinalIgnoreCase))
			.Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path)),
	];

	/// <summary>
	/// The generator as a build runs it, from the assembly the project file names: it cannot be
	/// referenced here, since it carries the language core these tests compile too.
	/// </summary>
	static readonly Lazy<IIncrementalGenerator> Generator = new(static () =>
	{
		var path = typeof(ReferencedLanguageTests).Assembly
			.GetCustomAttributes<AssemblyMetadataAttribute>()
			.Single(static attribute => attribute.Key == "DotGramGenerator")
			.Value!;
		var type = Assembly.LoadFrom(path).GetType("DotGram.Generation.GramGenerator", throwOnError: true)!;

		return (IIncrementalGenerator)Activator.CreateInstance(type)!;
	});

	/// <summary>A .gram file that never touched a disk.</summary>
	sealed class InMemoryFile(string path, string text) : AdditionalText
	{
		public override string Path { get; } = path;

		public override SourceText GetText(CancellationToken cancellationToken = default)
		{
			return SourceText.From(text);
		}
	}
}
