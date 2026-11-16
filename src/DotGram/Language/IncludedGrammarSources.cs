using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.Language;

/// <summary>The text an include is compiled with, where it came from, and the file it is, where it is one.</summary>
readonly record struct IncludedGrammarChoice(IncludedGrammarOrigin Origin, string? Text, string? FilePath);

/// <summary>
/// What a project's compilation answers about a grammar it includes, in the generator's terms: whether
/// the class is placed in it, what the class carries, and the project's file of a name. Every editor
/// asks one of these, and the choice among the three is <see cref="GramIncludes.Choose"/>, the
/// generator's own.
/// </summary>
abstract class IncludedGrammarSources
{
	/// <summary>The text the generator would compile the include with here.</summary>
	public IncludedGrammarChoice Select(GramInclude include)
	{
		string? resolved     = null;
		string? resolvedPath = null;

		var (carried, carriedPath) = Carried(include);

		var origin = GramIncludes.Choose(
			Placed(include.Type),
			carried,
			() =>
			{
				// The class's own reading: its inline grammar, or the one file of this project its source names.
				if (include.Source is { } written && !GramIncludes.IsPath(written))
					resolved = written;
				else
					(resolved, resolvedPath) = File(GramIncludes.Wanted(include));

				return resolved is not null;
			});

		return origin switch
		{
			IncludedGrammarOrigin.Carried  => new IncludedGrammarChoice(origin, carried, carriedPath),
			IncludedGrammarOrigin.Resolved => new IncludedGrammarChoice(origin, resolved, resolvedPath),
			_                              => new IncludedGrammarChoice(origin, null, null),
		};
	}

	/// <summary>Whether the class is in this compilation's source, not a referenced assembly's.</summary>
	protected abstract bool Placed(INamedTypeSymbol type);

	/// <summary>What the class carries to this compilation, and the file that text is where it is one.</summary>
	protected abstract (string? Text, string? Path) Carried(GramInclude include);

	/// <summary>The text of the one file of this project a source names, and its path; nothing where none or several do.</summary>
	protected abstract (string? Text, string? Path) File(string wanted);
}

/// <summary>
/// One compilation and its project's files, without the solution around them: a class of another
/// assembly carries what its <c>[GramSource]</c> says, as the generator wrote it there or a person did.
/// </summary>
sealed class CompilationIncludedGrammars(Compilation compilation, Func<string, string?>? file) : IncludedGrammarSources
{
	protected override bool Placed(INamedTypeSymbol type)
	{
		return type.DeclaringSyntaxReferences.Any(reference => compilation.ContainsSyntaxTree(reference.SyntaxTree));
	}

	protected override (string? Text, string? Path) Carried(GramInclude include)
	{
		return (include.Portable, null);
	}

	protected override (string? Text, string? Path) File(string wanted)
	{
		return (file?.Invoke(wanted), null);
	}
}

/// <summary>
/// A project of a solution, with the projects it references. A class another project of the solution
/// declares is, to the build, a class of a referenced assembly, and carries what that project's
/// generator writes into <c>[GramSource]</c> where it exports the grammar: the class's own grammar
/// with what it includes spliced on (<see cref="GrammarSplice"/>), each of those chosen as that
/// project's generator chooses it — computed here, since no generator runs to write it.
/// </summary>
/// <remarks>
/// A generator writes the attribute only for a grammar that compiles; a library whose grammar does not
/// fails its own build, and its consumers' with it, so that is not asked here. The grammar files are
/// read once, at <see cref="ReadAsync"/>; every file and project an answer depended on is in
/// <see cref="Documents"/> and <see cref="Projects"/>, for a caller that follows them.
/// </remarks>
sealed class SolutionIncludedGrammars : IncludedGrammarSources
{
	readonly Solution _solution;
	readonly ProjectId _project;
	readonly IReadOnlyDictionary<ProjectId, IReadOnlyList<(TextDocument Document, string Text)>> _grammars;
	readonly Shared _shared;

	sealed class Shared
	{
		public HashSet<DocumentId> Documents { get; } = [];

		public HashSet<ProjectId> Projects { get; } = [];

		public Dictionary<(ProjectId, INamedTypeSymbol), (string? Text, string? Path)> Carried { get; } = new(new CarriedKey());
	}

	sealed class CarriedKey : IEqualityComparer<(ProjectId, INamedTypeSymbol)>
	{
		public bool Equals((ProjectId, INamedTypeSymbol) x, (ProjectId, INamedTypeSymbol) y)
		{
			return x.Item1 == y.Item1 && SymbolEqualityComparer.Default.Equals(x.Item2, y.Item2);
		}

		public int GetHashCode((ProjectId, INamedTypeSymbol) key)
		{
			// No HashCode here: the Visual Studio extension compiles this for .NET Framework.
			return unchecked(key.Item1.GetHashCode() * 31 + SymbolEqualityComparer.Default.GetHashCode(key.Item2));
		}
	}

	SolutionIncludedGrammars(
		Solution solution,
		ProjectId project,
		IReadOnlyDictionary<ProjectId, IReadOnlyList<(TextDocument Document, string Text)>> grammars,
		Shared shared)
	{
		_solution = solution;
		_project  = project;
		_grammars = grammars;
		_shared   = shared;
	}

	/// <summary>The grammar files every answer read, the ones a choice fell on and the ones it passed over.</summary>
	public IReadOnlyCollection<DocumentId> Documents => _shared.Documents;

	/// <summary>The projects whose classes and files an answer depended on.</summary>
	public IReadOnlyCollection<ProjectId> Projects => _shared.Projects;

	/// <summary>The project's view, its grammar files and those of every project it references read now.</summary>
	public static async Task<SolutionIncludedGrammars> ReadAsync(Project project, CancellationToken cancellationToken)
	{
		var solution = project.Solution;
		var grammars = new Dictionary<ProjectId, IReadOnlyList<(TextDocument Document, string Text)>>();

		foreach (var id in solution.GetProjectDependencyGraph().GetProjectsThatThisProjectTransitivelyDependsOn(project.Id).Append(project.Id))
		{
			var files = new List<(TextDocument Document, string Text)>();

			foreach (var document in solution.GetProject(id)?.AdditionalDocuments ?? [])
				if (document.FilePath is { } path && path.EndsWith(GramIncludes.GramFileExtension, StringComparison.OrdinalIgnoreCase))
					files.Add((document, (await document.GetTextAsync(cancellationToken).ConfigureAwait(false)).ToString()));

			grammars[id] = files;
		}

		return new SolutionIncludedGrammars(solution, project.Id, grammars, new Shared());
	}

	/// <summary>The same files, asked as another project of them asks.</summary>
	SolutionIncludedGrammars For(ProjectId project)
	{
		return new SolutionIncludedGrammars(_solution, project, _grammars, _shared);
	}

	/// <summary>The project of the solution that declares a class; null where it is only in metadata.</summary>
	ProjectId? Declaring(INamedTypeSymbol type)
	{
		foreach (var declaration in type.DeclaringSyntaxReferences)
			if (_solution.GetDocumentId(declaration.SyntaxTree) is { } declared)
				return declared.ProjectId;

		return null;
	}

	protected override bool Placed(INamedTypeSymbol type)
	{
		return Declaring(type) == _project;
	}

	protected override (string? Text, string? Path) Carried(GramInclude include)
	{
		var declared = Declaring(include.Type);

		if (declared is not null)
			_shared.Projects.Add(declared);

		// Written on the class: a [GramSource] somebody wrote, or the one a generator has written there.
		if (include.Portable is { Length: > 0 } written)
			return (written, declared is null ? null : Equal(declared, written));

		if (declared is null || declared == _project || !GramHostOptions.Read(include.Attribute, include.Type, first: null).ExportGrammar)
			return (include.Portable, null);

		if (_shared.Carried.TryGetValue((declared, include.Type), out var known))
			return known;

		// What the declaring project's generator writes: the class's grammar as that project resolves it,
		// with what the class includes spliced on, each chosen as that project chooses it.
		var there = For(declared);
		var (own, path) = include.Source is { } inline && !GramIncludes.IsPath(inline)
			? (inline, null)
			: there.File(GramIncludes.Wanted(include));

		var result = (include.Portable, (string?)null);

		if (own is not null)
		{
			var parts = new List<GrammarSplice.Part>();

			foreach (var inner in GramIncludes.Walk(include.Type, Watch))
				if (there.Select(inner).Text is { } text)
					parts.Add(new GrammarSplice.Part(text, inner.Name, null));

			result = parts.Count == 0
				? (own, path)
				: (GrammarSplice.Join(new GrammarSplice.Part(own, null, null), parts).Text, null);
		}

		_shared.Carried[(declared, include.Type)] = result;

		return result;
	}

	/// <summary>A class an answer walked past: its declaration is followed.</summary>
	void Watch(INamedTypeSymbol type)
	{
		foreach (var declaration in type.DeclaringSyntaxReferences)
			if (_solution.GetDocumentId(declaration.SyntaxTree) is { } declared)
			{
				_shared.Documents.Add(declared);
				_shared.Projects.Add(declared.ProjectId);
			}
	}

	protected override (string? Text, string? Path) File(string wanted)
	{
		if (!_grammars.TryGetValue(_project, out var files))
			return (null, null);

		var found = files.Where(file => GramIncludes.Matches(file.Document.FilePath!, wanted)).Take(2).ToArray();

		// A file passed over is followed too: its going makes the name one file's.
		foreach (var file in found)
			_shared.Documents.Add(file.Document.Id);

		return found.Length == 1 ? (found[0].Text, found[0].Document.FilePath) : (null, null);
	}

	/// <summary>The declaring project's file whose text a carried grammar is, where one is: it opens as that file.</summary>
	string? Equal(ProjectId project, string text)
	{
		return _grammars.TryGetValue(project, out var files)
			? files.FirstOrDefault(file => file.Text == text).Document?.FilePath
			: null;
	}
}
