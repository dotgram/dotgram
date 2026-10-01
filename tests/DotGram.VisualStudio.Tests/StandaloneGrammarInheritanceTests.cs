using System;
using System.Linq;
using System.Threading.Tasks;

using DotGram.Grammar;
using DotGram.VisualStudio;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

using Xunit;

namespace DotGram.VisualStudio.Tests;

public sealed class StandaloneGrammarInheritanceTests
{
	const string Attributes = """
		namespace DotGram
		{
			[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
			sealed class GramAttribute(string source) : System.Attribute
			{
				public string IncludedAs { get; set; } = "";
				public bool Lexical { get; set; }
				public string Suffix { get; set; } = "";
			}

			sealed class GramSourceAttribute(string text) : System.Attribute;

			[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
			sealed class GramIncludeAttribute(System.Type grammar) : System.Attribute
			{
				public string As { get; set; } = "";
			}
		}
		""";

	const string Hosts = """
		[DotGram.Gram("SqlStandard92.gram", IncludedAs = "Sql92")]
		abstract class SqlStandard92;

		[DotGram.Gram("TransactSql.gram", Lexical = true)]
		[DotGram.GramInclude(typeof(SqlStandard92))]
		abstract class TransactSql;
		""";

	const string Standard = "Word = ['a'..'z']+\nnamespace Lexical\n{\nDigits = ['0'..'9']+\n}\nStart92 = Word\n";

	const string StandardPath = @"P:\Parsers\SqlStandard92.gram";
	const string DialectPath  = @"P:\Parsers\TransactSql.gram";

	static Project Parsers(AdhocWorkspace workspace, string declarations, string dialect = "Start = 'a'", params (string Path, string Text)[] more)
	{
		var project = workspace.AddProject(ProjectInfo.Create(
			ProjectId.CreateNewId(),
			VersionStamp.Default,
			"Parsers",
			"Parsers",
			LanguageNames.CSharp,
			parseOptions: CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview),
			metadataReferences: [MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location)]));
		project = project.AddDocument("Parsers.cs", SourceText.From(declarations), filePath: @"P:\Parsers\Parsers.cs").Project;
		project = project.AddAdditionalDocument("SqlStandard92.gram", SourceText.From(Standard), filePath: StandardPath).Project;
		project = project.AddAdditionalDocument("TransactSql.gram", SourceText.From(dialect), filePath: DialectPath).Project;

		foreach (var (path, text) in more)
			project = project.AddAdditionalDocument(System.IO.Path.GetFileName(path), SourceText.From(text), filePath: path).Project;

		Assert.True(workspace.TryApplyChanges(project.Solution));

		return workspace.CurrentSolution.GetProject(project.Id)!;
	}

	static StandaloneGrammarContext Context()
	{
		var (tail, map) = GrammarSplice.Join(
			new GrammarSplice.Part("", null, null),
			[new GrammarSplice.Part(Standard, "Sql92", null)]);

		return new StandaloneGrammarContext(
			tail,
			[new StandaloneIncludedGrammar("Sql92", Standard, StandardPath, map.Segments[1].Start)]);
	}

	[Fact]
	public async Task AppendsIncludedGrammarToAStandaloneDialect()
	{
		using var workspace = new AdhocWorkspace();
		var project = Parsers(workspace, Attributes + Hosts);

		var resolution = await StandaloneGrammarInheritance.ResolveAsync(
			project.Solution,
			DialectPath,
			TestContext.Current.CancellationToken);

		Assert.Equal(StandaloneGrammarState.Resolved, resolution.State);
		var context = Assert.NotNull(resolution.Context);
		Assert.Contains("namespace Sql92\n{\nWord = ['a'..'z']+", context.AnalysisTail, StringComparison.Ordinal);
		Assert.True(context.Lexical);

		var included = Assert.Single(context.Included);
		Assert.Equal("Sql92", included.Name);
		Assert.Equal(StandardPath, included.FilePath);
		Assert.Equal(Standard, context.AnalysisTail.Substring(included.Start, Standard.Length));

		// What is watched for a change: the host's declaration and the included file, in the
		// one project that holds them.
		Assert.Equal(new[] { project.Id }, resolution.Projects);
		Assert.Contains(project.Documents.Single().Id, resolution.Documents);
		Assert.Contains(project.AdditionalDocuments.Single(document => document.FilePath == StandardPath).Id, resolution.Documents);
	}

	/// <summary>
	/// A class spelled as the host whose attribute the compilation cannot bind yet — the
	/// generator has not run — is not settled; no class at all is no host.
	/// </summary>
	[Fact]
	public async Task TellsAHostNotReadyFromNoHost()
	{
		using var waiting = new AdhocWorkspace();
		var unbound = Parsers(waiting, Hosts);

		var notReady = await StandaloneGrammarInheritance.ResolveAsync(
			unbound.Solution, DialectPath, TestContext.Current.CancellationToken);

		using var plain = new AdhocWorkspace();
		var nobody = Parsers(plain, Attributes + "class Unrelated;");

		var noHost = await StandaloneGrammarInheritance.ResolveAsync(
			nobody.Solution, DialectPath, TestContext.Current.CancellationToken);

		Assert.Equal(StandaloneGrammarState.NotReady, notReady.State);
		Assert.Equal(StandaloneGrammarState.NoHost, noHost.State);
		Assert.Null(noHost.Context);
	}

	/// <summary>
	/// A suffixed reading hosts the file it names, and its <c>Lexical</c> is its own; the
	/// class's own reading is still the one an includer gets.
	/// </summary>
	[Fact]
	public async Task FindsTheHostThroughASuffixedReading()
	{
		const string hosts = """
			[DotGram.Gram("SqlStandard92.gram", IncludedAs = "Sql92")]
			abstract class SqlStandard92;

			[DotGram.Gram("Other.gram")]
			[DotGram.Gram("TransactSql.gram", Suffix = "Dialect", Lexical = true)]
			[DotGram.GramInclude(typeof(SqlStandard92))]
			abstract class TransactSql;
			""";

		using var workspace = new AdhocWorkspace();
		var project = Parsers(workspace, Attributes + hosts);

		var resolution = await StandaloneGrammarInheritance.ResolveAsync(
			project.Solution, DialectPath, TestContext.Current.CancellationToken);

		Assert.Equal(StandaloneGrammarState.Resolved, resolution.State);
		Assert.True(resolution.Context?.Lexical);
		Assert.Equal("Sql92", Assert.Single(resolution.Context!.Value.Included).Name);
	}

	/// <summary>
	/// A file name two files answer to is not guessed at: the generator refuses it and reads
	/// what the class carries, and so does the editor. A base class in between, with no
	/// grammar of its own, is watched all the same.
	/// </summary>
	[Fact]
	public async Task AnAmbiguousIncludedFileFallsBackOnWhatTheClassCarries()
	{
		const string hosts = """
			[DotGram.Gram("SqlStandard92.gram", IncludedAs = "Sql92")]
			[DotGram.GramSource("Carried = 'c'")]
			abstract class SqlStandard92;

			abstract class Between : SqlStandard92;

			[DotGram.Gram("TransactSql.gram")]
			abstract class TransactSql : Between;
			""";

		using var workspace = new AdhocWorkspace();
		var project = Parsers(workspace, Attributes + hosts, more: (@"P:\Parsers\Old\SqlStandard92.gram", "Old = 'o'"));

		var resolution = await StandaloneGrammarInheritance.ResolveAsync(
			project.Solution, DialectPath, TestContext.Current.CancellationToken);

		var included = Assert.Single(resolution.Context!.Value.Included);
		Assert.Equal("Carried = 'c'", included.Text);
		Assert.Null(included.FilePath);
		Assert.Contains(project.Documents.Single().Id, resolution.Documents);
	}

	[Fact]
	public void NavigatesFromTheDialectIntoTheIncludedFile()
	{
		const string dialect = "using Sql92;\nStart = Word & Sql92.Word & Sql92.Lexical.Digits\nparse Start";
		var context  = Context();
		var analysis = GramBufferAnalysis.Analyze(dialect, context, suppressContextDiagnostics: false);
		var symbols  = analysis.Document.Symbols;

		StandaloneDefinition? At(string word, int occurrence = 0)
		{
			var position = -1;

			for (var index = 0; index <= occurrence; index++)
				position = dialect.IndexOf(word, position + 1, StringComparison.Ordinal);

			return GramBufferAnalysis.ExternalDefinition(dialect, context, symbols, position);
		}

		// The included grammar's own name opens its file at the top.
		var alias = Assert.NotNull(At("Sql92"));
		Assert.Equal(StandardPath, alias.Target.Grammar.FilePath);
		Assert.Equal((0, 0), (alias.Target.Line, alias.Target.Column));

		// A name brought in by `using`, the same name qualified, and one qualified twice.
		var word = Assert.NotNull(At("Word"));
		Assert.Equal(StandardPath, word.Target.Grammar.FilePath);
		Assert.Equal((0, 0), (word.Target.Line, word.Target.Column));
		Assert.Equal(dialect.IndexOf("Word", StringComparison.Ordinal), word.Position);

		var qualified = Assert.NotNull(At("Word", 1));
		Assert.Equal((0, 0), (qualified.Target.Line, qualified.Target.Column));

		var digits = Assert.NotNull(At("Digits"));
		Assert.Equal(StandardPath, digits.Target.Grammar.FilePath);
		Assert.Equal((3, 0), (digits.Target.Line, digits.Target.Column));
		Assert.Equal(dialect.IndexOf("Digits", StringComparison.Ordinal), digits.Position);
		Assert.Equal("Digits".Length, digits.Length);

		// A rule of the dialect's own is not external.
		Assert.Null(At("Start"));
	}

	[Fact]
	public void FindsReferencesInTheIncludedGrammar()
	{
		const string dialect = "using Sql92;\nStart = Word\nparse Start";
		var context  = Context();
		var analysis = GramBufferAnalysis.Analyze(dialect, context, suppressContextDiagnostics: false);
		var word     = analysis.Document.Symbols.Single(symbol => symbol.Name == "Word");

		var references = GramBufferAnalysis.IncludedReferences(
			analysis.Included, context, dialect.Length, word.Name, word.DefinitionPosition);

		// The definition on the first line of the included file, and its use in `Start92`.
		Assert.Equal(
			new[] { (0, 0, true), (5, 10, false) },
			references.Select(reference => (reference.Location.Line, reference.Location.Column, reference.IsDefinition)));
		Assert.All(references, reference => Assert.Equal(StandardPath, reference.Location.Grammar.FilePath));
		Assert.All(references, reference => Assert.Equal("Word", Standard.Substring(reference.Location.Offset, reference.Length)));
	}

	[Fact]
	public void ADialectIsCleanOnceWhatItIncludesIsFoundAndQuietUntilThen()
	{
		const string dialect = "using Sql92;\nStart = Sql92.Word & Word\nparse Start";

		var found = GramBufferAnalysis.Analyze(dialect, Context(), suppressContextDiagnostics: false);
		var pending = GramBufferAnalysis.Analyze(dialect, null, suppressContextDiagnostics: true);

		Assert.Empty(found.Document.Diagnostics);
		Assert.Empty(pending.Document.Diagnostics);
	}

	/// <summary>
	/// Which changes to the solution send the host to be looked for again: those to what the
	/// grammar is compiled with, and not to its own text or to unrelated projects.
	/// </summary>
	[Fact]
	public async Task LooksForTheHostAgainOnlyWhenSomethingItDependsOnChanges()
	{
		using var workspace = new AdhocWorkspace();
		var project = Parsers(workspace, Attributes + Hosts);
		var other   = workspace.AddProject("Other", LanguageNames.CSharp);
		var stray   = workspace.AddDocument(other.Id, "Stray.cs", SourceText.From("class Stray;"));
		var before  = workspace.CurrentSolution;

		var resolution = await StandaloneGrammarInheritance.ResolveAsync(
			before, DialectPath, TestContext.Current.CancellationToken);

		var host     = project.Documents.Single();
		var standard = project.AdditionalDocuments.Single(document => document.FilePath == StandardPath);
		var own      = project.AdditionalDocuments.Single(document => document.FilePath == DialectPath);

		StandaloneGrammarConcern Concerns(WorkspaceChangeKind kind, ProjectId? changedProject, DocumentId? changedDocument, StandaloneGrammarResolution? state = null)
		{
			return GramBufferAnalysis.Concerns(
				new WorkspaceChangeEventArgs(kind, before, before, changedProject, changedDocument),
				state ?? resolution,
				DialectPath);
		}

		const StandaloneGrammarConcern full = StandaloneGrammarConcern.Full;
		const StandaloneGrammarConcern none = StandaloneGrammarConcern.None;

		Assert.Equal(full, Concerns(WorkspaceChangeKind.SolutionReloaded, null, null));
		Assert.Equal(full, Concerns(WorkspaceChangeKind.DocumentChanged, project.Id, host.Id));
		Assert.Equal(full, Concerns(WorkspaceChangeKind.AdditionalDocumentChanged, project.Id, standard.Id));
		Assert.Equal(full, Concerns(WorkspaceChangeKind.ProjectChanged, project.Id, null));

		Assert.Equal(none, Concerns(WorkspaceChangeKind.AdditionalDocumentChanged, project.Id, own.Id));
		Assert.Equal(none, Concerns(WorkspaceChangeKind.DocumentChanged, other.Id, stray.Id));
		Assert.Equal(none, Concerns(WorkspaceChangeKind.ProjectChanged, other.Id, null));

		// Until the host is found, an edit in the grammar's own project is looked at, one
		// document at a time; a document added there, or the grammar itself added to a
		// project, sends the host to be looked for; nothing in another project does.
		var noHost = StandaloneGrammarResolution.NoHost(project.Id);

		Assert.Equal(StandaloneGrammarConcern.Document, Concerns(WorkspaceChangeKind.DocumentChanged, project.Id, host.Id, noHost));
		Assert.Equal(full, Concerns(WorkspaceChangeKind.DocumentAdded, project.Id, host.Id, noHost));
		Assert.Equal(none, Concerns(WorkspaceChangeKind.DocumentChanged, other.Id, stray.Id, noHost));
		Assert.Equal(full, Concerns(WorkspaceChangeKind.AdditionalDocumentAdded, project.Id, own.Id, StandaloneGrammarResolution.NotReady));
		Assert.Equal(none, Concerns(WorkspaceChangeKind.DocumentChanged, project.Id, host.Id, StandaloneGrammarResolution.NotReady));

		// And a document is looked at for a class spelled as the host, by syntax alone.
		var root = await host.GetSyntaxRootAsync(TestContext.Current.CancellationToken);
		Assert.True(StandaloneGrammarInheritance.MayHost(root!, DialectPath));
		Assert.False(StandaloneGrammarInheritance.MayHost(root!, @"P:\Parsers\Unknown.gram"));
	}
}
