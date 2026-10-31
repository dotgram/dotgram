using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using DotGram.Grammar;
using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.LanguageServer;

/// <summary>
/// The C# a <c>.gram</c> file's host is looked for in, without a build: an ad-hoc project of the
/// workspace folders' C# files that declare a grammar, compiled against the framework's reference
/// assemblies and the generator's own attributes, with every <c>.gram</c> file as an additional file.
/// </summary>
/// <remarks>
/// <para>
/// What the shared core looks for a host in is a Roslyn <see cref="Solution"/>; Visual Studio hands in
/// its own, and this is the server's. A file joins the project when its syntax has an attribute named
/// as one of the generator's grammar attributes — <c>[Gram]</c>, <c>[GramInclude]</c>,
/// <c>[GramSource]</c> — so that a host's <c>Lexical</c> and what it includes are read as the build
/// reads them. Nothing else of the folder is compiled: a host's other members, the types they name
/// and the project's references are not there, and what depends on them is the build's to say.
/// </para>
/// <para>
/// Output and tooling folders are skipped (<c>bin</c>, <c>obj</c>, <c>node_modules</c> and every folder
/// whose name starts with a dot), and a folder with more than <see cref="MaxSourceFiles"/> C# files is
/// not read at all: its grammars are read as they stand.
/// </para>
/// </remarks>
sealed class FolderWorkspace
{
	public const int MaxSourceFiles = 2000;

	static readonly StringComparison PathComparison = OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
		? StringComparison.OrdinalIgnoreCase
		: StringComparison.Ordinal;

	static readonly HashSet<string> AttributeNames = AttributeNamesOf(
		GramIncludes.GramAttribute, GramIncludes.GramIncludeAttribute, GramIncludes.GramSourceAttribute);

	static readonly CSharpParseOptions ParseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);

	readonly object                   _gate = new();
	readonly IReadOnlyList<string>    _folders;
	readonly Dictionary<string, DocumentId> _sources   = new(StringComparer.OrdinalIgnoreCase);
	readonly Dictionary<string, DocumentId> _grammars  = new(StringComparer.OrdinalIgnoreCase);

	FolderWorkspace(AdhocWorkspace workspace, ProjectId project, IReadOnlyList<string> folders)
	{
		Workspace = workspace;
		Project   = project;
		_folders  = folders;
	}

	public AdhocWorkspace Workspace { get; }

	public ProjectId Project { get; }

	/// <summary>Whether grammars are looked for hosts in here; false for no folder, or one past the cap.</summary>
	public bool Hosts => _folders.Count > 0;

	/// <summary>A project with nothing of the user's in it, for Roslyn's answers about the framework's own types.</summary>
	public static FolderWorkspace Empty()
	{
		return Create([], static _ => { }, CancellationToken.None);
	}

	/// <summary>Reads the folders; logs and reads none where they hold more C# files than the cap.</summary>
	public static FolderWorkspace Create(IReadOnlyList<string> folders, Action<string> log, CancellationToken cancellationToken)
	{
		var sources  = new List<string>();
		var grammars = new List<string>();

		foreach (var folder in folders)
			Walk(folder, sources, grammars, cancellationToken);

		if (sources.Count > MaxSourceFiles)
		{
			log($"DotGram: {sources.Count} C# files in the workspace, more than {MaxSourceFiles}: grammars are read as they stand, without their hosts.");

			sources.Clear();
			grammars.Clear();
			folders = [];
		}

		var workspace = new AdhocWorkspace();
		var project   = workspace.AddProject(ProjectInfo.Create(
			ProjectId.CreateNewId(),
			VersionStamp.Create(),
			"DotGram.Workspace",
			"DotGram.Workspace",
			LanguageNames.CSharp,
			compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable),
			parseOptions: ParseOptions,
			metadataReferences: Basic.Reference.Assemblies.Net100.References.All));
		var result   = new FolderWorkspace(workspace, project.Id, folders);
		var markers  = GramCompiler.EmitMarkerAttributes();
		var solution = project.AddDocument(markers.HintName, SourceText.From(markers.Text)).Project.Solution;

		foreach (var path in sources)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (Read(path) is { } text && Declares(text))
				solution = result.AddSource(solution, path, text);
		}

		foreach (var path in grammars)
			if (Read(path) is { } text)
				solution = result.AddGrammar(solution, path, text);

		if (!workspace.TryApplyChanges(solution))
			throw new InvalidOperationException("The workspace refused its own first project.");

		return result;
	}

	/// <summary>Whether a file is in one of the folders, and so has its host looked for here.</summary>
	public bool Contains(string path)
	{
		foreach (var folder in _folders)
		{
			var root = Path.TrimEndingDirectorySeparator(folder) + Path.DirectorySeparatorChar;

			if (path.StartsWith(root, PathComparison))
				return true;
		}

		return false;
	}

	/// <summary>A watched file was created, changed or deleted: the project follows.</summary>
	public void FileChanged(string path, bool deleted)
	{
		if (!Hosts || !Contains(path) || Skipped(path))
			return;

		lock (_gate)
		{
			var solution = Workspace.CurrentSolution;
			var text     = deleted ? null : Read(path);

			if (path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
			{
				var declares = text is not null && Declares(text);

				if (_sources.TryGetValue(path, out var id))
				{
					solution = declares
						? solution.WithDocumentText(id, SourceText.From(text!))
						: solution.RemoveDocument(id);

					if (!declares)
						_sources.Remove(path);
				}
				else if (declares)
					solution = AddSource(solution, path, text!);
			}
			else if (path.EndsWith(GramIncludes.GramFileExtension, StringComparison.OrdinalIgnoreCase))
			{
				if (_grammars.TryGetValue(path, out var id))
				{
					solution = text is null
						? solution.RemoveAdditionalDocument(id)
						: solution.WithAdditionalDocumentText(id, SourceText.From(text));

					if (text is null)
						_grammars.Remove(path);
				}
				else if (text is not null)
					solution = AddGrammar(solution, path, text);
			}

			Workspace.TryApplyChanges(solution);
		}
	}

	/// <summary>An open grammar's text, which is what another grammar that includes it is compiled with.</summary>
	public void GrammarEdited(string path, SourceText text)
	{
		lock (_gate)
		{
			if (!_grammars.TryGetValue(path, out var id))
				return;

			Workspace.TryApplyChanges(Workspace.CurrentSolution.WithAdditionalDocumentText(id, text));
		}
	}

	/// <summary>Whether a C# file's syntax names a grammar attribute: a host, an inclusion, or a carried grammar.</summary>
	internal static bool Declares(string text)
	{
		// Every spelling of the three, alone or qualified, has this in it; a file without it is not parsed.
		if (!text.Contains("Gram", StringComparison.Ordinal))
			return false;

		var root = CSharpSyntaxTree.ParseText(text, ParseOptions).GetRoot();

		return root.DescendantNodes(static node =>
				node is CompilationUnitSyntax or BaseNamespaceDeclarationSyntax or TypeDeclarationSyntax or AttributeListSyntax)
			.OfType<AttributeSyntax>()
			.Any(static attribute => AttributeNames.Contains(SimpleName(attribute.Name)));
	}

	static string SimpleName(NameSyntax name)
	{
		return name switch
		{
			QualifiedNameSyntax qualified   => qualified.Right.Identifier.ValueText,
			AliasQualifiedNameSyntax global => global.Name.Identifier.ValueText,
			SimpleNameSyntax simple         => simple.Identifier.ValueText,
			_                               => "",
		};
	}

	/// <summary>How an attribute's metadata name may be written: <c>GramAttribute</c> and <c>Gram</c>.</summary>
	static HashSet<string> AttributeNamesOf(params string[] metadataNames)
	{
		var result = new HashSet<string>(StringComparer.Ordinal);

		foreach (var metadataName in metadataNames)
		{
			var name = metadataName.Substring(metadataName.LastIndexOf('.') + 1);

			result.Add(name);

			if (name.EndsWith("Attribute", StringComparison.Ordinal))
				result.Add(name.Substring(0, name.Length - "Attribute".Length));
		}

		return result;
	}

	Solution AddSource(Solution solution, string path, string text)
	{
		var id = DocumentId.CreateNewId(Project, path);

		_sources[path] = id;

		return solution.AddDocument(DocumentInfo.Create(
			id,
			Path.GetFileName(path),
			loader: TextLoader.From(TextAndVersion.Create(SourceText.From(text), VersionStamp.Create(), path)),
			filePath: path));
	}

	Solution AddGrammar(Solution solution, string path, string text)
	{
		var id = DocumentId.CreateNewId(Project, path);

		_grammars[path] = id;

		return solution.AddAdditionalDocument(DocumentInfo.Create(
			id,
			Path.GetFileName(path),
			loader: TextLoader.From(TextAndVersion.Create(SourceText.From(text), VersionStamp.Create(), path)),
			filePath: path));
	}

	static void Walk(string folder, List<string> sources, List<string> grammars, CancellationToken cancellationToken)
	{
		var pending = new Stack<string>();

		pending.Push(folder);

		while (pending.Count > 0)
		{
			cancellationToken.ThrowIfCancellationRequested();

			var directory = pending.Pop();

			try
			{
				foreach (var child in Directory.EnumerateDirectories(directory))
					if (!SkippedFolder(Path.GetFileName(child)))
						pending.Push(child);

				foreach (var file in Directory.EnumerateFiles(directory))
				{
					if (file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
					{
						sources.Add(file);

						// One past the cap is enough to know the folder is past it.
						if (sources.Count > MaxSourceFiles)
							return;
					}
					else if (file.EndsWith(GramIncludes.GramFileExtension, StringComparison.OrdinalIgnoreCase))
						grammars.Add(file);
				}
			}
			catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
			{
				// A folder that cannot be read holds nothing for this.
			}
		}
	}

	static bool SkippedFolder(string name)
	{
		return name.StartsWith('.') || name is "bin" or "obj" or "node_modules";
	}

	bool Skipped(string path)
	{
		foreach (var folder in _folders)
			if (path.StartsWith(folder, PathComparison))
				return Path.GetRelativePath(folder, path)
					.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
					.SkipLast(1)
					.Any(SkippedFolder);

		return true;
	}

	static string? Read(string path)
	{
		try
		{
			return File.ReadAllText(path);
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
		{
			return null;
		}
	}
}
