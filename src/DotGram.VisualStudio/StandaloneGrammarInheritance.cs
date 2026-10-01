using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Generation;
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

	/// <summary>Not settled, in a project that is known: the one that holds the grammar.</summary>
	public static StandaloneGrammarResolution NotReadyIn(ProjectId project)
	{
		return new StandaloneGrammarResolution(
			StandaloneGrammarState.NotReady, null, [project], Array.Empty<DocumentId>());
	}

	public static StandaloneGrammarResolution NoHost(ProjectId project)
	{
		return new StandaloneGrammarResolution(
			StandaloneGrammarState.NoHost, null, [project], Array.Empty<DocumentId>());
	}
}

/// <summary>Builds the included tail used to analyze a standalone grammar in its C# host context.</summary>
/// <remarks>
/// Which grammars are included, under which names and from which attribute is
/// <see cref="GramIncludes"/>, the generator's own walk; what is added here is reading them
/// out of a workspace rather than out of a compilation's additional files.
/// </remarks>
static class StandaloneGrammarInheritance
{
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
		var (host, hostDocument, reading, named) = await HostAsync(project, filePath, cancellationToken).ConfigureAwait(false);
		if (host is null)
			return named ? StandaloneGrammarResolution.NotReadyIn(project.Id) : StandaloneGrammarResolution.NoHost(project.Id);

		var projects  = new HashSet<ProjectId> { project.Id };
		var documents = new HashSet<DocumentId> { hostDocument!.Id };
		var included  = new List<StandaloneIncludedGrammar>();

		// Every class the walk passes is watched, a base without a grammar too: it may be
		// given one, or a [GramInclude], and then what is included changes.
		void Watch(INamedTypeSymbol type)
		{
			foreach (var declaration in type.DeclaringSyntaxReferences)
				if (solution.GetDocumentId(declaration.SyntaxTree) is { } declared)
				{
					documents.Add(declared);
					projects.Add(declared.ProjectId);
				}
		}

		foreach (var include in GramIncludes.Walk(host, Watch))
		{
			// The file first, from the host's own project, which is where the generator looks;
			// what the class carries is what there is across an assembly reference, and what
			// the generator falls back on where the file is missing or ambiguous.
			string? text = null;
			string? path = null;

			if (include.Source is { } written && !GramIncludes.IsPath(written))
				text = written;
			else
			{
				var wanted = GramIncludes.Wanted(include);
				var file   = FileDocument(project, wanted);

				if (file is not null)
				{
					text = (await file.GetTextAsync(cancellationToken).ConfigureAwait(false)).ToString();
					path = file.FilePath;
					documents.Add(file.Id);
				}
				else if (include.Portable is not null)
				{
					text = include.Portable;
					path = await CarriedFileAsync(solution, include.Type, wanted, include.Portable, cancellationToken).ConfigureAwait(false);
				}
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

		// The reading that names this file, and what it does not say it takes from the
		// class's own, as the generator reads a suffixed one.
		var lexical = Lexical(reading!) ?? (GramIncludes.Primary(host) is { } primary ? Lexical(primary) : null) ?? false;

		return new StandaloneGrammarResolution(
			StandaloneGrammarState.Resolved,
			new StandaloneGrammarContext(tail, included, lexical),
			projects,
			documents);
	}

	/// <summary>
	/// A lookup of the project's <c>.gram</c> files by a <c>[Gram("….gram")]</c> argument, for
	/// code that cannot wait on Roslyn: the texts are fetched now, and made strings only when
	/// one is asked for. A name two files answer to is answered by neither, as the generator
	/// refuses it.
	/// </summary>
	public static async Task<Func<string, string?>> GramFilesAsync(Project project, CancellationToken cancellationToken)
	{
		var files = new List<(string Path, SourceText Text)>();

		foreach (var document in project.AdditionalDocuments)
			if (document.FilePath is { } path && path.EndsWith(GramIncludes.GramFileExtension, StringComparison.OrdinalIgnoreCase))
				files.Add((path, await document.GetTextAsync(cancellationToken).ConfigureAwait(false)));

		return source =>
		{
			var found = files.Where(file => GramIncludes.Matches(file.Path, source)).ToArray();

			return found.Length == 1 ? found[0].Text.ToString() : null;
		};
	}

	/// <summary>
	/// The additional file of a project that a <c>[Gram("….gram")]</c> argument names, or null
	/// where none does or more than one does.
	/// </summary>
	public static TextDocument? FileDocument(Project project, string source)
	{
		var found = project.AdditionalDocuments
			.Where(candidate => candidate.FilePath is not null && GramIncludes.Matches(candidate.FilePath, source))
			.Take(2)
			.ToArray();

		return found.Length == 1 ? found[0] : null;
	}

	/// <summary>
	/// Whether a document declares a type spelled as this file's host: an attribute named like
	/// <c>Gram</c> whose argument names the file, or which has none and the class is named for it.
	/// Syntax only, so it is cheap enough to ask of every document an edit touches.
	/// </summary>
	public static bool MayHost(SyntaxNode root, string grammarPath)
	{
		// Down through namespaces and types only: a host is a type, and a type is never
		// inside a member.
		return root.DescendantNodes(static node =>
				node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax or TypeDeclarationSyntax)
			.OfType<TypeDeclarationSyntax>()
			.Any(declaration => MayHost(declaration, grammarPath));
	}

	/// <summary>
	/// The class whose attribute names this file, with the document declaring it and the
	/// reading that names it; or none, and whether some declaration spelled as one was found
	/// that the compilation cannot yet bind — the attribute comes from the generator, which
	/// may not have run.
	/// </summary>
	static async Task<(INamedTypeSymbol? Host, Document? Document, AttributeData? Reading, bool Named)> HostAsync(
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

			// This runs again whenever the project changes while the grammar has no host, so
			// walking every statement of every file would be paid on each edit.
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
				if (model.GetDeclaredSymbol(declaration, cancellationToken) is not INamedTypeSymbol type)
					continue;

				// Every reading, the suffixed ones too: `[Gram("B.gram", Suffix = "B")]` hosts
				// B.gram. One without an argument reads the class's own grammar.
				var primary = GramIncludes.Primary(type);

				foreach (var reading in GramIncludes.Grams(type))
				{
					var source = GramIncludes.Source(reading) ??
						(primary is not null && reading != primary ? GramIncludes.Source(primary) : null) ??
						type.Name + GramIncludes.GramFileExtension;

					if (GramIncludes.IsPath(source) && GramIncludes.Matches(grammarPath, source))
						return (type, document, reading, true);
				}
			}
		}

		return (null, null, null, named);
	}

	static bool MayHost(TypeDeclarationSyntax declaration, string grammarPath)
	{
		var defaultSource = declaration.Identifier.ValueText + GramIncludes.GramFileExtension;

		foreach (var attribute in declaration.AttributeLists.SelectMany(static list => list.Attributes))
		{
			var name = attribute.Name.ToString();
			if (name != "Gram" && name != "GramAttribute" &&
				!name.EndsWith(".Gram", StringComparison.Ordinal) &&
				!name.EndsWith(".GramAttribute", StringComparison.Ordinal))
				continue;

			var argument = attribute.ArgumentList?.Arguments.FirstOrDefault(
				static candidate => candidate.NameEquals is null);

			// A constant or an expression may name anything; the semantic model will tell.
			if (argument is null
				? GramIncludes.Matches(grammarPath, defaultSource)
				: argument.Expression is not LiteralExpressionSyntax { Token.Value: string source } ||
					GramIncludes.IsPath(source) && GramIncludes.Matches(grammarPath, source))
				return true;
		}

		return false;
	}

	static bool? Lexical(AttributeData reading)
	{
		return reading.NamedArguments
			.FirstOrDefault(static argument => argument.Key == "Lexical")
			.Value.Value as bool?;
	}

	/// <summary>
	/// The file a carried grammar was compiled from, where the class's own project is in the
	/// solution and still holds it unchanged: then a definition in it can be opened as the
	/// file rather than as a copy.
	/// </summary>
	static async Task<string?> CarriedFileAsync(
		Solution solution,
		INamedTypeSymbol type,
		string wanted,
		string carried,
		CancellationToken cancellationToken)
	{
		foreach (var declaration in type.DeclaringSyntaxReferences)
		{
			var declared = solution.GetDocumentId(declaration.SyntaxTree);
			var project  = declared is null ? null : solution.GetProject(declared.ProjectId);
			var file     = project is null ? null : FileDocument(project, wanted);

			if (file?.FilePath is not null &&
				(await file.GetTextAsync(cancellationToken).ConfigureAwait(false)).ToString() == carried)
				return file.FilePath;
		}

		return null;
	}
}
