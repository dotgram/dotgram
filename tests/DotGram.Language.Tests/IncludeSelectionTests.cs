using System;
using System.Collections.Generic;
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
	const string CommonFile   = "Word = 'a'\nFromCommon = 'm'\n";
	const string Inline       = "Word = 'a'\\nFromInline = 'i'\\n";
	const string Carried      = "Word = 'a'\\nFromCarried = 'p'\\n";

	const string Exported    = "[DotGram.Gram(\"Base.gram\", DefaultAlias = \"B\")]";
	const string NotExported = "[DotGram.Gram(\"Base.gram\", DefaultAlias = \"B\", ExportGrammar = false)]";

	// What the consumer may reach under B, one rule for each grammar a case offers.
	static readonly string[] Rules = ["FromLibrary", "FromConsumer", "FromInline", "FromCarried", "C.FromCommon"];

	static readonly CSharpParseOptions ParseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);

	static readonly MetadataReference[] Framework = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
		.Split(Path.PathSeparator)
		.Where(static path => Path.GetFileName(path).StartsWith("System.", StringComparison.Ordinal) || Path.GetFileName(path) is "netstandard.dll" or "mscorlib.dll")
		.Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path))
		.ToArray();

	static readonly (string Path, string Text)[] OneConsumerFile = [("/Consumer/Base.gram", ConsumerFile)];
	static readonly (string Path, string Text)[] TwoConsumerFiles = [("/Consumer/a/Base.gram", ConsumerFile), ("/Consumer/b/Base.gram", ConsumerFile)];

	/// <summary>
	/// One case: the library's class and files, the consumer's files, the rule under B the consumer
	/// reaches (null for none), and the generator's diagnostics on the consumer, by id, when it reaches
	/// for that rule — or for B.Word where it reaches none.
	/// </summary>
	sealed record Case(
		string Attribute,
		(string Path, string Text)[] LibraryFiles,
		(string Path, string Text)[] ConsumerFiles,
		string? Reached,
		string[] Diagnostics,
		string LibraryMore = "");

	static readonly Dictionary<string, Case> All = new()
	{
		// Exported: what the library carries, its file; the consumer's own file of the name is told of (GRAM0017).
		["exported"] = new(Exported, [("/Library/Base.gram", LibraryFile)], OneConsumerFile, "FromLibrary", ["GRAM0017"]),
		["exported, the consumer's file ambiguous"] = new(Exported, [("/Library/Base.gram", LibraryFile)], TwoConsumerFiles, "FromLibrary", []),
		["exported, no consumer file"] = new(Exported, [("/Library/Base.gram", LibraryFile)], [], "FromLibrary", []),

		// Not exported: the consumer's own file, or nothing.
		["not exported"] = new(NotExported, [("/Library/Base.gram", LibraryFile)], OneConsumerFile, "FromConsumer", []),
		["not exported, no consumer file"] = new(NotExported, [("/Library/Base.gram", LibraryFile)], [], null, ["GRAM0003", "GRAM3002"]),
		["not exported, the consumer's file ambiguous"] = new(NotExported, [("/Library/Base.gram", LibraryFile)], TwoConsumerFiles, null, ["GRAM0004", "GRAM3002"]),

		// Exported but empty, or without its file: nothing is carried.
		["exported empty"] = new(Exported, [("/Library/Base.gram", "")], OneConsumerFile, "FromConsumer", []),
		["exported empty, no consumer file"] = new(Exported, [("/Library/Base.gram", "")], [], null, ["GRAM0003", "GRAM3002"]),
		["no library file"] = new(Exported, [], OneConsumerFile, "FromConsumer", []),

		// Inline: carried where exported; a [GramSource] of its own comes first where not.
		["inline"] = new($"[DotGram.Gram(\"{Inline}\", DefaultAlias = \"B\")]", [], OneConsumerFile, "FromInline", []),
		["inline, not exported, carried"] = new($"[DotGram.Gram(\"{Inline}\", DefaultAlias = \"B\", ExportGrammar = false)] [DotGram.GramSource(\"{Carried}\")]", [], OneConsumerFile, "FromCarried", []),

		// What the library carries is its grammar with what it includes spliced on, an empty file of its own too.
		["exported empty, including another"] = new(
			Exported + " [DotGram.GramInclude(typeof(Common))]",
			[("/Library/Base.gram", ""), ("/Library/Common.gram", CommonFile)],
			OneConsumerFile,
			"C.FromCommon",
			["GRAM0017"],
			"[DotGram.Gram(\"Common.gram\", DefaultAlias = \"C\")] public partial class Common { }"),
	};

	public static TheoryData<string> Cases()
	{
		return [.. All.Keys];
	}

	[Theory]
	[MemberData(nameof(Cases))]
	public async Task The_editor_includes_what_the_generator_includes(string name)
	{
		var test     = All[name];
		var library  = $"namespace Library {{ {test.Attribute} public partial class Base {{ }} {test.LibraryMore} }}";
		var consumer = "[DotGram.Gram(\"Dialect.gram\")] [DotGram.GramInclude(typeof(Library.Base))] public partial class Dialect { }";
		var embedded = "[DotGram.Gram(\"Start = B.Word\\nparse Start\")] [DotGram.GramInclude(typeof(Library.Base))] public partial class Embedded { }";

		// The generator: the library compiled with it, and the consumer against the library's assembly.
		var image = Generate("Library", library, test.LibraryFiles, [], out _);

		foreach (var rule in Rules)
		{
			Generate(
				"Consumer",
				consumer,
				[("/Consumer/Dialect.gram", $"Start = B.{rule}\nparse Start\n"), .. test.ConsumerFiles],
				[MetadataReference.CreateFromImage(image)],
				out var diagnostics);

			var reached = !diagnostics.Any(static diagnostic => diagnostic.Id == "GRAM3002");

			Assert.True(reached == (rule == test.Reached), $"The generator {(reached ? "reaches" : "does not reach")} B.{rule}.");

			if (rule == (test.Reached ?? Rules[0]))
			{
				// Less the warning every case has: the included grammar's rules the consumer does not reach.
				var told = diagnostics
					.Select(static diagnostic => diagnostic.Id)
					.Where(static id => id.StartsWith("GRAM", StringComparison.Ordinal) && id != "GRAM4018")
					.Distinct()
					.Order(StringComparer.Ordinal);

				Assert.Equal(test.Diagnostics, told);
			}
		}

		// The editors: the two as projects of one solution, the consumer's grammar a file and one in C#.
		using var workspace = new AdhocWorkspace();

		var markers  = GramCompiler.EmitMarkerAttributes();
		var solution = workspace.CurrentSolution;

		ProjectId Add(string project, string source)
		{
			var id = ProjectId.CreateNewId(project);

			solution = solution
				.AddProject(ProjectInfo.Create(id, VersionStamp.Default, project, project, LanguageNames.CSharp, parseOptions: ParseOptions, metadataReferences: Framework))
				.AddDocument(DocumentId.CreateNewId(id), markers.HintName, SourceText.From(markers.Text))
				.AddDocument(DocumentId.CreateNewId(id), project + ".cs", SourceText.From(source));

			return id;
		}

		var libraryId  = Add("Library", library);
		var consumerId = Add("Consumer", consumer);
		var embeddedId = DocumentId.CreateNewId(consumerId);

		solution = solution
			.AddProjectReference(consumerId, new ProjectReference(libraryId))
			.AddDocument(embeddedId, "Embedded.cs", SourceText.From(embedded))
			.AddAdditionalDocument(DocumentId.CreateNewId(consumerId), "Dialect.gram", SourceText.From("Start = B.Word\nparse Start\n"), filePath: "/Consumer/Dialect.gram");

		foreach (var (path, text) in test.LibraryFiles)
			solution = solution.AddAdditionalDocument(DocumentId.CreateNewId(libraryId), Path.GetFileName(path), SourceText.From(text), filePath: path);

		foreach (var (path, text) in test.ConsumerFiles)
			solution = solution.AddAdditionalDocument(DocumentId.CreateNewId(consumerId), Path.GetFileName(path), SourceText.From(text), filePath: path);

		var expected = test.Reached?.Split('.')[^1];

		void Holds(string? text, string editor)
		{
			foreach (var rule in Rules.Select(static rule => rule.Split('.')[^1]))
				Assert.True(
					(text?.Contains(rule + " = ", StringComparison.Ordinal) ?? false) == (rule == expected),
					$"The {editor} {(rule == expected ? "does not include" : "includes")} {rule}: {text}");
		}

		// A grammar file's host, as both editors read it.
		var resolution = await StandaloneGrammarInheritance.ResolveAsync(solution, "/Consumer/Dialect.gram", TestContext.Current.CancellationToken);

		Holds(Assert.NotNull(resolution.Context).Included.SingleOrDefault(static item => item.Name == "B").Text, "standalone analysis");

		// A grammar in C#, as Visual Studio reads it.
		var document = solution.GetDocument(embeddedId)!;
		var model    = await document.GetSemanticModelAsync(TestContext.Current.CancellationToken);
		var root     = await document.GetSyntaxRootAsync(TestContext.Current.CancellationToken);
		var found    = Assert.Single(EmbeddedGrammarFinder.Find(model!, root!, TestContext.Current.CancellationToken, await EmbeddedIncludesAsync(document)));

		Holds(found.AnalysisText.Contains("namespace B", StringComparison.Ordinal) ? found.AnalysisText : null, "embedded analysis");
	}

	/// <summary>What the embedded analysis is given of the project, as the session gives it.</summary>
	static async Task<IncludedGrammarSources> EmbeddedIncludesAsync(Document document)
	{
		return await SolutionIncludedGrammars.ReadAsync(document.Project, TestContext.Current.CancellationToken);
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
