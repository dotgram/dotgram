using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

namespace DotGram.Language;

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

/// <summary>
/// A change to the solution as far as a standalone grammar is concerned, without the solutions
/// themselves: what changed, where, and the two things only the solutions can tell.
/// </summary>
/// <param name="OwnText">The grammar's own file was edited, which concerns no resolution: its text is the editor's.</param>
/// <param name="Holds">The project holds the grammar's file before or after the change; asked only for what can add or remove a document.</param>
readonly record struct StandaloneGrammarChange(
	WorkspaceChangeKind Kind,
	ProjectId?          Project,
	DocumentId?         Document,
	bool                OwnText,
	bool                Holds)
{
	public static StandaloneGrammarChange Of(WorkspaceChangeEventArgs change, string filePath)
	{
		var ownText = change.Kind == WorkspaceChangeKind.AdditionalDocumentChanged &&
			change.DocumentId is { } edited &&
			string.Equals(
				change.NewSolution.GetAdditionalDocument(edited)?.FilePath,
				filePath,
				StringComparison.OrdinalIgnoreCase);

		var holds = change.ProjectId is { } project &&
			AddsOrRemoves(change.Kind) &&
			(HeldIn(change.NewSolution, project, filePath) || HeldIn(change.OldSolution, project, filePath));

		return new StandaloneGrammarChange(change.Kind, change.ProjectId, change.DocumentId, ownText, holds);
	}

	/// <summary>
	/// Whether the change can change what the grammar is compiled with, as <paramref name="resolution"/>
	/// knows it: its host, what the host includes, or how it is read.
	/// </summary>
	/// <remarks>
	/// The grammar's own text is not among them — it is the editor's, and read from there —
	/// and nothing in a project that neither holds the grammar nor declares what it includes
	/// is either. Until the host is found, an edit to a document of the grammar's own project
	/// may be the one that makes it findable; that is answered by looking at the one document
	/// (<see cref="StandaloneGrammarConcern.Document"/>), not by looking for the host again.
	/// </remarks>
	public StandaloneGrammarConcern Concerns(StandaloneGrammarResolution resolution)
	{
		switch (Kind)
		{
			case WorkspaceChangeKind.SolutionAdded:
			case WorkspaceChangeKind.SolutionChanged:
			case WorkspaceChangeKind.SolutionRemoved:
			case WorkspaceChangeKind.SolutionCleared:
			case WorkspaceChangeKind.SolutionReloaded:
				return StandaloneGrammarConcern.Full;
		}

		if (Project is not { } project)
			return StandaloneGrammarConcern.Full;

		if (OwnText)
			return StandaloneGrammarConcern.None;

		if (Document is { } document && resolution.Documents.Contains(document))
			return StandaloneGrammarConcern.Full;

		var watched = resolution.Projects.Contains(project);

		if (AddsOrRemoves(Kind))
			// What can put the grammar into a project, or take it out: asked of both solutions,
			// since a removal is only in the old one.
			return watched || resolution.State != StandaloneGrammarState.Resolved && Holds
				? StandaloneGrammarConcern.Full
				: StandaloneGrammarConcern.None;

		switch (Kind)
		{
			case WorkspaceChangeKind.DocumentChanged:
				return watched && resolution.State != StandaloneGrammarState.Resolved
					? StandaloneGrammarConcern.Document
					: StandaloneGrammarConcern.None;

			case WorkspaceChangeKind.AdditionalDocumentChanged:
			case WorkspaceChangeKind.AnalyzerConfigDocumentChanged:
				return StandaloneGrammarConcern.None;

			default:
				return watched ? StandaloneGrammarConcern.Full : StandaloneGrammarConcern.None;
		}
	}

	static bool AddsOrRemoves(WorkspaceChangeKind kind)
	{
		switch (kind)
		{
			case WorkspaceChangeKind.ProjectAdded:
			case WorkspaceChangeKind.ProjectRemoved:
			case WorkspaceChangeKind.ProjectReloaded:
			case WorkspaceChangeKind.DocumentAdded:
			case WorkspaceChangeKind.DocumentRemoved:
			case WorkspaceChangeKind.DocumentReloaded:
			case WorkspaceChangeKind.AdditionalDocumentAdded:
			case WorkspaceChangeKind.AdditionalDocumentRemoved:
			case WorkspaceChangeKind.AdditionalDocumentReloaded:
				return true;

			default:
				return false;
		}
	}

	static bool HeldIn(Solution solution, ProjectId project, string filePath)
	{
		return solution.GetProject(project)?.AdditionalDocuments.Any(document =>
			string.Equals(document.FilePath, filePath, StringComparison.OrdinalIgnoreCase)) == true;
	}
}

/// <summary>What a <see cref="StandaloneGrammarHostTracker"/> asks of the editor that holds the grammar.</summary>
/// <param name="FilePath">The grammar's file, or null while it has none.</param>
/// <param name="ResolveAsync">Looks for the host in the solution as it is now.</param>
/// <param name="MayHostAsync">Whether one of these edited documents now declares a class that could be the host.</param>
/// <param name="Now">The editor's clock, in milliseconds.</param>
/// <param name="DelayAsync">Waits, on the editor's clock.</param>
/// <param name="Run">Starts work nobody awaits.</param>
/// <param name="Resolved">A resolution was published: the one before it, and it.</param>
/// <param name="Recoverable">Whether a failure is one to report and go on after.</param>
/// <param name="Report">A failure gone on after.</param>
/// <remarks>The tracker calls none of these under its lock.</remarks>
sealed record StandaloneGrammarHostServices(
	Func<string?>                                                                        FilePath,
	Func<string, CancellationToken, Task<StandaloneGrammarResolution>>                   ResolveAsync,
	Func<IReadOnlyList<DocumentId>, string, CancellationToken, Task<bool>>               MayHostAsync,
	Func<long>                                                                           Now,
	Func<int, CancellationToken, Task>                                                   DelayAsync,
	Action<Func<Task>>                                                                   Run,
	Action<StandaloneGrammarResolution, StandaloneGrammarResolution>                     Resolved,
	Func<Exception, bool>                                                                Recoverable,
	Action<Exception>                                                                    Report);

/// <summary>
/// A standalone grammar's host, looked for in the solution and looked for again whenever the
/// solution changes in a way that concerns it, until the grammar is closed: one resolution at a
/// time, each a generation that a newer request replaces.
/// </summary>
/// <remarks>
/// <para>
/// A request records what is to be looked at and replaces the running resolution in one step,
/// under the tracker's lock: the work the replaced one had taken goes back with the request's
/// own, so the newer generation takes all of it. A generation that is not the newest touches
/// nothing: it neither starts, nor publishes, nor requests again. The replaced generation is
/// cancelled only after the lock is let go, so that whatever its cancellation runs at once finds
/// it replaced already, and nothing it calls out to is called under the lock.
/// </para>
/// <para>
/// A request waits for the solution to settle, and each newer one waits again; a stream of
/// changes that never pauses would wait for ever. The wait is cut short so that a generation
/// starts at most <c>maxDeferralMilliseconds</c> after the first request it answers.
/// </para>
/// <para>
/// A resolution reads the solution as it is when it starts, and a change that comes while it
/// runs is judged against the resolution before it, which may not know what the change is
/// about: an included grammar is not among a not-ready resolution's documents. Such a change is
/// kept, without the solutions it carries and once for each kind of change to each document or
/// project, and asked again of the resolution that comes out.
/// </para>
/// </remarks>
sealed class StandaloneGrammarHostTracker(
	StandaloneGrammarHostServices services,
	int resolveDelayMilliseconds,
	int retryDelayMilliseconds,
	int retryCount,
	int maxDeferralMilliseconds)
{
	readonly object                           _gate = new();
	readonly HashSet<DocumentId>              _pendingDocuments = [];
	readonly HashSet<DocumentId>              _takenDocuments   = [];
	readonly HashSet<StandaloneGrammarChange> _kept             = [];

	StandaloneGrammarResolution _resolution = StandaloneGrammarResolution.NotReady;
	CancellationTokenSource?    _cancellation;
	long?                       _waitingSince;
	int                         _generation;
	int                         _retries;
	bool                        _pendingFull;
	bool                        _takenFull;
	bool                        _running;
	bool                        _closed;

	/// <summary>How the host stands now.</summary>
	public StandaloneGrammarResolution Resolution
	{
		get
		{
			lock (_gate)
				return _resolution;
		}
	}

	/// <summary>How many changes are kept to be judged again: one for each kind of change to each document or project.</summary>
	internal int Kept
	{
		get
		{
			lock (_gate)
				return _kept.Count;
		}
	}

	/// <summary>Starts looking for the host.</summary>
	public void Follow()
	{
		Start(Request(StandaloneGrammarConcern.Full, null, 0, resetRetries: false, services.Now()));
	}

	/// <summary>The solution changed: the host is looked for again where the change concerns it.</summary>
	public void WorkspaceChanged(WorkspaceChangeEventArgs change)
	{
		if (services.FilePath() is not { } filePath)
			return;

		var judged = StandaloneGrammarChange.Of(change, filePath);
		var now    = services.Now();
		Next? next;

		lock (_gate)
		{
			if (_closed)
				return;

			// Judged under the lock, so that no resolution ends between the judging and the keeping.
			var concern = judged.Concerns(_resolution);

			if (concern == StandaloneGrammarConcern.None)
			{
				// A resolution reading the solution now may have read it before this change.
				if (_running && !judged.OwnText)
					_kept.Add(judged);

				return;
			}

			next = Request(concern, change.DocumentId, resolveDelayMilliseconds, resetRetries: true, now);
		}

		Start(next);
	}

	/// <summary>The grammar is closed: nothing more is looked for.</summary>
	public void Close()
	{
		CancellationTokenSource? cancellation;

		lock (_gate)
		{
			if (_closed)
				return;

			_closed = true;
			_generation++;
			_kept.Clear();

			cancellation  = _cancellation;
			_cancellation = null;
		}

		Cancel(cancellation);
	}

	/// <summary>
	/// Records what is to be looked at and replaces the running resolution with a newer generation,
	/// handing it back the work the replaced one had taken. Called under the lock; the caller starts
	/// what it returns, and cancels the generation it replaced, once the lock is let go.
	/// </summary>
	/// <param name="now">The clock, read by the caller before it took the lock.</param>
	Next? Request(
		StandaloneGrammarConcern concern,
		DocumentId? document,
		int delay,
		bool resetRetries,
		long now)
	{
		lock (_gate)
		{
			if (_closed)
				return null;

			if (resetRetries)
				_retries = 0;

			if (concern == StandaloneGrammarConcern.Full)
				_pendingFull = true;
			else if (document is not null)
				_pendingDocuments.Add(document);

			_pendingFull |= _takenFull;
			_pendingDocuments.UnionWith(_takenDocuments);
			_takenFull = false;
			_takenDocuments.Clear();

			// The newer generation reads the solution later than every change kept so far.
			_running = false;
			_kept.Clear();

			// The solution may never settle: no generation waits longer than the limit from the
			// first request it answers.
			if (_waitingSince is { } since)
				delay = (int)Math.Min(delay, Math.Max(0, since + maxDeferralMilliseconds - now));
			else
				_waitingSince = now;

			var replaced = _cancellation;

			_cancellation = new CancellationTokenSource();

			return new Next(++_generation, _cancellation.Token, delay, replaced);
		}
	}

	/// <summary>Cancels the generation a request replaced, and starts the request's. Called on no lock.</summary>
	void Start(Next? next)
	{
		if (next is not { } run)
			return;

		Cancel(run.Replaced);
		services.Run(() => ResolveAsync(run.Generation, run.Delay, run.Token));
	}

	static void Cancel(CancellationTokenSource? cancellation)
	{
		if (cancellation is null)
			return;

		cancellation.Cancel();
		cancellation.Dispose();
	}

	async Task ResolveAsync(int generation, int delay, CancellationToken cancellationToken)
	{
		try
		{
			if (delay > 0)
				await services.DelayAsync(delay, cancellationToken).ConfigureAwait(false);

			bool         full;
			DocumentId[] edited;

			lock (_gate)
			{
				if (generation != _generation)
					return;

				_takenFull = _pendingFull;
				_takenDocuments.UnionWith(_pendingDocuments);
				_pendingFull = false;
				_pendingDocuments.Clear();
				_running      = true;
				_waitingSince = null;

				full   = _takenFull;
				edited = [.. _takenDocuments];
			}

			var filePath = services.FilePath();

			if (filePath is null ||
				// Only documents of the grammar's project were edited while it has no host: look for
				// the host again only if one of them now declares a class spelled as one.
				!full && !await services.MayHostAsync(edited, filePath, cancellationToken).ConfigureAwait(false))
			{
				Finish(generation);
				return;
			}

			StandaloneGrammarResolution resolution;

			try
			{
				resolution = await services.ResolveAsync(filePath, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception exception) when (exception is not OperationCanceledException && services.Recoverable(exception))
			{
				// The project system mutates the solution while it is loading. A transient
				// snapshot failure is equivalent to the context not being ready yet.
				resolution = StandaloneGrammarResolution.NotReady;
			}

			StandaloneGrammarResolution before;
			Next?                       next = null;
			var                         now  = services.Now();

			lock (_gate)
			{
				// Replaced while it ran: what it had taken went back to the newer generation.
				if (generation != _generation)
					return;

				before      = _resolution;
				_resolution = resolution;

				_takenFull = false;
				_takenDocuments.Clear();
				_running = false;

				// An edit to an included grammar made while the solution was read is not in what
				// was read, and nothing else would look at it again.
				var documents = new List<DocumentId>();
				var again     = Again(resolution, documents);

				if (resolution.State == StandaloneGrammarState.NotReady && _retries < retryCount)
					next = Request(StandaloneGrammarConcern.Full, null, retryDelayMilliseconds << _retries++, resetRetries: false, now);
				else if (again == StandaloneGrammarConcern.Full)
					next = Request(StandaloneGrammarConcern.Full, null, resolveDelayMilliseconds, resetRetries: false, now);
				else if (again == StandaloneGrammarConcern.Document)
				{
					_pendingDocuments.UnionWith(documents);
					next = Request(StandaloneGrammarConcern.Document, null, resolveDelayMilliseconds, resetRetries: false, now);
				}
			}

			services.Resolved(before, resolution);
			Start(next);
		}
		catch (OperationCanceledException exception) when (cancellationToken.IsCancellationRequested && exception.CancellationToken == cancellationToken)
		{
		}
		catch (Exception exception) when (services.Recoverable(exception))
		{
			services.Report(exception);
			Finish(generation);
		}
	}

	/// <summary>A resolution that ends with nothing published: the one before stands, and so does what was judged against it.</summary>
	void Finish(int generation)
	{
		lock (_gate)
		{
			if (generation != _generation)
				return;

			_takenFull = false;
			_takenDocuments.Clear();
			_running = false;
			_kept.Clear();
		}
	}

	/// <summary>What the changes kept while the resolution ran ask of what it found. Called under the lock.</summary>
	StandaloneGrammarConcern Again(StandaloneGrammarResolution resolution, ICollection<DocumentId> documents)
	{
		var result = StandaloneGrammarConcern.None;

		foreach (var change in _kept)
		{
			var concern = change.Concerns(resolution);

			if (concern == StandaloneGrammarConcern.Full)
			{
				result = StandaloneGrammarConcern.Full;
				break;
			}

			if (concern == StandaloneGrammarConcern.Document)
			{
				documents.Add(change.Document!);
				result = StandaloneGrammarConcern.Document;
			}
		}

		_kept.Clear();

		return result;
	}

	/// <summary>A generation a request starts, after the wait it starts with, and the one it replaced.</summary>
	readonly record struct Next(int Generation, CancellationToken Token, int Delay, CancellationTokenSource? Replaced);
}
