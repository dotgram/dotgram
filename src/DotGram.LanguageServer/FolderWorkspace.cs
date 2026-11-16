using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

using DotGram.Grammar;
using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.LanguageServer;

/// <summary>
/// The C# a <c>.gram</c> file's host is looked for in, without a build: the workspace folders read as
/// the projects they hold — one Roslyn project per <c>.csproj</c>, with every C# file below it and its
/// project references, and one per folder for the files no project file claims — each compiled against
/// the framework's reference assemblies and the generator's own attributes, with its <c>.gram</c>
/// files as additional files.
/// </summary>
/// <remarks>
/// <para>
/// What the shared core looks for a host in is a Roslyn <see cref="Solution"/>; Visual Studio hands in
/// its own, and this is the server's. A grammar's host is looked for in the project that holds the
/// grammar, as the build compiles it there: two projects that each have a <c>Parser.gram</c> and a host
/// for it are two hosts, and each project's included files are its own. A file belongs to the project
/// whose folder is the nearest above it, as an SDK project globs it. Every C# file of a project is
/// compiled — a <c>global using</c> in a file of its own and a base class without a grammar attribute
/// take part in what a host is — and the project file's <c>Using</c> items and <c>ImplicitUsings</c>
/// are read as global usings. Package references, <c>Directory.Build.props</c> and the project's other
/// properties are not read: what depends on them is the build's to say.
/// </para>
/// <para>
/// Output and tooling folders are skipped (<c>bin</c>, <c>obj</c>, <c>node_modules</c> and every folder
/// whose name starts with a dot), and folders with more than <see cref="MaxSourceFiles"/> C# files in
/// all are not read: their grammars are read as they stand. The folders can change while the server
/// runs (<see cref="Reload"/>); a project file created, changed or deleted reads them again.
/// </para>
/// </remarks>
sealed class FolderWorkspace
{
	public const int MaxSourceFiles = 2000;

	/// <summary>How many files and folders the walk looks at, at most, before it gives the folders up.</summary>
	public const int MaxEntries = 200_000;

	/// <summary>How long the walk takes, at most, before it gives the folders up: the first analysis waits for it.</summary>
	public static readonly TimeSpan MaxWalk = TimeSpan.FromSeconds(10);

	/// <summary>How long a reading of the folders is waited for, the walk and the reads of its files together.</summary>
	public static readonly TimeSpan MaxRead = MaxWalk + TimeSpan.FromSeconds(20);

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

	static readonly CSharpParseOptions ParseOptions = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.Preview);

	// What ImplicitUsings adds in a project of Microsoft.NET.Sdk.
	static readonly string[] ImplicitUsings =
	[
		"System",
		"System.Collections.Generic",
		"System.IO",
		"System.Linq",
		"System.Net.Http",
		"System.Threading",
		"System.Threading.Tasks",
	];

	const string ProjectExtension = ".csproj";

	readonly object                         _gate = new();
	readonly StringComparison               _comparison;
	readonly Dictionary<string, DocumentId> _sources;
	readonly Dictionary<string, DocumentId> _grammars;
	readonly Action<string>                 _log;
	readonly Func<string, string?>          _read;

	// The grammars open in the editor, in the folders or not: their text there, not their file's, is what
	// they are, until they close — in the folders they are in now, and in any added later.
	readonly Dictionary<string, SourceText> _open;

	IReadOnlyList<string> _folders = [];

	// The projects' folders, the nearest first: a file belongs to the first that holds it.
	IReadOnlyList<(string Root, ProjectId Project)> _owners = [];

	FolderWorkspace(AdhocWorkspace workspace, StringComparer paths, Action<string> log, Func<string, string?> read)
	{
		Workspace   = workspace;
		Paths       = paths;
		_comparison = paths == StringComparer.Ordinal ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
		_sources    = new(paths);
		_grammars   = new(paths);
		_open       = new(paths);
		_log        = log;
		_read       = read;
	}

	/// <summary>How this workspace compares paths: the file system's, or a test's.</summary>
	public StringComparer Paths { get; }

	/// <summary>
	/// A change this workspace made to its solution, told synchronously as it is made — not later from
	/// Roslyn's own event queue — so that what it sets going has begun by the time the change returns.
	/// </summary>
	public event Action<WorkspaceChangeEventArgs>? Changed;

	public AdhocWorkspace Workspace { get; }

	/// <summary>The folders read now; none for no folder, or folders past a cap.</summary>
	public IReadOnlyList<string> Folders => Volatile.Read(ref _folders);

	/// <summary>Whether grammars are looked for hosts in here; false for no folder, or folders past a cap.</summary>
	public bool Hosts => Folders.Count > 0;

	/// <summary>
	/// Reads the folders in the background, within a time: past it — an enumeration a network share
	/// stalls, which no bound inside the walk can see — the folders are given up, said so, and read as
	/// they stand; what the walk finds later is dropped.
	/// </summary>
	public static FolderWorkspace Load(IReadOnlyList<string> folders, Action<string> log, TimeSpan? limit = null)
	{
		return Load(cancellationToken => Create(folders, log, cancellationToken), log, limit ?? MaxRead);
	}

	/// <summary>The same with what reads the folders given: a test's hook.</summary>
	internal static FolderWorkspace Load(Func<CancellationToken, FolderWorkspace> create, Action<string> log, TimeSpan limit)
	{
		if (Within(create, log, limit, out var result))
			return result!;

		// The reading stops at its next look at the token; a workspace it finishes anyway is let go.
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
		var workspace = new AdhocWorkspace();
		var result    = new FolderWorkspace(workspace, paths ?? PathComparer, log, read ?? Read);

		try
		{
			var scan = result.Scan(folders, cancellationToken, maxEntries, maxWalk ?? MaxWalk);

			lock (result._gate)
			{
				var solution = result.Build(workspace.CurrentSolution, scan);

				cancellationToken.ThrowIfCancellationRequested();

				if (!workspace.TryApplyChanges(solution))
					throw new InvalidOperationException("The workspace refused its own first projects.");

				Volatile.Write(ref result._folders, scan.Folders);
			}

			return result;
		}
		catch
		{
			workspace.Dispose();
			throw;
		}
	}

	/// <summary>
	/// Reads the folders again — the client's folders changed, or a project file did — within the time a
	/// first reading has, and makes the projects what they say now: every grammar's host is looked for
	/// again. A reading past its time leaves no folder read.
	/// </summary>
	public void Reload(IReadOnlyList<string> folders, TimeSpan? limit = null)
	{
		if (!Within(cancellationToken => Scan(folders, cancellationToken, MaxEntries, MaxWalk), _log, limit ?? MaxRead, out var scan))
			scan = new Scanned([], [], [], []);

		WorkspaceChangeEventArgs? change = null;

		lock (_gate)
		{
			var before   = Workspace.CurrentSolution;
			var solution = Build(before, scan!);

			if (Workspace.TryApplyChanges(solution))
			{
				Volatile.Write(ref _folders, scan!.Folders);
				change = new WorkspaceChangeEventArgs(WorkspaceChangeKind.SolutionChanged, before, Workspace.CurrentSolution);
			}
		}

		Tell(change);
	}

	/// <summary>Runs a reading of the folders within a time; past it, says so and lets the reading go.</summary>
	static bool Within<T>(Func<CancellationToken, T> reading, Action<string> log, TimeSpan limit, out T? result)
		where T : class
	{
		var cancellation = new CancellationTokenSource();
		var task         = Task.Run(() => reading(cancellation.Token), CancellationToken.None);

		if (task.Wait(limit))
		{
			result = task.Result;
			return true;
		}

		cancellation.Cancel();
		_ = task.ContinueWith(
			static task =>
			{
				if (task.IsCompletedSuccessfully && task.Result is FolderWorkspace late)
					late.Workspace.Dispose();
				else
					_ = task.Exception;
			},
			TaskScheduler.Default);
		log($"DotGram: the workspace took longer than {limit.TotalSeconds:0} s to read: grammars are read as they stand, without their hosts.");

		result = null;
		return false;
	}

	/// <summary>What a reading of the folders found: the folders, and every file it read with its text.</summary>
	sealed record Scanned(
		IReadOnlyList<string> Folders,
		IReadOnlyList<(string Path, string Text)> Projects,
		IReadOnlyList<(string Path, string Text)> Sources,
		IReadOnlyList<(string Path, string Text)> Grammars);

	/// <summary>Walks the folders and reads what they hold; nothing where they are past a cap.</summary>
	Scanned Scan(IReadOnlyList<string> folders, CancellationToken cancellationToken, int maxEntries, TimeSpan maxWalk)
	{
		var full     = folders.Select(static folder => Path.TrimEndingDirectorySeparator(Path.GetFullPath(folder))).Distinct(Paths).ToArray();
		var projects = new List<string>();
		var sources  = new List<string>();
		var grammars = new List<string>();
		var walk     = new Walk(maxEntries, maxWalk);

		foreach (var folder in full)
			if (!walk.Folder(folder, projects, sources, grammars, cancellationToken))
				break;

		var given = sources.Count > MaxSourceFiles
			? $"{sources.Count} C# files in the workspace, more than {MaxSourceFiles}"
			: walk.GivenUp;

		if (given is not null)
		{
			_log($"DotGram: {given}: grammars are read as they stand, without their hosts.");

			return new Scanned([], [], [], []);
		}

		// A reading given up is looked at before every read: past its deadline it reads nothing more.
		List<(string Path, string Text)> ReadAll(List<string> paths)
		{
			var result = new List<(string Path, string Text)>(paths.Count);

			foreach (var path in paths)
			{
				cancellationToken.ThrowIfCancellationRequested();

				if (_read(path) is { } text)
					result.Add((path, text));
			}

			return result;
		}

		return new Scanned(full, ReadAll(projects), ReadAll(sources), ReadAll(grammars));
	}

	/// <summary>
	/// The solution of what a reading found, in place of every project <paramref name="solution"/> has;
	/// the open grammars are their editors' text. Under the lock.
	/// </summary>
	Solution Build(Solution solution, Scanned scan)
	{
		foreach (var id in solution.ProjectIds.ToArray())
			solution = solution.RemoveProject(id);

		_sources.Clear();
		_grammars.Clear();

		var owners  = new List<(string Root, ProjectId Project)>();
		var byFile  = new Dictionary<string, ProjectId>(Paths);
		var files   = new List<(ProjectId Project, ProjectFile File)>();
		var names   = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		var markers = GramCompiler.EmitMarkerAttributes();

		ProjectId Add(string root, string name, ProjectFile? file)
		{
			var assembly = name;

			for (var index = 2; !names.Add(assembly); index++)
				assembly = $"{name}{index}";

			var id = ProjectId.CreateNewId(name);

			solution = solution
				.AddProject(ProjectInfo.Create(
					id,
					VersionStamp.Create(),
					name,
					assembly,
					LanguageNames.CSharp,
					compilationOptions: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable),
					parseOptions: ParseOptions,
					metadataReferences: Basic.Reference.Assemblies.Net100.References.All))
				.AddDocument(DocumentId.CreateNewId(id), markers.HintName, SourceText.From(markers.Text));

			if (file is { Usings.Count: > 0 })
				solution = solution.AddDocument(
					DocumentId.CreateNewId(id),
					name + ".GlobalUsings.g.cs",
					SourceText.From(string.Concat(file.Usings.Select(static item => item + "\n"))));

			owners.Add((root, id));

			return id;
		}

		foreach (var (path, text) in scan.Projects)
		{
			var file = ProjectFile.Parse(path, text);
			var id   = Add(Path.GetDirectoryName(path)!, Path.GetFileNameWithoutExtension(path), file);

			byFile[path] = id;
			files.Add((id, file));
		}

		// A folder's files that no project file claims are a project of their own; and with no folder at
		// all there is still one project, for Roslyn's answers about the framework's types.
		foreach (var folder in scan.Folders)
			if (!owners.Any(owner => Paths.Equals(owner.Root, folder)))
				Add(folder, Path.GetFileName(folder) is { Length: > 0 } name ? name : "DotGram.Workspace", null);

		if (owners.Count == 0)
			Add("", "DotGram.Workspace", null);

		foreach (var (id, file) in files)
			foreach (var reference in file.References)
				if (byFile.TryGetValue(reference, out var target) &&
					target != id &&
					!solution.GetProjectDependencyGraph().GetProjectsThatThisProjectTransitivelyDependsOn(target).Contains(id))
					solution = solution.AddProjectReference(id, new ProjectReference(target));

		// The nearest folder first; of a project file and a folder of the same name, the project file.
		_owners = [.. owners.OrderByDescending(static owner => owner.Root.Length)];

		foreach (var (path, text) in scan.Sources)
			if (Owner(path) is { } owner)
				solution = AddSource(solution, owner, path, SourceText.From(text));

		foreach (var (path, text) in scan.Grammars)
			if (Owner(path) is { } owner)
				solution = AddGrammar(solution, owner, path, _open.TryGetValue(path, out var open) ? open : SourceText.From(text));

		// An open grammar not on disk yet is in the folders as its editor has it.
		foreach (var (path, text) in _open)
			if (!_grammars.ContainsKey(path) && Contained(scan.Folders, path) && !Skipped(scan.Folders, path) && Owner(path) is { } owner)
				solution = AddGrammar(solution, owner, path, text);

		return solution;
	}

	/// <summary>The project a file belongs to: the one whose folder is the nearest above it; null for none.</summary>
	ProjectId? Owner(string path)
	{
		foreach (var (root, project) in _owners)
			if (root.Length == 0 || path.StartsWith(Path.TrimEndingDirectorySeparator(root) + Path.DirectorySeparatorChar, _comparison))
				return project;

		return null;
	}

	/// <summary>Whether a file is in one of the folders, and so has its host looked for here.</summary>
	public bool Contains(string path)
	{
		return Contained(Folders, path);
	}

	bool Contained(IReadOnlyList<string> folders, string path)
	{
		foreach (var folder in folders)
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
		var folders = Folders;

		return folders.Count > 0 && Contained(folders, path) && !Skipped(folders, path);
	}

	/// <summary>A watched file was created, changed or deleted: read now, and the project follows.</summary>
	public void FileChanged(string path, bool deleted)
	{
		if (Watches(path))
			FileRead(path, deleted ? null : _read(path));
	}

	/// <summary>A watched file's text as it was read, or null where it is gone: the project follows.</summary>
	/// <remarks>The file is read by the caller, so that a read that stalls holds up nothing else.</remarks>
	public void FileRead(string path, string? text)
	{
		if (!Watches(path))
			return;

		// A project file says which project every file below it is in, and what the project references:
		// the folders are read again.
		if (path.EndsWith(ProjectExtension, StringComparison.OrdinalIgnoreCase))
		{
			Reload(Folders);
			return;
		}

		WorkspaceChangeEventArgs? change = null;

		lock (_gate)
		{
			var solution = Workspace.CurrentSolution;

			if (path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
			{
				if (_sources.TryGetValue(path, out var id))
				{
					if (text is not null)
						change = Apply(solution.WithDocumentText(id, SourceText.From(text)), WorkspaceChangeKind.DocumentChanged, id);
					else
					{
						_sources.Remove(path);
						change = Apply(solution.RemoveDocument(id), WorkspaceChangeKind.DocumentRemoved, id);
					}
				}
				else if (text is not null && Owner(path) is { } owner)
				{
					solution = AddSource(solution, owner, path, SourceText.From(text));
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
				else if (text is not null && Owner(path) is { } owner)
				{
					solution = AddGrammar(solution, owner, path, SourceText.From(text));
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
			? new WorkspaceChangeEventArgs(kind, before, Workspace.CurrentSolution, document.ProjectId, document)
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
		WorkspaceChangeEventArgs? change = null;

		lock (_gate)
		{
			// Kept wherever the grammar is: a folder added later finds it open.
			_open[path] = text;

			if (!Watches(path))
				return;

			if (_grammars.TryGetValue(path, out var id))
				change = Apply(Workspace.CurrentSolution.WithAdditionalDocumentText(id, text), WorkspaceChangeKind.AdditionalDocumentChanged, id);
			else if (Owner(path) is { } owner)
			{
				var solution = AddGrammar(Workspace.CurrentSolution, owner, path, text);

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
			if (!_open.ContainsKey(path))
				return;

			_open[path] = text;

			if (!_grammars.TryGetValue(path, out var id))
				return;

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

	Solution AddSource(Solution solution, ProjectId project, string path, SourceText text)
	{
		var id = DocumentId.CreateNewId(project, path);

		_sources[path] = id;

		return solution.AddDocument(DocumentInfo.Create(
			id,
			Path.GetFileName(path),
			loader: TextLoader.From(TextAndVersion.Create(text, VersionStamp.Create(), path)),
			filePath: path));
	}

	Solution AddGrammar(Solution solution, ProjectId project, string path, SourceText text)
	{
		var id = DocumentId.CreateNewId(project, path);

		_grammars[path] = id;

		return solution.AddAdditionalDocument(DocumentInfo.Create(
			id,
			Path.GetFileName(path),
			loader: TextLoader.From(TextAndVersion.Create(text, VersionStamp.Create(), path)),
			filePath: path));
	}

	/// <summary>What a project file says that this workspace reads: the projects it references and its global usings.</summary>
	sealed record ProjectFile(IReadOnlyList<string> References, IReadOnlyList<string> Usings)
	{
		public static ProjectFile Parse(string path, string text)
		{
			XDocument xml;

			try
			{
				xml = XDocument.Parse(text);
			}
			catch (XmlException)
			{
				return new ProjectFile([], []);
			}

			var folder     = Path.GetDirectoryName(path)!;
			var references = new List<string>();
			var usings     = new List<string>();

			foreach (var element in xml.Descendants())
			{
				switch (element.Name.LocalName)
				{
					// A reference to a project's assembly; not one to an analyzer, or one that takes only its build order.
					case "ProjectReference" when
						!string.Equals(Metadata(element, "ReferenceOutputAssembly"), "false", StringComparison.OrdinalIgnoreCase) &&
						!string.Equals(Metadata(element, "OutputItemType"), "Analyzer", StringComparison.OrdinalIgnoreCase):
						foreach (var include in Items(element))
							if (!include.Contains("$(", StringComparison.Ordinal))
								references.Add(Path.GetFullPath(Path.Combine(folder, include.Replace('\\', Path.DirectorySeparatorChar))));
						break;

					case "Using":
						var alias  = Metadata(element, "Alias");
						var isStatic = string.Equals(Metadata(element, "Static"), "true", StringComparison.OrdinalIgnoreCase);

						foreach (var include in Items(element))
							usings.Add(alias is { Length: > 0 }
								? $"global using {alias} = global::{include};"
								: isStatic ? $"global using static global::{include};" : $"global using global::{include};");
						break;

					case "ImplicitUsings" when element.Value.Trim() is var value &&
						(value.Equals("enable", StringComparison.OrdinalIgnoreCase) || value.Equals("true", StringComparison.OrdinalIgnoreCase)):
						usings.AddRange(FolderWorkspace.ImplicitUsings.Select(static item => $"global using global::{item};"));
						break;
				}
			}

			return new ProjectFile(references, [.. usings.Distinct(StringComparer.Ordinal)]);
		}

		static IEnumerable<string> Items(XElement element)
		{
			return ((string?)element.Attribute("Include") ?? "")
				.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		}

		/// <summary>An item's metadata, written as an attribute or as an element inside it.</summary>
		static string? Metadata(XElement element, string name)
		{
			return (string?)element.Attribute(name) ??
				element.Elements().FirstOrDefault(child => child.Name.LocalName == name)?.Value.Trim();
		}
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
		public bool Folder(string folder, List<string> projects, List<string> sources, List<string> grammars, CancellationToken cancellationToken)
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
						else if (file.EndsWith(ProjectExtension, StringComparison.OrdinalIgnoreCase))
							projects.Add(file);
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

	bool Skipped(IReadOnlyList<string> folders, string path)
	{
		foreach (var folder in folders)
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
