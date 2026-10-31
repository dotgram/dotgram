using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using DotGram.Grammar;

using Microsoft.CodeAnalysis;

namespace DotGram.Language;

/// <summary>The clock and the threads an analysis session runs on.</summary>
/// <remarks>
/// A session waits — for a burst of typing to end, for a solution to settle — and runs its work
/// off the caller's thread. Both are asked of this, so that a test can run a session on a clock it
/// moves itself and see in order what a real one does over seconds.
/// </remarks>
interface IGramScheduler
{
	/// <summary>Milliseconds since some fixed moment, never going back.</summary>
	long Now { get; }

	/// <summary>Completes after the delay, or is cancelled.</summary>
	Task DelayAsync(int milliseconds, CancellationToken cancellationToken);

	/// <summary>Starts work off the caller's thread.</summary>
	void Run(Func<Task> work);
}

/// <summary>The machine's clock and its thread pool.</summary>
sealed class GramScheduler : IGramScheduler
{
	readonly Stopwatch _clock = Stopwatch.StartNew();

	public static GramScheduler Instance { get; } = new();

	public long Now => _clock.ElapsedMilliseconds;

	public Task DelayAsync(int milliseconds, CancellationToken cancellationToken)
	{
		return Task.Delay(milliseconds, cancellationToken);
	}

	public void Run(Func<Task> work)
	{
		_ = Task.Run(work);
	}
}

/// <summary>What a standalone grammar's analysis session asks of the editor that holds the document.</summary>
/// <typeparam name="TVersion">
/// One version of the document's text, compared by reference: a result is published for the version
/// it was computed from, and only while that version is still the current one.
/// </typeparam>
interface IGramAnalysisHost<TVersion> where TVersion : class
{
	IGramScheduler Scheduler { get; }

	/// <summary>Where included grammars that have no file of their own are opened, and which paths are such copies.</summary>
	IIncludedGrammarStore Store { get; }

	/// <summary>The document's version now.</summary>
	TVersion Current { get; }

	/// <summary>The document's file as it is called now: a rename moves it.</summary>
	string? FilePath { get; }

	string Text(TVersion version);

	/// <summary>An analysis of one version carried over to the next, where nothing newer is known yet.</summary>
	GramDocument Translate(GramDocument document, TVersion from, TVersion to);

	/// <summary>The grammar's host, looked for in the solution as it is now.</summary>
	Task<StandaloneGrammarResolution> ResolveAsync(string filePath, CancellationToken cancellationToken);

	/// <summary>Whether one of these edited documents now declares a class that could be the grammar's host.</summary>
	Task<bool> MayHostAsync(IReadOnlyList<DocumentId> documents, string filePath, CancellationToken cancellationToken);

	/// <summary>Where the session's <c>Changed</c> event may be raised: Visual Studio raises its own events on its main thread.</summary>
	Task BeforeChangedAsync(CancellationToken cancellationToken);

	/// <summary>An exception the session caught and went on after.</summary>
	void Report(Exception exception);
}

/// <summary>What one analysis cost, and whether it was shown.</summary>
/// <param name="TextLength">The text analysed, the included grammars spliced on after it counted.</param>
/// <param name="Compile">The compiler's part: lexing, parsing, binding, normalization, the checks and emission.</param>
/// <param name="PostProcessing">The editor's part after it: classification, symbols, structure, and the result cut to the document's own text.</param>
/// <param name="AllocatedBytes">Allocated on the analysing thread, or -1 where the runtime cannot say.</param>
/// <param name="Published">False where a newer version came first and the result was dropped as stale.</param>
readonly record struct GramAnalysisTiming(int TextLength, TimeSpan Compile, TimeSpan PostProcessing, long AllocatedBytes, bool Published)
{
	public TimeSpan Total => Compile + PostProcessing;
}

/// <summary>An analysis that was published: the version it is of, and what was known when it was made.</summary>
sealed record GramAnalysisResult<TVersion>(
	TVersion Version,
	GramDocument Document,
	IReadOnlyList<GramSymbolOccurrence> IncludedSymbols,
	StandaloneGrammarResolution Resolution,
	GramAnalysisTiming Timing) where TVersion : class;

/// <summary>What every <see cref="GramAnalysisSession{TVersion}"/> shares: its timings, and the analysis and the questions that need no session.</summary>
static class GramAnalysisSession
{
	internal const int AnalysisDelayMilliseconds = 150;

	/// <summary>
	/// How long after the last change that concerns the grammar its host is looked for again:
	/// a build, a branch switch or a solution load is a burst of changes, answered once.
	/// </summary>
	internal const int ResolveDelayMilliseconds = 500;

	/// <summary>
	/// While the host cannot be asked yet, it is asked again this long after, doubling, a few
	/// times; every change to the project starts the count again. Nothing gives up for good.
	/// </summary>
	internal const int RetryDelayMilliseconds = 1000;
	internal const int RetryCount             = 5;

	/// <summary>
	/// How long a grammar whose host is not settled stays quiet about what depends on its
	/// includes. Past it the grammar is told about as it stands, and is told again the moment
	/// the host is found.
	/// </summary>
	internal const int SettleMilliseconds = 20000;

	internal static readonly GramDocument EmptyDocument = new([], [], [], [], [], [], []);
	internal static readonly IReadOnlyList<GramSymbolOccurrence> NoSymbols = [];

	internal static StandaloneDefinition? ExternalDefinition(
		string text,
		StandaloneGrammarContext context,
		IReadOnlyList<GramSymbolOccurrence> symbols,
		int position)
	{
		// A use the binder resolved past the end of the text: into an included grammar, by a
		// `using`, a qualified name or a name qualified more than once.
		foreach (var symbol in symbols)
			if (symbol.Position <= position && position < symbol.Position + symbol.Length)
				return symbol.DefinitionPosition >= text.Length &&
					context.Locate(symbol.DefinitionPosition, text.Length) is { } location
						? new StandaloneDefinition(symbol.Position, symbol.Length, location)
						: null;

		// The included grammar's own name, as `using Sql92;` and `Sql92.Word` write it.
		if (text.Length == 0)
			return null;

		var bounded = Math.Max(0, Math.Min(position, text.Length - 1));
		var start   = bounded;
		var end     = bounded;

		while (start > 0 && IsIdentifier(text[start - 1]))
			start--;

		while (end < text.Length && IsIdentifier(text[end]))
			end++;

		if (start == end)
			return null;

		var name = text.Substring(start, end - start);

		foreach (var included in context.Included)
			if (name == included.Name)
				return new StandaloneDefinition(start, end - start, new StandaloneLocation(included, 0, 0, 0));

		return null;
	}

	internal static IReadOnlyList<StandaloneReference> IncludedReferences(
		IReadOnlyList<GramSymbolOccurrence> included,
		StandaloneGrammarContext context,
		int ownLength,
		string name,
		int definitionPosition)
	{
		var result = new List<StandaloneReference>();

		foreach (var symbol in included)
			if (symbol.Name == name &&
				symbol.DefinitionPosition == definitionPosition &&
				context.Locate(symbol.Position, ownLength) is { } location)
				result.Add(new StandaloneReference(location, symbol.Length, symbol.IsDefinition));

		return result;
	}

	/// <summary>
	/// A grammar read with what it includes spliced on after it, and told about only as far as
	/// its own text goes. Until the includes are known, what depends on them is not said: a
	/// name from an included grammar is not missing, it has not been looked up yet.
	/// </summary>
	/// <remarks>
	/// Compiled as the generator compiles it: with the host's <c>Lexical</c>, and with the
	/// own text's length where something is included, so that a <c>parse</c> in an included
	/// grammar is that grammar's and not this one's.
	/// </remarks>
	internal static StandaloneAnalysis Analyze(
		string own,
		StandaloneGrammarContext? context,
		bool suppressContextDiagnostics)
	{
		return Analyze(own, context, suppressContextDiagnostics, null);
	}

	internal static StandaloneAnalysis Analyze(
		string own,
		StandaloneGrammarContext? context,
		bool suppressContextDiagnostics,
		GramAnalysisClock? clock)
	{
		var tail    = context?.AnalysisTail ?? "";
		var options = context is { } known
			? new GramAnalysisOptions
			{
				Own     = tail.Length == 0 ? null : own.Length,
				Cuts    = tail.Length == 0
					? null
					: GrammarSplice.Cuts(
						own + tail, known.Included.Select(item => (own.Length + item.Start, item.Text.Length))),
				Lexical = known.Lexical,
			}
			: null;
		var whole   = GramLanguageService.Analyze(own + tail, options, clock);

		clock?.StartPostProcessing();

		var result = new StandaloneAnalysis(
			Project(whole, own.Length, suppressContextDiagnostics),
			tail.Length == 0 ? NoSymbols : whole.Symbols.Where(item => item.Position >= own.Length).ToArray());

		clock?.Stop();

		return result;
	}

	/// <summary>
	/// Whether a change to the solution can change what this grammar is compiled with: its
	/// host, what the host includes, or how it is read.
	/// </summary>
	/// <remarks>
	/// The grammar's own text is not among them — it is the document's, and read from there —
	/// and nothing in a project that neither holds the grammar nor declares what it includes
	/// is either. Until the host is found, an edit to a document of the grammar's own project
	/// may be the one that makes it findable; that is answered by looking at the one document
	/// (<see cref="StandaloneGrammarConcern.Document"/>), not by looking for the host again.
	/// </remarks>
	internal static StandaloneGrammarConcern Concerns(
		WorkspaceChangeEventArgs change,
		StandaloneGrammarResolution resolution,
		string filePath)
	{
		switch (change.Kind)
		{
			case WorkspaceChangeKind.SolutionAdded:
			case WorkspaceChangeKind.SolutionChanged:
			case WorkspaceChangeKind.SolutionRemoved:
			case WorkspaceChangeKind.SolutionCleared:
			case WorkspaceChangeKind.SolutionReloaded:
				return StandaloneGrammarConcern.Full;
		}

		if (change.ProjectId is not { } project)
			return StandaloneGrammarConcern.Full;

		if (change.Kind == WorkspaceChangeKind.AdditionalDocumentChanged &&
			change.DocumentId is { } edited &&
			string.Equals(
				change.NewSolution.GetAdditionalDocument(edited)?.FilePath,
				filePath,
				StringComparison.OrdinalIgnoreCase))
			return StandaloneGrammarConcern.None;

		if (change.DocumentId is { } document && resolution.Documents.Contains(document))
			return StandaloneGrammarConcern.Full;

		var watched = resolution.Projects.Contains(project);

		switch (change.Kind)
		{
			case WorkspaceChangeKind.DocumentChanged:
				return watched && resolution.State != StandaloneGrammarState.Resolved
					? StandaloneGrammarConcern.Document
					: StandaloneGrammarConcern.None;

			case WorkspaceChangeKind.AdditionalDocumentChanged:
			case WorkspaceChangeKind.AnalyzerConfigDocumentChanged:
				return StandaloneGrammarConcern.None;

			// What can put the grammar into a project, or take it out: asked of both
			// solutions, since a removal is only in the old one.
			case WorkspaceChangeKind.ProjectAdded:
			case WorkspaceChangeKind.ProjectRemoved:
			case WorkspaceChangeKind.ProjectReloaded:
			case WorkspaceChangeKind.DocumentAdded:
			case WorkspaceChangeKind.DocumentRemoved:
			case WorkspaceChangeKind.DocumentReloaded:
			case WorkspaceChangeKind.AdditionalDocumentAdded:
			case WorkspaceChangeKind.AdditionalDocumentRemoved:
			case WorkspaceChangeKind.AdditionalDocumentReloaded:
				return watched ||
					resolution.State != StandaloneGrammarState.Resolved &&
					(Holds(change.NewSolution, project, filePath) || Holds(change.OldSolution, project, filePath))
						? StandaloneGrammarConcern.Full
						: StandaloneGrammarConcern.None;

			default:
				return watched ? StandaloneGrammarConcern.Full : StandaloneGrammarConcern.None;
		}
	}

	static bool Holds(Solution solution, ProjectId project, string filePath)
	{
		return solution.GetProject(project)?.AdditionalDocuments.Any(document =>
			string.Equals(document.FilePath, filePath, StringComparison.OrdinalIgnoreCase)) == true;
	}

	static bool IsIdentifier(char character)
	{
		return character == '_' || char.IsLetterOrDigit(character);
	}

	static GramDocument Project(GramDocument document, int length, bool suppressContextDiagnostics)
	{
		return new(
			document.Classifications.Where(item => item.Position < length).ToArray(),
			document.Diagnostics.Where(item =>
				item.Position < length &&
				(!suppressContextDiagnostics || IsSyntaxDiagnostic(item.Id))).ToArray(),
			document.Symbols.Where(item => item.Position < length).ToArray(),
			document.Braces.Where(item => item.OpenPosition < length && item.ClosePosition < length).ToArray(),
			document.FoldingRanges.Where(item => item.Position < length).ToArray(),
			document.DocumentSymbols.Where(item => item.Position < length).ToArray(),
			document.PublishedApis.Where(item => item.Position < length).ToArray());
	}

	static bool IsSyntaxDiagnostic(string id)
	{
		return id.StartsWith("GRAM1", StringComparison.Ordinal) ||
		id.StartsWith("GRAM2", StringComparison.Ordinal);
	}

	internal static bool Same(StandaloneGrammarContext? left, StandaloneGrammarContext? right)
	{
		return left is { } one && right is { } other
			? one.SameAs(other)
			: left is null && right is null;
	}
}

/// <summary>
/// The analysis of one open standalone <c>.gram</c> document, kept current: re-analysed shortly after
/// typing stops, compiled with what its host includes once the host is found, and found again when
/// the solution changes in a way that concerns it.
/// </summary>
/// <remarks>
/// The editor hands in its edits and its solution's changes and reads back the document of a
/// version; everything about when to analyse, when to look for the host and what to drop as stale
/// is decided here, the same for every editor.
/// </remarks>
sealed class GramAnalysisSession<TVersion> where TVersion : class
{
	static readonly GramDocument EmptyDocument = GramAnalysisSession.EmptyDocument;
	static readonly IReadOnlyList<GramSymbolOccurrence> NoSymbols = GramAnalysisSession.NoSymbols;

	readonly IGramAnalysisHost<TVersion> _host;
	readonly Func<string, StandaloneGrammarContext?, bool, GramAnalysisClock, StandaloneAnalysis> _analyze;
	readonly object        _gate         = new();
	readonly SemaphoreSlim _analysisGate = new(1, 1);

	TVersion?                           _snapshot;
	GramDocument?                       _document;
	IReadOnlyList<GramSymbolOccurrence> _includedSymbols = NoSymbols;
	StandaloneGrammarContext?           _analysedContext;
	TVersion?                           _scheduledSnapshot;
	CancellationTokenSource?            _analysisCancellation;
	GramAnalysisResult<TVersion>?       _result;

	bool                         _following;
	CancellationTokenSource?     _resolveCancellation;
	StandaloneGrammarResolution  _resolution = StandaloneGrammarResolution.NotReady;
	long?                        _followedSince;
	int                          _retries;
	bool                         _configured;
	bool                         _copy;
	bool                         _closed;
	bool                         _pendingFull;
	readonly HashSet<DocumentId> _pendingDocuments = [];

	public GramAnalysisSession(IGramAnalysisHost<TVersion> host)
		: this(host, null)
	{
	}

	/// <param name="host">The editor.</param>
	/// <param name="analyze">What analyses a text; <see cref="GramAnalysisSession.Analyze(string, StandaloneGrammarContext?, bool, GramAnalysisClock?)"/> where null. A test's hook.</param>
	internal GramAnalysisSession(
		IGramAnalysisHost<TVersion> host,
		Func<string, StandaloneGrammarContext?, bool, GramAnalysisClock, StandaloneAnalysis>? analyze)
	{
		_host    = host;
		_analyze = analyze ?? GramAnalysisSession.Analyze;
	}

	/// <summary>A version's analysis is ready, or the version's carried-over one is: ask for it again.</summary>
	public event Action<TVersion>? Changed;

	/// <summary>An analysis ended, published or dropped; what it cost.</summary>
	public event Action<GramAnalysisTiming>? Analyzed;

	public IIncludedGrammarStore Store => _host.Store;

	/// <summary>The last analysis published, or null before the first.</summary>
	public GramAnalysisResult<TVersion>? Result
	{
		get
		{
			lock (_gate)
				return _result;
		}
	}

	/// <summary>How the grammar's host stands now.</summary>
	public StandaloneGrammarResolution Resolution
	{
		get
		{
			lock (_gate)
				return _resolution;
		}
	}

	/// <summary>
	/// Starts following the grammar's host: found now, and found again whenever the solution
	/// changes in a way that concerns it, for as long as the document is open.
	/// </summary>
	/// <returns>
	/// Whether the editor is to hand in its solution's changes from now on: false where the session
	/// already follows, and for a read-only copy, which belongs to no project.
	/// </returns>
	public bool Follow(string filePath)
	{
		lock (_gate)
		{
			if (_configured)
				return false;

			_configured = true;

			// A read-only copy of what an assembly carries belongs to no project and includes
			// what it includes from somewhere out of reach: it is read as it stands, quiet about
			// what depends on its includes, and nothing is looked for.
			if (_host.Store.IsCopy(filePath))
			{
				_copy = true;
				return false;
			}

			_following     = true;
			_pendingFull   = true;
			_followedSince = _host.Scheduler.Now;
		}

		ScheduleResolve(0);
		_host.Scheduler.Run(SettleAsync);

		return true;
	}

	/// <summary>The document is closed: nothing more is looked for.</summary>
	public void Close()
	{
		lock (_gate)
		{
			if (_closed)
				return;

			_closed = true;
			_resolveCancellation?.Cancel();
		}
	}

	/// <summary>The document's analysis at a version: the published one, or empty with an analysis started.</summary>
	public GramDocument Document(TVersion snapshot)
	{
		lock (_gate)
		{
			if (_snapshot == snapshot && _document is not null)
				return _document;
		}

		ScheduleAnalysis(snapshot, immediate: true);
		return EmptyDocument;
	}

	/// <summary>
	/// Where a name at <paramref name="position"/> is defined, when that is in a grammar the
	/// host includes rather than in this document.
	/// </summary>
	public StandaloneDefinition? ExternalDefinition(TVersion snapshot, int position)
	{
		GramDocument?             document;
		StandaloneGrammarContext? context;

		lock (_gate)
		{
			var current = _snapshot == snapshot && _document is not null;

			document = current ? _document : null;
			context  = current ? _analysedContext : _resolution.Context;
		}

		return context is null
			? null
			: GramAnalysisSession.ExternalDefinition(_host.Text(snapshot), context.Value, document?.Symbols ?? NoSymbols, position);
	}

	/// <summary>
	/// The uses of a rule inside the grammars the host includes, and its definition where it
	/// is one of theirs: what Find All References adds to what is in the document.
	/// </summary>
	public IReadOnlyList<StandaloneReference> IncludedReferences(
		TVersion snapshot,
		string name,
		int definitionPosition)
	{
		IReadOnlyList<GramSymbolOccurrence> included;
		StandaloneGrammarContext?           context;

		lock (_gate)
		{
			if (_snapshot != snapshot || _document is null)
				return [];

			included = _includedSymbols;
			context  = _analysedContext;
		}

		return context is null
			? []
			: GramAnalysisSession.IncludedReferences(included, context.Value, _host.Text(snapshot).Length, name, definitionPosition);
	}

	/// <summary>The solution changed: the host is looked for again where the change concerns it.</summary>
	public void WorkspaceChanged(WorkspaceChangeEventArgs change)
	{
		StandaloneGrammarResolution resolution;
		string?                     filePath;

		lock (_gate)
		{
			if (_closed)
				return;

			resolution = _resolution;
			filePath   = _following ? _host.FilePath : null;
		}

		if (filePath is null)
			return;

		var concern = GramAnalysisSession.Concerns(change, resolution, filePath);

		if (concern == StandaloneGrammarConcern.None)
			return;

		lock (_gate)
		{
			_retries = 0;

			if (concern == StandaloneGrammarConcern.Full)
				_pendingFull = true;
			else
				_pendingDocuments.Add(change.DocumentId!);
		}

		ScheduleResolve(GramAnalysisSession.ResolveDelayMilliseconds);
	}

	void ScheduleResolve(int delay)
	{
		CancellationToken cancellationToken;

		lock (_gate)
		{
			if (_closed)
				return;

			_resolveCancellation?.Cancel();
			_resolveCancellation?.Dispose();
			_resolveCancellation = new CancellationTokenSource();
			cancellationToken    = _resolveCancellation.Token;
		}

		_host.Scheduler.Run(() => ResolveAsync(delay, cancellationToken));
	}

	async Task ResolveAsync(int delay, CancellationToken cancellationToken)
	{
		try
		{
			if (delay > 0)
				await _host.Scheduler.DelayAsync(delay, cancellationToken).ConfigureAwait(false);

			string?      filePath;
			bool         full;
			DocumentId[] edited;

			lock (_gate)
			{
				filePath  = _following ? _host.FilePath : null;
				full      = _pendingFull;
				edited    = [.. _pendingDocuments];

				_pendingFull = false;
				_pendingDocuments.Clear();
			}

			if (filePath is null)
				return;

			// Only documents of the grammar's project were edited while it has no host: look for
			// the host again only if one of them now declares a class spelled as one.
			if (!full && !await _host.MayHostAsync(edited, filePath, cancellationToken).ConfigureAwait(false))
				return;

			StandaloneGrammarResolution resolution;

			try
			{
				resolution = await _host.ResolveAsync(filePath, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception exception) when (exception is not OperationCanceledException and not OutOfMemoryException)
			{
				// The project system mutates the solution while it is loading. A transient
				// snapshot failure is equivalent to the context not being ready yet.
				resolution = StandaloneGrammarResolution.NotReady;
			}

			bool reanalyse;
			var  retry = -1;

			lock (_gate)
			{
				cancellationToken.ThrowIfCancellationRequested();

				var before = _resolution;

				_resolution = resolution;
				reanalyse   = before.State != resolution.State || !GramAnalysisSession.Same(before.Context, resolution.Context);

				if (reanalyse)
					_scheduledSnapshot = null;

				if (resolution.State == StandaloneGrammarState.NotReady && _retries < GramAnalysisSession.RetryCount)
					retry = GramAnalysisSession.RetryDelayMilliseconds << _retries++;
			}

			if (reanalyse)
				ScheduleAnalysis(_host.Current, immediate: true);

			if (retry >= 0)
			{
				lock (_gate)
					_pendingFull = true;

				ScheduleResolve(retry);
			}
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception exception) when (exception is not OutOfMemoryException)
		{
			_host.Report(exception);
		}
	}

	/// <summary>Once the settling time is over, a grammar still waiting for its host is told about as it stands.</summary>
	async Task SettleAsync()
	{
		await _host.Scheduler.DelayAsync(GramAnalysisSession.SettleMilliseconds, CancellationToken.None).ConfigureAwait(false);

		lock (_gate)
		{
			if (_closed || _resolution.State != StandaloneGrammarState.NotReady)
				return;

			_scheduledSnapshot = null;
		}

		ScheduleAnalysis(_host.Current, immediate: true);
	}

	/// <summary>
	/// The document was edited: what was known is carried over to the new version at once, and
	/// the new version is analysed once typing pauses.
	/// </summary>
	/// <remarks>Raises <see cref="Changed"/> on the caller's thread, before returning.</remarks>
	public void Edited(TVersion before, TVersion after)
	{
		lock (_gate)
		{
			_document = _snapshot == before && _document is not null
				? _host.Translate(_document, before, after)
				: EmptyDocument;
			_includedSymbols = NoSymbols;
			_snapshot = after;
		}

		Changed?.Invoke(after);
		ScheduleAnalysis(after, immediate: false);
	}

	void ScheduleAnalysis(TVersion snapshot, bool immediate)
	{
		CancellationToken cancellationToken;
		lock (_gate)
		{
			if (_scheduledSnapshot == snapshot)
				return;

			_analysisCancellation?.Cancel();
			_analysisCancellation?.Dispose();
			_analysisCancellation = new CancellationTokenSource();
			_scheduledSnapshot = snapshot;
			cancellationToken = _analysisCancellation.Token;
		}

		_host.Scheduler.Run(() => AnalyzeAsync(snapshot, immediate, cancellationToken));
	}

	async Task AnalyzeAsync(
		TVersion snapshot,
		bool immediate,
		CancellationToken cancellationToken)
	{
		try
		{
			if (!immediate)
				await _host.Scheduler.DelayAsync(GramAnalysisSession.AnalysisDelayMilliseconds, cancellationToken).ConfigureAwait(false);
			await _analysisGate.WaitAsync(cancellationToken).ConfigureAwait(false);

			GramAnalysisTiming timing;

			try
			{
				cancellationToken.ThrowIfCancellationRequested();
				StandaloneGrammarContext?   context;
				StandaloneGrammarResolution resolution;
				bool suppressContextDiagnostics;
				lock (_gate)
				{
					resolution = _resolution;
					context    = _resolution.Context;
					suppressContextDiagnostics =
						_copy ||
						_configured &&
						_resolution.State == StandaloneGrammarState.NotReady &&
						_host.Scheduler.Now - _followedSince < GramAnalysisSession.SettleMilliseconds;
				}

				var text     = _host.Text(snapshot);
				var clock    = GramAnalysisClock.Start();
				var analysis = _analyze(text, context, suppressContextDiagnostics, clock);

				clock.Stop();

				var published = false;

				lock (_gate)
				{
					cancellationToken.ThrowIfCancellationRequested();
					if (_host.Current == snapshot)
					{
						published = true;
						timing    = clock.Timing(text.Length + (context?.AnalysisTail.Length ?? 0), published: true);

						_snapshot        = snapshot;
						_document        = analysis.Document;
						_includedSymbols = analysis.Included;
						_analysedContext = context;
						_result          = new GramAnalysisResult<TVersion>(snapshot, analysis.Document, analysis.Included, resolution, timing);
					}
					else
						timing = clock.Timing(text.Length + (context?.AnalysisTail.Length ?? 0), published: false);
				}

				Analyzed?.Invoke(timing);

				if (!published)
					return;
			}
			finally
			{
				_analysisGate.Release();
			}

			await _host.BeforeChangedAsync(cancellationToken).ConfigureAwait(false);
			Changed?.Invoke(snapshot);
		}
		catch (OperationCanceledException)
		{
		}
		catch (Exception exception) when (exception is not OutOfMemoryException)
		{
			_host.Report(exception);
		}
	}
}

/// <summary>
/// Times one analysis in two parts, the compiler's and the editor's after it, and counts what it
/// allocated; the session reads it into a <see cref="GramAnalysisTiming"/>.
/// </summary>
sealed class GramAnalysisClock
{
	readonly Stopwatch _watch = Stopwatch.StartNew();
	readonly long      _allocatedBefore = Allocated();
	TimeSpan           _compileEnd;
	TimeSpan           _end;
	long               _allocated = -1;
	bool               _compiled;

	public static GramAnalysisClock Start()
	{
		return new GramAnalysisClock();
	}

	/// <summary>The compiler is done; what follows is the editor's.</summary>
	public void StartPostProcessing()
	{
		if (_compiled)
			return;

		_compiled   = true;
		_compileEnd = _watch.Elapsed;
	}

	public void Stop()
	{
		if (!_watch.IsRunning)
			return;

		_watch.Stop();
		_end = _watch.Elapsed;

		if (!_compiled)
		{
			_compiled   = true;
			_compileEnd = _end;
		}

		var after  = Allocated();
		_allocated = after < 0 || _allocatedBefore < 0 ? -1 : after - _allocatedBefore;
	}

	public GramAnalysisTiming Timing(int textLength, bool published)
	{
		return new GramAnalysisTiming(textLength, _compileEnd, _end - _compileEnd, _allocated, published);
	}

	static long Allocated()
	{
		#if NETCOREAPP3_0_OR_GREATER
		return GC.GetAllocatedBytesForCurrentThread();
		#else
		return -1;
		#endif
	}
}

/// <summary>What a change to the solution asks of a standalone grammar.</summary>
enum StandaloneGrammarConcern
{
	/// <summary>Nothing: it cannot change what the grammar is compiled with.</summary>
	None,

	/// <summary>A document was edited that may now declare the host: look at it, and only then for the host.</summary>
	Document,

	/// <summary>Look for the host again.</summary>
	Full,
}

/// <summary>A standalone grammar's analysis, and the symbols of what it includes kept for Find All References.</summary>
readonly record struct StandaloneAnalysis(GramDocument Document, IReadOnlyList<GramSymbolOccurrence> Included);

/// <summary>A name in the document whose definition is in an included grammar.</summary>
readonly record struct StandaloneDefinition(int Position, int Length, StandaloneLocation Target);

/// <summary>A use of a rule in an included grammar, or its definition there.</summary>
readonly record struct StandaloneReference(StandaloneLocation Location, int Length, bool IsDefinition);
