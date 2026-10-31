using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

	/// <summary>How many files and folders the walk looks at, at most, before it gives the folders up.</summary>
	public const int MaxEntries = 200_000;

	/// <summary>How long the walk takes, at most, before it gives the folders up: the first analysis waits for it.</summary>
	public static readonly TimeSpan MaxWalk = TimeSpan.FromSeconds(10);

	// The file system's: Hosts.cs and hosts.cs are two files on Linux, and one on Windows and macOS.
	static readonly StringComparison PathComparison = OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
		? StringComparison.OrdinalIgnoreCase
		: StringComparison.Ordinal;

	/// <summary>How the file system compares paths: every map of paths to what they are uses it.</summary>
	internal static readonly StringComparer PathComparer = StringComparer.FromComparison(PathComparison);

	// A link is not followed (the walk skips it by its attributes): a link to a parent folder is a walk
	// without an end.
	static readonly EnumerationOptions Enumeration = new()
	{
		AttributesToSkip   = 0,
		IgnoreInaccessible = true,
	};

	static readonly HashSet<string> AttributeNames = AttributeNamesOf(
		GramIncludes.GramAttribute, GramIncludes.GramIncludeAttribute, GramIncludes.GramSourceAttribute);

	static readonly CSharpParseOptions ParseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);

	readonly object                         _gate = new();
	readonly IReadOnlyList<string>          _folders;
	readonly StringComparison               _comparison;
	readonly Dictionary<string, DocumentId> _sources;
	readonly Dictionary<string, DocumentId> _grammars;

	// The grammars open in the editor: their text there, not their file's, is what they are, until they close.
	readonly Dictionary<string, SourceText> _open;

	FolderWorkspace(AdhocWorkspace workspace, ProjectId project, IReadOnlyList<string> folders, StringComparer paths)
	{
		Workspace   = workspace;
		Project     = project;
		_folders    = folders;
		Paths       = paths;
		_comparison = paths == StringComparer.Ordinal ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
		_sources    = new(paths);
		_grammars   = new(paths);
		_open       = new(paths);
	}

	/// <summary>How this workspace compares paths: the file system's, or a test's.</summary>
	public StringComparer Paths { get; }

	/// <summary>
	/// A change this workspace made to its solution, told synchronously as it is made — not later from
	/// Roslyn's own event queue — so that what it sets going has begun by the time the change returns.
	/// </summary>
	public event Action<WorkspaceChangeEventArgs>? Changed;

	public AdhocWorkspace Workspace { get; }

	public ProjectId Project { get; }

	/// <summary>Whether grammars are looked for hosts in here; false for no folder, or one past the cap.</summary>
	public bool Hosts => _folders.Count > 0;

	/// <summary>
	/// Reads the folders in the background, within a time: past it — an enumeration a network share
	/// stalls, which no bound inside the walk can see — the folders are given up, said so, and read as
	/// they stand; what the walk finds later is dropped.
	/// </summary>
	public static FolderWorkspace Load(IReadOnlyList<string> folders, Action<string> log, TimeSpan? limit = null)
	{
		return Load(cancellationToken => Create(folders, log, cancellationToken), log, limit ?? MaxWalk + TimeSpan.FromSeconds(20));
	}

	/// <summary>The same with what reads the folders given: a test's hook.</summary>
	internal static FolderWorkspace Load(Func<CancellationToken, FolderWorkspace> create, Action<string> log, TimeSpan limit)
	{
		var cancellation = new CancellationTokenSource();
		var reading = Task.Run(() => create(cancellation.Token), CancellationToken.None);

		if (reading.Wait(limit))
			return reading.Result;

		// The reading stops at its next look at the token; a workspace it finishes anyway is let go.
		cancellation.Cancel();
		_ = reading.ContinueWith(
			static task =>
			{
				if (task.IsCompletedSuccessfully)
					task.Result.Workspace.Dispose();
				else
					_ = task.Exception;
			},
			TaskScheduler.Default);
		log($"DotGram: the workspace took longer than {limit.TotalSeconds:0} s to read: grammars are read as they stand, without their hosts.");

		return Empty();
	}

	/// <summary>A project with nothing of the user's in it, for Roslyn's answers about the framework's own types.</summary>
	public static FolderWorkspace Empty()
	{
		return Create([], static _ => { }, CancellationToken.None);
	}

	/// <summary>
	/// Reads the folders; logs and reads none where they hold more C# files than the cap, or more
	/// entries than <paramref name="maxEntries"/>, or take longer than <paramref name="maxWalk"/> to walk.
	/// </summary>
	/// <param name="read">What reads a file; null where it cannot be read. A test's hook.</param>
	/// <param name="paths">How paths compare; the file system's where null. A test's hook.</param>
	public static FolderWorkspace Create(
		IReadOnlyList<string> folders,
		Action<string> log,
		CancellationToken cancellationToken,
		int maxEntries = MaxEntries,
		TimeSpan? maxWalk = null,
		Func<string, string?>? read = null,
		StringComparer? paths = null)
	{
		read ??= Read;

		var sources  = new List<string>();
		var grammars = new List<string>();
		var walk     = new Walk(maxEntries, maxWalk ?? MaxWalk);

		foreach (var folder in folders)
			if (!walk.Folder(folder, sources, grammars, cancellationToken))
				break;

		var given = sources.Count > MaxSourceFiles
			? $"{sources.Count} C# files in the workspace, more than {MaxSourceFiles}"
			: walk.GivenUp;

		if (given is not null)
		{
			log($"DotGram: {given}: grammars are read as they stand, without their hosts.");

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
		var result   = new FolderWorkspace(workspace, project.Id, folders, paths ?? PathComparer);
		var markers  = GramCompiler.EmitMarkerAttributes();
		var solution = project.AddDocument(markers.HintName, SourceText.From(markers.Text)).Project.Solution;

		try
		{
			// A reading given up is looked at before every read and before the project is made: past
			// its deadline it reads and builds nothing more.
			foreach (var path in sources)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (read(path) is { } text && Declares(text))
					solution = result.AddSource(solution, path, text);
			}

			foreach (var path in grammars)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (read(path) is { } text)
					solution = result.AddGrammar(solution, path, text);
			}

			cancellationToken.ThrowIfCancellationRequested();

			if (!workspace.TryApplyChanges(solution))
				throw new InvalidOperationException("The workspace refused its own first project.");

			return result;
		}
		catch
		{
			workspace.Dispose();
			throw;
		}
	}

	/// <summary>Whether a file is in one of the folders, and so has its host looked for here.</summary>
	public bool Contains(string path)
	{
		foreach (var folder in _folders)
		{
			var root = Path.TrimEndingDirectorySeparator(folder) + Path.DirectorySeparatorChar;

			if (path.StartsWith(root, _comparison))
				return true;
		}

		return false;
	}

	/// <summary>Whether a file's changes concern the project: in a folder, and not in one the walk skips.</summary>
	public bool Watches(string path)
	{
		return Hosts && Contains(path) && !Skipped(path);
	}

	/// <summary>A watched file was created, changed or deleted: read now, and the project follows.</summary>
	public void FileChanged(string path, bool deleted)
	{
		if (Watches(path))
			FileRead(path, deleted ? null : Read(path));
	}

	/// <summary>A watched file's text as it was read, or null where it is gone: the project follows.</summary>
	/// <remarks>The file is read by the caller, so that a read that stalls holds up nothing else.</remarks>
	public void FileRead(string path, string? text)
	{
		if (!Watches(path))
			return;

		WorkspaceChangeEventArgs? change = null;

		lock (_gate)
		{
			var solution = Workspace.CurrentSolution;

			if (path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
			{
				var declares = text is not null && Declares(text);

				if (_sources.TryGetValue(path, out var id))
				{
					if (declares)
						change = Apply(solution.WithDocumentText(id, SourceText.From(text!)), WorkspaceChangeKind.DocumentChanged, id);
					else
					{
						_sources.Remove(path);
						change = Apply(solution.RemoveDocument(id), WorkspaceChangeKind.DocumentRemoved, id);
					}
				}
				else if (declares)
				{
					solution = AddSource(solution, path, text!);
					change   = Apply(solution, WorkspaceChangeKind.DocumentAdded, _sources[path]);
				}
			}
			else if (path.EndsWith(GramIncludes.GramFileExtension, StringComparison.OrdinalIgnoreCase))
			{
				// An open grammar is its editor's text, whatever its file now says.
				if (_open.ContainsKey(path))
					return;

				if (_grammars.TryGetValue(path, out var id))
				{
					if (text is not null)
						change = Apply(solution.WithAdditionalDocumentText(id, SourceText.From(text)), WorkspaceChangeKind.AdditionalDocumentChanged, id);
					else
					{
						_grammars.Remove(path);
						change = Apply(solution.RemoveAdditionalDocument(id), WorkspaceChangeKind.AdditionalDocumentRemoved, id);
					}
				}
				else if (text is not null)
				{
					solution = AddGrammar(solution, path, text);
					change   = Apply(solution, WorkspaceChangeKind.AdditionalDocumentAdded, _grammars[path]);
				}
			}
		}

		Tell(change);
	}

	/// <summary>The solution made the workspace's; the change to tell, or null where it was refused.</summary>
	WorkspaceChangeEventArgs? Apply(Solution solution, WorkspaceChangeKind kind, DocumentId document)
	{
		var before = Workspace.CurrentSolution;

		return Workspace.TryApplyChanges(solution)
			? new WorkspaceChangeEventArgs(kind, before, Workspace.CurrentSolution, Project, document)
			: null;
	}

	/// <summary>Tells a change, outside the lock: what it sets going may ask the workspace again.</summary>
	void Tell(WorkspaceChangeEventArgs? change)
	{
		if (change is not null)
			Changed?.Invoke(change);
	}

	/// <summary>A grammar was opened: its text, unsaved edits and all, is what another grammar that includes it is compiled with.</summary>
	public void GrammarOpened(string path, SourceText text)
	{
		if (!Hosts || !Contains(path) || Skipped(path))
			return;

		WorkspaceChangeEventArgs? change;

		lock (_gate)
		{
			_open[path] = text;

			if (_grammars.TryGetValue(path, out var id))
				change = Apply(Workspace.CurrentSolution.WithAdditionalDocumentText(id, text), WorkspaceChangeKind.AdditionalDocumentChanged, id);
			else
			{
				var solution = AddGrammar(Workspace.CurrentSolution, path, text.ToString());

				change = Apply(solution, WorkspaceChangeKind.AdditionalDocumentAdded, _grammars[path]);
			}
		}

		Tell(change);
	}

	/// <summary>An open grammar was edited.</summary>
	public void GrammarEdited(string path, SourceText text)
	{
		WorkspaceChangeEventArgs? change;

		lock (_gate)
		{
			if (!_open.ContainsKey(path) || !_grammars.TryGetValue(path, out var id))
				return;

			_open[path] = text;

			change = Apply(Workspace.CurrentSolution.WithAdditionalDocumentText(id, text), WorkspaceChangeKind.AdditionalDocumentChanged, id);
		}

		Tell(change);
	}

	/// <summary>An open grammar was closed: what it is from now on is its file, which the caller reads and hands in.</summary>
	public void GrammarClosed(string path)
	{
		lock (_gate)
			_open.Remove(path);
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

	/// <summary>The folders' files, looked at within bounds: links are not followed, and a folder is seen once.</summary>
	sealed class Walk(int maxEntries, TimeSpan maxTime)
	{
		readonly HashSet<string> _seen  = new(PathComparer);
		readonly Stopwatch       _clock = Stopwatch.StartNew();
		int                      _entries;

		/// <summary>Why the walk gave the folders up, or null.</summary>
		public string? GivenUp { get; private set; }

		/// <summary>Walks one folder; false where the walk gave up.</summary>
		public bool Folder(string folder, List<string> sources, List<string> grammars, CancellationToken cancellationToken)
		{
			var pending = new Stack<string>();

			pending.Push(Path.GetFullPath(folder));

			while (pending.Count > 0)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var directory = pending.Pop();

				if (!_seen.Add(directory))
					continue;

				try
				{
					// Every entry is counted, a link that is not followed too.
					foreach (var entry in new DirectoryInfo(directory).EnumerateFileSystemInfos("*", Enumeration))
					{
						if (!Counted())
							return false;

						if ((entry.Attributes & FileAttributes.ReparsePoint) != 0)
							continue;

						if (entry is DirectoryInfo)
						{
							if (!SkippedFolder(entry.Name))
								pending.Push(entry.FullName);

							continue;
						}

						var file = entry.FullName;

						if (file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
						{
							sources.Add(file);

							// One past the cap is enough to know the folder is past it.
							if (sources.Count > MaxSourceFiles)
								return false;
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

			return true;
		}

		bool Counted()
		{
			if (++_entries > maxEntries)
				GivenUp = $"more than {maxEntries} files and folders in the workspace";
			else if (_clock.Elapsed > maxTime)
				GivenUp = $"the workspace took longer than {maxTime.TotalSeconds:0} s to look through";

			return GivenUp is null;
		}
	}

	static bool SkippedFolder(string name)
	{
		return name.StartsWith('.') || name is "bin" or "obj" or "node_modules";
	}

	bool Skipped(string path)
	{
		foreach (var folder in _folders)
			if (path.StartsWith(folder, _comparison))
				return Path.GetRelativePath(folder, path)
					.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
					.SkipLast(1)
					.Any(SkippedFolder);

		return true;
	}

	internal static string? Read(string path)
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
