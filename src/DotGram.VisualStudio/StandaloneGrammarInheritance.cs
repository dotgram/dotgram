using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.VisualStudio;

/// <summary>One grammar a standalone grammar's host includes, as it is spliced on.</summary>
/// <param name="Name">The namespace it is wrapped in: what the including grammar writes after <c>using</c>.</param>
/// <param name="Text">What is compiled: the file where the host's project has it, else what the class carries.</param>
/// <param name="FilePath">The file that text is, or null where it is only carried by an assembly.</param>
/// <param name="Start">Where the text begins in <see cref="StandaloneGrammarContext.AnalysisTail"/>.</param>
readonly record struct StandaloneIncludedGrammar(string Name, string Text, string? FilePath, int Start = 0)
{
	public int End => Start + Text.Length;

	/// <summary>The text's lines, worked out once however many places in it are asked about.</summary>
	public SourceText Lines { get; } = SourceText.From(Text);
}

/// <summary>What a standalone grammar is compiled with: what its host includes, and how it reads it.</summary>
readonly record struct StandaloneGrammarContext(
	string AnalysisTail,
	IReadOnlyList<StandaloneIncludedGrammar> Included,
	bool Lexical = false)
{
	/// <summary>Whether compiling with the other would say the same: the texts and the options.</summary>
	public bool SameAs(StandaloneGrammarContext other)
	{
		return Lexical == other.Lexical &&
			AnalysisTail == other.AnalysisTail &&
			Included.Select(static item => (item.Name, item.FilePath))
				.SequenceEqual(other.Included.Select(static item => (item.Name, item.FilePath)));
	}

	/// <summary>
	/// Which included grammar a position past the including grammar's own text is in, and
	/// where in it.
	/// </summary>
	/// <param name="position">A position in the whole that was analysed: the own text and then this tail.</param>
	/// <param name="ownLength">How long the own text was when it was analysed.</param>
	public StandaloneLocation? Locate(int position, int ownLength)
	{
		var inTail = position - ownLength;

		if (inTail < 0)
			return null;

		foreach (var included in Included)
			if (inTail >= included.Start && inTail <= included.End)
			{
				var offset = inTail - included.Start;
				var line   = included.Lines.Lines.GetLinePosition(offset);

				return new StandaloneLocation(included, offset, line.Line, line.Character);
			}

		return null;
	}
}

/// <summary>A place in an included grammar, as the editor opens it: zero-based line and column.</summary>
readonly record struct StandaloneLocation(StandaloneIncludedGrammar Grammar, int Offset, int Line, int Column);

enum StandaloneGrammarState
{
	/// <summary>Not settled yet: the project, or its generator, is not there to ask.</summary>
	NotReady,

	/// <summary>No class names this file as its grammar; it is read as it stands.</summary>
	NoHost,

	/// <summary>The host is found, and with it what the grammar includes.</summary>
	Resolved,
}

/// <summary>What a standalone grammar's host was found to be, and what to watch for it changing.</summary>
/// <param name="Projects">The host's project and every project declaring a grammar it includes.</param>
/// <param name="Documents">
/// The host's declaration, the included grammars' files and their classes' declarations:
/// an edit to any of them can change what the grammar is compiled with.
/// </param>
readonly record struct StandaloneGrammarResolution(
	StandaloneGrammarState State,
	StandaloneGrammarContext? Context,
	IReadOnlyCollection<ProjectId> Projects,
	IReadOnlyCollection<DocumentId> Documents)
{
	public static StandaloneGrammarResolution NotReady { get; } = new(
		StandaloneGrammarState.NotReady, null, Array.Empty<ProjectId>(), Array.Empty<DocumentId>());

	public static StandaloneGrammarResolution NoHost(ProjectId? project)
	{
		return new StandaloneGrammarResolution(
			StandaloneGrammarState.NoHost,
			null,
			project is null ? Array.Empty<ProjectId>() : [project],
			Array.Empty<DocumentId>());
	}
}

/// <summary>One grammar a host includes, as its attributes say, before any of it is read.</summary>
/// <param name="Source">The <c>[Gram]</c> argument: a <c>.gram</c> file's name, or the grammar's text.</param>
/// <param name="Carried">What the class carries in <c>[GramSource]</c>, which crosses an assembly reference.</param>
readonly record struct IncludedGrammarHost(INamedTypeSymbol Type, string Name, string Source, string? Carried);

/// <summary>Builds the included tail used to analyze a standalone grammar in its C# host context.</summary>
static class StandaloneGrammarInheritance
{
	const string GramAttribute = "DotGram.GramAttribute";
	const string GramIncludeAttribute = "DotGram.GramIncludeAttribute";
	const string GramSourceAttribute = "DotGram.GramSourceAttribute";

	public static async Task<StandaloneGrammarResolution> ResolveAsync(
		Solution solution,
		string filePath,
		CancellationToken cancellationToken)
	{
		var grammar = solution.Projects
			.SelectMany(static project => project.AdditionalDocuments)
			.FirstOrDefault(candidate => string.Equals(
				candidate.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
		if (grammar is null)
			return StandaloneGrammarResolution.NotReady;

		var project = grammar.Project;
		var (host, hostDocument, named) = await HostAsync(project, filePath, cancellationToken).ConfigureAwait(false);
		if (host is null)
			return named ? StandaloneGrammarResolution.NotReady : StandaloneGrammarResolution.NoHost(project.Id);

		var projects  = new HashSet<ProjectId> { project.Id };
		var documents = new HashSet<DocumentId> { hostDocument!.Id };
		var included  = new List<StandaloneIncludedGrammar>();

		foreach (var include in Includes(host))
		{
			foreach (var declaration in include.Type.DeclaringSyntaxReferences)
				if (solution.GetDocumentId(declaration.SyntaxTree) is { } declared)
				{
					documents.Add(declared);
					projects.Add(declared.ProjectId);
				}

			// The file first, from the host's own project, which is where the generator looks;
			// what the class carries is what there is across an assembly reference.
			string? text = null;
			string? path = null;

			if (IsFile(include.Source))
			{
				var file = FileDocument(project, include.Source);
				if (file is not null)
				{
					text = (await file.GetTextAsync(cancellationToken).ConfigureAwait(false)).ToString();
					path = file.FilePath;
					documents.Add(file.Id);
				}
			}
			else
				text = include.Source;

			if (text is null && include.Carried is not null)
			{
				text = include.Carried;
				path = await CarriedFileAsync(solution, include, cancellationToken).ConfigureAwait(false);
			}

			if (text is not null)
				included.Add(new StandaloneIncludedGrammar(include.Name, text, path));
		}

		var (tail, map) = GrammarSplice.Join(
			new GrammarSplice.Part("", null, null),
			included.Select(static item => new GrammarSplice.Part(item.Text, item.Name, null)).ToArray());

		// Segment 0 is the empty own text the tail was joined onto; each included grammar
		// follows in order.
		for (var index = 0; index < included.Count; index++)
			included[index] = included[index] with { Start = map.Segments[index + 1].Start };

		var lexical = PrimaryGram(host.GetAttributes())?.NamedArguments
			.FirstOrDefault(static argument => argument.Key == "Lexical")
			.Value.Value as bool? ?? false;

		return new StandaloneGrammarResolution(
			StandaloneGrammarState.Resolved,
			new StandaloneGrammarContext(tail, included, lexical),
			projects,
			documents);
	}

	/// <summary>Every grammar a host includes, nearest first, as the generator walks them.</summary>
	/// <remarks>
	/// Base classes and <c>[GramInclude]</c> both, each walked in turn, and a class already
	/// gathered is not gathered again. A class with no <c>[Gram]</c> of its own is walked past.
	/// What the included grammar is called is the includer's <c>As</c>, else its own
	/// <c>IncludedAs</c>, else its class name.
	/// </remarks>
	public static IReadOnlyList<IncludedGrammarHost> Includes(INamedTypeSymbol host)
	{
		var included = new List<IncludedGrammarHost>();
		var seen     = new HashSet<string>(StringComparer.Ordinal) { host.ToDisplayString() };
		var pending  = new Queue<(INamedTypeSymbol Type, string? As)>();

		for (var above = host.BaseType; above is not null; above = above.BaseType)
			pending.Enqueue((above, null));

		foreach (var named in NamedIncludes(host))
			pending.Enqueue(named);

		while (pending.Count > 0)
		{
			var (current, called) = pending.Dequeue();
			if (!seen.Add(current.ToDisplayString()))
				continue;

			foreach (var nested in NamedIncludes(current))
				pending.Enqueue(nested);

			var attribute = PrimaryGram(current.GetAttributes());
			if (attribute is null)
				continue;

			var source = attribute.ConstructorArguments.Length == 0
				? current.Name + ".gram"
				: attribute.ConstructorArguments[0].Value as string;
			if (source is null)
				continue;

			var carried = current.GetAttributes().FirstOrDefault(static candidate =>
				candidate.AttributeClass?.ToDisplayString() == GramSourceAttribute)?
				.ConstructorArguments.FirstOrDefault().Value as string;
			var name = called ?? attribute.NamedArguments
				.FirstOrDefault(static argument => argument.Key == "IncludedAs")
				.Value.Value as string ?? current.Name;

			included.Add(new IncludedGrammarHost(current, name, source, carried));
		}

		return included;
	}

	public static bool IsFile(string source)
	{
		return source.EndsWith(".gram", StringComparison.OrdinalIgnoreCase) &&
		source.IndexOf('\r') < 0 && source.IndexOf('\n') < 0;
	}

	/// <summary>
	/// A lookup of the project's <c>.gram</c> files by a <c>[Gram("….gram")]</c> argument, for
	/// code that cannot wait on Roslyn: the texts are fetched now, and made strings only when
	/// one is asked for.
	/// </summary>
	public static async Task<Func<string, string?>> GramFilesAsync(Project project, CancellationToken cancellationToken)
	{
		var files = new List<(string Path, SourceText Text)>();

		foreach (var document in project.AdditionalDocuments)
			if (document.FilePath is { } path && path.EndsWith(".gram", StringComparison.OrdinalIgnoreCase))
				files.Add((path, await document.GetTextAsync(cancellationToken).ConfigureAwait(false)));

		return source =>
		{
			foreach (var (path, text) in files)
				if (Matches(path, source))
					return text.ToString();

			return null;
		};
	}

	/// <summary>The additional file of a project that a <c>[Gram("….gram")]</c> argument names.</summary>
	public static TextDocument? FileDocument(Project project, string source)
	{
		return project.AdditionalDocuments.FirstOrDefault(candidate =>
			candidate.FilePath is not null && Matches(candidate.FilePath, source));
	}

	static IEnumerable<(INamedTypeSymbol Type, string? As)> NamedIncludes(INamedTypeSymbol type)
	{
		foreach (var attribute in type.GetAttributes())
			if (attribute.AttributeClass?.ToDisplayString() == GramIncludeAttribute &&
				attribute.ConstructorArguments is [{ Value: INamedTypeSymbol grammar }])
			{
				yield return (
					grammar,
					attribute.NamedArguments
						.FirstOrDefault(static argument => argument.Key == "As")
						.Value.Value as string);
			}
	}

	/// <summary>
	/// The class whose attribute names this file, with the document declaring it; or none, and
	/// whether some declaration spelled as one was found that the compilation cannot yet bind —
	/// the attribute comes from the generator, which may not have run.
	/// </summary>
	static async Task<(INamedTypeSymbol? Host, Document? Document, bool Named)> HostAsync(
		Project project,
		string grammarPath,
		CancellationToken cancellationToken)
	{
		var named = false;

		foreach (var document in project.Documents)
		{
			var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			if (root is null)
				continue;

			// Down through namespaces and types only: a host is a type, and a type is never
			// inside a member. This runs again whenever the project changes while the grammar
			// has no host, so walking every statement of every file would be paid on each edit.
			var candidates = root.DescendantNodes(static node =>
					node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax or TypeDeclarationSyntax)
				.OfType<TypeDeclarationSyntax>()
				.Where(declaration => MayHost(declaration, grammarPath))
				.ToArray();
			if (candidates.Length == 0)
				continue;

			named = true;

			// Getting a semantic model can force Roslyn to finish the project's compilation.
			// Large parser projects contain hundreds of unrelated source files, so first use
			// the attribute syntax to reduce that work to the file which names this grammar.
			var model = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			if (model is null)
				continue;

			foreach (var declaration in candidates)
			{
				var type = model.GetDeclaredSymbol(declaration, cancellationToken) as INamedTypeSymbol;
				var attribute = type is null ? null : PrimaryGram(type.GetAttributes());
				if (type is null || attribute is null)
					continue;

				var source = attribute.ConstructorArguments.Length == 0
					? type.Name + ".gram"
					: attribute.ConstructorArguments[0].Value as string;
				if (source is not null && IsFile(source) && Matches(grammarPath, source))
					return (type, document, true);
			}
		}

		return (null, null, named);
	}

	static bool MayHost(TypeDeclarationSyntax declaration, string grammarPath)
	{
		var defaultSource = declaration.Identifier.ValueText + ".gram";
		foreach (var attribute in declaration.AttributeLists.SelectMany(static list => list.Attributes))
		{
			var name = attribute.Name.ToString();
			if (name != "Gram" && name != "GramAttribute" &&
				!name.EndsWith(".Gram", StringComparison.Ordinal) &&
				!name.EndsWith(".GramAttribute", StringComparison.Ordinal))
				continue;

			var argument = attribute.ArgumentList?.Arguments.FirstOrDefault();
			if (argument?.Expression is LiteralExpressionSyntax literal &&
				literal.Token.Value is string source)
				return IsFile(source) && Matches(grammarPath, source);

			return Matches(grammarPath, defaultSource);
		}

		return false;
	}

	/// <summary>
	/// The class's own <c>[Gram]</c>, where it has several readings: the one without a
	/// <c>Suffix</c>, which is the class's own compilation, as the generator chooses it.
	/// </summary>
	static AttributeData? PrimaryGram(IEnumerable<AttributeData> attributes)
	{
		var grams = attributes
			.Where(static attribute => attribute.AttributeClass?.ToDisplayString() == GramAttribute)
			.ToArray();

		return grams.FirstOrDefault(static attribute =>
			attribute.NamedArguments.All(static named => named.Key != "Suffix")) ?? grams.FirstOrDefault();
	}

	/// <summary>
	/// The file a carried grammar was compiled from, where the class's own project is in the
	/// solution and still holds it unchanged: then a definition in it can be opened as the
	/// file rather than as a copy.
	/// </summary>
	static async Task<string?> CarriedFileAsync(
		Solution solution,
		IncludedGrammarHost include,
		CancellationToken cancellationToken)
	{
		if (!IsFile(include.Source))
			return null;

		foreach (var declaration in include.Type.DeclaringSyntaxReferences)
		{
			var declared = solution.GetDocumentId(declaration.SyntaxTree);
			var project  = declared is null ? null : solution.GetProject(declared.ProjectId);
			var file     = project is null ? null : FileDocument(project, include.Source);

			if (file?.FilePath is not null &&
				(await file.GetTextAsync(cancellationToken).ConfigureAwait(false)).ToString() == include.Carried)
				return file.FilePath;
		}

		return null;
	}

	static bool Matches(string filePath, string wanted)
	{
		var path = filePath.Replace('/', '\\');
		var suffix = wanted.Replace('/', '\\');
		if (!path.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
			return false;

		var boundary = path.Length - suffix.Length - 1;
		return boundary < 0 || path[boundary] == '\\';
	}
}
