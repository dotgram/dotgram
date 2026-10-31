using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Language;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace DotGram.LanguageServer;

/// <summary>One version of an open document's text, as the client numbered it.</summary>
/// <remarks>
/// Each version's text is a string of its own. Roslyn's <see cref="SourceText.WithChanges(TextChange[])"/>
/// would share the unchanged text between versions, but in the Roslyn the server compiles against it
/// throws on some edits that split a <c>\r\n</c> (<c>CompositeText.RemoveSplitLineBreaksAndEmptySegments</c>),
/// and a copy of even the largest grammar costs well under a millisecond. What changed is kept beside it,
/// for carrying the colours of the version before over until this one is analysed.
/// </remarks>
sealed class GramVersion
{
	static long _nextId;

	readonly Lazy<LineMap> _lines;

	public GramVersion(int number, SourceText text)
		: this(number, text.ToString(), null, null)
	{
	}

	GramVersion(int number, string text, LineMap? lines, (long From, TextChangeRange Change)? changed)
	{
		Id      = Interlocked.Increment(ref _nextId);
		Number  = number;
		String  = text;
		Text    = SourceText.From(text);
		Changed = changed;
		_lines  = lines is null ? new Lazy<LineMap>(() => new LineMap(Text)) : new Lazy<LineMap>(() => lines);
	}

	/// <summary>Which version this is, among every version of every document.</summary>
	public long Id { get; }

	public int Number { get; }

	public SourceText Text { get; }

	public string String { get; }

	public LineMap Lines => _lines.Value;

	/// <summary>The version this one was made from, and the one change that made it; null where it was made otherwise.</summary>
	public (long From, TextChangeRange Change)? Changed { get; }

	/// <summary>The version after the client's changes, applied in order, each to the text the one before left.</summary>
	public GramVersion Apply(int number, IReadOnlyList<TextDocumentContentChangeEvent> changes)
	{
		var text  = String;
		var lines = Lines;
		TextChangeRange? only = null;

		foreach (var change in changes)
		{
			if (change.Range is not { } range)
			{
				text  = change.Text;
				lines = new LineMap(text);
				only  = new TextChangeRange(new TextSpan(0, String.Length), text.Length);
				continue;
			}

			var start = lines.Offset(range.Start);
			var end   = Math.Max(start, lines.Offset(range.End));

			text  = string.Concat(text.AsSpan(0, start), change.Text, text.AsSpan(end));
			lines = lines.Apply(SourceText.From(text), start, end, change.Text.Length);
			only  = new TextChangeRange(TextSpan.FromBounds(start, end), change.Text.Length);
		}

		// Several changes at once are not carried over: their ranges would have to be composed.
		return new GramVersion(number, text, lines, changes.Count == 1 && only is { } single ? (Id, single) : null);
	}
}

/// <summary>
/// An open <c>.gram</c> document: the stage the editor-neutral analysis session runs on, fed with the
/// client's edits, and what the requests read of it.
/// </summary>
sealed class OpenGrammar : IGramAnalysisHost<GramVersion>
{
	readonly IGrammarServices _services;
	readonly object           _gate = new();

	readonly List<(GramVersion Version, TaskCompletionSource<GramDocument?> Done)> _waiting = [];

	// Read without the lock: the session asks for the current version while it holds its own, and
	// this document asks the session while it holds this one; one order of the two locks only.
	GramVersion          _current;
	GramVersion?         _lastPublished;
	Workspace?           _workspace;
	bool                 _started;
	bool                 _closed;

	public OpenGrammar(string uri, string? filePath, GramVersion first, IGrammarServices services)
	{
		Uri       = uri;
		FilePath  = filePath;
		_current  = first;
		_services = services;
		Session   = services.Analyze is { } analyze
			? new GramAnalysisSession<GramVersion>(this, analyze)
			: new GramAnalysisSession<GramVersion>(this);

		Session.Changed += Changed;
	}

	public string Uri { get; }

	public string? FilePath { get; }

	public GramAnalysisSession<GramVersion> Session { get; }

	/// <summary>
	/// A version's analysis was published and is still the document's: the server tells the client,
	/// at once and without waiting, since it is told under the document's lock.
	/// </summary>
	public event Action<OpenGrammar, GramAnalysisResult<GramVersion>, bool>? Published;

	public GramVersion Current => Volatile.Read(ref _current);

	/// <summary>
	/// Starts the first analysis, following the grammar's host where it has a file: once the server
	/// knows whether the file is in a folder it looks for hosts in. Until then nothing is analysed,
	/// so that nothing is told about the file before it is known how to read it.
	/// </summary>
	/// <param name="workspace">The workspace whose changes can change the grammar's host; null where none is followed.</param>
	/// <returns>False where the document was closed first.</returns>
	public bool Start(Workspace? workspace)
	{
		lock (_gate)
		{
			if (_closed || _started)
				return false;

			if (FilePath is { } path)
				Session.Follow(path);

			if (workspace is not null)
			{
				_workspace = workspace;
				workspace.WorkspaceChanged += WorkspaceChanged;
			}

			_started = true;
			Session.Document(_current);

			return true;
		}
	}

	public void Edit(int number, IReadOnlyList<TextDocumentContentChangeEvent> changes)
	{
		GramVersion before;
		GramVersion after;

		bool started;

		lock (_gate)
		{
			before  = _current;
			after   = before.Apply(number, changes);
			started = _started;

			Volatile.Write(ref _current, after);
		}

		// Before the start the session has seen no version, and the start analyses the latest.
		if (started)
			Session.Edited(before, after);
	}

	/// <summary>The server's workspace changed: the session looks for the host again where that concerns it.</summary>
	void WorkspaceChanged(object? sender, WorkspaceChangeEventArgs change)
	{
		Session.WorkspaceChanged(change);
	}

	/// <summary>The document is closed: nothing more is analysed, published or followed.</summary>
	/// <param name="closed">Told under the lock a publication takes, so that nothing published of the document comes after it.</param>
	public void Close(Action closed)
	{
		List<(GramVersion Version, TaskCompletionSource<GramDocument?> Done)> waiting;
		Workspace? workspace;

		lock (_gate)
		{
			if (_closed)
				return;

			_closed    = true;
			workspace  = _workspace;
			_workspace = null;
			waiting    = [.. _waiting];
			_waiting.Clear();

			closed();
		}

		if (workspace is not null)
			workspace.WorkspaceChanged -= WorkspaceChanged;

		Session.Close();

		foreach (var (_, done) in waiting)
			done.TrySetResult(null);
	}

	/// <summary>What the providers read of a version: its analysis as far as it is known now.</summary>
	public GramStandaloneView View(GramVersion version)
	{
		return View(version, Document(version));
	}

	/// <summary>
	/// A version's analysis as far as it is known now, its analysis started where it is not; for a
	/// version an edit has replaced, only what was published of it.
	/// </summary>
	/// <remarks>
	/// The session analyses the one version it was last asked about, so a request that still holds
	/// an older version must not ask for that one: the newer version's analysis would be dropped
	/// for an answer the client no longer wants.
	/// </remarks>
	public GramDocument Document(GramVersion version)
	{
		lock (_gate)
		{
			if (version == _current)
				return _started ? Session.Document(version) : GramAnalysisSession.EmptyDocument;

			return Session.Result is { } result && result.Version == version
				? result.Document
				: GramAnalysisSession.EmptyDocument;
		}
	}

	public GramStandaloneView View(GramVersion version, GramDocument document)
	{
		return new GramStandaloneView(
			() => version.String,
			version.Text.Length,
			document,
			position => Session.ExternalDefinition(version, position),
			(name, definitionPosition) => Session.IncludedReferences(version, name, definitionPosition));
	}

	/// <summary>
	/// The analysis of a version once it is published; null where a newer version came first, and
	/// the client, which has that one by now, no longer asks about this.
	/// </summary>
	public async Task<GramDocument?> AnalysedAsync(GramVersion version, CancellationToken cancellationToken)
	{
		var done = new TaskCompletionSource<GramDocument?>(TaskCreationOptions.RunContinuationsAsynchronously);

		lock (_gate)
		{
			if (Session.Result is { } result && result.Version == version)
				return result.Document;

			if (_closed || _current != version)
				return null;

			_waiting.Add((version, done));

			// Under the lock, so that an edit cannot come between: see Document.
			if (_started)
				Session.Document(version);
		}

		using (cancellationToken.Register(static state => ((TaskCompletionSource<GramDocument?>)state!).TrySetCanceled(), done))
			return await done.Task.ConfigureAwait(false);
	}

	void Changed(GramVersion version)
	{
		if (Session.Result is not { } result || result.Version != version)
			return;

		List<TaskCompletionSource<GramDocument?>> answered = [];
		List<TaskCompletionSource<GramDocument?>> overtaken = [];

		lock (_gate)
		{
			// Told to the client under the lock an edit and a close take, and only while the version
			// is the document's: no publication of an older version, or of a closed document, can
			// follow a newer one.
			if (!_closed && version == _current)
			{
				Published?.Invoke(this, result, _lastPublished == version);
				_lastPublished = version;
			}

			for (var index = _waiting.Count - 1; index >= 0; index--)
			{
				var (waitingFor, done) = _waiting[index];

				if (waitingFor == version)
					answered.Add(done);
				else if (waitingFor != _current)
					overtaken.Add(done);
				else
					continue;

				_waiting.RemoveAt(index);
			}
		}

		foreach (var done in answered)
			done.TrySetResult(result.Document);

		foreach (var done in overtaken)
			done.TrySetResult(null);
	}

	IGramScheduler IGramAnalysisHost<GramVersion>.Scheduler => _services.Scheduler;

	IIncludedGrammarStore IGramAnalysisHost<GramVersion>.Store => _services.Store;

	GramVersion IGramAnalysisHost<GramVersion>.Current => Current;

	string? IGramAnalysisHost<GramVersion>.FilePath => FilePath;

	string IGramAnalysisHost<GramVersion>.Text(GramVersion version)
	{
		return version.String;
	}

	GramDocument IGramAnalysisHost<GramVersion>.Translate(GramDocument document, GramVersion from, GramVersion to)
	{
		return Translate(document, from, to);
	}

	Task<StandaloneGrammarResolution> IGramAnalysisHost<GramVersion>.ResolveAsync(string filePath, CancellationToken cancellationToken)
	{
		return _services.ResolveAsync(filePath, cancellationToken);
	}

	Task<bool> IGramAnalysisHost<GramVersion>.MayHostAsync(IReadOnlyList<DocumentId> documents, string filePath, CancellationToken cancellationToken)
	{
		return _services.MayHostAsync(documents, filePath, cancellationToken);
	}

	Task IGramAnalysisHost<GramVersion>.RaiseAsync(Action raise, CancellationToken cancellationToken)
	{
		raise();
		return Task.CompletedTask;
	}

	void IGramAnalysisHost<GramVersion>.Report(Exception exception)
	{
		_services.Report(exception);
	}

	/// <summary>
	/// What was known of the old version carried over to the new one until it is analysed: the
	/// colours outside the edit, moved with the text; the rest is dropped, as in Visual Studio.
	/// </summary>
	internal static GramDocument Translate(GramDocument document, GramVersion from, GramVersion to)
	{
		if (to.Changed is not { } changed || changed.From != from.Id)
			return GramAnalysisSession.EmptyDocument;

		var changes = new[] { changed.Change };

		var result = new List<GramClassifiedSpan>(document.Classifications.Count);

		foreach (var item in document.Classifications)
		{
			if (Map(item.Position, item.Length, changes) is not { } position)
				continue;

			int? definition = item.DefinitionPosition is { } at && at < from.Text.Length
				? Map(at, 0, changes)
				: null;

			result.Add(new GramClassifiedSpan(
				position,
				item.Length,
				item.Kind,
				item.QuickInfo,
				definition,
				item.RuleSignature,
				item.RuleParameterCount,
				item.SymbolKind));
		}

		return new GramDocument(result, [], [], [], [], [], []);
	}

	/// <summary>Where a span of the old text is in the new one, or null where an edit touched it.</summary>
	static int? Map(int position, int length, IReadOnlyList<TextChangeRange> changes)
	{
		var shift = 0;

		// The ranges are in the old text's order and do not overlap: those after the span leave it
		// where it is, those before it move it, and one that reaches into it drops it.
		foreach (var change in changes)
		{
			if (change.Span.Start >= position + length)
				break;

			if (change.Span.End <= position)
			{
				shift += change.NewLength - change.Span.Length;
				continue;
			}

			return null;
		}

		return position + shift;
	}
}

/// <summary>What every open document shares: the server's clock, its store of included grammars, and the folder it looks for hosts in.</summary>
interface IGrammarServices
{
	IGramScheduler Scheduler { get; }

	IIncludedGrammarStore Store { get; }

	/// <summary>The analysis to run instead of the compiler's; a test's hook, null otherwise.</summary>
	Func<string, StandaloneGrammarContext?, bool, GramAnalysisClock, StandaloneAnalysis>? Analyze { get; }

	Task<StandaloneGrammarResolution> ResolveAsync(string filePath, CancellationToken cancellationToken);

	Task<bool> MayHostAsync(IReadOnlyList<DocumentId> documents, string filePath, CancellationToken cancellationToken);

	void Report(Exception exception);
}
