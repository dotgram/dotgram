using System;
using System.Collections.Generic;
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
/// </remarks>
sealed class OpenCSharp : IEmbeddedGrammarHost<GramVersion>
{
	/// <summary>How many versions are kept for carrying a span from an older one to the newest.</summary>
	const int History = 256;

	readonly IGrammarServices _services;
	readonly object           _gate = new();

	// The versions since the oldest one a span may still be carried from, the newest last; read and
	// written under the lock.
	readonly List<GramVersion> _versions = [];

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

		_versions.Add(first);
	}

	public string Uri { get; }

	public string? FilePath { get; }

	public GramVersion Current => Volatile.Read(ref _current);

	/// <summary>Taken while the server tells the client about the document, so that what it tells arrives in order.</summary>
	internal object Telling { get; } = new();

	/// <summary>The diagnostics last published for the document, under <see cref="Telling"/>; null before any.</summary>
	internal IReadOnlyList<HostDiagnostic>? ToldDiagnostics { get; set; }

	/// <summary>The analysis of a version was published, or the colours known were carried over to it.</summary>
	public event Action<OpenCSharp, GramVersion>? Changed;

	/// <summary>
	/// Starts the analysis, over the workspace the document's file is in; nothing is analysed before,
	/// since there is no project to read the file in until the folders are read.
	/// </summary>
	/// <returns>False where the document was closed first.</returns>
	public bool Start(FolderWorkspace workspace)
	{
		lock (_gate)
		{
			if (_closed || _session is not null)
				return false;

			_workspace         = workspace;
			workspace.Changed += WorkspaceChanged;

			var session = new EmbeddedGrammarSession<GramVersion>(this);

			session.Changed += SessionChanged;
			_session         = session;

			return true;
		}
	}

	public void Edit(int number, IReadOnlyList<TextDocumentContentChangeEvent> changes)
	{
		GramVersion before;
		GramVersion after;
		EmbeddedGrammarSession<GramVersion>? session;

		lock (_gate)
		{
			before  = _current;
			after   = before.Apply(number, changes);
			session = _session;

			_versions.Add(after);

			if (_versions.Count > History)
				_versions.RemoveRange(0, _versions.Count - History);

			Volatile.Write(ref _current, after);
		}

		// Outside the lock: the session tells of the colours it carried over at once, and what it tells is
		// read back under this lock.
		session?.Edited(before, after);
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

		lock (_gate)
			session = _closed ? null : _session;

		session?.WorkspaceChanged(change);
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
		List<GramVersion>? chain;

		lock (_gate)
			chain = Chain(from, to);

		return Translate(span, chain, to.Text.Length);
	}

	Document? IEmbeddedGrammarHost<GramVersion>.Document(GramVersion version)
	{
		FolderWorkspace? workspace;

		lock (_gate)
			workspace = _workspace;

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
	List<GramVersion>? Chain(GramVersion from, GramVersion to)
	{
		var result = new List<GramVersion>();

		if (from == to)
			return result;

		var end = _versions.IndexOf(to);

		if (end < 0)
			return null;

		for (var index = end; index > 0; index--)
		{
			var version = _versions[index];

			if (version.Changed is not { } changed || changed.From != _versions[index - 1].Id)
				return null;

			result.Add(version);

			if (_versions[index - 1] == from)
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
