using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.Language.Tests;

/// <summary>
/// Which grammar a class of another project is included with: the editors' choice, held to the
/// generator's own, run on the library and on a consumer that references it as an assembly.
/// </summary>
/// <remarks>
/// Each grammar the case offers — the library's file, the consumer's file, the class's inline grammar,
/// the [GramSource] it carries — defines a rule no other has. The editor must include the one whose
/// rule the generator lets the consumer reach, and the generator must refuse the others' rules.
/// </remarks>
public sealed class IncludeSelectionTests
{
	const string LibraryFile  = "Word = 'a'\nFromLibrary = 'l'\n";
	const string ConsumerFile = "Word = 'a'\nFromConsumer = 'c'\n";
	const string Inline       = "Word = 'a'\\nFromInline = 'i'\\n";
	const string Carried      = "Word = 'a'\\nFromCarried = 'p'\\n";

	static readonly string[] Rules = ["FromLibrary", "FromConsumer", "FromInline", "FromCarried"];

	static readonly CSharpParseOptions ParseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);

	static readonly MetadataReference[] Framework = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
		.Split(Path.PathSeparator)
		.Where(static path => Path.GetFileName(path).StartsWith("System.", StringComparison.Ordinal) || Path.GetFileName(path) is "netstandard.dll" or "mscorlib.dll")
		.Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path))
		.ToArray();

	public static TheoryData<string, string?, bool, string> Cases()
	{
		return new TheoryData<string, string?, bool, string>
		{
			// An exported grammar: what the library carries, its file.
			{ "[DotGram.Gram(\"Base.gram\", DefaultAlias = \"B\")]", LibraryFile, true, "FromLibrary" },

			// Not exported: the consumer's own file.
			{ "[DotGram.Gram(\"Base.gram\", DefaultAlias = \"B\", ExportGrammar = false)]", LibraryFile, true, "FromConsumer" },

			// Exported but empty: nothing is carried, and the consumer's own file is read.
			{ "[DotGram.Gram(\"Base.gram\", DefaultAlias = \"B\")]", "", true, "FromConsumer" },

			// The library has no file: nothing is carried either.
			{ "[DotGram.Gram(\"Base.gram\", DefaultAlias = \"B\")]", null, true, "FromConsumer" },

			// Inline and exported: the inline grammar, carried.
			{ $"[DotGram.Gram(\"{Inline}\", DefaultAlias = \"B\")]", null, true, "FromInline" },

			// Inline, not exported, with a [GramSource] of its own: what the class carries comes first.
			{ $"[DotGram.Gram(\"{Inline}\", DefaultAlias = \"B\", ExportGrammar = false)] [DotGram.GramSource(\"{Carried}\")]", null, true, "FromCarried" },
		};
	}

	[Theory]
	[MemberData(nameof(Cases))]
	public async Task The_editor_includes_what_the_generator_includes(string attribute, string? libraryFile, bool consumerHasFile, string expected)
	{
		var library  = $"namespace Library {{ {attribute} public partial class Base {{ }} }}";
		var consumer = "[DotGram.Gram(\"Dialect.gram\")] [DotGram.GramInclude(typeof(Library.Base))] public partial class Dialect { }";

		// The generator: the library compiled with it, and the consumer against the library's assembly.
		var image = Generate("Library", library, libraryFile is null ? [] : [("/Library/Base.gram", libraryFile)], [], out _);

		foreach (var rule in Rules)
		{
			Generate(
				"Consumer",
				consumer,
				[("/Consumer/Dialect.gram", $"Start = B.{rule}\nparse Start\n"), .. consumerHasFile ? new[] { ("/Consumer/Base.gram", ConsumerFile) } : []],
				[MetadataReference.CreateFromImage(image)],
				out var diagnostics);

			var reached = !diagnostics.Any(static diagnostic => diagnostic.Id == "GRAM3002");

			Assert.True(reached == (rule == expected), $"The generator {(reached ? "reaches" : "does not reach")} B.{rule}.");
		}

		// The editor: the two as projects of one solution.
		using var workspace = new AdhocWorkspace();

		var markers  = GramCompiler.EmitMarkerAttributes();
		var solution = workspace.CurrentSolution;

		ProjectId Add(string name, string source)
		{
			var id = ProjectId.CreateNewId(name);

			solution = solution
				.AddProject(ProjectInfo.Create(id, VersionStamp.Default, name, name, LanguageNames.CSharp, parseOptions: ParseOptions, metadataReferences: Framework))
				.AddDocument(DocumentId.CreateNewId(id), markers.HintName, SourceText.From(markers.Text))
				.AddDocument(DocumentId.CreateNewId(id), name + ".cs", SourceText.From(source));

			return id;
		}

		var libraryId  = Add("Library", library);
		var consumerId = Add("Consumer", consumer);

		solution = solution
			.AddProjectReference(consumerId, new ProjectReference(libraryId))
			.AddAdditionalDocument(DocumentId.CreateNewId(consumerId), "Dialect.gram", SourceText.From("Start = B.Word\nparse Start\n"), filePath: "/Consumer/Dialect.gram");

		if (libraryFile is not null)
			solution = solution.AddAdditionalDocument(DocumentId.CreateNewId(libraryId), "Base.gram", SourceText.From(libraryFile), filePath: "/Library/Base.gram");

		if (consumerHasFile)
			solution = solution.AddAdditionalDocument(DocumentId.CreateNewId(consumerId), "Base.gram", SourceText.From(ConsumerFile), filePath: "/Consumer/Base.gram");

		var resolution = await StandaloneGrammarInheritance.ResolveAsync(solution, "/Consumer/Dialect.gram", TestContext.Current.CancellationToken);
		var included   = Assert.Single(Assert.NotNull(resolution.Context).Included);

		Assert.Contains(expected + " = ", included.Text, StringComparison.Ordinal);
	}

	/// <summary>Runs the generator on one compilation; its image, which must build.</summary>
	static byte[] Generate(
		string name,
		string source,
		(string Path, string Text)[] files,
		MetadataReference[] references,
		out ImmutableArray<Diagnostic> diagnostics)
	{
		var compilation = CSharpCompilation.Create(
			name,
			[CSharpSyntaxTree.ParseText(source, ParseOptions, name + ".cs", cancellationToken: TestContext.Current.CancellationToken)],
			[.. Framework, .. references],
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

		CSharpGeneratorDriver
			.Create([Generator.Value.AsSourceGenerator()], [.. files.Select(static file => (AdditionalText)new File(file.Path, file.Text))], ParseOptions)
			.RunGeneratorsAndUpdateCompilation(compilation, out var generated, out diagnostics, TestContext.Current.CancellationToken);

		using var image = new MemoryStream();

		var emitted = generated.Emit(image, cancellationToken: TestContext.Current.CancellationToken);

		Assert.True(emitted.Success, string.Join("\n", emitted.Diagnostics.Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error)));

		return image.ToArray();
	}

	// The generator's own assembly, loaded as a build loads it: its types are its own, not this
	// project's copies of the shared sources (the reference is under an alias for that reason).
	static readonly Lazy<IIncrementalGenerator> Generator = new(static () =>
		(IIncrementalGenerator)Activator.CreateInstance(
			Assembly.Load("DotGram").GetType("DotGram.Generation.GramGenerator", throwOnError: true)!,
			nonPublic: true)!);

	sealed class File(string path, string text) : AdditionalText
	{
		public override string Path { get; } = path;

		public override SourceText GetText(CancellationToken cancellationToken = default)
		{
			return SourceText.From(text);
		}
	}
}
