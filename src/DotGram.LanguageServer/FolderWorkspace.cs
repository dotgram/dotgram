using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
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
/// the projects they hold, each compiled against the framework's reference assemblies and the
/// generator's own attributes, with its grammars as additional files.
/// </summary>
/// <remarks>
/// <para>
/// What the shared core looks for a host in is a Roslyn <see cref="Solution"/>; Visual Studio hands in
/// its own, and this is the server's. Every <c>.csproj</c> is a project, and a grammar's host is looked
/// for in the project that holds the grammar, as the build compiles it there.
/// </para>
/// <para>
/// Where the client trusts the workspace and the machine has a .NET SDK, MSBuild evaluates the projects
/// (<see cref="ProjectEvaluation"/>): a project's C# files, its grammars, the projects it references
/// and its global usings are then what the build has, every glob, <c>Remove</c>, condition and import
/// applied — a file below a nested project is in both where both glob it, a project's own
/// <c>Directory.Build.props</c> and a restored package's build files count. A project MSBuild cannot
/// evaluate, every project of an untrusted workspace (evaluating one runs its own code) and the files
/// no project claims are read by their folders instead: a file belongs to the project whose folder is
/// the nearest above it, the project file's <c>Using</c> items and <c>ImplicitUsings</c> are its global
/// usings, and conditions, imports and package references are not read.
/// </para>
/// <para>
/// Output and tooling folders are skipped (<c>bin</c>, <c>obj</c>, <c>node_modules</c> and every folder
/// whose name starts with a dot), and folders with more than <see cref="MaxSourceFiles"/> C# files in
/// all are not read: their grammars are read as they stand. The folders can change while the server
/// runs (<see cref="Reload"/>); a project file changed, created or deleted has its project evaluated
/// again, and a <c>.props</c> or <c>.targets</c> file every project.
/// </para>
/// </remarks>
sealed class FolderWorkspace
{
	public const int MaxSourceFiles = 2000;

	/// <summary>How many files and folders the walk looks at, at most, before it gives the folders up.</summary>
	public const int MaxEntries = 200_000;

	/// <summary>How long the walk takes, at most, before it gives the folders up: the first analysis waits for it.</summary>
	public static readonly TimeSpan MaxWalk = TimeSpan.FromSeconds(10);

	/// <summary>How long a reading of the folders is waited for, the walk, the evaluation and the reads of its files together.</summary>
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

	/// <summary>Evaluates projects with MSBuild: the projects, the folder it runs in, the token. A test's hook, and null where nothing is evaluated.</summary>
	internal delegate IReadOnlyDictionary<string, ProjectEvaluation> Evaluator(IReadOnlyList<string> projects, string directory, CancellationToken cancellationToken);

	readonly object                                 _gate   = new();
	readonly object                                 _reload = new();
	readonly StringComparison                       _comparison;
	readonly Dictionary<string, List<DocumentId>>   _sources;
	readonly Dictionary<string, List<DocumentId>>   _grammars;
	readonly Action<string>                         _log;
	readonly Func<string, string?>                  _read;
	readonly Evaluator?                             _evaluate;

	// The grammars open in the editor, in the folders or not: their text there, not their file's, is what
	// they are, until they close — in the folders they are in now, and in any added later.
	readonly Dictionary<string, SourceText> _open;

	// What MSBuild said of each project, kept until its project file, or a .props or .targets file, changes.
	readonly Dictionary<string, ProjectEvaluation> _evaluations;

	// The files read while a reading of the folders runs, the latest text of each (null: deleted): the
	// reading began before them, and what it builds takes them over in place of what it read.
	Dictionary<string, string?>? _since;

	// The open grammars saved below an evaluated project that did not take them in, evaluated again for
	// them once: a save after that is not another evaluation.
	readonly HashSet<string> _evaluatedFor = new(PathComparer);

	IReadOnlyList<string> _folders = [];

	// The projects' folders, the nearest first, each with whether MSBuild said what it holds.
	IReadOnlyList<(string Root, ProjectId Project, bool Evaluated)> _owners = [];

	// The projects whose build runs the DotGram generator.
	IReadOnlySet<ProjectId> _generating = new HashSet<ProjectId>();

	FolderWorkspace(AdhocWorkspace workspace, StringComparer paths, Action<string> log, Func<string, string?> read, Evaluator? evaluate)
	{
		Workspace    = workspace;
		Paths        = paths;
		_comparison  = paths == StringComparer.Ordinal ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
		_sources     = new(paths);
		_grammars    = new(paths);
		_open        = new(paths);
		_evaluations = new(paths);
		_log         = log;
		_read        = read;
		_evaluate    = evaluate;
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

	/// <summary>Whether MSBuild evaluated any of the projects.</summary>
	public bool Evaluated
	{
		get
		{
			lock (_gate)
				return _owners.Any(static owner => owner.Evaluated);
		}
	}

	/// <summary>Whether grammars are looked for hosts in here; false for no folder, or folders past a cap.</summary>
	public bool Hosts => Folders.Count > 0;

	/// <summary>The MSBuild of the machine's .NET SDK, run in a process of its own.</summary>
	internal static Evaluator MSBuild { get; } = (projects, directory, cancellationToken) =>
		ProjectEvaluation.Evaluate(projects, directory, PathComparer, cancellationToken);

	/// <summary>
	/// Reads the folders in the background, within a time: past it — an enumeration a network share
	/// stalls, which no bound inside the walk can see — the folders are given up, said so, and read as
	/// they stand; what the walk finds later is dropped.
	/// </summary>
	/// <param name="evaluate">What evaluates the projects; null where none is, as in a workspace the client does not trust.</param>
	public static FolderWorkspace Load(IReadOnlyList<string> folders, Action<string> log, TimeSpan? limit = null, Evaluator? evaluate = null)
	{
		return Load(cancellationToken => Create(folders, log, cancellationToken, evaluate: evaluate), log, limit ?? MaxRead, evaluate);
	}

	/// <summary>The same with what reads the folders given: a test's hook.</summary>
	internal static FolderWorkspace Load(Func<CancellationToken, FolderWorkspace> create, Action<string> log, TimeSpan limit, Evaluator? evaluate = null)
	{
		if (Within(create, log, limit, out var result))
			return result!;

		// The reading stops at its next look at the token; a workspace it finishes anyway is let go.
		return Empty(log, evaluate);
	}

	/// <summary>A project with nothing of the user's in it, for Roslyn's answers about the framework's own types.</summary>
	/// <param name="log">Told what a later <see cref="Reload"/> of folders added to it has to say.</param>
	/// <param name="evaluate">What evaluates the projects of folders added later.</param>
	public static FolderWorkspace Empty(Action<string>? log = null, Evaluator? evaluate = null)
	{
		return Create([], log ?? (static _ => { }), CancellationToken.None, evaluate: evaluate);
	}

	/// <summary>
	/// Reads the folders; logs and reads none where they hold more C# files than the cap, or more
	/// entries than <paramref name="maxEntries"/>, or take longer than <paramref name="maxWalk"/> to walk.
	/// </summary>
	/// <param name="read">What reads a file; null where it cannot be read. A test's hook.</param>
	/// <param name="paths">How paths compare; the file system's where null. A test's hook.</param>
	/// <param name="evaluate">What evaluates the projects; null where none is.</param>
	public static FolderWorkspace Create(
		IReadOnlyList<string> folders,
		Action<string> log,
		CancellationToken cancellationToken,
		int maxEntries = MaxEntries,
		TimeSpan? maxWalk = null,
		Func<string, string?>? read = null,
		StringComparer? paths = null,
		Evaluator? evaluate = null)
	{
		var workspace = new AdhocWorkspace();
		var result    = new FolderWorkspace(workspace, paths ?? PathComparer, log, read ?? Read, evaluate);

		try
		{
			var scan = result.Scan(folders, null, cancellationToken, maxEntries, maxWalk ?? MaxWalk);

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
	/// again. A reading past its time leaves no folder read. A file read while the reading runs is what it
	/// was read as then, not what the reading found before it; one reading runs at a time.
	/// </summary>
	/// <param name="folders">
	/// The folders to read; null for the folders the workspace has when this reading's turn comes, so
	/// that one asked for before a change of the folders, and run after it, does not undo it.
	/// </param>
	/// <param name="stale">
	/// The projects to evaluate again, by their files; the others keep what MSBuild said of them. Null
	/// evaluates every project again.
	/// </param>
	public void Reload(IReadOnlyList<string>? folders, IReadOnlySet<string>? stale = null, TimeSpan? limit = null)
	{
		lock (_reload)
		{
			folders ??= Folders;

			lock (_gate)
				_since = new Dictionary<string, string?>(Paths);

			Scanned? scan;

			try
			{
				if (!Within(cancellationToken => Scan(folders, stale, cancellationToken, MaxEntries, MaxWalk), _log, limit ?? MaxRead, out scan))
					scan = Scanned.None;
			}
			catch
			{
				lock (_gate)
					_since = null;

				throw;
			}

			WorkspaceChangeEventArgs? change = null;

			lock (_gate)
			{
				var before   = Workspace.CurrentSolution;
				var solution = Build(before, scan!);

				_since = null;

				if (Workspace.TryApplyChanges(solution))
				{
					Volatile.Write(ref _folders, scan!.Folders);
					change = new WorkspaceChangeEventArgs(WorkspaceChangeKind.SolutionChanged, before, Workspace.CurrentSolution);
				}
			}

			Tell([change]);
		}
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

	/// <summary>
	/// What a reading of the folders found: the folders, the project files with their text, what MSBuild
	/// said of the projects it evaluated, the C# and grammar files the walk found, and the text of every
	/// file the projects hold.
	/// </summary>
	sealed record Scanned(
		IReadOnlyList<string> Folders,
		IReadOnlyList<(string Path, string Text)> Projects,
		IReadOnlyDictionary<string, ProjectEvaluation> Evaluations,
		IReadOnlyList<string> Sources,
		IReadOnlyList<string> Grammars,
		IReadOnlyDictionary<string, string> Texts)
	{
		public static Scanned None { get; } = new([], [], new Dictionary<string, ProjectEvaluation>(), [], [], new Dictionary<string, string>());
	}

	/// <summary>Walks the folders, evaluates the projects and reads what they hold; nothing where they are past a cap.</summary>
	Scanned Scan(IReadOnlyList<string> folders, IReadOnlySet<string>? stale, CancellationToken cancellationToken, int maxEntries, TimeSpan maxWalk)
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

			return Scanned.None;
		}

		var evaluations = Evaluate(full, projects, stale, cancellationToken);

		// A project may compile C# from elsewhere, by links: that counts toward the cap as the folders' own.
		var compiled = new HashSet<string>(sources, Paths);

		foreach (var evaluation in evaluations.Values)
			compiled.UnionWith(evaluation.Compile);

		if (compiled.Count > MaxSourceFiles)
		{
			_log($"DotGram: {compiled.Count} C# files in the workspace's projects, more than {MaxSourceFiles}: grammars are read as they stand, without their hosts.");

			return Scanned.None;
		}
		var texts       = new Dictionary<string, string>(Paths);
		var read        = new List<(string Path, string Text)>();

		// A reading given up is looked at before every read: past its deadline it reads nothing more.
		void ReadOne(string path)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (!texts.ContainsKey(path) && _read(path) is { } text)
				texts[path] = text;
		}

		foreach (var path in projects)
		{
			cancellationToken.ThrowIfCancellationRequested();

			if (_read(path) is { } text)
				read.Add((path, text));
		}

		foreach (var path in sources.Concat(grammars))
			ReadOne(path);

		// A project may compile a file outside the folders, linked from elsewhere.
		foreach (var evaluation in evaluations.Values)
			foreach (var path in evaluation.Compile.Concat(evaluation.Grammars))
				ReadOne(path);

		return new Scanned(full, read, evaluations, sources, grammars, texts);
	}

	/// <summary>
	/// What MSBuild says of the projects: of those <paramref name="stale"/> names, or that it has not been
	/// asked about, asked now, a folder's at a time in that folder; of the others what it said before.
	/// Nothing where nothing evaluates, or where MSBuild cannot be run, which is said once.
	/// </summary>
	IReadOnlyDictionary<string, ProjectEvaluation> Evaluate(
		IReadOnlyList<string> folders,
		IReadOnlyList<string> projects,
		IReadOnlySet<string>? stale,
		CancellationToken cancellationToken)
	{
		var result = new Dictionary<string, ProjectEvaluation>(Paths);

		if (_evaluate is null || projects.Count == 0)
			return result;

		Dictionary<string, ProjectEvaluation> known;

		lock (_gate)
			known = new(_evaluations, Paths);

		var asked = new List<string>();

		foreach (var project in projects)
		{
			if (stale is not null && !stale.Contains(project) && known.TryGetValue(project, out var kept))
				result[project] = kept;
			else
				asked.Add(project);
		}

		foreach (var folder in folders)
		{
			var mine = asked.Where(project => Within(folder, project)).ToArray();

			if (mine.Length == 0)
				continue;

			try
			{
				foreach (var (project, evaluation) in _evaluate(mine, folder, cancellationToken))
					result[project] = evaluation;

				var missed = mine.Where(project => !result.ContainsKey(project)).Select(Path.GetFileName).ToArray();

				if (missed.Length > 0)
					_log($"DotGram: MSBuild could not evaluate {string.Join(", ", missed)}, read by its folder instead: build the project once to see why.");
			}
			catch (Exception exception) when (exception is InvalidOperationException or System.ComponentModel.Win32Exception or IOException or JsonException)
			{
				_log($"DotGram: MSBuild could not evaluate the projects of {folder}, which are read by their folders instead: {exception.Message}");
			}
		}

		lock (_gate)
		{
			_evaluations.Clear();

			foreach (var (project, evaluation) in result)
				_evaluations[project] = evaluation;
		}

		return result;
	}

	bool Within(string folder, string path)
	{
		return path.StartsWith(Path.TrimEndingDirectorySeparator(folder) + Path.DirectorySeparatorChar, _comparison);
	}

	/// <summary>
	/// The solution of what a reading found, in place of every project <paramref name="solution"/> has;
	/// the open grammars are their editors' text, and a file read since the reading began is what it was
	/// read as. Under the lock.
	/// </summary>
	Solution Build(Solution solution, Scanned scan)
	{
		foreach (var id in solution.ProjectIds.ToArray())
			solution = solution.RemoveProject(id);

		_sources.Clear();
		_grammars.Clear();

		var texts    = new Dictionary<string, string>(scan.Texts, Paths);
		var sources  = new List<string>(scan.Sources);
		var grammars = new List<string>(scan.Grammars);

		// What was read while the reading ran is newer than what it read.
		if (_since is { } since)
			foreach (var (path, text) in since)
			{
				if (text is null)
				{
					texts.Remove(path);
					continue;
				}

				texts[path] = text;

				if (path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) && !sources.Contains(path, Paths))
					sources.Add(path);
				else if (path.EndsWith(GramIncludes.GramFileExtension, StringComparison.OrdinalIgnoreCase) && !grammars.Contains(path, Paths))
					grammars.Add(path);
			}

		var owners  = new List<(string Root, ProjectId Project, bool Evaluated)>();
		var byFile  = new Dictionary<string, ProjectId>(Paths);
		var links   = new List<(ProjectId Project, IReadOnlyList<string> References)>();
		var names   = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		var markers = GramCompiler.EmitMarkerAttributes();
		var claimed = new HashSet<string>(Paths);
		var generating = new HashSet<ProjectId>();

		ProjectId Add(string root, string name, IReadOnlyList<string> usings, bool evaluated, bool generates = false)
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

			if (usings.Count > 0)
				solution = solution.AddDocument(
					DocumentId.CreateNewId(id),
					name + ".GlobalUsings.g.cs",
					SourceText.From(string.Concat(usings.Select(static item => item + "\n"))));

			owners.Add((root, id, evaluated));

			if (generates)
				generating.Add(id);

			return id;
		}

		foreach (var (path, text) in scan.Projects)
		{
			var name = Path.GetFileNameWithoutExtension(path);
			var root = Path.GetDirectoryName(path)!;

			if (scan.Evaluations.TryGetValue(path, out var evaluation))
			{
				var id = Add(root, name, evaluation.Usings, evaluated: true, evaluation.Generates);

				byFile[path] = id;
				links.Add((id, evaluation.References));

				foreach (var file in evaluation.Compile)
					if (texts.TryGetValue(file, out var source))
					{
						solution = AddSource(solution, id, file, SourceText.From(source));
						claimed.Add(file);
					}

				foreach (var file in evaluation.Grammars)
					if (texts.TryGetValue(file, out var grammar))
					{
						solution = AddGrammar(solution, id, file, _open.TryGetValue(file, out var open) ? open : SourceText.From(grammar));
						claimed.Add(file);
					}
			}
			else
			{
				var file = ProjectFile.Parse(path, text);
				var id   = Add(root, name, file.Usings, evaluated: false, file.Generates);

				byFile[path] = id;
				links.Add((id, file.References));
			}
		}

		// A folder's files that no project claims are a project of their own — where a project file MSBuild
		// did not evaluate is at the folder's root, that project — and with no folder at all there is still
		// one project, for Roslyn's answers about the framework's types.
		foreach (var folder in scan.Folders)
			if (!owners.Any(owner => !owner.Evaluated && Paths.Equals(owner.Root, folder)))
				Add(folder, Path.GetFileName(folder) is { Length: > 0 } name ? name : "DotGram.Workspace", [], evaluated: false);

		if (owners.Count == 0)
			Add("", "DotGram.Workspace", [], evaluated: false);

		foreach (var (id, references) in links)
			foreach (var reference in references)
				if (byFile.TryGetValue(reference, out var target) &&
					target != id &&
					!solution.GetProjectDependencyGraph().GetProjectsThatThisProjectTransitivelyDependsOn(target).Contains(id))
					solution = solution.AddProjectReference(id, new ProjectReference(target));

		// The nearest folder first; of a project file and a folder of the same name, the project file.
		_owners     = [.. owners.OrderByDescending(static owner => owner.Root.Length)];
		_generating = generating;

		// What MSBuild did not place is placed by its folder.
		foreach (var path in sources)
			if (!claimed.Contains(path) && texts.TryGetValue(path, out var text) && Placed(path) is { } owner)
				solution = AddSource(solution, owner, path, SourceText.From(text));

		foreach (var path in grammars)
			if (!claimed.Contains(path) && texts.TryGetValue(path, out var text) && Placed(path) is { } owner)
				solution = AddGrammar(solution, owner, path, _open.TryGetValue(path, out var open) ? open : SourceText.From(text));

		// An open grammar not on disk yet is in the folders as its editor has it.
		foreach (var (path, text) in _open)
			if (!_grammars.ContainsKey(path) && Contained(scan.Folders, path) && !Skipped(scan.Folders, path) && Placed(path) is { } owner)
				solution = AddGrammar(solution, owner, path, text);

		return solution;
	}

	/// <summary>
	/// The project a file MSBuild did not place belongs to: the one whose folder is the nearest above it,
	/// of those MSBuild did not evaluate — an evaluated project that does not have it left it out.
	/// </summary>
	ProjectId? Placed(string path)
	{
		foreach (var (root, project, evaluated) in _owners)
			if (!evaluated && (root.Length == 0 || Within(root, path)))
				return project;

		return null;
	}

	/// <summary>Whether an evaluated project holds the grammar. Under the lock.</summary>
	bool HeldByEvaluated(string path)
	{
		return _grammars.TryGetValue(path, out var ids) &&
			ids.Any(id => _owners.Any(owner => owner.Evaluated && owner.Project == id.ProjectId));
	}

	/// <summary>Whether an evaluated project's folder holds the file, whose glob may take it in.</summary>
	bool UnderEvaluated(string path, out HashSet<string> projects)
	{
		projects = new HashSet<string>(Paths);

		foreach (var (project, _) in _evaluations)
			if (Within(Path.GetDirectoryName(project)!, path))
				projects.Add(project);

		return projects.Count > 0;
	}

	/// <summary>
	/// Whether a C# file is compiled in a project whose build runs the DotGram generator, as the project
	/// says: MSBuild's items where it was evaluated, its file's where not.
	/// </summary>
	public bool Generates(string path)
	{
		lock (_gate)
			return _sources.TryGetValue(path, out var ids) && ids.Any(id => _generating.Contains(id.ProjectId));
	}

	/// <summary>Whether a file is in one of the folders, and so has its host looked for here.</summary>
	public bool Contains(string path)
	{
		return Contained(Folders, path);
	}

	bool Contained(IReadOnlyList<string> folders, string path)
	{
		foreach (var folder in folders)
			if (Within(folder, path))
				return true;

		return false;
	}

	/// <summary>Whether a file's changes concern the project: in a folder, and not in one the walk skips; or one a project compiles.</summary>
	public bool Watches(string path)
	{
		var folders = Folders;

		if (folders.Count == 0)
			return false;

		if (Contained(folders, path) && !Skipped(folders, path))
			return true;

		lock (_gate)
			return _sources.ContainsKey(path) || _grammars.ContainsKey(path);
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

		// A project file says what its project holds and references, and an imported file may say it of
		// any project: evaluated again.
		if (path.EndsWith(ProjectExtension, StringComparison.OrdinalIgnoreCase))
		{
			Reload(null, new HashSet<string>([path], Paths));
			return;
		}

		if (path.EndsWith(".props", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".targets", StringComparison.OrdinalIgnoreCase))
		{
			if (_evaluate is not null)
				Reload(null);

			return;
		}

		var cs      = path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase);
		var grammar = path.EndsWith(GramIncludes.GramFileExtension, StringComparison.OrdinalIgnoreCase);

		if (!cs && !grammar)
			return;

		var changes = new List<WorkspaceChangeEventArgs?>();
		HashSet<string>? evaluate = null;

		lock (_gate)
		{
			// A reading of the folders running now began before this: what it builds takes this over.
			if (_since is { } since)
				since[path] = text;

			if (text is null)
				_evaluatedFor.Remove(path);

			// An open grammar is its editor's text, whatever its file now says. One opened before it was on
			// disk is in the project its folder gives it; saved below an evaluated project, that project is
			// evaluated again — once — in case its glob takes the grammar in.
			if (grammar && _open.ContainsKey(path))
			{
				if (text is null || HeldByEvaluated(path) || !_evaluatedFor.Add(path) || !UnderEvaluated(path, out var mine))
					return;

				evaluate = mine;
			}

			var known = cs ? _sources : _grammars;

			if (evaluate is not null)
			{
				// Evaluated again, below.
			}
			else if (known.TryGetValue(path, out var ids))
			{
				var solution = Workspace.CurrentSolution;

				if (text is null)
				{
					known.Remove(path);

					foreach (var id in ids)
						solution = cs ? solution.RemoveDocument(id) : solution.RemoveAdditionalDocument(id);

					changes.AddRange(Apply(solution, cs ? WorkspaceChangeKind.DocumentRemoved : WorkspaceChangeKind.AdditionalDocumentRemoved, ids));
				}
				else
				{
					foreach (var id in ids)
						solution = cs ? solution.WithDocumentText(id, SourceText.From(text)) : solution.WithAdditionalDocumentText(id, SourceText.From(text));

					// A C# file saved is a document read again from disk: what it changes is any project's to
					// say, a host's global usings or its base class among them, not only the host's own file.
					changes.AddRange(Apply(solution, cs ? WorkspaceChangeKind.DocumentReloaded : WorkspaceChangeKind.AdditionalDocumentChanged, ids));
				}
			}
			else if (text is not null)
			{
				// A new file below an evaluated project is the project's where its globs take it in.
				if (UnderEvaluated(path, out var projects))
					evaluate = projects;
				else if (Placed(path) is { } owner)
				{
					var solution = cs
						? AddSource(Workspace.CurrentSolution, owner, path, SourceText.From(text))
						: AddGrammar(Workspace.CurrentSolution, owner, path, SourceText.From(text));

					changes.AddRange(Apply(solution, cs ? WorkspaceChangeKind.DocumentAdded : WorkspaceChangeKind.AdditionalDocumentAdded, (cs ? _sources : _grammars)[path]));
				}
			}
		}

		if (evaluate is not null)
			Reload(null, evaluate);
		else
			Tell(changes);
	}

	/// <summary>The solution made the workspace's; the changes to tell, one for each document, or none where it was refused.</summary>
	IReadOnlyList<WorkspaceChangeEventArgs> Apply(Solution solution, WorkspaceChangeKind kind, IReadOnlyList<DocumentId> documents)
	{
		var before = Workspace.CurrentSolution;

		return Workspace.TryApplyChanges(solution)
			? [.. documents.Select(document => new WorkspaceChangeEventArgs(kind, before, Workspace.CurrentSolution, document.ProjectId, document))]
			: [];
	}

	/// <summary>Tells changes, outside the lock: what they set going may ask the workspace again.</summary>
	void Tell(IEnumerable<WorkspaceChangeEventArgs?> changes)
	{
		foreach (var change in changes)
			if (change is not null)
				Changed?.Invoke(change);
	}

	/// <summary>A grammar was opened: its text, unsaved edits and all, is what another grammar that includes it is compiled with.</summary>
	public void GrammarOpened(string path, SourceText text)
	{
		IReadOnlyList<WorkspaceChangeEventArgs> changes = [];

		lock (_gate)
		{
			// Kept wherever the grammar is: a folder added later finds it open.
			_open[path] = text;

			if (!Watches(path))
				return;

			if (_grammars.TryGetValue(path, out var ids))
			{
				var solution = Workspace.CurrentSolution;

				foreach (var id in ids)
					solution = solution.WithAdditionalDocumentText(id, text);

				changes = Apply(solution, WorkspaceChangeKind.AdditionalDocumentChanged, ids);
			}
			else if (Placed(path) is { } owner)
			{
				var solution = AddGrammar(Workspace.CurrentSolution, owner, path, text);

				changes = Apply(solution, WorkspaceChangeKind.AdditionalDocumentAdded, _grammars[path]);
			}
		}

		Tell(changes);
	}

	/// <summary>An open grammar was edited.</summary>
	public void GrammarEdited(string path, SourceText text)
	{
		IReadOnlyList<WorkspaceChangeEventArgs> changes;

		lock (_gate)
		{
			if (!_open.ContainsKey(path))
				return;

			_open[path] = text;

			if (!_grammars.TryGetValue(path, out var ids))
				return;

			var solution = Workspace.CurrentSolution;

			foreach (var id in ids)
				solution = solution.WithAdditionalDocumentText(id, text);

			changes = Apply(solution, WorkspaceChangeKind.AdditionalDocumentChanged, ids);
		}

		Tell(changes);
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

		Remember(_sources, path, id);

		return solution.AddDocument(DocumentInfo.Create(
			id,
			Path.GetFileName(path),
			loader: TextLoader.From(TextAndVersion.Create(text, VersionStamp.Create(), path)),
			filePath: path));
	}

	Solution AddGrammar(Solution solution, ProjectId project, string path, SourceText text)
	{
		var id = DocumentId.CreateNewId(project, path);

		Remember(_grammars, path, id);

		return solution.AddAdditionalDocument(DocumentInfo.Create(
			id,
			Path.GetFileName(path),
			loader: TextLoader.From(TextAndVersion.Create(text, VersionStamp.Create(), path)),
			filePath: path));
	}

	static void Remember(Dictionary<string, List<DocumentId>> documents, string path, DocumentId id)
	{
		if (!documents.TryGetValue(path, out var ids))
			documents[path] = ids = [];

		ids.Add(id);
	}

	/// <summary>What a project file says that this workspace reads: the projects it references and its global usings.</summary>
	sealed record ProjectFile(IReadOnlyList<string> References, IReadOnlyList<string> Usings, bool Generates = false)
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
			var generates  = false;

			foreach (var element in xml.Descendants())
			{
				foreach (var include in Items(element))
					generates |= ProjectEvaluation.IsGenerator(element.Name.LocalName, include, Metadata(element, "OutputItemType"));

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

			return new ProjectFile(references, [.. usings.Distinct(StringComparer.Ordinal)], generates);
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
