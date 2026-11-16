using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;

namespace DotGram.Language;

/// <summary>The text an include is compiled with, where it came from, and the file it is, where it is one.</summary>
readonly record struct IncludedGrammarChoice(IncludedGrammarOrigin Origin, string? Text, string? FilePath)
{
	/// <summary>The choice among what the class carries and its own reading, by the generator's order (<see cref="GramIncludes.Choose"/>).</summary>
	public static IncludedGrammarChoice Of(bool placed, (string? Text, string? Path) carried, (string? Text, string? Path) resolved)
	{
		var origin = GramIncludes.Choose(placed, carried.Text, () => resolved.Text is not null);

		return origin switch
		{
			IncludedGrammarOrigin.Carried  => new IncludedGrammarChoice(origin, carried.Text, carried.Path),
			IncludedGrammarOrigin.Resolved => new IncludedGrammarChoice(origin, resolved.Text, resolved.Path),
			_                              => new IncludedGrammarChoice(origin, null, null),
		};
	}
}

/// <summary>
/// What a project's compilation answers about a grammar it includes, in the generator's terms: whether
/// the class is placed in it, what the class carries, and the class's own reading resolved there — its
/// inline grammar or the project's one file of the name. Every editor asks one of these, and chooses
/// among the answers by <see cref="GramIncludes.Choose"/>, the generator's own order.
/// </summary>
abstract class IncludedGrammarSources
{
	/// <summary>The text the generator would compile the include with here.</summary>
	public abstract Task<IncludedGrammarChoice> SelectAsync(GramInclude include, CancellationToken cancellationToken);

	/// <summary>A class the walk of a host's includes passed: what follows a source's answers follows it too.</summary>
	public virtual void Visited(INamedTypeSymbol type)
	{
	}

	/// <summary>
	/// What rebinds a class to the compilation that declares it, for <see cref="GramIncludes.Walk"/>;
	/// null where every class is already its own compilation's.
	/// </summary>
	public virtual Task<Func<INamedTypeSymbol, INamedTypeSymbol>?> RebinderAsync(CancellationToken cancellationToken)
	{
		return Task.FromResult<Func<INamedTypeSymbol, INamedTypeSymbol>?>(null);
	}

	/// <summary>Every include of a host and the text each is compiled with, the host's includes walked as its declaring compilations have them.</summary>
	public async Task<IReadOnlyList<(GramInclude Include, IncludedGrammarChoice Choice)>> IncludesAsync(INamedTypeSymbol host, CancellationToken cancellationToken)
	{
		var rebind = await RebinderAsync(cancellationToken).ConfigureAwait(false);
		var result = new List<(GramInclude, IncludedGrammarChoice)>();

		foreach (var include in GramIncludes.Walk(host, Visited, rebind))
			result.Add((include, await SelectAsync(include, cancellationToken).ConfigureAwait(false)));

		return result;
	}

	/// <summary>The class's own reading of its grammar where it is inline; null where its source names a file.</summary>
	protected static string? Inline(GramInclude include)
	{
		return include.Source is { } written && !GramIncludes.IsPath(written) ? written : null;
	}
}

/// <summary>
/// One compilation and its project's files, without the solution around them: a class of another
/// assembly carries what its <c>[GramSource]</c> says, as the generator wrote it there or a person did.
/// </summary>
sealed class CompilationIncludedGrammars(Compilation compilation, Func<string, string?>? file) : IncludedGrammarSources
{
	public IncludedGrammarChoice Select(GramInclude include)
	{
		var placed   = include.Type.DeclaringSyntaxReferences.Any(reference => compilation.ContainsSyntaxTree(reference.SyntaxTree));
		var resolved = Inline(include) ?? file?.Invoke(GramIncludes.Wanted(include));

		return IncludedGrammarChoice.Of(placed, (include.Portable, null), (resolved, null));
	}

	public override Task<IncludedGrammarChoice> SelectAsync(GramInclude include, CancellationToken cancellationToken)
	{
		return Task.FromResult(Select(include));
	}
}

/// <summary>What an answer depended on, for a caller that follows it: the documents and the projects.</summary>
sealed class IncludedGrammarDependencies
{
	readonly object _gate = new();
	readonly HashSet<DocumentId> _documents = [];
	readonly HashSet<ProjectId> _projects = [];

	public void Add(DocumentId document)
	{
		lock (_gate)
		{
			_documents.Add(document);
			_projects.Add(document.ProjectId);
		}
	}

	public void Add(ProjectId project)
	{
		lock (_gate)
			_projects.Add(project);
	}

	public void Add(IncludedGrammarDependencies other)
	{
		var (documents, projects) = other.Snapshot();

		lock (_gate)
		{
			_documents.UnionWith(documents);
			_projects.UnionWith(projects);
		}
	}

	public (IReadOnlyCollection<DocumentId> Documents, IReadOnlyCollection<ProjectId> Projects) Snapshot()
	{
		lock (_gate)
			return ([.. _documents], [.. _projects]);
	}
}

/// <summary>
/// A project of a solution. A class another project of the solution declares is, to the build, a class
/// of a referenced assembly, and carries what that project's generator writes into
/// <c>[GramSource]</c> where it exports the grammar: the class's grammar as that project reads it,
/// with what it includes spliced on (<see cref="GrammarSplice"/>), each chosen as that project chooses
/// it. That text is computed here, since no generator runs to write it, from the class as the declaring
/// project's own compilation has it — its includes resolve there, not in the asking project — and is
/// kept for as long as nothing that project depends on changes.
/// </summary>
/// <remarks>
/// A generator writes the attribute only for a grammar that compiles; a library whose grammar does not
/// fails its own build, and its consumers' with it, so that is not asked here. Only what an answer
/// needs is read: the asking project's files of the name, and the declaring projects' grammars.
/// </remarks>
sealed class SolutionIncludedGrammars : IncludedGrammarSources
{
	/// <summary>What a project's generator would carry on a class, by the class's metadata name, with the project version it holds for.</summary>
	sealed class Exports
	{
		public Dictionary<string, (VersionStamp Version, string? Text, string? Path, IncludedGrammarDependencies Dependencies)> Classes { get; } = new(StringComparer.Ordinal);
	}

	static readonly ConditionalWeakTable<ProjectId, Exports> Exported = new();

	readonly Project _project;

	SolutionIncludedGrammars(Project project, IncludedGrammarDependencies dependencies)
	{
		_project     = project;
		Dependencies = dependencies;
	}

	/// <summary>What the answers so far depended on.</summary>
	public IncludedGrammarDependencies Dependencies { get; }

	/// <summary>A project's sources; nothing is read until an include is asked about.</summary>
	public static SolutionIncludedGrammars For(Project project)
	{
		return new SolutionIncludedGrammars(project, new IncludedGrammarDependencies());
	}

	public override void Visited(INamedTypeSymbol type)
	{
		foreach (var declaration in type.DeclaringSyntaxReferences)
			if (_project.Solution.GetDocumentId(declaration.SyntaxTree) is { } declared)
				Dependencies.Add(declared);
	}

	public override async Task<IncludedGrammarChoice> SelectAsync(GramInclude include, CancellationToken cancellationToken)
	{
		var declared = Declaring(include.Type);
		var placed   = declared == _project.Id;
		var carried  = await CarriedAsync(include, declared, cancellationToken).ConfigureAwait(false);
		var resolved = Inline(include) is { } inline
			? (inline, null)
			: await FileAsync(_project, GramIncludes.Wanted(include), Dependencies, cancellationToken).ConfigureAwait(false);

		return IncludedGrammarChoice.Of(placed, carried, resolved);
	}

	/// <summary>
	/// The classes of the projects this one depends on, as each project's own compilation has them: the
	/// compilations are the ones the project's references are made of, so asking for them builds nothing.
	/// </summary>
	public override async Task<Func<INamedTypeSymbol, INamedTypeSymbol>?> RebinderAsync(CancellationToken cancellationToken)
	{
		var solution     = _project.Solution;
		var compilations = new Dictionary<ProjectId, Compilation>();

		foreach (var id in solution.GetProjectDependencyGraph().GetProjectsThatThisProjectTransitivelyDependsOn(_project.Id))
			if (solution.GetProject(id) is { } project && await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false) is { } compilation)
				compilations[id] = compilation;

		return type =>
		{
			if (Declaring(type) is not { } declared || declared == _project.Id || !compilations.TryGetValue(declared, out var compilation))
				return type;

			return compilation.Assembly.GetTypeByMetadataName(MetadataName(type.OriginalDefinition)) ?? type;
		};
	}

	/// <summary>The project of the solution that declares a class; null where it is only in metadata.</summary>
	ProjectId? Declaring(INamedTypeSymbol type)
	{
		foreach (var declaration in type.DeclaringSyntaxReferences)
			if (_project.Solution.GetDocumentId(declaration.SyntaxTree) is { } declared)
				return declared.ProjectId;

		return null;
	}

	async Task<(string? Text, string? Path)> CarriedAsync(GramInclude include, ProjectId? declared, CancellationToken cancellationToken)
	{
		if (declared is not null)
			Dependencies.Add(declared);

		// Written on the class: a [GramSource] somebody wrote, or the one a generator has written there,
		// opened as the declaring project's file where it is that file's text.
		if (include.Portable is { Length: > 0 } written)
			return (written, declared is null ? null : await EqualFileAsync(_project.Solution.GetProject(declared)!, written, cancellationToken).ConfigureAwait(false));

		if (declared is null || declared == _project.Id)
			return (include.Portable, null);

		if (!GramHostOptions.Read(include.Attribute, include.Type, first: null).ExportGrammar)
			return (null, null);

		var project = _project.Solution.GetProject(declared)!;
		var version = await project.GetDependentVersionAsync(cancellationToken).ConfigureAwait(false);
		var exports = Exported.GetValue(declared, static _ => new Exports());
		var name    = MetadataName(include.Type.OriginalDefinition);

		lock (exports)
			if (exports.Classes.TryGetValue(name, out var known) && known.Version == version)
			{
				Dependencies.Add(known.Dependencies);
				return (known.Text, known.Path);
			}

		var dependencies = new IncludedGrammarDependencies();
		var (text, path) = await ExportAsync(project, include, name, dependencies, cancellationToken).ConfigureAwait(false);

		lock (exports)
			exports.Classes[name] = (version, text, path, dependencies);

		Dependencies.Add(dependencies);

		return (text, path);
	}

	/// <summary>
	/// What the declaring project's generator writes into the class's <c>[GramSource]</c>: its grammar as
	/// that project resolves it — inline, or the file its source names, by default its name without type
	/// parameters (<see cref="GramIncludes.HostFileName"/>) — with its includes spliced on, each chosen as
	/// that project chooses it, from the class as that project's compilation has it.
	/// </summary>
	static async Task<(string? Text, string? Path)> ExportAsync(
		Project project,
		GramInclude include,
		string name,
		IncludedGrammarDependencies dependencies,
		CancellationToken cancellationToken)
	{
		dependencies.Add(project.Id);

		var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
		var type        = compilation?.Assembly.GetTypeByMetadataName(name) ?? include.Type.OriginalDefinition;
		var (own, path) = Inline(include) is { } inline
			? (inline, null)
			: await FileAsync(project, include.Source ?? GramIncludes.HostFileName(type.ToDisplayString()), dependencies, cancellationToken).ConfigureAwait(false);

		if (own is null)
			return (null, null);

		var there = new SolutionIncludedGrammars(project, dependencies);
		var parts = new List<GrammarSplice.Part>();

		foreach (var (inner, choice) in await there.IncludesAsync(type, cancellationToken).ConfigureAwait(false))
			if (choice.Text is { } text)
				parts.Add(new GrammarSplice.Part(text, inner.Name, null));

		return parts.Count == 0
			? (own, path)
			: (GrammarSplice.Join(new GrammarSplice.Part(own, null, null), parts).Text, null);
	}

	/// <summary>The text of the one grammar file of a project a source names, and its path; nothing where none or several do.</summary>
	static async Task<(string? Text, string? Path)> FileAsync(
		Project project,
		string wanted,
		IncludedGrammarDependencies dependencies,
		CancellationToken cancellationToken)
	{
		var found = project.AdditionalDocuments
			.Where(document => document.FilePath is { } path && GramIncludes.Matches(path, wanted))
			.Take(2)
			.ToArray();

		// A file passed over is followed too: its going makes the name one file's.
		foreach (var document in found)
			dependencies.Add(document.Id);

		if (found.Length != 1)
			return (null, null);

		return ((await found[0].GetTextAsync(cancellationToken).ConfigureAwait(false)).ToString(), found[0].FilePath);
	}

	/// <summary>The grammar file of a project whose text a carried grammar is, where one is.</summary>
	static async Task<string?> EqualFileAsync(Project project, string text, CancellationToken cancellationToken)
	{
		foreach (var document in project.AdditionalDocuments)
			if (document.FilePath is { } path &&
				path.EndsWith(GramIncludes.GramFileExtension, StringComparison.OrdinalIgnoreCase) &&
				(await document.GetTextAsync(cancellationToken).ConfigureAwait(false)).ToString() == text)
				return path;

		return null;
	}

	/// <summary>A class's name as metadata has it: its namespace, the classes it is nested in, and its arity.</summary>
	static string MetadataName(INamedTypeSymbol type)
	{
		var name = type.MetadataName;

		for (var outer = type.ContainingType; outer is not null; outer = outer.ContainingType)
			name = outer.MetadataName + "+" + name;

		return type.ContainingNamespace is { IsGlobalNamespace: false } space ? space.ToDisplayString() + "." + name : name;
	}
}
