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
/// A version shares the text it did not change with the version before
/// (<see cref="SourceText.WithChanges(TextChange[])"/>): a keystroke in a large grammar is not a copy of
/// all of it. The Roslyn the server compiles against throws on some edits next to a split <c>\r\n</c>
/// (<c>CompositeText.RemoveSplitLineBreaksAndEmptySegments</c>); such an edit makes the text afresh. The
/// text as one string is made where it is asked for — by an analysis, not by every keystroke. What
/// changed is kept beside it, for carrying the colours of the version before over until this one is
/// analysed.
/// </remarks>
sealed class GramVersion
{
	static long _nextId;

	readonly Lazy<string>  _string;
	readonly Lazy<LineMap> _lines;

	public GramVersion(int number, SourceText text)
		: this(number, text, null, null)
	{
	}

	GramVersion(int number, SourceText text, LineMap? lines, (long From, IReadOnlyList<TextChangeRange> Changes)? changed)
	{
		Id      = Interlocked.Increment(ref _nextId);
		Number  = number;
		Text    = text;
		Changed = changed;
		_string = new Lazy<string>(text.ToString);
		_lines  = lines is null ? new Lazy<LineMap>(() => new LineMap(Text)) : new Lazy<LineMap>(() => lines);
	}

	/// <summary>Which version this is, among every version of every document.</summary>
	public long Id { get; }

	public int Number { get; }

	public SourceText Text { get; }

	public string String => _string.Value;

	public LineMap Lines => _lines.Value;

	/// <summary>
	/// The version this one was made from, and the changes that made it, in order, each in the text the
	/// one before it left; null where it was made otherwise.
	/// </summary>
	public (long From, IReadOnlyList<TextChangeRange> Changes)? Changed { get; }

	/// <summary>The version after the client's changes, applied in order, each to the text the one before left.</summary>
	public GramVersion Apply(int number, IReadOnlyList<TextDocumentContentChangeEvent> changes)
	{
		var text  = Text;
		var lines = Lines;
		var made  = new List<TextChangeRange>(changes.Count);

		foreach (var change in changes)
		{
			if (change.Range is not { } range)
			{
				made.Add(new TextChangeRange(new TextSpan(0, lines.Length), change.Text.Length));
				text  = SourceText.From(change.Text);
				lines = new LineMap(text);
				continue;
			}

			var start = lines.Offset(range.Start);
			var end   = Math.Max(start, lines.Offset(range.End));

			text  = WithChange(text, start, end, change.Text);
			lines = lines.Apply(text, start, end, change.Text.Length);
			made.Add(new TextChangeRange(TextSpan.FromBounds(start, end), change.Text.Length));
		}

		return new GramVersion(number, text, lines, (Id, made));
	}

	static SourceText WithChange(SourceText text, int start, int end, string inserted)
	{
		try
		{
			return text.WithChanges(new TextChange(TextSpan.FromBounds(start, end), inserted));
		}
		catch (ArgumentOutOfRangeException)
		{
			// Roslyn's own failure next to a split line break: the text is made afresh instead.
			var whole = text.ToString();

			return SourceText.From(string.Concat(whole.AsSpan(0, start), inserted, whole.AsSpan(end)));
		}
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
	object?              _lastResult;
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

	/// <summary>How many requests wait for an analysis now.</summary>
	internal int Waiting
	{
		get
		{
			lock (_gate)
				return _waiting.Count;
		}
	}

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
		List<TaskCompletionSource<GramDocument?>> overtaken;

		lock (_gate)
		{
			before  = _current;
			after   = before.Apply(number, changes);
			started = _started;

			Volatile.Write(ref _current, after);

			// A request waiting for the version before has its answer now: it was overtaken, and the
			// version's text is not held for it.
			overtaken = [.. _waiting.Select(static item => item.Done)];
			_waiting.Clear();
		}

		foreach (var done in overtaken)
			done.TrySetResult(null);

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

		var waiter = (version, done);

		// A cancelled request lets go of the version it waited for at once, not at its publication.
		using (cancellationToken.Register(() =>
		{
			lock (_gate)
				_waiting.Remove(waiter);

			done.TrySetCanceled(cancellationToken);
		}))
			return await done.Task.ConfigureAwait(false);
	}

	void Changed(GramVersion version)
	{
		List<TaskCompletionSource<GramDocument?>> answered = [];
		List<TaskCompletionSource<GramDocument?>> overtaken = [];
		GramAnalysisResult<GramVersion>? result;

		lock (_gate)
		{
			// The latest result, read under the lock: two analyses of one version raise this in
			// either order, and the one told last must be the one made last.
			result = Session.Result;

			if (result is null || result.Version != version)
				return;

			// Told to the client under the lock an edit and a close take, and only while the version
			// is the document's and the result one not told yet: no publication of an older version,
			// of an older analysis, or of a closed document can follow a newer one.
			if (!_closed && version == _current && !ReferenceEquals(result, _lastResult))
			{
				Published?.Invoke(this, result, _lastPublished == version);
				_lastPublished = version;
				_lastResult    = result;
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

		var changes = changed.Changes;
		var result = new List<GramClassifiedSpan>(document.Classifications.Count);

		foreach (var item in document.Classifications)
		{
			if (Map(item.Position, item.Length, changes) is not { } position)
				continue;

			int? definition = item.DefinitionPosition is { } at && at < from.Text.Length
				? MapStart(at, changes)
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
	/// <summary>
	/// Where the start of a token is in the new text: text inserted right at it moves it, as it moves
	/// the token; a change that replaces its first character drops it, as it drops the token.
	/// </summary>
	static int? MapStart(int position, IReadOnlyList<TextChangeRange> changes)
	{
		foreach (var change in changes)
		{
			if (change.Span.End <= position)
			{
				position += change.NewLength - change.Span.Length;
				continue;
			}

			if (change.Span.Start > position)
				continue;

			return null;
		}

		return position;
	}

	static int? Map(int position, int length, IReadOnlyList<TextChangeRange> changes)
	{
		// The changes in the order they were made, each in the text the one before it left: one
		// after the span leaves it where it is, one before it moves it, one that reaches into it drops it.
		foreach (var change in changes)
		{
			if (change.Span.Start >= position + length)
				continue;

			if (change.Span.End <= position)
			{
				position += change.NewLength - change.Span.Length;
				continue;
			}

			return null;
		}

		return position;
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
