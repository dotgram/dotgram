using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.LanguageServer;

/// <summary>
/// An open C# document: the stage the editor-neutral analysis of the grammars written into it — its
/// <c>[Gram("…")]</c> strings, the strings passed to a <c>StringSyntax("DotGram")</c> parameter, and the
/// DSL literals of a <c>[GramLanguage]</c> — runs on, fed with the client's edits, as Visual Studio feeds
/// it a C# buffer's.
/// </summary>
/// <remarks>
/// The document is read as the server's workspace holds its file, its text replaced by the client's: the
/// workspace itself follows the file as it is saved, not as it is typed, as it does for every C# file.
/// <para>
/// Nothing is analysed of a document that cannot hold a grammar: one whose text names none of
/// <see cref="Markers"/> and whose project, and the projects it references, declare no DSL with a
/// <c>[GramLanguage]</c>. A semantic model of every C# file opened would cost what the C# extension
/// already pays, a second time, for nothing; the session starts once an edit or a change of the
/// workspace makes the document one that can.
/// </para>
/// <para>
/// One lock only is ever taken inside another: this document's, then its session's. What the session
/// asks of the document while it holds its own — a span carried to a later version, the text, the
/// workspace's document — is read without a lock, from fields written whole.
/// </para>
/// </remarks>
sealed class OpenCSharp : IEmbeddedGrammarHost<GramVersion>
{
	/// <summary>How many versions are kept for carrying a span from an older one to the newest.</summary>
	const int History = 256;

	/// <summary>What a C# text that holds a grammar, or passes one to a parameter that takes one, names.</summary>
	internal static readonly string[] Markers = ["[Gram", "Gram(", "GramAttribute", "StringSyntax", LanguageMarker];

	/// <summary>What a project that declares a DSL names, in its own files.</summary>
	const string LanguageMarker = "GramLanguage";

	static long _nextSession;

	readonly IGrammarServices _services;
	readonly object           _gate = new();

	// The versions since the oldest one a span may still be carried from, the newest last: written whole
	// under the lock, read without it.
	GramVersion[] _versions;

	GramVersion                       _current;
	FolderWorkspace?                  _workspace;
	EmbeddedGrammarSession<GramVersion>? _session;
	bool                              _closed;

	public OpenCSharp(string uri, string? filePath, GramVersion first, IGrammarServices services)
	{
		Uri       = uri;
		FilePath  = filePath;
		_current  = first;
		_services = services;
		_versions = [first];
		Session   = Interlocked.Increment(ref _nextSession);
	}

	public string Uri { get; }

	/// <summary>Which opening of a document this is, among every one the server has seen; later ones are greater.</summary>
	public long Session { get; }

	/// <summary>Whether the document's grammars are analysed: it can hold one.</summary>
	public bool Analysed
	{
		get
		{
			lock (_gate)
				return _session is not null;
		}
	}

	public string? FilePath { get; }

	public GramVersion Current => Volatile.Read(ref _current);

	/// <summary>Taken while the server tells the client about the document, so that what it tells arrives in order.</summary>
	internal object Telling { get; } = new();

	/// <summary>The diagnostics last published for the document, under <see cref="Telling"/>; null before any.</summary>
	internal IReadOnlyList<HostDiagnostic>? ToldDiagnostics { get; set; }

	/// <summary>The analysis of a version was published, or the colours known were carried over to it.</summary>
	public event Action<OpenCSharp, GramVersion>? Changed;

	/// <summary>
	/// Starts following the workspace the document's file is in, and the analysis where the document can
	/// hold a grammar; nothing is analysed before, since there is no project to read the file in until the
	/// folders are read.
	/// </summary>
	/// <returns>False where the document was closed first.</returns>
	public bool Start(FolderWorkspace workspace)
	{
		GramVersion current;

		lock (_gate)
		{
			if (_closed || _workspace is not null)
				return false;

			Volatile.Write(ref _workspace, workspace);
			workspace.Changed += WorkspaceChanged;
			current = _current;
		}

		if (Marked(current.Text, new TextSpan(0, current.Text.Length)))
			Begin();
		else
			_services.Scheduler.Run(DeclaredAsync);

		return true;
	}

	/// <summary>The analysis starts, where it has not.</summary>
	void Begin()
	{
		lock (_gate)
		{
			if (_closed || _session is not null || _workspace is null)
				return;

			var session = new EmbeddedGrammarSession<GramVersion>(this);

			session.Changed += SessionChanged;
			_session         = session;
		}
	}

	/// <summary>The analysis starts where the document's project, or one it references, declares a DSL.</summary>
	async Task DeclaredAsync()
	{
		try
		{
			if (Volatile.Read(ref _workspace) is not { } workspace || Analysed)
				return;

			if (await DeclaresLanguageAsync(workspace.Workspace.CurrentSolution, FilePath, CancellationToken.None).ConfigureAwait(false))
				Begin();
		}
		catch (Exception exception) when (GramFailures.Recoverable(exception))
		{
			_services.Report("Looking for the DSLs a C# document may use", exception);
		}
	}

	/// <summary>Whether the projects a C# file is compiled in, or a project they reference, declare a DSL in a file of their own.</summary>
	internal static async Task<bool> DeclaresLanguageAsync(Solution solution, string? filePath, CancellationToken cancellationToken)
	{
		if (filePath is null)
			return false;

		var graph    = solution.GetProjectDependencyGraph();
		var projects = new HashSet<ProjectId>();

		foreach (var id in solution.GetDocumentIdsWithFilePath(filePath))
		{
			projects.Add(id.ProjectId);
			projects.UnionWith(graph.GetProjectsThatThisProjectTransitivelyDependsOn(id.ProjectId));
		}

		foreach (var id in projects)
			foreach (var document in solution.GetProject(id)?.Documents ?? [])
			{
				// What the server adds to every project, the generator's attribute declarations among them, has no file.
				if (document.FilePath is null)
					continue;

				var text = document.TryGetText(out var loaded)
					? loaded
					: await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

				if (text.ToString().Contains(LanguageMarker, StringComparison.Ordinal))
					return true;
			}

		return false;
	}

	/// <summary>Whether a span of a text, and what touches it, names one of the <see cref="Markers"/>.</summary>
	internal static bool Marked(SourceText text, TextSpan span)
	{
		var reach = Markers.Max(static marker => marker.Length);
		var start = Math.Max(0, span.Start - reach);
		var end   = Math.Min(text.Length, span.End + reach);
		var part  = text.ToString(TextSpan.FromBounds(start, end));

		foreach (var marker in Markers)
			if (part.Contains(marker, StringComparison.Ordinal))
				return true;

		return false;
	}

	/// <summary>Whether what an edit inserted names one of the <see cref="Markers"/>.</summary>
	static bool Marked(SourceText before, SourceText after)
	{
		var delta = 0;

		foreach (var range in after.GetChangeRanges(before))
		{
			var start = range.Span.Start + delta;

			if (Marked(after, new TextSpan(start, range.NewLength)))
				return true;

			delta += range.NewLength - range.Span.Length;
		}

		return false;
	}

	public void Edit(int number, IReadOnlyList<TextDocumentContentChangeEvent> changes)
	{
		GramVersion before;
		GramVersion after;
		EmbeddedGrammarSession<GramVersion>? session;
		bool following;

		lock (_gate)
		{
			before    = _current;
			after     = before.Apply(number, changes);
			session   = _session;
			following = _workspace is not null && !_closed;

			var versions = _versions;

			Volatile.Write(ref _versions, [.. versions.Skip(Math.Max(0, versions.Length + 1 - History)), after]);
			Volatile.Write(ref _current, after);
		}

		// Outside the lock: the session tells of the colours it carried over at once, and what it tells is
		// read back under this lock.
		if (session is not null)
			session.Edited(before, after);
		else if (following && Marked(before.Text, after.Text))
			Begin();
	}

	/// <summary>The document is closed: nothing more is analysed or told.</summary>
	public void Close()
	{
		EmbeddedGrammarSession<GramVersion>? session;
		FolderWorkspace? workspace;

		lock (_gate)
		{
			if (_closed)
				return;

			_closed    = true;
			session    = _session;
			workspace  = _workspace;
			_workspace = null;
		}

		if (workspace is not null)
			workspace.Changed -= WorkspaceChanged;

		session?.Close();
	}

	/// <summary>
	/// What is known of a version, where it is still the document's; nothing otherwise, and nothing is
	/// started for it: the session analyses the one version it was last asked about, and a request that
	/// holds an older one must not take that analysis from the newer.
	/// </summary>
	public EmbeddedGrammarView View(GramVersion version)
	{
		lock (_gate)
		{
			if (_closed || _session is not { } session || version != _current)
				return EmbeddedGrammarView.None;

			return session.View(version);
		}
	}

	void SessionChanged(GramVersion version)
	{
		Changed?.Invoke(this, version);
	}

	void WorkspaceChanged(WorkspaceChangeEventArgs change)
	{
		EmbeddedGrammarSession<GramVersion>? session;
		bool closed;

		lock (_gate)
		{
			session = _session;
			closed  = _closed;
		}

		if (closed)
			return;

		// A document that could hold no grammar may now use a DSL a project declares.
		if (session is null)
			_services.Scheduler.Run(DeclaredAsync);
		else
			session.WorkspaceChanged(change);
	}

	IGramScheduler IEmbeddedGrammarHost<GramVersion>.Scheduler => _services.Scheduler;

	GramVersion IEmbeddedGrammarHost<GramVersion>.Current => Current;

	string? IEmbeddedGrammarHost<GramVersion>.FilePath => FilePath;

	string IEmbeddedGrammarHost<GramVersion>.Text(GramVersion version)
	{
		return version.String;
	}

	int IEmbeddedGrammarHost<GramVersion>.Length(GramVersion version)
	{
		return version.Text.Length;
	}

	char IEmbeddedGrammarHost<GramVersion>.At(GramVersion version, int position)
	{
		return version.Text[position];
	}

	TextSpan IEmbeddedGrammarHost<GramVersion>.Translate(TextSpan span, GramVersion from, GramVersion to)
	{
		// Without the lock: the session asks while it holds its own.
		return Translate(span, Chain(Volatile.Read(ref _versions), from, to), to.Text.Length);
	}

	Document? IEmbeddedGrammarHost<GramVersion>.Document(GramVersion version)
	{
		var workspace = Volatile.Read(ref _workspace);

		return workspace is null
			? null
			: EmbeddedGrammarDocuments.WithText(workspace.Workspace.CurrentSolution, FilePath, version.String);
	}

	Task IEmbeddedGrammarHost<GramVersion>.RaiseAsync(Action raise, CancellationToken cancellationToken)
	{
		raise();
		return Task.CompletedTask;
	}

	void IEmbeddedGrammarHost<GramVersion>.Report(string operation, Exception exception)
	{
		_services.Report(operation, exception);
	}

	/// <summary>
	/// The versions after <paramref name="from"/> up to <paramref name="to"/>, each made from the one
	/// before it; null where <paramref name="from"/> is not among those kept, or is not before the other.
	/// </summary>
	static List<GramVersion>? Chain(GramVersion[] versions, GramVersion from, GramVersion to)
	{
		var result = new List<GramVersion>();

		if (from == to)
			return result;

		var end = Array.IndexOf(versions, to);

		if (end < 0)
			return null;

		for (var index = end; index > 0; index--)
		{
			var version = versions[index];

			if (version.Changed is not { } changed || changed.From != versions[index - 1].Id)
				return null;

			result.Add(version);

			if (versions[index - 1] == from)
			{
				result.Reverse();
				return result;
			}
		}

		return null;
	}

	/// <summary>
	/// A span carried through the changes that made each version of <paramref name="chain"/>, edges not
	/// growing: text inserted at its start or its end stays outside it, as Visual Studio's edge-exclusive
	/// tracking keeps it; a span whose versions are not known any more is an empty one, where nothing is.
	/// </summary>
	internal static TextSpan Translate(TextSpan span, List<GramVersion>? chain, int length)
	{
		if (chain is null)
			return new TextSpan(Math.Min(span.Start, length), 0);

		var start = span.Start;
		var end   = span.End;

		foreach (var version in chain)
			foreach (var change in version.Changed!.Value.Changes)
			{
				start = Track(start, change, forward: true);
				end   = Track(end, change, forward: false);
			}

		start = Math.Min(start, length);
		end   = Math.Min(Math.Max(start, end), length);

		return TextSpan.FromBounds(start, end);
	}

	/// <summary>
	/// A point carried through one change: one before it stays, one after it moves by what the change
	/// added or took away, one inside it goes to the end of the new text where it tracks forward and to
	/// its start where it tracks back.
	/// </summary>
	static int Track(int point, TextChangeRange change, bool forward)
	{
		var start = change.Span.Start;
		var end   = change.Span.End;

		if (point < start)
			return point;

		if (point > end || point == end && end > start)
			return point + change.NewLength - change.Span.Length;

		return forward ? start + change.NewLength : start;
	}
}
