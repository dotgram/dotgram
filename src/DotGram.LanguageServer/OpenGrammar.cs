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
sealed class GramVersion
{
	readonly Lazy<string>  _string;
	readonly Lazy<LineMap> _lines;

	public GramVersion(int number, SourceText text)
	{
		Number  = number;
		Text    = text;
		_string = new Lazy<string>(text.ToString);
		_lines  = new Lazy<LineMap>(() => new LineMap(text));
	}

	public int Number { get; }

	public SourceText Text { get; }

	public string String => _string.Value;

	public LineMap Lines => _lines.Value;

	/// <summary>The version after the client's changes, applied in order, each to the text the one before left.</summary>
	public GramVersion Apply(int number, IReadOnlyList<TextDocumentContentChangeEvent> changes)
	{
		var text  = Text;
		var lines = Lines;

		foreach (var change in changes)
		{
			if (change.Range is not { } range)
			{
				text  = SourceText.From(change.Text);
				lines = new LineMap(text);
				continue;
			}

			var start = lines.Offset(range.Start);
			var end   = Math.Max(start, lines.Offset(range.End));

			text  = text.WithChanges(new TextChange(TextSpan.FromBounds(start, end), change.Text));
			lines = new LineMap(text);
		}

		return new GramVersion(number, text);
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

	GramVersion          _current;
	GramVersion?         _lastPublished;
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

	/// <summary>A version's analysis was published: the server tells the client.</summary>
	public event Action<OpenGrammar, GramAnalysisResult<GramVersion>, bool>? Published;

	public GramVersion Current
	{
		get
		{
			lock (_gate)
				return _current;
		}
	}

	/// <summary>Starts the first analysis, following the grammar's host where it has a file.</summary>
	public void Start()
	{
		if (FilePath is { } path)
			Session.Follow(path);

		Session.Document(Current);
	}

	public void Edit(int number, IReadOnlyList<TextDocumentContentChangeEvent> changes)
	{
		GramVersion before;
		GramVersion after;

		lock (_gate)
		{
			before   = _current;
			after    = before.Apply(number, changes);
			_current = after;
		}

		Session.Edited(before, after);
	}

	/// <summary>The server's workspace changed: the session looks for the host again where that concerns it.</summary>
	public void WorkspaceChanged(object? sender, WorkspaceChangeEventArgs change)
	{
		Session.WorkspaceChanged(change);
	}

	public void Close()
	{
		List<(GramVersion Version, TaskCompletionSource<GramDocument?> Done)> waiting;

		lock (_gate)
		{
			_closed = true;
			waiting = [.. _waiting];
			_waiting.Clear();
		}

		Session.Close();

		foreach (var (_, done) in waiting)
			done.TrySetResult(null);
	}

	/// <summary>What the providers read of a version: its analysis as far as it is known now.</summary>
	public GramStandaloneView View(GramVersion version)
	{
		return View(version, Session.Document(version));
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
		}

		Session.Document(version);

		using (cancellationToken.Register(static state => ((TaskCompletionSource<GramDocument?>)state!).TrySetCanceled(), done))
			return await done.Task.ConfigureAwait(false);
	}

	void Changed(GramVersion version)
	{
		if (Session.Result is not { } result || result.Version != version)
			return;

		List<TaskCompletionSource<GramDocument?>> answered = [];
		List<TaskCompletionSource<GramDocument?>> overtaken = [];
		bool republished;

		lock (_gate)
		{
			republished    = _lastPublished == version;
			_lastPublished = version;

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

		Published?.Invoke(this, result, republished);
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
		var changes = to.Text.GetChangeRanges(from.Text);

		if (changes.Count == 0)
			return document;

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
