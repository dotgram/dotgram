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
	// What the consumer may reach for, one rule for each grammar a case offers: under B, and under C, a
	// grammar B's class includes, which the walk of the consumer's includes reaches too.
	static readonly string[] Rules = ["B.FromLibrary", "B.FromConsumer", "B.FromInline", "B.FromCarried", "B.C.FromCommon", "C.FromCommon"];

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
	/// for that rule — or for B.Word where it reaches none. A case may put the included grammar's own
	/// include in a third project the library references and the consumer does not, may name the
	/// library's class otherwise in the consumer, and may name the grammar attribute by an alias there.
	/// </summary>
	sealed record Case(
		string Attribute,
		(string Path, string Text)[] LibraryFiles,
		(string Path, string Text)[] ConsumerFiles,
		string? Reached,
		string[] Diagnostics,
		string LibraryMore = "",
		string Class = "Base",
		string Included = "Library.Base",
		string? Common = null,
		(string Path, string Text)[]? CommonFiles = null,
		string[]? Also = null)
	{
		/// <summary>Every rule the consumer reaches: the one under B, and those <see cref="Also"/> names.</summary>
		public string[] Reaches => [.. Reached is null ? [] : new[] { "B." + Reached }, .. Also ?? []];
	}

	const string CommonProject = "namespace Shared { [DotGram.Gram(\"Common.gram\", DefaultAlias = \"C\")] public partial class Common { } }";

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
			"[DotGram.Gram(\"Common.gram\", DefaultAlias = \"C\")] public partial class Common { }",
			Also: ["C.FromCommon"]),

		// The same, the grammar it includes in a third project the consumer does not reference: reached
		// under B, and under C as the walk of the consumer's includes goes on into the library's.
		["exported empty, including another project's"] = new(
			Exported + " [DotGram.GramInclude(typeof(Shared.Common))]",
			[("/Library/Base.gram", "")],
			OneConsumerFile,
			"C.FromCommon",
			["GRAM0017"],
			Common: CommonProject,
			CommonFiles: [("/Shared/Common.gram", CommonFile)],
			Also: ["C.FromCommon"]),

		// A generic class: its grammar is its name's without the type parameters, Base.gram, and the
		// consumer includes it constructed.
		["exported, generic"] = new("[DotGram.Gram(DefaultAlias = \"B\")]", [("/Library/Base.gram", LibraryFile)], [], "FromLibrary", [], Class: "Base<T>", Included: "Library.Base<int>"),
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
		var library  = $"namespace Library {{ {test.Attribute} public partial class {test.Class} {{ }} {test.LibraryMore} }}";
		var consumer = $"[DotGram.Gram(\"Dialect.gram\")] [DotGram.GramInclude(typeof({test.Included}))] public partial class Dialect {{ }}";
		var embedded = $"[DotGram.Gram(\"Start = B.Word\\nparse Start\")] [DotGram.GramInclude(typeof({test.Included}))] public partial class Embedded {{ }}";

		// The generator: the library compiled with it, and the consumer against the library's assembly
		// (and the third project's, which a build hands it with the library's).
		var commonImage = test.Common is null ? null : Generate("Shared", test.Common, test.CommonFiles ?? [], [], out _, out _);
		MetadataReference[] below = commonImage is null ? [] : [MetadataReference.CreateFromImage(commonImage)];
		var image   = Generate("Library", library, test.LibraryFiles, below, out _, out var built);
		var carried = built.GetTypeByMetadataName(test.Class == "Base" ? "Library.Base" : "Library.Base`1")!.GetAttributes()
			.FirstOrDefault(static attribute => attribute.AttributeClass?.Name == "GramSourceAttribute")?.ConstructorArguments[0].Value as string;
		var told    = new Dictionary<string, string[]>();

		foreach (var rule in Rules)
		{
			Generate(
				"Consumer",
				consumer,
				[("/Consumer/Dialect.gram", $"Start = {rule}\nparse Start\n"), .. test.ConsumerFiles],
				[MetadataReference.CreateFromImage(image), .. below],
				out var diagnostics,
				out _);

			var reached = !diagnostics.Any(static diagnostic => diagnostic.Id == "GRAM3002");

			Assert.True(reached == test.Reaches.Contains(rule), $"The generator {(reached ? "reaches" : "does not reach")} {rule}.");

			told[rule] = [.. diagnostics.Select(static diagnostic => diagnostic.Id).Where(static id => id.StartsWith("GRAM", StringComparison.Ordinal)).Distinct().Order(StringComparer.Ordinal)];

			// Less the warning every case has: the included grammar's rules the consumer does not reach.
			if (rule == "B." + (test.Reached ?? "FromLibrary"))
				Assert.Equal(test.Diagnostics, told[rule].Where(static id => id != "GRAM4018"));
		}

		// The editors: the projects of one solution, the consumer's grammar a file and one in C#.
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

		if (test.Common is not null)
		{
			var commonId = Add("Shared", test.Common);

			solution = solution.AddProjectReference(libraryId, new ProjectReference(commonId));

			foreach (var (path, text) in test.CommonFiles ?? [])
				solution = solution.AddAdditionalDocument(DocumentId.CreateNewId(commonId), Path.GetFileName(path), SourceText.From(text), filePath: path);
		}

		solution = solution
			.AddProjectReference(consumerId, new ProjectReference(libraryId))
			.AddDocument(embeddedId, "Embedded.cs", SourceText.From(embedded))
			.AddAdditionalDocument(DocumentId.CreateNewId(consumerId), "Dialect.gram", SourceText.From("Start = B.Word\nparse Start\n"), filePath: "/Consumer/Dialect.gram");

		foreach (var (path, text) in test.LibraryFiles)
			solution = solution.AddAdditionalDocument(DocumentId.CreateNewId(libraryId), Path.GetFileName(path), SourceText.From(text), filePath: path);

		foreach (var (path, text) in test.ConsumerFiles)
			solution = solution.AddAdditionalDocument(DocumentId.CreateNewId(consumerId), Path.GetFileName(path), SourceText.From(text), filePath: path);

		// The text the generator compiles B with: what the library carries, the consumer's file, or none.
		var expected = test.Reached switch
		{
			null           => null,
			"FromConsumer" => ConsumerFile,
			_              => carried,
		};

		// A grammar file's host, as both editors read it: B is the text the generator compiles it with,
		// and every grammar diagnostic the generator reports of the consumer's grammar the editor shows.
		var resolution = await StandaloneGrammarInheritance.ResolveAsync(solution, "/Consumer/Dialect.gram", TestContext.Current.CancellationToken);
		var context    = Assert.NotNull(resolution.Context);

		Assert.Equal(expected, context.Included.SingleOrDefault(static item => item.Name == "B").Text);

		foreach (var rule in Rules)
		{
			var shown = GramAnalysisSession.Analyze($"Start = {rule}\nparse Start\n", context, suppressContextDiagnostics: false).Document.Diagnostics
				.Select(static diagnostic => diagnostic.Id)
				.Distinct()
				.Order(StringComparer.Ordinal);

			// What only the build says: the host's file not found (GRAM0003), found twice (GRAM0004), or
			// a local file that is not the grammar a referenced assembly carries (GRAM0017); and the
			// carried rules the consumer leaves unreached (GRAM4018), told of the build's whole grammar.
			Assert.True(
				told[rule].Where(static id => id is not ("GRAM0003" or "GRAM0004" or "GRAM0017" or "GRAM4018")).SequenceEqual(shown),
				$"For {rule} the generator tells [{string.Join(", ", told[rule])}] and the editor [{string.Join(", ", shown)}].");
		}

		// A grammar in C#, as Visual Studio reads it.
		var document = solution.GetDocument(embeddedId)!;
		var found    = Assert.Single(await EmbeddedAsync(document));
		// Exactly the generator's: B's text, and C's where the walk reaches it, each what its assembly carries.
		GrammarSplice.Part[] parts =
		[
			.. expected is null ? [] : new GrammarSplice.Part[] { new(expected, "B", null) },
			.. test.Reaches.Contains("C.FromCommon") ? new GrammarSplice.Part[] { new(CommonFile, "C", null) } : [],
		];
		var joined = parts.Length == 0 ? "Start = B.Word\nparse Start" : GrammarSplice.Join(new("Start = B.Word\nparse Start", null, null), parts).Text;

		Assert.Equal(joined, found.AnalysisText);
	}

	/// <summary>The grammars of a C# document as the embedded session finds them.</summary>
	static async Task<IReadOnlyList<EmbeddedGrammar>> EmbeddedAsync(Document document)
	{
		var model = await document.GetSemanticModelAsync(TestContext.Current.CancellationToken);
		var root  = await document.GetSyntaxRootAsync(TestContext.Current.CancellationToken);

		return await EmbeddedGrammarFinder.FindAsync(model!, root!, SolutionIncludedGrammars.For(document.Project), TestContext.Current.CancellationToken);
	}

	/// <summary>Runs the generator on one compilation; its image, which must build.</summary>
	static byte[] Generate(
		string name,
		string source,
		(string Path, string Text)[] files,
		MetadataReference[] references,
		out ImmutableArray<Diagnostic> diagnostics,
		out Compilation built)
	{
		var compilation = CSharpCompilation.Create(
			name,
			[CSharpSyntaxTree.ParseText(source, ParseOptions, name + ".cs", cancellationToken: TestContext.Current.CancellationToken)],
			[.. Framework, .. references],
			new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

		CSharpGeneratorDriver
			.Create([Generator.Value.AsSourceGenerator()], [.. files.Select(static file => (AdditionalText)new File(file.Path, file.Text))], ParseOptions)
			.RunGeneratorsAndUpdateCompilation(compilation, out var generated, out diagnostics, TestContext.Current.CancellationToken);

		built = generated;

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
